using GymManagementSystem.DAL.Data.DataSeed;
using GymManagementSystem.DAL.DbContexts;
using GymManagementSystem.DAL.Models; // تأكد من الـ using ده عشان ApplicationUser
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GymManagementSystem.PL
{
    public static class ProgramExtensions
    {
        public static async Task MigrateAndSeedAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<GymDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            // جلب خدمات الـ Identity المطلوبة للـ Seeding الجديد
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // التحقق من وجود أي Migrations معلقة وتطبيقها تلقائياً
            var pending = await dbContext.Database.GetPendingMigrationsAsync();
            if (pending.Any())
            {
                logger.LogInformation($"Applying {pending.Count()} Pending Migrations");
                await dbContext.Database.MigrateAsync();
            }

            // 1. الـ Seeding الخاص ببيانات الجيم الأساسية (الملفات والأجهزة مثلاً)
            var seedFolderPath = Path.Combine(app.Environment.ContentRootPath, "wwwroot", "Files");
            await GymDataSeeding.SeedAsync(dbContext, seedFolderPath, logger);

            // 2. الـ Seeding الجديد الخاص بالـ Identity (الحسابات والأدوار رنا وسارة)
            await IdentityDataSeeding.SeedAsync(roleManager, userManager, logger);
        }
    }
}