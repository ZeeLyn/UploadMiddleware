using System;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.LocalStorage
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddUploadLocalStorage(this IServiceCollection services, Action<LocalStorageConfigure> options)
        {
            services.AddUpload<LocalStorageUploadProcessor>();
            var config = new LocalStorageConfigure(services);
            options?.Invoke(config);
            services.AddSingleton<UploadConfigure>(config);
            return services.AddSingleton(config);
        }

        public static IServiceCollection AddUploadLocalStorage(this IServiceCollection services)
        {
            return services.AddUploadLocalStorage(options => { });
        }


        public static IServiceCollection AddChunkedUploadLocalStorage(this IServiceCollection services, Action<ChunkedUploadLocalStorageConfigure> options)
        {
            services.AddUpload<LocalStorageChunkedUploadProcessor>();
            services.AddScoped(typeof(IMergeHandler), typeof(MergeHandler));
            services.AddScoped<ICheckChunksProcessor, LocalStorageCheckChunksProcessor>();
            services.AddScoped<ICheckChunkProcessor, LocalStorageCheckChunkProcessor>();
            services.AddScoped<IUploadCompletedHandler, ChunkUploadCompletedHandler>();
            services.AddScoped<IMergeProcessor, LocalStorageMergeProcessor>();
            var config = new ChunkedUploadLocalStorageConfigure(services);
            options?.Invoke(config);
            if (string.IsNullOrWhiteSpace(config.RootDirectory))
                throw new ArgumentNullException(nameof(config.RootDirectory));
            if (string.IsNullOrWhiteSpace(config.ChunkFormName))
                throw new ArgumentNullException(config.ChunkFormName);
            if (string.IsNullOrWhiteSpace(config.ChunksFormName))
                throw new ArgumentNullException(config.ChunksFormName);
            services.AddSingleton<UploadConfigure>(config);
            return services.AddSingleton(config);
        }

        public static IServiceCollection AddChunkedUploadLocalStorage(this IServiceCollection services)
        {
            return services.AddChunkedUploadLocalStorage(options => { });
        }
    }
}
