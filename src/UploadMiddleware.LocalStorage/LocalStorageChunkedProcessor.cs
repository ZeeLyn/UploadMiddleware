using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.LocalStorage
{
    public class LocalStorageChunkedProcessor : IUploadProcessor
    {
        public Dictionary<string, string> FormData { get; } = new Dictionary<string, string>();

        public List<UploadFileResult> FileData { get; } = new List<UploadFileResult>();

        public Dictionary<string, string> QueryData { get; } = new Dictionary<string, string>();

        private ChunkedUploadLocalStorageConfigure Configure { get; }

        private const string TempFolder = "chunks";

        public LocalStorageChunkedProcessor(ChunkedUploadLocalStorageConfigure configure)
        {
            Configure = configure;
        }

        public async Task ProcessFile(Stream fileStream, string extensionName, HttpRequest request, string localFileName,
            string sectionName)
        {
            if (!FormData.TryGetValue(Configure.FileMd5FormName, out var md5))
                throw new ArgumentException($"未找到表单{Configure.FileMd5FormName}");
            if (string.IsNullOrWhiteSpace(md5))
                throw new ArgumentNullException(Configure.FileMd5FormName);
            if (md5.Length != 32)
                throw new ArgumentException("不合法的MD5值");

            if (!FormData.TryGetValue(Configure.ChunkFormName, out var chunkValue))
                throw new ArgumentException($"未找到表单{Configure.ChunkFormName}");
            if (string.IsNullOrWhiteSpace(chunkValue))
                throw new ArgumentNullException(Configure.ChunkFormName);

            var chunk = int.Parse(chunkValue);

            var chunksFolder = Path.Combine(Configure.SaveRootDirectory, TempFolder, md5);
            if (!Directory.Exists(chunksFolder))
                Directory.CreateDirectory(chunksFolder);
            var fileName = chunk + extensionName + ".$chunk";
            var url = Path.Combine(chunksFolder, fileName);
            await using var writeStream = new FileStream(url, FileMode.Create);
            await fileStream.CopyToAsync(writeStream, Configure.BufferSize);
            FileData.Add(new UploadFileResult { Name = sectionName, Url = Path.Combine("/", TempFolder, md5, fileName).Replace("\\", "/") });
        }
    }
}
