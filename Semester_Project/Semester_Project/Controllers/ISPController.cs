using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Semester_Project.Models;
using Semester_Project.Models.Interface;
using System.Collections.Generic;
using System;

namespace Semester_Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ISPController : Controller
    {
        private readonly ISPuserinterface repo;
        private readonly Semester_Project.Services.DashboardService _dashboardService;
        private readonly Microsoft.AspNetCore.Identity.UserManager<myappuser> _userManager;
        private readonly Semester_Project.Data.ApplicationDbContext _context;

        public ISPController(ISPuserinterface repo, Semester_Project.Services.DashboardService dashboardService, Microsoft.AspNetCore.Identity.UserManager<myappuser> userManager, Semester_Project.Data.ApplicationDbContext context)
        {
            this.repo = repo;
            _dashboardService = dashboardService;
            _userManager = userManager;
            _context = context;
        }

        // GET: ISP
        // GET: ISP
        public IActionResult Index(string? searchString, string? status)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status; // Keep track of filter
            List<ISP_user> Data = repo.Get(searchString, status);
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
            ViewBag.Packages = repo.GetPackages();
            return View(user);
        }

        // POST: Handle the update
        [HttpPost]
        public IActionResult EditCustomer(ISP_user updatedUser)
        {
             if (ModelState.IsValid)
            {
                // Update Price Logic based on Package
                if (updatedUser.InternetPackageId != null)
                {
                    var selectedPackage = repo.GetPackageById(updatedUser.InternetPackageId.Value);
                    if (selectedPackage != null)
                    {
                        updatedUser.Price = selectedPackage.Price;
                    }
                }

                // Check previous payment status
                bool wasUnpaid = false;
                var oldUser = repo.GetUserById(updatedUser.Id);
                // Note: repo.GetUserById returning the tracked entity might complicate things if we are not careful,
                // but since repo.UpdateUser handles its own retrieval and update properties, we just need to know the state.
                // However, since we are in the same scope, repo.GetUserById likely attaches the entity. 
                // Let's detach it or just check the boolean before UpdateUser overwrites values?
                // Actually repo.UpdateUser implementation fetches the user again. EF Core might track the same instance.
                // If 'oldUser' IS the internal tracked entity, and 'updatedUser' is the separate model binder object...
                // Inspection: repo.GetUserById does `dbContext.ISP_Users.FirstOrDefault(u => u.Id == id)`.
                // It returns a tracked entity.
                
                if (oldUser != null)
                {
                    if (oldUser.IsPaid != true) // was false or null
                    {
                        wasUnpaid = true;
                    }
                    // Detach to avoid conflict in Repo? 
                    // Repo.UpdateUser does: var existingUser = dbContext.ISP_Users.FirstOrDefault...
                    // If we already loaded it here, Repo will get the SAME instance.
                    // Repo then overwrites properties from 'updatedUser'.
                    // This is fine.
                }

                bool isUpdated = repo.UpdateUser(updatedUser);
                
                if (isUpdated)
                {
                    // Payment History Logic
                    if (wasUnpaid && updatedUser.IsPaid == true)
                    {
                         try
                         {
                            // 30 Days Expiry Logic
                            updatedUser.PackageExpiryDate = DateTime.Now.AddDays(30);
                            repo.UpdateUser(updatedUser); // Update again with date

                            var payment = new PaymentHistory
                            {
                                UserId = updatedUser.Id,
                                Amount = updatedUser.Price,
                                PaymentDate = DateTime.Now,
                                InvoiceNumber = $"INV-{DateTime.Now:yyyyMMdd}-{updatedUser.Id}-MN" // MN for Manual
                            };
                            _context.PaymentHistories.Add(payment);
                            _context.SaveChanges();
                         }
                         catch (System.Exception ex)
                         {
                             // Log error but don't stop the flow
                             System.Console.WriteLine($"Error recording manual payment: {ex.Message}");
                         }
                    }

                    TempData["SuccessMessage"] = "Customer details updated successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Packages = repo.GetPackages();
                    return View(updatedUser);
                }
            }
            ViewBag.Packages = repo.GetPackages();
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
        // POST: Confirm delete
        // POST: Confirm delete
        [HttpPost, ActionName("Delete")]
        public async System.Threading.Tasks.Task<IActionResult> DeleteConfirmed(int id)
        {
            var userProfile = repo.GetUserById(id);
            if (userProfile != null)
            {
                // Find Identity User
                myappuser identityUser = null;
                
                // 1. Try by IdentityUserId (New Standard)
                if (!string.IsNullOrEmpty(userProfile.IdentityUserId))
                {
                    identityUser = await _userManager.FindByIdAsync(userProfile.IdentityUserId);
                }

                // 2. Fallback to Email (Legacy Users)
                if (identityUser == null && !string.IsNullOrEmpty(userProfile.Email))
                {
                    identityUser = await _userManager.FindByEmailAsync(userProfile.Email);
                }

                if (identityUser != null)
                {
                    await _userManager.DeleteAsync(identityUser);
                }

                bool isDeleted = repo.DeleteUser(id);
                if (isDeleted)
                {
                    TempData["SuccessMessage"] = "Customer deleted successfully.";
                    return RedirectToAction("Index");
                }
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
        // POST: Add User - add user with selected package price
        // POST: Add User - add user with selected package price
        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> AddUser(Semester_Project.ViewModels.UserProvisioningViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Create Identity User
                var identityUser = new myappuser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true,
                    city = model.Address ?? "Unknown", 
                    state = "NA"
                };

                var result = await _userManager.CreateAsync(identityUser, model.Password);

                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(identityUser, "User");
                    if (!roleResult.Succeeded)
                    {
                        await _userManager.DeleteAsync(identityUser); // Cleanup
                        ModelState.AddModelError("", "Failed to assign user role.");
                         return View(model);
                    }

                    try 
                    {
                        // 2. Create Profile Data
                         var newUserProfile = new ISP_user
                        {
                            Name = model.Name,
                            Email = model.Email,
                            Phone = model.Phone,
                            Address = model.Address,
                            InternetPackageId = model.InternetPackageId,
                            IdentityUserId = identityUser.Id, // Link!
                            IsActive = true
                        };

                        if (model.InternetPackageId != null)
                        {
                            var selectedPackage = repo.GetPackageById(model.InternetPackageId.Value);
                            if (selectedPackage != null)
                            {
                                newUserProfile.Price = selectedPackage.Price;
                            }
                        }
                        
                        // New User - Start with active/paid? 
                        // If logic implies 'Add User' starts as unpaid, we leave it. 
                        // If we want them to be valid immediately:
                        // newUserProfile.PackageExpiryDate = DateTime.Now.AddDays(30);

                        repo.Add(newUserProfile);
                        TempData["SuccessMessage"] = "Customer created successfully and assigned 'User' role!";
                        return RedirectToAction("Index");
                    }
                    catch (System.Exception ex)
                    {
                        // Rollback: Delete the created Identity User if profile creation fails
                        await _userManager.DeleteAsync(identityUser);
                        System.Console.WriteLine($"Transaction Failed: {ex.Message}");
                        if (ex.InnerException != null) System.Console.WriteLine($"Inner: {ex.InnerException.Message}");
                        
                        ModelState.AddModelError("", $"Failed to create profile: {ex.Message}");
                    }
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            ViewBag.Packages = repo.GetPackages();
            return View(model);
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
        public async System.Threading.Tasks.Task<IActionResult> Dashboard()
        {
            await EnsureRolesConsistent();
            var viewModel = _dashboardService.GetDashboardViewModel();
            return View(viewModel);
        }

        private async System.Threading.Tasks.Task EnsureRolesConsistent()
        {
            // Only fetch profiles that might need linking or role assignment
            var profilesToSync = _context.ISP_Users
                .Where(u => string.IsNullOrEmpty(u.IdentityUserId) || u.IdentityUserId != null) 
                .ToList();

            foreach (var profile in profilesToSync)
            {
                myappuser? user = null;
                if (!string.IsNullOrEmpty(profile.IdentityUserId))
                {
                    user = await _userManager.FindByIdAsync(profile.IdentityUserId);
                }
                else if (!string.IsNullOrEmpty(profile.Email))
                {
                    user = await _userManager.FindByEmailAsync(profile.Email);
                    if (user != null)
                    {
                        profile.IdentityUserId = user.Id;
                        _context.ISP_Users.Update(profile);
                    }
                }

                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (!roles.Contains("User") && !roles.Contains("Admin"))
                    {
                        await _userManager.AddToRoleAsync(user, "User");
                    }
                }
            }
            
            if (_context.ChangeTracker.HasChanges())
            {
                await _context.SaveChangesAsync();
            }
        }









        // GET: Reset Password
        [HttpGet]
        public IActionResult ResetPassword(int id)
        {
            var user = repo.GetUserById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: Reset Password
        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> ResetPasswordConfirmed(int id)
        {
            var userProfile = repo.GetUserById(id);
            if (userProfile != null)
            {
                var identityUser = await _userManager.FindByEmailAsync(userProfile.Email);
                if (identityUser != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);
                    var result = await _userManager.ResetPasswordAsync(identityUser, token, "User@123");
                    if (result.Succeeded)
                    {
                         TempData["SuccessMessage"] = $"Password for {userProfile.Name} has been reset to 'User@123'.";
                         return RedirectToAction("Index");
                    }
                }
            }
            return RedirectToAction("Index");
        }

    }
}
