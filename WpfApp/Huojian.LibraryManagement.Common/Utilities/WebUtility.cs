using ShadowBot.Common.LocalizationResources;
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{

    public static class WebUtility
    {
        public static async Task<bool> DownloadFaviconAsync(string url, string fileName, int timeout = 5000)
        {
            try
            {
                // 先尝试直接获取favicon.ico
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                    return false;
                try
                {
                    using (var httpClient = new TimeoutHttpClient())
                    {
                        var bs = await httpClient.GetByteArrayAsync($"{uri.Scheme}://{uri.Host}/favicon.ico");
                        //有些取出来为网页源代码，开头：<!DOCTYPE html>,比如：https://demo-shop.winrobot360.com/favicon.ico
                        var docType = Encoding.UTF8.GetString(bs,0,9);
                        if (docType.ToUpper() != "<!DOCTYPE")
                        {
                            using (var fs = new FileStream(fileName, FileMode.Create))
                                fs.Write(bs, 0, bs.Length);
                            return true;
                        }
                    }
                }
                catch (Exception)
                {
                    FileSystem.Delete(fileName);
                }

                // 再尝试从源码中读取favicon.ico
                var source = await GetPageSourceAsync(url, timeout);
                if (source == null)
                    return false;
                var linkMatch = Regex.Match(source,
                    @"<link[^>]*rel=""shortcut icon""[^>]*>",
                    RegexOptions.IgnoreCase);
                if (!linkMatch.Success)
                {
                    linkMatch = Regex.Match(source,
                        @"<link[^>]*rel=""icon""[^>]*>",
                        RegexOptions.IgnoreCase);
                    if (!linkMatch.Success)
                        return false;
                }
                var hrefMatch = Regex.Match(linkMatch.Groups[0].Value,
                    @"href=""(?<ico>[\s\S]*?)""",
                    RegexOptions.IgnoreCase);
                if (!hrefMatch.Success)
                    return false;
                Uri baseUri;
                if (uri.Port == 0 || uri.Port == 443 || uri.Port == 80)
                    baseUri = new Uri($"{uri.Scheme}://{uri.Host}");
                else
                    baseUri = new Uri($"{uri.Scheme}://{uri.Host}:{uri.Port}");
                var faviconUri = new Uri(baseUri, hrefMatch.Groups["ico"].Value);
                using (var webClient = new TimeoutWebClient(timeout))
                {
                    await webClient.DownloadFileTaskAsync(faviconUri, fileName);
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to download favicon", ex);
                FileSystem.Delete(fileName);
                return false;
            }
            return true;
        }

        public static async Task<string> GetPageTitleAsync(string url, int timeout = 5000)
        {
            var source = await GetPageSourceAsync(url, timeout);
            if (source == null)
                return null;
            var match = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<title>[\s\S]*?)\</title\>",
                RegexOptions.IgnoreCase);
            if (!match.Success)
                return null;
            return match.Groups["title"].Value;
        }

        public static async Task<string> GetPageSourceAsync(string url, int timeout = 5000)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                return null;
            try
            {
                using (var httpClient = new TimeoutHttpClient())
                {
                    var str = await httpClient.GetStringAsync(uri);
                    return str;
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to download favicon", ex);
                return null;
            }
        }
    }
}
