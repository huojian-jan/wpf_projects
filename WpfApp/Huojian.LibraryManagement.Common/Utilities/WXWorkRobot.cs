using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Text;

namespace ShadowBot.Common.Utilities
{
    /// <summary>
    /// 企业微信自定义机器人，发送群消息
    /// https://work.weixin.qq.com/api/doc/90000/90136/91770
    /// </summary>
    public class WXWorkRobot
    {
        private string _webhook;

        public string Webhook
        {
            get { return _webhook; }
            set
            {
                if (value != _webhook)
                    _webhook = value;
            }
        }

        public WXWorkRobot(string webhook)
        {
            _webhook = webhook;
        }

        public void SendTextMessage(string content, List<string> mentionedList = null, List<string> mentionedMobileList = null)
        {
            var message = new
            {
                msgtype = "text",
                text = new
                {
                    content,
                    mentioned_list = mentionedList ?? new List<string>(),
                    mentioned_mobile_list = mentionedMobileList ?? new List<string>()
                }
            };

            SendMessage(message);
        }

        public void SendMarkdownMessage(string content)
        {
            var message = new
            {
                msgtype = "markdown",
                markdown = new
                {
                    content
                }
            };

            SendMessage(message);
        }

        private void SendMessage(object message)
        {
            var request = new RestRequest();
            request.AddHeader("Content-Type", "application/json");
            request.AddJsonBody(message);
            IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
            if (defaultWebProxy != null)
                defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            var client = new RestClient(new RestClientOptions(_webhook) { Encoding = Encoding.UTF8, Proxy = defaultWebProxy });
            var response = client.ExecuteAsync(request, Method.Post).Result;

            var responseContent = JsonConvert.DeserializeAnonymousType(response.Content.Replace("\r\n", ""), new { errcode = 0, errmsg = "" });
            if (responseContent.errcode != 0)
                throw new Exception(responseContent.errmsg);
        }
    }
}
