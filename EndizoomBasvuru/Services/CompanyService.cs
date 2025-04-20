using EndizoomBasvuru.Data.Context;
using EndizoomBasvuru.Entity;
using EndizoomBasvuru.Repository.Interfaces;
using EndizoomBasvuru.Services.Interfaces;
using EndizoomBasvuru.Services.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace EndizoomBasvuru.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IGenericRepository<Company> _companyRepository;
        private readonly IGenericRepository<CompanyImage> _companyImageRepository;
        private readonly IEmailService _emailService;
        private readonly TokenService _tokenService;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public CompanyService(
            IGenericRepository<Company> companyRepository,
            IGenericRepository<CompanyImage> companyImageRepository,
            IEmailService emailService,
            TokenService tokenService,
            IWebHostEnvironment hostingEnvironment)
        {
            _companyRepository = companyRepository;
            _companyImageRepository = companyImageRepository;
            _emailService = emailService;
            _tokenService = tokenService;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<CompanyLoginResponseDto?> AuthenticateAsync(LoginDto model)
        {
            var companies = await _companyRepository.FindAsync(c => c.Email == model.Email);
            var company = companies.FirstOrDefault();

            if (company == null)
                return null;

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(model.Password, company.Password))
                return null;

            // Update last login date
            company.UpdatedAt = DateTime.UtcNow;
            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();

            // Generate JWT token
            var token = _tokenService.GenerateCompanyToken(company);

            return new CompanyLoginResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                Email = company.Email,
                Token = token
            };
        }

        public async Task<bool> ChangePasswordAsync(int companyId, ChangePasswordDto model)
        {
            var companies = await _companyRepository.FindAsync(c => c.Id == companyId);
            var company = companies.FirstOrDefault();

            if (company == null)
                return false;

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, company.Password))
                return false;

            // Update with new password
            company.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _companyRepository.ExistsAsync(c => c.Email == email);
        }

        public async Task<IEnumerable<Company>> FindByEmailAsync(string email)
        {
            return await _companyRepository.FindAsync(c => c.Email == email);
        }

        public async Task<CompanyLoginResponseDto?> ResetPasswordAsync(ResetPasswordDto model)
        {
            var companies = await _companyRepository.FindAsync(c => c.Email == model.Email);
            var company = companies.FirstOrDefault();

            if (company == null)
                return null;

            // Verify current password
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, company.Password))
                return null;

            // Update with new password
            company.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            company.UpdatedAt = DateTime.UtcNow;
            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();

            // Generate JWT token
            var token = _tokenService.GenerateCompanyToken(company);

            return new CompanyLoginResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                Email = company.Email,
                Token = token
            };
        }

        public async Task<IEnumerable<CompanyResponseDto>> GetAllCompaniesAsync()
        {
            // Include ile ilişkili verileri de getir
            var companies = await _companyRepository.GetQueryable()
                .Include(c => c.CreatedBy)
                .Include(c => c.Images)
                .ToListAsync();
            
            // Şirketleri ve bağlı verileri içeren daha zengin bir yanıt oluştur
            return companies.Select(c => new CompanyResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                CompanyTitle = c.Title,
                TaxNumber = c.TaxNumber,
                Email = c.Email,
                ContactFullName = $"{c.ContactFirstName} {c.ContactLastName}",
                ContactPosition = c.ContactPosition,
                ContactPhone = c.ContactPhone,
                ContactEmail = c.ContactEmail,
                ItResponsibleName = c.ItResponsibleName,
                ItResponsiblePhone = c.ItResponsiblePhone,
                ItResponsibleEmail = c.ItResponsibleEmail,
                ProductionCapacity = c.ProductionCapacity,
                Region = c.Region,
                PackageType = c.PackageType,
                ConnectionStatus = c.ConnectionStatus.ToString(),
                Notes = c.Notes,
                Status = c.ConnectionStatus.ToString(),
                ContractPdfPath = c.ContractPath,
                IsTemplate = false,
                Images = c.Images?.Select(i => new CompanyImageDto
                {
                    Id = i.Id,
                    FilePath = i.FilePath,
                    Description = i.Description,
                    UploadDate = i.UploadDate
                }).ToList() ?? new List<CompanyImageDto>(),
                CreatedAt = c.CreatedAt,
                CreatedByName = c.CreatedBy != null ? $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}" : null
            });
        }

        public async Task<CompanyResponseDto?> GetCompanyByIdAsync(int id)
        {
            var company = await _companyRepository.GetQueryable()
                .Include(c => c.CreatedBy)
                .Include(c => c.Images)
                .FirstOrDefaultAsync(c => c.Id == id);
                
            if (company == null)
                return null;

            // Şirketin tüm detaylarını içeren zengin bir yanıt oluştur
            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                CompanyTitle = company.Title,
                TaxNumber = company.TaxNumber,
                Email = company.Email,
                ContactFullName = $"{company.ContactFirstName} {company.ContactLastName}",
                ContactPosition = company.ContactPosition,
                ContactPhone = company.ContactPhone,
                ContactEmail = company.ContactEmail,
                ItResponsibleName = company.ItResponsibleName,
                ItResponsiblePhone = company.ItResponsiblePhone,
                ItResponsibleEmail = company.ItResponsibleEmail,
                ProductionCapacity = company.ProductionCapacity,
                Region = company.Region,
                PackageType = company.PackageType,
                ConnectionStatus = company.ConnectionStatus.ToString(),
                Notes = company.Notes,
                Status = company.ConnectionStatus.ToString(),
                ContractPdfPath = company.ContractPath,
                IsTemplate = false,
                Images = company.Images?.Select(i => new CompanyImageDto
                {
                    Id = i.Id,
                    FilePath = i.FilePath,
                    Description = i.Description,
                    UploadDate = i.UploadDate
                }).ToList() ?? new List<CompanyImageDto>(),
                CreatedAt = company.CreatedAt,
                CreatedByName = company.CreatedBy != null ? $"{company.CreatedBy.FirstName} {company.CreatedBy.LastName}" : null
            };
        }

        public async Task<CompanyResponseDto?> GetCompanyProfileAsync(int companyId)
        {
            return await GetCompanyByIdAsync(companyId);
        }

        public async Task<CompanyResponseDto> RegisterCompanyAsync(CompanyRegisterDto model, int createdById)
        {
            // Generate a random password
            string randomPassword = GenerateRandomPassword();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(randomPassword);

            // Enum dönüşümü için varsayılan değer (beklemede)
            var connectionStatus = CompanyConnectionStatus.Pending;
            
            // Create new company
            var company = new Company
            {
                Name = model.CompanyName,
                Title = model.CompanyTitle,
                TaxNumber = model.TaxNumber,
                Email = model.CompanyEmail,
                ContactFirstName = model.ContactFirstName,
                ContactLastName = model.ContactLastName,
                ContactPosition = model.ContactPosition,
                ContactPhone = model.ContactPhone,
                ContactEmail = model.ContactEmail,
                ItResponsibleName = model.ItResponsibleName,
                ItResponsiblePhone = model.ItResponsiblePhone,
                ItResponsibleEmail = model.ItResponsibleEmail,
                ProductionCapacity = model.ProductionCapacity,
                Region = model.Region,
                PackageType = model.PackageType,
                ConnectionStatus = connectionStatus,
                Notes = model.Notes,
                Password = hashedPassword,
                CreatedAt = DateTime.UtcNow,
                CreatedById = createdById
            };

            // Save company to database
            await _companyRepository.AddAsync(company);
            await _companyRepository.SaveChangesAsync();

            // Şirket kaydedildikten sonra yetkiliyi de içerecek şekilde tekrar getir
            var savedCompany = await _companyRepository.GetQueryable()
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == company.Id);

            // Upload contract if exists
            if (model.ContractPdf != null)
            {
                await AddCompanyContractAsync(company.Id, model.ContractPdf);
            }

            // Upload images if exists
            if (model.Images != null && model.Images.Length > 0)
            {
                foreach (var image in model.Images)
                {
                    await AddCompanyImageAsync(company.Id, image, null);
                }
            }

            string contactFullName = "";
            if (company.ContactFirstName != null || company.ContactLastName != null)
            {
                contactFullName = $"{company.ContactFirstName} {company.ContactLastName}";
            }
            
            // Send welcome email with random password
            await _emailService.SendWelcomeEmailAsync(
                company.Email,
                contactFullName,
                randomPassword);

            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                CompanyTitle = company.Title,
                TaxNumber = company.TaxNumber,
                Email = company.Email,
                ContactFullName = contactFullName,
                ContactPosition = company.ContactPosition,
                ContactPhone = company.ContactPhone,
                ContactEmail = company.ContactEmail,
                ItResponsibleName = company.ItResponsibleName,
                ItResponsiblePhone = company.ItResponsiblePhone,
                ItResponsibleEmail = company.ItResponsibleEmail,
                ProductionCapacity = company.ProductionCapacity,
                Region = company.Region,
                PackageType = company.PackageType,
                ConnectionStatus = company.ConnectionStatus.ToString(),
                Notes = company.Notes,
                CreatedAt = company.CreatedAt,
                CreatedByName = savedCompany.CreatedBy != null ? $"{savedCompany.CreatedBy.FirstName} {savedCompany.CreatedBy.LastName}" : null
            };
        }

        public async Task<CompanyResponseDto> UpdateProfileAsync(int companyId, UpdateProfileDto model)
        {
            var companies = await _companyRepository.FindAsync(c => c.Id == companyId);
            var company = companies.FirstOrDefault();

            if (company == null)
                throw new KeyNotFoundException("Firma bulunamadı.");

            company.Name = model.CompanyName;
            company.Title = model.CompanyTitle;
            company.ContactFirstName = model.ContactFirstName;
            company.ContactLastName = model.ContactLastName;
            company.ContactPosition = model.ContactPosition;
            company.ContactPhone = model.ContactPhone;
            company.ItResponsibleName = model.ItResponsibleName;
            company.ItResponsiblePhone = model.ItResponsiblePhone;
            company.ItResponsibleEmail = model.ItResponsibleEmail;
            company.ProductionCapacity = model.ProductionCapacity;
            company.Region = model.Region;
            company.PackageType = model.PackageType;
            company.UpdatedAt = DateTime.UtcNow;

            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();

            // Güncellenmiş firmayı ilişkili verilerle birlikte getir
            var updatedCompany = await _companyRepository.GetQueryable()
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                CompanyTitle = company.Title,
                TaxNumber = company.TaxNumber,
                Email = company.Email,
                ContactFullName = $"{company.ContactFirstName} {company.ContactLastName}",
                ContactPosition = company.ContactPosition,
                ContactPhone = company.ContactPhone,
                ContactEmail = company.ContactEmail,
                ItResponsibleName = company.ItResponsibleName,
                ItResponsiblePhone = company.ItResponsiblePhone,
                ItResponsibleEmail = company.ItResponsibleEmail,
                ProductionCapacity = company.ProductionCapacity,
                Region = company.Region,
                PackageType = company.PackageType,
                CreatedAt = company.CreatedAt,
                UpdatedAt = company.UpdatedAt,
                CreatedByName = updatedCompany.CreatedBy != null ? $"{updatedCompany.CreatedBy.FirstName} {updatedCompany.CreatedBy.LastName}" : null
            };
        }

        private string GenerateRandomPassword()
        {
            // Hem rakam hem harf içeren 8 karakterli güçlü şifre oluştur
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghijklmnopqrstuvwxyz";
            var random = new Random();
            var password = new string(
                Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)])
                .ToArray());

            return password;
        }

        public async Task<CompanyImageDto> AddCompanyImageAsync(int companyId, IFormFile image, string? description)
        {
            // Firma varlığını kontrol et
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
                throw new KeyNotFoundException("Firma bulunamadı.");

            // Firma adı, tarihi ve türünü kullanarak anlamlı bir dosya adı oluştur
            string safeCompanyName = string.Join("_", company.Name.Split(Path.GetInvalidFileNameChars()));
            string fileExtension = Path.GetExtension(image.FileName);
            string dateStr = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(image.FileName);
            
            // Dosya adını formatla: Firma-Adı_Tarih_Açıklama.uzantı
            string fileName = $"{safeCompanyName}_{dateStr}";
            
            // Eğer açıklama varsa, dosya adına ekle
            if (!string.IsNullOrWhiteSpace(description))
            {
                string safeDescription = string.Join("_", description.Split(Path.GetInvalidFileNameChars()));
                fileName += $"_{safeDescription}";
            }
            
            // Uzantıyı ekle
            fileName += fileExtension;
            
            // Firma görsellerinin kaydedileceği klasör
            string companyImagesFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads", "companies", companyId.ToString(), "images");
            
            // Klasör yoksa oluştur
            if (!Directory.Exists(companyImagesFolder))
            {
                Directory.CreateDirectory(companyImagesFolder);
            }
            
            // Dosya yolu oluştur ve dosyayı kaydet
            string filePath = Path.Combine(companyImagesFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            
            // Veritabanında resim kaydını oluştur
            var companyImage = new CompanyImage
            {
                FilePath = Path.Combine("uploads", "companies", companyId.ToString(), "images", fileName),
                Description = description ?? string.Empty,
                UploadDate = DateTime.UtcNow,
                CompanyId = companyId
            };
            
            await _companyImageRepository.AddAsync(companyImage);
            await _companyImageRepository.SaveChangesAsync();
            
            // DTO'yu döndür
            return new CompanyImageDto
            {
                Id = companyImage.Id,
                FilePath = companyImage.FilePath,
                Description = companyImage.Description,
                UploadDate = companyImage.UploadDate
            };
        }

        public async Task<bool> DeleteCompanyImageAsync(int imageId, int companyId)
        {
            var images = await _companyImageRepository.FindAsync(i => i.Id == imageId && i.CompanyId == companyId);
            var image = images.FirstOrDefault();
            
            if (image == null)
                return false;
                
            // Fiziksel dosyayı sil
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, image.FilePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            
            // Veritabanından kaydı sil
            await _companyImageRepository.DeleteAsync(image);
            await _companyImageRepository.SaveChangesAsync();
            
            return true;
        }

        public async Task<string> AddCompanyContractAsync(int companyId, IFormFile contract)
        {
            // Firma varlığını kontrol et
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
                throw new KeyNotFoundException("Firma bulunamadı.");
                
            // Firma adı, tarihi ve türünü kullanarak anlamlı bir dosya adı oluştur
            string safeCompanyName = string.Join("_", company.Name.Split(Path.GetInvalidFileNameChars()));
            string fileExtension = Path.GetExtension(contract.FileName);
            string dateStr = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            
            // Dosya adını formatla: Firma-Adı_Sözleşme_Tarih.uzantı
            string fileName = $"{safeCompanyName}_Sozlesme_{dateStr}{fileExtension}";
            
            // Firma sözleşmelerinin kaydedileceği klasör
            string companyContractsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads", "companies", companyId.ToString(), "contracts");
            
            // Klasör yoksa oluştur
            if (!Directory.Exists(companyContractsFolder))
            {
                Directory.CreateDirectory(companyContractsFolder);
            }
            
            // Dosya yolu oluştur ve dosyayı kaydet
            string filePath = Path.Combine(companyContractsFolder, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await contract.CopyToAsync(fileStream);
            }
            
            string contractPath = Path.Combine("uploads", "companies", companyId.ToString(), "contracts", fileName);
            
            // Firma kaydında sözleşme yolunu güncelle
            company.ContractPath = contractPath;
            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();
            
            return contractPath;
        }

        public async Task<CompanyResponseDto> UpdateCompanyStatusAsync(int companyId, CompanyStatusUpdateDto model, int updatedById)
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            
            if (company == null)
                throw new KeyNotFoundException("Firma bulunamadı.");
             
            // Şirket durumunu güncelle - bu durumda ConnectionStatus olarak kabul ediyoruz
            if (model.Status == "Active")
                company.ConnectionStatus = CompanyConnectionStatus.Active;
            else if (model.Status == "Rejected")
                company.ConnectionStatus = CompanyConnectionStatus.Rejected;
            else
                company.ConnectionStatus = CompanyConnectionStatus.Pending;
                
            // Varsa notları güncelle
            if (!string.IsNullOrEmpty(model.Notes))
            {
                company.Notes = model.Notes;
            }
            
            // Güncelleyen kişiyi ekle
            company.UpdatedAt = DateTime.UtcNow;
            
            // Güncelleme için veritabanını güncelle
            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();
            
            // Firma bilgilerini ilişkili verilerle birlikte al
            var updatedCompany = await _companyRepository.GetQueryable()
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == companyId);
            
            // DTO olarak güncel şirket bilgilerini döndür
            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                CompanyTitle = company.Title,
                TaxNumber = company.TaxNumber,
                Email = company.Email,
                Status = company.ConnectionStatus.ToString(),
                ConnectionStatus = company.ConnectionStatus.ToString(),
                Notes = company.Notes,
                CreatedAt = company.CreatedAt,
                CreatedByName = updatedCompany.CreatedBy != null ? $"{updatedCompany.CreatedBy.FirstName} {updatedCompany.CreatedBy.LastName}" : null
            };
        }

        public async Task<IEnumerable<CompanyResponseDto>> FilterCompaniesAsync(CompanyFilterDto filter)
        {
            // Şirketleri ilişkili verilerle birlikte getir
            var query = _companyRepository.GetQueryable()
                .Include(c => c.CreatedBy)
                .Include(c => c.Images)
                .AsQueryable();
            
            // İsim filtresi
            if (!string.IsNullOrEmpty(filter.Name))
            {
                query = query.Where(c => c.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
            }
            
            // Bölge filtresi
            if (!string.IsNullOrEmpty(filter.Region))
            {
                query = query.Where(c => c.Region == filter.Region);
            }
            
            // Bağlantı durumu filtresi
            if (!string.IsNullOrEmpty(filter.ConnectionStatus))
            {
                if (Enum.TryParse<CompanyConnectionStatus>(filter.ConnectionStatus, out var status))
                {
                    query = query.Where(c => c.ConnectionStatus == status);
                }
            }
            
            // Durum filtresi - artık connection status'u kullanıyoruz
            if (!string.IsNullOrEmpty(filter.Status))
            {
                if (Enum.TryParse<CompanyConnectionStatus>(filter.Status, out var status))
                {
                    query = query.Where(c => c.ConnectionStatus == status);
                }
            }
            
            // Tarih aralığı filtreleri
            if (filter.FromDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= filter.FromDate.Value);
            }
            
            if (filter.ToDate.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= filter.ToDate.Value);
            }
            
            // Oluşturan kişi filtresi
            if (!string.IsNullOrEmpty(filter.CreatedBy))
            {
                // CreatedById olan şirketleri filtreliyoruz
                query = query.Where(c => c.CreatedById.HasValue);
            }

            // Sonuçları getir
            var companies = await query.ToListAsync();
            
            // Sonuçları dönüştür
            return companies.Select(c => new CompanyResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                CompanyTitle = c.Title,
                TaxNumber = c.TaxNumber,
                Email = c.Email,
                ContactFullName = $"{c.ContactFirstName} {c.ContactLastName}",
                ContactPosition = c.ContactPosition,
                ContactPhone = c.ContactPhone,
                ContactEmail = c.ContactEmail,
                ItResponsibleName = c.ItResponsibleName,
                ItResponsiblePhone = c.ItResponsiblePhone,
                ItResponsibleEmail = c.ItResponsibleEmail,
                ProductionCapacity = c.ProductionCapacity,
                Region = c.Region,
                PackageType = c.PackageType,
                Status = c.ConnectionStatus.ToString(),
                ConnectionStatus = c.ConnectionStatus.ToString(),
                Notes = c.Notes,
                ContractPdfPath = c.ContractPath,
                IsTemplate = false,
                Images = c.Images?.Select(i => new CompanyImageDto
                {
                    Id = i.Id,
                    FilePath = i.FilePath,
                    Description = i.Description,
                    UploadDate = i.UploadDate
                }).ToList() ?? new List<CompanyImageDto>(),
                CreatedAt = c.CreatedAt,
                CreatedByName = c.CreatedBy != null ? $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}" : null
            });
        }

        #region İstatistik Metodları

        /// <summary>
        /// Belirli bir dönem için firma sayısı istatistiklerini getirir
        /// </summary>
        public async Task<CompanyCountStatisticsDto> GetCompanyCountStatisticsAsync(StatisticPeriodType periodType, DateTime? date = null)
        {
            DateTime referenceDate = date ?? DateTime.UtcNow;
            DateTime startDate, endDate;

            // Dönem başlangıç ve bitiş tarihlerini belirle
            if (periodType == StatisticPeriodType.Daily)
            {
                startDate = referenceDate.Date;
                endDate = startDate.AddDays(1).AddSeconds(-1);
            }
            else // Monthly
            {
                startDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);
                endDate = startDate.AddMonths(1).AddSeconds(-1);
            }

            // Firmalar ve yöneticiler için temel sorgu
            var companies = await _companyRepository.GetQueryable()
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .Include(c => c.CreatedBy)
                .ToListAsync();

            // Toplam firma sayısı
            int totalCount = companies.Count();

            // Yönetici bazında firma sayıları
            var detailsByAdmin = companies
                .Where(c => c.CreatedBy != null)
                .GroupBy(c => c.CreatedBy!)
                .Select(g => new AdminCompanyCountDto
                {
                    AdminId = g.Key.Id,
                    AdminName = $"{g.Key.FirstName} {g.Key.LastName}",
                    AdminRole = g.Key.Role.ToString(),
                    CompanyCount = g.Count()
                })
                .ToList();

            return new CompanyCountStatisticsDto
            {
                TotalCount = totalCount,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                DetailsByAdmin = detailsByAdmin
            };
        }

        /// <summary>
        /// Belirli bir dönem için finansal istatistikleri getirir
        /// </summary>
        public async Task<FinancialStatisticsDto> GetFinancialStatisticsAsync(StatisticPeriodType periodType, DateTime? date = null)
        {
            DateTime referenceDate = date ?? DateTime.UtcNow;
            DateTime startDate, endDate;

            // Dönem başlangıç ve bitiş tarihlerini belirle
            if (periodType == StatisticPeriodType.Daily)
            {
                startDate = referenceDate.Date;
                endDate = startDate.AddDays(1).AddSeconds(-1);
            }
            else // Monthly
            {
                startDate = new DateTime(referenceDate.Year, referenceDate.Month, 1);
                endDate = startDate.AddMonths(1).AddSeconds(-1);
            }

            // Firmalar ve yöneticiler için temel sorgu
            var companies = await _companyRepository.GetQueryable()
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate)
                .Include(c => c.CreatedBy)
                .ToListAsync();

            // Toplam ciro ve komisyon
            decimal totalRevenue = companies.Sum(c => c.Revenue);
            decimal totalCommission = companies.Sum(c => c.Commission);

            // Yönetici bazında finansal veriler
            var detailsByAdmin = companies
                .Where(c => c.CreatedBy != null)
                .GroupBy(c => c.CreatedBy!)
                .Select(g => new AdminFinancialStatisticsDto
                {
                    AdminId = g.Key.Id,
                    AdminName = $"{g.Key.FirstName} {g.Key.LastName}",
                    AdminRole = g.Key.Role.ToString(),
                    Revenue = g.Sum(c => c.Revenue),
                    Commission = g.Sum(c => c.Commission)
                })
                .ToList();

            return new FinancialStatisticsDto
            {
                TotalRevenue = totalRevenue,
                TotalCommission = totalCommission,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                DetailsByAdmin = detailsByAdmin
            };
        }

        /// <summary>
        /// En son eklenen firmaları getirir
        /// </summary>
        public async Task<RecentCompaniesDto> GetRecentCompaniesAsync(int count = 10)
        {
            var companies = await _companyRepository.GetQueryable()
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .Include(c => c.CreatedBy)
                .ToListAsync();

            var result = new RecentCompaniesDto
            {
                Companies = companies.Select(c => new CompanyListItemDto
                {
                    Id = c.Id,
                    Name = c.Name ?? string.Empty,
                    Email = c.Email,
                    CreatedAt = c.CreatedAt,
                    Status = c.ConnectionStatus.ToString(),
                    Revenue = c.Revenue,
                    Commission = c.Commission,
                    AdminName = c.CreatedBy != null ? $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}" : "Sistem"
                }),
                TotalCount = companies.Count()
            };

            return result;
        }

        /// <summary>
        /// Son X gün içinde eklenen firmaları getirir
        /// </summary>
        public async Task<RecentCompaniesDto> GetNewCompaniesAsync(int days = 7, int count = 10)
        {
            var startDate = DateTime.UtcNow.AddDays(-days).Date;
            
            var companies = await _companyRepository.GetQueryable()
                .Where(c => c.CreatedAt >= startDate)
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .Include(c => c.CreatedBy)
                .ToListAsync();

            var result = new RecentCompaniesDto
            {
                Companies = companies.Select(c => new CompanyListItemDto
                {
                    Id = c.Id,
                    Name = c.Name ?? string.Empty,
                    Email = c.Email,
                    CreatedAt = c.CreatedAt,
                    Status = c.ConnectionStatus.ToString(),
                    Revenue = c.Revenue,
                    Commission = c.Commission,
                    AdminName = c.CreatedBy != null ? $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}" : "Sistem"
                }),
                TotalCount = companies.Count()
            };

            return result;
        }

        /// <summary>
        /// Onay bekleyen firmaları getirir
        /// </summary>
        public async Task<PendingCompaniesDto> GetPendingCompaniesAsync(int count = 10)
        {
            var companies = await _companyRepository.GetQueryable()
                .Where(c => c.ConnectionStatus == CompanyConnectionStatus.Pending)
                .OrderByDescending(c => c.CreatedAt)
                .Take(count)
                .Include(c => c.CreatedBy)
                .ToListAsync();

            var result = new PendingCompaniesDto
            {
                Companies = companies.Select(c => new CompanyListItemDto
                {
                    Id = c.Id,
                    Name = c.Name ?? string.Empty,
                    Email = c.Email,
                    CreatedAt = c.CreatedAt,
                    Status = c.ConnectionStatus.ToString(),
                    AdminName = c.CreatedBy != null ? $"{c.CreatedBy.FirstName} {c.CreatedBy.LastName}" : "Sistem"
                }),
                TotalCount = companies.Count()
            };

            return result;
        }

        #endregion
    }
} 