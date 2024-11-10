// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Twitter.BaseRequest
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace ticktick_WPF.Util.Twitter
{
  public class BaseRequest
  {
    private const string SIGNATURE_METHOD = "HMAC-SHA1";
    protected string m_UrlPrefix = string.Empty;
    private OAuthInfo m_OAuthInfo;
    public static string oauth_timestamp = "";
    public static string oauth_nonce = "";
    public static string request_token = "";
    public static string oauth_token_secret = "";
    public static string signature = "";

    public OAuthInfo OAuthInfo => this.m_OAuthInfo;

    public BaseRequest(string urlPrefix, OAuthInfo oAuthInfo)
    {
      if (string.IsNullOrEmpty(urlPrefix))
        throw new ArgumentNullException(nameof (urlPrefix));
      if (!urlPrefix.EndsWith("/"))
        urlPrefix += "/";
      this.m_UrlPrefix = urlPrefix;
      this.m_OAuthInfo = oAuthInfo;
    }

    private string GetSignatureForGetRequest(string url, Dictionary<string, string> parameters)
    {
      return this.GetSignature(new StringBuilder().Append("GET").Append("&").Append(url.EncodeRFC3986()).Append("&").Append(parameters.OrderBy<KeyValuePair<string, string>, string>((Func<KeyValuePair<string, string>, string>) (x => x.Key)).Select<KeyValuePair<string, string>, string>((Func<KeyValuePair<string, string>, string>) (x => string.Format("{0}={1}", (object) x.Key, (object) x.Value))).Join<string>("&").EncodeRFC3986()).ToString());
    }

    public virtual async Task<string> GetTwitterRequestTokenAsync(string twitterCallbackUrl)
    {
      string url = "https://api.twitter.com/oauth/request_token";
      BaseRequest.oauth_nonce = this.GetNonce();
      BaseRequest.oauth_timestamp = this.GetTimeStamp();
      Dictionary<string, string> parameters = new Dictionary<string, string>();
      parameters.Add("oauth_callback", Uri.EscapeDataString(twitterCallbackUrl));
      parameters.Add("oauth_consumer_key", this.m_OAuthInfo.ConsumerKey);
      parameters.Add("oauth_nonce", BaseRequest.oauth_nonce);
      parameters.Add("oauth_signature_method", "HMAC-SHA1");
      parameters.Add("oauth_timestamp", BaseRequest.oauth_timestamp);
      parameters.Add("oauth_version", "1.0");
      BaseRequest.signature = this.GetSignatureForGetRequest(url, parameters);
      string uriString = this.GetRequestUrl(url, parameters) + "&oauth_signature=" + Uri.EscapeDataString(BaseRequest.signature);
      HttpClient httpClient = new HttpClient();
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
      string empty = string.Empty;
      string stringAsync;
      try
      {
        stringAsync = await httpClient.GetStringAsync(new Uri(uriString));
      }
      catch (Exception ex)
      {
        return string.Empty;
      }
      string str1 = stringAsync;
      char[] chArray1 = new char[1]{ '&' };
      foreach (string str2 in str1.Split(chArray1))
      {
        char[] chArray2 = new char[1]{ '=' };
        string[] strArray = str2.Split(chArray2);
        switch (strArray[0])
        {
          case "oauth_token":
            BaseRequest.request_token = strArray[1];
            break;
          case "oauth_token_secret":
            BaseRequest.oauth_token_secret = strArray[1];
            break;
        }
      }
      return BaseRequest.request_token;
    }

    private string GetNonce() => new Random().Next(1000000000).ToString();

    private string GetTimeStamp()
    {
      return Math.Round((DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
    }

    private string GetRequestUrl(string url, Dictionary<string, string> parameters)
    {
      return string.Format("{0}?{1}", (object) url, (object) this.GetCustomParametersString(parameters));
    }

    private string GetCustomParametersString(Dictionary<string, string> parameters)
    {
      return parameters.Select<KeyValuePair<string, string>, string>((Func<KeyValuePair<string, string>, string>) (x => string.Format("{0}={1}", (object) x.Key, (object) x.Value))).Join<string>("&");
    }

    private string GetSignature(string sigBaseString)
    {
      using (HMACSHA1 hmacshA1 = new HMACSHA1(Encoding.UTF8.GetBytes(Uri.EscapeDataString(this.m_OAuthInfo.ConsumerSecret) + "&")))
        return Convert.ToBase64String(hmacshA1.ComputeHash(Encoding.UTF8.GetBytes(sigBaseString)));
    }
  }
}
