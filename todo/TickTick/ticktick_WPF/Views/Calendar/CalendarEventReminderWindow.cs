// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarEventReminderWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarEventReminderWindow : MyWindow, IComponentConnector
  {
    private ReminderModel _reminder;
    internal Image WindowIcon;
    internal Border TitleBorder;
    internal EmjTextBlock TitleText;
    internal Grid Buttons;
    private bool _contentLoaded;

    public CalendarEventReminderWindow(ReminderModel reminder, bool locked)
    {
      this.InitializeComponent();
      this._reminder = reminder;
      this.DataContext = (object) new CalendarReminderViewModel()
      {
        Id = reminder.EventId,
        Summary = (locked ? Utils.GetString("ReminderWhenLock") : reminder.Title),
        Description = (locked ? string.Empty : reminder.Content)
      };
      this.SourceInitialized += new EventHandler(this.OnWindowSourceInitialized);
      if (locked)
      {
        this.TitleBorder.SetValue(Grid.RowSpanProperty, (object) 2);
        this.TitleText.Margin = new Thickness(10.0, 20.0, 10.0, 20.0);
        this.Buttons.Visibility = Visibility.Collapsed;
      }
      Rect workArea = SystemParameters.WorkArea;
      this.Left = workArea.Width - this.Width;
      workArea = SystemParameters.WorkArea;
      this.Top = workArea.Height - 130.0 - 100.0;
      this.SetWindowIcon();
      ticktick_WPF.Notifier.GlobalEventManager.RemindHandled += new EventHandler<RemindMessage>(this.OnRemindHandled);
    }

    private void OnRemindHandled(object sender, RemindMessage e)
    {
    }

    private void OnWindowSourceInitialized(object sender, EventArgs e)
    {
      IntPtr handle = new WindowInteropHelper((Window) this).Handle;
      if (!(handle != IntPtr.Zero))
        return;
      NativeUtils.ShowWindow(handle, 4);
    }

    private void SetWindowIcon()
    {
      this.WindowIcon.Source = (ImageSource) AppIconUtils.GetIconImage();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => this.Close();

    private async void OnViewDetailClick(object sender, MouseButtonEventArgs e)
    {
      CalendarEventReminderWindow eventReminderWindow = this;
      if (eventReminderWindow.DataContext != null && eventReminderWindow.DataContext is CalendarReminderViewModel dataContext)
      {
        CalendarEventModel eventByEventId = await CalendarEventDao.GetEventByEventId(dataContext.Id);
        if (eventByEventId != null)
          App.NavigateEvent(eventByEventId.Id);
      }
      eventReminderWindow.Close();
    }

    private async void OnViewButtonClick(object sender, RoutedEventArgs e)
    {
      CalendarEventReminderWindow eventReminderWindow = this;
      if (eventReminderWindow.DataContext != null && eventReminderWindow.DataContext is CalendarReminderViewModel dataContext)
      {
        CalendarEventModel eventByEventId = await CalendarEventDao.GetEventByEventId(dataContext.Id);
        if (eventByEventId != null)
          App.NavigateEvent(eventByEventId.Id);
      }
      eventReminderWindow.Close();
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (Mouse.LeftButton != MouseButtonState.Pressed)
        return;
      this.DragMove();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      Communicator.NotifyCloseReminder(new RemindMessage(this._reminder));
      ticktick_WPF.Notifier.GlobalEventManager.RemindHandled -= new EventHandler<RemindMessage>(this.OnRemindHandled);
      base.OnClosing(e);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/calendareventreminderwindow.xaml", UriKind.Relative));
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
          this.WindowIcon = (Image) target;
          break;
        case 2:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCloseClick);
          break;
        case 3:
          ((UIElement) target).PreviewMouseDown += new MouseButtonEventHandler(this.OnPreviewMouseDown);
          break;
        case 4:
          this.TitleBorder = (Border) target;
          this.TitleBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnViewDetailClick);
          break;
        case 5:
          this.TitleText = (EmjTextBlock) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnViewDetailClick);
          break;
        case 7:
          this.Buttons = (Grid) target;
          break;
        case 8:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnViewButtonClick);
          break;
        case 9:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCloseClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
