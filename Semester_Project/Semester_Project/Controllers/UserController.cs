using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semester_Project.Data;
using Semester_Project.Models;
using Semester_Project6.Models.Interface;
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

        public UserController(UserManager<myappuser> userManager, ISPuserinterface repo, ApplicationDbContext context)
        {
            _userManager = userManager;
            _repo = repo;
            _context = context;
        }

        // GET: /User/MyProfile
        public async Task<IActionResult> MyProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            // Find the corresponding ISP_user record by email
            // Assuming ISP_user uses Email as a linking key or we need to find it.
            // The repo has GetUserById, but we only have Identity User here.
            // We need to match by Email.
            var allCustomers = _repo.Get();
            var customer = allCustomers.FirstOrDefault(c => c.Email == user.Email);

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

            var allCustomers = _repo.Get();
            var customer = allCustomers.FirstOrDefault(c => c.Email == user.Email);

            if (customer == null)
            {
                ViewData["Message"] = "No billing profile found.";
                return View();
            }

            // Fetch payment history
            var history = _context.PaymentHistories
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

            var allCustomers = _repo.Get();
            var customer = allCustomers.FirstOrDefault(c => c.Email == user.Email);
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
        public async Task<IActionResult> MakePayment(PaymentVerification model)
        {
            if (ModelState.IsValid)
            {
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
