// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.WeekMonthSwitch
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class WeekMonthSwitch : UserControl, IComponentConnector
  {
    private int _multiDays;
    private int _multiWeeks;
    internal WeekMonthSwitch Root;
    internal Border SwitchBorder;
    internal EscPopup ModePopup;
    internal Path DayIcon;
    internal TextBlock Day;
    internal Path WeekIcon;
    internal TextBlock Week;
    internal Path MonthIcon;
    internal TextBlock Month;
    internal ContentControl MultiDayGrid;
    internal Path MultiDayIcon;
    internal TextBlock MultiDay;
    internal TextBlock MultiDayTextBlock;
    internal Border SubDay;
    internal TextBlock MultiDayText;
    internal Border AddDay;
    internal ContentControl MultiWeekGrid;
    internal Path MultiWeekIcon;
    internal TextBlock MultiWeek;
    internal TextBlock MultiWeekTextBlock;
    internal Border SubWeek;
    internal TextBlock MultiWeekText;
    internal Border AddWeek;
    private bool _contentLoaded;

    public event WeekMonthSwitch.SwitchModeDelegate SwitchMode;

    public WeekMonthSwitch() => this.InitializeComponent();

    public void NavigateMode(string mode) => ((WeekMonthViewModel) this.DataContext).Mode = mode;

    public void QuickSwitch(Key key)
    {
      string mode = ((WeekMonthViewModel) this.DataContext).Mode;
      string to = "0";
      string label = "";
      switch (key)
      {
        case Key.D1:
        case Key.D:
          to = "2";
          label = "day_view";
          break;
        case Key.D2:
        case Key.W:
          to = "1";
          label = "week_view";
          break;
        case Key.D3:
        case Key.M:
          to = "0";
          label = "month_view";
          break;
      }
      if (!(mode != to))
        return;
      UserActCollectUtils.AddShortCutEvent("switch_views", label);
      ((WeekMonthViewModel) this.DataContext).Mode = to;
      WeekMonthSwitch.SwitchModeDelegate switchMode = this.SwitchMode;
      if (switchMode == null)
        return;
      switchMode(mode, to);
    }

    private void ShowSwitchPopup(object sender, MouseButtonEventArgs e)
    {
      this._multiDays = LocalSettings.Settings.ExtraSettings.GetCalendarDefaultMultiNum(true);
      this._multiWeeks = LocalSettings.Settings.ExtraSettings.GetCalendarDefaultMultiNum(false);
      string mode = ((WeekMonthViewModel) this.DataContext).Mode;
      bool flag1 = mode.StartsWith("D");
      bool flag2 = mode.StartsWith("W");
      int result;
      if (flag2 | flag1 && int.TryParse(mode.Substring(1), out result))
      {
        if (flag1)
          this._multiDays = result;
        else
          this._multiWeeks = result;
      }
      this.MultiDayText.Text = this._multiDays.ToString() ?? "";
      this.MultiWeekText.Text = this._multiWeeks.ToString() ?? "";
      this.MultiDayTextBlock.Text = this._multiDays.ToString() + " " + Utils.GetString(this._multiDays > 1 ? "PublicDays" : "PublicDay");
      this.MultiWeekTextBlock.Text = this._multiWeeks.ToString() + " " + Utils.GetString("PublicWeeks");
      switch (mode)
      {
        case "2":
          this.SetItemSelected(this.DayIcon, this.Day);
          break;
        case "1":
          this.SetItemSelected(this.WeekIcon, this.Week);
          break;
        case "0":
          this.SetItemSelected(this.MonthIcon, this.Month);
          break;
      }
      if (flag1)
        this.SetItemSelected(this.MultiDayIcon, this.MultiDay);
      else if (flag2)
        this.SetItemSelected(this.MultiWeekIcon, this.MultiWeek);
      this.SetAdjustButtonEnable();
      this.ModePopup.IsOpen = true;
    }

    private void SetAdjustButtonEnable()
    {
      this.SubDay.Cursor = this._multiDays > 1 ? Cursors.Hand : Cursors.No;
      this.SubDay.Opacity = this._multiDays > 1 ? 1.0 : 0.4;
      this.AddDay.Cursor = this._multiDays < 14 ? Cursors.Hand : Cursors.No;
      this.AddDay.Opacity = this._multiDays < 14 ? 1.0 : 0.4;
      this.SubWeek.Cursor = this._multiWeeks > 2 ? Cursors.Hand : Cursors.No;
      this.SubWeek.Opacity = this._multiWeeks > 2 ? 1.0 : 0.4;
      this.AddWeek.Cursor = this._multiWeeks < 6 ? Cursors.Hand : Cursors.No;
      this.AddWeek.Opacity = this._multiWeeks < 6 ? 1.0 : 0.4;
    }

    private void SetItemSelected(Path icon, TextBlock text)
    {
      this.DayIcon.Visibility = Visibility.Collapsed;
      this.WeekIcon.Visibility = Visibility.Collapsed;
      this.MonthIcon.Visibility = Visibility.Collapsed;
      this.MultiDayIcon.Visibility = Visibility.Collapsed;
      this.MultiWeekIcon.Visibility = Visibility.Collapsed;
      this.Day.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this.Week.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this.Month.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this.MultiDay.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this.MultiWeek.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      icon.Visibility = Visibility.Visible;
      text.SetResourceReference(TextBlock.ForegroundProperty, (object) "PrimaryColor");
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      string to = ((WeekMonthViewModel) this.DataContext).Mode;
      string from = to;
      if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is string tag)
      {
        switch (tag)
        {
          case "Day":
            to = "2";
            this.SetItemSelected(this.DayIcon, this.Day);
            break;
          case "Week":
            to = "1";
            this.SetItemSelected(this.WeekIcon, this.Week);
            break;
          case "Month":
            to = "0";
            this.SetItemSelected(this.MonthIcon, this.Month);
            break;
          case "MultiDay":
            to = "D" + this._multiDays.ToString();
            this.SetItemSelected(this.MultiDayIcon, this.MultiDay);
            break;
          case "MultiWeek":
            to = "W" + this._multiWeeks.ToString();
            this.SetItemSelected(this.MultiWeekIcon, this.MultiWeek);
            break;
        }
      }
      if (from != to)
      {
        ((WeekMonthViewModel) this.DataContext).Mode = to;
        WeekMonthSwitch.SwitchModeDelegate switchMode = this.SwitchMode;
        if (switchMode != null)
          switchMode(from, to);
      }
      if (this.SubDay.IsMouseOver || this.AddDay.IsMouseOver || this.SubWeek.IsMouseOver || this.AddWeek.IsMouseOver)
        return;
      this.ModePopup.IsOpen = false;
    }

    private void OnChangeNumClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is string tag)
      {
        switch (tag)
        {
          case "+Day":
            if (this._multiDays >= 14)
              return;
            this._multiDays = Math.Min(14, this._multiDays + 1);
            LocalSettings.Settings.ExtraSettings.SetCalendarDefaultMultiNum(true, this._multiDays);
            break;
          case "-Day":
            if (this._multiDays <= 1)
              return;
            this._multiDays = Math.Max(1, this._multiDays - 1);
            LocalSettings.Settings.ExtraSettings.SetCalendarDefaultMultiNum(true, this._multiDays);
            break;
          case "+Week":
            if (this._multiWeeks >= 6)
              return;
            this._multiWeeks = Math.Min(6, this._multiWeeks + 1);
            LocalSettings.Settings.ExtraSettings.SetCalendarDefaultMultiNum(false, this._multiWeeks);
            break;
          case "-Week":
            if (this._multiWeeks <= 2)
              return;
            this._multiWeeks = Math.Max(2, this._multiWeeks - 1);
            LocalSettings.Settings.ExtraSettings.SetCalendarDefaultMultiNum(false, this._multiWeeks);
            break;
        }
      }
      this.MultiDayText.Text = this._multiDays.ToString() ?? "";
      this.MultiWeekText.Text = this._multiWeeks.ToString() ?? "";
      this.MultiDayTextBlock.Text = this._multiDays.ToString() + " " + Utils.GetString(this._multiDays > 1 ? "PublicDays" : "PublicDay");
      this.MultiWeekTextBlock.Text = this._multiWeeks.ToString() + " " + Utils.GetString("PublicWeeks");
      this.SetAdjustButtonEnable();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/weekmonthswitch.xaml", UriKind.Relative));
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
          this.Root = (WeekMonthSwitch) target;
          break;
        case 2:
          this.SwitchBorder = (Border) target;
          this.SwitchBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.ShowSwitchPopup);
          break;
        case 3:
          this.ModePopup = (EscPopup) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 5:
          this.DayIcon = (Path) target;
          break;
        case 6:
          this.Day = (TextBlock) target;
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 8:
          this.WeekIcon = (Path) target;
          break;
        case 9:
          this.Week = (TextBlock) target;
          break;
        case 10:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 11:
          this.MonthIcon = (Path) target;
          break;
        case 12:
          this.Month = (TextBlock) target;
          break;
        case 13:
          this.MultiDayGrid = (ContentControl) target;
          this.MultiDayGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 14:
          this.MultiDayIcon = (Path) target;
          break;
        case 15:
          this.MultiDay = (TextBlock) target;
          break;
        case 16:
          this.MultiDayTextBlock = (TextBlock) target;
          break;
        case 17:
          this.SubDay = (Border) target;
          this.SubDay.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnChangeNumClick);
          break;
        case 18:
          this.MultiDayText = (TextBlock) target;
          break;
        case 19:
          this.AddDay = (Border) target;
          this.AddDay.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnChangeNumClick);
          break;
        case 20:
          this.MultiWeekGrid = (ContentControl) target;
          this.MultiWeekGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 21:
          this.MultiWeekIcon = (Path) target;
          break;
        case 22:
          this.MultiWeek = (TextBlock) target;
          break;
        case 23:
          this.MultiWeekTextBlock = (TextBlock) target;
          break;
        case 24:
          this.SubWeek = (Border) target;
          this.SubWeek.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnChangeNumClick);
          break;
        case 25:
          this.MultiWeekText = (TextBlock) target;
          break;
        case 26:
          this.AddWeek = (Border) target;
          this.AddWeek.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnChangeNumClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate void SwitchModeDelegate(string from, string to);
  }
}
