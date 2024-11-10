// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.ProxySettings
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Network;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class ProxySettings : UserControl, IComponentConnector
  {
    private ProxyModel testFailProxyModel;
    public static readonly DependencyProperty WindowModeProperty = DependencyProperty.Register(nameof (WindowMode), typeof (bool), typeof (ProxySettings), new PropertyMetadata((object) true, (PropertyChangedCallback) null));
    internal ProxySettings RootView;
    internal CustomSimpleComboBox ProxyTypeCombox;
    internal TextBox AddressText;
    internal TextBox PortText;
    internal TextBox UserText;
    internal TextBlock PasswordHint;
    internal PasswordBox PasswordText;
    internal TextBox DomainText;
    internal Button SaveButton;
    internal Button TestButton;
    private bool _contentLoaded;

    public event EventHandler Cancel;

    public event EventHandler Save;

    public bool WindowMode
    {
      get => (bool) this.GetValue(ProxySettings.WindowModeProperty);
      set => this.SetValue(ProxySettings.WindowModeProperty, (object) value);
    }

    public ProxySettings()
    {
      this.InitializeComponent();
      this.InitData();
    }

    private void InitData()
    {
      LocalSettings settings = LocalSettings.Settings;
      this.ProxyTypeCombox.SelectedIndex = settings.ProxyType;
      this.ProxyTypeCombox.ItemsSource = new List<string>()
      {
        Utils.GetString("NoProxy"),
        Utils.GetString("HttpProxy"),
        Utils.GetString("BrowserSettings")
      };
      this.AddressText.Text = settings.ProxyAddress;
      this.PortText.Text = settings.ProxyPort;
      this.UserText.Text = settings.ProxyUsername;
      if (!string.IsNullOrEmpty(settings.ProxyPassword))
      {
        this.PasswordText.Password = Utils.Base64Decode(settings.ProxyPassword);
        this.PasswordHint.Visibility = Visibility.Collapsed;
      }
      else
        this.PasswordHint.Visibility = Visibility.Visible;
      if (this.WindowMode)
        this.SaveButton.Visibility = Visibility.Visible;
      this.SetEnabled();
    }

    private void OnProxyTypeChanged(object sender, SimpleComboBoxViewModel e) => this.SetEnabled();

    private void SetEnabled()
    {
      this.SetTextEnabled(this.ProxyTypeCombox.SelectedIndex == 1);
      this.SetTestEnabled(this.ProxyTypeCombox.SelectedIndex == 1 || this.ProxyTypeCombox.SelectedIndex == 2);
    }

    private void SetTestEnabled(bool enabled) => this.TestButton.IsEnabled = enabled;

    private async void OnTestClick(object sender, RoutedEventArgs e)
    {
      UIElement ui = sender as UIElement;
      if (ui == null)
        ;
      else
      {
        ui.IsEnabled = false;
        try
        {
          this.testFailProxyModel = this.GetProxyModel();
          if (this.testFailProxyModel.proxy == null && (this.ProxyTypeCombox.SelectedIndex == 1 || this.ProxyTypeCombox.SelectedIndex == 2))
          {
            int num1 = (int) MessageBox.Show(Utils.GetString("NetworkFailed"));
          }
          else
          {
            string str = await ApiClient.TestProxy(this.testFailProxyModel);
            this.testFailProxyModel = (ProxyModel) null;
            int num2 = (int) MessageBox.Show(Utils.GetString("NetworkOk"));
            this.SaveProxy();
          }
        }
        catch (Exception ex)
        {
          int num = (int) MessageBox.Show(Utils.GetString("NetworkFailed"));
        }
        finally
        {
          ui.Dispatcher.Invoke<bool>((Func<bool>) (() => ui.IsEnabled = true));
        }
      }
    }

    private ProxyModel GetProxyModel()
    {
      return ProxyModel.Create(this.ProxyTypeCombox.SelectedIndex, this.AddressText.Text.Trim(), this.PortText.Text.Trim(), this.UserText.Text.Trim(), this.PasswordText.Password.Trim(), this.DomainText.Text.Trim());
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      ProxySettings sender1 = this;
      sender1.SaveProxy();
      EventHandler save = sender1.Save;
      if (save == null)
        return;
      save((object) sender1, (EventArgs) null);
    }

    public void SaveProxy()
    {
      if (this.GetProxyModel().Equals((object) this.testFailProxyModel) && !this.SaveButton.IsEnabled)
        return;
      LocalSettings settings = LocalSettings.Settings;
      settings.ProxyType = this.ProxyTypeCombox.SelectedIndex;
      settings.ProxyAddress = this.AddressText.Text.Trim();
      settings.ProxyPort = this.PortText.Text.Trim();
      settings.ProxyUsername = this.UserText.Text.Trim();
      if (string.IsNullOrEmpty(this.PasswordText.Password))
        return;
      settings.ProxyPassword = Utils.Base64Encode(this.PasswordText.Password.Trim());
    }

    private void SetTextEnabled(bool enabled)
    {
      this.AddressText.IsEnabled = enabled;
      this.PortText.IsEnabled = enabled;
      this.UserText.IsEnabled = enabled;
      this.PasswordText.IsEnabled = enabled;
      this.DomainText.IsEnabled = enabled;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler cancel = this.Cancel;
      if (cancel == null)
        return;
      cancel((object) this, (EventArgs) null);
    }

    private void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
      this.PasswordHint.Visibility = string.IsNullOrEmpty(this.PasswordText.Password) ? Visibility.Visible : Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/proxysettings.xaml", UriKind.Relative));
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
          this.RootView = (ProxySettings) target;
          break;
        case 2:
          this.ProxyTypeCombox = (CustomSimpleComboBox) target;
          break;
        case 3:
          this.AddressText = (TextBox) target;
          break;
        case 4:
          this.PortText = (TextBox) target;
          break;
        case 5:
          this.UserText = (TextBox) target;
          break;
        case 6:
          this.PasswordHint = (TextBlock) target;
          break;
        case 7:
          this.PasswordText = (PasswordBox) target;
          this.PasswordText.PasswordChanged += new RoutedEventHandler(this.OnPasswordChanged);
          break;
        case 8:
          this.DomainText = (TextBox) target;
          break;
        case 9:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 10:
          this.TestButton = (Button) target;
          this.TestButton.Click += new RoutedEventHandler(this.OnTestClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
