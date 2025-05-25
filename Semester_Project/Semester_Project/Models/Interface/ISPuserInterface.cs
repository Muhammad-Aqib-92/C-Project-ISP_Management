using Semester_Project.Models;
using System.Collections.Generic;

namespace Semester_Project6.Models.Interface
{
    public interface ISPuserinterface
    {
        void Add(ISP_user user);              // Add method using ISP_user object
        List<ISP_user> Get();
        ISP_user? GetUserById(int id);
        bool UpdateCustomer(ISP_user updatedCustomer);
        bool UpdateUser(ISP_user updatedUser);
        bool DeleteUser(int id);
        void MarkAsPaid(int id);
        void MarkAsUnpaid(int id);
        public void UpdateUserStatus(int userId, bool isActive);

        List<InternetPackage> GetPackages();
        InternetPackage GetPackageById(int id);

        public DashboardViewModel GetDashboardStats();

    }
}
