using Home_Sbdv.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Home_Sbdv.Services.Interfaces
{
    public interface IContactDirectoryService
    {
        Task<List<ContactDirectoryViewModel>> GetAllContactsAsync();
        Task<ContactDirectoryViewModel?> GetContactByIdAsync(int id);
        Task<bool> CreateContactAsync(ContactDirectoryViewModel model);
        Task<bool> UpdateContactAsync(int id, ContactDirectoryViewModel model);
        Task<bool> DeleteContactAsync(int id);
    }
} 