using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Processors
{
    public interface ICheckChunksProcessor
    {

        Task<ResponseResult> Process(IQueryCollection query, IFormCollection form, IHeaderDictionary headers);
    }
}
