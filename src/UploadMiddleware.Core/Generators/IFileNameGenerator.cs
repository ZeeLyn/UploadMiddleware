using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Generators
{
    public interface IFileNameGenerator
    {
        Task<string> Generate(Dictionary<string, string> formData, Dictionary<string, string> queryData, HttpRequest request, string extensionName);
    }
}
