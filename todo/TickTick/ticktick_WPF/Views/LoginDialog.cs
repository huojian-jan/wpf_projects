// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.LoginDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Properties;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.Misc;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views
{
  public class LoginDialog : Window, IComponentConnector
  {
    private bool isLoginSign;
    internal ComboBox ApiCombo;
    internal CarouselControl Carousel;
    internal Grid ReturnGrid;
    internal Grid ProxySetting;
    internal Image DidaLogoImage;
    internal Image TickLogoImage;
    internal Grid ThirdLoginGrid;
    internal Grid ThirdLoginEnGrid;
    internal Grid FacebookGrid;
    internal Grid AppleEnGrid;
    internal Grid ThirdLoginZnGrid;
    internal Grid WeiboGrid;
    internal Grid QQGrid;
    internal Grid AppleCNGrid;
    internal StackPanel homeGrid;
    internal Grid signGrid;
    internal TextBox nameTextBlock;
    internal TextBox emailTextBlock;
    internal TextBlock AccountRemainderTimesText;
    internal PasswordBox passwordPasswordBox;
    internal TextBlock PasswordRemainderTimesText;
    internal Grid signInButton;
    internal LoadingIndicator SignInIndicator;
    internal Grid signUpButton;
    internal LoadingIndicator SignUpIndicator;
    internal Grid ForgetAndSignupGrid;
    private bool _contentLoaded;

    public LoginDialog()
    {
      this.InitializeComponent();
      TickDbHelper.CreateDbAsync();
      this.Title = Utils.GetAppName();
      this.ThirdLoginZnGrid.Visibility = Visibility.Visible;
      this.ThirdLoginEnGrid.Visibility = Visibility.Collapsed;
      this.DidaLogoImage.Visibility = Visibility.Visible;
      this.TickLogoImage.Visibility = Visibility.Collapsed;
      this.emailTextBlock.Tag = (object) Utils.GetString("PhoneNumberAndEmail");
      this.emailTextBlock.Text = this.CheckIsTrueEMail(LocalSettings.Settings.LoginUserName) ? LocalSettings.Settings.LoginUserName : "";
      this.Carousel.InitData(CarouselItemViewModel.GetLoginCarouselModels());
      if (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\didaapitest.txt"))
        return;
      this.ApiCombo.Visibility = Visibility.Visible;
      string[] strArray = File.ReadAllLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\didaapitest.txt");
      if (strArray == null || strArray.Length == 0)
        return;
      foreach (string str in strArray)
      {
        ItemCollection items = this.ApiCombo.Items;
        ComboBoxItem newItem = new ComboBoxItem();
        newItem.Content = (object) str;
        items.Add((object) newItem);
      }
    }

    private bool CheckIsTrueEMail(string email)
    {
      string hostName = BaseUrl.GetHostName();
      return !email.Contains(hostName);
    }

    private void Login_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return || !(this.emailTextBlock.Text != "") || !(this.passwordPasswordBox.Password != ""))
        return;
      if (this.nameTextBlock.Visibility == Visibility.Collapsed)
        this.Login();
      else if (Utils.IsValidEmail(this.emailTextBlock.Text))
      {
        if (this.passwordPasswordBox.Password.Length >= 6 && this.passwordPasswordBox.Password.Length <= 20)
        {
          this.signUp();
        }
        else
        {
          CustomerDialog customerDialog = new CustomerDialog(Application.Current?.FindResource((object) "SignUpFailed").ToString(), Application.Current?.FindResource((object) "InvalidatePasswordLength").ToString(), MessageBoxButton.OK);
          customerDialog.Owner = Window.GetWindow((DependencyObject) this);
          customerDialog.ShowDialog();
        }
      }
      else
      {
        CustomerDialog customerDialog = new CustomerDialog(Application.Current?.FindResource((object) "SignUpFailed").ToString(), Application.Current?.FindResource((object) "InvalidateEmail").ToString(), MessageBoxButton.OK);
        customerDialog.Owner = Window.GetWindow((DependencyObject) this);
        customerDialog.ShowDialog();
      }
    }

    private async void signUp()
    {
      LoginDialog loginDialog = this;
      if (loginDialog.isLoginSign)
        return;
      loginDialog.isLoginSign = true;
      loginDialog.SignUpIndicator.Visibility = Visibility.Visible;
      (UserModel userModel, string str) = await LoginManager.SignUp(loginDialog.nameTextBlock.Text, loginDialog.emailTextBlock.Text, loginDialog.passwordPasswordBox.Password);
      if (userModel != null)
      {
        loginDialog.Hide();
        LocalSettings.Settings.NeedShowTutorial = true;
        LocalSettings.Settings.ExtraSettings.ShowProjectTimes = 3;
        await App.Instance.ShowMainWindow();
        loginDialog.Close();
      }
      else
      {
        if (string.IsNullOrEmpty(str))
        {
          switch (str)
          {
            case "invalidate_email":
              loginDialog.AccountRemainderTimesText.Text = Utils.GetString("InvalidateEmail");
              break;
            case "invalidate_password_length":
              loginDialog.PasswordRemainderTimesText.Text = Utils.GetString("InvalidatePasswordLength");
              break;
            case "username_exist":
              loginDialog.AccountRemainderTimesText.Text = Utils.GetString("UsernameExist");
              break;
          }
        }
        loginDialog.SignUpIndicator.Visibility = Visibility.Collapsed;
        loginDialog.isLoginSign = false;
      }
    }

    private async void Login()
    {
      LoginDialog loginDialog = this;
      if (loginDialog.isLoginSign)
        return;
      loginDialog.isLoginSign = true;
      loginDialog.SignInIndicator.Visibility = Visibility.Visible;
      (UserModel userModel, ApiException apiException) = await LoginManager.Login(loginDialog.emailTextBlock.Text, loginDialog.passwordPasswordBox.Password);
      if (userModel != null)
      {
        loginDialog.Hide();
        await App.Instance.ShowMainWindow();
        loginDialog.Close();
      }
      else
      {
        if (apiException != null)
        {
          switch (apiException.ErrorCode)
          {
            case "username_password_not_match":
              int result;
              if (int.TryParse(apiException.Data[(object) "remainderTimes"]?.ToString(), out result))
              {
                if (result == 0)
                {
                  loginDialog.PasswordRemainderTimesText.Text = string.Empty;
                  CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("AccountAtRisk"), Utils.GetString("AccountAtRiskMessage"), MessageBoxButton.OK);
                  customerDialog.Owner = (Window) loginDialog;
                  customerDialog.ShowDialog();
                  break;
                }
                if (result <= 3)
                {
                  loginDialog.PasswordRemainderTimesText.Text = string.Format(Utils.GetString(result > 1 ? "PasswordRemainderTimes" : "PasswordRemainderTime"), (object) result);
                  break;
                }
              }
              loginDialog.PasswordRemainderTimesText.Text = Utils.GetString("UsernamePasswordNotMatch");
              break;
            case "incorrect_password_too_many_times":
              loginDialog.PasswordRemainderTimesText.Text = string.Empty;
              CustomerDialog customerDialog1 = new CustomerDialog(Utils.GetString("AccountAtRisk"), Utils.GetString("AccountAtRiskMessage"), Utils.GetString("GotIt"), "");
              customerDialog1.Owner = (Window) loginDialog;
              customerDialog1.ShowDialog();
              break;
            case "username_not_exist":
              loginDialog.AccountRemainderTimesText.Text = Utils.GetString("UsernameNotExist");
              break;
            default:
              loginDialog.ShowLoginFailedDialog();
              int num = (int) MessageBox.Show(apiException.ErrorCode);
              break;
          }
        }
        else
          loginDialog.ShowLoginFailedDialog();
        loginDialog.SignInIndicator.Visibility = Visibility.Collapsed;
        loginDialog.isLoginSign = false;
      }
    }

    private void ShowLoginFailedDialog()
    {
      CustomerDialog dialog = new CustomerDialog(Utils.GetString("LoginFailed"), Utils.GetString("CheckNetworkProxyPre"), Utils.GetString("CheckNetworkProxyCenter"), string.Empty, Utils.GetString("OK"));
      dialog.TextClick += (EventHandler) ((o, s) =>
      {
        dialog.Close();
        try
        {
          new ProxyWindow() { Owner = ((Window) this) }.Show();
        }
        catch (Exception ex)
        {
        }
      });
      dialog.ShowDialog();
    }

    private async void SignUpClick(object sender, MouseButtonEventArgs e)
    {
      this.emailTextBlock.Tag = (object) Utils.GetString("Email1");
      this.nameTextBlock.Visibility = Visibility.Visible;
      this.signUpButton.Visibility = Visibility.Visible;
      this.signInButton.Visibility = Visibility.Collapsed;
      this.ForgetAndSignupGrid.Visibility = Visibility.Collapsed;
      this.emailTextBlock.Text = "";
      this.passwordPasswordBox.Password = "";
      Keyboard.Focus((IInputElement) this.nameTextBlock);
    }

    private void SignWithAccountClick(object sender, RoutedEventArgs e)
    {
      this.emailTextBlock.Tag = (object) Utils.GetString(Utils.IsDida() ? "PhoneNumberAndEmail" : "Email1");
      this.nameTextBlock.Visibility = Visibility.Collapsed;
      this.signUpButton.Visibility = Visibility.Collapsed;
      this.signInButton.Visibility = Visibility.Visible;
      this.ForgetAndSignupGrid.Visibility = Visibility.Visible;
      this.ShowOrHideElement((UIElement) this.homeGrid, false);
      this.ShowOrHideElement((UIElement) this.signGrid, true);
      this.ReturnGrid.Visibility = Visibility.Visible;
      this.emailTextBlock.Text = this.CheckIsTrueEMail(LocalSettings.Settings.LoginUserName) ? LocalSettings.Settings.LoginUserName : "";
      if (this.emailTextBlock.Text != "")
        Keyboard.Focus((IInputElement) this.passwordPasswordBox);
      else
        Keyboard.Focus((IInputElement) this.emailTextBlock);
    }

    private async void ReturnClick(object sender, MouseButtonEventArgs e)
    {
      this.ShowOrHideElement((UIElement) this.homeGrid, true);
      this.ShowOrHideElement((UIElement) this.signGrid, false);
      this.ReturnGrid.Visibility = Visibility.Collapsed;
      this.PasswordRemainderTimesText.Text = string.Empty;
      this.passwordPasswordBox.Password = string.Empty;
    }

    private void forgetPasswordButton_Click(object sender, RoutedEventArgs e)
    {
      this.ResetPassword();
    }

    private void ResetPassword() => Utils.ResetPassword(this.emailTextBlock.Text);

    private void signInButton_Click(object sender, RoutedEventArgs e) => this.Login();

    private void signUpButton_Click(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(this.emailTextBlock.Text) || string.IsNullOrEmpty(this.passwordPasswordBox.Password))
        return;
      this.signUp();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (LocalSettings.Settings.LoginUserId == "")
        Application.Current?.Shutdown();
      base.OnClosing(e);
    }

    private void ThirdLoginMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      ThirdLoginWebDialog thirdLoginWebDialog = new ThirdLoginWebDialog((sender is FrameworkElement frameworkElement ? frameworkElement.Tag : (object) null) as string);
      thirdLoginWebDialog.Owner = Window.GetWindow((DependencyObject) this);
      thirdLoginWebDialog.ShowDialog();
    }

    private void WeChatSignClick(object sender, RoutedEventArgs e)
    {
      ThirdLoginWebDialog thirdLoginWebDialog = new ThirdLoginWebDialog("login_Wechat");
      thirdLoginWebDialog.Owner = Window.GetWindow((DependencyObject) this);
      thirdLoginWebDialog.ShowDialog();
    }

    private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
    {
      if (!(sender is TextBox textBox))
        return;
      textBox.SelectAll();
    }

    private void OnProxySettingClick(object sender, MouseButtonEventArgs e)
    {
      ProxyWindow proxyWindow = new ProxyWindow();
      proxyWindow.Owner = (Window) this;
      proxyWindow.Show();
    }

    private void DragMove(object sender, MouseButtonEventArgs e) => this.DragMove();

    private void CloseWindow(object sender, MouseButtonEventArgs e) => this.Close();

    private void GoogleLoginClick(object sender, RoutedEventArgs e)
    {
      GoogleLogin login = new GoogleLogin(this);
      try
      {
        this.Closing += (CancelEventHandler) ((o, args) => login.ClearLogin());
        login.PerformLogin();
      }
      catch (Exception ex)
      {
        Utils.Toast(Utils.GetString("LoginFailed"));
      }
    }

    public async Task PerformGoogleByAccessToken(string code, string accessToken)
    {
      LoginDialog loginDialog = this;
      UserModel user = await Communicator.ThirdSignon("login_Google", site: "google.com", accessToken: accessToken, uId: code);
      if (user == null)
        return;
      if (string.IsNullOrEmpty(user.code))
      {
        await loginDialog.SaveThirdLogin(user);
      }
      else
      {
        LoginBindThirdDialog loginBindThirdDialog = new LoginBindThirdDialog(user);
        loginBindThirdDialog.Owner = (Window) loginDialog;
        loginBindThirdDialog.ShowDialog();
        await loginDialog.SaveThirdLogin(user);
      }
    }

    public async Task SaveThirdLogin(UserModel user)
    {
      LoginDialog loginDialog = this;
      if (string.IsNullOrEmpty(user.userId))
        return;
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
      loginDialog.Hide();
      await App.Instance.ShowMainWindow();
      loginDialog.Close();
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!string.IsNullOrEmpty(this.PasswordRemainderTimesText.Text))
        this.PasswordRemainderTimesText.Text = string.Empty;
      if (string.IsNullOrEmpty(this.AccountRemainderTimesText.Text))
        return;
      this.AccountRemainderTimesText.Text = string.Empty;
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      if (string.IsNullOrEmpty(this.PasswordRemainderTimesText.Text))
        return;
      this.PasswordRemainderTimesText.Text = string.Empty;
    }

    private void ShowMoreLoginType(object sender, MouseButtonEventArgs e)
    {
      this.ShowOrHideElement((UIElement) this.ThirdLoginGrid, true);
    }

    private void HideMoreSignGrid(object sender, MouseButtonEventArgs e)
    {
      this.ShowOrHideElement((UIElement) this.ThirdLoginGrid, false);
    }

    private void ShowOrHideElement(UIElement element, bool show)
    {
      element.IsHitTestVisible = show;
      element.Visibility = Visibility.Visible;
      Storyboard resource = (Storyboard) this.FindResource(show ? (object) "ShowStory" : (object) "HideStory");
      resource.Children[0].SetValue(Storyboard.TargetProperty, (object) element);
      resource.Begin();
      Task.Delay(120);
      if (show)
        return;
      element.Visibility = Visibility.Collapsed;
    }

    private void SignInWithApple(object sender, MouseButtonEventArgs e)
    {
      ticktick_WPF.Views.SignInWithApple apple = ticktick_WPF.Views.SignInWithApple.GetInstance(this);
      this.Closing += (CancelEventHandler) ((o, args) => apple.ClearLogin());
      apple.Start();
    }

    private void ClearEmailClick(object sender, MouseButtonEventArgs e)
    {
      this.emailTextBlock.Text = string.Empty;
      this.emailTextBlock.Focus();
    }

    private void OnApiChanged(object sender, SelectionChangedEventArgs e)
    {
      switch (this.ApiCombo.SelectedIndex)
      {
        case 0:
          Settings.Default.ApiTest = "";
          break;
        case 1:
          Settings.Default.ApiTest = "https://api-dev.365dida.com";
          break;
        case 2:
          Settings.Default.ApiTest = "https://api-test.365dida.com";
          break;
        case 3:
          Settings.Default.ApiTest = "https://api-future-test.365dida.com";
          break;
        case 4:
          Settings.Default.ApiTest = "https://build.dida365.com";
          break;
        default:
          Settings.Default.ApiTest = (string) ((ContentControl) this.ApiCombo.SelectedItem).Content;
          break;
      }
      Settings.Default.Save();
    }

    public async Task TryLoginApple(SignInWithAppleParma e)
    {
      LoginDialog loginDialog = this;
      UserModel user = await Communicator.LoginApple(e.accessToken, e.uId, e.email, e.name);
      try
      {
        loginDialog.Show();
        loginDialog.Activate();
        loginDialog.WindowState = WindowState.Normal;
      }
      catch (Exception ex)
      {
      }
      await Task.Delay(500);
      if (user?.userId != null && user.userId != "")
      {
        if (!string.IsNullOrEmpty(user.code))
        {
          LoginBindThirdDialog loginBindThirdDialog = new LoginBindThirdDialog(user);
          loginBindThirdDialog.Owner = (Window) loginDialog;
          loginBindThirdDialog.ShowDialog();
        }
        await UserDao.UpdateOrInsertUserModelListDbAsync(user);
        loginDialog.SaveThirdLogin(user);
        user = (UserModel) null;
      }
      else if (user == null)
        user = (UserModel) null;
      else if (string.IsNullOrEmpty(user.username))
      {
        user = (UserModel) null;
      }
      else
      {
        int num = (int) MessageBox.Show(Utils.GetString("LoginFailed"));
        user = (UserModel) null;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/logindialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.ApiCombo = (ComboBox) target;
          this.ApiCombo.SelectionChanged += new SelectionChangedEventHandler(this.OnApiChanged);
          break;
        case 2:
          this.Carousel = (CarouselControl) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.DragMove);
          break;
        case 4:
          this.ReturnGrid = (Grid) target;
          this.ReturnGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.ReturnClick);
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.CloseWindow);
          break;
        case 6:
          this.ProxySetting = (Grid) target;
          this.ProxySetting.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnProxySettingClick);
          break;
        case 7:
          this.DidaLogoImage = (Image) target;
          break;
        case 8:
          this.TickLogoImage = (Image) target;
          break;
        case 9:
          this.ThirdLoginGrid = (Grid) target;
          this.ThirdLoginGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.HideMoreSignGrid);
          break;
        case 10:
          this.ThirdLoginEnGrid = (Grid) target;
          break;
        case 11:
          this.FacebookGrid = (Grid) target;
          this.FacebookGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.ThirdLoginMouseLeftButtonUp);
          break;
        case 12:
          this.AppleEnGrid = (Grid) target;
          this.AppleEnGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SignInWithApple);
          break;
        case 13:
          this.ThirdLoginZnGrid = (Grid) target;
          break;
        case 14:
          this.WeiboGrid = (Grid) target;
          this.WeiboGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.ThirdLoginMouseLeftButtonUp);
          break;
        case 15:
          this.QQGrid = (Grid) target;
          this.QQGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.ThirdLoginMouseLeftButtonUp);
          break;
        case 16:
          this.AppleCNGrid = (Grid) target;
          this.AppleCNGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SignInWithApple);
          break;
        case 17:
          this.homeGrid = (StackPanel) target;
          break;
        case 18:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.WeChatSignClick);
          break;
        case 19:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.SignWithAccountClick);
          break;
        case 20:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.SignWithAccountClick);
          break;
        case 21:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.GoogleLoginClick);
          break;
        case 22:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowMoreLoginType);
          break;
        case 23:
          this.signGrid = (Grid) target;
          break;
        case 24:
          this.nameTextBlock = (TextBox) target;
          this.nameTextBlock.GotFocus += new RoutedEventHandler(this.TextBlock_GotFocus);
          break;
        case 25:
          this.emailTextBlock = (TextBox) target;
          this.emailTextBlock.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          this.emailTextBlock.GotFocus += new RoutedEventHandler(this.TextBlock_GotFocus);
          this.emailTextBlock.KeyDown += new KeyEventHandler(this.Login_KeyDown);
          break;
        case 26:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ClearEmailClick);
          break;
        case 27:
          this.AccountRemainderTimesText = (TextBlock) target;
          break;
        case 28:
          this.passwordPasswordBox = (PasswordBox) target;
          this.passwordPasswordBox.KeyDown += new KeyEventHandler(this.Login_KeyDown);
          this.passwordPasswordBox.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case 29:
          this.PasswordRemainderTimesText = (TextBlock) target;
          break;
        case 30:
          this.signInButton = (Grid) target;
          break;
        case 31:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.signInButton_Click);
          break;
        case 32:
          this.SignInIndicator = (LoadingIndicator) target;
          break;
        case 33:
          this.signUpButton = (Grid) target;
          break;
        case 34:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.signUpButton_Click);
          break;
        case 35:
          this.SignUpIndicator = (LoadingIndicator) target;
          break;
        case 36:
          this.ForgetAndSignupGrid = (Grid) target;
          break;
        case 37:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.forgetPasswordButton_Click);
          break;
        case 38:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SignUpClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
