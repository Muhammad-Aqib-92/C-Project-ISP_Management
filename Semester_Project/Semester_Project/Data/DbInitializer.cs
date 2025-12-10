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
            // Ensure the database is created
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.EnsureCreated();

            // Seed Roles
            string[] roles = { "SuperAdmin", "SupportAgent", "FieldTech", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed SuperAdmin
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
                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "SuperAdmin");
            }

            // Seed SupportAgent
            var supportEmail = "support@isp.com";
            var supportUser = await userManager.FindByEmailAsync(supportEmail);
            if (supportUser == null)
            {
                var user = new myappuser
                {
                    UserName = supportEmail,
                    Email = supportEmail,
                    EmailConfirmed = true,
                    city = "Call Center",
                    state = "NA"
                };
                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "SupportAgent");
            }

            // Seed FieldTech
            var techEmail = "tech@isp.com";
            var techUser = await userManager.FindByEmailAsync(techEmail);
            if (techUser == null)
            {
                var user = new myappuser
                {
                    UserName = techEmail,
                    Email = techEmail,
                    EmailConfirmed = true,
                    city = "Field Office",
                    state = "NA"
                };
                await userManager.CreateAsync(user, "Pa$$w0rd");
                await userManager.AddToRoleAsync(user, "FieldTech");
            }
        }
    }
}
