using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace ShadowBot.Common.Utilities
{
    public class FeiShuRobot
    {
        private string _webhook;        // 机器人的云端地址，让我们能通过网络定位找到机器人
        private string _secret;         // 若勾选签名校验，则必须输入密钥

        public string Webhook
        {
            get { return _webhook; }
            set
            {
                if (value != _webhook)
                    _webhook = value;
            }
        }

        public string Secret
        {
            get { return _secret; }
            set
            {
                if (value != _secret)
                    _secret = value;
            }
        }

        public FeiShuRobot(string webhook, string secret)
        {
            _webhook = webhook;
            _secret = secret;
        }

        public void SendTextMessage(string content)
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = (long)ts.TotalSeconds;

            var message = new
            {
                timestamp = timestamp,
                sign = GenerateSign(timestamp),
                msg_type = "text",
                content = new
                {
                    text = content
                }
            };

            SendMessage(message);
        }

        public void SendPostMessage(string title, List<object> text)
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var timestamp = (long)ts.TotalSeconds;

            var message = new
            {
                timestamp = timestamp,
                sign = GenerateSign(timestamp),
                msg_type = "post",
                content = new
                {
                    post = new
                    {
                        zh_cn = new
                        {
                            title = title,
                            content = text
                        }
                    }
                }
            };

            SendMessage(message);
        }

        private string GenerateSign(long timestamp)
        {
            if (!String.IsNullOrEmpty(_secret))
            {
                string stringToSign = $"{timestamp}\n{_secret}";
                //stringToSign = $"{1636978723}\n{_secret}";
                using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(stringToSign)))
                {
                    var signDigestBytes = hmacsha256.ComputeHash(new byte[] { });

                    //var signDigest = HttpUtility.UrlEncode(Convert.ToBase64String(signDigestBytes), Encoding.UTF8);
                    var signDigest = Convert.ToBase64String(signDigestBytes);

                    return signDigest;
                }
            }

            return String.Empty;
        }

        private void SendMessage(object message)
        {
            var request = new RestRequest();
            request.AddHeader("Content-Type", "text/plain");
            request.AddJsonBody(message);
            IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
            if (defaultWebProxy != null)
                defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            var client = new RestClient(new RestClientOptions(_webhook) { Encoding = Encoding.UTF8, Proxy = defaultWebProxy });
            var response = client.ExecuteAsync(request, Method.Post).Result;

            var responseContent = JsonConvert.DeserializeAnonymousType(response.Content.Replace("\r\n", ""), new { code = 0, msg = "" });
            if (responseContent.code != 0)
                throw new Exception(responseContent.msg);
        }
    }
}
