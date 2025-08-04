using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeAchievementss.Models;
using System.Threading.Tasks;

namespace EmployeeAchievementss.Controllers
{
    public abstract class BaseController : Controller
    {
        protected int? CurrentUserId => HttpContext.Session.GetInt32("UserId");
        protected string CurrentUserName => HttpContext.Session.GetString("UserName");
        protected bool IsAuthenticated => CurrentUserId.HasValue;

        protected async Task<User?> GetCurrentUserAsync(ApplicationDbContext context)
        {
            if (!CurrentUserId.HasValue) return null;
            return await context.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId.Value);
        }

        protected async Task<Manager?> GetCurrentManagerAsync(ApplicationDbContext context)
        {
            if (!CurrentUserId.HasValue) return null;
            return await context.Managers.FirstOrDefaultAsync(m => m.UserId == CurrentUserId.Value);
        }
    }
} 