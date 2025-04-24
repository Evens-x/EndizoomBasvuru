using EndizoomBasvuru.Entity;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EndizoomBasvuru.Services.Models
{
    // Company Register DTO
    public class CompanyRegisterDto
    {
        [StringLength(100, ErrorMessage = "Firma adı 100 karakterden fazla olamaz.")]
        public string? CompanyName { get; set; }
        
        [StringLength(200, ErrorMessage = "Şirket ünvanı 200 karakterden fazla olamaz.")]
        public string? CompanyTitle { get; set; }
        
        [Required(ErrorMessage = "Şirket emaili zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [StringLength(100, ErrorMessage = "Şirket emaili 100 karakterden fazla olamaz.")]
        public string CompanyEmail { get; set; } = null!;
        
        [StringLength(100, ErrorMessage = "Bölge 100 karakterden fazla olamaz.")]
        public string? Region { get; set; }
        
        [StringLength(20, ErrorMessage = "Vergi numarası 20 karakterden fazla olamaz.")]
        public string? TaxNumber { get; set; }
        
        [StringLength(50, ErrorMessage = "Üretim kapasitesi 50 karakterden fazla olamaz.")]
        public string? ProductionCapacity { get; set; }
        
        [StringLength(50, ErrorMessage = "Paket türü 50 karakterden fazla olamaz.")]
        public string? PackageType { get; set; }
        
        [StringLength(100, ErrorMessage = "İletişim adı 100 karakterden fazla olamaz.")]
        public string? ContactFirstName { get; set; }
        
        [StringLength(100, ErrorMessage = "İletişim soyadı 100 karakterden fazla olamaz.")]
        public string? ContactLastName { get; set; }
        
        [StringLength(100, ErrorMessage = "İletişim pozisyonu 100 karakterden fazla olamaz.")]
        public string? ContactPosition { get; set; }
        
        [StringLength(100, ErrorMessage = "İletişim emaili 100 karakterden fazla olamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string? ContactEmail { get; set; }
        
        [StringLength(20, ErrorMessage = "İletişim telefonu 20 karakterden fazla olamaz.")]
        public string? ContactPhone { get; set; }
        
        [StringLength(100, ErrorMessage = "IT sorumlusu adı 100 karakterden fazla olamaz.")]
        public string? ItResponsibleName { get; set; }
        
        [StringLength(20, ErrorMessage = "IT sorumlusu telefonu 20 karakterden fazla olamaz.")]
        public string? ItResponsiblePhone { get; set; }
        
        [StringLength(100, ErrorMessage = "IT sorumlusu emaili 100 karakterden fazla olamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string? ItResponsibleEmail { get; set; }
        
        public string? ConnectionStatus { get; set; } = "Beklemede";
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        [StringLength(200, ErrorMessage = "Şifre 200 karakterden fazla olamaz.")]
        public string? Password { get; set; }
        
        public int? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        
        // Contract & Images
        /// <summary>
        /// Şirket sözleşme dosyası (PDF, Word, vb.)
        /// </summary>
        public IFormFile? ContractPdf { get; set; }
        
        /// <summary>
        /// Şirket görselleri (JPG, PNG, vb.)
        /// </summary>
        public IFormFile[]? Images { get; set; }

        
        public string IsTemplate { get; set; } = string.Empty;
    }

    // Company Response DTO
    public class CompanyResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string CompanyTitle { get; set; } = null!;
        public string TaxNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        
        // Authorized Person
        public string ContactFullName { get; set; } = null!;
        public string ContactPosition { get; set; } = null!;
        public string ContactPhone { get; set; } = null!;
        public string ContactEmail { get; set; } = null!;
        
        // IT Responsible
        public string ItResponsibleName { get; set; } = null!;
        public string ItResponsiblePhone { get; set; } = null!;
        public string ItResponsibleEmail { get; set; } = null!;
        
        // Company Information
        public string ProductionCapacity { get; set; } = null!;
        public string Region { get; set; } = null!;
        public string PackageType { get; set; } = null!;
        public string ConnectionStatus { get; set; } = null!;
        public string Notes { get; set; } = null!;
        public string Status { get; set; } = null!;
        
        // Contract & Images
        public string ContractPdfPath { get; set; } = null!;
        public bool IsTemplate { get; set; } = false;
        public List<CompanyImageDto> Images { get; set; } = new List<CompanyImageDto>();
        
        // System Fields
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string CreatedByName { get; set; } = null!;
    }

    // Login DTO
    public class LoginDto
    {
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Şifre gereklidir.")]
        public string Password { get; set; } = null!;
    }

    // Company Login Response DTO
    public class CompanyLoginResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Token { get; set; } = null!;
    }

    // Change Password DTO
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mevcut şifre gereklidir.")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Şifre onayı gereklidir.")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = null!;
    }

    // Update Profile DTO
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Firma adı gereklidir.")]
        [StringLength(100, ErrorMessage = "Firma adı 100 karakterden fazla olamaz.")]
        public string CompanyName { get; set; } = null!;
        
        [Required(ErrorMessage = "Firma ünvanı gereklidir.")]
        [StringLength(200, ErrorMessage = "Firma ünvanı 200 karakterden fazla olamaz.")]
        public string CompanyTitle { get; set; } = null!;
        
        // Authorized Person
        [Required(ErrorMessage = "Yetkili kişi adı gereklidir.")]
        [StringLength(100, ErrorMessage = "İsim 100 karakterden fazla olamaz.")]
        public string ContactFirstName { get; set; } = null!;

        [Required(ErrorMessage = "Yetkili kişi soyadı gereklidir.")]
        [StringLength(100, ErrorMessage = "Soyisim 100 karakterden fazla olamaz.")]
        public string ContactLastName { get; set; } = null!;
        
        [Required(ErrorMessage = "Pozisyon gereklidir.")]
        [StringLength(100, ErrorMessage = "Pozisyon 100 karakterden fazla olamaz.")]
        public string ContactPosition { get; set; } = null!;
        
        [Required(ErrorMessage = "Telefon numarası gereklidir.")]
        [StringLength(20, ErrorMessage = "Telefon numarası 20 karakterden fazla olamaz.")]
        public string ContactPhone { get; set; } = null!;
        
        // IT Responsible
        [Required(ErrorMessage = "BT sorumlusu adı gereklidir.")]
        [StringLength(100, ErrorMessage = "İsim 100 karakterden fazla olamaz.")]
        public string ItResponsibleName { get; set; } = null!;
        
        [Required(ErrorMessage = "BT sorumlusu telefonu gereklidir.")]
        [StringLength(20, ErrorMessage = "Telefon numarası 20 karakterden fazla olamaz.")]
        public string ItResponsiblePhone { get; set; } = null!;
        
        [Required(ErrorMessage = "BT sorumlusu e-postası gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "E-posta 100 karakterden fazla olamaz.")]
        public string ItResponsibleEmail { get; set; } = null!;
        
        // Company Information
        [Required(ErrorMessage = "Üretim kapasitesi gereklidir.")]
        [StringLength(50, ErrorMessage = "Üretim kapasitesi 50 karakterden fazla olamaz.")]
        public string ProductionCapacity { get; set; } = null!;
        
        [Required(ErrorMessage = "Bölge gereklidir.")]
        [StringLength(100, ErrorMessage = "Bölge 100 karakterden fazla olamaz.")]
        public string Region { get; set; } = null!;
        
        [Required(ErrorMessage = "Paket tipi gereklidir.")]
        [StringLength(50, ErrorMessage = "Paket tipi 50 karakterden fazla olamaz.")]
        public string PackageType { get; set; } = null!;
    }
    
    // Company Filter DTO
    public class CompanyFilterDto
    {
        public string? Name { get; set; }
        public string? Region { get; set; }
        public string? ConnectionStatus { get; set; }
        public string? Status { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? TaxNumber { get; set; }
        public string? ContactPerson { get; set; }
        public string? CompanyTitle { get; set; }
        public string? Email { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? PackageType { get; set; }
        public decimal? MinRevenue { get; set; }
        public decimal? MaxRevenue { get; set; }
        public bool? HasContract { get; set; }
        public bool? HasImages { get; set; }
        public DateTime? UpdatedAfter { get; set; }
    }
    
    // Company Status Update DTO
    public class CompanyStatusUpdateDto
    {
        [Required(ErrorMessage = "Durum gereklidir.")]
        public string Status { get; set; } = null!;
        
        [StringLength(1000, ErrorMessage = "Notlar 1000 karakterden fazla olamaz.")]
        public string? Notes { get; set; }
    }
    
    // Company Image DTO
    public class CompanyImageDto
    {
        public int Id { get; set; }
        public string FilePath { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime UploadDate { get; set; }
    }

    // Reset Password DTO (Kimlik doğrulama gerektirmeyen şifre değiştirme için)
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mevcut şifre gereklidir.")]
        public string CurrentPassword { get; set; } = null!;

        [Required(ErrorMessage = "Yeni şifre gereklidir.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string NewPassword { get; set; } = null!;

        [Required(ErrorMessage = "Şifre onayı gereklidir.")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; } = null!;
    }

    /// <summary>
    /// İstatistik Dönem Tipi
    /// </summary>
    public enum StatisticPeriodType
    {
        Daily,
        Monthly
    }

    /// <summary>
    /// Firma sayısı istatistikleri DTO
    /// </summary>
    public class CompanyCountStatisticsDto
    {
        public int TotalCount { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public IEnumerable<AdminCompanyCountDto> DetailsByAdmin { get; set; } = new List<AdminCompanyCountDto>();
    }

    /// <summary>
    /// Yönetici bazında firma sayıları
    /// </summary>
    public class AdminCompanyCountDto
    {
        public int AdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string AdminRole { get; set; } = string.Empty;
        public int CompanyCount { get; set; }
    }

    /// <summary>
    /// Finansal istatistik DTO
    /// </summary>
    public class FinancialStatisticsDto
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalCommission { get; set; }
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public IEnumerable<AdminFinancialStatisticsDto> DetailsByAdmin { get; set; } = new List<AdminFinancialStatisticsDto>();
    }

    /// <summary>
    /// Yönetici bazında finansal istatistikler
    /// </summary>
    public class AdminFinancialStatisticsDto
    {
        public int AdminId { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string AdminRole { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
    }

    /// <summary>
    /// En son eklenen firmalar DTO
    /// </summary>
    public class RecentCompaniesDto
    {
        public IEnumerable<CompanyListItemDto> Companies { get; set; } = new List<CompanyListItemDto>();
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Onay bekleyen firmalar DTO
    /// </summary>
    public class PendingCompaniesDto
    {
        public IEnumerable<CompanyListItemDto> Companies { get; set; } = new List<CompanyListItemDto>();
        public int TotalCount { get; set; }
    }

    /// <summary>
    /// Firma Liste Öğesi DTO
    /// </summary>
    public class CompanyListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Commission { get; set; }
        public string AdminName { get; set; } = string.Empty;
    }

    // Company Update DTO
    public class CompanyUpdateDto
    {
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
        
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        [StringLength(100, ErrorMessage = "Şirket emaili 100 karakterden fazla olamaz.")]
        public string? Email { get; set; }
        
        [StringLength(50, ErrorMessage = "Üretim kapasitesi 50 karakterden fazla olamaz.")]
        public string? ProductionCapacity { get; set; }
        
        [StringLength(100, ErrorMessage = "Bölge 100 karakterden fazla olamaz.")]
        public string? Region { get; set; }
        
        [StringLength(50, ErrorMessage = "Paket türü 50 karakterden fazla olamaz.")]
        public string? PackageType { get; set; }
        
        [StringLength(1000)]
        public string? Notes { get; set; }
        
        public decimal? Revenue { get; set; } // Net Ciro
        
        public decimal? Commission { get; set; } // Komisyon
        
        public decimal? CommissionRate { get; set; } // Komisyon Oranı (%)
    }
} 