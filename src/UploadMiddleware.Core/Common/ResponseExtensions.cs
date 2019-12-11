using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace UploadMiddleware.Core.Common
{
    public static class ResponseExtensions
    {
        public static async Task WriteResponseAsync(this HttpResponse response, HttpStatusCode statusCode = HttpStatusCode.OK, string errorMsg = "OK", object data = null, IHeaderDictionary headers = null, string contentType = "application/json")
        {
            response.StatusCode = (int)statusCode;
            response.ContentType = contentType;
            if (headers != null && headers.Any())
            {
                foreach (var (key, value) in headers)
                {
                    response.Headers[key] = value;
                }
            }
            await response.WriteAsync(data == null ? JsonSerializer.Serialize(new { errmsg = errorMsg }) : JsonSerializer.Serialize(new { errmsg = errorMsg, data }));
        }
    }
}
