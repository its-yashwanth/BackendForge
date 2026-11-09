using MyFirstApi.Models;
using System.Text.Json.Serialization;
using MyFirstApi.Data;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using MyFirstApi.DTOs;
using System.ComponentModel.DataAnnotations;
using MyFirstApi.Services;
using MyFirstApi.Helpers;
using MyFirstApi.Handlers;
using MyFirstApi.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling =
        JsonNumberHandling.Strict;
});

// builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT access token."
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(
            builder.Configuration.GetConnectionString("DefaultConnection")
        )
    )
);

// builder.Services.AddScoped<StudentService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<RefreshTokenService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is missing.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],

            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<PasswordService>();

var app = builder.Build();
app.UseExceptionHandler();
app.UseMiddleware<RequestLoggingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();
var students = new List<Student>
{
    new Student
    {
        Id=1,
        Name="Yash",
        Branch="CSE",
        Year=3
    },
    new Student
    {
        Id = 2,
        Name = "Rahul",
        Branch = "ECE",
        Year = 2
    },
};

// //GET API
 
// app.MapGet("/api/hello", () =>
// {
//     return Results.Ok(new
//     {
//         message = "Hello from C# API!"
//     });
// });

app.MapGet("/api/student", async (IStudentService service) =>
{                                     
    var students = await service.GetAllStudents();
    return Results.Ok(students);
});
 


// //route parameters
app.MapGet("/api/studentss/{id}", async (int id, IStudentService service) =>
{   
    var student = await service.GetStudentById(id);

    if(student == null) return Results.NotFound();

    var response = new StudentResponseDto
    {
        Id = student.Id,
        Name = student.Name,
        Branch = student.Branch, 
        Year = student.Year
    };
    return Results.Ok(response);
});


//query parameters
app.MapGet("/api/students", async (string? branch, int? year, IStudentService service) =>
{   
    var response = await service.GetStudents(branch, year);
    return Results.Ok(response);
})
.RequireAuthorization();



//POST API

app.MapPost("/api/students", async (CreateStudentDto dto, IStudentService service) =>
{ 
    if(!ValidationHelper.TryValidate(dto, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }

    var response = await service.CreateStudent(dto);
    return Results.Created($"/api/students/{response.Id}", response);
})
.RequireAuthorization(policy => policy.RequireRole("Admin"));




// put API

app.MapPut("/api/student/{id}", async (int id, UpdateStudentDto dto, IStudentService service) =>
{   
    if(!ValidationHelper.TryValidate(dto, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }
    var student = await service.UpdateStudent(id, dto);
    if(student == null) return Results.NotFound();
    return Results.Ok(student);
})
.RequireAuthorization(policy => policy.RequireRole("Admin"));


app.MapPatch("/api/student/{id}", async (int id, PatchStudentDto dto, IStudentService service) =>
{  
    if (!ValidationHelper.TryValidate(dto, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }
    var response = await service.UpdateStudentPartial(id, dto);
    if(response == null) return Results.NotFound();
    return Results.Ok(response);
})
.RequireAuthorization(policy => policy.RequireRole("Admin"));


app.MapDelete("/api/student/{id}", async (int id, IStudentService service) =>
{
    var student = await service.DeleteStudent(id);
    if(student == null) return Results.NotFound();
    return Results.NoContent();
})
.RequireAuthorization(policy => policy.RequireRole("Admin"));


app.MapGet("/api/test-error", () =>
{
    throw new Exception("This is a test exception.");
});



app.MapPost("/api/auth/login", async (
    LoginDto dto,
    AppDbContext db,
    PasswordService passwordService,
    IConfiguration configuration,RefreshTokenService refreshTokenService) =>
{
    if (!ValidationHelper.TryValidate(
        dto, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }

    var username = dto.Username.Trim();

    var user = await db.Users
        .SingleOrDefaultAsync(u => u.Username == username);

    if (user is null ||
        !passwordService.VerifyPassword(user, dto.Password))
    {
        return Results.Unauthorized();
    }

    var jwtKey = configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("JWT key is missing.");

    var issuer = configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("JWT issuer is missing.");

    var audience = configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("JWT audience is missing.");

    var key = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(jwtKey));

    var credentials = new SigningCredentials(
        key,
        SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, user.Role)
    };
    
    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(30),
        signingCredentials: credentials
    );

    var accessToken = new JwtSecurityTokenHandler()
        .WriteToken(token);

    var refreshToken=refreshTokenService.GenerateToken();

    var refreshTokenHash=refreshTokenService.HashToken(refreshToken);


    var refreshTokenEntity = new RefreshToken
    {
        UserId = user.Id,
        TokenHash = refreshTokenHash,
        ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
    };
    

    db.RefreshTokens.Add(refreshTokenEntity);
    await db.SaveChangesAsync();
    return Results.Ok(new
    {
        accessToken,
        refreshToken,
        tokenType = "Bearer",
        expiresAt = token.ValidTo
    });
});


app.MapPost("/api/auth/register",async (RegisterDto dto,AppDbContext db,PasswordService passwordService) =>
{
    if(!ValidationHelper.TryValidate(dto,out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }

    var username =dto.Username.Trim();

  var existingUser =await db.Users.AnyAsync(user=>user.Username==username);

  if(existingUser)
    {
        return Results.Conflict(new
        {
            message = "Username is already taken"
        });
    }


    var user = new AppUser
    {
        Username =username,
        Role ="User"
    };

    user.PasswordHash = passwordService.HashPassword(user,dto.Password);

    db.Users.Add(user);
    await db.SaveChangesAsync();

      return Results.Created(
        $"/api/users/{user.Id}",
        new
        {
            user.Id,
            user.Username,
            user.Role
        });

});



app.MapPost("/api/auth/refresh", async (
    RefreshTokenDto dto,
    AppDbContext db,
    RefreshTokenService refreshTokenService,
    IConfiguration configuration) =>
{
    if (!ValidationHelper.TryValidate(
        dto, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }

    // 1. Hash the incoming refresh token
    var tokenHash = refreshTokenService.HashToken(dto.RefreshToken);

    // 2. Find the stored refresh token
    var storedToken = await db.RefreshTokens
        .Include(rt => rt.User)
        .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash);

    // 3. Validate the old refresh token
    if (storedToken is null ||
        storedToken.ExpiresAtUtc <= DateTime.UtcNow ||
        storedToken.RevokedAtUtc is not null)
    {
        return Results.Unauthorized();
    }

    // 4. Begin database transaction
    await using var transaction =
        await db.Database.BeginTransactionAsync();

    try
    {
        // 5. Revoke the old refresh token
        storedToken.RevokedAtUtc = DateTime.UtcNow;

        // 6. Generate a new refresh token
        var newRefreshToken = refreshTokenService.GenerateToken();

        var newRefreshTokenHash =
            refreshTokenService.HashToken(newRefreshToken);

        // 7. Store the new refresh token hash
        var newRefreshTokenEntity = new RefreshToken
        {
            UserId = storedToken.UserId,
            TokenHash = newRefreshTokenHash,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
        };

        db.RefreshTokens.Add(newRefreshTokenEntity);

        // 8. Generate a new access token
        var user = storedToken.User;

        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is missing.");

        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is missing.");

        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is missing.");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler()
            .WriteToken(token);

        // 9. Save database changes
        await db.SaveChangesAsync();

        // 10. Commit transaction
        await transaction.CommitAsync();

        // 11. Return both new tokens
        return Results.Ok(new
        {
            accessToken,
            refreshToken = newRefreshToken,
            tokenType = "Bearer",
            expiresAt = token.ValidTo
        });
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
});



app.MapPost("/api/auth/logout", async (
    LogoutDto dto,
    AppDbContext db,
    RefreshTokenService refreshTokenService) =>
{
    if (!ValidationHelper.TryValidate(
        dto, out var validationResults))
    {
        return Results.BadRequest(validationResults);
    }

    // Hash the received refresh token
    var tokenHash = refreshTokenService.HashToken(dto.RefreshToken);

    // Find the refresh token in database
    var storedToken = await db.RefreshTokens
        .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash);

    // Check whether token exists and is active
    if (storedToken is null ||
        storedToken.RevokedAtUtc is not null)
    {
        return Results.Unauthorized();
    }

    // Revoke the refresh token
    storedToken.RevokedAtUtc = DateTime.UtcNow;

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        message = "Logged out successfully."
    });
});


app.Run();

public partial class Program { }