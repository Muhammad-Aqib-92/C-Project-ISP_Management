using System;
using System.ComponentModel.DataAnnotations;

namespace Semester_Project.Models
{
    public class PaymentSetting
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Bank Name")]
        public string BankName { get; set; }

        [Required]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }

        [Required]
        [Display(Name = "Account Title")]
        public string AccountTitle { get; set; }
        
        [Display(Name = "Payment Instructions")]
        public string? Instructions { get; set; }
    }
}
