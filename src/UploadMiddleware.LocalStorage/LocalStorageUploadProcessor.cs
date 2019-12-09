using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Generators;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.LocalStorage
{
    public class LocalStorageUploadProcessor : IUploadProcessor
    {
        private LocalStorageConfigure Configure { get; }

        private IFileNameGenerator FileNameGenerator { get; }

        private ISubdirectoryGenerator SubdirectoryGenerator { get; }

        public LocalStorageUploadProcessor(LocalStorageConfigure configure, IFileNameGenerator fileNameGenerator, ISubdirectoryGenerator subdirectoryGenerator)
        {
            Configure = configure;
            FileNameGenerator = fileNameGenerator;
            SubdirectoryGenerator = subdirectoryGenerator;
        }

        public Dictionary<string, string> FormData { get; } = new Dictionary<string, string>();

        public List<UploadFileResult> FileData { get; } = new List<UploadFileResult>();

        public Dictionary<string, string> QueryData { get; } = new Dictionary<string, string>();

        public async Task ProcessFile(Stream fileStream, string extensionName, HttpRequest request, string localFileName, string sectionName)
        {
            var subDir = await SubdirectoryGenerator.Generate(FormData, QueryData, request, extensionName, sectionName);
            var folder = Path.Combine(Configure.RootDirectory, subDir);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var fileName = await FileNameGenerator.Generate(FormData, QueryData, request, extensionName, sectionName) + extensionName;
            var url = Path.Combine(folder, fileName);
            await using var writeStream = File.Create(url);
            if (fileStream.CanSeek && fileStream.Position != 0)
                fileStream.Seek(0, SeekOrigin.Begin);
            await fileStream.CopyToAsync(writeStream, Configure.BufferSize);
            FileData.Add(new UploadFileResult { Name = sectionName, Url = Path.Combine("/", subDir, fileName).Replace("\\", "/") });
        }
    }
}
