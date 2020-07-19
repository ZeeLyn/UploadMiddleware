using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Aliyun.OSS;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Generators;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.AliyunOSS
{
    public class AliyunOssStorageMergeProcessor : IMergeProcessor
    {
        private ChunkedUploadAliyunOssStorageConfigure Configure { get; }


        private IOss Client { get; }

        private IMemoryCache MemoryCache { get; }

        public AliyunOssStorageMergeProcessor(ChunkedUploadAliyunOssStorageConfigure configure, IOss client, IMemoryCache memoryCache)
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
                var req = new CompleteMultipartUploadRequest(Configure.BucketName, upload.Key,
                        upload.UploadId);
                foreach (var etag in upload.PartETag)
                {
                    req.PartETags.Add(etag);
                }
                var resp = Client.CompleteMultipartUpload(req);
                if (resp.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    return (false, "", resp.HttpStatusCode.ToString());
                MemoryCache.Remove(md5);
                await Task.CompletedTask;
                return (true, "/" + upload.Key, "");
            }
            catch (Exception e)
            {
                return (false, "", e.Message);
            }

        }
    }
}
