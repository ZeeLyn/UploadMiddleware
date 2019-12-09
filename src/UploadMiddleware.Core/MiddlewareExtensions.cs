using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using System;

namespace UploadMiddleware.Core
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseUpload(this IApplicationBuilder app, string route)
        {
            if (string.IsNullOrWhiteSpace(route))
                throw new ArgumentNullException(nameof(route));

            return app.UseMiddleware<UploadMiddleware>(Options.Create(new UploadOptions
            {
                Route = route
            }));
        }
    }
}
