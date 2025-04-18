using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EndizoomBasvuru.Entity
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [StringLength(200)]
        public string FilePath { get; set; } = null!;

        [Required]
        public DocumentType Type { get; set; }

        [Required]
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        
        // Belge durumu
        [Required]
        public DocumentStatus Status { get; set; } = DocumentStatus.Beklemede;
        
        // Onaylayan/Reddeden kullanıcı
        public int? ReviewedById { get; set; }
        
        // İnceleme tarihi
        public DateTime? ReviewDate { get; set; }
        
        // Red edilme sebebi
        [StringLength(500)]
        public string? RejectionReason { get; set; }

        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; } = null!;
    }

    public enum DocumentType
    {
        KimlikBelgesi = 1,
        VergiLevhasi = 2,
        ImzaSirkuleri = 3,
        FaaliyetBelgesi = 4,
        TicariSicilGazetesi = 5
    }
    
    public enum DocumentStatus
    {
        Beklemede = 1,
        Onaylandı = 2,
        Reddedildi = 3
    }
} 