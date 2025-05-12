using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Home_Sbdv.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home_Sbdv.Services.Implementations
{
    public class VisitorPassService : IVisitorPassService
    {
        private readonly AppDbContext _context;
        public VisitorPassService(AppDbContext context) { _context = context; }

        public async Task<List<VisitorPassRequestViewModel>> GetAllAsync()
        {
            return await _context.VisitorPassRequests
                .Include(v => v.RequestedBy)
                .OrderByDescending(v => v.RequestedAt)
                .Select(v => new VisitorPassRequestViewModel
                {
                    Id = v.Id,
                    RequestedByUserId = v.RequestedByUserId,
                    RequestedByUserName = v.RequestedBy.Email,
                    VisitorName = v.VisitorName,
                    VisitorContact = v.VisitorContact,
                    Purpose = v.Purpose,
                    VisitDate = v.VisitDate,
                    Status = v.Status,
                    RequestedAt = v.RequestedAt,
                    ApprovedByUserId = v.ApprovedByUserId,
                    ApprovedAt = v.ApprovedAt,
                    AuditTrail = v.AuditTrail
                }).ToListAsync();
        }

        public async Task<List<VisitorPassRequestViewModel>> GetAllRequestsAsync()
        {
            return await GetAllAsync();
        }

        public async Task<List<VisitorPassRequestViewModel>> GetByUserAsync(int userId)
        {
            return await _context.VisitorPassRequests
                .Include(v => v.RequestedBy)
                .Where(v => v.RequestedByUserId == userId)
                .OrderByDescending(v => v.RequestedAt)
                .Select(v => new VisitorPassRequestViewModel
                {
                    Id = v.Id,
                    RequestedByUserId = v.RequestedByUserId,
                    RequestedByUserName = v.RequestedBy.Email,
                    VisitorName = v.VisitorName,
                    VisitorContact = v.VisitorContact,
                    Purpose = v.Purpose,
                    VisitDate = v.VisitDate,
                    Status = v.Status,
                    RequestedAt = v.RequestedAt,
                    ApprovedByUserId = v.ApprovedByUserId,
                    ApprovedAt = v.ApprovedAt,
                    AuditTrail = v.AuditTrail
                }).ToListAsync();
        }

        public async Task<VisitorPassRequestViewModel?> GetByIdAsync(int id)
        {
            var v = await _context.VisitorPassRequests
                .Include(v => v.RequestedBy)
                .FirstOrDefaultAsync(v => v.Id == id);
                
            if (v == null) return null;
            
            return new VisitorPassRequestViewModel
            {
                Id = v.Id,
                RequestedByUserId = v.RequestedByUserId,
                RequestedByUserName = v.RequestedBy.Email,
                VisitorName = v.VisitorName,
                VisitorContact = v.VisitorContact,
                Purpose = v.Purpose,
                VisitDate = v.VisitDate,
                Status = v.Status,
                RequestedAt = v.RequestedAt,
                ApprovedByUserId = v.ApprovedByUserId,
                ApprovedAt = v.ApprovedAt,
                AuditTrail = v.AuditTrail
            };
        }

       public async Task<bool> CreateAsync(VisitorPassRequestViewModel model)
{
    try
    {
        // Get the user's name or email
        var user = await _context.Users.FindAsync(model.RequestedByUserId);
        var userName = user != null
            ? (!string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email)
            : $"UserId {model.RequestedByUserId}";

        var entity = new VisitorPassRequest
        {
            RequestedByUserId = model.RequestedByUserId,
            VisitorName = model.VisitorName,
            VisitorContact = model.VisitorContact,
            Purpose = model.Purpose,
            VisitDate = model.VisitDate,
            Status = "Pending",
            RequestedAt = DateTime.UtcNow,
            AuditTrail = $"Created by {userName} at {DateTime.UtcNow:dd/MM/yyyy h:mm:ss tt}\n"
        };
        _context.VisitorPassRequests.Add(entity);
        var result = await _context.SaveChangesAsync();
        Console.WriteLine($"VisitorPassRequest saved. Rows affected: {result}");
        return result > 0;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving VisitorPassRequest: {ex.Message}\n{ex.StackTrace}");
        return false;
    }
}

        public async Task<bool> ApproveAsync(int id, int approverUserId)
        {
            var v = await _context.VisitorPassRequests.FindAsync(id);
            if (v == null || v.Status != "Pending") return false;
            v.Status = "Approved";
            v.ApprovedByUserId = approverUserId;
            v.ApprovedAt = DateTime.UtcNow;
            // Get approver's name or email
            var user = await _context.Users.FindAsync(approverUserId);
            var userName = user != null ? (!string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email) : $"UserId {approverUserId}";
            v.AuditTrail += $"Approved by {userName} at {DateTime.UtcNow:dd/MM/yyyy h:mm:ss tt}\n";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeclineAsync(int id, int approverUserId)
        {
            var v = await _context.VisitorPassRequests.FindAsync(id);
            if (v == null || v.Status != "Pending") return false;
            v.Status = "Declined";
            v.ApprovedByUserId = approverUserId;
            v.ApprovedAt = DateTime.UtcNow;
            // Get decliner's name or email
            var user = await _context.Users.FindAsync(approverUserId);
            var userName = user != null ? (!string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email) : $"UserId {approverUserId}";
            v.AuditTrail += $"Declined by {userName} at {DateTime.UtcNow:dd/MM/yyyy h:mm:ss tt}\n";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckInAsync(int id, int staffUserId)
        {
            var v = await _context.VisitorPassRequests.FindAsync(id);
            if (v == null || v.Status != "Approved") return false;
            v.Status = "CheckedIn";
            // Get staff user's name or email
            var user = await _context.Users.FindAsync(staffUserId);
            var userName = user != null ? (!string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email) : $"UserId {staffUserId}";
            v.AuditTrail += $"Checked in by {userName} at {DateTime.UtcNow:dd/MM/yyyy h:mm:ss tt}\n";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CheckOutAsync(int id, int staffUserId)
        {
            var v = await _context.VisitorPassRequests.FindAsync(id);
            if (v == null || v.Status != "CheckedIn") return false;
            v.Status = "CheckedOut";
            // Get staff user's name or email
            var user = await _context.Users.FindAsync(staffUserId);
            var userName = user != null ? (!string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email) : $"UserId {staffUserId}";
            v.AuditTrail += $"Checked out by {userName} at {DateTime.UtcNow:dd/MM/yyyy h:mm:ss tt}\n";
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelAsync(int id, int userId)
        {
            var v = await _context.VisitorPassRequests.FindAsync(id);
            if (v == null || v.RequestedByUserId != userId || v.Status != "Pending") return false;
            v.Status = "Cancelled";
            
            // Get user's name or email
            var user = await _context.Users.FindAsync(userId);
            var userName = user != null ? (!string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email) : $"UserId {userId}";
            v.AuditTrail += $"Cancelled by {userName} at {DateTime.UtcNow:dd/MM/yyyy h:mm:ss tt}\n";
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateAsync(VisitorPassRequestViewModel model)
        {
            try
            {
                var request = await _context.VisitorPassRequests.FindAsync(model.Id);
                if (request == null || request.Status != "Pending") return false;

                // Update the fields
                request.VisitorName = model.VisitorName;
                request.VisitorContact = model.VisitorContact;
                request.Purpose = model.Purpose;
                request.VisitDate = model.VisitDate;

                // Add to audit trail
                var user = await _context.Users.FindAsync(model.RequestedByUserId);
                var userName = user != null ? (!string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Email) : $"UserId {model.RequestedByUserId}";
                request.AuditTrail += $"Updated by {userName} at {DateTime.UtcNow:dd/MM/yyyy h:mm:ss tt}\n";

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating VisitorPassRequest: {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }
    }
}