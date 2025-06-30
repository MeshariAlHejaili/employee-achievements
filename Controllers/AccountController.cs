using Microsoft.AspNetCore.Mvc;
using EmployeeAchievementss.Models;

namespace EmployeeAchievementss.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Info()
        {
            // Simulate getting user from session
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName") ?? "";
            var userEmail = "****@****.com";
            if (userId == null)
                return RedirectToAction("Login", "Auth");

            // Simulate achievements for this user
            var achievements = new List<Achievement> {
                new Achievement {
                    Id = 1,
                    Title = "مشروع نيوسواك",
                    Description = "إعادة تصميم كاملة لتدفق تأهيل المستخدمين لمشروع نيوسواك...",
                    Date = new DateTime(2024, 6, 25),
                    Owner = new User { Id = userId.Value, Name = userName }
                }
            };

            ViewBag.UserName = userName;
            ViewBag.UserEmail = userEmail;
            ViewBag.Achievements = achievements;
            return View();
        }
    }
} 