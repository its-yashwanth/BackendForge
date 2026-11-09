using System.ComponentModel.DataAnnotations;


namespace MyFirstApi.DTOs;
public class PatchStudentDto
{
    public string? Name { get; set; }

    public string? Branch { get; set; }
    

    [Range(1, 4)]
    public int? Year { get; set; }
}