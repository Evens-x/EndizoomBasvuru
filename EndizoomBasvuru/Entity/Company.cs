using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EndizoomBasvuru.Services.Models;

namespace EndizoomBasvuru.Entity
{
    public class Company
    {
        [Key]
        public int Id { get; set; }

        [StringLength(100, ErrorMessage = "Firma adı 100 karakterden fazla olamaz.")]
        public string? Name { get; set; }

        [StringLength(100, ErrorMessage = "İletişim adı 100 karakterden fazla olamaz.")]
        public string? ContactFirstName { get; set; }

        [StringLength(100, ErrorMessage = "İletişim soyadı 100 karakterden fazla olamaz.")]
        public string? ContactLastName { get; set; }

        [StringLength(100, ErrorMessage = "İletişim pozisyonu 100 karakterden fazla olamaz.")]
        public string? ContactPosition { get; set; }

        [StringLength(20, ErrorMessage = "İletişim telefonu 20 karakterden fazla olamaz.")]
        public string? ContactPhone { get; set; }

        [StringLength(100, ErrorMessage = "İletişim emaili 100 karakterden fazla olamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string? ContactEmail { get; set; }

        [StringLength(100, ErrorMessage = "IT sorumlusu adı 100 karakterden fazla olamaz.")]
        public string? ItResponsibleName { get; set; }

        [StringLength(20, ErrorMessage = "IT sorumlusu telefonu 20 karakterden fazla olamaz.")]
        public string? ItResponsiblePhone { get; set; }

        [StringLength(100, ErrorMessage = "IT sorumlusu emaili 100 karakterden fazla olamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string? ItResponsibleEmail { get; set; }

        [StringLength(200, ErrorMessage = "Şirket ünvanı 200 karakterden fazla olamaz.")]
        public string? Title { get; set; }

        [StringLength(20, ErrorMessage = "Vergi numarası 20 karakterden fazla olamaz.")]
        public string? TaxNumber { get; set; }

        [Required(ErrorMessage = "Şirket emaili zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [StringLength(100, ErrorMessage = "Şirket emaili 100 karakterden fazla olamaz.")]
        public string Email { get; set; } = null!;

        [StringLength(50, ErrorMessage = "Üretim kapasitesi 50 karakterden fazla olamaz.")]
        public string? ProductionCapacity { get; set; }

        [StringLength(100, ErrorMessage = "Bölge 100 karakterden fazla olamaz.")]
        public string? Region { get; set; }

        [StringLength(50, ErrorMessage = "Paket türü 50 karakterden fazla olamaz.")]
        public string? PackageType { get; set; }

        [StringLength(100, ErrorMessage = "Şifre 100 karakterden fazla olamaz.")]
        public string? Password { get; set; }

        public CompanyConnectionStatus ConnectionStatus { get; set; } = CompanyConnectionStatus.Pending;

        [StringLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public int? CreatedById { get; set; }
        public Admin? CreatedBy { get; set; }
        
        public int? UpdatedById { get; set; }
        public Admin? UpdatedBy { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Revenue { get; set; } = 0; // Net Ciro

        [Column(TypeName = "decimal(18,2)")]
        public decimal Commission { get; set; } = 0; // Komisyon

        [Column(TypeName = "decimal(18,2)")]
        public decimal CommissionRate { get; set; } = 0; // Komisyon Oranı (%)

        public string? ContractPath { get; set; }
        public string? VisualPath { get; set; }

        public List<Document> Documents { get; set; } = new();

        // Company Images
        public virtual ICollection<CompanyImage> Images { get; set; } = new List<CompanyImage>();
    }

    public enum CompanyConnectionStatus
    {
        Active = 0,
        Pending = 1,
        Rejected = 2
    }
} 