using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Semester_Project.Models;
using Semester_Project6.Models.Interface;
using System.Collections.Generic;

namespace Semester_Project6.Controllers
{
    [Authorize]
    public class ISPController : Controller
    {
        private readonly ISPuserinterface repo;
        private readonly Semester_Project.Services.DashboardService _dashboardService;

        public ISPController(ISPuserinterface repo, Semester_Project.Services.DashboardService dashboardService)
        {
            this.repo = repo;
            _dashboardService = dashboardService;
        }

        // GET: ISP
        [Authorize(Policy = "RequireTechAccess")]
        public IActionResult Index(string searchString)
        {
            ViewData["CurrentFilter"] = searchString;
            List<ISP_user> Data = repo.Get(searchString);
            return View("Customers", Data);
        }

        // Chat Page
        public IActionResult Chat()
        {
            return View();
        }

        // Status handling
        [HttpPost]
        public IActionResult ChangeStatus(int id, bool isActive)
        {
            repo.UpdateUserStatus(id, isActive);
            return RedirectToAction("Index");
        }

        // GET: Show the Edit form
        [HttpGet]
        public IActionResult EditCustomer(int id)
        {
            var user = repo.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Handle the update
        [HttpPost]
        public IActionResult EditCustomer(ISP_user updatedUser)
        {
            if (ModelState.IsValid)
            {
                bool isUpdated = repo.UpdateUser(updatedUser);
                if (isUpdated)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(updatedUser);
                }
            }
            return View(updatedUser);
        }

        // GET: Delete confirmation
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = repo.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: Confirm delete
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            bool isDeleted = repo.DeleteUser(id);
            if (isDeleted)
            {
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        // GET: Add User - show form with packages list
        [HttpGet]
        public IActionResult AddUser()
        {
            ViewBag.Packages = repo.GetPackages();
            return View();
        }

        // POST: Add User - add user with selected package price
        [HttpPost]
        public IActionResult AddUser(ISP_user user)
        {
            if (ModelState.IsValid)
            {
                if (user.InternetPackageId != null)
                {
                    var selectedPackage = repo.GetPackageById(user.InternetPackageId.Value);
                    if (selectedPackage != null)
                    {
                        user.Price = selectedPackage.Price;
                    }
                }
                repo.Add(user);
                return RedirectToAction("Index");
            }

            ViewBag.Packages = repo.GetPackages();
            return View(user);
        }

        // Login Page
        public IActionResult Login()
        {
            return View();
        }

        // Some info pages
        public IActionResult info()
        {
            return View();
        }

        public IActionResult infouser()
        {
            return View();
        }

        // Dashboard
        [Authorize] // Accessible to all authenticated users, view filters content
        public IActionResult Dashboard()
        {
            var viewModel = _dashboardService.GetDashboardViewModel();
            return View(viewModel);
        }









        //// Reports Page
        //public IActionResult Reports()
        //{
        //    return View();
        //}
    }
}
