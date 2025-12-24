using Microsoft.EntityFrameworkCore;
using Semester_Project.Data;
using Semester_Project.Models;
using Semester_Project.Models.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Semester_Project.Models.Repository
{
    public class Repository : ISPuserinterface
    {
        private readonly ApplicationDbContext dbContext;

        public Repository(ApplicationDbContext context)
        {
            dbContext = context;
        }

        // Handling status
        public void UpdateUserStatus(int userId, bool isActive)
        {
            var user = dbContext.ISP_Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                user.IsActive = isActive;

                // If user is marked inactive, set unpaid too
                if (!isActive)
                {
                    user.IsPaid = false;
                }

                dbContext.SaveChanges();
            }
        }

        public void MarkAsInactive(int id)
        {
            UpdateUserStatus(id, false);
        }

        public void MarkAsActive(int id)
        {
            UpdateUserStatus(id, true);
        }

        public void Add(ISP_user user)
        {
            user.IsActive = true;  // default value
            user.Price = user.Price == 0 ? 0 : user.Price;
            user.Cost = user.Cost == 0 ? 0 : user.Cost;

            dbContext.ISP_Users.Add(user);
            dbContext.SaveChanges();
        }

        // Dashboard work


        public List<ISP_user> Get()
        {
            try
            {
                return dbContext.ISP_Users
                                .AsNoTracking()
                                .Include(u => u.InternetPackage)
                                .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner: " + ex.InnerException.Message);
                throw;
            }
        }

        public List<ISP_user> Get(string? searchString, string? status = "")
        {
             var query = dbContext.ISP_Users.AsNoTracking().Include(u => u.InternetPackage).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(u => u.Name.ToLower().Contains(searchString) || 
                                         u.Email.ToLower().Contains(searchString));
            }

            if (!string.IsNullOrEmpty(status))
            {
                if (status.ToLower() == "paid")
                {
                    query = query.Where(u => u.IsPaid == true);
                }
                else if (status.ToLower() == "unpaid")
                {
                    query = query.Where(u => u.IsPaid != true); // Handle null or false
                }
            }

            return query.ToList();
        }

        public ISP_user? GetUserById(int id)
        {
            return dbContext.ISP_Users
                            .Include(u => u.InternetPackage)
                            .FirstOrDefault(u => u.Id == id);
        }

        public ISP_user? GetUserByEmail(string email)
        {
            return dbContext.ISP_Users
                            .AsNoTracking()
                            .Include(u => u.InternetPackage)
                            .FirstOrDefault(u => u.Email == email);
        }

        public ISP_user? GetUserByIdentityId(string identityId)
        {
            return dbContext.ISP_Users
                            .AsNoTracking()
                            .Include(u => u.InternetPackage)
                            .FirstOrDefault(u => u.IdentityUserId == identityId);
        }

        public bool UpdateCustomer(ISP_user updatedCustomer)
        {
            var existingUser = dbContext.ISP_Users.FirstOrDefault(u => u.Id == updatedCustomer.Id);
            if (existingUser != null)
            {
                existingUser.Name = updatedCustomer.Name;
                existingUser.Email = updatedCustomer.Email;
                existingUser.Phone = updatedCustomer.Phone;
                existingUser.Address = updatedCustomer.Address;
                existingUser.InternetPackageId = updatedCustomer.InternetPackageId;
                existingUser.IsPaid = updatedCustomer.IsPaid;
                existingUser.IsActive = updatedCustomer.IsActive;
                existingUser.Price = updatedCustomer.Price;
                existingUser.Cost = updatedCustomer.Cost;

                dbContext.SaveChanges();
                return true;
            }
            return false;
        }

        public bool UpdateUser(ISP_user updatedUser)
        {
            return UpdateCustomer(updatedUser);
        }

        public bool DeleteUser(int id)
        {
            var userToDelete = dbContext.ISP_Users.FirstOrDefault(u => u.Id == id);
            if (userToDelete != null)
            {
                dbContext.ISP_Users.Remove(userToDelete);
                dbContext.SaveChanges();
                return true;
            }
            return false;
        }

        public void MarkAsPaid(int id)
        {
            var user = dbContext.ISP_Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.IsPaid = true;
                dbContext.SaveChanges();
            }
        }

        public void MarkAsUnpaid(int id)
        {
            var user = dbContext.ISP_Users.FirstOrDefault(u => u.Id == id);
            if (user != null)
            {
                user.IsPaid = false;
                dbContext.SaveChanges();
            }
        }

        // Packages methods
        public List<InternetPackage> GetPackages()
        {
            try
            {
                return dbContext.InternetPackages.ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in GetPackages: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner: " + ex.InnerException.Message);
                throw;
            }
        }

        public InternetPackage? GetPackageById(int id)
        {
            return dbContext.InternetPackages.FirstOrDefault(p => p.Id == id);
        }
    }
}
