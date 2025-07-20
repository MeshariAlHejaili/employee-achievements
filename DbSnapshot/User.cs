using System;
using System.Collections.Generic;

namespace EmployeeAchievementss.DbSnapshot;

public partial class User
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ProfilePicture { get; set; }

    public string? Position { get; set; }

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public int DepartmentId { get; set; }

    public int? ManagerId { get; set; }

    public virtual ICollection<Achievement> Achievements { get; set; } = new List<Achievement>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual Manager? Manager { get; set; }

    public virtual Manager? ManagerNavigation { get; set; }
}
