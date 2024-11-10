// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.ThirdLoginWebDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Twitter;

#nullable disable
namespace ticktick_WPF.Views
{
  public class ThirdLoginWebDialog : Window, IComponentConnector
  {
    private const int INTERNET_OPTION_END_BROWSER_SESSION = 42;
    private string FBClient_id = "687713684576416";
    private string FBRedirect_uri = "https://ticktick.com/sign/facebook";
    private string FBUrl = "https://www.facebook.com/dialog/oauth?client_id={0}&redirect_uri={1}&state=THc&response_type=token&scope=email";
    private string GOOGLE_ACCESS_TOKEN_TAG = "GOOGLE_ACCESS_TOKEN_TAG_";
    private string GOOGLE_REFRESH_TOKEN_TAG = "GOOGLE_REFRESH_TOKEN_TAG_";
    private string GRANT_TYPE = "authorization_code";
    private string GRANT_TYPE_REFRESH = "refresh_token";
    private string OAUTH_SCOPE = "https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile";
    private string OAUTH_URL = "https://accounts.google.com/o/oauth2/auth";
    private string qqClient_id = "101139917";
    private string qqRedirect_uri = "https://dida365.com/sign/qq";
    private string qqUrl = "https://graph.qq.com/oauth2.0/show?which=Login&display=pc&client_id={0}&response_type=token&redirect_uri={1}&state=Lw";
    private string REDIRECT_URI = "http://localhost";
    private string REQUEST_EMIAL_ADDRESS_BASE_URL = "https://www.googleapis.com/oauth2/v1/userinfo?alt=json&access_token=";
    private string TOKEN_URL = "https://accounts.google.com/o/oauth2/token";
    private string TwitterCallBackUri = "oob";
    private string TwitterConsumerKey = "cQVZO67miyWT5xDhOtcuOVUYH";
    private string TwitterConsumerSecret = "rI6OxEvpGy0XfjTw995uOgagQHhQ4Nk0vqdk8IOHp5pL2QjppK";
    private string WeiBoClient_id = "18974718";
    private string WeiBoRedirect_uri = "https://www.dida365.com/sign/weibo";
    private string WeiBoUrl = "https://api.weibo.com/oauth2/authorize?client_id={0}&response_type=code&redirect_uri={1}";
    private string WXAppid = "wxf1429a73d311aad4";
    private string WXRedirect_uri = "https://dida365.com/sign/wechat";
    private string WXUrl = "https://open.weixin.qq.com/connect/qrconnect?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_login";
    internal Grid webGrid;
    internal WebBrowser loginWebBrowser;
    internal ProgressBar loginProgressBar;
    internal TextBlock LoginErrorText;
    internal Grid PinGrid;
    internal TextBox pinTextBox;
    internal Button PinLoginButton;
    internal StackPanel resultGrid;
    internal ProgressBar resultProgressBar;
    internal Image logoImage;
    private bool _contentLoaded;

    public ThirdLoginWebDialog() => this.InitializeComponent();

    public ThirdLoginWebDialog(string mode)
    {
      this.InitializeComponent();
      ThirdLoginWebDialog._DeleteEveryCookie(new Uri("https://www.facebook.com"));
      ThirdLoginWebDialog._DeleteEveryCookie(new Uri("https://www.twitter.com"));
      ThirdLoginWebDialog._DeleteEveryCookie(new Uri("https://api.twitter.com"));
      ThirdLoginWebDialog._DeleteEveryCookie(new Uri("https://www.ticktick.com"));
      ThirdLoginWebDialog._DeleteEveryCookie(new Uri("https://accounts.google.com"));
      CookieHelper.ClearCookie();
      WebBrowserZoomInvoker.AddZoomInvoker(this.loginWebBrowser);
      WebBrowserHelper.SetErrorSilent(this.loginWebBrowser);
      switch (mode)
      {
        case "login_Facebook":
          this.loginWebBrowser.Navigate(string.Format(this.FBUrl, (object) this.FBClient_id, (object) this.FBRedirect_uri));
          this.loginWebBrowser.Navigating += new NavigatingCancelEventHandler(this.FB_LoginWebBrowser_Navigating);
          LocalSettings.Settings.AccountType = "facebook";
          break;
        case "login_Twitter":
          this.login_Twitter();
          this.loginWebBrowser.Navigating += new NavigatingCancelEventHandler(this.TW_LoginWebBrowser_Navigating);
          LocalSettings.Settings.AccountType = "twitter";
          break;
        case "login_Weibo":
          this.loginWebBrowser.Navigate(string.Format(this.WeiBoUrl, (object) this.WeiBoClient_id, (object) this.WeiBoRedirect_uri));
          this.loginWebBrowser.Navigating += new NavigatingCancelEventHandler(this.WeiBo_LoginWebBrowser_Navigating);
          LocalSettings.Settings.AccountType = "weibo";
          this.Topmost = false;
          break;
        case "login_QQ":
          this.loginWebBrowser.Navigate(string.Format(this.qqUrl, (object) this.qqClient_id, (object) this.qqRedirect_uri));
          this.loginWebBrowser.Navigating += new NavigatingCancelEventHandler(this.QQ_LoginWebBrowser_Navigating);
          LocalSettings.Settings.AccountType = "tecent";
          break;
        case "login_Wechat":
          this.loginWebBrowser.Navigate(string.Format(this.WXUrl, (object) this.WXAppid, (object) this.WXRedirect_uri));
          this.loginWebBrowser.Navigating += new NavigatingCancelEventHandler(this.WX_LoginWebBrowser_Navigating);
          LocalSettings.Settings.AccountType = "wechat";
          break;
      }
      this.logoImage.Source = (ImageSource) new BitmapImage(new Uri("pack://application:,,,/Assets/logo_dida.png"));
    }

    private void TryShowLoginErrorHelper()
    {
      if (Environment.OSVersion.Version.Major != 6)
        return;
      this.LoginErrorText.Visibility = Visibility.Visible;
    }

    private async void login_Twitter()
    {
      ThirdLoginWebDialog thirdLoginWebDialog = this;
      Uri source = new Uri("https://api.twitter.com/oauth/authorize?oauth_token=" + await new TwitterClient(new OAuthInfo()
      {
        ConsumerKey = thirdLoginWebDialog.TwitterConsumerKey,
        ConsumerSecret = thirdLoginWebDialog.TwitterConsumerSecret
      }).GetTwitterRequestTokenAsync(thirdLoginWebDialog.TwitterCallBackUri));
      thirdLoginWebDialog.loginWebBrowser.Navigate(source);
      thirdLoginWebDialog.loginWebBrowser.Navigating += new NavigatingCancelEventHandler(thirdLoginWebDialog.TW_LoginWebBrowser_Navigating);
    }

    private async void TW_LoginWebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
    {
      this.loginProgressBar.Visibility = Visibility.Visible;
      if (!(e.Uri.ToString() == "https://api.twitter.com/oauth/authorize"))
        return;
      this.PinGrid.Visibility = Visibility.Visible;
    }

    private void pinTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
      if (this.pinTextBox == null || this.PinLoginButton == null)
        return;
      if (this.pinTextBox.Text.Length != 0)
        this.PinLoginButton.IsEnabled = true;
      else
        this.PinLoginButton.IsEnabled = false;
    }

    private async void PinLoginButton_Click(object sender, RoutedEventArgs e)
    {
      ThirdLoginWebDialog thirdLoginWebDialog = this;
      if (thirdLoginWebDialog.pinTextBox.Text.Length == 0)
        return;
      thirdLoginWebDialog.loginProgressBar.Visibility = Visibility.Visible;
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("https://api.twitter.com/oauth/access_token", auth: string.Format("Authorization: OAuth realm=\"\",oauth_consumer_key=\"{0}\",oauth_nonce=\"{1}\",oauth_signature_method=\"HMAC - SHA1\",oauth_timestamp=\"{2}\",oauth_token=\"{3}\",oauth_verifier=\"{4}\",oauth_version=\"1.0\",oauth_signature=\"{5}\"", (object) thirdLoginWebDialog.TwitterConsumerKey, (object) BaseRequest.oauth_nonce, (object) BaseRequest.oauth_timestamp, (object) BaseRequest.request_token, (object) thirdLoginWebDialog.pinTextBox.Text, (object) BaseRequest.signature), fulluri: true, isNeedErrorReturn: true);
      if (httpWebRequest.Contains("oauth_token_secret"))
      {
        string access_token = httpWebRequest.Substring(httpWebRequest.IndexOf("oauth_token") + 12, httpWebRequest.IndexOf("&", httpWebRequest.IndexOf("oauth_token")) - (httpWebRequest.IndexOf("oauth_token") + 12));
        string accessTokenSecret = httpWebRequest.Substring(httpWebRequest.IndexOf("oauth_token_secret") + 19, httpWebRequest.IndexOf("&", httpWebRequest.IndexOf("oauth_token_secret")) - (httpWebRequest.IndexOf("oauth_token_secret") + 19));
        thirdLoginWebDialog.loginProgressBar.Visibility = Visibility.Collapsed;
        thirdLoginWebDialog.webGrid.Visibility = Visibility.Collapsed;
        thirdLoginWebDialog.resultGrid.Visibility = Visibility.Visible;
        thirdLoginWebDialog.ThirdLoginSuccess("login_Twitter", access_token: access_token, accessTokenSecret: accessTokenSecret);
      }
      else
      {
        thirdLoginWebDialog.pinTextBox.Text = "";
        CustomerDialog customerDialog = new CustomerDialog(Application.Current?.FindResource((object) "LoginFailed").ToString(), Application.Current?.FindResource((object) "VerificationFailedHaveTry").ToString(), MessageBoxButton.OK);
        customerDialog.Owner = Window.GetWindow((DependencyObject) thirdLoginWebDialog);
        customerDialog.ShowDialog();
        thirdLoginWebDialog.login_Twitter();
      }
    }

    private async void FB_LoginWebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
    {
      ThirdLoginWebDialog thirdLoginWebDialog = this;
      if (!e.Uri.ToString().Contains("access_token"))
        return;
      string access_token = e.Uri.ToString().Substring(e.Uri.ToString().IndexOf("access_token") + 13, e.Uri.ToString().IndexOf("&", e.Uri.ToString().IndexOf("access_token")) - (e.Uri.ToString().IndexOf("access_token") + 13));
      thirdLoginWebDialog.loginWebBrowser.Navigating -= new NavigatingCancelEventHandler(thirdLoginWebDialog.FB_LoginWebBrowser_Navigating);
      e.Cancel = true;
      thirdLoginWebDialog.webGrid.Visibility = Visibility.Collapsed;
      thirdLoginWebDialog.resultGrid.Visibility = Visibility.Visible;
      thirdLoginWebDialog.ThirdLoginSuccess("login_Facebook", access_token: access_token);
    }

    private async void WeiBo_LoginWebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
    {
      ThirdLoginWebDialog thirdLoginWebDialog = this;
      if (!e.Uri.ToString().Contains("code="))
        return;
      string code = e.Uri.ToString().Substring(e.Uri.ToString().IndexOf("code=") + 5);
      if (!(code != ""))
        return;
      thirdLoginWebDialog.loginWebBrowser.Navigating -= new NavigatingCancelEventHandler(thirdLoginWebDialog.WeiBo_LoginWebBrowser_Navigating);
      e.Cancel = true;
      thirdLoginWebDialog.webGrid.Visibility = Visibility.Collapsed;
      thirdLoginWebDialog.resultGrid.Visibility = Visibility.Visible;
      thirdLoginWebDialog.ThirdLoginSuccess("login_Weibo", code);
    }

    private async void QQ_LoginWebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
    {
      ThirdLoginWebDialog thirdLoginWebDialog = this;
      if (!e.Uri.ToString().Contains("access_token"))
        return;
      string access_token = e.Uri.ToString().Substring(e.Uri.ToString().IndexOf("access_token") + 13, e.Uri.ToString().IndexOf("&", e.Uri.ToString().IndexOf("access_token")) - (e.Uri.ToString().IndexOf("access_token") + 13));
      thirdLoginWebDialog.loginWebBrowser.InvokeScript("eval", (object) "document.execCommand('Stop');");
      thirdLoginWebDialog.webGrid.Visibility = Visibility.Collapsed;
      thirdLoginWebDialog.resultGrid.Visibility = Visibility.Visible;
      string tencentOpenId = await Communicator.GetTencentOpenId(access_token);
      thirdLoginWebDialog.loginWebBrowser.Navigating -= new NavigatingCancelEventHandler(thirdLoginWebDialog.QQ_LoginWebBrowser_Navigating);
      e.Cancel = true;
      thirdLoginWebDialog.ThirdLoginSuccess("login_QQ", site: "qq.com", access_token: access_token, openId: tencentOpenId);
      access_token = (string) null;
    }

    private void WX_LoginWebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
    {
      if (!e.Uri.ToString().Contains("code="))
        return;
      string code = e.Uri.ToString().Substring(e.Uri.ToString().IndexOf("code=") + 5, e.Uri.ToString().IndexOf("&", e.Uri.ToString().IndexOf("code=")) - (e.Uri.ToString().IndexOf("code=") + 5));
      if (!(code != ""))
        return;
      this.loginWebBrowser.Navigating -= new NavigatingCancelEventHandler(this.WX_LoginWebBrowser_Navigating);
      e.Cancel = true;
      this.ThirdLoginSuccess("login_Wechat", code);
    }

    private async void ThirdLoginSuccess(
      string mode,
      string code = "",
      string site = "",
      string access_token = "",
      string openId = "",
      string accessTokenSecret = "")
    {
      ThirdLoginWebDialog thirdLoginWebDialog = this;
      UserModel user = new UserModel();
      switch (mode)
      {
        case "login_Google":
          user = await Communicator.ThirdSignon(mode, site: site, accessToken: access_token, uId: openId);
          break;
        case "login_Facebook":
          user = await Communicator.ThirdSignon(mode, accessToken: access_token);
          break;
        case "login_Twitter":
          user = await Communicator.ThirdSignon(mode, accessToken: access_token, accessTokenSecret: accessTokenSecret);
          break;
        case "login_Weibo":
          user = await Communicator.ThirdSignon(mode, code);
          break;
        case "login_QQ":
          user = await Communicator.ThirdSignon(mode, site: site, accessToken: access_token, uId: openId);
          break;
        case "login_Wechat":
          user = await Communicator.ThirdSignon(mode, code);
          break;
      }
      if (user == null)
        user = (UserModel) null;
      else if (user.userId == null)
        user = (UserModel) null;
      else if (!(user.userId != ""))
      {
        user = (UserModel) null;
      }
      else
      {
        await UserDao.UpdateOrInsertUserModelListDbAsync(user);
        await LocalSettings.ResetUserSettings(user, (string) null);
        await UserManager.PullUserInfo();
        await UserActCollectUtils.OnDeviceDataChanged();
        JumpHelper.InitJumpList();
        JumpHelper.InitChangeEvents();
        if (await Communicator.IsNewUser())
        {
          LocalSettings.Settings.NeedShowTutorial = true;
          LocalSettings.Settings.ExtraSettings.ShowProjectTimes = 3;
        }
        thirdLoginWebDialog.Owner.Hide();
        thirdLoginWebDialog.Hide();
        await App.Instance.ShowMainWindow();
        thirdLoginWebDialog.Close();
        thirdLoginWebDialog.Owner.Close();
        user = (UserModel) null;
      }
    }

    private void Restart()
    {
      string location = Assembly.GetExecutingAssembly().Location;
      this.Dispatcher.Invoke((Delegate) (() => Application.Current?.Shutdown()));
      int num = (int) MessageBox.Show("");
      Process.Start(location);
    }

    private void loginWebBrowser_LoadCompleted(object sender, NavigationEventArgs e)
    {
      this.loginProgressBar.Visibility = Visibility.Collapsed;
    }

    private static void _DeleteEveryCookie(Uri url)
    {
      string str1 = string.Empty;
      try
      {
        str1 = Application.GetCookie(url);
      }
      catch (Win32Exception ex)
      {
      }
      if (string.IsNullOrEmpty(str1))
        return;
      string str2 = str1;
      char[] chArray = new char[1]{ ';' };
      foreach (string str3 in str2.Split(chArray))
      {
        if (str3.IndexOf('=') > 0)
          ThirdLoginWebDialog._DeleteSingleCookie(str3.Substring(0, str3.IndexOf('=')).Trim(), url);
      }
    }

    private static void _DeleteSingleCookie(string name, Uri url)
    {
      try
      {
        DateTime dateTime = DateTime.UtcNow - TimeSpan.FromDays(1.0);
        string str = string.Format("{0}=; expires={1}; path=/; domain=.facebook.com", (object) name, (object) dateTime.ToString("R"));
        Application.SetCookie(url, str);
      }
      catch (Exception ex)
      {
      }
    }

    private void OnLoginFailClick(object sender, MouseButtonEventArgs e)
    {
      Utils.TryProcessStartUrl("https://help.dida365.com/forum/topic/8276/pc-%E7%AC%AC%E4%B8%89%E6%96%B9%E7%99%BB%E9%99%86%E5%A4%B1%E8%B4%A5%E8%A7%A3%E5%86%B3%E5%8A%9E%E6%B3%95");
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/thirdloginwebdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.webGrid = (Grid) target;
          break;
        case 2:
          this.loginWebBrowser = (WebBrowser) target;
          this.loginWebBrowser.LoadCompleted += new LoadCompletedEventHandler(this.loginWebBrowser_LoadCompleted);
          break;
        case 3:
          this.loginProgressBar = (ProgressBar) target;
          break;
        case 4:
          this.LoginErrorText = (TextBlock) target;
          this.LoginErrorText.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnLoginFailClick);
          break;
        case 5:
          this.PinGrid = (Grid) target;
          break;
        case 6:
          this.pinTextBox = (TextBox) target;
          this.pinTextBox.TextChanged += new TextChangedEventHandler(this.pinTextBox_TextChanged);
          break;
        case 7:
          this.PinLoginButton = (Button) target;
          this.PinLoginButton.Click += new RoutedEventHandler(this.PinLoginButton_Click);
          break;
        case 8:
          this.resultGrid = (StackPanel) target;
          break;
        case 9:
          this.resultProgressBar = (ProgressBar) target;
          break;
        case 10:
          this.logoImage = (Image) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
