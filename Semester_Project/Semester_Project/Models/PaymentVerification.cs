using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Semester_Project.Models
{
    public class PaymentVerification
    {
        [Key]
        public int Id { get; set; }

        public int ISP_userId { get; set; }
        
        [ForeignKey("ISP_userId")]
        public ISP_user? ISP_User { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Display(Name = "Transaction ID / Reference")]
        public string? TransactionReference { get; set; }

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ProcessedAt { get; set; }
        
        public string? AdminRemarks { get; set; }
    }
}
