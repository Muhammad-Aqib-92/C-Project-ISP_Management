using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Semester_Project.Models
{
    public enum TicketStatus
    {
        Open,
        Pending,
        Resolved
    }

    public class SupportTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Subject { get; set; }

        [Required]
        public string Description { get; set; }

        public TicketStatus Status { get; set; } = TicketStatus.Open;
        
        public string? Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Link to registered User (assuming tickets are from system users or ISP users? 
        // Requirement says "Foreign Key to ISP_user", but usually tickets are from logged in users.
        // I will link to ISP_user for simplicity as requested).
        public int? UserId { get; set; }

        [ForeignKey("UserId")]
        public ISP_user? User { get; set; }
    }
}
