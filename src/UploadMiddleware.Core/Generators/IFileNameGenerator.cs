using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Generators
{
    public interface IFileNameGenerator
    {
        Task<string> Generate(HttpRequest request, IQueryCollection query, IFormCollection form, IHeaderDictionary headers, string extensionName);
    }
}
