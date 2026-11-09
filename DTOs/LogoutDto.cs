using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.DTOs;


public class LogoutDto
{
    [Required]
    public string RefreshToken {get;set;} =string.Empty;
}
