using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home_Sbdv.Data;
using Microsoft.EntityFrameworkCore;
using Home_Sbdv.Models;


namespace Home_Sbdv.Services
{
    public class VisitorPassService
    {
        private readonly AppDbContext _context;

        public VisitorPassService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VisitorPassRequestViewModel>> GetAllRequestsAsync()
        {
            var requests = await _context.VisitorPassRequests
                .Include(r => r.RequestedBy)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return requests.Select(r => new VisitorPassRequestViewModel
            {
                Id = r.Id,
                VisitorName = r.VisitorName,
                VisitorContact = r.VisitorContact,
                Purpose = r.Purpose,
                VisitDate = r.VisitDate,
                Status = r.Status,
                RequestedAt = r.RequestedAt,
                RequestedByUserId = r.RequestedByUserId,
                RequestedByUserName = r.RequestedBy?.Email ?? "Unknown",
                AuditTrail = r.AuditTrail
            });
        }
    }
} 