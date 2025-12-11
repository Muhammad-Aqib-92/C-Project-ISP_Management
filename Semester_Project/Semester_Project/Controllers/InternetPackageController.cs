using Microsoft.AspNetCore.Mvc;
using Semester_Project.Models;
using Semester_Project.Data; // Your DbContext namespace
using Microsoft.AspNetCore.Authorization;
using System.Linq;

[Authorize(Roles = "Admin")]
public class InternetPackageController : Controller
{
    private readonly ApplicationDbContext dbContext;

    public InternetPackageController(ApplicationDbContext context)
    {
        dbContext = context;
    }

    // List all packages
    public IActionResult Index()
    {
        var packages = dbContext.InternetPackages.ToList();
        return View(packages);
    }

    // GET: Create new package
    public IActionResult Create()
    {
        return View();
    }

    // POST: Save new package
    [HttpPost]
    public IActionResult Create(InternetPackage package)
    {
        if (ModelState.IsValid)
        {
            dbContext.InternetPackages.Add(package);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(package);
    }

    // GET: Edit package
    public IActionResult Edit(int id)
    {
        var package = dbContext.InternetPackages.Find(id);
        return View(package);
    }

    // POST: Update package
    [HttpPost]
    public IActionResult Edit(InternetPackage package)
    {
        if (ModelState.IsValid)
        {
            dbContext.InternetPackages.Update(package);
            dbContext.SaveChanges();
            return RedirectToAction("Index");
        }
        return View(package);
    }

    // GET: Confirm delete
    public IActionResult Delete(int id)
    {
        var package = dbContext.InternetPackages.Find(id);
        return View(package);
    }

    // POST: Actually delete
    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        // Validation: Check if package is in use
        bool isInUse = dbContext.ISP_Users.Any(u => u.InternetPackageId == id);
        if (isInUse)
        {
            var package = dbContext.InternetPackages.Find(id);
            TempData["ErrorMessage"] = "Cannot delete this package because it is currently assigned to one or more customers.";
            return View("Delete", package);
        }

        var packageToDelete = dbContext.InternetPackages.Find(id);
        if (packageToDelete != null)
        {
            dbContext.InternetPackages.Remove(packageToDelete);
            dbContext.SaveChanges();
            TempData["SuccessMessage"] = "Package deleted successfully.";
        }
        return RedirectToAction("Index");
    }
}
