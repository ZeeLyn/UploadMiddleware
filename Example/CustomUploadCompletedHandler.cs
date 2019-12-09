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
        public async Task<ResponseResult> OnCompleted(Dictionary<string, string> formData, List<UploadFileResult> fileData, Dictionary<string, string> queryData, HttpRequest request)
        {
            return await Task.FromResult(new ResponseResult
            {
                Content = JsonConvert.SerializeObject(new { formData, fileData, queryData })
            });
        }
    }
}
