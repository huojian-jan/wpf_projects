// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.SearchProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public sealed class SearchProjectIdentity : ProjectIdentity
  {
    public SearchProjectIdentity()
    {
      this.SortOption = new SortOption()
      {
        groupBy = "none",
        orderBy = "none"
      };
      this.CanDrag = false;
    }

    public override string Id => "_special_id_search";

    public override bool LoadAll => true;

    public override string SortProjectId => (string) null;
  }
}
