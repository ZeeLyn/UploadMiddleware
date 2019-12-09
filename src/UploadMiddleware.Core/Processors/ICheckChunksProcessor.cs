using System.Collections.Generic;
using System.Threading.Tasks;

namespace UploadMiddleware.Core.Processors
{
    public interface ICheckChunksProcessor
    {
        Dictionary<string, string> FormData { get; }

        Task<ResponseResult> Process();
    }
}
