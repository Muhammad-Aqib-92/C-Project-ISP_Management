using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Semester_Project.Models;
using Semester_Project6.Models.Interface;
using System.Collections.Generic;

namespace Semester_Project6.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class BillingController : Controller
    {
        private readonly ISPuserinterface repo;
        private readonly Semester_Project.Services.InvoiceService _invoiceService;
        private readonly Semester_Project.Data.ApplicationDbContext _context;

        public BillingController(ISPuserinterface repo, Semester_Project.Services.InvoiceService invoiceService, Semester_Project.Data.ApplicationDbContext context)
        {
            this.repo = repo;
            _invoiceService = invoiceService;
            _context = context;
        }

        // Billing Page
        public IActionResult Index(int? month, int? year)
        {
            EnsurePaymentDataConsistent(); // Run check automatically

            List<ISP_user> data = repo.Get();


            if (month.HasValue && year.HasValue)
            {
                // Filter by Payment History? Or by Expiry?
                // Request says "month wise filter functionality". Usually for Billing this means "Who paid in this month" or "Who is due".
                // Let's filter by "Users who have a PaymentHistory in this month/year".
                var userIdsPaidInMonth = _context.PaymentHistories
                    .Where(p => p.PaymentDate.Month == month.Value && p.PaymentDate.Year == year.Value)
                    .Select(p => p.UserId)
                    .Distinct()
                    .ToList();
                
                 data = data.Where(u => userIdsPaidInMonth.Contains(u.Id)).ToList();
                 ViewBag.FilterMonth = month;
                 ViewBag.FilterYear = year;
            }

            return View(data);
        }

        // POST: Mark customer as paid
        [HttpPost]
        public IActionResult MarkAsPaid(int id)
        {
            repo.MarkAsPaid(id);

             // Set 30-day Expiry
            var user = repo.GetUserById(id);
            if (user != null)
            {
                user.PackageExpiryDate = System.DateTime.Now.AddDays(30);
                repo.UpdateUser(user);
            }

            // Create Payment History record
            if (user != null && user.InternetPackage != null)
            {
                var payment = new PaymentHistory
                {
                    UserId = user.Id,
                    Amount = user.Price, // Assuming Price is the monthly amount
                    PaymentDate = System.DateTime.Now,
                    InvoiceNumber = $"INV-{System.DateTime.Now:yyyyMMdd}-{user.Id}"
                };
                _context.PaymentHistories.Add(payment);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // Mark Customer as unpaid
        [HttpPost]
        public IActionResult MarkAsUnpaid(int id)
        {
            repo.MarkAsUnpaid(id);
            return RedirectToAction("Index");
        }

        public IActionResult DownloadInvoice(int id)
        {
            // Find latest payment for this user
            var payment = _context.PaymentHistories
                .Where(p => p.UserId == id)
                .OrderByDescending(p => p.PaymentDate)
                .FirstOrDefault();

            if (payment == null)
            {
                var userCheck = repo.GetUserById(id);
                if (userCheck != null && userCheck.IsPaid == true && userCheck.InternetPackage != null)
                {
                    // Fallback: Generate a temporary payment record for display
                    payment = new PaymentHistory
                    {
                        UserId = userCheck.Id,
                        Amount = userCheck.Price,
                        PaymentDate = System.DateTime.Now, 
                        InvoiceNumber = $"INV-Generated-{userCheck.Id}"
                    };
                }
                else
                {
                    return NotFound("No payment history found for this user. Please mark the user as 'Paid' to generate an invoice.");
                }
            }

            var user = repo.GetUserById(id);
            if (user == null) return NotFound();

            var pdfBytes = _invoiceService.GenerateInvoice(user, payment);
            return File(pdfBytes, "application/pdf", $"Invoice_{payment.InvoiceNumber}.pdf");
        }


        private void EnsurePaymentDataConsistent()
        {
            try
            {
                var paidUsers = repo.Get().Where(u => u.IsPaid == true).ToList();
                int fixedCount = 0;
                int updatedCount = 0;

                foreach (var user in paidUsers)
                {
                    var history = _context.PaymentHistories.FirstOrDefault(p => p.UserId == user.Id);
                    
                    if (history == null)
                    {
                        var payment = new PaymentHistory
                        {
                            UserId = user.Id,
                            Amount = user.Price > 0 ? user.Price : (user.InternetPackage?.Price ?? 0),
                            PaymentDate = System.DateTime.Now,
                            InvoiceNumber = $"INV-{System.DateTime.Now:yyyyMMdd}-{user.Id}-FIX"
                        };
                        _context.PaymentHistories.Add(payment);
                        fixedCount++;
                    }
                    else if (history.Amount == 0 && user.Price > 0)
                    {
                        history.Amount = user.Price;
                        history.PaymentDate = System.DateTime.Now; 
                        _context.PaymentHistories.Update(history);
                        updatedCount++;
                    }
                }

                if (fixedCount > 0 || updatedCount > 0)
                {
                    _context.SaveChanges();
                }
            }
            catch (System.Exception) 
            {
                // fail silently
            }
        }
}
}
