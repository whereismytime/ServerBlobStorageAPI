using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlobStorageAPI.Interfaces;
using BlobStorageAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BlobStorageAPI.Repositories
{
    public class BlobRepository : IBlobRepository
    {
        private readonly BlobServiceClient _blobServiceClient;

        private static readonly Dictionary<string, string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", "image/jpeg" }, { ".jpeg", "image/jpeg" }, { ".png", "image/png" },
            { ".bmp", "image/bmp" }, { ".gif", "image/gif" }, { ".mp4", "video/mp4" },
            { ".avi", "video/x-msvideo" }, { ".mov", "video/quicktime" }, { ".wmv", "video/x-ms-wmv" },
            { ".flv", "video/x-flv" }, { ".mkv", "video/x-matroska" }, { ".mp3", "audio/mpeg" },
            { ".wav", "audio/wav" }, { ".aac", "audio/aac" }, { ".flac", "audio/flac" }, { ".ogg", "audio/ogg" }
        };

        private static readonly List<string> AllowedImageExtensions = new() { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
        private static readonly List<string> AllowedVideoExtensions = new() { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv" };
        private static readonly List<string> AllowedAudioExtensions = new() { ".mp3", ".wav", ".aac", ".flac", ".ogg" };

        public BlobRepository(BlobServiceClient blobServiceClient)
        {
            _blobServiceClient = blobServiceClient;
        }

        private BlobContainerClient GetContainerClient(string containerName)
        {
            return _blobServiceClient.GetBlobContainerClient(containerName);
        }

        private bool IsAllowedContainer(string containerName) =>
            containerName.Equals("images", StringComparison.OrdinalIgnoreCase) ||
            containerName.Equals("audio", StringComparison.OrdinalIgnoreCase) ||
            containerName.Equals("video", StringComparison.OrdinalIgnoreCase);

        private void ValidateFileExtension(string containerName, string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(ext)) throw new ApplicationException("Cannot determine file extension.");

            if (!IsAllowedContainer(containerName))
                throw new ApplicationException($"Container '{containerName}' is not allowed.");

            if (containerName == "images" && !AllowedImageExtensions.Contains(ext) ||
                containerName == "audio" && !AllowedAudioExtensions.Contains(ext) ||
                containerName == "video" && !AllowedVideoExtensions.Contains(ext))
                throw new ApplicationException($"Extension '{ext}' is not allowed in container '{containerName}'.");
        }

        public async Task<BlobObject?> DownloadBlobFileAsync(string containerName, string fileName)
        {
            try
            {
                if (IsAllowedContainer(containerName))
                    ValidateFileExtension(containerName, fileName);

                var blobClient = GetContainerClient(containerName).GetBlobClient(fileName);

                if (await blobClient.ExistsAsync())
                {
                    BlobDownloadResult result = await blobClient.DownloadContentAsync();
                    var stream = result.Content.ToStream();
                    string contentType = AllowedMimeTypes.GetValueOrDefault(Path.GetExtension(fileName)!.ToLowerInvariant(), result.Details.ContentType);

                    return new BlobObject { Content = stream, ContentType = contentType };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error downloading file '{fileName}' from container '{containerName}'.", ex);
            }
        }

        public async Task<string> UploadBlobFileAsync(string containerName, string filePath, string fileName)
        {
            try
            {
                ValidateFileExtension(containerName, fileName);
                var blobClient = GetContainerClient(containerName).GetBlobClient(fileName);
                await blobClient.UploadAsync(filePath, overwrite: true);
                return blobClient.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error uploading file '{fileName}' to container '{containerName}'.", ex);
            }
        }

        public async Task<List<string>> GetBlobFilesAsync(string containerName)
        {
            try
            {
                if (!IsAllowedContainer(containerName))
                    throw new ApplicationException($"Access to container '{containerName}' is not allowed.");

                var result = new List<string>();
                await foreach (var blob in GetContainerClient(containerName).GetBlobsAsync())
                    result.Add(blob.Name);
                return result;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error listing files in container '{containerName}'.", ex);
            }
        }

        public async Task DeleteBlobFileAsync(string containerName, string fileName)
        {
            try
            {
                if (IsAllowedContainer(containerName))
                    ValidateFileExtension(containerName, fileName);

                var blobClient = GetContainerClient(containerName).GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Error deleting file '{fileName}' from container '{containerName}'.", ex);
            }
        }
    }
}