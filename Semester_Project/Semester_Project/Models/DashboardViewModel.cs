namespace Semester_Project.Models
{
    public class DashboardViewModel
    {
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public int UnpaidCustomers { get; set; }
        public decimal Profit { get; set; }

        // New properties for enhanced dashboard
        public List<ISP_user> RecentCustomers { get; set; } = new List<ISP_user>();
        public Dictionary<string, int> PackageDistribution { get; set; } = new Dictionary<string, int>();
        public int OpenTickets { get; set; }

        // Chart Data
        public List<string> RevenueMonths { get; set; } = new List<string>();
        public List<decimal> RevenueAmounts { get; set; } = new List<decimal>();
    }
}