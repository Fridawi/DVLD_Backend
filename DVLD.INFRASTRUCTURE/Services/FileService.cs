using DVLD.CORE.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
namespace DVLD.INFRASTRUCTURE.Services
{
    public class FileService : IFileService
    {
        private readonly string _basePath;

        public FileService(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            var configuredPath = configuration["FileStorage:UploadsPath"];

            if (Path.IsPathRooted(configuredPath))
            {
                _basePath = configuredPath;
            }
            else
            {
                _basePath = Path.Combine(webHostEnvironment.ContentRootPath, configuredPath ?? "Resources/Uploads");
            }
        }

        public async Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string folderName)
        {
            if (fileStream == null || fileStream.Length == 0)
                throw new ArgumentException("File content cannot be empty.");

            string uploadsFolder = Path.Combine(_basePath, folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            string extension = Path.GetExtension(originalFileName);
            string uniqueFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var newFileStream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(newFileStream);
            }

            return uniqueFileName;
        }

        public void DeleteFile(string fileName, string folderName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;

            string filePath = Path.Combine(_basePath, folderName, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public bool IsImage(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".avif", ".webp" };
            var extension = Path.GetExtension(fileName).ToLower();

            return allowedExtensions.Contains(extension);
        }
    }
}
