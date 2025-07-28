using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using EmployeeAchievementss.Models;

namespace EmployeeAchievementss.Services
{
    public interface IFileUploadService
    {
        Task<AchievementPhotoUploadResult> UploadAchievementPhotoAsync(IFormFile file, int achievementId);
        Task<bool> DeleteAchievementPhotoAsync(int photoId);
        Task<bool> DeleteAchievementPhotosAsync(int achievementId);
    }

    public class AchievementPhotoUploadResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public AchievementPhoto? Photo { get; set; }
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly ApplicationDbContext _context;

        // Configuration constants
        private const int MaxFileSizeBytes = 2 * 1024 * 1024; // 2MB
        private const int MaxPhotosPerAchievement = 4;
        private const int MaxImageWidth = 1920;
        private const int MaxImageHeight = 1080;
        private const int ThumbnailWidth = 300;
        private const int ThumbnailHeight = 300;
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/bmp" };

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger, ApplicationDbContext context)
        {
            _environment = environment;
            _logger = logger;
            _context = context;
        }

        public async Task<AchievementPhotoUploadResult> UploadAchievementPhotoAsync(IFormFile file, int achievementId)
        {
            try
            {
                // Validate file
                var validationResult = ValidateFile(file);
                if (!validationResult.Success)
                {
                    return new AchievementPhotoUploadResult { Success = false, ErrorMessage = validationResult.ErrorMessage };
                }

                // Check if achievement exists
                var achievement = await _context.Achievements.FindAsync(achievementId);
                if (achievement == null)
                {
                    return new AchievementPhotoUploadResult { Success = false, ErrorMessage = "الإنجاز غير موجود" };
                }

                // Check photo limit
                var existingPhotosCount = await _context.AchievementPhotos
                    .Where(p => p.AchievementId == achievementId)
                    .CountAsync();
                
                if (existingPhotosCount >= MaxPhotosPerAchievement)
                {
                    return new AchievementPhotoUploadResult { Success = false, ErrorMessage = $"يمكن رفع {MaxPhotosPerAchievement} صور كحد أقصى لكل إنجاز" };
                }

                // Create directories if they don't exist
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "achievements");
                var thumbnailsDir = Path.Combine(uploadsDir, "thumbnails");
                
                Directory.CreateDirectory(uploadsDir);
                Directory.CreateDirectory(thumbnailsDir);

                // Generate unique filename
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var uniqueFileName = GenerateUniqueFileName();
                var originalFileName = Path.GetFileName(file.FileName);

                // Save original file
                var filePath = Path.Combine(uploadsDir, $"{uniqueFileName}{fileExtension}");
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Process image (resize if needed and create thumbnail)
                var thumbnailFileName = await ProcessImageAsync(filePath, uniqueFileName, fileExtension);

                // Create database record
                var photo = new AchievementPhoto
                {
                    FileName = uniqueFileName,
                    OriginalFileName = originalFileName,
                    FileExtension = fileExtension.TrimStart('.'),
                    FileSize = file.Length,
                    ContentType = file.ContentType,
                    ThumbnailFileName = thumbnailFileName,
                    AchievementId = achievementId,
                    DisplayOrder = existingPhotosCount + 1,
                    CreatedAt = DateTime.Now
                };

                _context.AchievementPhotos.Add(photo);
                await _context.SaveChangesAsync();

                return new AchievementPhotoUploadResult
                {
                    Success = true,
                    Photo = photo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading achievement photo for achievement {AchievementId}", achievementId);
                return new AchievementPhotoUploadResult { Success = false, ErrorMessage = "حدث خطأ أثناء رفع الصورة" };
            }
        }

        public async Task<bool> DeleteAchievementPhotoAsync(int photoId)
        {
            try
            {
                var photo = await _context.AchievementPhotos.FindAsync(photoId);
                if (photo == null)
                    return false;

                // Delete physical files
                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "achievements");
                var filePath = Path.Combine(uploadsDir, photo.FullFileName);
                var thumbnailPath = Path.Combine(uploadsDir, "thumbnails", photo.FullThumbnailFileName);

                if (File.Exists(filePath))
                    File.Delete(filePath);

                if (!string.IsNullOrEmpty(photo.ThumbnailFileName) && File.Exists(thumbnailPath))
                    File.Delete(thumbnailPath);

                // Delete database record
                _context.AchievementPhotos.Remove(photo);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting achievement photo {PhotoId}", photoId);
                return false;
            }
        }

        public async Task<bool> DeleteAchievementPhotosAsync(int achievementId)
        {
            try
            {
                var photos = await _context.AchievementPhotos
                    .Where(p => p.AchievementId == achievementId)
                    .ToListAsync();

                var uploadsDir = Path.Combine(_environment.WebRootPath, "uploads", "achievements");

                foreach (var photo in photos)
                {
                    var filePath = Path.Combine(uploadsDir, photo.FullFileName);
                    var thumbnailPath = Path.Combine(uploadsDir, "thumbnails", photo.FullThumbnailFileName);

                    if (File.Exists(filePath))
                        File.Delete(filePath);

                    if (!string.IsNullOrEmpty(photo.ThumbnailFileName) && File.Exists(thumbnailPath))
                        File.Delete(thumbnailPath);
                }

                _context.AchievementPhotos.RemoveRange(photos);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting achievement photos for achievement {AchievementId}", achievementId);
                return false;
            }
        }

        private (bool Success, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (false, "الملف فارغ");

            if (file.Length > MaxFileSizeBytes)
                return (false, $"حجم الملف يجب أن يكون أقل من {MaxFileSizeBytes / (1024 * 1024)} ميجابايت");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return (false, "نوع الملف غير مدعوم. الأنواع المدعومة: JPG, PNG, GIF, BMP");

            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return (false, "نوع الملف غير مدعوم");

            return (true, null);
        }

        private string GenerateUniqueFileName()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            var randomBytes = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var randomString = Convert.ToBase64String(randomBytes).Replace("/", "").Replace("+", "").Substring(0, 8);
            return $"{timestamp}_{randomString}";
        }

        private async Task<string?> ProcessImageAsync(string filePath, string fileName, string extension)
        {
            try
            {
                using var image = Image.FromFile(filePath);
                
                // Resize if image is too large
                var resizedImage = ResizeImageIfNeeded(image);
                
                // Create thumbnail
                var thumbnail = CreateThumbnail(image);
                
                // Save resized image (if resized)
                if (resizedImage != image)
                {
                    image.Dispose();
                    resizedImage.Save(filePath, GetImageFormat(extension));
                    resizedImage.Dispose();
                }

                // Save thumbnail
                var thumbnailFileName = $"{fileName}_thumb";
                var thumbnailPath = Path.Combine(_environment.WebRootPath, "uploads", "achievements", "thumbnails", $"{thumbnailFileName}{extension}");
                thumbnail.Save(thumbnailPath, GetImageFormat(extension));
                thumbnail.Dispose();

                return thumbnailFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image {FilePath}", filePath);
                return null;
            }
        }

        private Image ResizeImageIfNeeded(Image image)
        {
            if (image.Width <= MaxImageWidth && image.Height <= MaxImageHeight)
                return image;

            var ratio = Math.Min((double)MaxImageWidth / image.Width, (double)MaxImageHeight / image.Height);
            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var resizedImage = new Bitmap(newWidth, newHeight);
            using (var graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        private Image CreateThumbnail(Image image)
        {
            var thumbnail = new Bitmap(ThumbnailWidth, ThumbnailHeight);
            using (var graphics = Graphics.FromImage(thumbnail))
            {
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                
                // Calculate aspect ratio to maintain proportions
                var ratio = Math.Min((double)ThumbnailWidth / image.Width, (double)ThumbnailHeight / image.Height);
                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);
                
                // Center the image
                var x = (ThumbnailWidth - newWidth) / 2;
                var y = (ThumbnailHeight - newHeight) / 2;
                
                graphics.DrawImage(image, x, y, newWidth, newHeight);
            }

            return thumbnail;
        }

        private ImageFormat GetImageFormat(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                ".png" => ImageFormat.Png,
                ".gif" => ImageFormat.Gif,
                ".bmp" => ImageFormat.Bmp,
                _ => ImageFormat.Jpeg
            };
        }
    }
} 