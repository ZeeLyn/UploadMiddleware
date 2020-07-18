using System;
using COSXML;
using COSXML.Auth;
using COSXML.Model.Object;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.TencentCOS
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddUploadTencentCOS(this IServiceCollection services, Action<TencentCosStorageConfigure> options)
        {
            services.AddUpload<TencentCosStorageUploadProcessor>();
            var config = new TencentCosStorageConfigure(services);
            options?.Invoke(config);
            services.AddSingleton(config);

            services.AddSingleton(new CosXmlConfig.Builder()
                .SetConnectionTimeoutMs(60000) //设置连接超时时间，单位毫秒，默认45000ms
                .SetReadWriteTimeoutMs(40000) //设置读写超时时间，单位毫秒，默认45000ms
                .IsHttps(true) //设置默认 HTTPS 请求
                .SetAppid(config.AppId) //设置腾讯云账户的账户标识 APPID
                .SetRegion(config.Region) //设置一个默认的存储桶地域
                .SetDebugLog(false) //显示日志
                .Build());

            services.AddScoped<QCloudCredentialProvider>(sc => new DefaultQCloudCredentialProvider(config.SecretId, config.SecretKey, 600));
            services.AddSingleton<UploadConfigure>(config);
            return services;
        }
        public static IServiceCollection AddChunkedUploadTencentCOS(this IServiceCollection services, Action<ChunkedUploadTencentCosStorageConfigure> options)
        {
            services.AddUpload<TencentCosStorageChunkedUploadProcessor>();
            services.AddScoped(typeof(IMergeHandler), typeof(MergeHandler));
            services.AddScoped<ICheckChunksProcessor, TencentCosStorageCheckChunksProcessor>();
            services.AddScoped<ICheckChunkProcessor, TencentCosStorageCheckChunkProcessor>();
            services.AddScoped<IUploadCompletedHandler, ChunkUploadCompletedHandler>();
            services.AddScoped<IMergeProcessor, TencentCosStorageMergeProcessor>();
            var config = new ChunkedUploadTencentCosStorageConfigure(services);
            options?.Invoke(config);
            if (string.IsNullOrWhiteSpace(config.RootDirectory))
                throw new ArgumentNullException(nameof(config.RootDirectory));
            services.AddSingleton<UploadConfigure>(config);
            services.AddMemoryCache();
            //services.AddSingleton(new MemoryCache(new MemoryCacheOptions()));
            services.AddSingleton(new CosXmlConfig.Builder()
                .SetConnectionTimeoutMs(60000) //设置连接超时时间，单位毫秒，默认45000ms
                .SetReadWriteTimeoutMs(40000) //设置读写超时时间，单位毫秒，默认45000ms
                .IsHttps(true) //设置默认 HTTPS 请求
                .SetAppid(config.AppId) //设置腾讯云账户的账户标识 APPID
                .SetRegion(config.Region) //设置一个默认的存储桶地域
                .SetDebugLog(false) //显示日志
                .Build());

            services.AddScoped<QCloudCredentialProvider>(sc => new DefaultQCloudCredentialProvider(config.SecretId, config.SecretKey, 600));
            return services.AddSingleton(config);
        }
    }
}
