using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Generators
{
    public class SubdirectoryGenerator : ISubdirectoryGenerator
    {
        public async Task<string> Generate(Dictionary<string, string> formData, Dictionary<string, string> queryData, HttpRequest request, string extensionName)
        {
            return await Task.FromResult(DateTime.Now.ToString("yyyyMMdd"));
        }
    }
}
