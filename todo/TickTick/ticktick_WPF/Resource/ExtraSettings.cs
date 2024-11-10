// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Resource.ExtraSettings
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Resource
{
  public class ExtraSettings
  {
    public bool MiniCalendarEnabled;

    public long LastPullYearPromoTime { get; set; }

    public bool UseSystemTheme { get; set; }

    public string AppTheme { get; set; }

    public int LastActiveTime { get; set; }

    public int LastCompleteTime { get; set; }

    public bool TLUsed { get; set; }

    public string AppFontFamily { get; set; }

    public long LastPullHolidayTime { get; set; }

    public bool ShowCalWeekend { get; set; } = true;

    public long FocusPushPoint { get; set; }

    public string StickyDefaultColor { get; set; }

    public bool StickyDefaultPin { get; set; }

    public bool StickyHideInTaskBar { get; set; }

    public bool ResetStickyWhenAlign { get; set; }

    public int StickySpacing { get; set; } = 20;

    public int StickyFont { get; set; } = 1;

    public double StickyOpacity { get; set; } = 100.0;

    public bool ShowNoteInCalArrange { get; set; } = true;

    public bool ShowParentInCalArrange { get; set; }

    public bool MenuFold { get; set; }

    public string EpT { get; set; }

    public int CpltStoryTimes { get; set; }

    public string RemindSound { get; set; }

    public int RemindDetail { get; set; } = 1;

    public int NumDisplayType { get; set; }

    public int ShowCompleteLine { get; set; }

    public string RecentTimezone { get; set; }

    public string ShowHideStickyShortCut { get; set; }

    public string CurrentFocus { get; set; }

    public bool TwitterNotified { get; set; }

    public string CalMultiViewNums { get; set; } = "5,2";

    public int SortOrderOfSummary { get; set; } = 6400;

    public bool KbShowAdd { get; set; }

    public bool PwShowAdd { get; set; } = true;

    public int KbSize { get; set; } = 1;

    public string TodayAttachmentCount { get; set; }

    public string SelectedSummaryTemplateId { get; set; }

    public string DefaultSummaryTemplate { get; set; }

    public bool DoNotDisturbInCalendar { get; set; }

    public bool SummaryFilterImported { get; set; }

    public bool TimelineShowNote { get; set; } = true;

    public bool TimelineShowParent { get; set; }

    public bool TimelineArrangeOverDue { get; set; }

    public bool ReminderCalculated { get; set; }

    public long BindCalendarExpireCheckTime { get; set; }

    public bool SummaryTemplateTooltipShown { get; set; }

    public Dictionary<string, string> ABTestData { get; set; } = new Dictionary<string, string>();

    public string AppConfigEtag { get; set; }

    public int AppConfigInterval { get; set; } = 14400;

    public int LastClearTaskTime { get; set; }

    public string TaskFoldStatus { get; set; }

    public int ShowProjectTimes { get; set; }

    public int GetCalendarDefaultMultiNum(bool isMultiDay)
    {
      if (this.CalMultiViewNums != null)
      {
        string[] strArray = this.CalMultiViewNums.Split(',');
        int result;
        if (strArray.Length == 2 && int.TryParse(strArray[!isMultiDay ? 1 : 0], out result))
          return isMultiDay ? Math.Max(1, Math.Min(14, result)) : Math.Max(2, Math.Min(6, result));
      }
      return !isMultiDay ? 2 : 5;
    }

    public void SetCalendarDefaultMultiNum(bool isMultiDay, int num)
    {
      if (isMultiDay)
      {
        int calendarDefaultMultiNum = this.GetCalendarDefaultMultiNum(false);
        this.CalMultiViewNums = num.ToString() + "," + calendarDefaultMultiNum.ToString();
      }
      else
        this.CalMultiViewNums = this.GetCalendarDefaultMultiNum(true).ToString() + "," + num.ToString();
    }
  }
}
