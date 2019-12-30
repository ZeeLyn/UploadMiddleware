using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using UploadMiddleware.Core;
using UploadMiddleware.Core.Handlers;

namespace Example
{
    public class CustomUploadCompletedHandler : IUploadCompletedHandler
    {
        public async Task<ResponseResult> OnCompleted(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, IReadOnlyList<UploadFileResult> fileData)
        {
            return await Task.FromResult(new ResponseResult
            {
                Content = JsonConvert.SerializeObject(new { form, fileData, query, headers })
            });
        }
    }
}
