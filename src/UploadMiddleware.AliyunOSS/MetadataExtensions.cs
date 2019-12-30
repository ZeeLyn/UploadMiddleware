using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace UploadMiddleware.AliyunOSS
{
    public static class MetadataExtensions
    {
        /// <summary>
        /// $(form:key)
        /// $(query:key)
        /// $LocalFileName
        /// $SectionName
        /// </summary>
        /// <param name="meta"></param>
        /// <param name="localFileName"></param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        internal static string Resolve(this string meta, IQueryCollection queryData, IFormCollection formData, string localFileName, string sectionName)
        {
            if (string.IsNullOrWhiteSpace(meta))
                return meta;
            meta = meta.Replace("$LocalFileName", HttpUtility.UrlEncode(localFileName)).Replace("$SectionName", HttpUtility.UrlEncode(sectionName));
            var form = Regex.Match(meta, @"(?<=\$\(form:)[^\)]+");
            if (form.Success)
            {
                meta = form.Groups.ToList().Aggregate(meta,
                    (current, item) => current.Replace($"$(form:{item.Value})", formData.TryGetValue(item.Value, out var value) ? HttpUtility.UrlEncode(value) : ""));
            }
            var query = Regex.Match(meta, @"(?<=\$\(query:)[^\)]+");
            if (query.Success)
            {
                meta = query.Groups.ToList().Aggregate(meta, (current, item) => current.Replace($"$(query:{item.Value})", queryData.TryGetValue(item.Value, out var value) ? HttpUtility.UrlEncode(value) : ""));
            }
            return meta;
        }


    }
}
