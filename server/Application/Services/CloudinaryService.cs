using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace DatingApp.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly string _appFolder;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;

            _appFolder = config["Cloudinary:AppFolder"] ?? "aura";
        }

        public class CloudinaryUploadResult
        {
            public string Url { get; set; }
            public string PublicId { get; set; }
        }

        public async Task<CloudinaryUploadResult> UploadImageAsync(IFormFile file, string subfolder = "avatars")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File khong hop le");

            if (file.Length > 10 * 1024 * 1024)
                throw new ArgumentException("File vuot qua 10MB");

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Chi chap nhan file anh (jpg, png, webp, gif)");

            var fullFolder = $"{_appFolder}/{subfolder}";

            await using var stream = file.OpenReadStream();

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = fullFolder,
                Transformation = subfolder == "avatars"
                    ? new Transformation()
                        .Width(400).Height(400)
                        .Crop("fill")
                        .Gravity("face")
                        .Quality("auto")
                        .FetchFormat("auto")
                    : new Transformation()
                        .Width(1200)
                        .Crop("limit")
                        .Quality("auto")
                        .FetchFormat("auto")
            };

            var result = await _cloudinary.UploadAsync(uploadParams);

            if (result.Error != null)
                throw new Exception($"Cloudinary error: {result.Error.Message}");

            return new CloudinaryUploadResult
            {
                Url = result.SecureUrl.ToString(),
                PublicId = result.PublicId
            };
        }

        public async Task DeleteImageAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId)) return;
            var deleteParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deleteParams);
        }

        public static string? ExtractPublicId(string? url)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (!url.Contains("cloudinary.com")) return null;

            try
            {
                var uploadIndex = url.IndexOf("/upload/");
                if (uploadIndex < 0) return null;

                var afterUpload = url[(uploadIndex + 8)..];

                if (afterUpload.StartsWith("v") && afterUpload.Contains("/"))
                {
                    var firstSlash = afterUpload.IndexOf('/');
                    afterUpload = afterUpload[(firstSlash + 1)..];
                }

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
