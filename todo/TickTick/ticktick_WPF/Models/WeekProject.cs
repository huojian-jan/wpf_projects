// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.WeekProject
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
  public class WeekProject : SmartProject
  {
    public override string Id => "_special_id_week";

    public override string UserEventId => "n7d";

    public override string Name => Utils.GetString("Next7Day");

    public override Geometry Icon
    {
      get => Utils.GetIcon("CalWeekIcon" + DateTime.Today.DayOfWeek.ToString().Substring(0, 2));
    }

    public override string IconText => DateTime.Now.DayOfWeek.ToString().Substring(0, 2);

    public override DateTime? DefaultDate => new DateTime?(DateTime.Today);

    public override int SortOrder
    {
      get => LocalSettings.Settings.SortOrderOfWeek;
      set => LocalSettings.Settings.SortOrderOfWeek = value;
    }
  }
}
