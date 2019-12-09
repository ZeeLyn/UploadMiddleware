using System.Collections.Generic;
using Aliyun.OSS;
using Microsoft.Extensions.DependencyInjection;
using UploadMiddleware.Core;

namespace UploadMiddleware.AliyunOSS
{
    public class AliyunOssStorageConfigure : UploadConfigure
    {
        public AliyunOssStorageConfigure(IServiceCollection services) : base(services)
        {
            RootDirectory = "middleware/upload";
        }

        /// <summary>
        /// OSS access key Id
        /// </summary>
        public string AccessId { get; set; }

        /// <summary>
        /// OSS key secret
        /// </summary>
        public string AccessKeySecret { get; set; }
        /// <summary>
        /// OSS endpoint
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// OSS bucket name
        /// </summary>
        public string BucketName { get; set; }

        /// <summary>
        /// STS security token
        /// </summary>
        public string SecurityToken { get; set; }

        /// <summary>
        /// 设置文件的HTTP头，每种文件格式可以单独配置,可使用以下占位符:
        /// $(form:name):   表单数据，name是具体的form表单key;
        /// $(query:name):   query参数，name是具体的query参数key;
        /// $LocalFileName:  本地文件名;
        /// $SectionName:  MultipartBody的name值;
        /// </summary>
        public Dictionary<string, ObjectMetadata> Metadata { get; } = new Dictionary<string, ObjectMetadata>
        {
            {
                ".jpg",
                new ObjectMetadata {ContentType = "image/jpeg"}
            },
            {".jpeg", new ObjectMetadata {ContentType = "image/jpeg"}},
            {".gif", new ObjectMetadata {ContentType = "image/gif"}},
            {".png", new ObjectMetadata {ContentType = "image/png"}}
        };
    }
}
