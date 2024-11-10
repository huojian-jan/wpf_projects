// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TodayProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TodayProject : SmartProject
  {
    public override string Id => "_special_id_today";

    public override string Name => Utils.GetString("Today");

    public override Geometry Icon => Utils.GetIcon("CalDayIcon" + DateTime.Today.Day.ToString());

    public override string IconText => DateTime.Now.Day.ToString();

    public override DateTime? DefaultDate => new DateTime?(DateTime.Today);

    public override int SortOrder
    {
      get => LocalSettings.Settings.SortOrderOfToday;
      set => LocalSettings.Settings.SortOrderOfToday = value;
    }

    public override string UserEventId => "today";
  }
}
