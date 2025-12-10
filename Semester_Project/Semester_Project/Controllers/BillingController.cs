using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Semester_Project.Models;
using Semester_Project6.Models.Interface;
using System.Collections.Generic;

namespace Semester_Project6.Controllers
{
    [Authorize]
    [Authorize(Policy = "RequireManagerAccess")]
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
        public IActionResult Index()
        {
            List<ISP_user> Data = repo.Get();
            return View(Data);
        }

        // POST: Mark customer as paid
        [HttpPost]
        public IActionResult MarkAsPaid(int id)
        {
            repo.MarkAsPaid(id);

            // Create Payment History record
            var user = repo.GetUserById(id);
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
                return NotFound("No payment history found for this user.");
            }

            var user = repo.GetUserById(id);
            if (user == null) return NotFound();

            var pdfBytes = _invoiceService.GenerateInvoice(user, payment);
            return File(pdfBytes, "application/pdf", $"Invoice_{payment.InvoiceNumber}.pdf");
        }
    }
}
