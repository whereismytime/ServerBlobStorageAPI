using System.IO;

namespace BlobStorageAPI.Models
{
    public class BlobObject
    {
        public Stream? Content { get; set; }
        public string? ContentType { get; set; }
    }
}