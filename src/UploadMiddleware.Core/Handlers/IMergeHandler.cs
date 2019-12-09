using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public interface IMergeHandler
    {
        Task<ResponseResult> OnCompleted(Dictionary<string, string> formData, Dictionary<string, string> queryData, string fileName, HttpRequest request);
    }
}
