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

        private async Task<string?> SaveFileAsync(IFormFile? file, string subDirectory)
        {
            if (file == null || file.Length == 0)
                return null;

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_environment.ContentRootPath, "SecureFiles", subDirectory);
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return a secure identifier for the file
            return $"{subDirectory}/{uniqueFileName}";
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
                        Image_Path = s.Image_Path,
                        Attachment_Path = s.Attachment_Path
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
                    Image_Path = request.Image_Path,
                    Attachment_Path = request.Attachment_Path
                };

                return new ServiceResult<ServiceReqViewModel> { Data = viewModel, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<ServiceReqViewModel> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResult<ServiceReqViewModel>> CreateRequestAsync(ServiceReqViewModel model, IFormFile? imageFile)
        {
            try
            {
                string? imagePath = null;
                if (imageFile != null && imageFile.Length > 0)
                {
                    imagePath = await SaveFileAsync(imageFile, "service-requests/images");
                }

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

                if (imageFile != null && imageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(request.Image_Path))
                    {
                        var oldFilePath = Path.Combine(_environment.ContentRootPath, "SecureFiles", request.Image_Path);
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }
                    request.Image_Path = await SaveFileAsync(imageFile, "service-requests/images");
                    model.Image_Path = request.Image_Path;
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
                        Image_Path = s.Image_Path,
                        Attachment_Path = s.Attachment_Path
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

        public async Task<ServiceResult<bool>> CreateServiceRequestAsync(ServiceReqViewModel model)
        {
            try
            {
                // Save attachment if provided
                string? attachmentPath = null;
                if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                {
                    attachmentPath = await SaveFileAsync(model.AttachmentFile, "service-requests");
                }

                // Save image if provided
                string? imagePath = null;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    imagePath = await SaveFileAsync(model.ImageFile, "service-requests/images");
                }

                var request = new ServiceRequest
                {
                    Userid = model.UserId,
                    Request_Type = model.Request_Type,
                    Description = model.Description,
                    Status = ServiceRequestStatus.Pending.ToString(),
                    Submitted_at = DateTime.Now,
                    Image_Path = imagePath,
                    Attachment_Path = attachmentPath
                };

                _context.ServiceRequests.Add(request);
                await _context.SaveChangesAsync();

                return new ServiceResult<bool> { Data = true, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResult<bool>> UpdateServiceRequestAsync(int id, ServiceReqViewModel model)
        {
            try
            {
                var existingRequest = await _context.ServiceRequests.FindAsync(id);
                if (existingRequest == null)
                {
                    return new ServiceResult<bool> { Success = false, Message = "Service request not found" };
                }

                // Update basic properties
                existingRequest.Request_Type = model.Request_Type;
                existingRequest.Description = model.Description;
                existingRequest.Status = model.Status.ToString();

                // Handle attachment update
                if (model.AttachmentFile != null && model.AttachmentFile.Length > 0)
                {
                    // Delete old attachment if exists
                    if (!string.IsNullOrEmpty(existingRequest.Attachment_Path))
                    {
                        var oldFilePath = Path.Combine(_environment.ContentRootPath, "SecureFiles", existingRequest.Attachment_Path);
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }
                    existingRequest.Attachment_Path = await SaveFileAsync(model.AttachmentFile, "service-requests");
                }

                // Handle image update
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(existingRequest.Image_Path))
                    {
                        var oldFilePath = Path.Combine(_environment.ContentRootPath, "SecureFiles", existingRequest.Image_Path);
                        if (File.Exists(oldFilePath))
                        {
                            File.Delete(oldFilePath);
                        }
                    }
                    existingRequest.Image_Path = await SaveFileAsync(model.ImageFile, "service-requests/images");
                }

                _context.Update(existingRequest);
                await _context.SaveChangesAsync();

                return new ServiceResult<bool> { Data = true, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool> { Success = false, Message = ex.Message };
            }
        }

        public async Task<ServiceResult<bool>> DeleteServiceRequestAsync(int id)
        {
            try
            {
                var request = await _context.ServiceRequests.FindAsync(id);
                if (request == null)
                {
                    return new ServiceResult<bool> { Success = false, Message = "Service request not found" };
                }

                // Delete attachment file if exists
                if (!string.IsNullOrEmpty(request.Attachment_Path))
                {
                    var filePath = Path.Combine(_environment.ContentRootPath, "SecureFiles", request.Attachment_Path);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                // Delete image file if exists
                if (!string.IsNullOrEmpty(request.Image_Path))
                {
                    var filePath = Path.Combine(_environment.ContentRootPath, "SecureFiles", request.Image_Path);
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }

                _context.ServiceRequests.Remove(request);
                await _context.SaveChangesAsync();

                return new ServiceResult<bool> { Data = true, Success = true };
            }
            catch (Exception ex)
            {
                return new ServiceResult<bool> { Success = false, Message = ex.Message };
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
    }
}