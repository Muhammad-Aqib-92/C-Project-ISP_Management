using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Semester_Project.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Semester_Project.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, UserManager<myappuser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            // 1. Roles: Admin & User
            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@isp.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                var user = new myappuser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    city = "Headquarters",
                    state = "NA"
                };
                // Password: Admin@123
                await userManager.CreateAsync(user, "Admin@123"); 
                await userManager.AddToRoleAsync(user, "Admin");
            }
            else
            {
                // Ensure existing user has Admin role
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                   await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                // Reset Password to ensure access
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                await userManager.ResetPasswordAsync(adminUser, token, "Admin@123");
            }

            // 3. Seed Standard User (Customer)
            var userEmail = "user@isp.com";
            var standardUser = await userManager.FindByEmailAsync(userEmail);
            if (standardUser == null)
            {
                var user = new myappuser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    city = "Residential Area",
                    state = "NA"
                };
                // Password: User@123
                await userManager.CreateAsync(user, "User@123");
                await userManager.AddToRoleAsync(user, "User");
            }
            else
            {
                 // Ensure existing user has User role
                if (!await userManager.IsInRoleAsync(standardUser, "User"))
                {
                   await userManager.AddToRoleAsync(standardUser, "User");
                }
                 // Reset Password to ensure access
                var token = await userManager.GeneratePasswordResetTokenAsync(standardUser);
                await userManager.ResetPasswordAsync(standardUser, token, "User@123");
            }
        }
    }
}
