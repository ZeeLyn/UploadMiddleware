using System;
using System.Collections.Generic;

namespace UploadMiddleware.Core.Common
{
    public class FileSignature
    {
        private static readonly Dictionary<string, (int, List<byte[]>)> FilesSignature =
            new Dictionary<string, (int, List<byte[]>)>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    ".jpg", (0, new List<byte[]>
                    {
                        new byte[] {0xFF, 0xD8, 0xFF, 0xE0},
                        new byte[] {0xFF, 0xD8, 0xFF, 0xE1},
                        new byte[] {0xFF, 0xD8, 0xFF, 0xE2},
                        new byte[] {0xFF, 0xD8, 0xFF, 0xE3},
                        new byte[] {0xFF, 0xD8, 0xFF, 0xE8}
                    })
                },
                {
                    ".png", (0, new List<byte[]>
                    {
                        new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A}
                    })
                },
                {
                    ".gif", (0, new List<byte[]>
                    {
                        new byte[] {0x47, 0x49, 0x46, 0x38}
                    })
                },
                {
                    ".bmp", (0, new List<byte[]>
                    {
                        new byte[] {0x42, 0x4D}
                    })
                },
                {
                    ".mp3", (0, new List<byte[]>
                    {
                        new byte[] {0x49, 0x44, 0x33}
                    })
                },
                {
                    ".mp4", (4, new List<byte[]>
                    {
                        new byte[] { 0x66, 0x74, 0x79, 0x70, 0x6D, 0x70, 0x34, 0x32}
                    })
                },
                {
                    ".rar", (0, new List<byte[]>
                    {
                        new byte[] {0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00}
                    })
                },
                {
                    ".zip", (0, new List<byte[]>
                    {
                        new byte[] {0x50, 0x4B, 0x03, 0x04},
                        new byte[] {0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45},
                        new byte[] {0x50, 0x4B, 0x53, 0x70, 0x58},
                        new byte[] {0x50, 0x4B, 0x05, 0x06},
                        new byte[] {0x50, 0x4B, 0x07, 0x08},
                        new byte[] {0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70},
                        new byte[] {0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x01, 0x00}
                    })
                }
            };

        /// <summary>
        /// 添加文件签名，可在去这两个网站上查询：
        /// 1：https://www.filesignatures.net/index.php?page=search
        /// 2：https://en.wikipedia.org/wiki/List_of_file_signatures
        /// </summary>
        /// <param name="extensionName"></param>
        /// <param name="signature"></param>
        /// <param name="offset">偏移量</param>
        public static void AddSignature(string extensionName, List<byte[]> signature, int offset = 0)
        {
            if (offset < 0)
                throw new ArgumentException("offset不能小于0");
            FilesSignature.Add(extensionName, (offset, signature));
        }

        public static bool GetSignature(string extensionName, out List<byte[]> signature, out int offset)
        {
            var r = FilesSignature.TryGetValue(extensionName, out var value);
            if (r)
            {
                signature = value.Item2;
                offset = value.Item1;
            }
            else
            {
                signature = null;
                offset = 0;
            }
            return r;
        }

        public static Dictionary<string, (int, List<byte[]>)> GetAllSignature()
        {
            return FilesSignature;
        }
    }
}
