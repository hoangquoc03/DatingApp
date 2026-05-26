using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace DatingApp.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        /// <summary>
        /// Upload ?nh lên Cloudinary, tr? v? URL public.
        /// </summary>
        public async Task<string> UploadImageAsync(IFormFile file, string folder = "avatars")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File không h?p l?");

            // Gi?i h?n 5MB
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File vý?t quá 5MB");

            // Ch? cho phép ?nh
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Ch? ch?p nh?n file ?nh (jpg, png, webp, gif)");

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                // T? ð?ng crop v? 400x400 cho avatar
                Transformation = new Transformation()
                    .Width(400).Height(400)
                    .Crop("fill")
                    .Gravity("face")
                    .Quality("auto")
                    .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Cloudinary error: {result.Error.Message}");

            return result.SecureUrl.ToString();
        }

        /// <summary>
        /// Xóa ?nh kh?i Cloudinary theo publicId.
        /// </summary>
        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return;

            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }

        /// <summary>
        /// L?y publicId t? Cloudinary URL ð? xóa ?nh c?.
        /// VD: "https://res.cloudinary.com/demo/image/upload/avatars/abc123.jpg" ? "avatars/abc123"
        /// </summary>
        public static string? ExtractPublicId(string? url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (!url.Contains("cloudinary.com")) return null;

            try
            {
                // T?m "/upload/" trong URL và l?y ph?n sau
                var uploadIndex = url.IndexOf("/upload/");
                if (uploadIndex < 0) return null;

                var afterUpload = url[(uploadIndex + 8)..]; // b? "/upload/"

                // B? version n?u có (v1234567890/)
                if (afterUpload.StartsWith("v") && afterUpload.Contains("/"))
                {
                    var firstSlash = afterUpload.IndexOf('/');
                    afterUpload = afterUpload[(firstSlash + 1)..];
                }

                // B? extension
                var lastDot = afterUpload.LastIndexOf('.');
                return lastDot >= 0 ? afterUpload[..lastDot] : afterUpload;
            }
            catch
            {
                return null;
            }
        }
    }
}