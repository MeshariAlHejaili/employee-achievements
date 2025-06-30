using Microsoft.AspNetCore.Mvc;
using EmployeeAchievementss.Models; // if you create a LoginViewModel

namespace EmployeeAchievementss.Controllers
{
    public class AuthController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            
            if ((email == "admin@company.com" && password == "1234") || (email=="admin@a" && password=="1234"))
            {
                HttpContext.Session.SetInt32("UserId", 1);
                HttpContext.Session.SetString("UserName", "تشاري براون");
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid login credentials";
            return View();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
