// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.SetRepeatDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net;
using Ical.Net.DataTypes;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class SetRepeatDialog : UserControl, ITabControl, IComponentConnector
  {
    private const string None = "None";
    private const string Daily = "Daily";
    private const string Weekly = "Weekly";
    private const string Monthly = "Monthly";
    private const string Yearly = "Yearly";
    private const string Lunar = "Lunar";
    private const string OfficialWorkingDays = "OfficialWorkingDays";
    private const string Custom = "Custom";
    private const string Ebbinghaus = "Ebbinghaus";
    private bool _calendarMode;
    private readonly Popup _popup;
    private RepeatExtra _repeat;
    internal UpDownSelectListView ListView;
    internal Grid CustomContainer;
    internal CustomRepeatDialog CustomRepeatDialog;
    private bool _contentLoaded;

    public SetRepeatDialog(Popup popup, RepeatExtra repeat, bool calenderMode = false)
    {
      this.InitializeComponent();
      this._popup = popup;
      this._repeat = repeat;
      this._calendarMode = calenderMode;
      this._popup.Child = (UIElement) this;
      this._popup.Closed -= new EventHandler(this.OnPopupClosed);
      this._popup.Closed += new EventHandler(this.OnPopupClosed);
      this.SetButtonSelected(repeat, calenderMode);
      this.ListView.ItemTemplateSelector = (DataTemplateSelector) new RepeatTemplateSelector();
    }

    public void Reset(RepeatExtra repeatExtra, bool calender = false)
    {
      this._repeat = repeatExtra;
      this._calendarMode = calender;
      this.SetButtonSelected(repeatExtra, calender);
      this.ListView.Visibility = Visibility.Visible;
      this.CustomContainer.Visibility = Visibility.Collapsed;
    }

    private void SetButtonSelected(RepeatExtra repeatExtra, bool calendarMode = false)
    {
      DateTime date = Utils.IsEmptyDate(this._repeat.DefaultDate) ? DateTime.Today : this._repeat.DefaultDate;
      bool flag1 = string.IsNullOrEmpty(repeatExtra?.RepeatFlag);
      bool flag2 = repeatExtra?.RepeatFrom == "2";
      List<RepeatViewModel> source = new List<RepeatViewModel>()
      {
        new RepeatViewModel(Utils.GetString("EveryDay"), "", "Daily", !flag1 & flag2 && repeatExtra.RepeatFlag.Contains("DAILY") && !repeatExtra.RepeatFlag.Contains("TT_SKIP=HOLIDAY,WEEKEND")),
        new RepeatViewModel(Utils.GetString("EveryWeek"), "(" + DateUtils.GetWeekTextByWeekDay((int) date.DayOfWeek) + ")", "Weekly", !flag1 & flag2 && repeatExtra.RepeatFlag.Contains("WEEKLY")),
        new RepeatViewModel(Utils.GetString("EveryMonth"), "(" + DateUtils.FormatDay(date) + ")", "Monthly", !flag1 & flag2 && repeatExtra.RepeatFlag.Contains("MONTHLY")),
        new RepeatViewModel(Utils.GetString("EveryYear"), "(" + DateUtils.FormatShortMonthDay(date) + ")", "Yearly", !flag1 && (flag2 || repeatExtra.RepeatFlag.Contains("BYMONTH")) && repeatExtra.RepeatFlag.Contains("YEARLY") && !repeatExtra.RepeatFlag.Contains("LUNAR")),
        new RepeatViewModel(),
        new RepeatViewModel(Utils.GetString("Custom"), "", "Custom", !flag1 && !flag2 && (!repeatExtra.RepeatFlag.Contains("BYMONTH") || !repeatExtra.RepeatFlag.Contains("YEARLY")) && !repeatExtra.RepeatFlag.Contains("FORGETTINGCURVE"))
      };
      if (!calendarMode)
      {
        source.Insert(5, new RepeatViewModel());
        source.Insert(5, new RepeatViewModel(Utils.GetString("Ebbinghaus"), "", "Ebbinghaus", !flag1 && repeatExtra.RepeatFlag.Contains("FORGETTINGCURVE")));
        source.Insert(5, new RepeatViewModel(Utils.GetString("EveryLunarYear"), "(" + DateUtils.GetLunarMonthDay(date) + ")", "Lunar", !flag1 & flag2 && repeatExtra.RepeatFlag.Contains("LUNAR")));
        source.Insert(5, new RepeatViewModel(Utils.GetString("OfficialWorkingDays"), "(" + Utils.GetString("MonToFri") + ")", "OfficialWorkingDays", !flag1 & flag2 && repeatExtra.RepeatFlag.Contains("TT_SKIP=HOLIDAY,WEEKEND")));
      }
      (source.FirstOrDefault<RepeatViewModel>((Func<RepeatViewModel, bool>) (i => i.Selected)) ?? source[0]).HoverSelected = true;
      this.ListView.ItemsSource = (IEnumerable) source;
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      EventHandler popupClosed = this.PopupClosed;
      if (popupClosed == null)
        return;
      popupClosed(sender, e);
    }

    public bool IsShowing => this._popup != null && this._popup.IsOpen;

    public event EventHandler<RepeatExtra> RepeatSelect;

    public event EventHandler PopupClosed;

    public event EventHandler PopupOpened;

    private void OnItemSelected(bool enter, UpDownSelectViewModel e)
    {
      if (!(e is RepeatViewModel repeatViewModel))
        return;
      RecurrencePattern recurrencePattern = new RecurrencePattern()
      {
        Interval = 1
      };
      string str1 = repeatViewModel.Value;
      if (str1 != null)
      {
        switch (str1.Length)
        {
          case 4:
            if (str1 == "None")
              break;
            break;
          case 5:
            switch (str1[0])
            {
              case 'D':
                if (str1 == "Daily")
                {
                  recurrencePattern.Frequency = FrequencyType.Daily;
                  break;
                }
                break;
              case 'L':
                if (str1 == "Lunar")
                  break;
                break;
            }
            break;
          case 6:
            switch (str1[0])
            {
              case 'C':
                if (str1 == "Custom")
                  break;
                break;
              case 'W':
                if (str1 == "Weekly")
                {
                  recurrencePattern.Frequency = FrequencyType.Weekly;
                  break;
                }
                break;
              case 'Y':
                if (str1 == "Yearly")
                {
                  recurrencePattern.Frequency = FrequencyType.Yearly;
                  break;
                }
                break;
            }
            break;
          case 7:
            if (str1 == "Monthly")
            {
              recurrencePattern.Frequency = FrequencyType.Monthly;
              break;
            }
            break;
          case 10:
            if (str1 == "Ebbinghaus")
              break;
            break;
        }
      }
      switch (repeatViewModel.Value)
      {
        case "Lunar":
          EventHandler<RepeatExtra> repeatSelect1 = this.RepeatSelect;
          if (repeatSelect1 != null)
          {
            repeatSelect1((object) this, this.GetLunarRepeatFlag());
            break;
          }
          break;
        case "Custom":
          this.ShowCustomRepeatDialog(enter);
          break;
        case "Ebbinghaus":
          EventHandler<RepeatExtra> repeatSelect2 = this.RepeatSelect;
          if (repeatSelect2 != null)
          {
            repeatSelect2((object) this, this.GetEbbinghausRepeatFlag());
            break;
          }
          break;
        case "Daily":
          EventHandler<RepeatExtra> repeatSelect3 = this.RepeatSelect;
          if (repeatSelect3 != null)
          {
            repeatSelect3((object) this, new RepeatExtra()
            {
              RepeatFlag = "RRULE:FREQ=DAILY",
              RepeatFrom = "2"
            });
            break;
          }
          break;
        case "OfficialWorkingDays":
          EventHandler<RepeatExtra> repeatSelect4 = this.RepeatSelect;
          if (repeatSelect4 != null)
          {
            repeatSelect4((object) this, new RepeatExtra()
            {
              RepeatFlag = "RRULE:FREQ=DAILY;INTERVAL=1;TT_SKIP=HOLIDAY,WEEKEND",
              RepeatFrom = "2"
            });
            break;
          }
          break;
        default:
          string str2 = string.Empty;
          if (recurrencePattern.ToString() != "FREQ:NONE")
            str2 = "RRULE:" + recurrencePattern?.ToString();
          EventHandler<RepeatExtra> repeatSelect5 = this.RepeatSelect;
          if (repeatSelect5 != null)
          {
            repeatSelect5((object) this, new RepeatExtra()
            {
              RepeatFlag = str2,
              RepeatFrom = "2"
            });
            break;
          }
          break;
      }
      if (!(repeatViewModel.Value != "Custom"))
        return;
      this._popup.IsOpen = false;
    }

    private RepeatExtra GetEbbinghausRepeatFlag()
    {
      return new RepeatExtra()
      {
        RepeatFrom = "0",
        RepeatFlag = RepeatUtils.GetEbbinghausRepeatFlag()
      };
    }

    private RepeatExtra GetLunarRepeatFlag()
    {
      return new RepeatExtra()
      {
        RepeatFrom = "2",
        RepeatFlag = RepeatUtils.GetLunarRepeatFlag(this._repeat.DefaultDate)
      };
    }

    private void ShowCustomRepeatDialog(bool enter)
    {
      RepeatFromType repeatType = RepeatUtils.GetRepeatType(this._repeat.RepeatFrom, this._repeat.RepeatFlag);
      string repeatFlag1 = string.Empty;
      if (this._repeat.RepeatFrom != "2")
        repeatFlag1 = this._repeat.RepeatFlag;
      this.CustomRepeatDialog.Init(this._popup, repeatType, repeatFlag1, this._calendarMode, new DateTime?(this._repeat.DefaultDate));
      this.CustomRepeatDialog.SetTabEnter(enter);
      this.CustomRepeatDialog.OnCustomRepeatSet += (CustomRepeatDialog.CustomRepeatSetDelegate) ((customRepeatFrom, repeatFlag) =>
      {
        this._repeat.RepeatFrom = customRepeatFrom;
        this._repeat.RepeatFlag = repeatFlag;
        EventHandler<RepeatExtra> repeatSelect = this.RepeatSelect;
        if (repeatSelect == null)
          return;
        repeatSelect((object) this.CustomRepeatDialog, new RepeatExtra()
        {
          RepeatFrom = customRepeatFrom,
          RepeatFlag = repeatFlag
        });
      });
      this.ListView.Visibility = Visibility.Collapsed;
      this.CustomContainer.Visibility = Visibility.Visible;
    }

    public void ClosePopup() => this._popup.IsOpen = false;

    public ITabControl GetTabControlChild()
    {
      return this.ListView.Visibility == Visibility.Visible ? (ITabControl) this.ListView : (ITabControl) this.CustomRepeatDialog;
    }

    public bool HandleTab(bool shift) => this.GetTabControlChild().HandleTab(shift);

    public bool HandleEnter() => this.GetTabControlChild().HandleEnter();

    public bool HandleEsc() => this.GetTabControlChild().HandleEsc();

    public bool UpDownSelect(bool isUp) => this.GetTabControlChild().UpDownSelect(isUp);

    public bool LeftRightSelect(bool isLeft) => this.GetTabControlChild().LeftRightSelect(isLeft);

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/setrepeatdialog.xaml", UriKind.Relative));
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
          this.ListView = (UpDownSelectListView) target;
          break;
        case 2:
          this.CustomContainer = (Grid) target;
          break;
        case 3:
          this.CustomRepeatDialog = (CustomRepeatDialog) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
