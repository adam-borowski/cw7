using Cw7.DTOs.Requests;
using Cw7.Models;
using System.Data.SqlClient;
using System;
using System.Data;

namespace Cw7.Services {
    public class SqlServerStudentDbService : IStudentDbService
    {
        private string CONNECTION_STRING = "Server=localhost;Database=testDB;User Id=s18164;Password=123ASDqwe;";

        public Student GetStudent(string indexNumber) {
            using(var connection = new SqlConnection(CONNECTION_STRING))
            using(var command = new SqlCommand()) {
                command.Connection = connection;
                connection.Open();
                command.CommandText = "SELECT FirstName, LastName, IndexNumber, BirthDate FROM Student s WHERE s.IndexNumber = @indexnumber";
                command.Parameters.AddWithValue("indexnumber", indexNumber);
                var reader = command.ExecuteReader();
                if(reader.Read()) {
                    return new Student {
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        IndexNumber = reader["IndexNumber"].ToString(),
                        BirthDate = DateTime.Parse(reader["BirthDate"].ToString())
                    };
                }
                return null;
            }
        }

        public Enrollment EnrollStudent(EnrollStudentRequest request) {
            using(var connection = new SqlConnection(CONNECTION_STRING))
            using(var command = new SqlCommand()) {
                command.Connection = connection;
                connection.Open();
                var transaction = connection.BeginTransaction();
                command.Transaction = transaction;

                command.CommandText = "SELECT IdStudy FROM Studies WHERE Name = @studies";
                command.Parameters.AddWithValue("studies", request.Studies);
                var reader = command.ExecuteReader();
                if (!reader.Read()) {
                    return null;
                }
                var idStudy = Int32.Parse(reader["IdStudy"].ToString());
                reader.Close();

                var enrollment = new Enrollment();

                command.CommandText = "SELECT e.IdEnrollment, e.Semester, e.IdStudy, e.StartDate FROM Enrollment e INNER JOIN Studies s ON s.IdStudy = e.IdStudy WHERE s.Name = @studyname";
                command.Parameters.AddWithValue("studyname", request.Studies);
                reader = command.ExecuteReader();
                if (reader.Read()) {
                    enrollment.IdEnrollment = Int32.Parse(reader["IdEnrollment"].ToString());
                    enrollment.Semester = Int32.Parse(reader["Semester"].ToString());
                    enrollment.IdStudy = Int32.Parse(reader["IdStudy"].ToString());
                    enrollment.StartDate = DateTime.Parse(reader["StartDate"].ToString());
                }
                else {
                    var idEnrollement = 1;

                    reader.Close();

                    command.CommandText = "SELECT MAX(IdEnrollement) + 1 FROM Enrollment";
                    reader = command.ExecuteReader();
                    if (reader.Read()) {
                        idEnrollement = Int32.Parse(reader[0].ToString());
                    }
                    enrollment.IdEnrollment = idEnrollement;
                    enrollment.Semester = 1;
                    enrollment.IdStudy = idStudy;
                    enrollment.StartDate = DateTime.Now;

                    command.CommandText = "INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate) " +
                        "VALUES (@idenrollment, @semester, @idstudy, @startdate)";
                    command.Parameters.AddWithValue("idenrollment", enrollment.IdEnrollment);
                    command.Parameters.AddWithValue("semester", enrollment.Semester);
                    command.Parameters.AddWithValue("idstudy", enrollment.IdStudy);
                    command.Parameters.AddWithValue("startdate", enrollment.StartDate.Date);
                    command.ExecuteNonQuery();
                }
                reader.Close();

                command.CommandText = "SELECT 1 FROM Student s WHERE s.IndexNumber = @indexnumber";
                command.Parameters.AddWithValue("indexnumber", request.IndexNumber);
                reader = command.ExecuteReader();
                if (reader.Read()) {
                    reader.Close();
                    transaction.Rollback();
                    return null;
                }
                reader.Close();
                command.CommandText = "INSERT INTO Student (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) " +
                        "VALUES (@index, @firstname, @lastname, @birthdate, @idenrollement)";
                command.Parameters.AddWithValue("index", request.IndexNumber);
                command.Parameters.AddWithValue("firstname", request.FirstName);
                command.Parameters.AddWithValue("lastname", request.LastName);
                command.Parameters.AddWithValue("birthdate", request.BirthDate);
                command.Parameters.AddWithValue("idenrollement", enrollment.IdEnrollment);
                command.ExecuteNonQuery();

                transaction.Commit();

                return enrollment;
            }
        }

        public Enrollment PromoteStudents(PromoteStudentRequest request) {
            using(var connection = new SqlConnection(CONNECTION_STRING))
            using(var command = new SqlCommand()) {
                command.Connection = connection;
                connection.Open();

                command.CommandText = "SELECT IdStudy FROM Studies WHERE Name = @studies";
                command.Parameters.AddWithValue("studies", request.Studies);

                var reader = command.ExecuteReader();
                if (!reader.Read()) {
                    return null;
                }
                var idStudy = Int32.Parse(reader["IdStudy"].ToString());
                reader.Close();

                int enrollmentId = 0;

                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@Studies", request.Studies));
                command.Parameters.Add(new SqlParameter("@Semester", request.Semester));

                var returnParameter = command.Parameters.Add("@ReturnVal", SqlDbType.Int);
                returnParameter.Direction = ParameterDirection.ReturnValue;

                command.ExecuteNonQuery();
                enrollmentId = Int32.Parse(returnParameter.Value.ToString());

                var enrollment = new Enrollment();
                command.CommandText = "SELECT e.IdEnrollment, e.Semester, e.IdStudy, e.StartDate FROM Enrollment e WHERE e.IdEnrollment = @idenrollment";
                command.Parameters.AddWithValue("idenrollment", enrollmentId);
                reader = command.ExecuteReader();
                if (reader.Read()) {
                    enrollment.IdEnrollment = Int32.Parse(reader["IdEnrollment"].ToString());
                    enrollment.Semester = Int32.Parse(reader["Semester"].ToString());
                    enrollment.IdStudy = Int32.Parse(reader["IdStudy"].ToString());
                    enrollment.StartDate = DateTime.Parse(reader["StartDate"].ToString());
                }

                return enrollment;
            }
        }
    }
}