using System;
using Aliyun.OSS;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;


namespace UploadMiddleware.AliyunOSS
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddUploadAliyunOSS(this IServiceCollection services, Action<AliyunOssStorageConfigure> options)
        {
            services.AddUpload<AliyunOssStorageUploadProcessor>();
            var config = new AliyunOssStorageConfigure(services);
            options?.Invoke(config);
            services.AddSingleton(config);
            services.AddSingleton<IOss>(string.IsNullOrWhiteSpace(config.SecurityToken) ? new OssClient(config.Endpoint, config.AccessId, config.AccessKeySecret) : new OssClient(config.Endpoint, config.AccessId, config.AccessKeySecret, config.SecurityToken));
            services.AddSingleton<UploadConfigure>(config);
            return services;
        }

        public static IServiceCollection AddUploadAliyunOSS(this IServiceCollection services)
        {
            return services.AddUploadAliyunOSS(options => { });
        }

        public static IServiceCollection AddChunkedUploadAliyunOSS(this IServiceCollection services, Action<ChunkedUploadAliyunOssStorageConfigure> options)
        {
            services.AddUpload<AliyunOssStorageChunkedUploadProcessor>();
            services.AddScoped(typeof(IMergeHandler), typeof(MergeHandler));
            services.AddScoped<ICheckChunksProcessor, AliyunOssStorageCheckChunksProcessor>();
            services.AddScoped<ICheckChunkProcessor, AliyunOssStorageCheckChunkProcessor>();
            services.AddScoped<IUploadCompletedHandler, ChunkUploadCompletedHandler>();
            services.AddScoped<IMergeProcessor, AliyunOssStorageMergeProcessor>();
            var config = new ChunkedUploadAliyunOssStorageConfigure(services);
            options?.Invoke(config);
            services.AddSingleton(config);
            services.AddSingleton<IOss>(string.IsNullOrWhiteSpace(config.SecurityToken) ? new OssClient(config.Endpoint, config.AccessId, config.AccessKeySecret) : new OssClient(config.Endpoint, config.AccessId, config.AccessKeySecret, config.SecurityToken));
            services.AddSingleton<UploadConfigure>(config);
            return services;
        }
    }
}
