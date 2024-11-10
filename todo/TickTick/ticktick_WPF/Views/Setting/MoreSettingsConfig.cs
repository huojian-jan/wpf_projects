// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.MoreSettingsConfig
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
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.QuickAdd;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class MoreSettingsConfig : UserControl, IComponentConnector
  {
    internal ScrollViewer ScrollViewer;
    internal Border ContainerBorder;
    internal NewTaskDefault NewTaskDefault;
    internal LockSettings LockSettings;
    internal TextBlock SetPasswordButton;
    internal TemplateControl TemplateControl;
    private bool _contentLoaded;

    public MoreSettingsConfig()
    {
      this.InitializeComponent();
      this.TemplateControl.SetSettingMode();
      this.TemplateControl.Init(TemplateKind.Task);
      this.Loaded += (RoutedEventHandler) ((sender, args) => this.ScrollViewer.ScrollToTop());
    }

    public MoreSettingsConfig(TemplateKind kind, AddTaskViewModel addModel)
    {
      this.InitializeComponent();
      this.TemplateControl.SetSettingMode();
      this.TemplateControl.Init(kind, addModel);
    }

    public void ResetTaskDefault() => this.NewTaskDefault.InitData();

    private void ProxyManageClick(object sender, MouseButtonEventArgs e)
    {
      ProxyWindow proxyWindow = new ProxyWindow();
      proxyWindow.Owner = Window.GetWindow((DependencyObject) this);
      proxyWindow.ShowDialog();
    }

    public async Task DelayScrollToTemplate()
    {
      await Task.Delay(200);
      System.Windows.Point point = this.TemplateControl.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) this.ContainerBorder);
      if (point.Y <= 0.0)
        return;
      this.ScrollViewer.ScrollToVerticalOffset(point.Y);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setting/moresettingsconfig.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.ScrollViewer = (ScrollViewer) target;
          break;
        case 2:
          this.ContainerBorder = (Border) target;
          break;
        case 3:
          this.NewTaskDefault = (NewTaskDefault) target;
          break;
        case 4:
          this.LockSettings = (LockSettings) target;
          break;
        case 5:
          this.SetPasswordButton = (TextBlock) target;
          this.SetPasswordButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.ProxyManageClick);
          break;
        case 6:
          this.TemplateControl = (TemplateControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
