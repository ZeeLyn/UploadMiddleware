using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Generators
{
    public class FileNameGenerator : IFileNameGenerator
    {
        public async Task<string> Generate(HttpRequest request, IQueryCollection query, IFormCollection form, IHeaderDictionary headers, string extensionName)
        {
            return await Task.FromResult(Guid.NewGuid().ToString("N"));
        }
    }
}
