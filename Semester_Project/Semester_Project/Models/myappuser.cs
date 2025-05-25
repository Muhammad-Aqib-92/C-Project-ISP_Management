using Microsoft.AspNetCore.Identity;

namespace Semester_Project.Models
{
    public class myappuser :IdentityUser
    {
        public string city { get; set; }

        public string state { get; set; }
    }
}
