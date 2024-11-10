// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Lock.ResetPasswordWindow
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
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Lock
{
  public class ResetPasswordWindow : Window, IComponentConnector
  {
    private ResetMode _resetMode;
    internal TextBlock ResetHint;
    internal PasswordBox PasswordText;
    internal TextBlock PasswordInvalidText;
    internal Button SaveButton;
    private bool _contentLoaded;

    public ResetPasswordWindow(ResetMode mode)
    {
      this.InitializeComponent();
      this.InitData(mode);
    }

    private void InitData(ResetMode mode)
    {
      this._resetMode = mode;
      this.PasswordText.Focus();
      this.SetSaveMode(ResetPasswordWindow.SaveMode.Disabled);
      switch (mode)
      {
        case ResetMode.Unlock:
          this.Title = Utils.GetString("UnlockTickTick");
          this.ResetHint.Text = string.Format(Utils.GetString("ResetPasswordHint"), (object) LocalSettings.Settings.LoginUserName);
          break;
        case ResetMode.Modify:
          this.Title = Utils.GetString("ChangePassword");
          this.ResetHint.Text = string.Format(Utils.GetString("ModifyResetPassword"), (object) LocalSettings.Settings.LoginUserName);
          break;
        case ResetMode.Clear:
          this.Title = Utils.GetString("DisablePassword");
          this.ResetHint.Text = string.Format(Utils.GetString("ClearResetPassword"), (object) LocalSettings.Settings.LoginUserName);
          break;
      }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      this.PasswordInvalidText.Visibility = Visibility.Collapsed;
      this.SetSaveMode(string.IsNullOrEmpty(this.PasswordText.Password) ? ResetPasswordWindow.SaveMode.Disabled : ResetPasswordWindow.SaveMode.Normal);
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      ResetPasswordWindow resetPasswordWindow = this;
      if (!Utils.IsNetworkAvailable())
      {
        Utils.Toast(Utils.GetString("NoNetwork"));
      }
      else
      {
        resetPasswordWindow.SetSaveMode(ResetPasswordWindow.SaveMode.Verifing);
        string password = resetPasswordWindow.PasswordText.Password;
        if (string.IsNullOrEmpty(password))
          return;
        string username = LocalSettings.Settings.LoginUserName;
        try
        {
          UserModel userModel = await Communicator.SignOn(username, password);
          if (userModel == null || !(userModel.username == username))
            throw new Exception();
          await AppLockDao.ClearPassword();
          resetPasswordWindow.Close();
          if (resetPasswordWindow._resetMode != ResetMode.Clear)
          {
            new SetPasswordWindow(SetPasswordMode.Reset).ShowDialog();
            await App.Unlock();
            App.ShowMainWindow(string.Empty, string.Empty, true);
          }
        }
        catch (Exception ex)
        {
          resetPasswordWindow.SetSaveMode(ResetPasswordWindow.SaveMode.Normal);
          resetPasswordWindow.PasswordInvalidText.Visibility = Visibility.Visible;
          resetPasswordWindow.PasswordText.Focus();
        }
        username = (string) null;
      }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnForgetPasswordClick(object sender, MouseButtonEventArgs e)
    {
      this.Close();
      new ForgetPasswordWindow(false).ShowDialog();
    }

    private void SetSaveMode(ResetPasswordWindow.SaveMode mode)
    {
      this.SaveButton.IsEnabled = mode == ResetPasswordWindow.SaveMode.Normal;
      if (mode != ResetPasswordWindow.SaveMode.Disabled)
      {
        if (mode == ResetPasswordWindow.SaveMode.Normal)
        {
          this.SaveButton.Opacity = 1.0;
          this.SaveButton.Content = (object) Utils.GetString("Confirm");
          this.PasswordText.IsEnabled = true;
        }
        else
        {
          this.SaveButton.Opacity = 0.36000001430511475;
          this.SaveButton.Content = (object) Utils.GetString("Verifying");
          this.PasswordText.IsEnabled = false;
        }
      }
      else
      {
        this.SaveButton.Opacity = 0.36000001430511475;
        this.SaveButton.Content = (object) Utils.GetString("Confirm");
        this.PasswordText.IsEnabled = true;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/lock/resetpasswordwindow.xaml", UriKind.Relative));
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
          this.PasswordText = (PasswordBox) target;
          this.PasswordText.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case 3:
          this.PasswordInvalidText = (TextBlock) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnForgetPasswordClick);
          break;
        case 5:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 6:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    private enum SaveMode
    {
      Disabled,
      Verifing,
      Normal,
    }
  }
}
