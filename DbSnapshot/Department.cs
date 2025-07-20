using System;
using System.Collections.Generic;

namespace EmployeeAchievementss.DbSnapshot;

public partial class Department
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Manager> Managers { get; set; } = new List<Manager>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
