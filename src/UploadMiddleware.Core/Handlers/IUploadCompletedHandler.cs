using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public interface IUploadCompletedHandler
    {
        Task<ResponseResult> OnCompleted(Dictionary<string, string> formData, List<UploadFileResult> fileData, Dictionary<string, string> queryData, HttpRequest request);
    }
}
