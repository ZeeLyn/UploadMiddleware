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
        Task<(bool Success, byte[] FileSignature)> Validate(string fileName, Stream stream);
    }

    public class FileValidator : IFileValidator
    {
        private UploadConfigure Configure { get; }
        public FileValidator(UploadConfigure configure)
        {
            Configure = configure;
        }
        public async Task<(bool Success, byte[] FileSignature)> Validate(string fileName, Stream stream)
        {
            var extensionName = Path.GetExtension(fileName);
            if (!Configure.AllowFileExtension.Contains(extensionName))
                return (false, null);
            if (!FileSignature.GetSignature(extensionName, out var signatures))
                return (false, null);
            var maxLen = signatures.Max(m => m.Length);
            var headerBytes = new byte[maxLen];
            stream.Read(headerBytes, 0, maxLen);
            return await Task.FromResult((signatures.Any(signature =>
               headerBytes.Take(signature.Length).SequenceEqual(signature)), headerBytes));
        }
    }
}
