using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using UploadMiddleware.Core.Handlers;
using UploadMiddleware.Core.Processors;

namespace UploadMiddleware.Core
{
    public class UploadMiddleware
    {
        private RequestDelegate Next { get; }

        private UploadOptions Options { get; }

        private IFileValidator FileValidator { get; }

        private UploadConfigure Configure { get; }

        public UploadMiddleware(RequestDelegate next, IOptions<UploadOptions> options, IFileValidator fileValidator, UploadConfigure configure)
        {
            Next = next;
            Options = options.Value;
            FileValidator = fileValidator;
            Configure = configure;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.Request.Path.Equals(Options.Route, StringComparison.CurrentCultureIgnoreCase))
            {
                await Next(context);
                return;
            }

            if (!HttpMethods.IsPost(context.Request.Method))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            if (!(Configure.AuthorizationFilter?.Invoke(context) ?? true))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return;
            }

            //await Next(context);

            context.Request.Query.TryGetValue("action", out var action);

            switch (action.ToString().ToLower())
            {
                #region 检查已经上传的分片数量
                case "chunks":
                    var checker = context.RequestServices.GetRequiredService<ICheckChunksProcessor>();

                    foreach (var (key, value) in context.Request.Form)
                    {
                        checker.FormData.Add(key, value);
                    }

                    var checkResult = await checker.Process();
                    context.Response.StatusCode = (int)checkResult.StatusCode;
                    context.Response.ContentType = checkResult.ContextType;
                    if (checkResult.Headers != null && checkResult.Headers.Any())
                    {
                        foreach (var (key, value) in checkResult.Headers)
                        {
                            context.Response.Headers[key] = value;
                        }
                    }
                    await context.Response.WriteAsync(checkResult.Content);
                    break;
                #endregion

                #region 检查分片完整性
                case "chunk":
                    var chunkChecker = context.RequestServices.GetRequiredService<ICheckChunkProcessor>();
                    foreach (var (key, value) in context.Request.Form)
                    {
                        chunkChecker.FormData.Add(key, value);
                    }
                    var chunkCheckResult = await chunkChecker.Process();
                    context.Response.StatusCode = (int)chunkCheckResult.StatusCode;
                    context.Response.ContentType = chunkCheckResult.ContextType;
                    if (chunkCheckResult.Headers != null && chunkCheckResult.Headers.Any())
                    {
                        foreach (var (key, value) in chunkCheckResult.Headers)
                        {
                            context.Response.Headers[key] = value;
                        }
                    }
                    await context.Response.WriteAsync(chunkCheckResult.Content);
                    break;
                #endregion

                #region 合并分片
                case "merge":
                    var merger = context.RequestServices.GetRequiredService<IMergeProcessor>();
                    foreach (var (key, value) in context.Request.Query)
                    {
                        merger.QueryData[key] = value;
                    }
                    foreach (var item in context.Request.Form)
                    {
                        merger.FormData.Add(item.Key, item.Value);
                    }

                    try
                    {
                        var fileName = await merger.Process(context.Request);
                        var mergeHandler = context.RequestServices.GetRequiredService<IMergeHandler>();
                        var mergerResult = await mergeHandler.OnCompleted(merger.FormData, merger.QueryData, fileName, context.Request);
                        context.Response.StatusCode = (int)mergerResult.StatusCode;
                        context.Response.ContentType = mergerResult.ContextType;
                        if (mergerResult.Headers != null && mergerResult.Headers.Any())
                        {
                            foreach (var (key, value) in mergerResult.Headers)
                            {
                                context.Response.Headers[key] = value;
                            }
                        }
                        await context.Response.WriteAsync(mergerResult.Content);
                    }
                    catch (Exception e)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync($"{{\"errorMsg\":\"{e.Message}\"}}");
                    }
                    break;
                #endregion

                #region 上传
                default:
                    try
                    {
                        if (!MediaTypeHeaderValue.TryParse(context.Request.ContentType, out var contentType))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                            await context.Response.WriteAsync("ContentType must be multipart/form-data.");
                            return;
                        }
                        if (!contentType.MediaType.Equals("multipart/form-data", StringComparison.CurrentCultureIgnoreCase))
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                            await context.Response.WriteAsync("ContentType must be multipart/form-data.");
                            return;
                        }
                        var processor = context.RequestServices.GetRequiredService<IUploadProcessor>();

                        foreach (var (key, value) in context.Request.Query)
                        {
                            processor.QueryData[key] = value;
                        }
                        var boundary = context.Request.GetMultipartBoundary();
                        var reader = new MultipartReader(boundary, context.Request.Body);
                        var section = await reader.ReadNextSectionAsync();

                        while (section != null)
                        {
                            var header = section.GetContentDispositionHeader();
                            if (header != null)
                            {
                                if (header.FileName.HasValue || header.FileNameStar.HasValue)
                                {
                                    var fileSection = section.AsFileSection();
                                    var extensionName = Path.GetExtension(fileSection.FileName);
                                    await processor.ProcessFile(fileSection.FileStream, extensionName, context.Request, fileSection.FileName, fileSection.Name);
                                }
                                else
                                {
                                    var formSection = section.AsFormDataSection();
                                    processor.FormData[formSection.Name] = await formSection.GetValueAsync();
                                }
                            }
                            section = await reader.ReadNextSectionAsync();
                        }


                        if (!processor.FileData.Any())
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"errorMsg\":\"未发现上传文件\"}");
                            return;
                        }

                        var completeHandler = context.RequestServices.GetRequiredService<IUploadCompletedHandler>();
                        var result = await completeHandler.OnCompleted(processor.FormData, processor.FileData, processor.QueryData, context.Request);
                        context.Response.StatusCode = (int)result.StatusCode;
                        context.Response.ContentType = result.ContextType;
                        if (result.Headers != null && result.Headers.Any())
                        {
                            foreach (var (key, value) in result.Headers)
                            {
                                context.Response.Headers[key] = value;
                            }
                        }
                        await context.Response.WriteAsync(result.Content);
                    }
                    catch (Exception e)
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync($"{{\"errorMsg\":\"{e.Message}\"}}");
                    }
                    break;
                    #endregion
            }
        }
    }
}
