using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Semester_Project6.Models.Interface;
using System.Collections.Generic;

namespace Semester_Project6.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly ISPuserinterface repo;

        public ReportController(ISPuserinterface repo)
        {
            this.repo = repo;
        }

        public IActionResult Index()
        {
            var users = repo.Get(); // Get all users with their package details
            return View(users);     // Pass this list to the view
        }
    }
}
