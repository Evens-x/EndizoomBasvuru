using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EndizoomBasvuru.Services.Interfaces;
using EndizoomBasvuru.Services.Models;
using EndizoomBasvuru.Entity;
using System.Security.Claims;

namespace EndizoomBasvuru.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var admin = await _adminService.AuthenticateAsync(model);
            if (admin == null)
                return Unauthorized(new { message = "E-posta veya şifre yanlış." });

            return Ok(admin);
        }

        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AdminRegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int createdById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (createdById == 0)
                return Unauthorized();

            var result = await _adminService.RegisterAdminAsync(model, createdById);
            return Ok(result);
        }
        
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
        [HttpGet]
        public async Task<IActionResult> GetAllAdmins()
        {
            var admins = await _adminService.GetAllAdminsAsync();
            return Ok(admins);
        }
        
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAdminById(int id)
        {
            var admin = await _adminService.GetAdminByIdAsync(id);
            if (admin == null)
                return NotFound(new { message = "Yönetici bulunamadı." });

            return Ok(admin);
        }
        
        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAdmin(int id, [FromBody] AdminUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _adminService.UpdateAdminAsync(id, model);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            var result = await _adminService.DeleteAdminAsync(id);
            if (!result)
                return NotFound(new { message = "Yönetici bulunamadı." });

            return Ok(new { message = "Yönetici başarıyla silindi." });
        }
        
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (adminId == 0)
                return Unauthorized();

            var result = await _adminService.ChangePasswordAsync(adminId, model);
            if (!result)
                return BadRequest(new { message = "Mevcut şifre yanlış." });

            return Ok(new { message = "Şifre başarıyla değiştirildi." });
        }
    }
}
