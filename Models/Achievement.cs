using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeAchievementss.Models
{
    public class Achievement
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;
        
        public DateTime Date { get; set; }
        
        [Required]
        public int OwnerId { get; set; }
        
        [ForeignKey("OwnerId")]
        public virtual User? Owner { get; set; }

        // [Required]
        // public int DepartmentId { get; set; }
        // [ForeignKey("DepartmentId")]
        // public virtual Department? Department { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending";
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
        
        // Computed properties (not stored in DB)
        [NotMapped]
        public int LikesCount => Likes?.Count ?? 0;
        
        [NotMapped]
        public int CommentsCount => Comments?.Count ?? 0;
        
        [NotMapped]
        public bool IsLikedByCurrentUser { get; set; }
    }

    public class Comment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;
        
        public DateTime Date { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [Required]
        public int AchievementId { get; set; }
        
        [ForeignKey("AchievementId")]
        public virtual Achievement? Achievement { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class Like
    {
        [Key]
        public int Id { get; set; }
        
        public DateTime Date { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }
        
        [Required]
        public int AchievementId { get; set; }
        
        [ForeignKey("AchievementId")]
        public virtual Achievement? Achievement { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
} 