using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.LocalStorage
{
    public class LocalStorageCheckChunksProcessor : ICheckChunksProcessor
    {
        private ChunkedUploadLocalStorageConfigure Configure { get; }

        public LocalStorageCheckChunksProcessor(ChunkedUploadLocalStorageConfigure configure)
        {
            Configure = configure;
        }

        //public Dictionary<string, string> FormData { get; } = new Dictionary<string, string>();

        public async Task<ResponseResult> Process(HttpRequest request, IQueryCollection query, IFormCollection form, IHeaderDictionary headers)
        {
            if (!headers.TryGetValue(ConstConfigs.FileMd5HeaderKey, out var md5) || string.IsNullOrWhiteSpace(md5))
            {
                return await Task.FromResult(new ResponseResult
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMsg = "The md5 value of the file cannot be empty."
                });
            }
            if (md5.ToString().Length != 32)
            {
                return await Task.FromResult(new ResponseResult
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMsg = "不合法的MD5值."
                });
            }
            var dir = Path.Combine(Configure.RootDirectory, "chunks", md5);
            if (!Directory.Exists(dir))
            {
                return await Task.FromResult(new ResponseResult
                {
                    Content = new { chunks = 0 }
                });
            }

            var dirInfo = new DirectoryInfo(dir);
            var files = dirInfo.GetFiles();
            var chunks = files.Length == 0 ? 0 : files.Length - 1;
            return await Task.FromResult(new ResponseResult
            {
                //最后一个文件可能因为中断损坏所以减1
                Content = new { chunks }
            });
        }
    }
}
