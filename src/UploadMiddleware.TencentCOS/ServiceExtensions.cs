using System;
using COSXML;
using COSXML.Auth;
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

            services.RegClient(config);

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
            services.RegClient(config);
            return services.AddSingleton(config);
        }

        private static void RegClient(this IServiceCollection services, TencentCosStorageConfigure configure)
        {
            services.AddSingleton(new CosXmlConfig.Builder()
                .SetConnectionTimeoutMs(60000) //设置连接超时时间，单位毫秒，默认45000ms
                .SetReadWriteTimeoutMs(40000) //设置读写超时时间，单位毫秒，默认45000ms
                .IsHttps(true) //设置默认 HTTPS 请求
                .SetAppid(configure.AppId) //设置腾讯云账户的账户标识 APPID
                .SetRegion(configure.Region) //设置一个默认的存储桶地域
                .SetDebugLog(false) //显示日志
                .Build());
            services.AddSingleton<QCloudCredentialProvider>(sc => new DefaultQCloudCredentialProvider(configure.SecretId, configure.SecretKey, 600));
            services.AddScoped(sc => new CosXmlServer(sc.GetRequiredService<CosXmlConfig>(), sc.GetRequiredService<QCloudCredentialProvider>()));
        }
    }
}
