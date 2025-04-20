using EndizoomBasvuru.Entity;
using System.ComponentModel.DataAnnotations;

namespace EndizoomBasvuru.Services.Models
{
    public class AdminLoginResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Token { get; set; } = null!;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; }
    }
    
    public class AdminRegisterDto
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı 50 karakterden fazla olamaz.")]
        public string Username { get; set; } = null!;
        
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "E-posta 100 karakterden fazla olamaz.")]
        public string Email { get; set; } = null!;
        
        [Required(ErrorMessage = "Ad gereklidir.")]
        [StringLength(50, ErrorMessage = "Ad 50 karakterden fazla olamaz.")]
        public string FirstName { get; set; } = null!;
        
        [Required(ErrorMessage = "Soyad gereklidir.")]
        [StringLength(50, ErrorMessage = "Soyad 50 karakterden fazla olamaz.")]
        public string LastName { get; set; } = null!;
        
        [StringLength(20, ErrorMessage = "Telefon numarası 20 karakterden fazla olamaz.")]
        public string? PhoneNumber { get; set; }
        
        [StringLength(20, ErrorMessage = "Şirket numarası 20 karakterden fazla olamaz.")]
        public string? CompanyNumber { get; set; }
        
        [Required(ErrorMessage = "Rol gereklidir.")]
        public UserRole Role { get; set; }
        
        [Required(ErrorMessage = "Şifre gereklidir.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; } = null!;
        
        [StringLength(1000, ErrorMessage = "Zimmetler 1000 karakterden fazla olamaz.")]
        public string? Assignments { get; set; }
    }
    
    public class AdminUpdateDto
    {
        [Required(ErrorMessage = "Kullanıcı adı gereklidir.")]
        [StringLength(50, ErrorMessage = "Kullanıcı adı 50 karakterden fazla olamaz.")]
        public string Username { get; set; } = null!;
        
        [Required(ErrorMessage = "E-posta gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100, ErrorMessage = "E-posta 100 karakterden fazla olamaz.")]
        public string Email { get; set; } = null!;
        
        [Required(ErrorMessage = "Ad gereklidir.")]
        [StringLength(50, ErrorMessage = "Ad 50 karakterden fazla olamaz.")]
        public string FirstName { get; set; } = null!;
        
        [Required(ErrorMessage = "Soyad gereklidir.")]
        [StringLength(50, ErrorMessage = "Soyad 50 karakterden fazla olamaz.")]
        public string LastName { get; set; } = null!;
        
        [StringLength(20, ErrorMessage = "Telefon numarası 20 karakterden fazla olamaz.")]
        public string? PhoneNumber { get; set; }
        
        [StringLength(20, ErrorMessage = "Şirket numarası 20 karakterden fazla olamaz.")]
        public string? CompanyNumber { get; set; }
        
        [Required(ErrorMessage = "Rol gereklidir.")]
        public UserRole Role { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
    
    public class AdminResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string? CompanyNumber { get; set; }
        public UserRole Role { get; set; }
        public string RoleName => Role.ToString();
        public bool IsAdmin => Role == UserRole.Admin;
        public bool IsMarketing => Role == UserRole.Marketing;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
    
    // Admin durum değiştirme DTO
    public class AdminStatusUpdateDto
    {
        [Required(ErrorMessage = "Aktiflik durumu gereklidir.")]
        public bool IsActive { get; set; }
    }

    // Pazarlamacı istatistikleri
    public class MarketingUserStatsDto
    {
        public int AdminId { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public bool IsActive { get; set; }
        
        // Toplam istatistikler
        public int TotalCompanies { get; set; }
        public int TotalActiveCompanies { get; set; }
        public int TotalRejectedCompanies { get; set; }
        public int TotalPendingCompanies { get; set; }
        
        // Günlük istatistikler
        public int DailyCompanies { get; set; }
        public int DailyActiveCompanies { get; set; }
        public int DailyRejectedCompanies { get; set; }
        
        // Aylık istatistikler
        public int MonthlyCompanies { get; set; }
        public int MonthlyActiveCompanies { get; set; }
        public int MonthlyRejectedCompanies { get; set; }
    }
    
    // İstatistik periyodu için DTO
    public class StatPeriodDto
    {
        public DateTime? Date { get; set; }
    }
} 