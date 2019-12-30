using System;
using System.Collections.Generic;
using System.Text;

namespace UploadMiddleware.Core
{
    public class ConstConfigs
    {  /// <summary>
       /// 传输分片数量的表单name（默认：chunks）
       /// </summary>
        public const string ChunksHeaderKey = "chunks";

        /// <summary>
        /// 传输分片索引的表单name,分片索引从0开始（默认：chunk）
        /// </summary>
        public const string ChunkHeaderKey = "chunk";

        /// <summary>
        /// 传输文件的MD5值的表单name，注意是文件不是分片(默认：md5)
        /// </summary>
        public const string FileMd5HeaderKey = "file-md5";

        /// <summary>
        /// 传输分片的MD5值的表单name（默认：chunk_md5）
        /// </summary>
        public const string ChunkMd5HeaderKey = "chunk-md5";
    }
}
