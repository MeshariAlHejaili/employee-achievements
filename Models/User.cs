using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeAchievementss.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? ProfilePicture { get; set; }
        
        // [StringLength(100)]
        // public string? Department { get; set; }
        
        [StringLength(100)]
        public string? Position { get; set; }
        
        [Required]
        public int DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public virtual Department? DepartmentRef { get; set; }

        public int? ManagerId { get; set; }
        [ForeignKey("ManagerId")]
        public virtual Manager? Manager { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Navigation properties
        public virtual ICollection<Achievement>? Achievements { get; set; } = new List<Achievement>();
        public virtual ICollection<Like>? Likes { get; set; } = new List<Like>();
        public virtual ICollection<Comment>? Comments { get; set; } = new List<Comment>();
    }
} 