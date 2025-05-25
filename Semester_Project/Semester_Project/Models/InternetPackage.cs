using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Semester_Project.Models
{
    [Table("Packages")] // Match this with your actual DB table name
    public class InternetPackage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PackageName { get; set; }

        [Required]
        public int Speed { get; set; }  // Mbps or other unit

        [Required]
        [Precision(18, 2)]
        public decimal Price { get; set; }  // Price charged to user

        [Required]
        [Precision(18, 2)]
        public decimal Cost { get; set; }   // Cost to ISP or your cost basis

        // Navigation property initialized to avoid null refs
        public ICollection<ISP_user> UserSubscriptions { get; set; } = new List<ISP_user>();
    }
}
