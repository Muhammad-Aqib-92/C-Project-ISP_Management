using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Semester_Project.Models;

namespace Semester_Project.Data
{
    public class ApplicationDbContext : IdentityDbContext<myappuser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ISP_user> ISP_Users { get; set; }
        public DbSet<InternetPackage> InternetPackages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: Configure table names or relationships explicitly here if needed
        }
    }
}
