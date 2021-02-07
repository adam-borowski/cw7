using Cw7.Services;
using Cw7.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System;

namespace Cw7.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase {
        private readonly IStudentDbService _dbService;
        public IConfiguration Configuration { get; }

        public EnrollmentsController(IStudentDbService dbService, IConfiguration configuration) {
            _dbService = dbService;
            Configuration = configuration;
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

        [HttpPost]
        public IActionResult Login() {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Jan"),
                new Claim(ClaimTypes.Role, "student")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Secret Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
            (
                issuer: "gakko",
                audience: "students",
                claims: claims,
                expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds
            );

            return Ok(new 
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken = Guid.NewGuid()
            });
        }
    }
}
