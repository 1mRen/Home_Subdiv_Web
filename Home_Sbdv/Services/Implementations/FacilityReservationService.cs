using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public class FacilityReservationService : IFacilityReservationService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<FacilityReservationService> _logger;

        public FacilityReservationService(AppDbContext context, ILogger<FacilityReservationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<FacilityReservationViewModel>> GetAllReservationsAsync()
        {
            var reservations = await _context.FacilityReservations
                .Include(r => r.User)
                .Include(r => r.Facility)
                .ToListAsync();

            return reservations.Select(ProjectToViewModel).ToList();
        }

        public async Task<FacilityReservationViewModel> GetReservationByIdAsync(int id)
        {
            var reservation = await _context.FacilityReservations
                .Include(r => r.User)
                .Include(r => r.Facility)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            return reservation != null ? ProjectToViewModel(reservation) : null;
        }

        public async Task<(bool Success, string ErrorMessage)> CreateReservationAsync(FacilityReservationViewModel model, int userId)
        {
            // Check for reservation conflicts
            var conflict = await CheckForConflict(model.FacilityId, model.ReservationDate,
                model.StartTime, model.EndTime, null);

            if (conflict != null)
            {
                return (false, $"Conflict detected: This facility is already reserved from {conflict.StartTime:hh\\:mm} to {conflict.EndTime:hh\\:mm}.");
            }

            var reservation = new FacilityReservation
            {
                FacilityId = model.FacilityId,
                ReservationDate = model.ReservationDate,
                StartTime = model.StartTime,
                EndTime = model.EndTime,
                Status = "Pending",
                UserId = userId
            };

            _context.Add(reservation);
            await _context.SaveChangesAsync();
            return (true, null);
        }

        public async Task<(bool Success, string ErrorMessage)> UpdateReservationAsync(int id, FacilityReservationViewModel model, int userId, bool isAdmin)
        {
            var reservation = await _context.FacilityReservations.FindAsync(id);
            if (reservation == null)
            {
                return (false, "Reservation not found.");
            }

            if (!isAdmin && reservation.UserId != userId)
            {
                return (false, "You do not have permission to edit this reservation.");
            }

            // Check for reservation conflicts
            var conflict = await CheckForConflict(model.FacilityId, model.ReservationDate,
                model.StartTime, model.EndTime, id);

            if (conflict != null)
            {
                return (false, $"Conflict detected: This facility is already reserved from {conflict.StartTime:hh\\:mm} to {conflict.EndTime:hh\\:mm}.");
            }

            reservation.FacilityId = model.FacilityId;
            reservation.ReservationDate = model.ReservationDate;
            reservation.StartTime = model.StartTime;
            reservation.EndTime = model.EndTime;
            reservation.Status = model.Status;

            try
            {
                _context.Update(reservation);
                await _context.SaveChangesAsync();
                return (true, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                return (false, "Unable to save changes. The reservation was modified by another user.");
            }
        }

        public async Task<bool> UpdateReservationStatusAsync(int id, string status)
        {
            var reservation = await _context.FacilityReservations.FindAsync(id);
            if (reservation == null)
            {
                return false;
            }

            reservation.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReservationAsync(int id, int userId, bool isAdmin)
        {
            var reservation = await _context.FacilityReservations.FindAsync(id);
            if (reservation == null)
            {
                return false;
            }

            if (!isAdmin && reservation.UserId != userId)
            {
                return false;
            }

            _context.FacilityReservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanUserModifyReservation(int reservationId, int userId, bool isAdmin)
        {
            if (isAdmin)
            {
                return true;
            }

            var reservation = await _context.FacilityReservations.FindAsync(reservationId);
            return reservation != null && reservation.UserId == userId;
        }

        // New methods for dashboard
        public async Task<List<FacilityReservationViewModel>> GetRecentReservationsAsync(int count)
        {
            var reservations = await _context.FacilityReservations
                .Include(r => r.User)
                .Include(r => r.Facility)
                .OrderByDescending(r => r.ReservationDate)
                .ThenByDescending(r => r.StartTime)
                .Take(count)
                .ToListAsync();

            return reservations.Select(ProjectToViewModel).ToList();
        }

        public async Task<List<FacilityReservationViewModel>> GetUserReservationsAsync(int userId)
        {
            var reservations = await _context.FacilityReservations
                .Include(r => r.Facility)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ThenByDescending(r => r.StartTime)
                .ToListAsync();

            return reservations.Select(ProjectToViewModel).ToList();
        }

        private async Task<FacilityReservation> CheckForConflict(int facilityId, DateTime reservationDate,
            TimeSpan startTime, TimeSpan endTime, int? excludeReservationId)
        {
            return await _context.FacilityReservations
                .FirstOrDefaultAsync(r =>
                    r.FacilityId == facilityId &&
                    r.ReservationDate.Date == reservationDate.Date &&
                    startTime < r.EndTime && endTime > r.StartTime &&
                    (excludeReservationId == null || r.ReservationId != excludeReservationId));
        }

        private FacilityReservationViewModel ProjectToViewModel(FacilityReservation reservation)
        {
            return new FacilityReservationViewModel
            {
                ReservationId = reservation.ReservationId,
                FacilityId = reservation.FacilityId,
                FacilityName = reservation.Facility?.FacilityName ?? "Unknown",
                ReservationDate = reservation.ReservationDate,
                StartTime = reservation.StartTime,
                EndTime = reservation.EndTime,
                Status = reservation.Status,
                CreatedBy = reservation.User?.Id ?? 0,
                CreatedByName = reservation.User?.FullName ?? "Unknown"
            };
        }
    }
}