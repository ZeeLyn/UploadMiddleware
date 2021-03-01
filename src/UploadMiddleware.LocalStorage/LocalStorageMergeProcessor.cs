using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Generators;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.LocalStorage
{
    public class LocalStorageMergeProcessor : IMergeProcessor
    {
        private ChunkedUploadLocalStorageConfigure Configure { get; }

        private IFileNameGenerator FileNameGenerator { get; }

        private ISubdirectoryGenerator SubdirectoryGenerator { get; }

        public LocalStorageMergeProcessor(ChunkedUploadLocalStorageConfigure configure, IFileNameGenerator fileNameGenerator, ISubdirectoryGenerator subdirectoryGenerator)
        {
            Configure = configure;
            FileNameGenerator = fileNameGenerator;
            SubdirectoryGenerator = subdirectoryGenerator;
        }


        public async Task<(bool Success, string FileName, string ErrorMsg)> Process(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, HttpRequest request)
        {
            if (!headers.TryGetValue(ConstConfigs.FileMd5HeaderKey, out var md5) || string.IsNullOrWhiteSpace(md5))
            {
                return (false, "", "The md5 value of the file cannot be empty.");
            }

            if (md5.ToString().Length != 32)
            {
                return (false, "", "不合法的MD5值.");
            }

            if (!headers.TryGetValue(ConstConfigs.ChunksHeaderKey, out var chunksValue) || string.IsNullOrWhiteSpace(chunksValue))
            {
                return (false, "", "The chunks value of the file cannot be empty.");
            }

            var chunks = int.Parse(chunksValue);
            var chunksDir = Path.Combine(string.IsNullOrWhiteSpace(Configure.ChunksRootDirectory) ? Configure.RootDirectory : Configure.ChunksRootDirectory, "chunks", md5);
            if (!Directory.Exists(chunksDir))
            {
                return (false, "", "请先上传文件.");
            }

            var dirInfo = new DirectoryInfo(chunksDir);
            var files = dirInfo.GetFiles().OrderBy(p => p.Name).ToList();
            if (files.Count == 0 || files.Count < chunks)
            {
                return (false, "", "文件分片数量不合法，无法合并.");
            }

            var extensionName = Path.GetExtension(files.First().Name.Replace(".$chunk", ""));
            var subDir = await SubdirectoryGenerator.Generate(query, form, headers, extensionName, request);
            var folder = Path.Combine(Configure.RootDirectory, subDir);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            var fileName = await FileNameGenerator.Generate(query, form, headers, extensionName, request) + extensionName;
            var url = Path.Combine(folder, fileName);
            await using var writeStream = new FileStream(url, FileMode.Append);
            {
                foreach (var file in files)
                {
                    await using var readStream = file.OpenRead();
                    await readStream.CopyToAsync(writeStream, Configure.BufferSize);
                }
            }

            var serverUrl = Path.Combine("/", subDir, fileName).Replace("\\", "/");

            var localFileName = "";
            var infoPath = Path.Combine(chunksDir, "info");
            if (File.Exists(infoPath))
                localFileName = await File.ReadAllTextAsync(infoPath);


            if (Configure.DeleteChunksOnMerged)
                Directory.Delete(chunksDir, true);

            try
            {
                var callback = request.HttpContext.RequestServices.GetService<IUploadCompletedCallbackHandler>();
                if (callback != null)
                    await callback.OnCompletedAsync(serverUrl, localFileName, request);
            }
            catch
            {
                // ignored
            }

            return (true, serverUrl, "");
        }
    }
}
