using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Processors
{
    public interface IUploadProcessor
    {
        Dictionary<string, string> FormData { get; }

        List<UploadFileResult> FileData { get; }

        Dictionary<string, string> QueryData { get; }

        Task<(bool Success, string ErrorMessage)> Process(Stream fileStream, string extensionName, HttpRequest request, string localFileName, string sectionName);

    }
}
