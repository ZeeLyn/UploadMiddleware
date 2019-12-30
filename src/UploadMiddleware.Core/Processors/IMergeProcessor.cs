using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Processors
{
    public interface IMergeProcessor
    {
        //Dictionary<string, string> FormData { get; }

        //Dictionary<string, string> QueryData { get; }

        Task<(bool Success, string FileName, string ErrorMsg)> Process(HttpRequest request, IQueryCollection query, IFormCollection form, IHeaderDictionary headers);
    }
}
