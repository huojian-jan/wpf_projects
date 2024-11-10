// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Lock.SetPasswordWindow
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
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Lock
{
  public class SetPasswordWindow : Window, IComponentConnector
  {
    private readonly SetPasswordMode _mode;
    private Func<Task> _saveHandler;
    internal TextBlock LockHint;
    internal Grid OldPasswordGrid;
    internal PasswordBox OldPasswordText;
    internal TextBlock NewPasswordTitle;
    internal PasswordBox NewPasswordText;
    internal TextBlock PasswordInvalidText;
    internal TextBlock ForgetPasswordText;
    internal Button SaveButton;
    private bool _contentLoaded;

    public SetPasswordWindow(SetPasswordMode mode)
    {
      this.InitializeComponent();
      this._mode = mode;
      this.InitData();
    }

    private void InitData()
    {
      switch (this._mode)
      {
        case SetPasswordMode.Init:
          this.Title = Utils.GetString("SetPassword");
          this.LockHint.Visibility = Visibility.Collapsed;
          this.HideOldPassword();
          this._saveHandler = new Func<Task>(this.SavePassword);
          this.NewPasswordText.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case SetPasswordMode.FirstTry:
          this.Title = Utils.GetString("SetPassword");
          this.LockHint.Text = Utils.GetString("TrySetPasswordHint");
          this.HideOldPassword();
          this._saveHandler = new Func<Task>(this.SavePassword);
          this.NewPasswordText.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case SetPasswordMode.Clear:
          this.Title = Utils.GetString("DisablePassword");
          this.ForgetPasswordText.Visibility = Visibility.Visible;
          this.LockHint.Text = Utils.GetString("ClearPasswordHint");
          this.PasswordInvalidText.Text = Utils.GetString("IncorrectUnlockPassword");
          this.HideOldPassword();
          this.NewPasswordTitle.Visibility = Visibility.Collapsed;
          this._saveHandler = new Func<Task>(this.ClearPassword);
          this.NewPasswordText.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case SetPasswordMode.Modify:
          this.Title = Utils.GetString("ChangePassword");
          this.ForgetPasswordText.Visibility = Visibility.Visible;
          this.LockHint.Visibility = Visibility.Collapsed;
          this._saveHandler = new Func<Task>(this.ChangePassword);
          this.OldPasswordText.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case SetPasswordMode.Reset:
          this.Title = Utils.GetString("SetPassword");
          this.LockHint.Text = Utils.GetString("ClearResetPasswordHint");
          this.HideOldPassword();
          this._saveHandler = new Func<Task>(this.SavePassword);
          this.NewPasswordText.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
      }
    }

    private void HideOldPassword()
    {
      this.OldPasswordGrid.Visibility = Visibility.Collapsed;
      this.NewPasswordText.Focus();
    }

    private async void OnPasswordKeyDown(object sender, KeyEventArgs e)
    {
      SetPasswordWindow setPasswordWindow = this;
      switch (e.Key)
      {
        case Key.Return:
          await setPasswordWindow.SaveAction();
          break;
        case Key.Escape:
          setPasswordWindow.Close();
          break;
      }
    }

    private async Task SaveAction()
    {
      if (this._saveHandler == null)
        return;
      Func<Task> saveHandler = this._saveHandler;
      await (saveHandler != null ? saveHandler() : (Task) null);
    }

    private async Task ClearPassword()
    {
      SetPasswordWindow setPasswordWindow = this;
      if (await AppLockCache.GetLockPassword() != setPasswordWindow.NewPasswordText.Password)
      {
        setPasswordWindow.PasswordInvalidText.Visibility = Visibility.Visible;
        setPasswordWindow.OldPasswordText.Focus();
        setPasswordWindow.OldPasswordText.SelectAll();
      }
      else
      {
        await AppLockDao.ClearPassword();
        setPasswordWindow.Close();
      }
    }

    private async Task ChangePassword()
    {
      SetPasswordWindow setPasswordWindow = this;
      if (await AppLockCache.GetLockPassword() != setPasswordWindow.OldPasswordText.Password)
      {
        setPasswordWindow.PasswordInvalidText.Visibility = Visibility.Visible;
        setPasswordWindow.OldPasswordText.Focus();
        setPasswordWindow.OldPasswordText.SelectAll();
      }
      else if (string.IsNullOrEmpty(setPasswordWindow.NewPasswordText.Password))
      {
        await AppLockDao.ClearPassword();
        setPasswordWindow.Close();
      }
      else
        await setPasswordWindow.SavePassword();
    }

    private async Task SavePassword()
    {
      SetPasswordWindow setPasswordWindow = this;
      await AppLockDao.SaveLockPassword(setPasswordWindow.NewPasswordText.Password);
      setPasswordWindow.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      this.PasswordInvalidText.Visibility = Visibility.Collapsed;
      this.SaveButton.IsEnabled = !string.IsNullOrEmpty(sender is PasswordBox passwordBox ? passwordBox.Password : (string) null);
    }

    private void OnForgetPasswordClick(object sender, MouseButtonEventArgs e)
    {
      this.Hide();
      if (!(LocalSettings.Settings.AccountType != "mail_account"))
        new ResetPasswordWindow(this._mode == SetPasswordMode.Clear ? ResetMode.Clear : ResetMode.Modify).ShowDialog();
      else
        new ForgetPasswordWindow(true).ShowDialog();
      this.Close();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e) => await this.SaveAction();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/lock/setpasswordwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.LockHint = (TextBlock) target;
          break;
        case 2:
          this.OldPasswordGrid = (Grid) target;
          break;
        case 3:
          this.OldPasswordText = (PasswordBox) target;
          break;
        case 4:
          this.NewPasswordTitle = (TextBlock) target;
          break;
        case 5:
          this.NewPasswordText = (PasswordBox) target;
          this.NewPasswordText.KeyDown += new KeyEventHandler(this.OnPasswordKeyDown);
          this.NewPasswordText.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case 6:
          this.PasswordInvalidText = (TextBlock) target;
          break;
        case 7:
          this.ForgetPasswordText = (TextBlock) target;
          this.ForgetPasswordText.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnForgetPasswordClick);
          break;
        case 8:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 9:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
