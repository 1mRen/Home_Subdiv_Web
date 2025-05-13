using Home_Sbdv.Data;
using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Home_Sbdv.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home_Sbdv.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public FeedbackService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // Existing methods remain unchanged...
        public async Task<List<FeedbackViewModel>> GetAllFeedbackAsync()
        {
            return await _context.Feedbacks
                .Include(f => f.SubmittedBy)
                .Include(f => f.AssignedTo)
                .Select(f => new FeedbackViewModel
                {
                    Id = f.Id,
                    Title = f.Title,
                    Description = f.Description,
                    Type = f.Type,
                    Status = f.Status,
                    StaffResponse = f.StaffResponse,
                    AssignedToId = f.AssignedToId,
                    SubmittedById = f.SubmittedById,
                    SubmittedByName = f.SubmittedBy.FullName,
                    AssignedToName = f.AssignedTo != null ? f.AssignedTo.FullName : null,
                    AttachmentPath = f.AttachmentPath,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt,
                    SubmittedBy = f.SubmittedBy,
                    AssignedTo = f.AssignedTo
                })
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<FeedbackViewModel> GetFeedbackByIdAsync(int id)
        {
            var feedback = await _context.Feedbacks
                .Include(f => f.SubmittedBy)
                .Include(f => f.AssignedTo)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (feedback == null) return null;

            return new FeedbackViewModel
            {
                Id = feedback.Id,
                Title = feedback.Title,
                Description = feedback.Description,
                Type = feedback.Type,
                Status = feedback.Status,
                StaffResponse = feedback.StaffResponse,
                AssignedToId = feedback.AssignedToId,
                SubmittedById = feedback.SubmittedById,
                SubmittedByName = feedback.SubmittedBy != null ? feedback.SubmittedBy.FullName : null,
                AssignedToName = feedback.AssignedTo != null ? feedback.AssignedTo.FullName : null,
                AttachmentPath = feedback.AttachmentPath,
                CreatedAt = feedback.CreatedAt,
                UpdatedAt = feedback.UpdatedAt,
                SubmittedBy = feedback.SubmittedBy,
                AssignedTo = feedback.AssignedTo
            };
        }

        public async Task<bool> CreateFeedbackAsync(FeedbackViewModel model)
        {
            try
            {
                var feedback = new Feedback
                {
                    Title = model.Title,
                    Description = model.Description,
                    Type = model.Type,
                    Status = "Pending", // Default status for new feedback
                    StaffResponse = null,
                    AssignedToId = null, // No assignment by default
                    SubmittedById = model.SubmittedById,
                    AttachmentPath = model.AttachmentPath,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = null
                };

                _context.Feedbacks.Add(feedback);
                await _context.SaveChangesAsync();

                // Notify staff about new feedback
                await _notificationService.NotifyFeedbackSubmitted(feedback.Id, feedback.SubmittedById, feedback.Type);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateFeedbackAsync(int id, FeedbackViewModel model)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null) return false;

                feedback.Title = model.Title;
                feedback.Description = model.Description;
                feedback.Type = model.Type;
                feedback.Status = model.Status;
                feedback.StaffResponse = model.StaffResponse;
                feedback.AssignedToId = model.AssignedToId;
                feedback.AttachmentPath = model.AttachmentPath ?? feedback.AttachmentPath;
                feedback.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Notify user about feedback response
                await _notificationService.NotifyFeedbackResponse(feedback.Id, feedback.SubmittedById, feedback.StaffResponse);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteFeedbackAsync(int id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null)
            {
                return false;
            }

            int submittedById = feedback.SubmittedById;

            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync();

            // Notify user and admins about deletion
            await _notificationService.NotifyUserFeedbackDeleted(id, submittedById);
            await _notificationService.NotifyAdminsFeedbackDeleted(id);

            return true;
        }

        public async Task<bool> UpdateFeedbackStatusAsync(int id, FeedbackResponseViewModel responseModel)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null) return false;

                feedback.Status = responseModel.Status;
                feedback.StaffResponse = responseModel.StaffResponse;
                feedback.UpdatedAt = DateTime.UtcNow;

                // Add logging for debugging
                Console.WriteLine($"Updating feedback {id} - Status: {responseModel.Status}, ResponseLength: {responseModel.StaffResponse?.Length ?? 0}");

                await _context.SaveChangesAsync();

                // Notify user about feedback response
                await _notificationService.NotifyFeedbackResponse(feedback.Id, feedback.SubmittedById, responseModel.StaffResponse);

                // Notify user if feedback is resolved or closed
                if (feedback.Status == "Resolved" || feedback.Status == "Closed")
                {
                    await _notificationService.NotifyFeedbackStatusChanged(feedback.Id, feedback.SubmittedById, feedback.Status);
                    // Notify admins as well
                    await _notificationService.NotifyAdminsFeedbackResolvedOrClosed(feedback.Id, feedback.Status);
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error updating feedback status: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        public async Task<bool> AssignFeedbackAsync(int id, int staffId)
        {
            try
            {
                var feedback = await _context.Feedbacks.FindAsync(id);
                if (feedback == null) return false;

                var staff = await _context.Users.FindAsync(staffId);
                if (staff == null) return false;

                int? previousStaffId = feedback.AssignedToId;
                bool isNewAssignment = previousStaffId != staffId;

                feedback.AssignedToId = staffId;
                feedback.Status = "In Progress";
                feedback.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Only notify if assignment changed
                if (isNewAssignment)
                {
                    await _notificationService.NotifyFeedbackAssignedToStaff(feedback.Id, staffId, feedback.Type);
                    if (previousStaffId.HasValue)
                    {
                        await _notificationService.NotifyFeedbackUnassignedFromStaff(feedback.Id, previousStaffId.Value, feedback.Type);
                    }
                    // Notify admins about assignment
                    await _notificationService.NotifyAdminsFeedbackAssigned(feedback.Id, staff.FullName);
                    // Notify user about assignment
                    await _notificationService.NotifyUserFeedbackAssigned(feedback.Id, feedback.SubmittedById, staff.FullName);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<List<FeedbackViewModel>> GetUserFeedbackAsync(int userId)
        {
            return await _context.Feedbacks
                .Include(f => f.SubmittedBy)
                .Include(f => f.AssignedTo)
                .Where(f => f.SubmittedById == userId)
                .Select(f => new FeedbackViewModel
                {
                    Id = f.Id,
                    Title = f.Title,
                    Description = f.Description,
                    Type = f.Type,
                    Status = f.Status,
                    StaffResponse = f.StaffResponse,
                    AssignedToId = f.AssignedToId,
                    SubmittedById = f.SubmittedById,
                    SubmittedByName = f.SubmittedBy != null ? f.SubmittedBy.FullName : null,
                    AssignedToName = f.AssignedTo != null ? f.AssignedTo.FullName : null,
                    AttachmentPath = f.AttachmentPath,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FeedbackViewModel>> GetAssignedFeedbackAsync(int staffId)
        {
            return await _context.Feedbacks
                .Include(f => f.SubmittedBy)
                .Include(f => f.AssignedTo)
                .Where(f => f.AssignedToId == staffId)
                .Select(f => new FeedbackViewModel
                {
                    Id = f.Id,
                    Title = f.Title,
                    Description = f.Description,
                    Type = f.Type,
                    Status = f.Status,
                    StaffResponse = f.StaffResponse,
                    AssignedToId = f.AssignedToId,
                    SubmittedById = f.SubmittedById,
                    SubmittedByName = f.SubmittedBy != null ? f.SubmittedBy.FullName : null,
                    AssignedToName = f.AssignedTo != null ? f.AssignedTo.FullName : null,
                    AttachmentPath = f.AttachmentPath,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<FeedbackViewModel>> GetRecentFeedbackAsync(int count)
        {
            return await _context.Feedbacks
                .Include(f => f.SubmittedBy)
                .Include(f => f.AssignedTo)
                .OrderByDescending(f => f.CreatedAt)
                .Take(count)
                .Select(f => new FeedbackViewModel
                {
                    Id = f.Id,
                    Title = f.Title,
                    Description = f.Description,
                    Type = f.Type,
                    Status = f.Status,
                    StaffResponse = f.StaffResponse,
                    AssignedToId = f.AssignedToId,
                    SubmittedById = f.SubmittedById,
                    SubmittedByName = f.SubmittedBy != null ? f.SubmittedBy.FullName : null,
                    AssignedToName = f.AssignedTo != null ? f.AssignedTo.FullName : null,
                    AttachmentPath = f.AttachmentPath,
                    CreatedAt = f.CreatedAt,
                    UpdatedAt = f.UpdatedAt
                })
                .ToListAsync();
        }

        public List<SelectListItem> GetFeedbackTypeList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Complaint", Text = "Complaint" },
                new SelectListItem { Value = "Suggestion", Text = "Suggestion" },
                new SelectListItem { Value = "Question", Text = "Question" },
                new SelectListItem { Value = "Appreciation", Text = "Appreciation" }
            };
        }

        public List<SelectListItem> GetFeedbackStatusList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Pending" },
                new SelectListItem { Value = "In Progress", Text = "In Progress" },
                new SelectListItem { Value = "Resolved", Text = "Resolved" },
                new SelectListItem { Value = "Closed", Text = "Closed" }
            };
        }

        // New methods for the view models
        public async Task<FeedbackListViewModel> GetFeedbackListViewModelAsync()
        {
            var feedbacks = await GetAllFeedbackAsync();
            return new FeedbackListViewModel
            {
                Feedbacks = feedbacks,
                TotalFeedbacks = feedbacks.Count,
                PendingFeedbacks = feedbacks.Count(f => f.Status == "Pending"),
                InProgressFeedbacks = feedbacks.Count(f => f.Status == "In Progress"),
                ResolvedFeedbacks = feedbacks.Count(f => f.Status == "Resolved")
            };
        }

        public async Task<FeedbackListViewModel> GetUserFeedbackListViewModelAsync(int userId)
        {
            var feedbacks = await GetUserFeedbackAsync(userId);
            return new FeedbackListViewModel
            {
                Feedbacks = feedbacks,
                TotalFeedbacks = feedbacks.Count,
                PendingFeedbacks = feedbacks.Count(f => f.Status == "Pending"),
                InProgressFeedbacks = feedbacks.Count(f => f.Status == "In Progress"),
                ResolvedFeedbacks = feedbacks.Count(f => f.Status == "Resolved")
            };
        }

        public async Task<FeedbackListViewModel> GetAssignedFeedbackListViewModelAsync(int staffId)
        {
            var feedbacks = await GetAssignedFeedbackAsync(staffId);
            return new FeedbackListViewModel
            {
                Feedbacks = feedbacks,
                TotalFeedbacks = feedbacks.Count,
                PendingFeedbacks = feedbacks.Count(f => f.Status == "Pending"),
                InProgressFeedbacks = feedbacks.Count(f => f.Status == "In Progress"),
                ResolvedFeedbacks = feedbacks.Count(f => f.Status == "Resolved")
            };
        }

        public async Task<FeedbackStatisticsViewModel> GetFeedbackStatisticsViewModelAsync()
        {
            var feedbacks = await _context.Feedbacks.ToListAsync();

            // Calculate type distribution
            var typeDistribution = feedbacks
                .GroupBy(f => f.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate monthly trends for the last 12 months
            var startDate = DateTime.UtcNow.AddMonths(-11).Date;
            var monthlyTrends = new Dictionary<string, int>();

            for (int i = 0; i < 12; i++)
            {
                var month = startDate.AddMonths(i);
                var monthName = month.ToString("MMM yyyy");
                var count = feedbacks.Count(f => f.CreatedAt.Year == month.Year && f.CreatedAt.Month == month.Month);
                monthlyTrends.Add(monthName, count);
            }

            return new FeedbackStatisticsViewModel
            {
                TotalFeedbacks = feedbacks.Count,
                PendingFeedbacks = feedbacks.Count(f => f.Status == "Pending"),
                InProgressFeedbacks = feedbacks.Count(f => f.Status == "In Progress"),
                ResolvedFeedbacks = feedbacks.Count(f => f.Status == "Resolved"),
                TypeDistribution = typeDistribution,
                MonthlyTrends = monthlyTrends
            };
        }
    }
}