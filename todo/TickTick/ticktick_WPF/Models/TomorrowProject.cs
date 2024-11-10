// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TomorrowProject
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
  public class TomorrowProject : SmartProject
  {
    public override string Id => "_special_id_tomorrow";

    public override string Name => Utils.GetString("Tomorrow");

    public override string UserEventId => "tomorrow";

    public override Geometry Icon => Utils.GetIcon("IcTomorrowProject");

    public override DateTime? DefaultDate => new DateTime?(DateTime.Today.AddDays(1.0));

    public override int SortOrder
    {
      get => LocalSettings.Settings.SortOrderOfTomorrow;
      set => LocalSettings.Settings.SortOrderOfTomorrow = value;
    }
  }
}
