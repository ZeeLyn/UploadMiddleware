using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public interface IMergeHandler
    {
        Task<ResponseResult> OnCompleted(HttpRequest request, IQueryCollection query, IFormCollection form, IHeaderDictionary headers, string fileName);
    }
}
