

using MyFirstApi.DTOs;
using MyFirstApi.Models;

namespace MyFirstApi.Services;

public interface IStudentService
{
    Task<Student?> GetStudentById(int id);

    Task<List<Student>> GetAllStudents();

    Task<List<StudentResponseDto>> GetStudents(
        string? branch,
        int? year);

    Task<StudentResponseDto> CreateStudent(
        CreateStudentDto dto);

    Task<Student?> UpdateStudent(
        int id,
        UpdateStudentDto dto);

    Task<Student?> UpdateStudentPartial(
        int id,
        PatchStudentDto dto);

    Task<Student?> DeleteStudent(int id);
}