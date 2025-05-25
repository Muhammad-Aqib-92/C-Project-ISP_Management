using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Semester_Project.Models
{
    public class ISP_user
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        [Required]
        public string Email { get; set; }

        public string? Phone { get; set; }

        public string? Address { get; set; }

        // Foreign key to InternetPackage
        public int? InternetPackageId { get; set; }

        [ForeignKey("InternetPackageId")]
        public InternetPackage? InternetPackage { get; set; }

        public bool? IsPaid { get; set; } = false;

        public bool IsActive { get; set; } = true;

        [Precision(18, 2)]
        public decimal Price { get; set; } = 0;

        [Precision(18, 2)]
        public decimal Cost { get; set; } = 0;
    }
}
