using System.Diagnostics;
using EmployeeAchievementss.Models;
using EmployeeAchievementss.Services;
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

        public class DeletePhotoRequest
        {
            public int PhotoId { get; set; }
        }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IFileUploadService _fileUploadService;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, IFileUploadService fileUploadService)
        {
            _logger = logger;
            _context = context;
            _fileUploadService = fileUploadService;
        }

        public async Task<IActionResult> Index()
        {
            // Get current user ID from session
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            
            // Get only approved achievements with related data
            var achievements = await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Comments)
                    .ThenInclude(c => c.User)
                .Include(a => a.Likes)
                    .ThenInclude(l => l.User)
                .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
                .Where(a => a.Status == "Approved")
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
        public async Task<IActionResult> AddAchievement(Achievement achievement, List<IFormFile> photos)
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

                // Upload photos if provided
                if (photos != null && photos.Any())
                {
                    foreach (var photo in photos.Take(4)) // Limit to 4 photos
                    {
                        if (photo.Length > 0)
                        {
                            var uploadResult = await _fileUploadService.UploadAchievementPhotoAsync(photo, achievement.Id);
                            if (!uploadResult.Success)
                            {
                                // Log the error but don't fail the entire achievement creation
                                _logger.LogWarning("Failed to upload photo for achievement {AchievementId}: {Error}", 
                                    achievement.Id, uploadResult.ErrorMessage);
                            }
                        }
                    }
                }

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
                .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
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
        public async Task<IActionResult> Edit(int id, Achievement achievement, List<IFormFile> photos)
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
                        .Include(a => a.Photos)
                        .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == currentUserId.Value);

                    if (existingAchievement == null)
                    {
                        return Forbid();
                    }

                    // Update only allowed fields
                    existingAchievement.Title = achievement.Title;
                    existingAchievement.Description = achievement.Description;
                    existingAchievement.Date = achievement.Date;

                    // Upload new photos if provided
                    if (photos != null && photos.Any())
                    {
                        var currentPhotoCount = existingAchievement.Photos.Count;
                        var maxNewPhotos = Math.Max(0, 4 - currentPhotoCount);
                        
                        foreach (var photo in photos.Take(maxNewPhotos))
                        {
                            if (photo.Length > 0)
                            {
                                var uploadResult = await _fileUploadService.UploadAchievementPhotoAsync(photo, achievement.Id);
                                if (!uploadResult.Success)
                                {
                                    _logger.LogWarning("Failed to upload photo for achievement {AchievementId}: {Error}", 
                                        achievement.Id, uploadResult.ErrorMessage);
                                }
                            }
                        }
                    }

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

            // Delete associated photos first
            await _fileUploadService.DeleteAchievementPhotosAsync(id);

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
                    .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
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

        [HttpGet]
        public async Task<IActionResult> GetAchievementPhotos(int achievementId)
        {
            try
            {
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (!currentUserId.HasValue)
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                // Get manager record for this user
                var manager = await _context.Managers.FirstOrDefaultAsync(m => m.UserId == currentUserId.Value);
                if (manager == null)
                {
                    return Json(new { success = false, message = "غير مصرح لك" });
                }

                // Get the achievement and verify it belongs to manager's employees
                var achievement = await _context.Achievements
                    .Include(a => a.Owner)
                    .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
                    .FirstOrDefaultAsync(a => a.Id == achievementId);

                if (achievement == null)
                {
                    return Json(new { success = false, message = "الإنجاز غير موجود" });
                }

                // Check if the achievement's owner is managed by this manager
                if (achievement.Owner.ManagerId != manager.Id)
                {
                    return Json(new { success = false, message = "الإنجاز لا يتبع إدارتك" });
                }

                var photos = achievement.Photos.Select(p => new
                {
                    id = p.Id,
                    fileName = p.FileName,
                    originalFileName = p.OriginalFileName,
                    fileExtension = p.FileExtension,
                    fileSize = p.FileSize,
                    contentType = p.ContentType,
                    thumbnailFileName = p.ThumbnailFileName,
                    displayOrder = p.DisplayOrder,
                    filePath = $"/uploads/achievements/{p.FullFileName}",
                    thumbnailPath = $"/uploads/achievements/thumbnails/{p.FullThumbnailFileName}"
                }).ToList();

                return Json(new { success = true, photos = photos });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting achievement photos for achievement {AchievementId}", achievementId);
                return Json(new { success = false, message = "حدث خطأ أثناء جلب الصور" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeletePhoto([FromBody] DeletePhotoRequest request)
        {
            try
            {
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (!currentUserId.HasValue)
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }

                // Get the photo and verify ownership
                var photo = await _context.AchievementPhotos
                    .Include(p => p.Achievement)
                    .FirstOrDefaultAsync(p => p.Id == request.PhotoId);

                if (photo == null)
                {
                    return Json(new { success = false, message = "الصورة غير موجودة" });
                }

                // Check if current user owns the achievement
                if (photo.Achievement.OwnerId != currentUserId.Value)
                {
                    return Json(new { success = false, message = "غير مصرح لك بحذف هذه الصورة" });
                }

                var success = await _fileUploadService.DeleteAchievementPhotoAsync(request.PhotoId);
                if (success)
                {
                    return Json(new { success = true, message = "تم حذف الصورة بنجاح" });
                }
                else
                {
                    return Json(new { success = false, message = "حدث خطأ أثناء حذف الصورة" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting photo {PhotoId}", request.PhotoId);
                return Json(new { success = false, message = "حدث خطأ أثناء حذف الصورة" });
            }
        }

        // Manager Dashboard
        public async Task<IActionResult> ManagerDashboard()
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Find manager record for this user
            var manager = await _context.Managers.FirstOrDefaultAsync(m => m.UserId == currentUserId.Value);
            if (manager == null)
            {
                return Forbid(); // Not a manager
            }

            // Get pending achievements for manager's employees only (must be in manager's department and managed by this manager)
            var pendingAchievements = await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
                .Where(a => a.Status == "Pending" && a.Owner.ManagerId == manager.Id)
                .ToListAsync();

            // Get employees for report filters
            var employees = await _context.Users.Where(u => u.ManagerId == manager.Id).ToListAsync();
            ViewBag.Manager = manager;
            ViewBag.Employees = employees;
            return View(pendingAchievements);
        }

        public class AchievementActionRequest
        {
            public int id { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAchievement([FromBody] AchievementActionRequest req)
        {
            int id = req.id;
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
            }

            // Get manager record for this user
            var manager = await _context.Managers.FirstOrDefaultAsync(m => m.UserId == currentUserId.Value);
            if (manager == null)
            {
                return Json(new { success = false, message = "غير مصرح لك" });
            }

            // Get the achievement and its owner
            var achievement = await _context.Achievements
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(a => a.Id == id && a.Status == "Pending");
            if (achievement == null)
            {
                _logger.LogWarning($"[ApproveAchievement] Achievement not found or not pending. AchievementId: {id}");
                return Json(new { success = false, message = "الإنجاز غير موجود أو ليس قيد الانتظار" });
            }

            // Debug: Log all relevant IDs
            _logger.LogInformation($"[ApproveAchievement] AchievementId: {achievement.Id}, OwnerId: {achievement.OwnerId}, Owner.ManagerId: {achievement.Owner?.ManagerId}, LoggedInManagerId: {manager.Id}");

            // Check if the achievement's owner is managed by this manager
            if (achievement.Owner == null || achievement.Owner.ManagerId != manager.Id)
            {
                _logger.LogWarning($"[ApproveAchievement] Manager mismatch. AchievementId: {achievement.Id}, Owner.ManagerId: {achievement.Owner?.ManagerId}, LoggedInManagerId: {manager.Id}");
                return Json(new { success = false, message = "الإنجاز لا يتبع إدارتك" });
            }

            achievement.Status = "Approved";
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "تمت الموافقة على الإنجاز" });
        }

        [HttpPost]
        public async Task<IActionResult> RejectAchievement([FromBody] AchievementActionRequest req)
        {
            int id = req.id;
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
            }

            // Get manager record for this user
            var manager = await _context.Managers.FirstOrDefaultAsync(m => m.UserId == currentUserId.Value);
            if (manager == null)
            {
                return Json(new { success = false, message = "غير مصرح لك" });
            }

            // Get the achievement and its owner
            var achievement = await _context.Achievements
                .Include(a => a.Owner)
                .FirstOrDefaultAsync(a => a.Id == id && a.Status == "Pending");
            if (achievement == null)
            {
                _logger.LogWarning($"[RejectAchievement] Achievement not found or not pending. AchievementId: {id}");
                return Json(new { success = false, message = "الإنجاز غير موجود أو ليس قيد الانتظار" });
            }

            // Debug: Log all relevant IDs
            _logger.LogInformation($"[RejectAchievement] AchievementId: {achievement.Id}, OwnerId: {achievement.OwnerId}, Owner.ManagerId: {achievement.Owner?.ManagerId}, LoggedInManagerId: {manager.Id}");

            // Check if the achievement's owner is managed by this manager
            if (achievement.Owner == null || achievement.Owner.ManagerId != manager.Id)
            {
                _logger.LogWarning($"[RejectAchievement] Manager mismatch. AchievementId: {achievement.Id}, Owner.ManagerId: {achievement.Owner?.ManagerId}, LoggedInManagerId: {manager.Id}");
                return Json(new { success = false, message = "الإنجاز لا يتبع إدارتك" });
            }

            achievement.Status = "Rejected";
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "تم رفض الإنجاز" });
        }

        // Report generation (by employee, by date, summary)
        public async Task<IActionResult> GenerateReport(string type, int? employeeId, DateTime? startDate, DateTime? endDate)
        {
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }

            var manager = await _context.Managers.FirstOrDefaultAsync(m => m.UserId == currentUserId.Value);
            if (manager == null)
            {
                return Forbid();
            }

            var achievements = await _context.Achievements
                .Include(a => a.Owner)
                .Where(a => a.Owner.DepartmentId == manager.DepartmentId && a.Status == "Approved")
                .ToListAsync();

            // Apply filters
            if (type == "byEmployee" && employeeId.HasValue)
            {
                achievements = achievements.Where(a => a.OwnerId == employeeId.Value).ToList();
            }
            if (type == "byDate" && startDate.HasValue && endDate.HasValue)
            {
                achievements = achievements.Where(a => a.Date.Date >= startDate.Value.Date && a.Date.Date <= endDate.Value.Date).ToList();
            }

            object report = null;
            switch (type)
            {
                case "byEmployee":
                    report = achievements.GroupBy(a => a.Owner.Name)
                        .Select(g => new { employee = g.Key, count = g.Count(), achievements = g.ToList() })
                        .ToList();
                    break;
                case "byDate":
                    report = achievements.GroupBy(a => a.Date.Date)
                        .Select(g => new { date = g.Key, count = g.Count(), achievements = g.ToList() })
                        .ToList();
                    break;
                case "summary":
                default:
                    report = new
                    {
                        total = achievements.Count,
                        employees = achievements.Select(a => a.Owner.Name).Distinct().Count(),
                        achievements = achievements
                    };
                    break;
            }

            return Json(new { success = true, report });
        }
    }
}
