using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeAchievementss.Models
{
    public class AchievementPhoto
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;
        
        [Required]
        [StringLength(10)]
        public string FileExtension { get; set; } = string.Empty;
        
        [Required]
        public long FileSize { get; set; }
        
        [Required]
        [StringLength(50)]
        public string ContentType { get; set; } = string.Empty;
        
        [StringLength(255)]
        public string? ThumbnailFileName { get; set; }
        
        public int DisplayOrder { get; set; } = 0;
        
        [Required]
        public int AchievementId { get; set; }
        
        [ForeignKey("AchievementId")]
        public virtual Achievement? Achievement { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Computed properties
        [NotMapped]
        public string FilePath => $"uploads/achievements/{FileName}";
        
        [NotMapped]
        public string ThumbnailPath => !string.IsNullOrEmpty(ThumbnailFileName) 
            ? $"uploads/achievements/thumbnails/{ThumbnailFileName}" 
            : FilePath;
        
        [NotMapped]
        public string FullFileName => $"{FileName}.{FileExtension}";
        
        [NotMapped]
        public string FullThumbnailFileName => !string.IsNullOrEmpty(ThumbnailFileName) 
            ? $"{ThumbnailFileName}.{FileExtension}" 
            : FullFileName;
    }
} 