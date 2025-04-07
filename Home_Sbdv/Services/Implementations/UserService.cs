using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Users>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<Users> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task<bool> CreateUserAsync(Users newUser)
        {
            try
            {
                // Hash the password
                newUser.Password = BCrypt.Net.BCrypt.HashPassword(newUser.Password);
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(int id, Users updatedUser)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return false;
                }
                // Update only necessary fields
                user.FirstName = updatedUser.FirstName;
                user.LastName = updatedUser.LastName;
                user.Role = updatedUser.Role;
                user.Address = updatedUser.Address;
                user.Gender = updatedUser.Gender;
                user.OwnershipStatus = updatedUser.OwnershipStatus;
                user.ContactNumber = updatedUser.ContactNumber;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return false;
                }
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // New methods for view model support
        public async Task<EditUserViewModel> GetUserForEditingAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
                return null;

            return new EditUserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                ContactNumber = user.ContactNumber,
                Role = user.Role,
                Address = user.Address,
                Gender = user.Gender,
                OwnershipStatus = user.OwnershipStatus
            };
        }

        public async Task<bool> UpdateUserFromViewModelAsync(EditUserViewModel model)
        {
            try
            {
                var user = await _context.Users.FindAsync(model.Id);

                if (user == null)
                    return false;

                // Update fields from view model
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.ContactNumber = model.ContactNumber;
                user.Role = model.Role;
                user.Address = model.Address;
                user.Gender = model.Gender;
                user.OwnershipStatus = model.OwnershipStatus;

                // Email and Username are readonly in the form, so we don't update them

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}