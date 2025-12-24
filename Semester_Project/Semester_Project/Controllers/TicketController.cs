using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semester_Project.Data;
using Semester_Project.Models;
using Semester_Project.Models.Interface;
using System.Linq;

namespace Semester_Project.Controllers
{
    [Authorize] // Allow both Admin and User
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISPuserinterface _repo;

        private readonly Microsoft.AspNetCore.Identity.UserManager<myappuser> _userManager;

        public TicketController(ApplicationDbContext context, ISPuserinterface repo, Microsoft.AspNetCore.Identity.UserManager<myappuser> userManager)
        {
            _context = context;
            _repo = repo;
            _userManager = userManager;
        }

        // GET: Ticket List
        public async System.Threading.Tasks.Task<IActionResult> Index()
        {
             var user = await _userManager.GetUserAsync(User);
             var tickets = _context.SupportTickets.Include(t => t.User).AsNoTracking().AsQueryable();

              if (!User.IsInRole("Admin"))
              {
                  // Optimized lookup
                  var userProfile = _repo.GetUserByIdentityId(user.Id);
                  
                  // Fallback to Email if legacy or not linked
                  if (userProfile == null)
                  {
                      userProfile = _repo.GetUserByEmail(user.Email);
                  }

                  if (userProfile != null)
                  {
                      tickets = tickets.Where(t => t.UserId == userProfile.Id);
                  }
                  else
                  {
                      tickets = tickets.Where(t => t.UserId == -1); // Show nothing if profile not found
                  }
             }

            return View(tickets.OrderByDescending(t => t.CreatedDate).ToList());
        }

        // GET: Create Ticket
        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.Users = _repo.Get();
            }
            return View();
        }

        // POST: Create Ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async System.Threading.Tasks.Task<IActionResult> Create(SupportTicket ticket)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            
             if (!User.IsInRole("Admin"))
            {
                // Optimized lookup
                var userProfile = _repo.GetUserByIdentityId(currentUser.Id);
                
                if (userProfile == null)
                {
                    userProfile = _repo.GetUserByEmail(currentUser.Email);
                }

                if (userProfile != null)
                {
                    ticket.UserId = userProfile.Id;
                    // Clear validation error for UserId since we just set it manually
                    ModelState.Remove("UserId"); 
                    ModelState.Remove("User"); // Fix: Navigation property validation error
                }
                else
                {
                    ModelState.AddModelError("", "Could not find your Customer Profile. Please contact Admin.");
                }
            }
            else 
            {
                 // Admin creating for someone else
                 if(ticket.UserId == null)
                 {
                      ModelState.AddModelError("UserId", "Please select a user.");
                 }
            }

            if (ModelState.IsValid)
            {
                ticket.Status = TicketStatus.Open; 
                ticket.CreatedDate = System.DateTime.Now;
                _context.SupportTickets.Add(ticket);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ticket created successfully!";
                return RedirectToAction(nameof(Index));
            }
            
            // Debugging: Log errors to Console
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    System.Console.WriteLine($"Error in {state.Key}: {error.ErrorMessage}");
                }
            }

            if (User.IsInRole("Admin"))
            {
                ViewBag.Users = _repo.Get();
            }
            return View(ticket);
        }

        // POST: Resolve Ticket (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult MarkResolved(int id, string remarks)
        {
            var ticket = _context.SupportTickets.Find(id);
            if (ticket != null)
            {
                ticket.Status = TicketStatus.Resolved;
                ticket.Remarks = remarks; // Save remarks
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Ticket marked as Resolved with remarks.";
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var ticket = _context.SupportTickets.Find(id);
            if (ticket != null)
            {
                _context.SupportTickets.Remove(ticket);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Ticket deleted.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
