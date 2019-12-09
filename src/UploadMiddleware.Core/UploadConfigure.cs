using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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


        private long _multipartBodyLengthLimit;

        /// <summary>
        /// 允许上传的Body上限,Kestrel服务下才起作用
        /// </summary>
        public long MultipartBodyLengthLimit
        {
            get => _multipartBodyLengthLimit;
            set
            {
                _multipartBodyLengthLimit = value;
                Services.Configure<FormOptions>(configureOptions =>
                {
                    configureOptions.MultipartBodyLengthLimit = value;
                });
            }
        }

        /// <summary>
        /// 文件保存的根目录
        /// </summary>
        public string RootDirectory { get; set; } = Path.Combine(Directory.GetCurrentDirectory(), "upload");

        /// <summary>
        /// 允许上传的文件格式
        /// </summary>
        public HashSet<string> AllowFileExtension { get; } = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif" };

        /// <summary>
        /// 子目录名生成器
        /// </summary>
        public Func<HttpRequest, string, string> SubdirectoryGenerator { get; set; } //= (request, extensionName) => Path.Combine(DateTime.Now.ToString("yyyyMMdd"), "img");

        /// <summary>
        /// 文件名生成器（默认以返回GUID）
        /// </summary>
        public Func<HttpRequest, string, string> FileNameGenerator { get; set; } =
           (request, extensionName) => Guid.NewGuid().ToString("N");

        /// <summary>
        /// 授权过滤器
        /// </summary>
        public Func<HttpContext, bool> AuthorizationFilter { get; set; }

        /// <summary>
        /// 缓冲池大小（默认1MB）
        /// </summary>
        public int BufferSize { get; set; } = 1024 * 1024;

        /// <summary>
        /// 添加自定义(文件/分片)上传完成结果组装Handler
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

        /// <summary>
        /// 添加自定义文件格式验证器
        /// </summary>
        /// <typeparam name="TFileValidator"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddFileValidator<TFileValidator>() where TFileValidator : IFileValidator
        {
            return Services.AddScoped(typeof(IFileValidator), typeof(TFileValidator));
        }

    }
}
