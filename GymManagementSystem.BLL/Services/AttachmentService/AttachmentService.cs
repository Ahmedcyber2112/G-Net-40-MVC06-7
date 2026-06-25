using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GymManagementSystem.BLL.Services.AttachmentService
{
    public class AttachmentService : IAttachmentService
    {
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png" };

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AttachmentService> _logger;

        // 1. الكونستراكتور المظبوط (عشان ياخد البيانات من الـ Program.cs)
        public AttachmentService(IWebHostEnvironment env, ILogger<AttachmentService> logger)
        {
            _env = env;
            _logger = logger;
        }

        // 2. دالة الرفع (UploadAsync) كاملة باللوجيك اللي عملناه
        public async Task<string?> UploadAsync(Stream FileStream, string FileName, string FolderName, CancellationToken ct)
        {
            if (FileStream is null || !FileStream.CanRead) return null;
            if (FileStream.Length == 0) return null;

            if (FileStream.Length > _maxFileSize)
            {
                _logger.LogWarning("Rejected Upload: File Too Large {Size} bytes", FileStream.Length);
                return null;
            }

            var Extension = Path.GetExtension(FileName);
            if (string.IsNullOrEmpty(Extension) || !_allowedExtensions.Contains(Extension.ToLower()))
            {
                _logger.LogWarning("Rejected Upload: Extension {Ext} Not Allowed", Extension);
                return null;
            }

            var UploadsFolder = Path.Combine(_env.WebRootPath, FolderName);
            Directory.CreateDirectory(UploadsFolder);

            var StoredFileName = $"{Guid.NewGuid()}{Extension}";
            var FilePath = Path.Combine(UploadsFolder, StoredFileName);

            try
            {
                await using var FS = new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                await FileStream.CopyToAsync(FS, ct);
                return StoredFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed To Upload File {FileName}", FileName);
                return null;
            }
        }

        // 3. باقي دوال الإنترفيس (بنحطهم عشان الخط الأحمر يختفي لحد ما نبرمجهم)
        public bool Delete(string FileName, string FolderName)
        {
            // التأكد إن اسم الملف والفولدر مبعوتين صح
            if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(FolderName))
                return false;

            try
            {
                // تكوين المسار الكامل للملف جوه الـ wwwroot
                var FullPath = Path.Combine(_env.WebRootPath, FolderName, FileName);

                // لو الملف مش موجود أصلاً، هنرجع false
                if (!File.Exists(FullPath))
                    return false;

                // حذف الملف
                File.Delete(FullPath);
                return true;
            }
            catch (Exception ex)
            {
                // تسجيل الخطأ لو حصلت أي مشكلة أثناء الحذف
                _logger.LogError(ex, "Failed To Delete File {FileName}", FileName);
                return false;
            }
        }

        public (Stream stream, string ContentType)? GetFile(string FileName, string FolderName)
        {
            // 1. التأكد من إن الأسماء مبعوتة صح
            if (string.IsNullOrEmpty(FileName) || string.IsNullOrEmpty(FolderName))
                return null;

            // 2. تكوين المسار الكامل للملف جوه الـ wwwroot
            var FullPath = Path.Combine(_env.WebRootPath, FolderName, FileName);

            // 3. لو الملف مش موجود، نرجع null
            if (!File.Exists(FullPath))
                return null;

            // 4. تحديد نوع الملف (MIME Type) بناءً على الامتداد
            var ContentType = Path.GetExtension(FullPath).ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream" // النوع الافتراضي لو مش صورة
            };

            // 5. فتح الملف للقراءة وبعته كـ Stream
            var stream = new FileStream(FullPath, FileMode.Open, FileAccess.Read, FileShare.Read);

            return (stream, ContentType);
        }
    }
}