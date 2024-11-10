// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.ListItemSelectModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class ListItemSelectModel
  {
    public string Id;
    public string ChildId;
    public DisplayType ItemType;
    public TaskSelectType Type;

    public ListItemSelectModel(
      string id,
      string childId,
      DisplayType itemType,
      TaskSelectType type)
    {
      this.Id = id;
      this.ChildId = childId;
      this.ItemType = itemType;
      this.Type = type;
    }
  }
}
