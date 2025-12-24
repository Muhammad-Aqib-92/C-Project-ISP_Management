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
            var totalCustomers = _context.ISP_Users.AsNoTracking().Count();

            var totalRevenue = _context.ISP_Users
                .AsNoTracking()
                .Where(u => u.IsPaid == true && u.InternetPackage != null)
                .Sum(u => u.Price);

            var unpaidCustomers = _context.ISP_Users.AsNoTracking().Count(u => u.IsPaid == false);
            var pendingAmount = _context.ISP_Users
                .AsNoTracking()
                .Where(u => u.IsPaid == false)
                .Sum(u => u.Price);

            var paidCustomers = _context.ISP_Users.AsNoTracking().Count(u => u.IsPaid == true);

            var cost = _context.ISP_Users
                .AsNoTracking()
                .Where(u => u.IsPaid == true && u.InternetPackage != null)
                .Sum(u => u.InternetPackage.Cost);

            var profit = totalRevenue - cost;

            // Fetch 5 most recent customers
            var recentCustomers = _context.ISP_Users
                .Include(u => u.InternetPackage)
                .AsNoTracking()
                .OrderByDescending(u => u.Id)
                .Take(5)
                .ToList();

            // Calculate package distribution
            var packageStats = _context.ISP_Users
                .AsNoTracking()
                .Where(u => u.InternetPackage != null)
                .GroupBy(u => u.InternetPackage.PackageName)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .ToDictionary(k => k.Name, v => v.Count);

            // Chart Data Generation (Real Data from PaymentHistory)
            var revenueMonths = new List<string>();
            var revenueAmounts = new List<decimal>();
            var today = DateTime.Today;
            var sixMonthsAgo = today.AddMonths(-5);
            var startDate = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            // Fetch payments for the last 6 months
            var recentPayments = _context.PaymentHistories
                .AsNoTracking()
                .Where(p => p.PaymentDate >= startDate)
                .ToList();

            for (int i = 5; i >= 0; i--)
            {
                var targetDate = today.AddMonths(-i);
                revenueMonths.Add(targetDate.ToString("MMM"));
                
                var monthTotal = recentPayments
                    .Where(p => p.PaymentDate.Year == targetDate.Year && p.PaymentDate.Month == targetDate.Month)
                    .Sum(p => p.Amount);
                
                revenueAmounts.Add(monthTotal);
            }

            return new DashboardViewModel
            {
                TotalCustomers = totalCustomers,
                TotalRevenue = totalRevenue,
                UnpaidCustomers = unpaidCustomers,
                PendingAmount = pendingAmount,
                PaidCustomers = paidCustomers,
                Profit = profit,
                RecentCustomers = recentCustomers,
                PackageDistribution = packageStats,
                RevenueMonths = revenueMonths,
                RevenueAmounts = revenueAmounts,
                OpenTickets = _context.SupportTickets.AsNoTracking().Count(t => t.Status == TicketStatus.Open)
            };
        }
    }
}
