using System;
using System.Text;
using Huojian.LibraryManagement.Common;

namespace ShadowBot.Common.Utilities
{
    public class Base64Helper
    {
        public static string ConvertBase64IfNotAllAscii(string utf8string)
        {
            if (string.IsNullOrEmpty(utf8string)) return string.Empty;

            try
            {
                var allAscii = true;
                foreach (var c in utf8string)
                {
                    if (!char.IsAscii(c))
                    {
                        allAscii = false;
                        break;
                    }
                }

                if (allAscii)
                {
                    return utf8string;
                }

                return Convert.ToBase64String(new UTF8Encoding().GetBytes(utf8string));

            }
            catch (Exception e)
            {
                Logging.Warn("ConvertBase64IfNotAllAscii failed.", e);
            }

            return string.Empty;
        }
    }
}
