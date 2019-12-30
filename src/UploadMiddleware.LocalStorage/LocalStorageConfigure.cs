using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.LocalStorage
{
    public class LocalStorageConfigure : UploadConfigure
    {
        public LocalStorageConfigure(IServiceCollection services) : base(services)
        {

        }
    }

    public class ChunkedUploadLocalStorageConfigure : LocalStorageConfigure
    {
        public ChunkedUploadLocalStorageConfigure(IServiceCollection services) : base(services)
        {
        }

        /// <summary>
        /// 存放分片的跟目录，不设置则默认使用RootDirectory
        /// </summary>
        public string ChunksRootDirectory { get; set; }



        /// <summary>
        /// 当分片合并完成时，是否删除分片，(默认：True)
        /// </summary>
        public bool DeleteChunksOnMerged { get; set; } = true;

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
}
