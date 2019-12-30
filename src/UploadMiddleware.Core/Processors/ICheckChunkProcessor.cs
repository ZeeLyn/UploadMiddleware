using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Processors
{
    public interface ICheckChunkProcessor
    {
        //Dictionary<string, string> FormData { get; }

        Task<ResponseResult> Process(HttpRequest request, IQueryCollection query, IFormCollection form, IHeaderDictionary headers);
    }
}
