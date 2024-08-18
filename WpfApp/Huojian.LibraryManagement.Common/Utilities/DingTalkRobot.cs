using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    /// <summary>
    /// 钉钉自定义机器人，发送群消息
    /// https://ding-doc.dingtalk.com/doc#/serverapi2/qf2nxq
    /// </summary>
    public class DingTalkRobot
    {
        private string _webhook;        // 机器人的云端地址，让我们能通过网络定位找到机器人

        private DingTalkRobotSecurityPolicy _dingTalkRobotSecurityPolicy;   // 与机器人协商好的安全约定，可有多条约定
        private string _keyword;        // CustomKeyword策略下：发送的内容必须带有此关键字
        private string _secret;         // AttachSign策略下：必须使用此凭证签名

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

        public string Keyword
        {
            get { return _keyword; }
            set
            {
                if (value != _keyword)
                    _keyword = value;
            }
        }

        public DingTalkRobotSecurityPolicy DingTalkRobotSecurityPolicy
        {
            get { return _dingTalkRobotSecurityPolicy; }
            set
            {
                if (value != _dingTalkRobotSecurityPolicy)
                    _dingTalkRobotSecurityPolicy = value;
            }
        }

        public DingTalkRobot(string webhook,
                             DingTalkRobotSecurityPolicy dingTalkRobotSecurityPolicy = DingTalkRobotSecurityPolicy.AttachSign,
                             string keyword = null,
                             string secret = null,
                             List<string> ips = null)
        {
            _webhook = webhook;

            _dingTalkRobotSecurityPolicy = dingTalkRobotSecurityPolicy;
            _keyword = keyword ?? string.Empty;
            _secret = secret ?? string.Empty;
        }

        public void SendTextMessage(string content, List<string> atMobiles = null, bool isAtAll = false)
        {
            var message = new
            {
                msgtype = "text",
                text = new
                {
                    content
                },
                at = new
                {
                    atMobiles = atMobiles ?? new List<string>(),
                    isAtAll
                }
            };

            SendMessage(message);
        }

        public void SendMarkdownMessage(string title, string text, List<string> atMobiles = null, bool isAtAll = false)
        {
            var message = new
            {
                msgtype = "markdown",
                markdown = new
                {
                    title,
                    text
                },
                at = new
                {
                    atMobiles = atMobiles ?? new List<string>(),
                    isAtAll
                }
            };

            SendMessage(message);
        }

        private void SendMessage(object message)
        {
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(message);

            var requestUrl = ReloadRequestUrl();
            IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
            if (defaultWebProxy != null)
                defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            var client = new RestClient(new RestClientOptions(requestUrl) { Encoding = Encoding.UTF8, Proxy = defaultWebProxy });
            var response = client.ExecuteAsync(request, Method.Post).Result;

            var responseContent = JsonConvert.DeserializeAnonymousType(response.Content.Replace("\r\n", ""), new { errcode = 0, errmsg = "" });
            if (responseContent.errcode != 0)
                throw new Exception(responseContent.errmsg);
        }

        private string ReloadRequestUrl()
        {
            if (_dingTalkRobotSecurityPolicy.HasFlag(DingTalkRobotSecurityPolicy.AttachSign))
            {
                var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var timestamp = (long)Math.Round(ts.TotalMilliseconds);
                var stringToSign = $"{timestamp}\n{_secret}";

                var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_secret));
                var signDigestBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));

                var signDigest = System.Net.WebUtility.UrlEncode(Convert.ToBase64String(signDigestBytes));

                return $"{_webhook}&timestamp={timestamp}&sign={signDigest}";
            }
            else
            {
                return _webhook;
            }
        }
    }

    // 安全策略支持叠加
    public enum DingTalkRobotSecurityPolicy
    {
        AttachSign = 1,
        CustomKeyword = 2
    }
}
