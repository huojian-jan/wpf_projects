// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Network.ApiClient
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ticktick_WPF.Resource;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Network
{
  public class ApiClient
  {
    private static readonly TimeSpan ProxyTimeout = TimeSpan.FromSeconds(5.0);
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(30.0);
    private static readonly TimeSpan UploadTimeout = TimeSpan.FromSeconds(100.0);
    private const int UploadType = 1;
    private static ProxyModel _proxyModel;
    private static HttpClient _http;
    private static HttpClient _uploadHttp;

    public static Task<T> SendFileAsync<T>(string uri, params (string, string)[] files)
    {
      return UtilHttp.SendFileAsync<T>(ApiClient.GetHttpClient(1), uri, files);
    }

    public static Task<T> PostJsonAsync<T>(string uri, object jsonObj)
    {
      return UtilHttp.PostJsonStrAsync<T>(ApiClient.GetHttpClient(), uri, JsonConvert.SerializeObject(jsonObj));
    }

    public static Task<bool> DeleteAsync(string uri)
    {
      return UtilHttp.DeleteAsync(ApiClient.GetHttpClient(), uri);
    }

    public static Task<T> PostJsonStrAsync<T>(string uri, string json)
    {
      return UtilHttp.PostJsonStrAsync<T>(ApiClient.GetHttpClient(), uri, json);
    }

    public static Task<T> GetJsonAsync<T>(string uri)
    {
      return UtilHttp.GetJsonAsync<T>(ApiClient.GetHttpClient(), uri);
    }

    public static Task<string> GetFile(string uri, string fileName)
    {
      return UtilHttp.GetFile(ApiClient.GetHttpClient(1), uri, fileName);
    }

    public static async Task<string> TestProxy(ProxyModel proxyModel)
    {
      Stopwatch watch = Stopwatch.StartNew();
      string uri = BaseUrl.GetApiDomain() + "/about/status";
      UtilLog.Info("TestProxy proxyModel=" + proxyModel?.ToString() + ": " + uri);
      using (HttpClient http = new HttpClient((HttpMessageHandler) ApiClient.CreateApiMessageProcessingHandler(proxyModel))
      {
        Timeout = ApiClient.ProxyTimeout
      })
      {
        try
        {
          using (HttpResponseMessage response = await http.GetAsync(uri))
          {
            bool flag = response.StatusCode == HttpStatusCode.OK;
            if (!flag)
              flag = await response.Content.ReadAsStringAsync() == "ok";
            if (flag)
              return "ok";
            string str = await response.Content.ReadAsStringAsync();
            UtilLog.Info("TestProxy test(" + watch.ElapsedMilliseconds.ToString() + "ms): " + uri + ", result: " + str);
            return str;
          }
        }
        catch (Exception ex)
        {
          watch.Stop();
          UtilLog.Warn("TestProxy test(" + watch.ElapsedMilliseconds.ToString() + "ms) failed: " + uri + ", ex: " + ex?.ToString());
          throw;
        }
      }
    }

    public static HttpClient GetDownloadClient()
    {
      return new HttpClient((HttpMessageHandler) ApiClient.CreateApiMessageProcessingHandler(ApiClient.GetProxyModel()))
      {
        Timeout = ApiClient.UploadTimeout
      };
    }

    private static HttpClient GetHttpClient(int type = 0)
    {
      ProxyModel proxyModel = ApiClient.GetProxyModel();
      if (!proxyModel.IsSame(ApiClient._proxyModel))
      {
        UtilLog.Info("GetHttpClient create HttpClient use " + proxyModel?.ToString() + ", old: " + ApiClient._proxyModel?.ToString());
        ApiMessageProcessingHandler processingHandler = ApiClient.CreateApiMessageProcessingHandler(proxyModel);
        ApiClient._http = new HttpClient((HttpMessageHandler) processingHandler)
        {
          Timeout = ApiClient.Timeout
        };
        ApiClient._uploadHttp = new HttpClient((HttpMessageHandler) processingHandler)
        {
          Timeout = ApiClient.UploadTimeout
        };
        ApiClient._proxyModel = proxyModel;
      }
      return type != 1 ? ApiClient._http : ApiClient._uploadHttp;
    }

    private static ApiMessageProcessingHandler CreateApiMessageProcessingHandler(
      ProxyModel proxyModel)
    {
      ApiMessageProcessingHandler processingHandler = new ApiMessageProcessingHandler();
      processingHandler.InnerHandler = (HttpMessageHandler) new HttpClientHandler()
      {
        Proxy = proxyModel?.proxy,
        UseProxy = (proxyModel?.proxy != null)
      };
      return processingHandler;
    }

    private static ProxyModel GetProxyModel()
    {
      LocalSettings settings = LocalSettings.Settings;
      return ProxyModel.Create(settings.ProxyType, settings.ProxyAddress, settings.ProxyPort, settings.ProxyUsername, settings.ProxyPassword, settings.ProxyDomain);
    }
  }
}
