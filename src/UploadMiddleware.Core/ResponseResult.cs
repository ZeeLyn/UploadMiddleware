using System.Net;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.Core
{
    public class ResponseResult
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        public string ContextType { get; set; } = "application/json";

        public object Content { get; set; }

        public string ErrorMsg { get; set; } = "OK";

        public IHeaderDictionary Headers { get; set; }
    }
}
