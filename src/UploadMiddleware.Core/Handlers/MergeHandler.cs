using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public class MergeHandler : IMergeHandler
    {
        public async Task<ResponseResult> OnCompleted(Dictionary<string, string> formData, Dictionary<string, string> queryData, string fileName, HttpRequest request)
        {
            return await Task.FromResult(new ResponseResult
            {
                Content = $"{{\"errorMsg\":\"OK\",\"data\":\"{fileName}\"}}"
            });
        }
    }
}
