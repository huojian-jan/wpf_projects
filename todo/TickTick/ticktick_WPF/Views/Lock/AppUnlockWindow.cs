// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Lock.AppUnlockWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Lock
{
  public class AppUnlockWindow : Window, IComponentConnector
  {
    private static AppUnlockWindow _unlockWindow;
    internal PasswordBox PasswordText;
    internal TextBlock PasswordInvalidText;
    internal Button SaveButton;
    private bool _contentLoaded;

    public event EventHandler OnUnlock;

    public AppUnlockWindow()
    {
      this.InitializeComponent();
      this.Activated += new EventHandler(this.OnActivated);
      this.Deactivated += new EventHandler(this.OnDeactivated);
    }

    private void OnDeactivated(object sender, EventArgs e) => Keyboard.Focus((IInputElement) null);

    private async void OnActivated(object sender, EventArgs e)
    {
      await Task.Delay(100);
      this.PasswordText.Focus();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e) => this.TryUnlockApp();

    private async void TryUnlockApp()
    {
      AppUnlockWindow sender = this;
      if (string.IsNullOrEmpty(sender.PasswordText.Password))
        return;
      string lockPassword = await AppLockCache.GetLockPassword();
      if (sender.PasswordText.Password == lockPassword)
      {
        UtilLog.Info("TryUnlockApp success");
        await App.Unlock();
        EventHandler onUnlock = sender.OnUnlock;
        if (onUnlock != null)
          onUnlock((object) sender, (EventArgs) null);
        sender.Close();
      }
      else
      {
        sender.PasswordInvalidText.Visibility = Visibility.Visible;
        sender.PasswordText.SelectAll();
      }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnPasswordKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          this.TryUnlockApp();
          break;
        case Key.Escape:
          this.Close();
          break;
      }
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      this.PasswordInvalidText.Visibility = Visibility.Collapsed;
      this.SaveButton.IsEnabled = this.PasswordText.Password.Length > 0;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private async void OnForgetPasswordClick(object sender, MouseButtonEventArgs e)
    {
      AppUnlockWindow appUnlockWindow = this;
      appUnlockWindow.Hide();
      if (!(LocalSettings.Settings.AccountType != "mail_account"))
        new ResetPasswordWindow(ResetMode.Unlock).ShowDialog();
      else
        new ForgetPasswordWindow(true).ShowDialog();
      appUnlockWindow.Close();
    }

    public static async void TryUnlockApp(Action handler)
    {
      if (AppUnlockWindow._unlockWindow == null)
      {
        AppUnlockWindow._unlockWindow = new AppUnlockWindow();
        AppUnlockWindow._unlockWindow.Closed += new EventHandler(AppUnlockWindow.OnUnlockClosed);
        AppUnlockWindow._unlockWindow.OnUnlock += (EventHandler) ((sender, arg) =>
        {
          Action action = handler;
          if (action == null)
            return;
          action();
        });
      }
      try
      {
        await Task.Delay(200);
        AppUnlockWindow._unlockWindow.Show();
        AppUnlockWindow._unlockWindow.Activate();
        await Task.Delay(1000);
        AppUnlockWindow._unlockWindow?.PasswordText?.Focus();
      }
      catch (Exception ex)
      {
      }
    }

    private static void OnUnlockClosed(object sender, EventArgs e)
    {
      AppUnlockWindow._unlockWindow = (AppUnlockWindow) null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/lock/appunlockwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.PasswordText = (PasswordBox) target;
          this.PasswordText.KeyDown += new KeyEventHandler(this.OnPasswordKeyDown);
          this.PasswordText.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case 2:
          this.PasswordInvalidText = (TextBlock) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnForgetPasswordClick);
          break;
        case 4:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
