using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.DTOs;

public class UpdateStudentDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Branch { get; set; } = string.Empty;

    [Range(1, 4)]
    public int Year { get; set; }
}