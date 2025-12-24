using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Semester_Project.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Semester_Project.Services
{
    public class ExpirationCheckService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpirationCheckService> _logger;

        public ExpirationCheckService(IServiceProvider serviceProvider, ILogger<ExpirationCheckService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expiration Check Service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                        var today = DateTime.Today;

                        // Find users who are Paid but their expiration date has passed
                        var expiredUsers = context.ISP_Users
                            .Where(u => u.IsPaid == true && u.PackageExpiryDate != null && u.PackageExpiryDate < today)
                            .ToList();

                        if (expiredUsers.Any())
                        {
                            _logger.LogInformation($"Found {expiredUsers.Count} expired subscriptions. Updating status...");

                            foreach (var user in expiredUsers)
                            {
                                user.IsPaid = false;
                                // Optional: Reset Expiry Date or keep it to show when it expired?
                                // Keeping it allows us to say "Expired on [Date]"
                            }

                            await context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for expired subscriptions.");
                }

                // Check again in 24 hours (or testing interval)
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
