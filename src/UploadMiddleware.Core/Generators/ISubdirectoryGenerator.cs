using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Generators
{
    public interface ISubdirectoryGenerator
    {
        Task<string> Generate(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, string extensionName);
    }
}
