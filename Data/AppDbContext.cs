
using Microsoft.EntityFrameworkCore;
using MyFirstApi.Models;

namespace MyFirstApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students { get; set; }

    public DbSet<AppUser> Users { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>()
            .HasIndex(user => user.Username)
            .IsUnique();



            modelBuilder.Entity<RefreshToken>()
    .HasOne(token => token.User)
    .WithMany()
    .HasForeignKey(token => token.UserId)
    .OnDelete(DeleteBehavior.Cascade);

modelBuilder.Entity<RefreshToken>()
    .HasIndex(token => token.TokenHash)
    .IsUnique();
    }
}
