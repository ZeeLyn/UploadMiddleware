using System.IO;
using System.Threading.Tasks;
using COSXML;
using COSXML.Model.Object;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Generators;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.TencentCOS
{
    public class TencentCosStorageUploadProcessor : IUploadProcessor
    {
        private TencentCosStorageConfigure Configure { get; }

        private CosXml Client { get; }

        private IFileNameGenerator FileNameGenerator { get; }

        private ISubdirectoryGenerator SubdirectoryGenerator { get; }

        private IFileValidator FileValidator { get; }


        public TencentCosStorageUploadProcessor(TencentCosStorageConfigure configure, CosXml client, IFileNameGenerator fileNameGenerator, ISubdirectoryGenerator subdirectoryGenerator, IFileValidator fileValidator)
        {
            Configure = configure;
            Client = client;
            FileNameGenerator = fileNameGenerator;
            SubdirectoryGenerator = subdirectoryGenerator;
            FileValidator = fileValidator;
        }

        public async Task<(bool Success, UploadFileResult Result, string ErrorMessage)> Process(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, Stream fileStream, string extensionName, string localFileName, string sectionName, HttpRequest request)
        {
            var (success, errorMsg, fileSignature) = await FileValidator.Validate(localFileName, fileStream);
            if (!success)
                return (false, null, errorMsg);

            var subDir = await SubdirectoryGenerator.Generate(query, form, headers, extensionName, request);
            var folder = Path.Combine(Configure.RootDirectory, subDir);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var fileName = await FileNameGenerator.Generate(query, form, headers, extensionName, request) + extensionName;
            var url = Path.Combine(folder, fileName).Replace("\\", "/");
            await using var stream = new MemoryStream();
            if (fileSignature != null && fileSignature.Length > 0)
                stream.Write(fileSignature, 0, fileSignature.Length);
            await fileStream.CopyToAsync(stream, Configure.BufferSize);
            stream.Seek(0, SeekOrigin.Begin);
            var fileBytes = new byte[stream.Length];
            await stream.ReadAsync(fileBytes);
            var req = new PutObjectRequest(Configure.Bucket, url, fileBytes);

            var resp = Client.PutObject(req);
            if (resp.httpCode != 200)
                return (false, null, resp.httpMessage);
            var serverPath = "/" + url;
            try
            {
                var callback = request.HttpContext.RequestServices.GetService<IUploadCompletedCallbackHandler>();
                if (callback != null)
                    await callback.OnCompletedAsync(serverPath, localFileName, request);
            }
            catch
            {
                // ignored
            }
            return (true, new UploadFileResult { Name = sectionName, Url = serverPath }, "");
        }
    }
}
