using EndizoomBasvuru.Entity;
using EndizoomBasvuru.Repository.Interfaces;
using EndizoomBasvuru.Services.Interfaces;
using EndizoomBasvuru.Services.Models;

namespace EndizoomBasvuru.Services
{
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<Admin> _adminRepository;
        private readonly TokenService _tokenService;

        public AdminService(
            IGenericRepository<Admin> adminRepository,
            TokenService tokenService)
        {
            _adminRepository = adminRepository;
            _tokenService = tokenService;
        }

        public async Task<AdminLoginResponseDto?> AuthenticateAsync(LoginDto model)
        {
            var admins = await _adminRepository.FindAsync(a => a.Email == model.Email);
            var admin = admins.FirstOrDefault();

            if (admin == null)
                return null;

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(model.Password, admin.Password))
                return null;

            // Update last login date
            admin.LastLoginAt = DateTime.UtcNow;
            await _adminRepository.UpdateAsync(admin);
            await _adminRepository.SaveChangesAsync();

            // Generate JWT token
            var token = _tokenService.GenerateAdminToken(admin);

            return new AdminLoginResponseDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                FullName = $"{admin.FirstName} {admin.LastName}",
                Token = token,
                Role = admin.Role
            };
        }
        
        public async Task<AdminResponseDto> RegisterAdminAsync(AdminRegisterDto model, int createdById)
        {
            // E-posta adresinin daha önce kullanılıp kullanılmadığını kontrol et
            var existingAdmins = await _adminRepository.FindAsync(a => a.Email == model.Email || a.Username == model.Username);
            if (existingAdmins.Any())
            {
                throw new InvalidOperationException("Bu e-posta adresi veya kullanıcı adı zaten kullanılıyor.");
            }
            
            // Şifreyi hash'le
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(model.Password);
            
            // Yeni admin kullanıcısını oluştur
            var admin = new Admin
            {
                Username = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Password = hashedPassword,
                Role = model.Role,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdById
            };
            
            // Veritabanına kaydet
            await _adminRepository.AddAsync(admin);
            await _adminRepository.SaveChangesAsync();
            
            // Yanıt DTO'sunu oluştur ve döndür
            return new AdminResponseDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                FullName = $"{admin.FirstName} {admin.LastName}",
                Role = admin.Role,
                CreatedAt = admin.CreatedAt
            };
        }
        
        public async Task<IEnumerable<AdminResponseDto>> GetAllAdminsAsync()
        {
            var admins = await _adminRepository.GetAllAsync();
            
            return admins.Select(a => new AdminResponseDto
            {
                Id = a.Id,
                Username = a.Username,
                Email = a.Email,
                FullName = $"{a.FirstName} {a.LastName}",
                Role = a.Role,
                CreatedAt = a.CreatedAt
            });
        }
        
        public async Task<AdminResponseDto?> GetAdminByIdAsync(int id)
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            
            if (admin == null)
                return null;
                
            return new AdminResponseDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                FullName = $"{admin.FirstName} {admin.LastName}",
                Role = admin.Role,
                CreatedAt = admin.CreatedAt
            };
        }
        
        public async Task<AdminResponseDto> UpdateAdminAsync(int id, AdminUpdateDto model)
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            
            if (admin == null)
                throw new KeyNotFoundException("Yönetici bulunamadı.");
                
            // E-posta veya kullanıcı adının başka bir kullanıcı tarafından kullanılıp kullanılmadığını kontrol et
            var existingAdmins = await _adminRepository.FindAsync(a => 
                (a.Email == model.Email || a.Username == model.Username) && a.Id != id);
                
            if (existingAdmins.Any())
            {
                throw new InvalidOperationException("Bu e-posta adresi veya kullanıcı adı başka bir kullanıcı tarafından kullanılıyor.");
            }
            
            // Admin bilgilerini güncelle
            admin.Username = model.Username;
            admin.Email = model.Email;
            admin.FirstName = model.FirstName;
            admin.LastName = model.LastName;
            admin.Role = model.Role;
            
            // Veritabanını güncelle
            await _adminRepository.UpdateAsync(admin);
            await _adminRepository.SaveChangesAsync();
            
            // Yanıt DTO'sunu oluştur ve döndür
            return new AdminResponseDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                FullName = $"{admin.FirstName} {admin.LastName}",
                Role = admin.Role,
                CreatedAt = admin.CreatedAt
            };
        }
        
        public async Task<bool> DeleteAdminAsync(int id)
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            
            if (admin == null)
                return false;
                
            await _adminRepository.DeleteAsync(admin);
            await _adminRepository.SaveChangesAsync();
            
            return true;
        }
        
        public async Task<bool> ChangePasswordAsync(int adminId, ChangePasswordDto model)
        {
            var admin = await _adminRepository.GetByIdAsync(adminId);
            
            if (admin == null)
                return false;
                
            // Mevcut şifreyi kontrol et
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, admin.Password))
                return false;
                
            // Yeni şifreyi hash'le ve güncelle
            admin.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            
            await _adminRepository.UpdateAsync(admin);
            await _adminRepository.SaveChangesAsync();
            
            return true;
        }
    }
} 