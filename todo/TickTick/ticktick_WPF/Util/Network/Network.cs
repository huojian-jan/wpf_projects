// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Network.Network
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ticktick_WPF.Resource;
using TickTickModels;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Network
{
  public class Network
  {
    private static int _times;
    private static int _errortimes;

    public static async Task<bool> GetHttpWebRequestSuccess(
      string api,
      string content = "",
      List<KeyValuePair<string, string>> paramList = null,
      string auth = null,
      string mode = "POST",
      bool fulluri = false,
      bool isNeedErrorReturn = false,
      string filePath = null,
      bool needReconnect = true,
      bool defaultApi = false,
      bool useMs = false)
    {
      return (await ticktick_WPF.Util.Network.Network.GetHttpWebRequestResult(api, content, paramList, auth, mode, fulluri, isNeedErrorReturn, filePath, needReconnect, defaultApi, useMs)).Item2;
    }

    public static async Task<string> GetHttpWebRequest(
      string api,
      string content = "",
      List<KeyValuePair<string, string>> paramList = null,
      string auth = null,
      string mode = "POST",
      bool fulluri = false,
      bool isNeedErrorReturn = false,
      string filePath = null,
      bool needReconnect = true,
      bool defaultApi = false,
      bool useMs = false)
    {
      return (await ticktick_WPF.Util.Network.Network.GetHttpWebRequestResult(api, content, paramList, auth, mode, fulluri, isNeedErrorReturn, filePath, needReconnect, defaultApi, useMs)).Item1;
    }

    public static async Task<(string, bool)> GetHttpWebRequestResult(
      string api,
      string content = "",
      List<KeyValuePair<string, string>> paramList = null,
      string auth = null,
      string mode = "POST",
      bool fulluri = false,
      bool isNeedErrorReturn = false,
      string filePath = null,
      bool needReconnect = true,
      bool defaultApi = false,
      bool useMs = false)
    {
      string responseReturn = "";
      bool success = false;
      return auth == string.Empty ? (responseReturn, success) : await Task.Run<(string, bool)>((Func<Task<(string, bool)>>) (async () =>
      {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        try
        {
          using (HttpClient httpClient = HttpClientHelper.GetHttpClient())
          {
            HttpResponseMessage response = (HttpResponseMessage) null;
            try
            {
              HttpRequestMessage httpReqMsg = HttpClientHelper.GetHttpReqMsg(auth);
              string str = BaseUrl.GetApiDomain(defaultApi);
              if (useMs)
                str = str.Replace(nameof (api), "ms");
              string uriString = fulluri ? api : str + api;
              httpReqMsg.RequestUri = new Uri(uriString);
              switch (mode)
              {
                case "POST":
                  if (paramList == null || paramList.Count == 0)
                  {
                    if (filePath != null)
                    {
                      string fileName = Path.GetFileName(filePath);
                      MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                      FileStream fileStream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read);
                      StreamContent content1 = new StreamContent((Stream) fileStream, (int) fileStream.Length);
                      multipartFormDataContent.Add((HttpContent) content1, "file", fileName);
                      httpReqMsg.Method = HttpMethod.Post;
                      httpReqMsg.Content = (HttpContent) multipartFormDataContent;
                      response = await httpClient.SendAsync(httpReqMsg);
                      fileStream.Close();
                      fileStream = (FileStream) null;
                      break;
                    }
                    httpReqMsg.Method = HttpMethod.Post;
                    httpReqMsg.Content = (HttpContent) new StringContent(content, Encoding.UTF8, "application/json");
                    response = await httpClient.SendAsync(httpReqMsg);
                    break;
                  }
                  httpReqMsg.Method = HttpMethod.Post;
                  httpReqMsg.Content = (HttpContent) new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>) paramList);
                  response = await httpClient.SendAsync(httpReqMsg);
                  break;
                case "GET":
                  httpReqMsg.Method = HttpMethod.Get;
                  response = await httpClient.SendAsync(httpReqMsg);
                  break;
                case "PUT":
                  httpReqMsg.Method = HttpMethod.Put;
                  httpReqMsg.Content = (HttpContent) new StringContent(content, Encoding.UTF8, "application/json");
                  response = await httpClient.SendAsync(httpReqMsg);
                  break;
                case "DELETE":
                  httpReqMsg.Method = HttpMethod.Delete;
                  httpReqMsg.Content = (HttpContent) new StringContent(content, Encoding.UTF8, "application/json");
                  response = await httpClient.SendAsync(httpReqMsg);
                  break;
              }
              success = response != null && response.StatusCode == HttpStatusCode.OK;
              if (response != null && response.StatusCode == HttpStatusCode.OK | isNeedErrorReturn)
              {
                responseReturn = (await UtilHttp.ToStringResponseAsync(response)).Content;
                if (App.ProExpiredSyncError)
                  App.ProExpiredSyncError = false;
              }
              else if (response != null && response.StatusCode == HttpStatusCode.InternalServerError)
              {
                responseReturn = (await UtilHttp.ToStringResponseAsync(response)).Content;
                if (!string.IsNullOrEmpty(responseReturn))
                  ticktick_WPF.Util.Network.Network.HandleApiError(responseReturn);
              }
              else if (response != null)
              {
                if (response.StatusCode != HttpStatusCode.Unauthorized)
                {
                  if (response.StatusCode != HttpStatusCode.Forbidden)
                    goto label_36;
                }
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                  ++ticktick_WPF.Util.Network.Network._times;
                  if (ticktick_WPF.Util.Network.Network._times < 10)
                    return (responseReturn, success);
                }
                ticktick_WPF.Util.Network.Network._times = 0;
                UtilLog.Info("log out " + response.StatusCode.ToString() + "   " + api);
                MainWindowManager.TokenOutDate();
              }
            }
            finally
            {
              response?.Dispose();
            }
label_36:
            response = (HttpResponseMessage) null;
          }
        }
        catch (Exception ex)
        {
          sw.Stop();
          UtilLog.Warn(string.Format("Network.GetHttpWebRequest ,url {0}, {1} ms,", (object) api, (object) sw.ElapsedMilliseconds) + ex?.ToString());
        }
        ticktick_WPF.Util.Network.Network._times = 0;
        if (success)
        {
          ticktick_WPF.Util.Network.Network._errortimes = 0;
          if (App.GetWindowShow())
            MainWindowManager.SetSyncErrorStatus(NetworkError.None);
        }
        else
        {
          ++ticktick_WPF.Util.Network.Network._errortimes;
          if (ticktick_WPF.Util.Network.Network._errortimes == 5 && App.GetWindowShow())
            await ticktick_WPF.Util.Network.Network.CheckConnection();
        }
        return (responseReturn, success);
      }));
    }

    public static async Task PostLog(string uri, string content = "", string auth = null)
    {
      await Task.Run((Func<Task>) (async () =>
      {
        try
        {
          using (HttpClient httpClient = HttpClientHelper.GetHttpClient())
          {
            httpClient.DefaultRequestHeaders.Add("Authorization", "OAuth " + auth);
            httpClient.DefaultRequestHeaders.Add("User-Agent", "TickTick/W-" + Utils.GetVersion());
            httpClient.DefaultRequestHeaders.Add("x-device", Utils.GetDeviceInfo());
            MultipartFormDataContent content1 = new MultipartFormDataContent();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            StreamContent content2 = new StreamContent((Stream) stream, (int) stream.Length);
            content1.Add((HttpContent) content2, "file", "WindowsLog.txt");
            HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(new Uri(uri), (HttpContent) content1);
            stream.Close();
            stream = (MemoryStream) null;
          }
        }
        catch (Exception ex)
        {
        }
      }));
    }

    public static async Task<bool> PostFiles(string uri, List<string> files)
    {
      if (files == null || files.Count == 0)
        return true;
      HttpResponseMessage response = (HttpResponseMessage) null;
      return await Task.Run<bool>((Func<Task<bool>>) (async () =>
      {
        try
        {
          using (HttpClient httpClient = HttpClientHelper.GetHttpClient())
          {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "TickTick/W-" + Utils.GetVersion());
            httpClient.DefaultRequestHeaders.Add("x-device", Utils.GetDeviceInfo());
            List<string> stringList = files;
            // ISSUE: explicit non-virtual call
            if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) != 0)
            {
              MultipartFormDataContent content1 = new MultipartFormDataContent();
              List<FileStream> streams = new List<FileStream>();
              foreach (string file in files)
              {
                string fileName = Path.GetFileName(file);
                FileStream content2 = System.IO.File.Open(file, FileMode.Open, FileAccess.Read);
                if (content2.Length <= 0L)
                {
                  content2.Close();
                }
                else
                {
                  streams.Add(content2);
                  StreamContent content3 = new StreamContent((Stream) content2, (int) content2.Length);
                  content1.Add((HttpContent) content3, "file", fileName);
                }
              }
              response = await httpClient.PostAsync(new Uri(uri), (HttpContent) content1);
              streams.ForEach((Action<FileStream>) (s => s.Close()));
              return response != null && response.StatusCode < HttpStatusCode.InternalServerError;
            }
          }
        }
        catch (Exception ex)
        {
          UserActCollectUtils.SendException(ex, ExceptionType.OtherThread);
          return true;
        }
        return false;
      }));
    }

    private static void HandleApiError(string result)
    {
      if (string.IsNullOrEmpty(result))
        return;
      switch (JsonConvert.DeserializeObject<ErrorModel>(result)?.errorCode)
      {
        case "need_pro":
          App.Window.HandleOnNeedPro();
          break;
        case "need_upgrade_client":
          App.Window.HandleNeedUpgradeClient();
          break;
      }
    }

    public static async Task<string> GetHttpRealUrlRequest(string uri, string auth = null)
    {
      string responseReturn = "";
      HttpResponseMessage response;
      return await Task.Run<string>((Func<string>) (() =>
      {
        try
        {
          using (HttpClient httpClient = new HttpClient((HttpMessageHandler) new HttpClientHandler()
          {
            AllowAutoRedirect = false
          }))
          {
            httpClient.DefaultRequestHeaders.Add("User-Agent", "TickTick/W-" + Utils.GetVersion());
            httpClient.DefaultRequestHeaders.Add("x-device", Utils.GetDeviceInfo());
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri(uri));
            response = httpClient.GetAsync(new Uri(uri)).Result;
            responseReturn = response.Headers.GetValues("Location").First<string>();
            httpRequestMessage.Dispose();
          }
        }
        catch (Exception ex)
        {
        }
        return responseReturn;
      }));
    }

    public static async Task<bool> PostDataCollect(string uri, string json)
    {
      try
      {
        object obj = await ApiClient.PostJsonStrAsync<object>(uri, json);
      }
      catch (ApiException ex)
      {
        if (ex.Error.StatusCode >= HttpStatusCode.InternalServerError)
          return false;
        UtilLog.Warn(uri + " POST: " + json + ", ex: " + ex?.ToString());
      }
      catch (HttpRequestException ex)
      {
        return false;
      }
      catch (Exception ex)
      {
        UtilLog.Warn(ExceptionUtils.BuildExceptionMessage(ex));
      }
      return true;
    }

    public static async Task<bool> TestNetWork()
    {
      return await Task.Run<bool>((Func<Task<bool>>) (async () =>
      {
        Stopwatch watch = new Stopwatch();
        watch.Start();
        using (HttpClient httpClient = HttpClientHelper.GetHttpClient())
        {
          try
          {
            using (HttpResponseMessage response = await httpClient.GetAsync(BaseUrl.GetApiDomain() + "/about/status"))
            {
              bool flag = response.StatusCode == HttpStatusCode.OK;
              if (!flag)
                flag = await response.Content.ReadAsStringAsync() == "ok";
              if (flag)
                return true;
              string str = await response.Content.ReadAsStringAsync();
              UtilLog.Info("TestNextWork (" + watch.ElapsedMilliseconds.ToString() + " ms) , result: " + str);
              return false;
            }
          }
          catch (Exception ex)
          {
            watch.Stop();
            UtilLog.Warn("TestNextWork (" + watch.ElapsedMilliseconds.ToString() + " ms), ex: " + ex?.ToString());
          }
        }
        return false;
      }));
    }

    public static async Task CheckConnection()
    {
      if (!Utils.IsNetworkAvailable())
      {
        Utils.Toast(Utils.GetString("NoNetwork"));
        MainWindowManager.SetSyncErrorStatus(NetworkError.NoNetwork);
      }
      else if (!await ticktick_WPF.Util.Network.Network.TestNetWork())
        MainWindowManager.SetSyncErrorStatus(LocalSettings.Settings.ProxyType == 0 ? NetworkError.Error : NetworkError.CheckProxy);
      else
        MainWindowManager.SetSyncErrorStatus(NetworkError.None);
    }
  }
}
