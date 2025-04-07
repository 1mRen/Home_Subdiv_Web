using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public class FacilityService : IFacilityService
    {
        private readonly AppDbContext _context;

        public FacilityService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<FacilityViewModel>> GetAllFacilitiesAsync()
        {
            return await _context.Facilities
                .Select(f => new FacilityViewModel
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    Description = f.Description,
                    Location = f.Location,
                    AvailabilityStatus = f.AvailabilityStatus,
                    UpdatedAt = f.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<FacilityViewModel> GetFacilityByIdAsync(int id)
        {
            return await _context.Facilities
                .Select(f => new FacilityViewModel
                {
                    FacilityId = f.FacilityId,
                    FacilityName = f.FacilityName,
                    Description = f.Description,
                    Location = f.Location,
                    AvailabilityStatus = f.AvailabilityStatus,
                    UpdatedAt = f.UpdatedAt
                })
                .FirstOrDefaultAsync(f => f.FacilityId == id);
        }

        public async Task<bool> CreateFacilityAsync(FacilityViewModel facilityModel, string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
            {
                return false;
            }

            var newFacility = new Facilities
            {
                FacilityName = facilityModel.FacilityName,
                Description = facilityModel.Description,
                Location = facilityModel.Location,
                AvailabilityStatus = facilityModel.AvailabilityStatus,
            };

            _context.Facilities.Add(newFacility);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateFacilityAsync(int id, FacilityViewModel updatedFacility)
        {
            var existingFacility = await _context.Facilities.FirstOrDefaultAsync(f => f.FacilityId == id);
            if (existingFacility == null)
            {
                return false;
            }

            existingFacility.FacilityName = updatedFacility.FacilityName;
            existingFacility.Description = updatedFacility.Description;
            existingFacility.Location = updatedFacility.Location;
            existingFacility.AvailabilityStatus = updatedFacility.AvailabilityStatus;
            existingFacility.UpdatedAt = DateTime.UtcNow;

            _context.Update(existingFacility);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFacilityAsync(int id)
        {
            var facilityItem = await _context.Facilities.FindAsync(id);
            if (facilityItem == null)
            {
                return false;
            }

            _context.Facilities.Remove(facilityItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public List<SelectListItem> GetAvailabilityStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Available", Text = "Available" },
                new SelectListItem { Value = "Maintenance", Text = "Maintenance" },
                new SelectListItem { Value = "Closed", Text = "Closed" }
            };
        }
    }
}