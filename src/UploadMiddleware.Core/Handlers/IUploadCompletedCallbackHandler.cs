using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core.Handlers
{
    public interface IUploadCompletedCallbackHandler
    {
        Task OnCompletedAsync(string fileName, string localFileName, HttpRequest request);
    }
}
