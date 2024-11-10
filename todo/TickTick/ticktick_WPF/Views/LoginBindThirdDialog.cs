// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.LoginBindThirdDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class LoginBindThirdDialog : Window, IOkCancelWindow, IComponentConnector
  {
    private UserModel _user;
    internal Grid ContinueGrid;
    internal Grid InputGrid;
    private bool _contentLoaded;

    public LoginBindThirdDialog(UserModel user)
    {
      this.InitializeComponent();
      this.DataContext = (object) new LoginBindThirdViewModel()
      {
        Email = user.username,
        NotUploading = true
      };
      this._user = user;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      if (this.GetTemplateChild("TitleGrid") is Grid templateChild)
        templateChild.Visibility = Visibility.Collapsed;
      base.OnApplyTemplate();
    }

    public void Ok()
    {
      if (this.InputGrid.IsVisible)
      {
        this.Save();
      }
      else
      {
        if (!this.ContinueGrid.IsVisible)
          return;
        this.InputGrid.Visibility = Visibility.Visible;
        this.ContinueGrid.Visibility = Visibility.Collapsed;
      }
    }

    public async void Save()
    {
      LoginBindThirdDialog loginBindThirdDialog = this;
      if (!(loginBindThirdDialog.DataContext is LoginBindThirdViewModel model))
      {
        model = (LoginBindThirdViewModel) null;
      }
      else
      {
        model.NetworkBroken = false;
        model.PasswordInvaild = false;
        model.PasswordNotSame = false;
        model.NotUploading = false;
        if (string.IsNullOrEmpty(model.Password) || model.Password.Length < 6)
        {
          model.PasswordInvaild = true;
          model.NotUploading = true;
          model = (LoginBindThirdViewModel) null;
        }
        else if (model.PasswordComfirm != model.Password)
        {
          model.PasswordNotSame = true;
          model.NotUploading = true;
          model = (LoginBindThirdViewModel) null;
        }
        else
        {
          string json = await Communicator.ThirdChangePassword(loginBindThirdDialog._user.code, model.Password, model.PasswordComfirm, loginBindThirdDialog._user.token);
          if (!string.IsNullOrEmpty(json))
          {
            JObject jobject = JObject.Parse(json);
            if (jobject != null && jobject["token"] != null)
            {
              loginBindThirdDialog._user.token = jobject["token"].ToString();
              loginBindThirdDialog.Close();
              model = (LoginBindThirdViewModel) null;
              return;
            }
          }
          model.NetworkBroken = true;
          model.NotUploading = true;
          model = (LoginBindThirdViewModel) null;
        }
      }
    }

    public void OnCancel() => this.Close();

    private void OnNewPasswordOkClick(object sender, RoutedEventArgs e) => this.Save();

    private void OnNewPasswordCancelClick(object sender, RoutedEventArgs e) => this.OnCancel();

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext is LoginBindThirdViewModel dataContext) || !(sender is PasswordBox passwordBox))
        return;
      dataContext.Password = passwordBox.Password;
      dataContext.PasswordInvaild = false;
    }

    private void OnPasswordComfirmChanged(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext is LoginBindThirdViewModel dataContext) || !(sender is PasswordBox passwordBox))
        return;
      dataContext.PasswordComfirm = passwordBox.Password;
      dataContext.PasswordNotSame = false;
    }

    private void OnContinueOkClick(object sender, RoutedEventArgs e) => this.Ok();

    private void OnContinueCancelClick(object sender, RoutedEventArgs e) => this.OnCancel();

    private void CancelPathMouseLeftUp(object sender, MouseButtonEventArgs e) => this.OnCancel();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/loginbindthirddialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.ContinueGrid = (Grid) target;
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.CancelPathMouseLeftUp);
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnContinueCancelClick);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnContinueOkClick);
          break;
        case 5:
          this.InputGrid = (Grid) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.CancelPathMouseLeftUp);
          break;
        case 7:
          ((PasswordBox) target).PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case 8:
          ((PasswordBox) target).PasswordChanged += new RoutedEventHandler(this.OnPasswordComfirmChanged);
          break;
        case 9:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnNewPasswordCancelClick);
          break;
        case 10:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnNewPasswordOkClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
