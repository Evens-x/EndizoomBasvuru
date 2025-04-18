using EndizoomBasvuru.Entity;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EndizoomBasvuru.Services.Models
{
    // Document Upload DTO
    public class DocumentUploadDto
    {
        [Required(ErrorMessage = "Belge adı gereklidir.")]
        [StringLength(100, ErrorMessage = "Belge adı 100 karakterden fazla olamaz.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Belge türü gereklidir.")]
        public DocumentType Type { get; set; }

        [Required(ErrorMessage = "Dosya gereklidir.")]
        public IFormFile File { get; set; } = null!;
    }

    // Document Response DTO
    public class DocumentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public DateTime UploadDate { get; set; }
        public string FilePath { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? RejectionReason { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string? ReviewedBy { get; set; }
    }
    
    // Document Review DTO
    public class DocumentReviewDto
    {
        [Required(ErrorMessage = "Belge durumu gereklidir.")]
        public DocumentStatus Status { get; set; }
        
        [StringLength(500, ErrorMessage = "Red sebebi 500 karakterden fazla olamaz.")]
        public string? RejectionReason { get; set; }
    }
} 