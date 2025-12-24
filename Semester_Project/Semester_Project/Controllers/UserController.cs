using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semester_Project.Data;
using Semester_Project.Models;
using Semester_Project.Models.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace Semester_Project.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly UserManager<myappuser> _userManager;
        private readonly ISPuserinterface _repo;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserController(UserManager<myappuser> userManager, ISPuserinterface repo, ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _userManager = userManager;
            _repo = repo;
            _context = context;
            _environment = environment;
        }

        // GET: /User/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Optimized lookup
            var customer = _repo.GetUserByIdentityId(user.Id);
            
            // Fallback for legacy
            if (customer == null) customer = _repo.GetUserByEmail(user.Email);

            if (customer == null)
            {
                ViewData["Message"] = "No customer profile found linked to this account.";
                return View();
            }

            return View(customer);
        }

        // GET: /User/MyBill
        public async Task<IActionResult> MyBill()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = _repo.GetUserByIdentityId(user.Id);
            if (customer == null) customer = _repo.GetUserByEmail(user.Email);

            if (customer == null)
            {
                ViewData["Message"] = "No billing profile found.";
                return View();
            }

            // Fetch payment history
            var history = _context.PaymentHistories
                .AsNoTracking()
                .Where(p => p.UserId == customer.Id)
                .OrderByDescending(p => p.PaymentDate)
                .ToList();

            ViewBag.PaymentHistory = history;
            return View(customer);
        }

        // GET: /User/MakePayment
        public async Task<IActionResult> MakePayment()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var customer = _repo.GetUserByIdentityId(user.Id);
            if (customer == null) customer = _repo.GetUserByEmail(user.Email);
            if (customer == null) return NotFound();

            var settings = await _context.PaymentSettings.FirstOrDefaultAsync();
            ViewBag.PaymentSettings = settings;

            var model = new PaymentVerification
            {
                ISP_userId = customer.Id,
                Amount = customer.Price
            };

            return View(model);
        }

        // POST: /User/MakePayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakePayment(PaymentVerification model, IFormFile? receiptFile)
        {
            if (ModelState.IsValid)
            {
                // Handle File Upload
                if (receiptFile != null && receiptFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "receipts");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = $"{Guid.NewGuid()}_{receiptFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await receiptFile.CopyToAsync(fileStream);
                    }
                    model.ReceiptPath = "/uploads/receipts/" + uniqueFileName;
                }

                model.Status = "Pending";
                model.CreatedAt = DateTime.Now;
                _context.PaymentVerifications.Add(model);
                
                // Get customer details for notification
                var customer = await _context.ISP_Users.FindAsync(model.ISP_userId);
                
                // Notify All Admins
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                foreach (var admin in adminUsers)
                {
                    var transactionInfo = !string.IsNullOrEmpty(model.TransactionReference) 
                        ? $" - Reference: {model.TransactionReference}" 
                        : "";
                    
                    _context.Notifications.Add(new Notification
                    {
                        UserId = admin.Id,
                        Title = "💰 New Payment Verification",
                        Message = $"{customer?.Name ?? "Customer"} submitted a payment verification for {model.Amount:C}{transactionInfo}. Click to review.",
                        Type = "Info",
                        Link = "/Payment/Verifications"
                    });
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Payment verification requested successfully. You will be notified once admin reviews it.";
                return RedirectToAction(nameof(MyProfile));
            }
            // Reload settings if error
            var settings = await _context.PaymentSettings.FirstOrDefaultAsync();
            ViewBag.PaymentSettings = settings;
            return View(model);
        }
    }
}
