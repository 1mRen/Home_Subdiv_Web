using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Home_Sbdv.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Home_Sbdv.Services.Implementations
{
    public class ContactDirectoryService : IContactDirectoryService
    {
        private readonly AppDbContext _context;
        public ContactDirectoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ContactDirectoryViewModel>> GetAllContactsAsync()
        {
            return await _context.ContactDirectory
                .Select(c => new ContactDirectoryViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Department = c.Department,
                    Phone = c.Phone,
                    Email = c.Email,
                    Description = c.Description,
                    PhotoUrl = c.PhotoUrl
                })
                .ToListAsync();
        }

        public async Task<ContactDirectoryViewModel?> GetContactByIdAsync(int id)
        {
            var c = await _context.ContactDirectory.FindAsync(id);
            if (c == null) return null;
            return new ContactDirectoryViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Department = c.Department,
                Phone = c.Phone,
                Email = c.Email,
                Description = c.Description,
                PhotoUrl = c.PhotoUrl
            };
        }

        public async Task<bool> CreateContactAsync(ContactDirectoryViewModel model)
        {
            var entity = new ContactDirectory
            {
                Name = model.Name,
                Department = model.Department,
                Phone = model.Phone,
                Email = model.Email,
                Description = model.Description
            };
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "ContactPhotos");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.PhotoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PhotoFile.CopyToAsync(stream);
                }
                entity.PhotoUrl = "/Uploads/ContactPhotos/" + uniqueFileName;
            }
            _context.ContactDirectory.Add(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateContactAsync(int id, ContactDirectoryViewModel model)
        {
            var entity = await _context.ContactDirectory.FindAsync(id);
            if (entity == null) return false;
            entity.Name = model.Name;
            entity.Department = model.Department;
            entity.Phone = model.Phone;
            entity.Email = model.Email;
            entity.Description = model.Description;
            if (model.PhotoFile != null && model.PhotoFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "ContactPhotos");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(model.PhotoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.PhotoFile.CopyToAsync(stream);
                }
                entity.PhotoUrl = "/Uploads/ContactPhotos/" + uniqueFileName;
            }
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteContactAsync(int id)
        {
            var entity = await _context.ContactDirectory.FindAsync(id);
            if (entity == null) return false;
            _context.ContactDirectory.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 