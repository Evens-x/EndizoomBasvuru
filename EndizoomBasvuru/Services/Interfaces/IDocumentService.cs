using EndizoomBasvuru.Entity;
using EndizoomBasvuru.Services.Models;

namespace EndizoomBasvuru.Services.Interfaces
{
    public interface IDocumentService
    {
        Task<DocumentResponseDto> UploadDocumentAsync(int companyId, DocumentUploadDto model);
        Task<IEnumerable<DocumentResponseDto>> GetCompanyDocumentsAsync(int companyId);
        Task<DocumentResponseDto?> GetDocumentByIdAsync(int documentId, int companyId);
        Task<bool> DeleteDocumentAsync(int documentId, int companyId);
        Task<DocumentResponseDto> ReviewDocumentAsync(int documentId, DocumentReviewDto model, int reviewedById);
        Task<IEnumerable<DocumentResponseDto>> GetPendingDocumentsAsync();
        IEnumerable<object> GetDocumentTypes();
    }
} 