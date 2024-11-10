// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.AddSubscribeDialog
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
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class AddSubscribeDialog : ContentControl, ITabControl, IComponentConnector
  {
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof (SelectedIndex), typeof (int), typeof (AddSubscribeDialog), new PropertyMetadata((object) -1, (PropertyChangedCallback) null));
    private readonly Popup _popup;
    internal AddSubscribeDialog Root;
    internal StackPanel Container;
    internal TextBlock CalDavSummary;
    private bool _contentLoaded;

    public int SelectedIndex
    {
      get => (int) this.GetValue(AddSubscribeDialog.SelectedIndexProperty);
      set => this.SetValue(AddSubscribeDialog.SelectedIndexProperty, (object) value);
    }

    public AddSubscribeDialog(UIElement element)
    {
      this.InitializeComponent();
      EscPopup escPopup = new EscPopup();
      escPopup.PlacementTarget = element;
      escPopup.StaysOpen = false;
      escPopup.AllowsTransparency = true;
      escPopup.VerticalOffset = 25.0;
      escPopup.HorizontalOffset = -45.0;
      escPopup.Placement = PlacementMode.Right;
      this._popup = (Popup) escPopup;
      this._popup.Child = (UIElement) this;
    }

    public void Show() => this._popup.IsOpen = true;

    private void OnActionClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is FrameworkElement frameworkElement)
      {
        string str = frameworkElement.Tag.ToString();
        if (!LimitCache.CheckCalendarCount(str))
          return;
        this.OnAction(str);
      }
      this._popup.IsOpen = false;
    }

    private void OnAction(string tag)
    {
      switch (tag)
      {
        case "Google":
          BindGoogleAccount.GetInstance().Start();
          break;
        case "Exchange":
          BindAccountWindow bindAccountWindow1 = new BindAccountWindow((SubscribeCalendar) null, Constants.BindAccountType.Exchange);
          bindAccountWindow1.Owner = (Window) App.Window;
          bindAccountWindow1.Show();
          break;
        case "CalDAV":
          BindAccountWindow bindAccountWindow2 = new BindAccountWindow((SubscribeCalendar) null, Constants.BindAccountType.CalDAV);
          bindAccountWindow2.Owner = (Window) App.Window;
          bindAccountWindow2.Show();
          break;
        case "Outlook":
          CookieHelper.ClearCookie();
          new BindOutlookWindow().ShowDialog();
          break;
        case "iCloud":
          BindICloudWindow bindIcloudWindow = new BindICloudWindow((SubscribeCalendar) null);
          bindIcloudWindow.Owner = (Window) App.Window;
          bindIcloudWindow.Show();
          break;
        case "URL":
          AddUrlCalendarWindow urlCalendarWindow = new AddUrlCalendarWindow();
          urlCalendarWindow.Owner = (Window) App.Window;
          urlCalendarWindow.Show();
          break;
      }
      UserActCollectUtils.AddClickEvent("project_list_ui", "event", "add");
    }

    public bool HandleTab(bool shift)
    {
      this.UpDownSelect(shift);
      return true;
    }

    public bool HandleEnter()
    {
      if (this.SelectedIndex >= 0 && this.Container.Children.Count > this.SelectedIndex)
      {
        this.OnAction((this.Container.Children[this.SelectedIndex] is FrameworkElement child ? child.Tag : (object) null) as string);
        this._popup.IsOpen = false;
      }
      return true;
    }

    public bool HandleEsc() => false;

    public bool UpDownSelect(bool isUp)
    {
      if (this.SelectedIndex < 0)
      {
        this.SelectedIndex = isUp ? 5 : 0;
        return true;
      }
      this.SelectedIndex = (this.SelectedIndex + (isUp ? 5 : 7)) % 6;
      return true;
    }

    public bool LeftRightSelect(bool isLeft) => true;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/project/addsubscribedialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (AddSubscribeDialog) target;
          break;
        case 2:
          this.Container = (StackPanel) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnActionClick);
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnActionClick);
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnActionClick);
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnActionClick);
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnActionClick);
          break;
        case 8:
          this.CalDavSummary = (TextBlock) target;
          break;
        case 9:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnActionClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
