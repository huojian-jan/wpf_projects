// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineConstants
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public static class TimelineConstants
  {
    public const string RangeDay = "day";
    public const string RangeWeek = "week";
    public const string RangeMonth = "month";
    public const string RangeYear = "year";
    public const double OneLineHeight = 40.0;
    public const double HeadHeight = 28.0;
    public const double ShowWeekHeadHeight = 42.0;
    public const double GroupWidthMin = 100.0;
    public const double GroupWidthDefault = 140.0;
    public const double ArrangePanelWidth = 215.0;
    public const double CellMinWidth = 24.0;
    public const double ResizeThumbWidth = 8.0;
    public const double CellMargin = 2.0;
    public const double CellInlineMarginRightMin = 104.0;
    public const double CellOutlineTextMaxWidth = 120.0;
    public const double CellOutlineTextMinWidth = 30.0;
    public const string TimelineArrangeSectionStatusPrefix = "Arrange_";
    private static readonly Dictionary<int, double> OneDayWidthMap = new Dictionary<int, double>()
    {
      {
        1,
        216.0
      },
      {
        2,
        180.0
      },
      {
        3,
        144.0
      },
      {
        4,
        108.0
      },
      {
        5,
        72.0
      },
      {
        6,
        56.0
      },
      {
        7,
        44.0
      },
      {
        8,
        32.0
      },
      {
        9,
        20.0
      }
    };
    private static readonly Dictionary<string, bool> NeedsSplitLineMap = new Dictionary<string, bool>()
    {
      {
        "day",
        false
      },
      {
        "week",
        false
      },
      {
        "month",
        true
      },
      {
        "year",
        true
      }
    };
    private static readonly Dictionary<string, int> MinChangeDaysMap = new Dictionary<string, int>()
    {
      {
        "day",
        1
      },
      {
        "week",
        1
      },
      {
        "month",
        1
      },
      {
        "year",
        1
      }
    };
    private static readonly Dictionary<string, int> NewTaskDefaultDaysMap = new Dictionary<string, int>()
    {
      {
        "day",
        1
      },
      {
        "week",
        1
      },
      {
        "month",
        5
      },
      {
        "year",
        14
      }
    };
    private static readonly Dictionary<string, string> RangeI18nKey = new Dictionary<string, string>()
    {
      {
        "day",
        "TimelineDay"
      },
      {
        "week",
        "TimelineWeek"
      },
      {
        "month",
        "TimelineMonth"
      },
      {
        "year",
        "TimelineYear"
      }
    };

    public static string RangeDefault => "day";

    public static string SortDefault => Constants.SortType.sortOrder.ToString();

    public static double GetOneDayWidth(int index)
    {
      double num;
      return !TimelineConstants.OneDayWidthMap.TryGetValue(index, out num) ? TimelineConstants.OneDayWidthMap[TimelineConstants.GetRangeDefaultWidthIndex(TimelineConstants.RangeDefault)] : num;
    }

    public static bool GetNeedsSplitLine(string key)
    {
      bool flag;
      return string.IsNullOrEmpty(key) || !TimelineConstants.NeedsSplitLineMap.TryGetValue(key, out flag) ? TimelineConstants.NeedsSplitLineMap[TimelineConstants.RangeDefault] : flag;
    }

    public static int GetMinChangeDays(string key)
    {
      int num;
      return string.IsNullOrEmpty(key) || !TimelineConstants.MinChangeDaysMap.TryGetValue(key, out num) ? TimelineConstants.MinChangeDaysMap[TimelineConstants.RangeDefault] : num;
    }

    public static int GetNewTaskDefaultDays(string key)
    {
      int num;
      return string.IsNullOrEmpty(key) || !TimelineConstants.NewTaskDefaultDaysMap.TryGetValue(key, out num) ? TimelineConstants.NewTaskDefaultDaysMap[TimelineConstants.RangeDefault] : num;
    }

    public static string GetRangeI18nKey(string key)
    {
      string str;
      return string.IsNullOrEmpty(key) || !TimelineConstants.RangeI18nKey.TryGetValue(key, out str) ? TimelineConstants.RangeI18nKey[TimelineConstants.RangeDefault] : str;
    }

    public static string GetDayWidthRange(int index)
    {
      if (index >= 8)
        return "month";
      return index < 6 ? "day" : "week";
    }

    public static int GetRangeDefaultWidthIndex(string range)
    {
      switch (range)
      {
        case "day":
          return 4;
        case "week":
          return 7;
        case "month":
          return 9;
        default:
          return 4;
      }
    }
  }
}
