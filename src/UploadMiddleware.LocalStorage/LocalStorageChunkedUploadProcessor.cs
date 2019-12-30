using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.LocalStorage
{
    public class LocalStorageChunkedUploadProcessor : IUploadProcessor
    {

        public List<UploadFileResult> FileData { get; } = new List<UploadFileResult>();

        private ChunkedUploadLocalStorageConfigure Configure { get; }

        private IFileValidator FileValidator { get; }

        private const string TempFolder = "chunks";

        public LocalStorageChunkedUploadProcessor(ChunkedUploadLocalStorageConfigure configure, IFileValidator fileValidator)
        {
            Configure = configure;
            FileValidator = fileValidator;
        }

        public async Task<(bool Success, string ErrorMessage)> Process(HttpRequest request, IQueryCollection query, IFormCollection form, IHeaderDictionary headers, Stream fileStream, string extensionName, string localFileName,
            string sectionName)
        {
            if (!headers.TryGetValue(ConstConfigs.FileMd5HeaderKey, out var md5))
                return (false, $"未找到Header key:{ConstConfigs.FileMd5HeaderKey}.");

            if (string.IsNullOrWhiteSpace(md5))
                return (false, "文件MD5值不能为空.");

            if (md5.ToString().Length != 32)
                return (false, "不合法的MD5值.");

            var chunk = 0;

            if (headers.TryGetValue(ConstConfigs.ChunkHeaderKey, out var chunkValue))
            {
                if (string.IsNullOrWhiteSpace(chunkValue))
                    return (false, "分片索引值不能为空");
                chunk = int.Parse(chunkValue);
            }

            //if (!FormData.TryGetValue(Configure.ChunkFormName, out var chunkValue))
            //    return (false, $"未找到表单{Configure.ChunkFormName}.");



            //var chunk = int.Parse(chunkValue);

            byte[] signature = null;
            //只验证第一个分片
            if (chunk == 0)
            {
                var (success, errorMsg, fileSignature) = await FileValidator.Validate(localFileName, fileStream);
                if (!success)
                    return (false, errorMsg);
                signature = fileSignature;
            }

            var chunksFolder = Path.Combine(string.IsNullOrWhiteSpace(Configure.ChunksRootDirectory) ? Configure.RootDirectory : Configure.ChunksRootDirectory, TempFolder, md5);
            if (!Directory.Exists(chunksFolder))
                Directory.CreateDirectory(chunksFolder);
            var fileName = chunk + extensionName + ".$chunk";
            var url = Path.Combine(chunksFolder, fileName);
            await using var writeStream = new FileStream(url, FileMode.Create);
            if (signature != null && signature.Length > 0)
                writeStream.Write(signature, 0, signature.Length);
            await fileStream.CopyToAsync(writeStream, Configure.BufferSize);
            FileData.Add(new UploadFileResult { Name = sectionName, Url = Path.Combine("/", TempFolder, md5, fileName).Replace("\\", "/") });
            return (true, "");
        }
    }
}
