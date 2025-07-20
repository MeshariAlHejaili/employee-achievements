using System.ComponentModel.DataAnnotations;

namespace EmployeeAchievementss.Models
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
    }
} 