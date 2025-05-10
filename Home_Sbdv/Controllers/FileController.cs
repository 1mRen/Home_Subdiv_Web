using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Home_Sbdv.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileController> _logger;

        public FileController(IWebHostEnvironment environment, ILogger<FileController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpGet("secure-file/{*filePath}")]
        public async Task<IActionResult> GetSecureFile(string filePath)
        {
            try
            {
                // Validate file path to prevent directory traversal
                if (filePath.Contains("..") || filePath.Contains(":"))
                {
                    return BadRequest("Invalid file path");
                }

                var fullPath = Path.Combine(_environment.ContentRootPath, "SecureFiles", filePath);
                
                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound();
                }

                // Get file info
                var fileInfo = new FileInfo(fullPath);
                var contentType = GetContentType(fileInfo.Extension);

                // Read file into memory
                var memory = new MemoryStream();
                using (var stream = new FileStream(fullPath, FileMode.Open))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                return File(memory, contentType, fileInfo.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving file: {FilePath}", filePath);
                return StatusCode(500, "Error serving file");
            }
        }

        private string GetContentType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
        }
    }
} 