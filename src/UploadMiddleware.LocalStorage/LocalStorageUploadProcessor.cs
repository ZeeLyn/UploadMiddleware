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

        private IFileValidator FileValidator { get; }

        public LocalStorageUploadProcessor(LocalStorageConfigure configure, IFileNameGenerator fileNameGenerator, ISubdirectoryGenerator subdirectoryGenerator, IFileValidator fileValidator)
        {
            Configure = configure;
            FileNameGenerator = fileNameGenerator;
            SubdirectoryGenerator = subdirectoryGenerator;
            FileValidator = fileValidator;
        }

        public List<UploadFileResult> FileData { get; } = new List<UploadFileResult>();


        public async Task<(bool Success, string ErrorMessage)> Process(HttpRequest request, IQueryCollection query, IFormCollection form, IHeaderDictionary headers, Stream fileStream, string extensionName, string localFileName, string sectionName)
        {
            var (success, errorMsg, fileSignature) = await FileValidator.Validate(localFileName, fileStream);
            if (!success)
                return (false, errorMsg);
            var subDir = await SubdirectoryGenerator.Generate(request, query, form, headers, extensionName);
            var folder = Path.Combine(Configure.RootDirectory, subDir);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var fileName = await FileNameGenerator.Generate(request, query, form, headers, extensionName) + extensionName;
            var url = Path.Combine(folder, fileName);
            await using var writeStream = File.Create(url);
            if (fileSignature != null && fileSignature.Length > 0)
                writeStream.Write(fileSignature, 0, fileSignature.Length);
            await fileStream.CopyToAsync(writeStream, Configure.BufferSize);
            FileData.Add(new UploadFileResult { Name = sectionName, Url = Path.Combine("/", subDir, fileName).Replace("\\", "/") });
            return (true, "");
        }
    }
}
