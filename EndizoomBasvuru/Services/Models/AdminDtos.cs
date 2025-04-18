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
        
        [Required(ErrorMessage = "Rol gereklidir.")]
        public UserRole Role { get; set; }
        
        [Required(ErrorMessage = "Şifre gereklidir.")]
        [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        public string Password { get; set; } = null!;
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
        
        [Required(ErrorMessage = "Rol gereklidir.")]
        public UserRole Role { get; set; }
    }
    
    public class AdminResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 