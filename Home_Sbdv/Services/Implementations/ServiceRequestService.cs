using Home_Sbdv.Entities;
using Home_Sbdv.Models;
using Home_Sbdv.Data;
using Home_Sbdv.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace Home_Sbdv.Services.Implementations
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ServiceRequestService(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private async Task<string?> SaveImageAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
                return null;

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "service-requests");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            // Always use the tilde prefix for consistency
            return $"~/uploads/service-requests/{uniqueFileName}";
        }

        public async Task<ServiceResult<List<ServiceReqViewModel>>> GetAllRequestsAsync()
        {
            try
            {
                var requests = await _context.ServiceRequests
                    .Include(s => s.User)
                    .OrderByDescending(s => s.Submitted_at)
                    .Select(s => new ServiceReqViewModel
                    {
                        RequestId = s.Req_Id,
                        UserId = s.Userid,
                        Request_Type = s.Request_Type,
                        Description = s.Description,
                        Status = s.Status,
                        Submitted_at = s.Submitted_at,
                        SubmittedByName = s.User.FullName,
                        Image_Path = s.Image_Path
                    })
                    .ToListAsync();

                return new ServiceResult<List<ServiceReqViewModel>> { Data = requests, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<ServiceReqViewModel>> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResult<ServiceReqViewModel>> GetRequestByIdAsync(int id)
        {
            try
            {
                var request = await _context.ServiceRequests
                    .Include(s => s.User)
                    .FirstOrDefaultAsync(s => s.Req_Id == id);

                if (request == null)
                    return new ServiceResult<ServiceReqViewModel> { Success = false, Message = "Request not found" };

                var viewModel = new ServiceReqViewModel
                {
                    RequestId = request.Req_Id,
                    UserId = request.Userid,
                    Request_Type = request.Request_Type,
                    Description = request.Description,
                    Status = request.Status,
                    Submitted_at = request.Submitted_at,
                    SubmittedByName = request.User.FullName,
                    // Ensure image path has the correct format for Url.Content
                    Image_Path = NormalizeImagePath(request.Image_Path)
                };

                return new ServiceResult<ServiceReqViewModel> { Data = viewModel, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ServiceReqViewModel> { Success = false, Message = ex.Message };
            }
        }

        // Helper method to ensure image paths are consistent
        private string? NormalizeImagePath(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            // If the path doesn't start with ~, add it
            if (!path.StartsWith("~"))
            {
                if (path.StartsWith("/"))
                    return $"~{path}";
                else
                    return $"~/{path}";
            }

            return path;
        }

        public async Task<ServiceResult<ServiceReqViewModel>> CreateRequestAsync(ServiceReqViewModel model, IFormFile? imageFile)
        {
            try
            {
                var imagePath = await SaveImageAsync(imageFile);

                var request = new ServiceRequest
                {
                    Userid = model.UserId,
                    Request_Type = model.Request_Type,
                    Description = model.Description,
                    Status = "Pending",
                    Submitted_at = DateTime.Now,
                    Image_Path = imagePath
                };

                _context.ServiceRequests.Add(request);
                await _context.SaveChangesAsync();

                model.RequestId = request.Req_Id;
                model.Status = request.Status;
                model.Submitted_at = request.Submitted_at;
                model.Image_Path = request.Image_Path;

                return new ServiceResult<ServiceReqViewModel> { Data = model, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ServiceReqViewModel> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResult<ServiceReqViewModel>> UpdateRequestAsync(int id, ServiceReqViewModel model, IFormFile? imageFile)
        {
            try
            {
                var request = await _context.ServiceRequests.FindAsync(id);
                if (request == null)
                    return new ServiceResult<ServiceReqViewModel> { Success = false, Message = "Request not found" };

                request.Request_Type = model.Request_Type;
                request.Description = model.Description;

                if (imageFile != null)
                {
                    var imagePath = await SaveImageAsync(imageFile);
                    request.Image_Path = imagePath;
                    model.Image_Path = imagePath;
                }

                await _context.SaveChangesAsync();

                model.Status = request.Status;
                model.Submitted_at = request.Submitted_at;

                return new ServiceResult<ServiceReqViewModel> { Data = model, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ServiceReqViewModel> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResult<bool>> DeleteRequestAsync(int id)
        {
            try
            {
                var request = await _context.ServiceRequests.FindAsync(id);
                if (request == null)
                    return new ServiceResult<bool> { Success = false, Message = "Request not found" };

                _context.ServiceRequests.Remove(request);
                await _context.SaveChangesAsync();

                return new ServiceResult<bool> { Data = true, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResult<ServiceReqViewModel>> ApproveRequestAsync(int id)
        {
            try
            {
                var request = await _context.ServiceRequests.FindAsync(id);
                if (request == null)
                    return new ServiceResult<ServiceReqViewModel> { Success = false, Message = "Request not found" };

                request.Status = ServiceRequestStatus.Approved.ToString();
                await _context.SaveChangesAsync();

                var viewModel = new ServiceReqViewModel
                {
                    RequestId = request.Req_Id,
                    UserId = request.Userid,
                    Request_Type = request.Request_Type,
                    Description = request.Description,
                    Status = request.Status,
                    Submitted_at = request.Submitted_at,
                    Image_Path = NormalizeImagePath(request.Image_Path)
                };

                return new ServiceResult<ServiceReqViewModel> { Data = viewModel, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ServiceReqViewModel> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResult<ServiceReqViewModel>> RejectRequestAsync(int id)
        {
            try
            {
                var request = await _context.ServiceRequests.FindAsync(id);
                if (request == null)
                    return new ServiceResult<ServiceReqViewModel> { Success = false, Message = "Request not found" };

                request.Status = ServiceRequestStatus.Disapproved.ToString();
                await _context.SaveChangesAsync();

                var viewModel = new ServiceReqViewModel
                {
                    RequestId = request.Req_Id,
                    UserId = request.Userid,
                    Request_Type = request.Request_Type,
                    Description = request.Description,
                    Status = request.Status,
                    Submitted_at = request.Submitted_at,
                    Image_Path = NormalizeImagePath(request.Image_Path)
                };

                return new ServiceResult<ServiceReqViewModel> { Data = viewModel, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ServiceReqViewModel> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResult<ServiceReqViewModel>> UpdateStatusAsync(int id, string status)
        {
            try
            {
                var request = await _context.ServiceRequests.FindAsync(id);
                if (request == null)
                    return new ServiceResult<ServiceReqViewModel> { Success = false, Message = "Request not found" };

                if (!Enum.TryParse<ServiceRequestStatus>(status, out _))
                    return new ServiceResult<ServiceReqViewModel> { Success = false, Message = "Invalid status" };

                request.Status = status;
                await _context.SaveChangesAsync();

                var viewModel = new ServiceReqViewModel
                {
                    RequestId = request.Req_Id,
                    UserId = request.Userid,
                    Request_Type = request.Request_Type,
                    Description = request.Description,
                    Status = request.Status,
                    Submitted_at = request.Submitted_at,
                    Image_Path = NormalizeImagePath(request.Image_Path)
                };

                return new ServiceResult<ServiceReqViewModel> { Data = viewModel, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ServiceReqViewModel> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResult<List<ServiceReqViewModel>>> GetUserRequestsAsync(int userId)
        {
            try
            {
                var requests = await _context.ServiceRequests
                    .Include(s => s.User)
                    .Where(s => s.Userid == userId)
                    .OrderByDescending(s => s.Submitted_at)
                    .Select(s => new ServiceReqViewModel
                    {
                        RequestId = s.Req_Id,
                        UserId = s.Userid,
                        Request_Type = s.Request_Type,
                        Description = s.Description,
                        Status = s.Status,
                        Submitted_at = s.Submitted_at,
                        SubmittedByName = s.User.FullName,
                        Image_Path = s.Image_Path
                    })
                    .ToListAsync();

                // Normalize all image paths in the list
                foreach (var request in requests)
                {
                    request.Image_Path = NormalizeImagePath(request.Image_Path);
                }

                return new ServiceResult<List<ServiceReqViewModel>> { Data = requests, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<List<ServiceReqViewModel>> { Success = false, Message = ex.Message };
            }
        }
    }
}