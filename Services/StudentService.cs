using Microsoft.EntityFrameworkCore;
using MyFirstApi.DTOs;
using MyFirstApi.Models;
using MyFirstApi.Data;

namespace MyFirstApi.Services;

public class StudentService : IStudentService
{
    private readonly AppDbContext _context;

    public StudentService(AppDbContext context)
    {
        _context = context;
    }


    public async Task<Student?> GetStudentById(int id)
    {
        return await _context.Students.FirstOrDefaultAsync(s=>s.Id==id);
    } 

    public async Task<List<Student>> GetAllStudents()
    {
        return await _context.Students.ToListAsync();
    }

    public async Task<List<StudentResponseDto>> GetStudents(string? branch,int? year)
    {
        var query=_context.Students.AsQueryable();
        if(!string.IsNullOrEmpty(branch))
        {
            query=query.Where(s=>s.Branch==branch);
        }

        if(year.HasValue)
        {
            query=query.Where(s=>s.Year==year.Value);
        }
        
        var response= await query.Select(student=>new StudentResponseDto
        {
            Id=student.Id,
            Name=student.Name,
            Branch=student.Branch,
            Year=student.Year
        }).ToListAsync();
        return response;
    }




    public async Task<StudentResponseDto> CreateStudent(CreateStudentDto dto)
{
    var student = new Student
    {
        Name = dto.Name,
        Branch = dto.Branch,
        Year = dto.Year
    };

    _context.Students.Add(student);

    await _context.SaveChangesAsync();

    var response = new StudentResponseDto
    {
        Id = student.Id,
        Name = student.Name,
        Branch = student.Branch,
        Year = student.Year
    };

    return response;
}

public async Task<Student?> UpdateStudent(
    int id,
    UpdateStudentDto dto)
{
    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.Id == id);

    if (student == null)
        return null;

    student.Name = dto.Name;
    student.Branch = dto.Branch;
    student.Year = dto.Year;

    await _context.SaveChangesAsync();

    return student;
}

public async Task<Student?> UpdateStudentPartial(
    int id,
    PatchStudentDto dto)
{
    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.Id == id);

    if (student == null)
        return null;

    if (dto.Name != null)
    {
        student.Name = dto.Name;
    }

    if (dto.Branch != null)
    {
        student.Branch = dto.Branch;
    }

    if (dto.Year.HasValue)
    {
        student.Year = dto.Year.Value;
    }

    await _context.SaveChangesAsync();

    return student;
}

public async Task<Student?> DeleteStudent(int id)
{
    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.Id == id);

    if (student == null)
        return null;

    _context.Students.Remove(student);

    await _context.SaveChangesAsync();

    return student;
}
}

