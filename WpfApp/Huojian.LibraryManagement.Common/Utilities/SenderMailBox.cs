using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class SenderMailBox
    {
        // 邮件服务器地址
        public string Server{ get; set; }
        public int Port { get; set; }
        public bool UseSsl { get; set; }

        // 认证信息
        public string UserName { get; set; }
        public string AuthCode { get; set; }

        // 发件人
        //public string SenderName { get; set; }
        public string SenderAddress { get; set; }
    }
}
