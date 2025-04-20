using System.ComponentModel.DataAnnotations;

namespace EndizoomBasvuru.Entity
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = null!;

        [Required]
        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = null!;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(20)]
        public string? CompanyNumber { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Admin;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        
        public int? CreatedBy { get; set; }
        public bool IsActive { get; set; } = true;
        
        [StringLength(1000)]
        public string? Assignments { get; set; }
    }
} 