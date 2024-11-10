// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.CustomizedSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class CustomizedSection : Section
  {
    private bool _canSort;
    public bool CanDelete = true;

    public CustomizedSection(bool canSort = true)
      : base()
    {
      this._canSort = canSort;
    }

    public CustomizedSection(ColumnModel column, bool canSort = true)
      : base()
    {
      this.Name = column.name;
      this.SectionEntityId = column.id;
      this.SectionId = column.id;
      this.Ordinal = column.sortOrder.GetValueOrDefault();
      this.ProjectId = column.projectId;
      this.Customized = true;
      this._canSort = canSort;
    }

    public override bool CanSwitch(DisplayType displayType) => this._canSort;

    public override bool CanSort(string sortBy) => this._canSort;
  }
}
