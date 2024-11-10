// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Network.HttpClientHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Net;
using System.Net.Http;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util.Network
{
  public static class HttpClientHelper
  {
    public const string DidaCertKey = "30 82 01 0a 02 82 01 01 00 bd 55 69 85 59 a9 38 84 53 11 1b 8d 2b 38 12 d4 07 fc 53 4b db 8e c0 90 3d a6 3d 8e 38 1b 06 1a ee 8a b7 b3 04 b0 af 9e 6f ed 2d 95 5c 0d 40 13 53 06 97 c0 45 56 7c fa 5c 80 75 04 bf 53 00 af a4 34 c8 51 04 17 38 ba 10 12 f3 c5 2f 51 52 08 85 50 48 87 1c 53 0c 6c 66 c9 b2 90 0d fe 90 5b b7 10 89 7c 2c e0 4c 66 ae c5 77 04 c4 a6 bd 39 73 f7 59 e4 e7 ff 7a ac fc 2c 6f 5a 92 c6 58 42 ac 25 6b 1a e4 cc 56 91 33 07 a6 8e 7f 53 90 3e da 8d 09 b2 27 6f d0 cd c1 c5 bd 9c 06 54 91 9a c7 ec 7b 8b 23 ab e8 0e a6 2a 33 4e cd bc 1d 9d 8b fa 09 76 4b 6f d7 d8 df 2c ec af e1 3d fa 61 56 27 7f 2b a9 b6 4f 43 54 e3 9c c9 5e 13 6e 38 a2 45 4c 84 4e cf a4 eb d5 5c c8 17 e0 72 d7 13 eb 5a b4 14 c0 01 e0 96 73 0d 93 4e ec e7 ad b1 28 31 68 1f 0b da ad 24 ea 1d 4d 4d 6c 69 1c f1 02 03 01 00 01";
    public const string TickCertKey = "30 82 02 0a 02 82 02 01 00 e6 42 78 89 29 5a 8a 8f b7 5b 15 35 83 d1 17 e1 e1 b1 74 45 f0 dc 0a 61 80 f1 08 07 fc bf 92 02 f8 00 ea 26 57 7c 52 9b 66 bf 77 e4 98 71 9f c5 c6 c4 16 d4 84 8e 12 fd e4 51 9d ab d4 e1 39 7b 72 03 69 86 47 2f cb 67 93 9a d8 6a d8 8e b8 d1 ea b8 f2 e7 62 35 9d fe 62 e2 87 78 8a 2a 2d dd a9 27 89 13 e8 a9 48 7c ed d0 9c 12 98 75 ae fa 89 d4 63 0f b1 e5 fd a4 e1 ff 32 55 91 f7 49 e6 63 b6 b0 67 cf 21 a3 00 03 a1 1c 26 d7 39 d4 64 af ad 6f 4a b8 13 3c 26 e1 dc e3 4b 7f 5e db 72 5c df 93 e4 a9 af 7b 8a a1 c6 f4 0e ed d2 42 13 c1 c0 e1 63 11 46 ac 28 71 d6 fa 29 ea 84 14 d5 96 f4 4c 25 9b 09 6c 42 67 71 2a 40 ba b2 cd c5 14 97 60 ca f0 78 72 b5 f2 de 6d 33 21 bc 78 60 2a 3b e5 b7 52 35 b5 e9 1d b7 cb be 33 6d 4e 75 8a 68 02 a0 3a c7 84 f9 a6 88 f2 f4 02 c5 f3 8f 8b 34 fe 09 4e b3 a4 69 e5 e4 4a 4a 22 1a ac de 53 0f 37 9d 5c 0a dd 25 f6 2d c0 3a 92 ab 6e 57 e0 59 fa ff 0e 03 95 4d 8a 63 14 44 71 36 1a ed 1b f2 35 31 d5 61 56 1e fe 2c f1 69 28 e9 55 3c 97 2c ee 40 cd cc b0 d3 2d c4 81 fe aa 79 13 77 82 a2 62 c9 45 12 b4 de 5c ca 75 54 7d 2f 6b 4a 8c df 1e 01 10 60 b5 fe e1 81 a2 d3 56 fc d9 25 53 ec b5 46 eb 6c 3d cf fa 08 23 5d 64 df fe b6 fa 39 33 8a 51 b5 41 93 20 b8 89 b4 2a b3 5f 81 f5 7e 13 f5 91 98 bb 37 ac ce 6e eb 62 73 59 09 12 54 78 a3 77 64 7b 15 5f 57 ff bb ef 04 a9 cb cf 0f 2c 6b ea f3 44 95 ee ea 8f fe dd 49 6c d5 21 c1 aa e8 1d f7 59 11 48 50 9f 16 87 e5 da 1c de 84 36 a3 81 0b 1d 8f 60 16 59 62 5e d6 6d 28 1e b8 0b 6f 44 95 51 c0 eb 8d ac 90 d5 da 33 00 f0 ab 1e cb 71 d2 9e f2 79 d1 53 ef 58 19 ef 77 02 03 01 00 01";

    public static HttpRequestMessage GetHttpReqMsg(string auth)
    {
      HttpRequestMessage httpReqMsg = new HttpRequestMessage();
      httpReqMsg.Headers.Add("Authorization", "OAuth " + auth);
      httpReqMsg.Headers.Add("User-Agent", "TickTick/W-" + Utils.GetVersion());
      httpReqMsg.Headers.Add("x-device", Utils.GetDeviceInfo() ?? "");
      httpReqMsg.Headers.Add("traceid", Utils.GetGuid());
      httpReqMsg.Headers.Add("hl", Utils.GetLanguage());
      httpReqMsg.Headers.Add("x-tz", TimeZoneData.LocalTimeZoneModel.TimeZoneName);
      return httpReqMsg;
    }

    public static bool TryGetEndPoint(string addr, string port, out string endPoint)
    {
      endPoint = (string) null;
      Uri result;
      if (!Uri.TryCreate(addr.Contains("://") ? addr : "http://" + addr, UriKind.Absolute, out result) || !(result.Scheme == Uri.UriSchemeHttp) || !ushort.TryParse(port, out ushort _))
        return false;
      endPoint = result.Host + ":" + port;
      return true;
    }

    public static HttpClient GetHttpClient()
    {
      LocalSettings settings = LocalSettings.Settings;
      switch (settings.ProxyType)
      {
        case 1:
          string endPoint;
          if (!HttpClientHelper.TryGetEndPoint(settings.ProxyAddress, settings.ProxyPort, out endPoint))
            return new HttpClient();
          WebProxy webProxy = new WebProxy(endPoint);
          if (!string.IsNullOrEmpty(settings.ProxyUsername) && !string.IsNullOrEmpty(settings.ProxyPassword))
          {
            string password = Utils.Base64Decode(settings.ProxyPassword);
            if (string.IsNullOrEmpty(settings.ProxyDomain))
            {
              NetworkCredential networkCredential = new NetworkCredential(settings.ProxyUsername, password);
              webProxy.Credentials = (ICredentials) networkCredential;
            }
            else
            {
              NetworkCredential networkCredential = new NetworkCredential(settings.ProxyUsername, password, settings.ProxyDomain);
              webProxy.Credentials = (ICredentials) networkCredential;
            }
          }
          return new HttpClient((HttpMessageHandler) new HttpClientHandler()
          {
            Proxy = (IWebProxy) webProxy,
            UseProxy = true
          });
        case 2:
          return new HttpClient((HttpMessageHandler) new HttpClientHandler()
          {
            Proxy = WebRequest.GetSystemWebProxy(),
            UseProxy = true
          });
        default:
          return new HttpClient((HttpMessageHandler) new HttpClientHandler()
          {
            UseProxy = false
          });
      }
    }
  }
}
