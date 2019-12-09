using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.LocalStorage
{
    public class LocalStorageMergeProcessor : IMergeProcessor
    {
        private ChunkedUploadLocalStorageConfigure Configure { get; }

        public LocalStorageMergeProcessor(ChunkedUploadLocalStorageConfigure configure)
        {
            Configure = configure;
        }

        public Dictionary<string, string> FormData { get; } = new Dictionary<string, string>();

        public Dictionary<string, string> QueryData { get; } = new Dictionary<string, string>();

        public async Task<string> Process(HttpRequest request)
        {
            if (!FormData.TryGetValue(Configure.FileMd5FormName, out var md5) || string.IsNullOrWhiteSpace(md5))
            {
                throw new ArgumentException("The md5 value of the file cannot be empty.");
            }
            if (md5.Length != 32)
            {
                throw new ArgumentException("不合法的MD5值.");
            }

            if (!FormData.TryGetValue(Configure.ChunksFormName, out var chunksValue) || string.IsNullOrWhiteSpace(chunksValue))
            {
                throw new ArgumentException("The chunks value of the file cannot be empty.");
            }

            var chunks = int.Parse(chunksValue);
            var chunksDir = Path.Combine(Configure.SaveRootDirectory, "chunks", md5);
            if (!Directory.Exists(chunksDir))
            {
                throw new ArgumentException("请先上传文件");
            }

            var dirInfo = new DirectoryInfo(chunksDir);
            var files = dirInfo.GetFiles().OrderBy(p => p.Name).ToList();
            if (files.Count == 0 || files.Count < chunks)
            {
                throw new Exception("文件分片数量不合法，无法合并");
            }

            var extensionName = Path.GetExtension(files.First().Name.Replace(".$chunk", ""));
            var subDir = Configure.SubdirectoryGenerator?.Invoke(request, extensionName) ?? "";
            var folder = Path.Combine(Configure.SaveRootDirectory, subDir);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var fileName = Configure.FileNameGenerator.Invoke(request, extensionName) + extensionName;
            var url = Path.Combine(folder, fileName);
            await using var writeStream = new FileStream(url, FileMode.Append);
            {
                foreach (var file in files)
                {
                    await using var readStream = file.OpenRead();
                    await readStream.CopyToAsync(writeStream, Configure.BufferSize);
                }
            }
            if (Configure.DeleteChunksOnMerged)
                Directory.Delete(chunksDir, true);

            return Path.Combine(subDir, fileName).Replace("\\", "/");
        }
    }
}
