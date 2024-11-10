// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.PomoLocalSetting
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Models
{
  public class PomoLocalSetting
  {
    public string PomoType { get; set; } = "Focus";

    public string PomoSound { get; set; } = "none";

    public bool PomoTimer { get; set; } = true;

    public bool AutoNextPomo { get; set; }

    public bool AutoBreak { get; set; }

    public int PomoWindowTop { get; set; } = -1;

    public int PomoWindowLeft { get; set; } = -1;

    public bool PomoTopMost { get; set; }

    public bool PomoExpand { get; set; }

    public double PomoWindowOpacity { get; set; } = 0.9;

    public int AutoPomoTimes { get; set; } = 4;

    public string PomoWindowTheme { get; set; } = "light";

    public bool LightsOn { get; set; }

    public double DetailWidth { get; set; } = 0.667;

    public bool AutoShowWidget { get; set; }

    public bool OpenWidget { get; set; }

    public bool GuideShow { get; set; }

    public string DisplayType { get; set; }

    public string MiniStatisticsTypes { get; set; } = "0,2";

    public string AddRecordType { get; set; } = "pomodoro";

    public DateTime EarliestFetchDate { get; set; }

    public string FocusEndSound { get; set; }

    public string BreakEndSound { get; set; }

    public double MiniWindowWidth { get; set; }
  }
}
