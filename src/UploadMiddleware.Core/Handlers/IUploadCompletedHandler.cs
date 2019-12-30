using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public interface IUploadCompletedHandler
    {
        Task<ResponseResult> OnCompleted(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, IReadOnlyList<UploadFileResult> fileData);
    }
}
