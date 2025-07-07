using System.Diagnostics;
using EmployeeAchievementss.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAchievementss.Controllers
{
    public class ToggleLikeRequest
    {
        public int AchievementId { get; set; }
    }

    public class AddCommentRequest
    {
        public int AchievementId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get current user ID from session
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            
            // Get achievements with related data
            var achievements = await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Comments)
                    .ThenInclude(c => c.User)
                .Include(a => a.Likes)
                    .ThenInclude(l => l.User)
                .OrderByDescending(a => a.Date)
                .ToListAsync();

            // Set IsLikedByCurrentUser for each achievement
            if (currentUserId.HasValue)
            {
                foreach (var achievement in achievements)
                {
                    achievement.IsLikedByCurrentUser = achievement.Likes.Any(l => l.UserId == currentUserId.Value);
                }
            }

            ViewBag.CurrentUserId = currentUserId;
            return View(achievements);
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

        public async Task<IActionResult> Details(int id)
        {
            var achievement = await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Comments)
                    .ThenInclude(c => c.User)
                .Include(a => a.Likes)
                    .ThenInclude(l => l.User)
                .FirstOrDefaultAsync(a => a.Id == id);

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
        public async Task<IActionResult> AddAchievement(Achievement achievement)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (!currentUserId.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }

                achievement.OwnerId = currentUserId.Value;
                achievement.Date = DateTime.Now;
                achievement.CreatedAt = DateTime.Now;

                _context.Achievements.Add(achievement);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            return View(achievement);
        }

        // Edit Achievement
        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var achievement = await _context.Achievements
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (achievement == null)
                return NotFound();

            // Check if current user owns this achievement
            if (achievement.OwnerId != currentUserId.Value)
            {
                return Forbid();
            }

            return View(achievement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Achievement achievement)
        {
            if (id != achievement.Id)
            {
                return NotFound();
            }

            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verify ownership
                    var existingAchievement = await _context.Achievements
                        .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == currentUserId.Value);

                    if (existingAchievement == null)
                    {
                        return Forbid();
                    }

                    // Update only allowed fields
                    existingAchievement.Title = achievement.Title;
                    existingAchievement.Description = achievement.Description;
                    existingAchievement.Date = achievement.Date;

                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AchievementExists(achievement.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(achievement);
        }

        // Delete Achievement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var achievement = await _context.Achievements
                .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == currentUserId.Value);

            if (achievement == null)
            {
                return NotFound();
            }

            _context.Achievements.Remove(achievement);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        private bool AchievementExists(int id)
        {
            return _context.Achievements.Any(e => e.Id == id);
        }

        // API Endpoints for Likes and Comments
        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeRequest request)
        {
            try
            {
                // Get current user ID from session
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (!currentUserId.HasValue)
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                // Check if achievement exists
                var achievement = await _context.Achievements
                    .Include(a => a.Likes)
                    .FirstOrDefaultAsync(a => a.Id == request.AchievementId);

                if (achievement == null)
                {
                    return Json(new { success = false, message = "الإنجاز غير موجود" });
                }

                // Check if user already liked this achievement
                var existingLike = await _context.Likes
                    .FirstOrDefaultAsync(l => l.UserId == currentUserId.Value && l.AchievementId == request.AchievementId);

                if (existingLike != null)
                {
                    // Unlike
                    _context.Likes.Remove(existingLike);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        isLiked = false,
                        likesCount = achievement.Likes.Count - 1,
                        message = "تم إلغاء الإعجاب بنجاح"
                    });
                }
                else
                {
                    // Like
                    var newLike = new Like
                    {
                        UserId = currentUserId.Value,
                        AchievementId = request.AchievementId,
                        Date = DateTime.Now,
                        CreatedAt = DateTime.Now
                    };

                    _context.Likes.Add(newLike);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        isLiked = true,
                        likesCount = achievement.Likes.Count + 1,
                        message = "تم الإعجاب بنجاح"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling like for achievement {AchievementId}", request.AchievementId);
                return Json(new { success = false, message = "حدث خطأ أثناء تحديث الإعجاب" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] AddCommentRequest request)
        {
            try
            {
                // Get current user ID from session
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                var currentUserName = HttpContext.Session.GetString("UserName");
                
                if (!currentUserId.HasValue || string.IsNullOrEmpty(currentUserName))
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return Json(new { success = false, message = "يجب كتابة محتوى التعليق" });
                }

                // Check if achievement exists
                var achievement = await _context.Achievements
                    .Include(a => a.Comments)
                    .FirstOrDefaultAsync(a => a.Id == request.AchievementId);

                if (achievement == null)
                {
                    return Json(new { success = false, message = "الإنجاز غير موجود" });
                }

                // Create new comment
                var newComment = new Comment
                {
                    Content = request.Content,
                    Date = DateTime.Now,
                    UserId = currentUserId.Value,
                    AchievementId = request.AchievementId,
                    CreatedAt = DateTime.Now
                };

                _context.Comments.Add(newComment);
                await _context.SaveChangesAsync();

                // Get the user info for the response
                var user = await _context.Users.FindAsync(currentUserId.Value);

                var response = new
                {
                    success = true,
                    comment = new
                    {
                        id = newComment.Id,
                        content = newComment.Content,
                        date = newComment.Date,
                        user = new
                        {
                            id = user.Id,
                            name = user.Name
                        }
                    },
                    commentsCount = achievement.Comments.Count + 1,
                    message = "تم إضافة التعليق بنجاح"
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding comment for achievement {AchievementId}", request.AchievementId);
                return Json(new { success = false, message = "حدث خطأ أثناء إضافة التعليق" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAchievements()
        {
            try
            {
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                
                var achievements = await _context.Achievements
                    .Include(a => a.Owner)
                    .Include(a => a.Comments)
                        .ThenInclude(c => c.User)
                    .Include(a => a.Likes)
                        .ThenInclude(l => l.User)
                    .OrderByDescending(a => a.Date)
                    .ToListAsync();

                // Set IsLikedByCurrentUser for each achievement
                if (currentUserId.HasValue)
                {
                    foreach (var achievement in achievements)
                    {
                        achievement.IsLikedByCurrentUser = achievement.Likes.Any(l => l.UserId == currentUserId.Value);
                    }
                }

                return Json(new { success = true, achievements = achievements });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching achievements");
                return Json(new { success = false, message = "حدث خطأ أثناء جلب الإنجازات" });
            }
        }
    }
}
