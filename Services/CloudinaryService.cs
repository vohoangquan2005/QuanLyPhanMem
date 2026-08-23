using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using HotelManagement.Exceptions;
using HotelManagement.Settings;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace HotelManagement.Services
{
    public class CloudinaryService
    {
        public const string UserAvatarFolder = "hotel-management/avatars";

        private readonly Cloudinary _cloudinary;
        private readonly string _cloudName;

        private static readonly string[] AllowedImageTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private static readonly string[] AllowedImageExtensions =
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public CloudinaryService(
            Cloudinary cloudinary,
            IOptions<CloudinarySetting> cloudinarySettings)
        {
            _cloudinary = cloudinary;
            _cloudName = cloudinarySettings.Value.CloudName;
        }

        private void ValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new BadRequestException("Vui lòng chọn hình ảnh!");
            }

            if (file.Length > MaxFileSize)
            {
                throw new BadRequestException("Kích thước hình ảnh không được vượt quá 5MB!");
            }

            if (!AllowedImageTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                throw new BadRequestException(
                    "Chỉ được phép upload ảnh JPG, JPEG, PNG hoặc WEBP!");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                throw new BadRequestException("Định dạng hình ảnh không hợp lệ!");
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string? folder = null)
        {
            ValidateImage(file);

            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream)
            };

            if (!string.IsNullOrWhiteSpace(folder))
            {
                uploadParams.Folder = folder;
                uploadParams.PublicId = Guid.NewGuid().ToString("N");
            }

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            if (uploadResult.Error != null)
            {
                throw new BadRequestException(uploadResult.Error.Message);
            }

            var secureUrl = uploadResult.SecureUrl?.ToString();
            if (string.IsNullOrWhiteSpace(secureUrl)
                || !Uri.TryCreate(secureUrl, UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps)
            {
                throw new BadRequestException("Cloudinary không trả về URL HTTPS hợp lệ!");
            }

            return secureUrl;
        }

        public Task DeleteImageAsync(string? imageUrl)
        {
            return DeleteImageAsync(imageUrl, requiredFolder: null);
        }

        public async Task DeleteImageAsync(string? imageUrl, string? requiredFolder)
        {
            if (!TryGetManagedPublicId(imageUrl, requiredFolder, out var publicId))
            {
                return;
            }

            var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));
            if (result.Error != null)
            {
                throw new BadRequestException(result.Error.Message);
            }
        }

        private bool TryGetManagedPublicId(
            string? imageUrl,
            string? requiredFolder,
            out string publicId)
        {
            publicId = string.Empty;

            if (string.IsNullOrWhiteSpace(imageUrl)
                || !Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps
                || !string.Equals(uri.Host, "res.cloudinary.com", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var segments = uri.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.UnescapeDataString)
                .ToArray();

            if (segments.Length < 5
                || !string.Equals(segments[0], _cloudName, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(segments[1], "image", StringComparison.Ordinal)
                || !string.Equals(segments[2], "upload", StringComparison.Ordinal))
            {
                return false;
            }

            var versionIndex = Array.FindIndex(
                segments,
                3,
                segment => Regex.IsMatch(segment, "^v[0-9]+$", RegexOptions.CultureInvariant));

            if (versionIndex < 0 || versionIndex + 1 >= segments.Length)
            {
                return false;
            }

            var publicIdSegments = segments[(versionIndex + 1)..];
            var lastSegment = publicIdSegments[^1];
            var extension = Path.GetExtension(lastSegment);

            if (!AllowedImageExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            publicIdSegments[^1] = Path.GetFileNameWithoutExtension(lastSegment);
            publicId = string.Join("/", publicIdSegments);

            if (string.IsNullOrWhiteSpace(publicId))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(requiredFolder))
            {
                var folderPrefix = requiredFolder.TrimEnd('/') + "/";
                if (!publicId.StartsWith(folderPrefix, StringComparison.Ordinal))
                {
                    publicId = string.Empty;
                    return false;
                }
            }

            return true;
        }
    }
}
