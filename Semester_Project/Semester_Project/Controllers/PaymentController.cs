using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semester_Project.Data;
using Semester_Project.Models;

namespace Semester_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Payment/Settings
        public async Task<IActionResult> Settings()
        {
            var setting = await _context.PaymentSettings.FirstOrDefaultAsync();
            return View(setting ?? new PaymentSetting());
        }

        // POST: Payment/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(PaymentSetting model)
        {
            if (ModelState.IsValid)
            {
                if (model.Id == 0)
                {
                    _context.PaymentSettings.Add(model);
                }
                else
                {
                    _context.PaymentSettings.Update(model);
                }
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Payment settings updated successfully.";
                return RedirectToAction(nameof(Settings));
            }
            return View(model);
        }

        // GET: Payment/Verifications
        public async Task<IActionResult> Verifications()
        {
            var requests = await _context.PaymentVerifications
                .Include(p => p.ISP_User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(requests);
        }

        // POST: Payment/Approve/5
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var request = await _context.PaymentVerifications.FindAsync(id);
            if (request == null) return NotFound();

            if (request.Status == "Approved")
            {
                TempData["ErrorMessage"] = "Request already approved.";
                return RedirectToAction(nameof(Verifications));
            }

            // 1. Mark Request Approved
            request.Status = "Approved";
            request.ProcessedAt = DateTime.Now;

            // 2. Mark User Paid
            var user = await _context.ISP_Users.FindAsync(request.ISP_userId);
            if (user != null)
            {
                user.IsPaid = true;
                user.PackageExpiryDate = DateTime.Now.AddMonths(1); // Renew for 1 month
                
                // Add Payment History
                var invoiceNumber = $"INV-{user.Id}-{DateTime.Now:yyyyMMddHHmmss}";
                _context.PaymentHistories.Add(new PaymentHistory
                {
                    UserId = user.Id,
                    Amount = request.Amount,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = "Bank Transfer",
                    InvoiceNumber = invoiceNumber
                });

                // Add Notification
                var packageName = user.InternetPackage?.PackageName ?? "Your plan";
                var expiryDate = user.PackageExpiryDate?.ToString("MMMM dd, yyyy") ?? "renewal date";
                
                _context.Notifications.Add(new Notification
                {
                    UserId = user.IdentityUserId,
                    Title = "✅ Payment Verified",
                    Message = $"Your payment of {request.Amount:C} has been confirmed! Your {packageName} service is now active until {expiryDate}. Thank you!",
                    Type = "Success",
                    Link = "/User/MyProfile"
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Payment approved and user service renewed.";
            return RedirectToAction(nameof(Verifications));
        }

        // POST: Payment/Reject/5
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.PaymentVerifications.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = "Rejected";
            request.ProcessedAt = DateTime.Now;
            
            // Notification
            var user = await _context.ISP_Users.FindAsync(request.ISP_userId);
            if (user != null && !string.IsNullOrEmpty(user.IdentityUserId))
            {
                var transactionRef = !string.IsNullOrEmpty(request.TransactionReference) 
                    ? $" (Reference: {request.TransactionReference})" 
                    : "";
                
                _context.Notifications.Add(new Notification
                {
                    UserId = user.IdentityUserId,
                    Title = "⚠️ Payment Verification Issue",
                    Message = $"Your payment verification{transactionRef} could not be confirmed. Please contact support for assistance or submit a new verification with correct details.",
                    Type = "Danger",
                    Link = "/Ticket/Create"
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Payment rejected.";
            return RedirectToAction(nameof(Verifications));
        }
    }
}
