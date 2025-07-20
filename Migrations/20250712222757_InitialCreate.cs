using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EmployeeAchievementss.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProfilePicture = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OwnerId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Achievements_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Content = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AchievementId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Comments_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Comments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Likes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    AchievementId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Likes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Likes_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Likes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Department", "Email", "Name", "Password", "Position", "ProfilePicture" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 6, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "تطوير البرمجيات", "mashari@amana.com", "مشاري الحربي", "123456", "مطور برمجيات", null },
                    { 2, new DateTime(2024, 6, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "إدارة المنتج", "sara@amana.com", "سارة أحمد", "123456", "مدير منتج", null },
                    { 3, new DateTime(2024, 6, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), "تطوير البرمجيات", "ahmed@amana.com", "أحمد محمد", "123456", "مطور خلفي", null }
                });

            migrationBuilder.InsertData(
                table: "Achievements",
                columns: new[] { "Id", "CreatedAt", "Date", "Description", "OwnerId", "Title" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 6, 2, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "إعادة تصميم كاملة لتدفق تأهيل المستخدمين لمشروع نيوسواك، مما أدى إلى تحسين تجربة المستخدم بنسبة 40% وتقليل وقت التدريب إلى النصف.", 1, "مشروع نيوسواك" },
                    { 2, new DateTime(2024, 6, 2, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "تطوير نظام تقارير جديد للإدارة يوفر رؤى شاملة عن أداء الفريق ومؤشرات الأداء الرئيسية.", 2, "تطوير نظام التقارير" },
                    { 3, new DateTime(2024, 6, 2, 9, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "تحسين أداء النظام الأساسي بنسبة 60% من خلال تحسين قاعدة البيانات وتحسين الخوارزميات.", 3, "تحسين أداء النظام" }
                });

            migrationBuilder.InsertData(
                table: "Comments",
                columns: new[] { "Id", "AchievementId", "Content", "CreatedAt", "Date", "UserId" },
                values: new object[,]
                {
                    { 1, 1, "عمل رائع! هذا سيساعد كثيراً في تحسين تجربة المستخدمين.", new DateTime(2024, 6, 3, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 25, 10, 30, 0, 0, DateTimeKind.Unspecified), 2 },
                    { 2, 1, "أحسنت! هذا التطوير سيحدث فرقاً كبيراً.", new DateTime(2024, 6, 3, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 25, 11, 15, 0, 0, DateTimeKind.Unspecified), 3 },
                    { 3, 2, "ممتاز! هذا النظام سيوفر لنا رؤية واضحة عن الأداء.", new DateTime(2024, 6, 3, 10, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 20, 14, 20, 0, 0, DateTimeKind.Unspecified), 1 }
                });

            migrationBuilder.InsertData(
                table: "Likes",
                columns: new[] { "Id", "AchievementId", "CreatedAt", "Date", "UserId" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 6, 4, 11, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 25, 9, 0, 0, 0, DateTimeKind.Unspecified), 2 },
                    { 2, 1, new DateTime(2024, 6, 4, 11, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 25, 9, 30, 0, 0, DateTimeKind.Unspecified), 3 },
                    { 3, 2, new DateTime(2024, 6, 4, 11, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 6, 20, 10, 0, 0, 0, DateTimeKind.Unspecified), 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_OwnerId",
                table: "Achievements",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_AchievementId",
                table: "Comments",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_UserId",
                table: "Comments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_AchievementId",
                table: "Likes",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId_AchievementId",
                table: "Likes",
                columns: new[] { "UserId", "AchievementId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments");

            migrationBuilder.DropTable(
                name: "Likes");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
