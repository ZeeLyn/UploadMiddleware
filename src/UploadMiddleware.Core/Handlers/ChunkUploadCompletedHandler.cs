using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public class ChunkUploadCompletedHandler : IUploadCompletedHandler
    {
        public async Task<ResponseResult> OnCompleted(Dictionary<string, string> formData, List<UploadFileResult> fileData, Dictionary<string, string> queryData, HttpRequest request)
        {
            return await Task.FromResult(new ResponseResult
            {
                Content = null
            });
        }
    }
}
