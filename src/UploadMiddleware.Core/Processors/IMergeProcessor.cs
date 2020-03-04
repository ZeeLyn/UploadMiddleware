using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Processors
{
    public interface IMergeProcessor
    {
        Task<(bool Success, string FileName, string ErrorMsg)> Process(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, HttpRequest request);
    }
}
