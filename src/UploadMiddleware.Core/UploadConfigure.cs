using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.Core
{
    public class UploadConfigure
    {
        public UploadConfigure(IServiceCollection services)
        {
            Services = services;
        }
        protected internal IServiceCollection Services { get; }

        public long MultipartBodyLengthLimit { get; set; }

        public HashSet<string> AllowFileExtension { get; } = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif" };

        public Func<HttpRequest, string, string> SubdirectoryGenerator { get; set; } //= (request, extensionName) => Path.Combine(DateTime.Now.ToString("yyyyMMdd"), "img");

        public Func<HttpRequest, string, string> FileNameGenerator { get; set; } =
           (request, extensionName) => Guid.NewGuid().ToString("N");

        public Func<HttpContext, bool> AuthorizationFilter { get; set; }

        public int BufferSize { get; set; } = 1024;

        /// <summary>
        /// 添加自定义上传完成结果组装Handler
        /// </summary>
        /// <typeparam name="TUploadCompletedHandler"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddUploadCompletedHandler<TUploadCompletedHandler>() where TUploadCompletedHandler : IUploadCompletedHandler
        {
            return Services.AddScoped(typeof(IUploadCompletedHandler), typeof(TUploadCompletedHandler));
        }

        /// <summary>
        /// 添加自定义上传处理器
        /// </summary>
        /// <typeparam name="TUploadProcessor"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddUploadProcessor<TUploadProcessor>() where TUploadProcessor : IUploadProcessor
        {
            return Services.AddScoped(typeof(IUploadProcessor), typeof(TUploadProcessor));
        }
    }
}
