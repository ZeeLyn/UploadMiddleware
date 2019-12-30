using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Generators
{
    public class SubdirectoryGenerator : ISubdirectoryGenerator
    {
        public async Task<string> Generate(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, string extensionName)
        {
            return await Task.FromResult(DateTime.Now.ToString("yyyyMMdd"));
        }
    }
}
