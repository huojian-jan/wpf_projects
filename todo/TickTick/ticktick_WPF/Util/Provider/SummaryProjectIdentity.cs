// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.SummaryProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class SummaryProjectIdentity : SmartProjectIdentity
  {
    public override bool CanEdit { get; }

    public override string Id => "_special_id_summary";

    public override string SortProjectId => (string) null;

    public override bool CanAddTask() => false;

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return (ProjectIdentity) new SummaryProjectIdentity();
    }

    public override string GetDisplayTitle() => Utils.GetString("Summary");
  }
}
