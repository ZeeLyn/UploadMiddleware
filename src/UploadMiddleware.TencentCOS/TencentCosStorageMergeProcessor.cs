using System.Threading.Tasks;
using COSXML;
using COSXML.CosException;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.TencentCOS
{
    public class TencentCosStorageMergeProcessor : IMergeProcessor
    {
        private ChunkedUploadTencentCosStorageConfigure Configure { get; }


        private CosXml Client { get; }

        private IMemoryCache MemoryCache { get; }

        public TencentCosStorageMergeProcessor(ChunkedUploadTencentCosStorageConfigure configure, CosXml client, IMemoryCache memoryCache)
        {
            Configure = configure;
            MemoryCache = memoryCache;
            Client = client;
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


            if (!MemoryCache.TryGetValue(md5, out PartUploadRecording upload))
                return (false, "", "请先上传分片再合并");
            try
            {
                var req =
                    new COSXML.Model.Object.CompleteMultipartUploadRequest(Configure.Bucket, upload.Key,
                        upload.UploadId);
                req.SetPartNumberAndETag(upload.PartETag);
                var resp = Client.CompleteMultiUpload(req);

                var serverPath = "/" + upload.Key;
                try
                {
                    var callback = request.HttpContext.RequestServices.GetService<IUploadCompletedCallbackHandler>();
                    if (callback != null)
                        await callback.OnCompletedAsync(serverPath, upload.LocalFileName);
                }
                finally
                {
                    if (resp.httpCode == 200)
                        MemoryCache.Remove(md5);
                }

                return (true, serverPath, "");
            }
            catch (CosServerException e)
            {
                return (false, "", $"ErrorCode:{e.errorCode};ErrorMessage:{e.errorMessage}");
            }

        }
    }
}
