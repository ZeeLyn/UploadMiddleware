using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UploadMiddleware.Core.Common;

namespace UploadMiddleware.Core
{
    public interface IFileValidator
    {
        /// <summary>
        /// 验证文件是否合法
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="stream"></param>
        /// <returns>因为multipart/form-data是流式数据，验证方法内部可能需要读取流，所以需要返回被读取部分的字节数组</returns>
        Task<(bool Success, string ErrorMsg, byte[] FileSignature)> Validate(string fileName, Stream stream);
    }

    public class FileValidator : IFileValidator
    {
        private UploadConfigure Configure { get; }
        public FileValidator(UploadConfigure configure)
        {
            Configure = configure;
        }
        public async Task<(bool Success, string ErrorMsg, byte[] FileSignature)> Validate(string fileName, Stream stream)
        {
            var extensionName = Path.GetExtension(fileName);
            if (!Configure.AllowFileExtension.Contains(extensionName))
                return (false, "Illegal file format.", null);
            if (!FileSignature.GetSignature(extensionName, out var signatures, out var offset))
                return (false, $"未找到{extensionName}文件的签名，通过FileSignature.AddSignature()方法添加签名。", null);
            var maxLen = signatures.Max(m => m.Length + offset);
            var headerBytes = new byte[maxLen];
            stream.Read(headerBytes, 0, maxLen);
            var success = signatures.Any(signature => headerBytes.Skip(offset).Take(signature.Length).SequenceEqual(signature));
            return await Task.FromResult((success, success ? "" : "Illegal file format.", headerBytes));
        }
    }
}
