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
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [Authorize(Roles = nameof(UserRole.Company))]
        [HttpGet("company")]
        public async Task<IActionResult> GetCompanyDocuments()
        {
            int companyId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (companyId == 0)
                return Unauthorized();

            var documents = await _documentService.GetCompanyDocumentsAsync(companyId);
            return Ok(documents);
        }

        [Authorize(Roles = nameof(UserRole.Company))]
        [HttpGet("company/{id}")]
        public async Task<IActionResult> GetCompanyDocumentById(int id)
        {
            int companyId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (companyId == 0)
                return Unauthorized();

            var document = await _documentService.GetDocumentByIdAsync(id, companyId);
            if (document == null)
                return NotFound(new { message = "Belge bulunamadı." });

            return Ok(document);
        }

        [HttpGet("types")]
        public IActionResult GetDocumentTypes()
        {
            var documentTypes = Enum.GetValues(typeof(DocumentType))
                .Cast<DocumentType>()
                .Select(t => new { Id = (int)t, Name = t.ToString() })
                .ToList();
                
            return Ok(documentTypes);
        }

        [Authorize(Roles = nameof(UserRole.Company))]
        [HttpPost]
        public async Task<IActionResult> UploadDocument([FromForm] DocumentUploadDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int companyId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (companyId == 0)
                return Unauthorized();

            try
            {
                var document = await _documentService.UploadDocumentAsync(companyId, model);
                return Ok(document);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Belge yüklenirken bir hata oluştu.", error = ex.Message });
            }
        }

        [Authorize(Roles = nameof(UserRole.Company))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            int companyId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (companyId == 0)
                return Unauthorized();

            var result = await _documentService.DeleteDocumentAsync(id, companyId);
            if (!result)
                return NotFound(new { message = "Belge bulunamadı." });

            return Ok(new { message = "Belge başarıyla silindi." });
        }
        
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingDocuments()
        {
            var documents = await _documentService.GetPendingDocumentsAsync();
            return Ok(documents);
        }
        
        [Authorize(Roles = $"{nameof(UserRole.Admin)},{nameof(UserRole.Marketing)}")]
        [HttpPost("{id}/review")]
        public async Task<IActionResult> ReviewDocument(int id, [FromBody] DocumentReviewDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            int reviewedById = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (reviewedById == 0)
                return Unauthorized();

            try
            {
                var document = await _documentService.ReviewDocumentAsync(id, model, reviewedById);
                return Ok(document);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Belge incelenirken bir hata oluştu.", error = ex.Message });
            }
        }
    }
}