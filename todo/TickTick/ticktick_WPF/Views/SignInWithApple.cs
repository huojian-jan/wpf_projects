// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.SignInWithApple
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views
{
  public class SignInWithApple
  {
    private LoginDialog _parentDialog;
    private static readonly object _locker = new object();
    private static SignInWithApple _instance;

    public SignInWithApple() => App.RegisterURIProtocol();

    public static SignInWithApple GetInstance(LoginDialog dialog)
    {
      if (SignInWithApple._instance == null)
      {
        lock (SignInWithApple._locker)
        {
          if (SignInWithApple._instance == null)
            SignInWithApple._instance = new SignInWithApple();
        }
      }
      SignInWithApple._instance._parentDialog = dialog;
      return SignInWithApple._instance;
    }

    public void Start()
    {
      UriNotifier.Uri -= new EventHandler<UriModel>(this.OnUri);
      UriNotifier.Uri += new EventHandler<UriModel>(this.OnUri);
      Utils.TryProcessStartUrl("https://appleid.apple.com/auth/authorize?client_id=com.ticktick.task.service&redirect_uri=https%3A%2F%2F" + BaseUrl.Domain + "%2Fsign%2Fwin%2Fapple&response_type=code%20id_token&state=Lw%3D%3D&scope=email%20name&response_mode=form_post");
    }

    private void OnUri(object sender, UriModel e)
    {
      if (!e.Path.StartsWith("sign/apple"))
        return;
      UriNotifier.Uri -= new EventHandler<UriModel>(this.OnUri);
      SignInWithAppleParma e1 = new SignInWithAppleParma();
      if (e.Parmas.ContainsKey("code"))
        e1.accessToken = e.Parmas["code"];
      if (e.Parmas.ContainsKey("user"))
      {
        string parma = e.Parmas["user"];
        try
        {
          if (!string.IsNullOrEmpty(parma))
          {
            JObject jobject = JObject.Parse(parma);
            e1.name = string.Format("{0} {1}", (object) jobject["name"][(object) "firstName"], (object) jobject["name"][(object) "lastName"]);
            e1.email = jobject["email"].ToString();
          }
        }
        catch
        {
        }
      }
      if (e.Parmas.ContainsKey("id_token"))
      {
        string parma = e.Parmas["id_token"];
        try
        {
          foreach (Claim claim in new JwtSecurityTokenHandler().ReadJwtToken(parma).Claims)
          {
            if (claim.Type == "sub")
              e1.uId = claim.Value;
          }
        }
        catch (Exception ex)
        {
          UtilLog.Error("JWT Decode Exception " + ex.Message);
        }
      }
      this.TryLoginApple(e1);
    }

    private async void TryLoginApple(SignInWithAppleParma e)
    {
      this._parentDialog?.TryLoginApple(e);
    }

    public void ClearLogin() => this._parentDialog = (LoginDialog) null;
  }
}
