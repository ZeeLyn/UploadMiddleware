using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public class ChunkUploadCompletedHandler : IUploadCompletedHandler
    {
        public async Task<ResponseResult> OnCompleted(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, IReadOnlyList<UploadFileResult> fileData)
        {
            return await Task.FromResult(new ResponseResult
            {
                Content = null
            });
        }
    }
}
