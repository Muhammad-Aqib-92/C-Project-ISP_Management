using Microsoft.EntityFrameworkCore;
using Semester_Project.Data;
using Semester_Project.Models;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Semester_Project.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public DashboardViewModel GetDashboardViewModel()
        {
            var totalCustomers = _context.ISP_Users.Count();

            var totalRevenue = _context.ISP_Users
                .Where(u => u.IsPaid == true && u.InternetPackage != null)
                .Sum(u => u.Price);

            var unpaidCustomers = _context.ISP_Users.Count(u => u.IsPaid == false);

            var cost = _context.ISP_Users
                .Where(u => u.IsPaid == true && u.InternetPackage != null)
                .Sum(u => u.InternetPackage.Cost);

            var profit = totalRevenue - cost;

            // Fetch 5 most recent customers
            var recentCustomers = _context.ISP_Users
                .Include(u => u.InternetPackage)
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ToList();

            // Calculate package distribution
            var packageStats = _context.ISP_Users
                .Where(u => u.InternetPackage != null)
                .GroupBy(u => u.InternetPackage.PackageName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionary(k => k.Name, v => v.Count);

            // Chart Data Generation (Simulating history as PaymentHistory table is not yet implemented)
            var revenueMonths = new List<string>();
            var revenueAmounts = new List<decimal>();
            var today = DateTime.Today;

            for (int i = 5; i >= 0; i--)
            {
                var month = today.AddMonths(-i);
                revenueMonths.Add(month.ToString("MMM"));
                
                if (i == 0)
                {
                    revenueAmounts.Add(totalRevenue);
                }
                else
                {
                    // Mock historical data for demonstration
                    revenueAmounts.Add(totalRevenue * (decimal)(0.8 + (0.02 * i))); 
                }
            }

            return new DashboardViewModel
            {
                TotalCustomers = totalCustomers,
                TotalRevenue = totalRevenue,
                UnpaidCustomers = unpaidCustomers,
                Profit = profit,
                RecentCustomers = recentCustomers,
                PackageDistribution = packageStats,
                RevenueMonths = revenueMonths,
                RevenueAmounts = revenueAmounts,
                OpenTickets = _context.SupportTickets.Count(t => t.Status == TicketStatus.Open)
            };
        }
    }
}
