using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using COSXML;
using COSXML.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.TencentCOS
{
    public class TencentCosStorageCheckChunkProcessor : ICheckChunkProcessor
    {
        private static readonly MD5 Md5 = MD5.Create();
        private ChunkedUploadTencentCosStorageConfigure Configure { get; }
        private IMemoryCache MemoryCache { get; }
        private CosXml Client { get; }
        public TencentCosStorageCheckChunkProcessor(ChunkedUploadTencentCosStorageConfigure configure, CosXmlConfig cosXmlConfig, QCloudCredentialProvider qCloudCredentialProvider, IMemoryCache memoryCache)
        {
            Configure = configure;
            MemoryCache = memoryCache;
            Client = new CosXmlServer(cosXmlConfig, qCloudCredentialProvider);
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

            if (!MemoryCache.TryGetValue(md5, out PartUploadNotes upload))
            {
                return await Task.FromResult(new ResponseResult
                {
                    Content = new { exist = false }
                });

            }
            try
            {
                var resp = Client.ListParts(new COSXML.Model.Object.ListPartsRequest(Configure.Bucket, upload.Key, upload.UploadId));
                if (resp.httpCode != 200)
                    return await Task.FromResult(new ResponseResult
                    {
                        Content = new { exist = false }
                    });
                return await Task.FromResult(new ResponseResult
                {
                    Content = new { exist = resp.listParts.parts.Any(p => p.partNumber == (chunk + 1).ToString()) }
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
