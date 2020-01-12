using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
using Microsoft.Extensions.Primitives;
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
            if (!context.Request.Path.Equals(Options.Route, StringComparison.OrdinalIgnoreCase))
            {
                await Next(context);
                return;
            }

            if (!HttpMethods.IsPost(context.Request.Method))
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }



            context.Request.Query.TryGetValue("action", out var action);

            switch (action.ToString().ToLower())
            {
                #region 检查已经上传的分片数量
                case "chunks":
                    var checker = context.RequestServices.GetService<ICheckChunksProcessor>();
                    if (checker == null)
                    {
                        await context.Response.WriteResponseAsync(HttpStatusCode.NotFound, "Not Found!");
                        break;
                    }
                    var checkResult = await checker.Process(context.Request.Query, context.Request.HasFormContentType ? context.Request.Form : null, context.Request.Headers);
                    await context.Response.WriteResponseAsync(checkResult.StatusCode, checkResult.ErrorMsg, checkResult.Content, checkResult.Headers);
                    break;
                #endregion

                #region 检查分片完整性
                case "chunk":
                    var chunkChecker = context.RequestServices.GetService<ICheckChunkProcessor>();
                    if (chunkChecker == null)
                    {
                        await context.Response.WriteResponseAsync(HttpStatusCode.NotFound, "Not Found!");
                        break;
                    }
                    var chunkCheckResult = await chunkChecker.Process(context.Request.Query, context.Request.HasFormContentType ? context.Request.Form : null, context.Request.Headers);
                    await context.Response.WriteResponseAsync(chunkCheckResult.StatusCode, chunkCheckResult.ErrorMsg, chunkCheckResult.Content, chunkCheckResult.Headers);
                    break;
                #endregion

                #region 合并分片
                case "merge":
                    var merger = context.RequestServices.GetService<IMergeProcessor>();
                    if (merger == null)
                    {
                        await context.Response.WriteResponseAsync(HttpStatusCode.NotFound, "Not Found!");
                        break;
                    }
                    try
                    {
                        var merge = await merger.Process(context.Request.Query, context.Request.HasFormContentType ? context.Request.Form : null, context.Request.Headers);
                        if (!merge.Success)
                        {
                            await context.Response.WriteResponseAsync(HttpStatusCode.BadRequest, merge.ErrorMsg);
                            return;
                        }
                        var mergeHandler = context.RequestServices.GetRequiredService<IMergeHandler>();
                        var mergerResult = await mergeHandler.OnCompleted(context.Request.Query, context.Request.HasFormContentType ? context.Request.Form : null, context.Request.Headers, merge.FileName);
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
                        if (!contentType.MediaType.Equals("multipart/form-data", StringComparison.OrdinalIgnoreCase))
                        {
                            await context.Response.WriteResponseAsync(HttpStatusCode.UnsupportedMediaType, "ContentType must be multipart/form-data.");
                            return;
                        }
                        var processor = context.RequestServices.GetRequiredService<IUploadProcessor>();
                        var boundary = context.Request.GetMultipartBoundary();
                        var reader = new MultipartReader(boundary, context.Request.Body);
                        var section = await reader.ReadNextSectionAsync();
                        var formDic = new Dictionary<string, StringValues>();
                        var fileResult = new List<UploadFileResult>();
                        while (section != null)
                        {
                            var header = section.GetContentDispositionHeader();
                            if (header != null)
                            {
                                if (header.FileName.HasValue || header.FileNameStar.HasValue)
                                {
                                    var fileSection = section.AsFileSection();
                                    var extensionName = Path.GetExtension(fileSection.FileName);
                                    var (success, uploadResult, errorMessage) = await processor.Process(context.Request.Query, new FormCollection(formDic), context.Request.Headers, fileSection.FileStream, extensionName, fileSection.FileName, fileSection.Name);
                                    if (!success)
                                    {
                                        await context.Response.WriteResponseAsync(HttpStatusCode.BadRequest, errorMessage);
                                        return;
                                    }

                                    fileResult.Add(uploadResult);
                                }
                                else
                                {
                                    var formSection = section.AsFormDataSection();
                                    var value = new StringValues(await formSection.GetValueAsync());
                                    if (formDic.TryGetValue(formSection.Name, out var oldValue))
                                        formDic[formSection.Name] = StringValues.Concat(oldValue, value);
                                    else
                                        formDic[formSection.Name] = value;
                                }
                            }
                            section = await reader.ReadNextSectionAsync();
                        }


                        if (!fileResult.Any())
                        {
                            await context.Response.WriteResponseAsync(HttpStatusCode.BadRequest, "未发现上传文件");
                            return;
                        }

                        var completeHandler = context.RequestServices.GetRequiredService<IUploadCompletedHandler>();
                        var result = await completeHandler.OnCompleted(context.Request.Query, new FormCollection(formDic), context.Request.Headers, fileResult);
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
