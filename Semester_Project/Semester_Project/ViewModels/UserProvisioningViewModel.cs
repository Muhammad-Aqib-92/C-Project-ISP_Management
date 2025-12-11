using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Semester_Project.Models;

namespace Semester_Project.ViewModels
{
    public class UserProvisioningViewModel
    {
        // Identity Fields
        [Required]
        [EmailAddress]
        [Display(Name = "Email (Login Username)")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // ISP_user Profile Fields
        [Required]
        public string Name { get; set; }

        [Phone]
        public string? Phone { get; set; }

        public string? Address { get; set; }

        [Display(Name = "Internet Package")]
        public int? InternetPackageId { get; set; }
    }
}
