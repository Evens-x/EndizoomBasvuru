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
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CompanyRegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Firmayı kaydeden kişinin ID'sini al
            int createdById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (createdById == 0)
                return Unauthorized();

            // Check if email already exists
            if (await _companyService.CheckEmailExistsAsync(model.CompanyEmail))
                return BadRequest(new { message = "Bu e-posta adresi zaten kullanılıyor." });

            var result = await _companyService.RegisterCompanyAsync(model, createdById);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var company = await _companyService.AuthenticateAsync(model);
            if (company == null)
                return Unauthorized(new { message = "E-posta veya şifre yanlış." });

            return Ok(company);
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _companyService.ResetPasswordAsync(model);
            if (result == null)
                return BadRequest(new { message = "E-posta veya mevcut şifre yanlış." });

            return Ok(result);
        }


        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Company)},{nameof(UserRole.Marketing)}")]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int companyId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (companyId == 0)
                return Unauthorized();

            var result = await _companyService.ChangePasswordAsync(companyId, model);
            if (!result)
                return BadRequest(new { message = "Mevcut şifre yanlış." });

            return Ok(new { message = "Şifre başarıyla değiştirildi." });
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Company)},{nameof(UserRole.Marketing)}")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            int companyId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (companyId == 0)
                return Unauthorized();

            var profile = await _companyService.GetCompanyProfileAsync(companyId);
            if (profile == null)
                return NotFound(new { message = "Profil bulunamadı." });

            return Ok(profile);
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Company)},{nameof(UserRole.Marketing)}")]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int companyId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (companyId == 0)
                return Unauthorized();

            try
            {
                var result = await _companyService.UpdateProfileAsync(companyId, model);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "AdminOrMarketing")]

        [HttpGet("Get-All-Companies")]
        public async Task<IActionResult> GetAllCompanies()
        {
            // Developer hata ayıklama için rol bilgisi kontrolü
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            Console.WriteLine($"User Roles: {string.Join(", ", userRoles)}");
            
            var companies = await _companyService.GetAllCompaniesAsync();
            return Ok(companies);
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Company)},{nameof(UserRole.Marketing)}")]
        [HttpGet("filter")]
        public async Task<IActionResult> FilterCompanies([FromQuery] CompanyFilterDto filter)
        {
            var companies = await _companyService.FilterCompaniesAsync(filter);
            return Ok(companies);
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Company)},{nameof(UserRole.Marketing)}")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            var company = await _companyService.GetCompanyByIdAsync(id);
            if (company == null)
                return NotFound(new { message = "Firma bulunamadı." });

            return Ok(company);
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Company)},{nameof(UserRole.Marketing)}")]
        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateCompanyStatus(int id, [FromBody] CompanyStatusUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int updatedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (updatedById == 0)
                return Unauthorized();

            try
            {
                var result = await _companyService.UpdateCompanyStatusAsync(id, model, updatedById);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        
        // Dosya yükleme metodu için yeni DTO sınıfı oluştur
        public class CompanyImageUploadDto
        {
            public IFormFile Image { get; set; } = null!;
            public string? Description { get; set; }
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Company)},{nameof(UserRole.Marketing)}")]
        [HttpPost("{id}/images")]
        public async Task<IActionResult> AddCompanyImageByAdmin(int id, [FromForm] CompanyImageUploadDto model)
        {
            if (model.Image == null || model.Image.Length == 0)
                return BadRequest(new { message = "Lütfen bir resim yükleyin." });

            int adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (adminId == 0)
                return Unauthorized();

            try
            {
                var result = await _companyService.AddCompanyImageAsync(id, model.Image, model.Description);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Resim yüklenirken bir hata oluştu.", error = ex.Message });
            }
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Company)},{nameof(UserRole.Marketing)}")]
        [HttpPost("images")]
        public async Task<IActionResult> AddCompanyImage([FromForm] CompanyImageUploadDto model)
        {
            if (model.Image == null || model.Image.Length == 0)
                return BadRequest(new { message = "Lütfen bir resim yükleyin." });

            int companyId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (companyId == 0)
                return Unauthorized();

            try
            {
                var result = await _companyService.AddCompanyImageAsync(companyId, model.Image, model.Description);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Resim yüklenirken bir hata oluştu.", error = ex.Message });
            }
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Company)},{nameof(UserRole.Marketing)}")]
        [HttpDelete("images/{id}")]
        public async Task<IActionResult> DeleteCompanyImage(int id)
        {
            int companyId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (companyId == 0)
                return Unauthorized();

            var result = await _companyService.DeleteCompanyImageAsync(id, companyId);
            if (!result)
                return NotFound(new { message = "Resim bulunamadı." });

            return Ok(new { message = "Resim başarıyla silindi." });
        }
        
        // Sözleşme yükleme için DTO sınıfı
        public class ContractUploadDto
        {
            public IFormFile Contract { get; set; } = null!;
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Company)},{nameof(UserRole.Marketing)}")]
        [HttpPost("contract")]
        public async Task<IActionResult> UploadContract([FromForm] ContractUploadDto model)
        {
            if (model.Contract == null || model.Contract.Length == 0)
                return BadRequest(new { message = "Lütfen bir sözleşme yükleyin." });

            int companyId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (companyId == 0)
                return Unauthorized();

            try
            {
                var result = await _companyService.AddCompanyContractAsync(companyId, model.Contract);
                return Ok(new { filePath = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sözleşme yüklenirken bir hata oluştu.", error = ex.Message });
            }
        }

        /// <summary>
        /// Belirli bir pazarlama kullanıcısı tarafından eklenmiş şirketleri getirir
        /// </summary>
        /// <param name="marketingUserId">Pazarlama kullanıcısının ID'si</param>
        /// <returns>Pazarlama kullanıcısı tarafından eklenmiş şirketlerin listesi</returns>
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
        [HttpGet("by-marketing-user/{marketingUserId}")]
        public async Task<IActionResult> GetCompaniesByMarketingUser(int marketingUserId)
        {
            try
            {
                // Kullanıcı kendi rolü Marketing ise, sadece kendi eklediği şirketleri görebilir
                string? userRoleValue = User.FindFirst(ClaimTypes.Role)?.Value;
                int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                
                if (userRoleValue == UserRole.Marketing.ToString() && currentUserId != marketingUserId)
                {
                    return Forbid("Sadece kendi eklediğiniz şirketleri görüntüleyebilirsiniz.");
                }
                
                var companies = await _companyService.GetCompaniesByMarketingUserAsync(marketingUserId);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Şirketler getirilirken bir hata oluştu: {ex.Message}" });
            }
        }
        
        /// <summary>
        /// Giriş yapmış pazarlama kullanıcısının eklediği şirketleri getirir
        /// </summary>
        /// <returns>Giriş yapmış pazarlama kullanıcısının eklediği şirketlerin listesi</returns>
        [Authorize(Roles = nameof(UserRole.Marketing))]
        [HttpGet("my-companies")]
        public async Task<IActionResult> GetMyCompanies()
        {
            try
            {
                int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (currentUserId == 0)
                    return Unauthorized();
                
                var companies = await _companyService.GetCompaniesByMarketingUserAsync(currentUserId);
                return Ok(companies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Şirketler getirilirken bir hata oluştu: {ex.Message}" });
            }
        }

        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
        [HttpPut("{id}/update")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyUpdateDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int updatedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (updatedById == 0)
                return Unauthorized();

            try
            {
                var result = await _companyService.UpdateCompanyAsync(id, model, updatedById);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
