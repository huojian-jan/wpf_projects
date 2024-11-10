// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.GoogleLogin
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Views;

#nullable disable
namespace ticktick_WPF.Util
{
  public class GoogleLogin
  {
    private LoginDialog _dialog;
    private const string clientID = "366263775281-fbdi26qafnn9k6aoli4893k87srqun06.apps.googleusercontent.com";
    private const string clientSecret = "z4A-okmmX8bQB3iYBc4rkHqM";
    private const string authorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
    private const string tokenEndpoint = "https://www.googleapis.com/oauth2/v4/token";
    private const string userInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";

    public GoogleLogin(LoginDialog dialog) => this._dialog = dialog;

    public static int GetRandomUnusedPort()
    {
      TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
      tcpListener.Start();
      int port = ((IPEndPoint) tcpListener.LocalEndpoint).Port;
      tcpListener.Stop();
      return port;
    }

    public async void PerformLogin()
    {
      string state = GoogleLogin.randomDataBase64url(32U);
      string code_verifier = GoogleLogin.randomDataBase64url(32U);
      string str1 = GoogleLogin.base64urlencodeNoPadding(GoogleLogin.sha256(code_verifier));
      string redirectURI = string.Format("http://{0}:{1}/", (object) IPAddress.Loopback, (object) GoogleLogin.GetRandomUnusedPort());
      this.output("redirect URI: " + redirectURI);
      HttpListener http = new HttpListener();
      http.Prefixes.Add(redirectURI);
      this.output("Listening..");
      try
      {
        http.Start();
      }
      catch (HttpListenerException ex)
      {
        new CustomerDialog(Utils.GetString("AccessDenied"), Utils.GetString("AccessDeniedMessage"), MessageBoxButton.OK).ShowDialog();
        state = (string) null;
        code_verifier = (string) null;
        redirectURI = (string) null;
        return;
      }
      Process.Start(string.Format("{0}?response_type=code&scope=https://www.googleapis.com/auth/userinfo.profile+https://www.googleapis.com/auth/userinfo.email&redirect_uri={1}&client_id={2}&state={3}&code_challenge={4}&code_challenge_method={5}", (object) "https://accounts.google.com/o/oauth2/v2/auth", (object) Uri.EscapeDataString(redirectURI), (object) "366263775281-fbdi26qafnn9k6aoli4893k87srqun06.apps.googleusercontent.com", (object) state, (object) str1, (object) "S256"));
      HttpListenerContext contextAsync = await http.GetContextAsync();
      HttpListenerResponse response = contextAsync.Response;
      byte[] bytes = Encoding.UTF8.GetBytes(string.Format("<html><head><meta http-equiv='refresh' content='10;url=https://google.com'></head><body>Login successfully, please wait...</body></html>"));
      response.ContentLength64 = (long) bytes.Length;
      Stream responseOutput = response.OutputStream;
      responseOutput.WriteAsync(bytes, 0, bytes.Length).ContinueWith((Action<Task>) (task =>
      {
        responseOutput.Close();
        http.Stop();
        Console.WriteLine("HTTP server stopped.");
      }));
      if (contextAsync.Request.QueryString.Get("error") != null)
      {
        this.output(string.Format("OAuth authorization error: {0}.", (object) contextAsync.Request.QueryString.Get("error")));
        state = (string) null;
        code_verifier = (string) null;
        redirectURI = (string) null;
      }
      else if (contextAsync.Request.QueryString.Get("code") == null || contextAsync.Request.QueryString.Get("state") == null)
      {
        this.output("Malformed authorization response. " + contextAsync.Request.QueryString?.ToString());
        state = (string) null;
        code_verifier = (string) null;
        redirectURI = (string) null;
      }
      else
      {
        string code = contextAsync.Request.QueryString.Get("code");
        string str2 = contextAsync.Request.QueryString.Get("state");
        if (str2 != state)
        {
          this.output(string.Format("Received request with invalid state ({0})", (object) str2));
          state = (string) null;
          code_verifier = (string) null;
          redirectURI = (string) null;
        }
        else
        {
          this.output("Authorization code: " + code);
          this.performCodeExchange(code, code_verifier, redirectURI);
          state = (string) null;
          code_verifier = (string) null;
          redirectURI = (string) null;
        }
      }
    }

    private async void performCodeExchange(string code, string code_verifier, string redirectURI)
    {
      HttpWebRequest tokenRequest;
      if (this._dialog == null)
      {
        tokenRequest = (HttpWebRequest) null;
      }
      else
      {
        this.output("Exchanging code for tokens...");
        string requestUriString = "https://www.googleapis.com/oauth2/v4/token";
        string s = string.Format("code={0}&redirect_uri={1}&client_id={2}&code_verifier={3}&client_secret={4}&scope=&grant_type=authorization_code", (object) code, (object) Uri.EscapeDataString(redirectURI), (object) "366263775281-fbdi26qafnn9k6aoli4893k87srqun06.apps.googleusercontent.com", (object) code_verifier, (object) "z4A-okmmX8bQB3iYBc4rkHqM");
        tokenRequest = (HttpWebRequest) WebRequest.Create(requestUriString);
        tokenRequest.Method = "POST";
        tokenRequest.ContentType = "application/x-www-form-urlencoded";
        tokenRequest.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        byte[] bytes = Encoding.ASCII.GetBytes(s);
        tokenRequest.ContentLength = (long) bytes.Length;
        StreamReader reader;
        try
        {
          Stream stream = tokenRequest.GetRequestStream();
          await stream.WriteAsync(bytes, 0, bytes.Length);
          stream.Close();
          reader = new StreamReader((await tokenRequest.GetResponseAsync()).GetResponseStream());
          try
          {
            string endAsync = await reader.ReadToEndAsync();
            this.output(endAsync);
            string accessToken = JsonConvert.DeserializeObject<Dictionary<string, string>>(endAsync)["access_token"];
            this.userinfoCall(code, accessToken);
          }
          finally
          {
            reader?.Dispose();
          }
          reader = (StreamReader) null;
          stream = (Stream) null;
        }
        catch (WebException ex)
        {
          if (ex.Status == WebExceptionStatus.ProtocolError)
          {
            if (ex.Response is HttpWebResponse response)
            {
              reader = new StreamReader(response.GetResponseStream());
              try
              {
                int num = (int) System.Windows.Forms.MessageBox.Show(string.Format("GoogleLoginErrorCode: {0} \n {1}", (object) response.StatusCode, (object) await reader.ReadToEndAsync()));
              }
              finally
              {
                reader?.Dispose();
              }
              reader = (StreamReader) null;
            }
            response = (HttpWebResponse) null;
          }
          else
          {
            int num1 = (int) System.Windows.Forms.MessageBox.Show(ex.Message);
          }
        }
        tokenRequest = (HttpWebRequest) null;
      }
    }

    private async void userinfoCall(string code, string accessToken)
    {
      await this._dialog?.PerformGoogleByAccessToken(code, accessToken);
    }

    public void output(string output) => Console.WriteLine(output);

    public static string randomDataBase64url(uint length)
    {
      RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider();
      byte[] buffer = new byte[(int) length];
      byte[] data = buffer;
      cryptoServiceProvider.GetBytes(data);
      return GoogleLogin.base64urlencodeNoPadding(buffer);
    }

    public static byte[] sha256(string inputStirng)
    {
      return new SHA256Managed().ComputeHash(Encoding.ASCII.GetBytes(inputStirng));
    }

    public static string base64urlencodeNoPadding(byte[] buffer)
    {
      return Convert.ToBase64String(buffer).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    public void ClearLogin() => this._dialog = (LoginDialog) null;
  }
}
