using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class TimeoutWebClient : WebClient
    {
        private readonly int _timeout;
        private readonly CookieContainer _cookieContainer;

        public TimeoutWebClient(int timeout)
        {
            _timeout = timeout;
            _cookieContainer = new CookieContainer();
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
            if (defaultWebProxy != null)
                defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;

            var webRequest = (HttpWebRequest)base.GetWebRequest(address);
            webRequest.CookieContainer = _cookieContainer;
            webRequest.Timeout = _timeout;
            webRequest.ReadWriteTimeout = _timeout;
            webRequest.Proxy = defaultWebProxy;
            return webRequest;
        }
    }

    public class TimeoutHttpClient : HttpClient
    {
        public TimeoutHttpClient() : base(new HttpClientHandler()
        {
            UseCookies = true,
            UseProxy = true,
            CookieContainer = new CookieContainer(),
            Proxy = new Func<IWebProxy>(() =>
            {
                IWebProxy defaultWebProxy = WebRequest.DefaultWebProxy;
                if (defaultWebProxy != null)
                    defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
                return defaultWebProxy;
            })(),
        })
        {
            Timeout = TimeSpan.FromSeconds(3);
        }
    }
}
