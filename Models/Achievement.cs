using System;

namespace EmployeeAchievementss.Models
{
    public class Achievement
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public User? Owner { get; set; }
    }
} 