// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.AllProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class AllProject : SmartProject
  {
    public override string Id => "_special_id_all";

    public override string Name => Utils.GetString("All");

    public override Geometry Icon => Utils.GetIcon("IcAllProject");

    public override string UserEventId => "all";

    public override int SortOrder
    {
      get => LocalSettings.Settings.SortOrderOfAll;
      set => LocalSettings.Settings.SortOrderOfAll = value;
    }
  }
}
