using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EndizoomBasvuru.Entity
{
    public class CompanyImage
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string FilePath { get; set; } = null!;
        
        [StringLength(100)]
        public string Description { get; set; } = null!;
        
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; } = null!;
    }
} 