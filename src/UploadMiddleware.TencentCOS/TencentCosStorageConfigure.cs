using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.TencentCOS
{
    public class TencentCosStorageConfigure : UploadConfigure
    {
        public TencentCosStorageConfigure(IServiceCollection services) : base(services)
        {
            RootDirectory = "middleware/upload";
        }
        public string AppId { get; set; }

        public string Region { get; set; }

        public string SecretId { get; set; }

        public string SecretKey { get; set; }

        public string Bucket { get; set; }

    }


    public class ChunkedUploadTencentCosStorageConfigure : TencentCosStorageConfigure
    {
        public ChunkedUploadTencentCosStorageConfigure(IServiceCollection services) : base(services)
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

    public class PartUploadNotes
    {
        public string UploadId { get; set; }

        public string Key { get; set; }

        public Dictionary<int, string> PartETag { get; set; } = new Dictionary<int, string>();
    }
}
