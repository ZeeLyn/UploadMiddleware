using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public class MergeHandler : IMergeHandler
    {
        public async Task<ResponseResult> OnCompleted(HttpRequest request, IQueryCollection query, IFormCollection form, IHeaderDictionary headers, string fileName)
        {
            return await Task.FromResult(new ResponseResult
            {
                Content = fileName
            });
        }
    }
}
