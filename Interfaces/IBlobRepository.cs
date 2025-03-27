using BlobStorageAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlobStorageAPI.Interfaces
{
    public interface IBlobRepository
    {
        Task<BlobObject?> DownloadBlobFileAsync(string containerName, string fileName);
        Task<string> UploadBlobFileAsync(string containerName, string filePath, string fileName);
        Task DeleteBlobFileAsync(string containerName, string fileName);
        Task<List<string>> GetBlobFilesAsync(string containerName);
    }
}