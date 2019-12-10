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
        /// 传输分片数量的表单name（默认：chunks）
        /// </summary>
        public string ChunksFormName { get; set; } = "chunks";

        /// <summary>
        /// 传输分片索引的表单name,分片索引从0开始（默认：chunk）
        /// </summary>
        public string ChunkFormName { get; set; } = "chunk";

        /// <summary>
        /// 传输文件的MD5值的表单name，注意是文件不是分片(默认：md5)
        /// </summary>
        public string FileMd5FormName { get; set; } = "md5";

        /// <summary>
        /// 传输分片的MD5值的表单name（默认：chunk_md5）
        /// </summary>
        public string ChunkMd5FormName { get; set; } = "chunk_md5";

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
