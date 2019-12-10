using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Aliyun.OSS;
using Microsoft.AspNetCore.Http;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Generators;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.AliyunOSS
{
    public class AliyunOssStorageUploadProcessor : IUploadProcessor
    {
        private AliyunOssStorageConfigure Configure { get; }

        private IOss Client { get; }

        private IFileNameGenerator FileNameGenerator { get; }

        private ISubdirectoryGenerator SubdirectoryGenerator { get; }

        private IFileValidator FileValidator { get; }


        public AliyunOssStorageUploadProcessor(AliyunOssStorageConfigure configure, IOss client, IFileNameGenerator fileNameGenerator, ISubdirectoryGenerator subdirectoryGenerator, IFileValidator fileValidator)
        {
            Configure = configure;
            Client = client;
            FileNameGenerator = fileNameGenerator;
            SubdirectoryGenerator = subdirectoryGenerator;
            FileValidator = fileValidator;
        }

        public Dictionary<string, string> FormData { get; } = new Dictionary<string, string>();

        public List<UploadFileResult> FileData { get; } = new List<UploadFileResult>();

        public Dictionary<string, string> QueryData { get; } = new Dictionary<string, string>();

        public async Task<(bool Success, string ErrorMessage)> ProcessFile(Stream fileStream, string extensionName, HttpRequest request, string localFileName, string sectionName)
        {
            var (success, fileSignature) = await FileValidator.Validate(localFileName, fileStream);
            if (!success)
                return (false, "Illegal file format.");

            var subDir = await SubdirectoryGenerator.Generate(FormData, QueryData, request, extensionName);
            var folder = Path.Combine(Configure.RootDirectory, subDir);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var fileName = await FileNameGenerator.Generate(FormData, QueryData, request, extensionName) + extensionName;
            var url = Path.Combine(folder, fileName).Replace("\\", "/");
            await using var stream = new MemoryStream();
            if (fileSignature != null && fileSignature.Length > 0)
                stream.Write(fileSignature, 0, fileSignature.Length);
            await fileStream.CopyToAsync(stream, Configure.BufferSize);
            stream.Seek(0, SeekOrigin.Begin);
            if (!Configure.Metadata.TryGetValue(extensionName, out var meta))
            {
                meta = new ObjectMetadata { ContentType = "application/octet-stream" };
            }

            meta.ContentDisposition = meta.ContentDisposition.Resolve(FormData, QueryData, localFileName, sectionName);
            if (meta.UserMetadata != null)
            {
                foreach (var (key, _) in meta.UserMetadata)
                {
                    meta.UserMetadata[key] = meta.UserMetadata[key].Resolve(FormData, QueryData, localFileName, sectionName);
                }
            }

            Client.PutObject(Configure.BucketName, url, stream, meta);

            FileData.Add(new UploadFileResult { Name = sectionName, Url = "/" + url });
            return (true, "");
        }
    }
}
