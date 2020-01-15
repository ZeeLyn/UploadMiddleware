using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core.Generators;
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
        /// Multipart Body的上限,Kestrel服务下才起作用,IIS下请在web.config里配置
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
        public string RootDirectory { get; set; } = Path.Combine(AppContext.BaseDirectory, "upload");

        /// <summary>
        /// 允许上传的文件格式(以"."开头)
        /// </summary>
        public HashSet<string> AllowFileExtension { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif" };

        /// <summary>
        /// 添加允许上传的文件格式(以"."开头)，默认支持.jpg,.jpeg,.png,.gif
        /// </summary>
        /// <param name="ext"></param>
        public void AddAllowFileExtension(params string[] ext)
        {
            foreach (var f in ext)
            {
                var extString = f.StartsWith(".") ? f : "." + f;
                if (AllowFileExtension.Contains(extString))
                    continue;
                AllowFileExtension.Add(extString);
            }
        }

        /// <summary>
        /// 缓冲池大小（默认64KB）,推荐不要超过64KB，超过后会写磁盘
        /// </summary>
        public int BufferSize { get; set; } = 1024 * 64;

        /// <summary>
        /// 添加自定义(文件/分片)上传完成返回结果组装Handler
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
            return Services.AddSingleton(typeof(IFileValidator), typeof(TFileValidator));
        }

        /// <summary>
        /// 添加子目录生成器
        /// </summary>
        /// <typeparam name="TSubdirectoryGenerator"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddSubdirectoryGenerator<TSubdirectoryGenerator>() where TSubdirectoryGenerator : ISubdirectoryGenerator
        {
            return Services.AddSingleton(typeof(ISubdirectoryGenerator), typeof(TSubdirectoryGenerator));
        }

        /// <summary>
        /// 添加文件名生成器
        /// </summary>
        /// <typeparam name="TFileNameGenerator"></typeparam>
        /// <returns></returns>
        public IServiceCollection AddFileNameGenerator<TFileNameGenerator>() where TFileNameGenerator : IFileNameGenerator
        {
            return Services.AddSingleton(typeof(IFileNameGenerator), typeof(TFileNameGenerator));
        }
    }
}
