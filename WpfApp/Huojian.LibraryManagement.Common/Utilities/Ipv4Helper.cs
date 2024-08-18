using System.Net;

namespace Huojian.LibraryManagement.Common.Utilities
{
    public class Ipv4Helper
    {
        public static string GetIpv4()
        {
            string name = Dns.GetHostName();
            IPAddress[] ipadrlist = Dns.GetHostAddresses(name);
            return ipadrlist.FirstOrDefault(m => m.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?.ToString();
        }

        public static bool TryGetIpv4(out string ipv4)
        {
            try
            {
                ipv4 = GetIpv4();
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message, ex);
                ipv4 = string.Empty;
                return false;
            }
        }
    }
}
