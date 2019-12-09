using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public class UploadCompletedHandler : IUploadCompletedHandler
    {
        public virtual async Task<ResponseResult> OnCompleted(Dictionary<string, string> formData, List<UploadFileResult> fileData, Dictionary<string, string> queryData, HttpRequest request)
        {
            return await Task.FromResult(new ResponseResult
            {
                Content = $"{{\"errorMsg\":\"OK\",\"data\":[{string.Join(",", fileData.Select(p => "\"" + p.Url + "\""))}]}}"
            });
        }
    }
}
