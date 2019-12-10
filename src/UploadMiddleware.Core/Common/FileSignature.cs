using System;
using System.Collections.Generic;

namespace UploadMiddleware.Core.Common
{
    public class FileSignature
    {
        private static readonly Dictionary<string, List<byte[]>> FilesSignature =
            new Dictionary<string, List<byte[]>>(StringComparer.OrdinalIgnoreCase)
            {
                {
                    ".jpg", new List<byte[]>
                    {
                        new byte[] {0xFF, 0xD8, 0xFF, 0xE0},
                        new byte[] {0xFF, 0xD8, 0xFF, 0xE1},
                        new byte[] {0xFF, 0xD8, 0xFF, 0xE2},
                        new byte[] {0xFF, 0xD8, 0xFF, 0xE3},
                        new byte[] {0xFF, 0xD8, 0xFF, 0xE8}
                    }
                },
                {
                    ".png", new List<byte[]>
                    {
                        new byte[] {0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A}
                    }
                },
                {
                    ".gif", new List<byte[]>
                    {
                        new byte[] {0x47, 0x49, 0x46, 0x38}
                    }
                },
                {
                    ".bmp", new List<byte[]>
                    {
                        new byte[] {0x42, 0x4D}
                    }
                },
                {
                    ".mp3", new List<byte[]>
                    {
                        new byte[] {0x49, 0x44, 0x33}
                    }
                },
                {
                    ".mp4", new List<byte[]>
                    {
                        new byte[] { 0x66, 0x74, 0x79, 0x70, 0x4D, 0x53, 0x4E, 0x56 }
                    }
                },
                {
                    ".rar", new List<byte[]>
                    {
                        new byte[] {0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x00}
                    }
                },
                {
                    ".zip", new List<byte[]>
                    {
                        new byte[] {0x50, 0x4B, 0x03, 0x04},
                        new byte[] {0x50, 0x4B, 0x4C, 0x49, 0x54, 0x45},
                        new byte[] {0x50, 0x4B, 0x53, 0x70, 0x58},
                        new byte[] {0x50, 0x4B, 0x05, 0x06},
                        new byte[] {0x50, 0x4B, 0x07, 0x08},
                        new byte[] {0x57, 0x69, 0x6E, 0x5A, 0x69, 0x70},
                        new byte[] {0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x01, 0x00}
                    }
                }
            };

        /// <summary>
        /// 添加文件签名，可在此网站上查询：https://www.filesignatures.net/index.php?page=search
        /// </summary>
        /// <param name="extensionName"></param>
        /// <param name="signature"></param>
        public static void AddSignature(string extensionName, List<byte[]> signature)
        {
            FilesSignature.Add(extensionName, signature);
        }

        public static bool GetSignature(string extensionName, out List<byte[]> signature)
        {
            return FilesSignature.TryGetValue(extensionName, out signature);
        }

        public static Dictionary<string, List<byte[]>> GetAllSignature()
        {
            return FilesSignature;
        }
    }
}
