# UploadMiddleware
文件上传中间件，支持分片断点上传

# Packages & Status
Packages | NuGet
---------|------
UploadMiddleware.Core|[![NuGet package](https://buildstats.info/nuget/UploadMiddleware.Core)](https://www.nuget.org/packages/UploadMiddleware.Core)
UploadMiddleware.LocalStorage|[![NuGet package](https://buildstats.info/nuget/UploadMiddleware.LocalStorage)](https://www.nuget.org/packages/UploadMiddleware.LocalStorage)
UploadMiddleware.AliyunOSS|[![NuGet package](https://buildstats.info/nuget/UploadMiddleware.AliyunOSS)](https://www.nuget.org/packages/UploadMiddleware.AliyunOSS)


## 注册&配置

### 本地存储
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews();
    services.AddUploadLocalStorage(options =>
    {
        options.RootDirectory = Configuration.GetSection("SaveRootDirectory").Value;
        options.AllowFileExtension.Add(".mp4");
        options.AddUploadCompletedHandler<CustomUploadCompletedHandler>();
    });
}
```

### 分片上传，本地存储
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews();

    services.AddChunkedUploadLocalStorage(options =>
    {
        options.AllowFileExtension.Add(".zip");
        options.AllowFileExtension.Add(".mp4");
        options.RootDirectory = Configuration.GetSection("SaveRootDirectory").Value;
        options.DeleteChunksOnMerged = true;
    });
}
```

### 上传阿里云OSS
```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllersWithViews();
    services.AddUploadAliyunOSS(options =>
    {
        options.AccessId = Configuration.GetSection("OSS:AccessId").Value;
        options.AccessKeySecret = Configuration.GetSection("OSS:AccessKeySecret").Value;
        options.BucketName = Configuration.GetSection("OSS:BucketName").Value;
        options.Endpoint = Configuration.GetSection("OSS:Endpoint").Value;
        options.RootDirectory = Configuration.GetSection("OSS:RootDirectory").Value;
    });
}
```

### 启用中间件
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    //配置上传路由
    app.UseUpload("/upload");
}

```

## 路由示例&说明(以下实例路由可在Configure方法里定义)

**上传文件路由**

路由 | 请求类型 | ContentType
---------|------|--------
/upload|POST|multipart/form-data

必须参数|类型|说明
---------|------|--------
文件流|file|至少要上传一个文件
md5|string|文件的MD5值，分片上传时必须
chunk|int|当前上的分片索引，从0开始，分片上传时必须


* * *



**检查已经上传的分片数量**

路由 | 请求类型 | ContentType
---------|------|--------
/upload?action=chunks|POST|application/x-www-form-urlencoded

必须参数|类型|说明
---------|------|--------
md5|string|文件的MD5值


* * *
**检查单个分片的完整性**

路由 | 请求类型 | ContentType
---------|------|--------
/upload?action=chunk|POST|application/x-www-form-urlencoded

必须参数|类型|说明
---------|------|--------
chunk_md5|string|分片的MD5值
md5|string|文件的MD5值
chunk|int|分片索引，从0开始

* * *

**合并分片**

路由 | 请求类型 | ContentType
---------|------|--------
/upload?action=merge|POST|application/x-www-form-urlencoded

必须参数|类型|说明
---------|------|--------
md5|string|文件的MD5值
chunks|int|分片数量

* * *


## 配置

参数 | 类型 | 说明
---------|------|--------
RootDirectory|string|文件保存的根目录
MultipartBodyLengthLimit|long|Multipart Body的上限,Kestrel服务下才起作用,IIS下请在web.config里配置
AllowFileExtension|HashSet|允许上传的文件格式(以"."开头)
BufferSize|int|缓冲池大小（默认64KB）,推荐不要超过64KB，超过后会写磁盘
ChunksRootDirectory|string|存放分片的跟目录，不设置则默认使用RootDirectory
ChunksFormName|string|传输分片数量的表单name（默认：chunks）
ChunkFormName|string| 传输分片索引的表单name,分片索引从0开始（默认：chunk）
FileMd5FormName|string|传输文件的MD5值的表单name，注意是文件不是分片(默认：md5)
ChunkMd5FormName|string|传输分片的MD5值的表单name（默认：chunk_md5）
DeleteChunksOnMerged|bool|当分片合并完成时，是否删除分片，(默认：True)
AccessId|string|OSS access key Id
AccessKeySecret|string|OSS key secret
Endpoint|string|OSS endpoint
BucketName|string|OSS bucket name
SecurityToken|string|STS security token
Metadata|Dictionary|设置文件的元数据，每种文件格式可以单独配置,可使用以下占位符:$(form:name):   表单数据，name是具体的form表单key;$(query:name):   query参数，name是具体的query参数key;$LocalFileName:  本地文件名;$SectionName:  MultipartBody的name值;
AddUploadCompletedHandler|method|添加自定义(文件/分片)上传完成返回结果组装Handler
AddUploadProcessor|method|添加自定义上传处理器
AddFileValidator|method|添加自定义文件格式验证器
AddSubdirectoryGenerator|method|添加子目录生成器
AddFileNameGenerator|method|添加文件名生成器
AddCheckChunksProcessor|method|添加自定义分片数量检测器
AddCheckChunkProcessor|method|添加自定义分片完整性检测器
AddMergeProcessor|method|添加自定义分片合并器
AddMergeHandler|method|添加分片合并完成返回结果组装Handler

