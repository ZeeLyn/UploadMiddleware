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
using UploadMiddleware.Core.Common;
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

            //if (!(Configure.AuthorizationFilter?.Invoke(context) ?? true))
            //{
            //    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    return;
            //}

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
                    await context.Response.WriteResponseAsync(checkResult.StatusCode, checkResult.ErrorMsg, checkResult.Content, checkResult.Headers);
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
                    await context.Response.WriteResponseAsync(chunkCheckResult.StatusCode, chunkCheckResult.ErrorMsg, chunkCheckResult.Content, chunkCheckResult.Headers);
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
                        var merge = await merger.Process(context.Request);
                        if (!merge.Success)
                        {
                            await context.Response.WriteResponseAsync(HttpStatusCode.BadRequest, merge.ErrorMsg);
                            return;
                        }
                        var mergeHandler = context.RequestServices.GetRequiredService<IMergeHandler>();
                        var mergerResult = await mergeHandler.OnCompleted(merger.FormData, merger.QueryData, merge.FileName, context.Request);
                        await context.Response.WriteResponseAsync(mergerResult.StatusCode, mergerResult.ErrorMsg, mergerResult.Content, mergerResult.Headers);
                    }
                    catch (Exception e)
                    {
                        await context.Response.WriteResponseAsync(HttpStatusCode.BadRequest, e.Message);
                    }
                    break;
                #endregion

                #region 上传
                default:
                    try
                    {
                        if (!MediaTypeHeaderValue.TryParse(context.Request.ContentType, out var contentType))
                        {
                            await context.Response.WriteResponseAsync(HttpStatusCode.UnsupportedMediaType, "ContentType must be multipart/form-data.");
                            return;
                        }
                        if (!contentType.MediaType.Equals("multipart/form-data", StringComparison.CurrentCultureIgnoreCase))
                        {
                            await context.Response.WriteResponseAsync(HttpStatusCode.UnsupportedMediaType, "ContentType must be multipart/form-data.");
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
                                    var (success, errorMessage) = await processor.Process(fileSection.FileStream, extensionName, context.Request, fileSection.FileName, fileSection.Name);
                                    if (!success)
                                    {
                                        await context.Response.WriteResponseAsync(HttpStatusCode.BadRequest, errorMessage);
                                        return;
                                    }
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
                            await context.Response.WriteResponseAsync(HttpStatusCode.BadRequest, "未发现上传文件");
                            return;
                        }

                        var completeHandler = context.RequestServices.GetRequiredService<IUploadCompletedHandler>();
                        var result = await completeHandler.OnCompleted(processor.FormData, processor.FileData, processor.QueryData, context.Request);
                        await context.Response.WriteResponseAsync(result.StatusCode, result.ErrorMsg, result.Content, result.Headers);
                    }
                    catch (Exception e)
                    {
                        await context.Response.WriteResponseAsync(HttpStatusCode.BadRequest, e.Message);
                    }
                    break;
                    #endregion
            }
        }
    }
}
