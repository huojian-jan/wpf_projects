// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Lock.ForgetPasswordWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Lock
{
  public class ForgetPasswordWindow : Window, IComponentConnector
  {
    private readonly bool _thirdPartyUser;
    internal TextBlock ResetHint;
    private bool _contentLoaded;

    public ForgetPasswordWindow(bool thirdPartyUser)
    {
      this.InitializeComponent();
      this.ResetHint.Text = thirdPartyUser ? string.Format(Utils.GetString("ForgetPasswordHint"), (object) ForgetPasswordWindow.GetLoginType(LocalSettings.Settings.AccountType)) : Utils.GetString("ForgetAndResetPasswordHint");
      this._thirdPartyUser = thirdPartyUser;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      ForgetPasswordWindow forgetPasswordWindow = this;
      forgetPasswordWindow.Hide();
      if (!forgetPasswordWindow._thirdPartyUser)
        Utils.ResetPassword(LocalSettings.Settings.LoginUserName);
      LocalSettings.Settings.NeedResetPassword = true;
      LocalSettings.Settings.NeedResetUserId = LocalSettings.Settings.LoginUserId;
      await AppLockDao.ClearPassword();
      await App.Window.ForceLogout();
      forgetPasswordWindow.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private static string GetLoginType(string key)
    {
      switch (key)
      {
        case "facebook":
          return "Facebook";
        case "google":
          return "Google";
        case "twitter":
          return "Twitter";
        case "wechat":
          return Utils.GetString("Wechat");
        case "weibo":
          return Utils.GetString("Weibo");
        case "tecent":
          return "QQ";
        default:
          return string.Empty;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/lock/forgetpasswordwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.ResetHint = (TextBlock) target;
          break;
        case 2:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
