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
            var companies = await _companyRepository.GetAllAsync();
            return companies.Select(c => new CompanyResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                ContactFullName = $"{c.ContactFirstName} {c.ContactLastName}",
                Email = c.Email,
                CreatedAt = c.CreatedAt
            });
        }

        public async Task<CompanyResponseDto?> GetCompanyByIdAsync(int id)
        {
            var company = await _companyRepository.GetByIdAsync(id);
            if (company == null)
                return null;

            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                ContactFullName = $"{company.ContactFirstName} {company.ContactLastName}",
                Email = company.Email,
                CreatedAt = company.CreatedAt
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
                CreatedByName = company.CreatedById.HasValue ? "Admin" : null
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

            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();

            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                CompanyTitle = company.Title,
                ContactFullName = $"{company.ContactFirstName} {company.ContactLastName}",
                ContactPosition = company.ContactPosition,
                ContactPhone = company.ContactPhone,
                ItResponsibleName = company.ItResponsibleName,
                ItResponsiblePhone = company.ItResponsiblePhone,
                ItResponsibleEmail = company.ItResponsibleEmail,
                ProductionCapacity = company.ProductionCapacity,
                Region = company.Region,
                PackageType = company.PackageType,
                Email = company.Email,
                CreatedAt = company.CreatedAt
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
            
            // Güncelleme için veritabanını güncelle
            await _companyRepository.UpdateAsync(company);
            await _companyRepository.SaveChangesAsync();
            
            // DTO olarak güncel şirket bilgilerini döndür
            return new CompanyResponseDto
            {
                Id = company.Id,
                Name = company.Name,
                CompanyTitle = company.Title,
                TaxNumber = company.TaxNumber,
                Email = company.Email,
                Status = company.ConnectionStatus.ToString(),
                CreatedAt = company.CreatedAt
            };
        }

        public async Task<IEnumerable<CompanyResponseDto>> FilterCompaniesAsync(CompanyFilterDto filter)
        {
            // Tüm şirketleri al
            var allCompanies = await _companyRepository.GetAllAsync();
            
            // Filtrelemeyi yap
            var query = allCompanies.AsQueryable();
            
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
            
            // Sonuçları dönüştür
            return query.Select(c => new CompanyResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                CompanyTitle = c.Title,
                TaxNumber = c.TaxNumber,
                Email = c.Email,
                ContactFullName = $"{c.ContactFirstName} {c.ContactLastName}",
                Status = c.ConnectionStatus.ToString(),
                ConnectionStatus = c.ConnectionStatus.ToString(),
                Region = c.Region,
                CreatedAt = c.CreatedAt,
                CreatedByName = c.CreatedById.HasValue ? "Admin" : null
            });
        }
    }
} 