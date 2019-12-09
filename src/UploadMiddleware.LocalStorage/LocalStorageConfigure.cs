using System.IO;
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
        /// <summary>
        /// 
        /// </summary>
        public string SaveRootDirectory { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "upload");

    }

    public class ChunkedUploadLocalStorageConfigure : LocalStorageConfigure
    {
        public ChunkedUploadLocalStorageConfigure(IServiceCollection services) : base(services)
        {
        }
        public string ChunksFormName { get; set; } = "chunks";

        public string ChunkFormName { get; set; } = "chunk";

        public string FileMd5FormName { get; set; } = "md5";

        public string ChunkMd5FormName { get; set; } = "chunk_md5";

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
        /// 添加合并完成结果组装Handler
        /// </summary>
        /// <typeparam name="TMergeHandler"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddMergeHandler<TMergeHandler>() where TMergeHandler : IMergeHandler
        {
            return Services.AddScoped(typeof(IMergeHandler), typeof(TMergeHandler));
        }

    }
}
