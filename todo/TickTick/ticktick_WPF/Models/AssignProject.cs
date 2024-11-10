// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.AssignProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class AssignProject : SmartProject
  {
    public override string Id => "_special_id_assigned";

    public override string UserEventId => "assign_to_me";

    public override string Name => Utils.GetString("AssignToMe");

    public override Geometry Icon => Utils.GetIcon("IcAssignToMe");

    public override bool CanDrop => false;

    public override int SortOrder
    {
      get => LocalSettings.Settings.SortOrderOfAssign;
      set => LocalSettings.Settings.SortOrderOfAssign = value;
    }
  }
}
