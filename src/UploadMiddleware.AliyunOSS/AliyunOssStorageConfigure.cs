using System.Collections.Generic;
using Aliyun.OSS;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.AliyunOSS
{
    public class AliyunOssStorageConfigure : UploadConfigure
    {
        public AliyunOssStorageConfigure(IServiceCollection services) : base(services)
        {
            RootDirectory = "middleware/upload";
        }

        /// <summary>
        /// OSS access key Id
        /// </summary>
        public string AccessId { get; set; }

        /// <summary>
        /// OSS key secret
        /// </summary>
        public string AccessKeySecret { get; set; }
        /// <summary>
        /// OSS endpoint
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// OSS bucket name
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// STS security token
        /// </summary>
        public string SecurityToken { get; set; }

        /// <summary>
        /// 设置文件的元数据，每种文件格式可以单独配置,可使用以下占位符:
        /// $(form:name):   表单数据，name是具体的form表单key;
        /// $(query:name):   query参数，name是具体的query参数key;
        /// $LocalFileName:  本地文件名;
        /// $SectionName:  MultipartBody的name值;
        /// </summary>
        public Dictionary<string, ObjectMetadata> Metadata { get; } = new Dictionary<string, ObjectMetadata>
        {
            {
                ".jpg",
                new ObjectMetadata {ContentType = "image/jpeg"}
            },
            {".jpeg", new ObjectMetadata {ContentType = "image/jpeg"}},
            {".gif", new ObjectMetadata {ContentType = "image/gif"}},
            {".png", new ObjectMetadata {ContentType = "image/png"}},
            {".mp4", new ObjectMetadata {ContentType = "video/mpeg4"}}
        };
    }

    public class ChunkedUploadAliyunOssStorageConfigure : AliyunOssStorageConfigure
    {
        public ChunkedUploadAliyunOssStorageConfigure(IServiceCollection services) : base(services)
        {

        }

        /// <summary>
        /// 添加自定义分片数量检测器
        /// </summary>
        /// <typeparam name="TCheckChunksProcessor"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddCheckChunksProcessor<TCheckChunksProcessor>() where TCheckChunksProcessor : ICheckChunksProcessor
        {
            return Services.AddScoped(typeof(ICheckChunksProcessor), typeof(TCheckChunksProcessor));
        }

        /// <summary>
        /// 添加自定义分片完整性检测器
        /// </summary>
        /// <typeparam name="TCheckChunkProcessor"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddCheckChunkProcessor<TCheckChunkProcessor>() where TCheckChunkProcessor : ICheckChunkProcessor
        {
            return Services.AddScoped(typeof(ICheckChunkProcessor), typeof(TCheckChunkProcessor));
        }

        /// <summary>
        /// 添加自定义分片合并器
        /// </summary>
        /// <typeparam name="TMergeProcessor"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddMergeProcessor<TMergeProcessor>() where TMergeProcessor : IMergeProcessor
        {
            return Services.AddScoped(typeof(IMergeProcessor), typeof(TMergeProcessor));
        }

        /// <summary>
        /// 添加分片合并完成返回结果组装Handler
        /// </summary>
        /// <typeparam name="TMergeHandler"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddMergeHandler<TMergeHandler>() where TMergeHandler : IMergeHandler
        {
            return Services.AddScoped(typeof(IMergeHandler), typeof(TMergeHandler));
        }

    }

    public class PartUploadRecording
    {
        public string UploadId { get; set; }

        public string Key { get; set; }

        public string LocalFileName { get; set; }

        public List<PartETag> PartETag { get; set; } = new List<PartETag>();
    }

}
