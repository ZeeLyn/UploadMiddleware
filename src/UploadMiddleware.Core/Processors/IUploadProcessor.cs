using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Processors
{
    public interface IUploadProcessor
    {
        Task<(bool Success, UploadFileResult Result, string ErrorMessage)> Process(IQueryCollection query, IFormCollection form, IHeaderDictionary headers, Stream fileStream, string extensionName, string localFileName, string sectionName, HttpRequest request);

    }
}
