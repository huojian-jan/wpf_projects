// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.AddUrlCalendarWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.MainListView;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class AddUrlCalendarWindow : Window, IComponentConnector
  {
    private readonly ticktick_WPF.Views.Config.SubscribeCalendar _parent;
    internal TextBox UrlEditText;
    internal TextBlock InvalidHintText;
    internal Button SaveButton;
    internal Button CancelButton;
    private bool _contentLoaded;

    public AddUrlCalendarWindow(ticktick_WPF.Views.Config.SubscribeCalendar parent = null)
    {
      this.InitializeComponent();
      this._parent = parent;
      this.InitData();
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void InitData()
    {
      try
      {
        string text = Clipboard.GetText();
        if (string.IsNullOrEmpty(text) || !text.StartsWith("https:") && !text.StartsWith("webcal:") && !text.EndsWith("calendar.ics"))
          return;
        this.UrlEditText.Text = text;
        this.UrlEditText.SelectAll();
      }
      catch (Exception ex)
      {
      }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      AddUrlCalendarWindow urlCalendarWindow = this;
      urlCalendarWindow.SetSaving(true);
      urlCalendarWindow.InvalidHintText.Visibility = Visibility.Collapsed;
      string url = urlCalendarWindow.UrlEditText.Text.Trim();
      if (CacheManager.GetSubscribeCalendars().Any<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (c => c.Url == url)))
      {
        urlCalendarWindow.ShowInvalidHint(Utils.GetString("UrlExist"));
        urlCalendarWindow.SetSaving(false);
      }
      else
      {
        if (await urlCalendarWindow.SubscribeCalendar(url))
        {
          urlCalendarWindow._parent?.LoadData();
          ListViewContainer.ReloadProjectData();
          urlCalendarWindow.Close();
        }
        else
          urlCalendarWindow.ShowInvalidHint(Utils.GetString("UrlNotValid"));
        urlCalendarWindow.SetSaving(false);
      }
    }

    private void SetSaving(bool isSaving)
    {
      if (isSaving)
      {
        this.SaveButton.Content = (object) Utils.GetString("Subscribing");
        this.SaveButton.IsEnabled = false;
        this.SaveButton.Opacity = 0.699999988079071;
        this.UrlEditText.IsReadOnly = true;
        this.CancelButton.IsEnabled = false;
      }
      else
      {
        this.SaveButton.Content = (object) Utils.GetString("Subscribe");
        this.SaveButton.IsEnabled = true;
        this.SaveButton.Opacity = 1.0;
        this.UrlEditText.IsReadOnly = false;
        this.CancelButton.IsEnabled = true;
      }
    }

    private void ShowInvalidHint(string hint)
    {
      this.InvalidHintText.Text = hint;
      this.InvalidHintText.Visibility = Visibility.Visible;
      this.UrlEditText.Focus();
      this.UrlEditText.SelectAll();
    }

    private async Task<bool> SubscribeCalendar(string remotePath)
    {
      return await SubscribeCalendarHelper.SubscribeCalendar(remotePath) != null;
    }

    private async void OnAddWindowLoaded(object sender, RoutedEventArgs e)
    {
      await Task.Delay(100);
      this.UrlEditText.Focus();
    }

    private void OnInputTextChanged(object sender, TextChangedEventArgs e)
    {
      this.InvalidHintText.Visibility = Visibility.Collapsed;
    }

    protected override void OnClosed(EventArgs eventArgs)
    {
      try
      {
        if (this.Owner == null)
          return;
        this.Owner.Activate();
      }
      catch (Exception ex)
      {
      }
    }

    private void ClearUrlText(object sender, MouseButtonEventArgs e)
    {
      this.UrlEditText.Text = string.Empty;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/addurlcalendarwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnAddWindowLoaded);
          break;
        case 2:
          this.UrlEditText = (TextBox) target;
          this.UrlEditText.TextChanged += new TextChangedEventHandler(this.OnInputTextChanged);
          break;
        case 3:
          this.InvalidHintText = (TextBlock) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ClearUrlText);
          break;
        case 5:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 6:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
