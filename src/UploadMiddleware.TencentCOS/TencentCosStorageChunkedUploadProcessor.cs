using System;
using System.IO;
using System.Threading.Tasks;
using COSXML;
using COSXML.Auth;
using COSXML.CosException;
using COSXML.Model.Tag;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Generators;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.TencentCOS
{
    public class TencentCosStorageChunkedUploadProcessor : IUploadProcessor
    {
        private ChunkedUploadTencentCosStorageConfigure Configure { get; }

        private IFileValidator FileValidator { get; }

        private CosXml Client { get; }

        private IMemoryCache MemoryCache { get; }

        private IFileNameGenerator FileNameGenerator { get; }

        private ISubdirectoryGenerator SubdirectoryGenerator { get; }

        public TencentCosStorageChunkedUploadProcessor(ChunkedUploadTencentCosStorageConfigure configure, IFileValidator fileValidator, CosXmlConfig cosXmlConfig, QCloudCredentialProvider qCloudCredentialProvider, IMemoryCache memoryCache, IFileNameGenerator fileNameGenerator, ISubdirectoryGenerator subdirectoryGenerator)
        {
            Configure = configure;
            FileValidator = fileValidator;
            MemoryCache = memoryCache;
            FileNameGenerator = fileNameGenerator;
            SubdirectoryGenerator = subdirectoryGenerator;
            Client = new CosXmlServer(cosXmlConfig, qCloudCredentialProvider);
        }

        public async Task<(bool Success, UploadFileResult Result, string ErrorMessage)> Process(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, Stream fileStream, string extensionName, string localFileName,
            string sectionName, HttpRequest request)
        {
            if (!headers.TryGetValue(ConstConfigs.FileMd5HeaderKey, out var md5))
                return (false, null, $"未找到Header key:{ConstConfigs.FileMd5HeaderKey}.");

            if (string.IsNullOrWhiteSpace(md5))
                return (false, null, "文件MD5值不能为空.");

            if (md5.ToString().Length != 32)
                return (false, null, "不合法的MD5值.");

            var chunk = 0;

            if (headers.TryGetValue(ConstConfigs.ChunkHeaderKey, out var chunkValue))
            {
                if (string.IsNullOrWhiteSpace(chunkValue))
                    return (false, null, "分片索引值不能为空");
                chunk = int.Parse(chunkValue);
            }

            PartUploadNotes PartUploadNotes;
            byte[] signature = null;
            //只验证第一个分片
            if (chunk == 0)
            {
                var subDir = await SubdirectoryGenerator.Generate(query, form, headers, extensionName, request);
                var folder = Path.Combine(Configure.RootDirectory, subDir);

                var fileName = await FileNameGenerator.Generate(query, form, headers, extensionName, request) + extensionName;
                var url = Path.Combine(folder, fileName).Replace("\\", "/");

                var res = Client.InitMultipartUpload(
                    new COSXML.Model.Object.InitMultipartUploadRequest(Configure.Bucket, url));
                if (res.httpCode != 200)
                    return (false, null, res.httpMessage);

                PartUploadNotes = new PartUploadNotes
                {
                    UploadId = res.initMultipartUpload.uploadId,
                    Key = url
                };

                MemoryCache.Set(md5, PartUploadNotes, TimeSpan.FromHours(2));
                var (success, errorMsg, fileSignature) = await FileValidator.Validate(localFileName, fileStream);
                if (!success)
                    return (false, null, errorMsg);
                signature = fileSignature;
            }
            else
            {
                MemoryCache.TryGetValue(md5, out PartUploadNotes);
            }



            await using var stream = new MemoryStream();
            if (signature != null && signature.Length > 0)
                stream.Write(signature, 0, signature.Length);

            await fileStream.CopyToAsync(stream, Configure.BufferSize);
            stream.Seek(0, SeekOrigin.Begin);

            var fileBytes = new byte[stream.Length];
            await stream.ReadAsync(fileBytes);

            try
            {
                var resp = Client.UploadPart(new COSXML.Model.Object.UploadPartRequest(Configure.Bucket, PartUploadNotes.Key, chunk + 1, PartUploadNotes.UploadId, fileBytes));
                if (resp.httpCode != 200)
                    return (false, null, resp.httpMessage);

                PartUploadNotes.PartETag[chunk + 1] = resp.eTag;
                return (true, new UploadFileResult { Name = sectionName, Url = "" }, "");
            }
            catch (CosServerException e)
            {
                return (false, null, $"ErrorCode:{e.errorCode};ErrorMessage:{e.errorMessage}");
            }
        }
    }
}
