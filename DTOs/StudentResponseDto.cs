namespace MyFirstApi.DTOs;

public class StudentResponseDto
{
    public int Id{ get; set;}
    public string Name{get;set;}=string.Empty;
    public string Branch { get; set;}=string.Empty;
    public int Year {get; set;}
}