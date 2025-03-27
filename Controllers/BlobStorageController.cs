using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BlobStorageAPI.Models;
using BlobStorageAPI.Interfaces;
using System.IO;

namespace BlobStorageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlobStorageController : ControllerBase
    {
        private readonly IBlobRepository _blobRepository;

        public BlobStorageController(IBlobRepository blobRepository)
        {
            _blobRepository = blobRepository;
        }

        [HttpGet("DownloadBlobFile")]
        public async Task<IActionResult> DownloadBlobFile([FromQuery] string containerName, [FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(containerName) || string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Container name and file name are required.");

            var result = await _blobRepository.DownloadBlobFileAsync(containerName, fileName);

            if (result == null || result.Content == null)
                return NotFound("File not found.");

            return File(result.Content, result.ContentType ?? "application/octet-stream");
        }

        [HttpPost("UploadBlobFile")]
        public async Task<IActionResult> UploadBlobFile([FromBody] BlobContentModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.FilePath) || string.IsNullOrWhiteSpace(model.FileName))
                return BadRequest("Invalid file data.");

            var containerName = GetContainerNameFromFileName(model.FileName);

            var fileUrl = await _blobRepository.UploadBlobFileAsync(containerName, model.FilePath, model.FileName);
            return Ok(fileUrl);
        }

        [HttpDelete("DeleteBlobFile")]
        public async Task<IActionResult> DeleteBlobFile([FromQuery] string containerName, [FromQuery] string fileName)
        {
            if (string.IsNullOrWhiteSpace(containerName) || string.IsNullOrWhiteSpace(fileName))
                return BadRequest("Container name and file name are required.");

            await _blobRepository.DeleteBlobFileAsync(containerName, fileName);
            return Ok("File deleted successfully.");
        }

        [HttpGet("GetBlobFiles")]
        public async Task<IActionResult> GetBlobFiles([FromQuery] string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
                return BadRequest("Container name is required.");

            var result = await _blobRepository.GetBlobFilesAsync(containerName);
            return Ok(result);
        }

        private string GetContainerNameFromFileName(string fileName)
        {
            var ext = Path.GetExtension(fileName)?.ToLowerInvariant();

            return ext switch
            {
                ".jpg" or ".jpeg" or ".png" or ".bmp" or ".gif" => "images",
                ".mp3" or ".wav" or ".aac" or ".flac" or ".ogg" => "audio",
                ".mp4" or ".avi" or ".mov" or ".wmv" or ".flv" or ".mkv" => "video",
                _ => throw new ApplicationException("Unsupported file extension.")
            };
        }
    }
}
