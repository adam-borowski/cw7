using Cw7.Services;
using Cw7.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Cw7.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase {
        private readonly IStudentDbService _dbService;

        public EnrollmentsController(IStudentDbService dbService) {
            _dbService = dbService;
        }

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request) {
            var response = _dbService.EnrollStudent(request);
            if (response == null) {
                return BadRequest();
            }
            return Created("http://localhost:5001", response);
        }

        [HttpPost("promotions")]
        public IActionResult StudentPromotions(PromoteStudentRequest request) {
            var response = _dbService.PromoteStudents(request);
            if (response == null) {
                return NotFound();
            }

            return Created("http://localhost:5001", response);
        }
    }
}
