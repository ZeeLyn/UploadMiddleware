using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UploadMiddleware.Core.Handlers;

namespace Example
{
    public class CustomUploadCompletedCallbackHandler : IUploadCompletedCallbackHandler
    {
        public async Task OnCompletedAsync(string fileName, string localFileName)
        {

        }
    }
}
