using System.Diagnostics;
using EmployeeAchievementss.Models;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeAchievementss.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Details(int id)
        {
            // Simulate achievements list
            var users = new List<User> {
                new User { Id = 1, Name = "مشاري الحربي" },
                new User { Id = 2, Name = "سارة أحمد" }
            };
            var achievements = new List<Achievement> {
                new Achievement {
                    Id = 1,
                    Title = "مشروع نيوسواك",
                    Description = "إعادة تصميم كاملة لتدفق تأهيل المستخدمين لمشروع نيوسواك...",
                    Date = new DateTime(2024, 6, 25),
                    Owner = users[0]
                },
                new Achievement {
                    Id = 2,
                    Title = "تطوير نظام التقارير",
                    Description = "تطوير نظام تقارير جديد للإدارة...",
                    Date = new DateTime(2024, 6, 20),
                    Owner = users[1]
                }
            };
            var achievement = achievements.FirstOrDefault(a => a.Id == id);
            if (achievement == null)
                return NotFound();
            return View(achievement);
        }

        public IActionResult AddAchievement()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAchievement(Achievement achievement)
        {
            // Simulate saving the achievement (in-memory or to DB)
            // You can add logic to save to a real database here
            // For now, just redirect to Index
            return RedirectToAction("Index");
        }
    }
}
