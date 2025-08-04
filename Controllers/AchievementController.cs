using EmployeeAchievementss.Models;
using EmployeeAchievementss.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeAchievementss.Controllers
{
    public class DeletePhotoRequest
    {
        public int PhotoId { get; set; }
    }
    public class AchievementController : BaseController
    {
        private readonly ILogger<AchievementController> _logger;
        private readonly AchievementService _achievementService;
        private readonly PhotoService _photoService;

        public AchievementController(ILogger<AchievementController> logger, AchievementService achievementService, PhotoService photoService)
        {
            _logger = logger;
            _achievementService = achievementService;
            _photoService = photoService;
        }

        public IActionResult AddAchievement()
        {
            return View("~/Views/Home/AddAchievement.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAchievement(Achievement achievement, List<IFormFile> photos)
        {
            if (ModelState.IsValid)
            {
                var currentUserId = CurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return RedirectToAction("Login", "Auth");
                }
                achievement.OwnerId = currentUserId.Value;
                achievement.Date = DateTime.Now;
                achievement.CreatedAt = DateTime.Now;
                await _achievementService.AddAchievementAsync(achievement, photos);
                return RedirectToAction("Index", "Feed");
            }
            return View(achievement);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var currentUserId = CurrentUserId;
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }
            var achievement = await _achievementService.GetAchievementByIdAsync(id);
            if (achievement == null)
                return NotFound();
            if (achievement.OwnerId != currentUserId.Value)
            {
                return Forbid();
            }
            return View("~/Views/Home/Edit.cshtml", achievement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Achievement achievement, List<IFormFile> photos)
        {
            if (id != achievement.Id)
            {
                return NotFound();
            }
            var currentUserId = CurrentUserId;
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingAchievement = await _achievementService.GetAchievementByIdAsync(id);
                    if (existingAchievement == null)
                    {
                        return Forbid();
                    }
                    if (existingAchievement.OwnerId != currentUserId.Value)
                    {
                        return Forbid();
                    }
                    await _achievementService.UpdateAchievementAsync(achievement, photos);
                    return RedirectToAction("Index", "Feed");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _achievementService.AchievementExistsAsync(achievement.Id))
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = CurrentUserId;
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }
            var achievement = await _achievementService.GetAchievementByIdAsync(id);
            if (achievement == null)
            {
                return NotFound();
            }
            if (achievement.OwnerId != currentUserId.Value)
            {
                return Forbid();
            }
            await _photoService.DeleteAchievementPhotosAsync(id);
            await _achievementService.DeleteAchievementAsync(id);
            return RedirectToAction("Index", "Feed");
        }

        private bool AchievementExists(int id)
        {
            return _achievementService.AchievementExists(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetAchievementPhotos(int achievementId)
        {
            try
            {
                var currentUserId = CurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }
                var manager = await _photoService.GetManagerByUserIdAsync(currentUserId.Value);
                if (manager == null)
                {
                    return Json(new { success = false, message = "غير مصرح لك" });
                }
                var achievement = await _achievementService.GetAchievementByIdAsync(achievementId);
                if (achievement == null)
                {
                    return Json(new { success = false, message = "الإنجاز غير موجود" });
                }
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
                var currentUserId = CurrentUserId;
                if (!currentUserId.HasValue)
                {
                    return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
                }
                var photo = await _photoService.GetAchievementPhotoByIdAsync(request.PhotoId);
                if (photo == null)
                {
                    return Json(new { success = false, message = "الصورة غير موجودة" });
                }
                if (photo.Achievement.OwnerId != currentUserId.Value)
                {
                    return Json(new { success = false, message = "غير مصرح لك بحذف هذه الصورة" });
                }
                var success = await _photoService.DeleteAchievementPhotoAsync(request.PhotoId);
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
    }
} 