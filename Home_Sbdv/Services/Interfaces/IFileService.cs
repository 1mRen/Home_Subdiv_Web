using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Home_Sbdv.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Uploads a file to the specified directory
        /// </summary>
        /// <param name="file">The file to upload</param>
        /// <param name="directory">The target directory</param>
        /// <returns>The path to the uploaded file</returns>
        Task<string> UploadFileAsync(IFormFile file, string directory);

        /// <summary>
        /// Deletes a file from the system
        /// </summary>
        /// <param name="filePath">The path to the file to delete</param>
        /// <returns>True if the file was deleted successfully</returns>
        Task<bool> DeleteFileAsync(string filePath);

        /// <summary>
        /// Gets the full URL for a file
        /// </summary>
        /// <param name="filePath">The relative path of the file</param>
        /// <returns>The full URL to access the file</returns>
        string GetFileUrl(string filePath);
    }
} 