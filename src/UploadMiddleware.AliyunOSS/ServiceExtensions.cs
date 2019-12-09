using System;
using Aliyun.OSS;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core;


namespace UploadMiddleware.AliyunOSS
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddUploadAliyunOSS(this IServiceCollection services, Action<AliyunOssStorageConfigure> options)
        {
            services.AddUpload<AliyunOssStorageProcessor>();
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
    }
}
