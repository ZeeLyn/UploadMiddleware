using System;
using System.Collections.Generic;

namespace UploadMiddleware.Core.Common
{
    public class FileSignature
    {
        private static readonly Dictionary<string, List<(int Offset, byte[] Signatures)>> FilesSignature =
            new Dictionary<string, List<(int, byte[])>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    ".jpg", new List<(int, byte[])>
                    {
                        (0, new byte[] {0xFF, 0xD8, 0xFF, 0xE0}),
                        (0, new byte[] {0xFF, 0xD8, 0xFF, 0xE1}),
                        (0, new byte[] {0xFF, 0xD8, 0xFF, 0xE8})
                    }
                },
                {
                    ".jpeg", new List<(int, byte[])>
                    {
                        (0, new byte[] {0xFF, 0xD8, 0xFF, 0xE0}),
                        (0, new byte[] {0xFF, 0xD8, 0xFF, 0xE2}),
                        (0, new byte[] {0xFF, 0xD8, 0xFF, 0xE3})
                    }
                },
                {
                    ".png", new List<(int, byte[])>
                    {
                        (0, new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A})
                    }
                },
                {
                    ".gif", new List<(int, byte[])>
                    {
                        (0, new byte[] {0x47, 0x49, 0x46, 0x38})
                    }
                },
                {
                    ".bmp", new List<(int, byte[])>
                    {
                        (0, new byte[] {0x42, 0x4D})
                    }
                },
                {
                    ".mp3", new List<(int, byte[])>
                    {
                        (0, new byte[] {0x49, 0x44, 0x33})
                    }
                },
                {
                    ".mp4", new List<(int, byte[])>
                    {
                        (4, new byte[] {0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32})
                    }
                },
                {
                    ".rar", new List<(int, byte[])>
                    {
                        (0, new byte[] {0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00})
                    }
                },
                {
                    ".zip", new List<(int, byte[])>
                    {
                        (0, new byte[] {0x50, 0x4B, 0x03, 0x04}),
                        (0, new byte[] {0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45}),
                        (0, new byte[] {0x50, 0x4B, 0x53, 0x70, 0x58}),
                        (0, new byte[] {0x50, 0x4B, 0x05, 0x06}),
                        (0, new byte[] {0x50, 0x4B, 0x07, 0x08}),
                        (0, new byte[] {0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70}),
                        (0, new byte[] {0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x01, 0x00})
                    }
                },
                {
                    ".pdf", new List<(int, byte[])>
                    {
                        (0, new byte[] {0x25, 0x50, 0x44, 0x46})
                    }
                },
                {
                    ".doc", new List<(int, byte[])>
                    {
                        (0, new byte[] {0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1}),
                        (0, new byte[] {0x0D, 0x44, 0x4F, 0x43}),
                        (0, new byte[] {0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, 0x00}),
                        (0, new byte[] {0xDB, 0xA5, 0x2D, 0x00})
                    }
                },
                {
                    ".docx", new List<(int, byte[])>
                    {
                        (0, new byte[] {0x50, 0x4B, 0x03, 0x04}),
                        (0, new byte[] {0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00})
                    }
                }
            };

        /// <summary>
        /// 添加文件签名，可在去这两个网站上查询：
        /// 1：https://www.filesignatures.net/index.php?page=search
        /// 2：https://en.wikipedia.org/wiki/List_of_file_signatures
        /// </summary>
        /// <param name="extensionName"></param>
        /// <param name="signature"></param>
        public static void AddSignature(string extensionName, List<(int offset, byte[] signature)> signature)
        {
            //if (signature < 0)
            //    throw new ArgumentException("offset不能小于0");
            FilesSignature.Add(extensionName, signature);
        }

        public static bool GetSignature(string extensionName, out List<(int Offset, byte[] Signature)> signature)
        {
            return FilesSignature.TryGetValue(extensionName, out signature);
        }

        public static Dictionary<string, List<(int Offset, byte[] Signatures)>> GetAllSignature()
        {
            return FilesSignature;
        }
    }
}
