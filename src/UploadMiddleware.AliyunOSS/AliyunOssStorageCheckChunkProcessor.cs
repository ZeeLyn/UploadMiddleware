using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Aliyun.OSS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.AliyunOSS
{
    public class AliyunOssStorageCheckChunkProcessor : ICheckChunkProcessor
    {
        private static readonly MD5 Md5 = MD5.Create();
        private ChunkedUploadAliyunOssStorageConfigure Configure { get; }
        private IMemoryCache MemoryCache { get; }
        private IOss Client { get; }
        public AliyunOssStorageCheckChunkProcessor(ChunkedUploadAliyunOssStorageConfigure configure, IOss client, IMemoryCache memoryCache)
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

            if (!headers.TryGetValue(ConstConfigs.ChunkMd5HeaderKey, out var chunkMd5) || string.IsNullOrWhiteSpace(chunkMd5))
            {
                return await Task.FromResult(new ResponseResult
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMsg = "The md5 value of the chunk cannot be empty."
                });
            }
            if (chunkMd5.ToString().Length != 32)
            {
                return await Task.FromResult(new ResponseResult
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMsg = "不合法的MD5值"
                });
            }

            if (!headers.TryGetValue(ConstConfigs.ChunkHeaderKey, out var chunkValue) || string.IsNullOrWhiteSpace(chunkValue) || !int.TryParse(chunkValue, out var chunk))
            {
                return await Task.FromResult(new ResponseResult
                {
                    StatusCode = System.Net.HttpStatusCode.BadRequest,
                    ErrorMsg = "分片索引不能为空"
                });
            }

            if (!MemoryCache.TryGetValue(md5, out PartUploadRecording upload))
            {
                return await Task.FromResult(new ResponseResult
                {
                    Content = new { exist = false }
                });

            }
            try
            {
                var resp = Client.ListParts(new ListPartsRequest(Configure.BucketName, upload.Key, upload.UploadId));
                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    return await Task.FromResult(new ResponseResult
                    {
                        Content = new { exist = false }
                    });
                return await Task.FromResult(new ResponseResult
                {
                    Content = new { exist = resp.Parts.Any(p => p.PartETag.PartNumber == chunk + 1) }
                });
            }
            catch
            {
                return await Task.FromResult(new ResponseResult
                {
                    Content = new { exist = false }
                });
            }
        }
    }
}
