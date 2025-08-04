using EmployeeAchievementss.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EmployeeAchievementss.Services
{
    public class AchievementService
    {
        private readonly ApplicationDbContext _context;
        public AchievementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Achievement>> GetApprovedAchievementsAsync()
        {
            return await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.Likes).ThenInclude(l => l.User)
                .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
                .Where(a => a.Status == "Approved")
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<Achievement?> GetAchievementDetailsAsync(int id)
        {
            return await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.Likes).ThenInclude(l => l.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Achievement?> GetAchievementForEditAsync(int id, int userId)
        {
            return await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
                .FirstOrDefaultAsync(a => a.Id == id && a.OwnerId == userId);
        }

        public async Task AddAchievementAsync(Achievement achievement, List<IFormFile> photos)
        {
            _context.Achievements.Add(achievement);
            await _context.SaveChangesAsync();
            // Photo upload logic should be handled in PhotoService, but placeholder here for now
        }

        public async Task<Achievement?> GetAchievementByIdAsync(int id)
        {
            return await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
                .Include(a => a.Comments).ThenInclude(c => c.User)
                .Include(a => a.Likes).ThenInclude(l => l.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateAchievementAsync(Achievement achievement, List<IFormFile> photos)
        {
            var existing = await _context.Achievements.FindAsync(achievement.Id);
            if (existing != null)
            {
                existing.Title = achievement.Title;
                existing.Description = achievement.Description;
                existing.Date = achievement.Date;
                // Photo upload logic should be handled in PhotoService, but placeholder here for now
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> AchievementExistsAsync(int id)
        {
            return await _context.Achievements.AnyAsync(e => e.Id == id);
        }

        public bool AchievementExists(int id)
        {
            return _context.Achievements.Any(e => e.Id == id);
        }

        public async Task DeleteAchievementAsync(int id)
        {
            var achievement = await _context.Achievements.FindAsync(id);
            if (achievement != null)
            {
                _context.Achievements.Remove(achievement);
                await _context.SaveChangesAsync();
            }
        }

        public void RemoveLike(Achievement achievement, Like like)
        {
            achievement.Likes.Remove(like);
            _context.Likes.Remove(like);
            _context.SaveChanges();
        }

        public void AddLike(Achievement achievement, int userId)
        {
            var newLike = new Like
            {
                UserId = userId,
                AchievementId = achievement.Id,
                Date = DateTime.Now,
                CreatedAt = DateTime.Now
            };
            achievement.Likes.Add(newLike);
            _context.Likes.Add(newLike);
            _context.SaveChanges();
        }

        public void AddComment(Achievement achievement, int userId, string content)
        {
            var newComment = new Comment
            {
                Content = content,
                Date = DateTime.Now,
                UserId = userId,
                AchievementId = achievement.Id,
                CreatedAt = DateTime.Now
            };
            achievement.Comments.Add(newComment);
            _context.Comments.Add(newComment);
            _context.SaveChanges();
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }
    }
} 