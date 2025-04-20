using EndizoomBasvuru.Entity;
using EndizoomBasvuru.Repository.Interfaces;
using EndizoomBasvuru.Services.Interfaces;
using EndizoomBasvuru.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace EndizoomBasvuru.Services
{
    public class AdminService : IAdminService
    {
        private readonly IGenericRepository<Admin> _adminRepository;
        private readonly IGenericRepository<Company> _companyRepository;
        private readonly TokenService _tokenService;

        public AdminService(
            IGenericRepository<Admin> adminRepository,
            IGenericRepository<Company> companyRepository,
            TokenService tokenService)
        {
            _adminRepository = adminRepository;
            _companyRepository = companyRepository;
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
                
            // Check if user is active
            if (!admin.IsActive)
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
                Role = admin.Role,
                IsActive = admin.IsActive
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
                PhoneNumber = model.PhoneNumber,
                CompanyNumber = model.CompanyNumber,
                Assignments = model.Assignments,
                Password = hashedPassword,
                Role = model.Role,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdById,
                IsActive = true
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
                PhoneNumber = admin.PhoneNumber,
                CompanyNumber = admin.CompanyNumber,
                Role = admin.Role,
                CreatedAt = admin.CreatedAt,
                IsActive = admin.IsActive
            };
        }
        
        public async Task<IEnumerable<AdminResponseDto>> GetAllAdminsAsync()
        {
            var admins = await _adminRepository.GetAllAsync();
            
            // Rol bazlı sıralama: önce Admin, sonra Marketing kullanıcıları
            return admins
                .OrderBy(a => a.Role != UserRole.Admin) // Admin'ler ilk önce
                .ThenBy(a => a.Role != UserRole.Marketing) // Marketing ikinci
                .ThenBy(a => a.FirstName) // Sonra alfabetik sıra
                .Select(a => new AdminResponseDto
                {
                    Id = a.Id,
                    Username = a.Username,
                    Email = a.Email,
                    FullName = $"{a.FirstName} {a.LastName}",
                    PhoneNumber = a.PhoneNumber,
                    CompanyNumber = a.CompanyNumber,
                    Role = a.Role,
                    CreatedAt = a.CreatedAt,
                    IsActive = a.IsActive,
                    LastLoginAt = a.LastLoginAt
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
                PhoneNumber = admin.PhoneNumber,
                CompanyNumber = admin.CompanyNumber,
                Role = admin.Role,
                CreatedAt = admin.CreatedAt,
                IsActive = admin.IsActive,
                LastLoginAt = admin.LastLoginAt
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
            admin.PhoneNumber = model.PhoneNumber;
            admin.CompanyNumber = model.CompanyNumber;
            admin.Role = model.Role;
            admin.IsActive = model.IsActive;
            
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
                PhoneNumber = admin.PhoneNumber,
                CompanyNumber = admin.CompanyNumber,
                Role = admin.Role,
                CreatedAt = admin.CreatedAt,
                IsActive = admin.IsActive,
                LastLoginAt = admin.LastLoginAt
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

        /// <summary>
        /// Belirli roldeki kullanıcıları getirir
        /// </summary>
        /// <param name="role">Filtrelenecek kullanıcı rolü</param>
        /// <returns>Belirtilen roldeki kullanıcı listesi</returns>
        public async Task<IEnumerable<AdminResponseDto>> GetAdminsByRoleAsync(UserRole role)
        {
            var admins = await _adminRepository.FindAsync(a => a.Role == role);
            
            return admins.Select(a => new AdminResponseDto
            {
                Id = a.Id,
                Username = a.Username,
                Email = a.Email,
                FullName = $"{a.FirstName} {a.LastName}",
                PhoneNumber = a.PhoneNumber,
                CompanyNumber = a.CompanyNumber,
                Role = a.Role,
                CreatedAt = a.CreatedAt,
                IsActive = a.IsActive,
                LastLoginAt = a.LastLoginAt
            });
        }

        /// <summary>
        /// Admin ve Pazarlama rolündeki tüm kullanıcıları getirir
        /// </summary>
        /// <returns>Admin ve Pazarlama rolündeki kullanıcı listesi</returns>
        public async Task<IEnumerable<AdminResponseDto>> GetAdminAndMarketingUsersAsync()
        {
            var admins = await _adminRepository.FindAsync(a => 
                a.Role == UserRole.Admin || a.Role == UserRole.Marketing);
            
            return admins
                .OrderBy(a => a.Role) // Önce Admin, sonra Marketing
                .ThenBy(a => a.FirstName) // Sonra alfabetik
                .Select(a => new AdminResponseDto
                {
                    Id = a.Id,
                    Username = a.Username,
                    Email = a.Email,
                    FullName = $"{a.FirstName} {a.LastName}",
                    PhoneNumber = a.PhoneNumber,
                    CompanyNumber = a.CompanyNumber,
                    Role = a.Role,
                    CreatedAt = a.CreatedAt,
                    IsActive = a.IsActive,
                    LastLoginAt = a.LastLoginAt
                });
        }

        /// <summary>
        /// Admin aktiflik durumunu günceller (aktif/pasif)
        /// </summary>
        /// <param name="id">Admin ID</param>
        /// <param name="model">Aktiflik durumu bilgisi</param>
        /// <returns>Güncellenmiş admin bilgileri</returns>
        public async Task<AdminResponseDto> UpdateAdminStatusAsync(int id, AdminStatusUpdateDto model)
        {
            var admin = await _adminRepository.GetByIdAsync(id);
            
            if (admin == null)
                throw new KeyNotFoundException("Yönetici bulunamadı.");
                
            // Durumu güncelle
            admin.IsActive = model.IsActive;
            
            // Veritabanını güncelle
            await _adminRepository.UpdateAsync(admin);
            await _adminRepository.SaveChangesAsync();
            
            // Güncellenmiş admin bilgilerini döndür
            return new AdminResponseDto
            {
                Id = admin.Id,
                Username = admin.Username,
                Email = admin.Email,
                FullName = $"{admin.FirstName} {admin.LastName}",
                Role = admin.Role,
                CreatedAt = admin.CreatedAt,
                IsActive = admin.IsActive,
                LastLoginAt = admin.LastLoginAt
            };
        }

        /// <summary>
        /// Belirli bir pazarlama kullanıcısının istatistiklerini getirir
        /// </summary>
        /// <param name="userId">Pazarlama kullanıcı ID'si</param>
        /// <param name="date">İstatistik tarihi (bugün için null)</param>
        /// <returns>Pazarlama kullanıcısının istatistikleri</returns>
        public async Task<MarketingUserStatsDto> GetMarketingUserStatsAsync(int userId, DateTime? date = null)
        {
            // Tarih boş ise bugünü kullan
            date ??= DateTime.UtcNow;
            
            // Kullanıcıyı veritabanından getir
            var admin = await _adminRepository.GetByIdAsync(userId);
            
            if (admin == null)
                throw new KeyNotFoundException("Kullanıcı bulunamadı.");
                
            // Bugünün başlangıç ve bitiş zamanları
            var todayStart = date.Value.Date;
            var todayEnd = todayStart.AddDays(1).AddTicks(-1);
            
            // Bu ayın başlangıç ve bitiş zamanları
            var monthStart = new DateTime(date.Value.Year, date.Value.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddTicks(-1);
            
            // Kullanıcı tarafından eklenen tüm firmaları getir
            var companies = await _companyRepository.FindAsync(c => c.CreatedById == userId);
            
            // Günlük eklenen firmalar
            var dailyCompanies = companies.Where(c => c.CreatedAt >= todayStart && c.CreatedAt <= todayEnd);
            
            // Aylık eklenen firmalar
            var monthlyCompanies = companies.Where(c => c.CreatedAt >= monthStart && c.CreatedAt <= monthEnd);
            
            // İstatistik DTO'sunu oluştur
            var stats = new MarketingUserStatsDto
            {
                AdminId = admin.Id,
                FullName = $"{admin.FirstName} {admin.LastName}",
                Email = admin.Email,
                IsActive = admin.IsActive,
                
                // Toplam istatistikler
                TotalCompanies = companies.Count(),
                TotalActiveCompanies = companies.Count(c => c.ConnectionStatus == CompanyConnectionStatus.Active),
                TotalRejectedCompanies = companies.Count(c => c.ConnectionStatus == CompanyConnectionStatus.Rejected),
                TotalPendingCompanies = companies.Count(c => c.ConnectionStatus == CompanyConnectionStatus.Pending),
                
                // Günlük istatistikler
                DailyCompanies = dailyCompanies.Count(),
                DailyActiveCompanies = dailyCompanies.Count(c => c.ConnectionStatus == CompanyConnectionStatus.Active),
                DailyRejectedCompanies = dailyCompanies.Count(c => c.ConnectionStatus == CompanyConnectionStatus.Rejected),
                
                // Aylık istatistikler
                MonthlyCompanies = monthlyCompanies.Count(),
                MonthlyActiveCompanies = monthlyCompanies.Count(c => c.ConnectionStatus == CompanyConnectionStatus.Active),
                MonthlyRejectedCompanies = monthlyCompanies.Count(c => c.ConnectionStatus == CompanyConnectionStatus.Rejected)
            };
            
            return stats;
        }

        /// <summary>
        /// Tüm pazarlama kullanıcılarının istatistiklerini getirir
        /// </summary>
        /// <param name="date">İstatistik tarihi (bugün için null)</param>
        /// <returns>Pazarlama kullanıcılarının istatistiklerinin listesi</returns>
        public async Task<IEnumerable<MarketingUserStatsDto>> GetAllMarketingUsersStatsAsync(DateTime? date = null)
        {
            // Pazarlama kullanıcılarını getir
            var marketingUsers = await _adminRepository.FindAsync(a => a.Role == UserRole.Marketing);
            
            // Her pazarlama kullanıcısı için istatistikleri hesapla
            var statsList = new List<MarketingUserStatsDto>();
            foreach (var user in marketingUsers)
            {
                var stats = await GetMarketingUserStatsAsync(user.Id, date);
                statsList.Add(stats);
            }
            
            // İstatistikleri toplam firma sayısına göre sırala (çoktan aza)
            return statsList.OrderByDescending(s => s.TotalCompanies);
        }
    }
} 