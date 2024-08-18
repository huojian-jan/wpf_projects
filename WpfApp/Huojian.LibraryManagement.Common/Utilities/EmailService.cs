using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class EmailService
    {
        public EmailProvider Provider { get; }
        public string Name { get; }
        public string Ip { get; }
        public int Port { get; }
        public int SSLPort { get; }

        public bool UseSsl { get; }

        public EmailService(EmailProvider provider, string name, string ip, int port, int sslPort, bool useSsl = true)
        {
            Provider = provider;
            Name = name;
            Ip = ip;
            Port = port;
            SSLPort = sslPort;
            UseSsl = useSsl;
        }
    }

    public enum EmailProvider
    {
        Custom,
        Netease163,
        Netease126,
        TencentQQ,
        Google,
        Outlook,
        iCloud
    }
}
