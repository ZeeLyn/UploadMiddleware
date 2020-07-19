using System;
using System.IO;
using System.Threading.Tasks;
using Aliyun.OSS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Generators;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.AliyunOSS
{
    public class AliyunOssStorageChunkedUploadProcessor : IUploadProcessor
    {
        private ChunkedUploadAliyunOssStorageConfigure Configure { get; }

        private IFileValidator FileValidator { get; }

        private IOss Client { get; }

        private IMemoryCache MemoryCache { get; }

        private IFileNameGenerator FileNameGenerator { get; }

        private ISubdirectoryGenerator SubdirectoryGenerator { get; }

        public AliyunOssStorageChunkedUploadProcessor(ChunkedUploadAliyunOssStorageConfigure configure, IFileValidator fileValidator, IOss client, IMemoryCache memoryCache, IFileNameGenerator fileNameGenerator, ISubdirectoryGenerator subdirectoryGenerator)
        {
            Configure = configure;
            FileValidator = fileValidator;
            MemoryCache = memoryCache;
            FileNameGenerator = fileNameGenerator;
            SubdirectoryGenerator = subdirectoryGenerator;
            Client = client;
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

            PartUploadRecording PartUploadRecording;
            byte[] signature = null;
            //只验证第一个分片
            if (chunk == 0)
            {
                var subDir = await SubdirectoryGenerator.Generate(query, form, headers, extensionName, request);
                var folder = Path.Combine(Configure.RootDirectory, subDir);

                var fileName = await FileNameGenerator.Generate(query, form, headers, extensionName, request) + extensionName;
                var url = Path.Combine(folder, fileName).Replace("\\", "/");

                var res = Client.InitiateMultipartUpload(new InitiateMultipartUploadRequest(Configure.BucketName, url));
                if (res.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    return (false, null, res.HttpStatusCode.ToString());

                PartUploadRecording = new PartUploadRecording
                {
                    UploadId = res.UploadId,
                    Key = url
                };

                MemoryCache.Set(md5, PartUploadRecording, TimeSpan.FromHours(2));
                var (success, errorMsg, fileSignature) = await FileValidator.Validate(localFileName, fileStream);
                if (!success)
                    return (false, null, errorMsg);
                signature = fileSignature;
            }
            else
            {
                if (!MemoryCache.TryGetValue(md5, out PartUploadRecording))
                {
                    return (false, null, "分片记录丢失");
                }
            }



            await using var stream = new MemoryStream();
            if (signature != null && signature.Length > 0)
                stream.Write(signature, 0, signature.Length);

            await fileStream.CopyToAsync(stream, Configure.BufferSize);
            stream.Seek(0, SeekOrigin.Begin);

            try
            {
                var resp = Client.UploadPart(new UploadPartRequest(Configure.BucketName, PartUploadRecording.Key, PartUploadRecording.UploadId)
                {
                    InputStream = stream,
                    PartSize = stream.Length,
                    PartNumber = chunk + 1
                });
                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    return (false, null, resp.HttpStatusCode.ToString());

                PartUploadRecording.PartETag.Add(resp.PartETag);
                return (true, new UploadFileResult { Name = sectionName, Url = "" }, "");
            }
            catch (Exception e)
            {
                return (false, null, e.Message);
            }
        }
    }
}
