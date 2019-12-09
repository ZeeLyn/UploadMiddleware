using System.IO;
using System.Threading.Tasks;

namespace UploadMiddleware.Core
{
    public interface IValidateFile
    {
        Task<bool> Validate(string fileName, Stream stream);
    }

    public class ValidateFile : IValidateFile
    {
        private UploadConfigure Configure { get; }
        public ValidateFile(UploadConfigure configure)
        {
            Configure = configure;
        }
        public async Task<bool> Validate(string fileName, Stream stream)
        {
            var extensionName = Path.GetExtension(fileName);
            return await Task.FromResult(Configure.AllowFileExtension.Contains(extensionName));
        }
    }
}
