
using Microsoft.AspNetCore.Identity;
using MyFirstApi.Models;

namespace MyFirstApi.Services;

public class PasswordService
{
    private readonly PasswordHasher<AppUser> _hasher = new();

    public string HashPassword(AppUser user, string password)
    {
        return _hasher.HashPassword(user, password);
    }

    public bool VerifyPassword(AppUser user, string password)
    {
        var result = _hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}
