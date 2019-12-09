using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.LocalStorage
{
    public class LocalStorageProcessor : IUploadProcessor
    {
        private LocalStorageConfigure Configure { get; }

        public LocalStorageProcessor(LocalStorageConfigure configure)
        {
            Configure = configure;
        }

        public Dictionary<string, string> FormData { get; } = new Dictionary<string, string>();

        public List<UploadFileResult> FileData { get; } = new List<UploadFileResult>();

        public Dictionary<string, string> QueryData { get; } = new Dictionary<string, string>();

        public async Task ProcessFile(Stream fileStream, string extensionName, HttpRequest request, string localFileName, string sectionName)
        {
            var subDir = Configure.SubdirectoryGenerator?.Invoke(request, extensionName) ?? "";
            var folder = Path.Combine(Configure.SaveRootDirectory, subDir);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var fileName = Configure.FileNameGenerator.Invoke(request, extensionName) + extensionName;
            var url = Path.Combine(folder, fileName);
            await using var writeStream = File.Create(url);
            await fileStream.CopyToAsync(writeStream, Configure.BufferSize);
            FileData.Add(new UploadFileResult { Name = sectionName, Url = Path.Combine("/", subDir, fileName).Replace("\\", "/") });
        }
    }
}
