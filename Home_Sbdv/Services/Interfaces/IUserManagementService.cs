// IUserManagementService.cs (Updated)
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public interface IUserManagementService
    {
        Task<List<Users>> GetAllUsersAsync();
        Task<Users> GetUserByIdAsync(int id);
        Task<bool> CreateUserAsync(Users newUser);
        Task<bool> UpdateUserAsync(int id, Users updatedUser);
        Task<bool> DeleteUserAsync(int id);
        // View model support
        Task<EditUserViewModel> GetUserForEditingAsync(int id);
        Task<bool> UpdateUserFromViewModelAsync(EditUserViewModel model);
        // Dashboard methods
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetUnverifiedUsersCountAsync();
        Task<List<Users>> GetRecentlyRegisteredUsersAsync(int count);
        Task<List<Users>> GetUsersByRoleAsync(string role);
        // Role-based user lists
        Task<List<Users>> GetStaffListAsync();
        Task<List<Users>> GetHomeownerListAsync();
        Task<List<Users>> GetAdminListAsync();

        // Role-based selection for dropdowns
        Task<List<SelectListItem>> GetStaffSelectListAsync();
        Task<List<SelectListItem>> GetHomeownerSelectListAsync();
        Task<List<SelectListItem>> GetAdminSelectListAsync();
    }
}