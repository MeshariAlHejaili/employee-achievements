using System;
using System.Collections.Generic;

namespace EmployeeAchievementss.DbSnapshot;

public partial class Comment
{
    public int Id { get; set; }

    public string Content { get; set; } = null!;

    public DateTime Date { get; set; }

    public int UserId { get; set; }

    public int AchievementId { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual Achievement Achievement { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
