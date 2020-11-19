using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace UploadMiddleware.Core.Handlers
{
    public interface IUploadCompletedCallbackHandler
    {
        Task OnCompletedAsync(string fileName, string localFileName);
    }
}
