namespace DVLD.CORE.Interfaces
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(Stream fileStream, string originalFileName, string folderName);
        void DeleteFile(string fileName, string folderName);
        bool IsImage(string fileName);
    }
}
