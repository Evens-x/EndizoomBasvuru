using EndizoomBasvuru.Entity;
using EndizoomBasvuru.Repository.Interfaces;
using EndizoomBasvuru.Services.Interfaces;
using EndizoomBasvuru.Services.Models;

namespace EndizoomBasvuru.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly IGenericRepository<Document> _documentRepository;
        private readonly IGenericRepository<Company> _companyRepository;
        private readonly IGenericRepository<Admin> _adminRepository;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public DocumentService(
            IGenericRepository<Document> documentRepository,
            IGenericRepository<Company> companyRepository,
            IGenericRepository<Admin> adminRepository,
            IWebHostEnvironment hostingEnvironment)
        {
            _documentRepository = documentRepository;
            _companyRepository = companyRepository;
            _adminRepository = adminRepository;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<bool> DeleteDocumentAsync(int documentId, int companyId)
        {
            var documents = await _documentRepository.FindAsync(d => d.Id == documentId && d.CompanyId == companyId);
            var document = documents.FirstOrDefault();

            if (document == null)
                return false;

            // Delete physical file if it exists
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, document.FilePath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            await _documentRepository.DeleteAsync(document);
            await _documentRepository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<DocumentResponseDto>> GetCompanyDocumentsAsync(int companyId)
        {
            var documents = await _documentRepository.FindAsync(d => d.CompanyId == companyId);
            return documents.Select(d => new DocumentResponseDto
            {
                Id = d.Id,
                Name = d.Name,
                Type = d.Type.ToString(),
                UploadDate = d.UploadDate,
                FilePath = d.FilePath,
                Status = d.Status.ToString(),
                RejectionReason = d.RejectionReason,
                ReviewDate = d.ReviewDate
            });
        }

        public async Task<DocumentResponseDto?> GetDocumentByIdAsync(int documentId, int companyId)
        {
            var documents = await _documentRepository.FindAsync(d => d.Id == documentId && d.CompanyId == companyId);
            var document = documents.FirstOrDefault();

            if (document == null)
                return null;

            return new DocumentResponseDto
            {
                Id = document.Id,
                Name = document.Name,
                Type = document.Type.ToString(),
                UploadDate = document.UploadDate,
                FilePath = document.FilePath,
                Status = document.Status.ToString(),
                RejectionReason = document.RejectionReason,
                ReviewDate = document.ReviewDate
            };
        }

        public async Task<DocumentResponseDto> UploadDocumentAsync(int companyId, DocumentUploadDto model)
        {
            // Verify company exists
            var companies = await _companyRepository.FindAsync(c => c.Id == companyId);
            var company = companies.FirstOrDefault();

            if (company == null)
                throw new KeyNotFoundException("Firma bulunamadı.");

            // Save file to disk
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.File.FileName;
            string uploadsFolder = Path.Combine(_hostingEnvironment.ContentRootPath, "uploads", companyId.ToString());

            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.File.CopyToAsync(fileStream);
            }

            // Create document record
            var document = new Document
            {
                Name = model.Name,
                Type = model.Type,
                FilePath = Path.Combine("uploads", companyId.ToString(), uniqueFileName),
                UploadDate = DateTime.UtcNow,
                CompanyId = companyId,
                Status = DocumentStatus.Beklemede
            };

            await _documentRepository.AddAsync(document);
            await _documentRepository.SaveChangesAsync();

            return new DocumentResponseDto
            {
                Id = document.Id,
                Name = document.Name,
                Type = document.Type.ToString(),
                UploadDate = document.UploadDate,
                FilePath = document.FilePath,
                Status = document.Status.ToString()
            };
        }

        public async Task<DocumentResponseDto> ReviewDocumentAsync(int documentId, DocumentReviewDto model, int reviewedById)
        {
            // Belgeyi bul
            var documents = await _documentRepository.FindAsync(d => d.Id == documentId);
            var document = documents.FirstOrDefault();
            
            if (document == null)
                throw new KeyNotFoundException("Belge bulunamadı.");
                
            // İnceleme bilgilerini güncelle
            document.Status = model.Status;
            document.ReviewedById = reviewedById;
            document.ReviewDate = DateTime.UtcNow;
            
            // Eğer reddedilmişse, sebep ekle
            if (model.Status == DocumentStatus.Reddedildi)
            {
                if (string.IsNullOrEmpty(model.RejectionReason))
                    throw new ArgumentException("Belge reddedildiğinde bir red sebebi belirtilmelidir.");
                    
                document.RejectionReason = model.RejectionReason;
            }
            else
            {
                document.RejectionReason = null;
            }
            
            // Belgeyi güncelle
            await _documentRepository.UpdateAsync(document);
            await _documentRepository.SaveChangesAsync();
            
            // İnceleyen admin bilgilerini bul
            string reviewedByName = "";
            if (reviewedById > 0)
            {
                var admin = await _adminRepository.GetByIdAsync(reviewedById);
                if (admin != null)
                {
                    reviewedByName = $"{admin.FirstName} {admin.LastName}";
                }
            }
            
            // Yanıt DTO'sunu oluştur ve döndür
            return new DocumentResponseDto
            {
                Id = document.Id,
                Name = document.Name,
                Type = document.Type.ToString(),
                UploadDate = document.UploadDate,
                FilePath = document.FilePath,
                Status = document.Status.ToString(),
                RejectionReason = document.RejectionReason,
                ReviewDate = document.ReviewDate,
                ReviewedBy = reviewedByName
            };
        }
        
        public async Task<IEnumerable<DocumentResponseDto>> GetPendingDocumentsAsync()
        {
            // Bekleyen tüm belgeleri bul
            var documents = await _documentRepository.FindAsync(d => d.Status == DocumentStatus.Beklemede);
            
            // Her belge için şirket bilgisini içerecek şekilde yanıtı oluştur
            var documentsWithCompanies = new List<DocumentResponseDto>();
            
            foreach (var document in documents)
            {
                var company = await _companyRepository.GetByIdAsync(document.CompanyId);
                
                var dto = new DocumentResponseDto
                {
                    Id = document.Id,
                    Name = document.Name,
                    Type = document.Type.ToString(),
                    UploadDate = document.UploadDate,
                    FilePath = document.FilePath,
                    Status = document.Status.ToString(),
                    RejectionReason = document.RejectionReason,
                    ReviewDate = document.ReviewDate
                };
                
                // Şirket adını açıklamaya ekle
                if (company != null)
                {
                    dto.Name = $"{dto.Name} ({company.Name})";
                }
                
                documentsWithCompanies.Add(dto);
            }
            
            return documentsWithCompanies;
        }

        public IEnumerable<object> GetDocumentTypes()
        {
            return Enum.GetValues(typeof(DocumentType))
                .Cast<DocumentType>()
                .Select(t => new { Id = (int)t, Name = t.ToString() })
                .ToList();
        }
    }
} 