using Cw7.DTOs.Requests;
using Cw7.Models;

namespace Cw7.Services
{
    public interface IStudentDbService
    {
        Enrollment EnrollStudent(EnrollStudentRequest request);
        Enrollment PromoteStudents(PromoteStudentRequest request);

        Student GetStudent(string indexNumber);
    }
}