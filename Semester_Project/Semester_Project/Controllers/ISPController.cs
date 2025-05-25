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

        public ISPController(ISPuserinterface repo)
        {
            this.repo = repo;
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
            return RedirectToAction("UserList");
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
                    return RedirectToAction("Customers");
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
                return RedirectToAction("Customers");
            }
            return NotFound();
        }

        // Customers list, authorized for "UserOnly" policy
        [Authorize(Policy = "UserOnly")]
        public IActionResult Customers()
        {
            List<ISP_user> Data = repo.Get();
            return View(Data);
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
                return RedirectToAction("Customers");
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

        [Authorize(Policy = "AdminOnly")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult infouser()
        {
            return View();
        }

        // Billing Page
        public IActionResult Billing()
        {
            List<ISP_user> Data = repo.Get();
            return View(Data);
        }

        // POST: Mark customer as paid
        [HttpPost]
        public IActionResult MarkAsPaid(int id)
        {
            repo.MarkAsPaid(id);
            return RedirectToAction("Billing");
        }

        // Mark Customer as unpaid
        [HttpPost]
        public IActionResult MarkAsUnpaid(int id)
        {
            repo.MarkAsUnpaid(id);
            return RedirectToAction("Billing");
        }

        // Dashboard Dynamic
        public IActionResult Dashboard()
        {
            var model = repo.GetDashboardStats();
            return View(model);
        }

        //Reports
        [Authorize]
        public IActionResult Reports()
        {
            var users = repo.Get(); // Get all users with their package details
            return View(users);     // Pass this list to the view
        }







        //// Reports Page
        //public IActionResult Reports()
        //{
        //    return View();
        //}
    }
}
