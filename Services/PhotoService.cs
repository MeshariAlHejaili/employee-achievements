using EmployeeAchievementss.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace EmployeeAchievementss.Services
{
    public class PhotoService
    {
        private readonly IFileUploadService _fileUploadService;
        private readonly ApplicationDbContext _context;
        public PhotoService(IFileUploadService fileUploadService, ApplicationDbContext context)
        {
            _fileUploadService = fileUploadService;
            _context = context;
        }

        public async Task<bool> DeleteAchievementPhotoAsync(int photoId)
        {
            return await _fileUploadService.DeleteAchievementPhotoAsync(photoId);
        }

        public async Task DeleteAchievementPhotosAsync(int achievementId)
        {
            var photos = await _context.AchievementPhotos.Where(p => p.AchievementId == achievementId).ToListAsync();
            foreach (var photo in photos)
            {
                await _fileUploadService.DeleteAchievementPhotoAsync(photo.Id);
            }
        }

        public async Task<Manager?> GetManagerByUserIdAsync(int userId)
        {
            return await _context.Managers.FirstOrDefaultAsync(m => m.UserId == userId);
        }

        public async Task<AchievementPhoto?> GetAchievementPhotoByIdAsync(int photoId)
        {
            return await _context.AchievementPhotos.Include(p => p.Achievement).FirstOrDefaultAsync(p => p.Id == photoId);
        }

        // Add more methods for upload and retrieval as needed
    }
} 