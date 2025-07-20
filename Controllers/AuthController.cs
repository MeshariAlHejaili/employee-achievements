using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EmployeeAchievementss.Models;

namespace EmployeeAchievementss.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Find user in database
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Password == password);
            
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.Name);
                // Check if user is a manager
                var isManager = _context.Managers.Any(m => m.UserId == user.Id);
                HttpContext.Session.SetString("IsManager", isManager ? "true" : "false");
                return RedirectToAction("Index", "Home");
            }
            
            // Log the failed login attempt for debugging
            Console.WriteLine($"Login failed for email: {email}");
            ViewBag.Error = "بيانات الدخول غير صحيحة";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Debug action to check database
        public IActionResult Debug()
        {
            try
            {
                var users = _context.Users.ToList();
                return Json(new { 
                    success = true, 
                    userCount = users.Count,
                    users = users.Select(u => new { u.Id, u.Name, u.Email, u.Password })
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
