// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.TouchPad;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarWindow : IndependentWindow
  {
    public static CalendarWindow Window;
    public static string Name = DisplayModule.Calendar.ToString();
    private CalendarControl _calendar;

    public static bool IsShowing => CalendarWindow.Window != null;

    private CalendarWindow(WindowModel windowModel)
      : base(windowModel)
    {
      this.MinWidth = 580.0;
      this.MinHeight = 462.0;
      this.KeyUp += new KeyEventHandler(this.OnWindowKeyUp);
      this.Id = CalendarWindow.Name;
      this._calendar = new CalendarControl(true);
      this.Container.Children.Add((UIElement) this._calendar);
      this.Title = Utils.GetString("Calendar");
      this.Loaded += new RoutedEventHandler(this.OnWindowLoaded);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      if (!(PresentationSource.FromVisual((Visual) this) is HwndSource hwndSource))
        return;
      hwndSource.AddHook(new HwndSourceHook(this.Hook));
      if (!TouchPadHelper.TouchpadHelper.Exists())
        return;
      TouchPadHelper.TouchpadHelper.RegisterInput(hwndSource.Handle);
    }

    private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      switch (msg)
      {
        case (int) byte.MaxValue:
          TouchpadContact[] input = TouchPadHelper.TouchpadHelper.ParseInput(lParam);
          if ((input != null ? (((IEnumerable<TouchpadContact>) input).Any<TouchpadContact>((Func<TouchpadContact, bool>) (c => c.ContactId == 1)) ? 1 : 0) : 0) != 0)
          {
            this.OnDoubleFingerTouch();
            break;
          }
          break;
        case 526:
          this.OnMouseTilt(CalendarWindow.LOWORD(wParam));
          return (IntPtr) 1;
      }
      return IntPtr.Zero;
    }

    private void OnDoubleFingerTouch() => this._calendar?.OnDoubleFingerTouch();

    private static int HIWORD(IntPtr ptr) => ptr.ToInt32() >> 16 & (int) ushort.MaxValue;

    private static int LOWORD(IntPtr ptr) => ((int) ptr.ToInt64() >> 16) % 256;

    private void OnMouseTilt(int offset)
    {
      if (!this.IsActive)
        return;
      this._calendar?.OnTouchScroll(offset);
    }

    private void OnWindowKeyUp(object sender, KeyEventArgs e) => this._calendar?.OnKeyUp(sender, e);

    public static async Task ShowWindow(bool force = true)
    {
      WindowModel windowModel = await WindowModelDao.GetModelByIdAsync(CalendarWindow.Name);
      if (!force)
      {
        if (windowModel == null)
        {
          windowModel = (WindowModel) null;
          return;
        }
        if (windowModel.Closed)
        {
          windowModel = (WindowModel) null;
          return;
        }
      }
      if (CalendarWindow.Window != null)
      {
        CalendarWindow.Window.Show();
        CalendarWindow.Window.Activate();
        CalendarWindow.Window.Reload();
        windowModel = (WindowModel) null;
      }
      else
      {
        if (windowModel == null || windowModel.Closed)
          IndependentWindow.AddShowEvent("calendar");
        if (windowModel == null)
          windowModel = await IndependentWindow.GetNewWindowModel(CalendarWindow.Name);
        else
          await WindowModelDao.OpenWindow(windowModel);
        CalendarWindow.Window = new CalendarWindow(windowModel);
        CalendarWindow.Window.Show();
        CalendarWindow.Window.SetBackBrush();
        windowModel = (WindowModel) null;
      }
    }

    private void Reload() => this._calendar?.Reload();

    protected override void OnClosing()
    {
      this.Container.Children.Clear();
      CalendarWindow.Window = (CalendarWindow) null;
    }

    public override void Print() => this._calendar?.Print(false);

    public static void OpenOrCloseWindow(System.Windows.Window window)
    {
      if (CalendarWindow.Window != null)
      {
        CalendarWindow.Window.Close();
      }
      else
      {
        if (!ProChecker.CheckPro(ProType.CalendarView) || !IndependentWindow.CheckCount(window))
          return;
        CalendarWindow.ShowWindow();
      }
    }
  }
}
