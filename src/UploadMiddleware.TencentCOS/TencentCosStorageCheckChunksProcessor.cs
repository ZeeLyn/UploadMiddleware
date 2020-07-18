using System.IO;
using System.Threading.Tasks;
using COSXML;
using COSXML.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.TencentCOS
{
    public class TencentCosStorageCheckChunksProcessor : ICheckChunksProcessor
    {
        private ChunkedUploadTencentCosStorageConfigure Configure { get; }

        private IMemoryCache MemoryCache { get; }
        private CosXml Client { get; }

        public TencentCosStorageCheckChunksProcessor(ChunkedUploadTencentCosStorageConfigure configure, CosXmlConfig cosXmlConfig, QCloudCredentialProvider qCloudCredentialProvider, IMemoryCache memoryCache)
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
            if (!MemoryCache.TryGetValue(md5, out PartUploadNotes upload))
            {
                return await Task.FromResult(new ResponseResult
                {
                    Content = new { chunks = 0 }
                });

            }
            try
            {
                var resp = Client.ListParts(new COSXML.Model.Object.ListPartsRequest(Configure.Bucket, upload.Key, upload.UploadId));

                return await Task.FromResult(new ResponseResult
                {
                    Content = new { chunks = resp.listParts.parts.Count }
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
