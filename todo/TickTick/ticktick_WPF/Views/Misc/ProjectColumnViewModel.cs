// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ProjectColumnViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class ProjectColumnViewModel : SelectableItemViewModel
  {
    public ProjectColumnViewModel(ColumnModel column)
    {
      this.Title = column.name;
      this.Icon = Utils.GetIconData("IcGroupByCustom");
      this.Id = column.id;
      this.ParentId = column.projectId;
      this.IsSubItem = true;
      this.Type = "normal";
    }
  }
}
