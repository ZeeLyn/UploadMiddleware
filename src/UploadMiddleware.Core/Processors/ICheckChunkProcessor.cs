using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Processors
{
    public interface ICheckChunkProcessor
    {
        Task<ResponseResult> Process(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, HttpRequest request);
    }
}
