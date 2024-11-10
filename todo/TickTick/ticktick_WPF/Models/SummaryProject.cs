// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.SummaryProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class SummaryProject : SmartProject
  {
    public override string Id => "_special_id_summary";

    public override string UserEventId => "summary";

    public override string Name => Utils.GetString("Summary");

    public override Geometry Icon => Utils.GetIcon("IcSummaryProject");

    public override bool CanDrop => false;

    public override int SortOrder
    {
      get => LocalSettings.Settings.SortOrderOfSummary;
      set => LocalSettings.Settings.SortOrderOfSummary = value;
    }
  }
}
