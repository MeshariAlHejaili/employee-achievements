using EmployeeAchievementss.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using EmployeeAchievementss.Services;
using System.IO; // Added for Path and Directory
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Drawing;
using QuestPDF.Elements;
using System.Globalization;

namespace EmployeeAchievementss.Controllers
{
    public class AchievementActionRequest
    {
        public int id { get; set; }
    }
    public class ManagerController : BaseController
    {
        private readonly ILogger<ManagerController> _logger;
        private readonly ManagerService _managerService;

        public ManagerController(ILogger<ManagerController> logger, ManagerService managerService)
        {
            _logger = logger;
            _managerService = managerService;
        }

        public async Task<IActionResult> ManagerDashboard()
        {
            var currentUserId = CurrentUserId;
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }
            var manager = await _managerService.GetManagerByUserId(currentUserId.Value);
            if (manager == null)
            {
                return Forbid();
            }
            var pendingAchievements = await _managerService.GetPendingAchievementsForManager(manager.Id);
            var employees = await _managerService.GetEmployeesByManagerId(manager.Id);
            ViewBag.Manager = manager;
            ViewBag.Employees = employees;
            return View("~/Views/Home/ManagerDashboard.cshtml", pendingAchievements);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveAchievement([FromBody] AchievementActionRequest req)
        {
            int id = req.id;
            var currentUserId = CurrentUserId;
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
            }
            var manager = await _managerService.GetManagerByUserId(currentUserId.Value);
            if (manager == null)
            {
                return Json(new { success = false, message = "غير مصرح لك" });
            }
            var achievement = await _managerService.GetAchievementByIdAndStatus(id, "Pending");
            if (achievement == null)
            {
                _logger.LogWarning($"[ApproveAchievement] Achievement not found or not pending. AchievementId: {id}");
                return Json(new { success = false, message = "الإنجاز غير موجود أو ليس قيد الانتظار" });
            }
            _logger.LogInformation($"[ApproveAchievement] AchievementId: {achievement.Id}, OwnerId: {achievement.OwnerId}, Owner.ManagerId: {achievement.Owner?.ManagerId}, LoggedInManagerId: {manager.Id}");
            if (achievement.Owner == null || achievement.Owner.ManagerId != manager.Id)
            {
                _logger.LogWarning($"[ApproveAchievement] Manager mismatch. AchievementId: {achievement.Id}, Owner.ManagerId: {achievement.Owner?.ManagerId}, LoggedInManagerId: {manager.Id}");
                return Json(new { success = false, message = "الإنجاز لا يتبع إدارتك" });
            }
            achievement.Status = "Approved";
            await _managerService.SaveChangesAsync();
            return Json(new { success = true, message = "تمت الموافقة على الإنجاز" });
        }

        [HttpPost]
        public async Task<IActionResult> RejectAchievement([FromBody] AchievementActionRequest req)
        {
            int id = req.id;
            var currentUserId = CurrentUserId;
            if (!currentUserId.HasValue)
            {
                return Json(new { success = false, message = "يجب تسجيل الدخول أولاً" });
            }
            var manager = await _managerService.GetManagerByUserId(currentUserId.Value);
            if (manager == null)
            {
                return Json(new { success = false, message = "غير مصرح لك" });
            }
            var achievement = await _managerService.GetAchievementByIdAndStatus(id, "Pending");
            if (achievement == null)
            {
                _logger.LogWarning($"[RejectAchievement] Achievement not found or not pending. AchievementId: {id}");
                return Json(new { success = false, message = "الإنجاز غير موجود أو ليس قيد الانتظار" });
            }
            _logger.LogInformation($"[RejectAchievement] AchievementId: {achievement.Id}, OwnerId: {achievement.OwnerId}, Owner.ManagerId: {achievement.Owner?.ManagerId}, LoggedInManagerId: {manager.Id}");
            if (achievement.Owner == null || achievement.Owner.ManagerId != manager.Id)
            {
                _logger.LogWarning($"[RejectAchievement] Manager mismatch. AchievementId: {achievement.Id}, Owner.ManagerId: {achievement.Owner?.ManagerId}, LoggedInManagerId: {manager.Id}");
                return Json(new { success = false, message = "الإنجاز لا يتبع إدارتك" });
            }
            achievement.Status = "Rejected";
            await _managerService.SaveChangesAsync();
            return Json(new { success = true, message = "تم رفض الإنجاز" });
        }

        public async Task<IActionResult> GenerateReport(string type, int? employeeId, DateTime? startDate, DateTime? endDate)
        {
            var currentUserId = CurrentUserId;
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Auth");
            }
            var manager = await _managerService.GetManagerByUserId(currentUserId.Value);
            if (manager == null)
            {
                return Forbid();
            }
            var achievements = await _managerService.GetApprovedAchievementsByDepartment(manager.DepartmentId);
            if (type == "byEmployee" && employeeId.HasValue)
            {
                achievements = achievements.Where(a => a.OwnerId == employeeId.Value).ToList();
            }
            if (type == "byDate" && startDate.HasValue && endDate.HasValue)
            {
                achievements = achievements.Where(a => a.Date.Date >= startDate.Value.Date && a.Date.Date <= endDate.Value.Date).ToList();
            }

            // if (type == "summary")
            // {
            //     // Generate PDF using QuestPDF
            //     string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "amana-logo.png");
            //     byte[] logoBytes = System.IO.File.Exists(logoPath) ? System.IO.File.ReadAllBytes(logoPath) : null;
            //     string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm", new CultureInfo("ar-SA"));
            //     // Get manager name, fallback to session if needed
            //     string managerName = manager.User?.Name;
            //     if (string.IsNullOrWhiteSpace(managerName))
            //     {
            //         managerName = HttpContext.Session.GetString("UserName") ?? "-";
            //     }
            //     var achievementsList = achievements.ToList();

            //     var pdfBytes = Document.Create(container =>
            //     {
            //         container.Page(page =>
            //         {
            //             page.Size(PageSizes.A4);
            //             page.Margin(30);
            //             page.DefaultTextStyle(x => x.FontFamily("Tajawal").FontSize(12));
            //             page.Content()
            //                 .Column(col =>
            //                 {
            //                     col.Item().Row(row =>
            //                     {
            //                         row.ConstantItem(120).AlignCenter().AlignMiddle().Height(60).Image(logoBytes);
            //                         row.RelativeItem().Column(headerCol =>
            //                         {
            //                             headerCol.Item().Text($"تاريخ التقرير: {dateStr}").FontSize(10).AlignRight();
            //                             headerCol.Item().Text($"اسم المدير: {managerName}").FontSize(10).AlignRight();
            //                         });
            //                     });
            //                     col.Item().PaddingVertical(10);
            //                     col.Item().Table(table =>
            //                     {
            //                         // Header
            //                         table.ColumnsDefinition(columns =>
            //                         {
            //                             columns.RelativeColumn(2); // Owner Name
            //                             columns.RelativeColumn(3); // Title (wider)
            //                             columns.RelativeColumn(1); // Has Images
            //                             columns.RelativeColumn(2); // Date
            //                             columns.RelativeColumn(1); // Likes
            //                             columns.RelativeColumn(1); // Comments
            //                             columns.RelativeColumn(2); // Achievement ID
            //                         });
            //                         table.Header(header =>
            //                         {
            //                             header.Cell().Element(CellStyle).Text("اسم صاحب الإنجاز").AlignRight();
            //                             header.Cell().Element(CellStyle).Text("العنوان").AlignRight();
            //                             header.Cell().Element(CellStyle).Text("يوجد صور").AlignRight();
            //                             header.Cell().Element(CellStyle).Text("التاريخ").AlignRight();
            //                             header.Cell().Element(CellStyle).Text("عدد الإعجابات").AlignRight();
            //                             header.Cell().Element(CellStyle).Text("عدد التعليقات").AlignRight();
            //                             header.Cell().Element(CellStyle).Text("معرّف الإنجاز").AlignRight();

            //                             static IContainer CellStyle(IContainer container) => container.DefaultTextStyle(x => x.SemiBold()).Background("#F0F0F0").Padding(4).Border(1);
            //                         });
            //                         // Rows
            //                         foreach (var a in achievementsList)
            //                         {
            //                             table.Cell().Element(CellStyle).Text(a.Owner?.Name ?? "-").AlignRight();
            //                             table.Cell().Element(CellStyle).Text(a.Title ?? "-").AlignRight();
            //                             table.Cell().Element(CellStyle).Text(a.HasPhotos ? "نعم" : "لا").AlignRight();
            //                             table.Cell().Element(CellStyle).Text(a.Date.ToString("yyyy-MM-dd")).AlignRight();
            //                             table.Cell().Element(CellStyle).Text(a.LikesCount.ToString()).AlignRight();
            //                             table.Cell().Element(CellStyle).Text(a.CommentsCount.ToString()).AlignRight();
            //                             table.Cell().Element(CellStyle).Text(a.Id.ToString()).AlignRight();

            //                             static IContainer CellStyle(IContainer container) => container.Padding(4).Border(1);
            //                         }
            //                     });
            //                 });
            //         });
            //     }).GeneratePdf();
            //     string fileName = $"ملخص_الإنجازات_{dateStr.Replace(":", "-")}.pdf";
            //     return File(pdfBytes, "application/pdf", fileName);
            // }

            if (type == "summary")
            {
                // Generate PDF using QuestPDF
                string logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "amana-logo.png");
                byte[] logoBytes = System.IO.File.Exists(logoPath) ? System.IO.File.ReadAllBytes(logoPath) : null;
                string dateStr = DateTime.Now.ToString("yyyy-MM-dd HH:mm", new CultureInfo("ar-SA"));
                // Get manager name, fallback to session if needed
                string managerName = manager.User?.Name;
                if (string.IsNullOrWhiteSpace(managerName))
                {
                    managerName = HttpContext.Session.GetString("UserName") ?? "-";
                }
                var achievementsList = achievements.ToList();

                var pdfBytes = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontFamily("Tajawal").FontSize(12));
                        page.Content()
                            .Column(col =>
                            {
                                col.Item().Row(row =>
                                {
                                    row.ConstantItem(120).AlignCenter().AlignMiddle().Height(60).Image(logoBytes);
                                    row.RelativeItem().Column(headerCol =>
                                    {
                                        headerCol.Item().Text($"تاريخ التقرير: {dateStr}").FontSize(10).AlignRight();
                                        headerCol.Item().Text($"اسم المدير: {managerName}").FontSize(10).AlignRight();
                                    });
                                });
                                col.Item().PaddingVertical(10);
                                col.Item().Border(1).Table(table =>
                                {
                                    // Header
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(2); // Achievement ID
                                        columns.RelativeColumn(1); // Has Images
                                        columns.RelativeColumn(1); // Comments
                                        columns.RelativeColumn(1); // Likes
                                        columns.RelativeColumn(2); // Date
                                        columns.RelativeColumn(3); // Title (wider)
                                        columns.RelativeColumn(2); // Owner Name
                                    });

                                    // Apply border to all cells
                                    static IContainer CellStyle(IContainer container) =>
                                        container.BorderBottom(1).BorderLeft(1).BorderRight(1).PaddingVertical(8).PaddingHorizontal(4);

                                    table.Header(header =>
                                    {
                                        header.Cell().Element(CellStyle).BorderTop(1).Text("معرّف الإنجاز").AlignRight();
                                        header.Cell().Element(CellStyle).BorderTop(1).Text("عدد التعليقات").AlignRight();
                                        header.Cell().Element(CellStyle).BorderTop(1).Text("عدد الإعجابات").AlignRight();
                                        header.Cell().Element(CellStyle).BorderTop(1).Text("يوجد صور").AlignRight();
                                        header.Cell().Element(CellStyle).BorderTop(1).Text("التاريخ").AlignRight();
                                        header.Cell().Element(CellStyle).BorderTop(1).Text("العنوان").AlignRight();
                                        header.Cell().Element(CellStyle).BorderTop(1).Text("اسم صاحب الإنجاز").AlignRight();

                                        static IContainer HeaderCellStyle(IContainer container) =>
                                            CellStyle(container).DefaultTextStyle(x => x.SemiBold()).Background("#F0F0F0");
                                    });

                                    // Rows
                                    foreach (var a in achievementsList)
                                    {
                                        table.Cell().Element(CellStyle).Text(a.Id.ToString()).AlignRight();
                                        table.Cell().Element(CellStyle).Text(a.CommentsCount.ToString()).AlignRight();
                                        table.Cell().Element(CellStyle).Text(a.LikesCount.ToString()).AlignRight();
                                        table.Cell().Element(CellStyle).Text(a.HasPhotos ? "نعم" : "لا").AlignRight();
                                        table.Cell().Element(CellStyle).Text(a.Date.ToString("yyyy-MM-dd")).AlignRight();
                                        table.Cell().Element(CellStyle).Text(a.Title ?? "-").AlignRight();
                                        table.Cell().Element(CellStyle).Text(a.Owner?.Name ?? "-").AlignRight();
                                    }
                                });
                            });
                    });
                }).GeneratePdf();
                string fileName = $"ملخص_الإنجازات_{dateStr.Replace(":", "-")}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
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