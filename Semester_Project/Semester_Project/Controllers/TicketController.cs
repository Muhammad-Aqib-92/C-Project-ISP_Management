using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semester_Project.Data;
using Semester_Project.Models;
using Semester_Project6.Models.Interface;
using System.Linq;

namespace Semester_Project.Controllers
{
    [Authorize]
    public class TicketController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISPuserinterface _repo;

        public TicketController(ApplicationDbContext context, ISPuserinterface repo)
        {
            _context = context;
            _repo = repo;
        }

        // GET: Ticket List (Admin sees all, User sees theirs - logic can be improved later)
        public IActionResult Index()
        {
            var tickets = _context.SupportTickets.Include(t => t.User).OrderByDescending(t => t.CreatedDate).ToList();
            return View(tickets);
        }

        // GET: Create Ticket
        public IActionResult Create()
        {
            ViewBag.Users = _repo.Get();
            return View();
        }

        // POST: Create Ticket
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SupportTicket ticket)
        {
            if (ModelState.IsValid)
            {
                _context.SupportTickets.Add(ticket);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Users = _repo.Get();
            return View(ticket);
        }

        // POST: Resolve Ticket (Admin only)
        [HttpPost]
        public IActionResult MarkResolved(int id)
        {
            var ticket = _context.SupportTickets.Find(id);
            if (ticket != null)
            {
                ticket.Status = TicketStatus.Resolved;
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var ticket = _context.SupportTickets.Find(id);
            if (ticket != null)
            {
                _context.SupportTickets.Remove(ticket);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
