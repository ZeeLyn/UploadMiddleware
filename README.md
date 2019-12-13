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

请求参数|类型|说明
---------|------|--------
file|binary|至少要上传一个文件
md5|string|文件的MD5值，分片上传时必须
chunk|int|当前上的分片索引，从0开始，分片上传时必须


* * *



**检查已经上传的分片数量**

路由 | 请求类型 | ContentType|说明
---------|------|--------|------
/upload?action=chunks|POST|application/x-www-form-urlencoded|此接口只适用于单线程，分片按顺序上传

请求参数|类型|说明
---------|------|--------
md5|string|文件的MD5值


返回值|类型|说明
---------|------|--------
errmsg|string|具体的错误信息
chunks|int|已经上传的分片数量，默认会是已经上传的分片数量减一，因为可能最后一个分片不完整


* * *
**检查单个分片的完整性**

路由 | 请求类型 | ContentType
---------|------|--------
/upload?action=chunk|POST|application/x-www-form-urlencoded

请求参数|类型|说明
---------|------|--------
chunk_md5|string|分片的MD5值
md5|string|文件的MD5值
chunk|int|分片索引，从0开始

返回值|类型|说明
---------|------|--------
errmsg|string|具体的错误信息
data|int|0或1,1表示分片完整

* * *

**合并分片**

路由 | 请求类型 | ContentType
---------|------|--------
/upload?action=merge|POST|application/x-www-form-urlencoded

请求参数|类型|说明
---------|------|--------
md5|string|文件的MD5值
chunks|int|分片数量

* * *


## 可选配置

参数 | 类型 | 说明
---------|------|--------
RootDirectory|string|文件保存的根目录
MultipartBodyLengthLimit|long|Multipart Body的上限,Kestrel服务下才起作用,IIS下请在web.config里配置
AllowFileExtension|HashSet|允许上传的文件格式(以"."开头),默认有：.jpg,.jpeg,.png,.gif,可以自行添加和删除
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



## 关于分片上传
* 分片上传会有两种情况，一种是前端多线程上传，一种是单线程上传。
* 如果是多线程上传，上传分片的顺序可能是乱的，所以如果你需要支持断点续传的话，需要在上传前检查分片的完整性。
* 单线程上传的话就简单一些，只要前端保证按顺序上传就可以，如果需要支持断点续传，只需要在上传前检查已经上传的分片数量，默认情况下返回的分片数量会是已经上传的分片数量减一，因为最后一个分片可能是不完整的，当然你也可以选择没个分片都验证完整性。
* 前端推荐使用WebUploader，但是WebUploader分片上传有一个小问题：如果启用了分片上传，上传的文件大小又小于分片大小，WebUploader就不会分片，Form里也就不会带上分片索引和分片数量，所以这个需要自己处理一下。

## 关于文件格式验证
* 很多人上传文件只是单纯的验证文件的后缀名，这是不安全的。
* UploadMiddleware默认的文件验证器，不止验证了后缀名，还会根据后缀名验证文件的[签名](https://en.wikipedia.org/wiki/File_signature),具体的文件签名可以去这两个地方查询：<https://www.filesignatures.net/index.php?page=search> ,  <https://en.wikipedia.org/wiki/List_of_file_signatures>
* UploadMiddleware内置了常用(.jpg、.png、.gif、.bmp、.mp3、.mp4、.rar、.zip)的文件签名，可通过FileSignature类查询和添加

## 关于权限控制
* 权限过滤器本来就是中间件，UploadMiddleware也是中间件，所以UploadMiddleware内不能加权限过滤器。
* 解决办法是在项目里写一个跟中间件路由一致的空api，api不需要有任何逻辑，也不会被调用，并加上AuthorizeAttribtue即可解决授权问题。
