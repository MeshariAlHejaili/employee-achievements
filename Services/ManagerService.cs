using EmployeeAchievementss.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeAchievementss.Services
{
    public class ManagerService
    {
        private readonly ApplicationDbContext _context;
        public ManagerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Manager?> GetManagerByUserId(int userId)
        {
            return await _context.Managers.FirstOrDefaultAsync(m => m.UserId == userId);
        }

        public async Task<List<Achievement>> GetPendingAchievementsForManager(int managerId)
        {
            return await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Comments)
                .Include(a => a.Likes)
                .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
                .Where(a => a.Status == "Pending" && a.Owner.ManagerId == managerId)
                .ToListAsync();
        }

        public async Task<List<User>> GetEmployeesByManagerId(int managerId)
        {
            return await _context.Users.Where(u => u.ManagerId == managerId).ToListAsync();
        }

        public async Task<Achievement?> GetAchievementByIdAndStatus(int id, string status)
        {
            return await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Comments)
                .Include(a => a.Likes)
                .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
                .FirstOrDefaultAsync(a => a.Id == id && a.Status == status);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<List<Achievement>> GetApprovedAchievementsByDepartment(int departmentId)
        {
            return await _context.Achievements
                .Include(a => a.Owner)
                .Include(a => a.Comments)
                .Include(a => a.Likes)
                .Include(a => a.Photos.OrderBy(p => p.DisplayOrder))
                .Where(a => a.Owner.DepartmentId == departmentId && a.Status == "Approved")
                .ToListAsync();
        }
    }
} 