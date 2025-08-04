using System.Diagnostics;
using EmployeeAchievementss.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Linq;
using EmployeeAchievementss.Services;

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
    public class FeedController : BaseController
    {
        private readonly ILogger<FeedController> _logger;
        private readonly AchievementService _achievementService;

        public FeedController(ILogger<FeedController> logger, AchievementService achievementService)
        {
            _logger = logger;
            _achievementService = achievementService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUserId = CurrentUserId;
            var achievements = await _achievementService.GetApprovedAchievementsAsync();
            if (currentUserId.HasValue)
            {
                foreach (var achievement in achievements)
                {
                    achievement.IsLikedByCurrentUser = achievement.Likes.Any(l => l.UserId == currentUserId.Value);
                }
            }
            ViewBag.CurrentUserId = currentUserId;
            return View("~/Views/Home/Index.cshtml", achievements);
        }

        public IActionResult Privacy()
        {
            return View("~/Views/Home/Privacy.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("~/Views/Home/Error.cshtml", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Details(int id)
        {
            var achievement = await _achievementService.GetAchievementDetailsAsync(id);
            if (achievement == null)
                return NotFound();
            return View("~/Views/Home/Details.cshtml", achievement);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike([FromBody] ToggleLikeRequest request)
        {
            try
            {
                var currentUserId = CurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }
                var achievement = await _achievementService.GetAchievementDetailsAsync(request.AchievementId);
                if (achievement == null)
                {
                    return Json(new { success = false, message = "الإنجاز غير موجود" });
                }
                var existingLike = achievement.Likes.FirstOrDefault(l => l.UserId == currentUserId.Value);
                if (existingLike != null)
                {
                    _achievementService.RemoveLike(achievement, existingLike);
                    return Json(new { success = true, isLiked = false, likesCount = achievement.Likes.Count , message = "تم إلغاء الإعجاب بنجاح" });
                }
                else
                {
                    _achievementService.AddLike(achievement, currentUserId.Value);
                    return Json(new { success = true, isLiked = true, likesCount = achievement.Likes.Count , message = "تم الإعجاب بنجاح" });
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
                var currentUserId = CurrentUserId;
                var currentUserName = CurrentUserName;
                if (!currentUserId.HasValue || string.IsNullOrEmpty(currentUserName))
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }
                if (string.IsNullOrWhiteSpace(request.Content))
                {
                    return Json(new { success = false, message = "يجب كتابة محتوى التعليق" });
                }
                var achievement = await _achievementService.GetAchievementDetailsAsync(request.AchievementId);
                if (achievement == null)
                {
                    return Json(new { success = false, message = "الإنجاز غير موجود" });
                }
                _achievementService.AddComment(achievement, currentUserId.Value, request.Content);
                var user = await _achievementService.GetUserByIdAsync(currentUserId.Value);
                var response = new
                {
                    success = true,
                    comment = new
                    {
                        id = achievement.Comments.Last().Id, // Assuming the last comment is the one just added
                        content = achievement.Comments.Last().Content,
                        date = achievement.Comments.Last().Date,
                        user = new { id = user.Id, name = user.Name }
                    },
                    commentsCount = achievement.Comments.Count,
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
                var currentUserId = CurrentUserId;
                var achievements = await _achievementService.GetApprovedAchievementsAsync();
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