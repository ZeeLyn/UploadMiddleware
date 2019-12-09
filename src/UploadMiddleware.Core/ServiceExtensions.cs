using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core.Generators;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.Core
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddUpload<TUploadProcessor>(this IServiceCollection services) where TUploadProcessor : IUploadProcessor
        {
            services.AddScoped(typeof(IUploadProcessor), typeof(TUploadProcessor));
            //默认上传结束handler
            services.AddScoped<IUploadCompletedHandler, UploadCompletedHandler>();
            services.AddSingleton<IFileValidator, FileValidator>();
            services.AddSingleton<IFileNameGenerator, FileNameGenerator>();
            services.AddSingleton<ISubdirectoryGenerator, SubdirectoryGenerator>();
            return services;
        }
    }
}
