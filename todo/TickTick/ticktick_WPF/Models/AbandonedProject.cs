// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.AbandonedProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class AbandonedProject : SmartProject
  {
    public override string Id => "_special_id_abandoned";

    public override string UserEventId => "abandoned";

    public override string Name => Utils.GetString("Abandoned");

    public override Geometry Icon => Utils.GetIcon("IcAbandonedProject");

    public override bool IsCompleted => true;

    public override int SortOrder => 2147483637;
  }
}
