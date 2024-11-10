// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TrashProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class TrashProjectIdentity : SmartProjectIdentity
  {
    public override bool CanEdit { get; }

    public override string Id => "_special_id_trash";

    public override string SortProjectId => (string) null;

    public bool IsPerson { get; set; } = true;

    public TrashProjectIdentity() => this.SortOption = new SortOption();

    public override bool CanAddTask() => false;

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return (ProjectIdentity) new TrashProjectIdentity();
    }

    public override string GetDisplayTitle() => Utils.GetString("Trash");
  }
}
