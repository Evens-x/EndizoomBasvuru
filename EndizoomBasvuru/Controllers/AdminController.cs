using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EndizoomBasvuru.Services.Interfaces;
using EndizoomBasvuru.Services.Models;
using EndizoomBasvuru.Entity;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;

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

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
        [HttpGet("by-role/{role}")]
        public async Task<IActionResult> GetAdminsByRole(string role)
        {
            if (!Enum.TryParse<UserRole>(role, true, out var userRole))
            {
                return BadRequest(new { message = "Geçersiz rol değeri." });
            }

            var admins = await _adminService.GetAdminsByRoleAsync(userRole);
            return Ok(admins);
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
        [HttpGet("admin-and-marketing")]
        public async Task<IActionResult> GetAdminAndMarketingUsers()
        {
            var users = await _adminService.GetAdminAndMarketingUsersAsync();
            return Ok(users);
        }
        
        /// <summary>
        /// Admin'in yetkilendirmesi ile yönetici aktiflik durumunu (aktif/pasif) değiştirir
        /// </summary>
        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAdminStatus(int id, [FromBody] AdminStatusUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _adminService.UpdateAdminStatusAsync(id, model);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Belirli bir pazarlama kullanıcısının istatistiklerini gösterir
        /// </summary>
        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpGet("marketing/{id}/stats")]
        public async Task<IActionResult> GetMarketingUserStats(int id, [FromQuery] DateTime? date = null)
        {
            try
            {
                var stats = await _adminService.GetMarketingUserStatsAsync(id, date);
                return Ok(stats);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
        /// <summary>
        /// Tüm pazarlama kullanıcılarının istatistiklerini gösterir
        /// </summary>
        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpGet("marketing/stats")]
        public async Task<IActionResult> GetAllMarketingUsersStats([FromQuery] DateTime? date = null)
        {
            var stats = await _adminService.GetAllMarketingUsersStatsAsync(date);
            return Ok(stats);
        }
        
        /// <summary>
        /// Belirli bir pazarlama kullanıcısının eklediği şirketleri gösterir
        /// </summary>
        [Authorize(Roles = nameof(UserRole.Admin))]
        [HttpGet("marketing/{id}/companies")]
        public async Task<IActionResult> GetMarketingUserCompanies(int id)
        {
            try
            {
                // Önce kullanıcının Marketing rolüne sahip olup olmadığını kontrol et
                var admin = await _adminService.GetAdminByIdAsync(id);
                if (admin == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı." });
                }
                
                if (admin.Role != UserRole.Marketing)
                {
                    return BadRequest(new { message = "Bu kullanıcı bir pazarlama kullanıcısı değil." });
                }
                
                // CompanyService'i enjekte et ve pazarlamacının şirketlerini getir
                var companyService = HttpContext.RequestServices.GetService<ICompanyService>();
                if (companyService == null)
                {
                    return StatusCode(500, new { message = "Servis kullanılamıyor." });
                }
                
                var companies = await companyService.GetCompaniesByMarketingUserAsync(id);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Şirketler getirilirken bir hata oluştu: {ex.Message}" });
            }
        }
    }
}
