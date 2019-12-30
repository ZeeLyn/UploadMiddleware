using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public class UploadCompletedHandler : IUploadCompletedHandler
    {
        public virtual async Task<ResponseResult> OnCompleted(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, IReadOnlyList<UploadFileResult> fileData)
        {
            return await Task.FromResult(new ResponseResult
            {
                Content = fileData.Select(p => p.Url).ToArray()
            });
        }
    }
}
