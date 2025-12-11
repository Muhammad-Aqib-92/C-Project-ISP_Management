using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Semester_Project.Models;
using Semester_Project.Services;
using System.Threading.Tasks;

namespace Semester_Project.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<myappuser> _userManager;

        public NotificationController(INotificationService notificationService, UserManager<myappuser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                await _notificationService.MarkAllAsReadAsync(userId);
            }
            return Ok();
        }
    }
}
