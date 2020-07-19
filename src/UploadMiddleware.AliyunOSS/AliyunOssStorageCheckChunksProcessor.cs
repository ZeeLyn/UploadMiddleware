using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aliyun.OSS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.AliyunOSS
{
    public class AliyunOssStorageCheckChunksProcessor : ICheckChunksProcessor
    {
        private ChunkedUploadAliyunOssStorageConfigure Configure { get; }

        private IMemoryCache MemoryCache { get; }
        private IOss Client { get; }

        public AliyunOssStorageCheckChunksProcessor(ChunkedUploadAliyunOssStorageConfigure configure, IOss client, IMemoryCache memoryCache)
        {
            Configure = configure;
            MemoryCache = memoryCache;
            Client = client;
        }

        //public Dictionary<string, string> FormData { get; } = new Dictionary<string, string>();

        public async Task<ResponseResult> Process(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, HttpRequest request)
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
            if (!MemoryCache.TryGetValue(md5, out PartUploadRecording upload))
            {
                return await Task.FromResult(new ResponseResult
                {
                    Content = new { chunks = 0 }
                });

            }
            try
            {
                var resp = Client.ListParts(new ListPartsRequest(Configure.BucketName, upload.Key, upload.UploadId));

                return await Task.FromResult(new ResponseResult
                {
                    Content = new { chunks = resp.Parts.Count() }
                });
            }
            catch
            {
                return await Task.FromResult(new ResponseResult
                {
                    Content = new { chunks = 0 }
                });
            }
        }
    }
}
