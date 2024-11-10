// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchOperationViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchOperationViewModel : UpDownSelectViewModel
  {
    public SearchOperationType Type { get; set; }

    public string Keyword { get; set; }

    public string Title { get; set; }

    public string Shortcut { get; set; }

    public Geometry Icon { get; set; }

    public Pinyin Pinyin { get; set; }

    public string PtfId { get; set; }

    public string SearchText { get; set; }

    public string Emoji { get; set; }

    public SearchOperationViewModel()
    {
    }

    public SearchOperationViewModel(string sectionName)
    {
      this.Title = sectionName;
      this.IsEnable = false;
    }

    public SearchOperationViewModel(SearchOperationType type)
    {
      this.Type = type;
      this.Title = SearchOperationViewModel.GetTitle(type);
      this.Pinyin = PinyinUtils.ToPinyin(this.Title);
      this.Icon = SearchOperationViewModel.GetIcon(type);
      this.Shortcut = SearchOperationViewModel.GetShortcut(type);
      this.Keyword = this.GetKeyword(type);
    }

    private string GetKeyword(SearchOperationType type)
    {
      string keyword = string.Empty;
      string str = string.Empty;
      if (ShortcutModel.GoShortcutDict.ContainsKey(type))
        keyword = "G" + ShortcutModel.GoShortcutDict[type];
      switch (type)
      {
        case SearchOperationType.AddTask:
          return LocalSettings.Settings.ShortCutModel.GetPropertyValue("AddTask");
        case SearchOperationType.GlobalAddTask:
          return LocalSettings.Settings.ShortcutAddTask;
        case SearchOperationType.ShowOrHideApp:
          return LocalSettings.Settings.ShortcutOpenOrClose;
        case SearchOperationType.OpenOrCloseFocusWindow:
          return LocalSettings.Settings.PomoShortcut;
        case SearchOperationType.NewSticky:
          return LocalSettings.Settings.CreateStickyShortCut;
        case SearchOperationType.Calendar:
          return LocalSettings.Settings.ShortCutModel.JumpCalendar;
        case SearchOperationType.Habit:
          return LocalSettings.Settings.ShortCutModel.JumpCalendar;
        case SearchOperationType.SearchTask:
          return LocalSettings.Settings.ShortCutModel.GetPropertyValue("SearchTask");
        case SearchOperationType.GoAll:
          str = LocalSettings.Settings.ShortCutModel.JumpAll;
          break;
        case SearchOperationType.GoToday:
          str = LocalSettings.Settings.ShortCutModel.JumpToday;
          break;
        case SearchOperationType.GoTomorrow:
          str = LocalSettings.Settings.ShortCutModel.JumpTomorrow;
          break;
        case SearchOperationType.GoNext7Day:
          str = LocalSettings.Settings.ShortCutModel.JumpWeek;
          break;
        case SearchOperationType.GoAssignToMe:
          str = LocalSettings.Settings.ShortCutModel.JumpAssign;
          break;
        case SearchOperationType.GoInbox:
          str = LocalSettings.Settings.ShortCutModel.JumpInbox;
          break;
        case SearchOperationType.GoCompleted:
          str = LocalSettings.Settings.ShortCutModel.JumpComplete;
          break;
        case SearchOperationType.GoAbandoned:
          str = LocalSettings.Settings.ShortCutModel.JumpAbandon;
          break;
        case SearchOperationType.GoTrash:
          str = LocalSettings.Settings.ShortCutModel.JumpTrash;
          break;
        case SearchOperationType.GoSummary:
          str = LocalSettings.Settings.ShortCutModel.JumpSummary;
          break;
        case SearchOperationType.ShowShortCut:
          return "?";
        case SearchOperationType.ShowHideSticky:
          return LocalSettings.Settings.ShowHideStickyShortCut;
      }
      if (!string.IsNullOrEmpty(str))
        keyword = keyword + " " + str;
      return keyword;
    }

    public static Geometry GetIcon(SearchOperationType type)
    {
      string index;
      switch (type)
      {
        case SearchOperationType.AddTask:
          index = "IcAdd";
          break;
        case SearchOperationType.GlobalAddTask:
          index = "IcCircleAdd";
          break;
        case SearchOperationType.OpenOrCloseFocusWindow:
          index = "IcFocus";
          break;
        case SearchOperationType.NewSticky:
          index = "IcSticky";
          break;
        case SearchOperationType.Calendar:
          index = "IcCalendar";
          break;
        case SearchOperationType.SearchTask:
          index = "IcSearch";
          break;
        case SearchOperationType.GoSettings:
          index = "IcSettings";
          break;
        case SearchOperationType.GoToday:
          index = "CalDayIcon" + DateTime.Today.Day.ToString();
          break;
        case SearchOperationType.GoNext7Day:
          index = "CalWeekIcon" + DateTime.Today.DayOfWeek.ToString().Substring(0, 2);
          break;
        case SearchOperationType.GoAssignToMe:
          index = "IcAssignToMe";
          break;
        case SearchOperationType.GoTrash:
          index = "IcTrash";
          break;
        case SearchOperationType.HelpCenter:
          index = "IcHelp";
          break;
        case SearchOperationType.ShowShortCut:
          index = "IcKeyboard";
          break;
        default:
          index = SearchOperationType.GoSettings >= type || SearchOperationType.HelpCenter <= type ? "IcLine" + type.ToString() : "IcLine" + type.ToString().Substring(2);
          break;
      }
      return Utils.GetIcon(index);
    }

    public static string GetTitle(SearchOperationType type)
    {
      return SearchOperationType.GoSettings <= type && SearchOperationType.HelpCenter > type ? string.Format(Utils.GetString("GoList"), (object) Utils.GetString(type.ToString().Substring(2))) : Utils.GetString(type.ToString());
    }

    public static string GetShortcut(SearchOperationType type)
    {
      string shortcut = string.Empty;
      string str = string.Empty;
      if (ShortcutModel.GoShortcutDict.ContainsKey(type))
        shortcut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) ShortcutModel.GoShortcutDict[type]);
      switch (type)
      {
        case SearchOperationType.AddTask:
          return LocalSettings.Settings.ShortCutModel.GetPropertyValue("AddTask");
        case SearchOperationType.GlobalAddTask:
          return LocalSettings.Settings.ShortcutAddTask;
        case SearchOperationType.ShowOrHideApp:
          return LocalSettings.Settings.ShortcutOpenOrClose;
        case SearchOperationType.OpenOrCloseFocusWindow:
          return LocalSettings.Settings.PomoShortcut;
        case SearchOperationType.NewSticky:
          return LocalSettings.Settings.CreateStickyShortCut;
        case SearchOperationType.Calendar:
          return LocalSettings.Settings.ShortCutModel.JumpCalendar;
        case SearchOperationType.Habit:
          return LocalSettings.Settings.ShortCutModel.JumpCalendar;
        case SearchOperationType.SearchTask:
          return LocalSettings.Settings.ShortCutModel.GetPropertyValue("SearchTask");
        case SearchOperationType.GoAll:
          str = LocalSettings.Settings.ShortCutModel.JumpAll;
          break;
        case SearchOperationType.GoToday:
          str = LocalSettings.Settings.ShortCutModel.JumpToday;
          break;
        case SearchOperationType.GoTomorrow:
          str = LocalSettings.Settings.ShortCutModel.JumpTomorrow;
          break;
        case SearchOperationType.GoNext7Day:
          str = LocalSettings.Settings.ShortCutModel.JumpWeek;
          break;
        case SearchOperationType.GoAssignToMe:
          str = LocalSettings.Settings.ShortCutModel.JumpAssign;
          break;
        case SearchOperationType.GoInbox:
          str = LocalSettings.Settings.ShortCutModel.JumpInbox;
          break;
        case SearchOperationType.GoCompleted:
          str = LocalSettings.Settings.ShortCutModel.JumpComplete;
          break;
        case SearchOperationType.GoAbandoned:
          str = LocalSettings.Settings.ShortCutModel.JumpAbandon;
          break;
        case SearchOperationType.GoTrash:
          str = LocalSettings.Settings.ShortCutModel.JumpTrash;
          break;
        case SearchOperationType.GoSummary:
          str = LocalSettings.Settings.ShortCutModel.JumpSummary;
          break;
        case SearchOperationType.ShowShortCut:
          return "?";
        case SearchOperationType.ShowHideSticky:
          return LocalSettings.Settings.ShowHideStickyShortCut;
      }
      if (!string.IsNullOrEmpty(str))
        shortcut = shortcut + " , " + str;
      return shortcut;
    }
  }
}
