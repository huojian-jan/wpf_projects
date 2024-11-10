// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.ClosedProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public sealed class ClosedProjectIdentity : ProjectIdentity
  {
    public ClosedProjectIdentity(string projectId) => this.ProjectId = projectId;

    private string ProjectId { get; }

    public override string QueryId => this.ProjectId;

    public override string GetDisplayTitle() => Utils.GetString("TheListClosed");
  }
}
