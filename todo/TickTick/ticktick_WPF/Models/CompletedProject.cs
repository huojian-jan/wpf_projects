// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CompletedProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class CompletedProject : SmartProject
  {
    public override string Id => "_special_id_completed";

    public override string UserEventId => "completed";

    public override string Name => Utils.GetString("Completed");

    public override Geometry Icon => Utils.GetIcon("IcCompletedProject");

    public override bool IsCompleted => true;

    public override int SortOrder => 2147483636;
  }
}
