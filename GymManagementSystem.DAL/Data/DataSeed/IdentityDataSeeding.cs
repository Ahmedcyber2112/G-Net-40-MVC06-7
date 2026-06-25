using GymManagementSystem.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymManagementSystem.DAL.Data.DataSeed
{
    public static class IdentityDataSeeding
    {
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ILogger logger,
            CancellationToken ct = default)
        {
            try
            {
                bool HasUsers = userManager.Users.Any();
                bool HasRoles = roleManager.Roles.Any();

                if (HasUsers && HasRoles) return;

                // 1. إنشاء الأدوار (Roles) إذا لم تكن موجودة
                if (!HasRoles)
                {
                    var Roles = new List<IdentityRole>()
                    {
                        new IdentityRole() { Name = "SuperAdmin" },
                        new IdentityRole() { Name = "Admin" }
                    };

                    foreach (var roleName in Roles.Select(R => R.Name))
                    {
                        if (!await roleManager.RoleExistsAsync(roleName!))
                        {
                            var roleResult = await roleManager.CreateAsync(new IdentityRole(roleName!));
                            if (!roleResult.Succeeded)
                            {
                                logger.LogError("Failed To Create Role: {Role}, {Errors}", roleName,
                                    string.Join(";", roleResult.Errors.Select(e => e.Description)));
                            }
                        }
                    }
                }

                // 2. إنشاء مستخدم الـ SuperAdmin الأساسي
                if (!HasUsers)
                {
                    var MainAdmin = new ApplicationUser()
                    {
                        FirstName = "Gerges",
                        LastName = "Malak",
                        UserName = "GergesMalak",
                        Email = "gmf20003@gmail.com",
                        PhoneNumber = "01558584205"
                    };

                    var mainAdminResult = await userManager.CreateAsync(MainAdmin, "GergesMalak");
                    if (mainAdminResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(MainAdmin, "SuperAdmin");
                        logger.LogInformation($"Seeded SuperAdmin: {MainAdmin.Email}");
                    }
                    else
                    {
                        logger.LogError("Failed To Create Seed SuperAdmin: {Errors}",
                            string.Join(";", mainAdminResult.Errors.Select(e => e.Description)));
                    }

                    // 3. إنشاء مستخدم الـ Admin الأساسي الثاني
                    var Admin01 = new ApplicationUser()
                    {
                        FirstName = "Hanan",
                        LastName = "Malak",
                        UserName = "HananMalak",
                        Email = "hmf20008@gmail.com",
                        PhoneNumber = "01274340054"
                    };

                    var CreateResult = await userManager.CreateAsync(Admin01, "GergesMalak");
                    if (CreateResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(Admin01, "Admin"); // هنا يتم ربطه بدور Admin
                        logger.LogInformation($"Seeded Admin01: {Admin01.Email}");
                    }
                    else
                    {
                        logger.LogError("Failed To Create Seed Admin01: {Errors}",
                            string.Join(";", CreateResult.Errors.Select(e => e.Description)));
                        return;
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Identity Seeding Failed");
                throw;
            }
        }
    }
}