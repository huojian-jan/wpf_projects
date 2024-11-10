// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.Section
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Dal;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class Section
  {
    public const string OutdatedId = "outdated";
    public static Section NoAssignee = new Section(Utils.GetString("NotAssigned"), -2147483647, new DateTime?(), "noassignee", "-1");
    private bool _canSort;
    private bool _canSwitch;

    public Section(bool canSort = false, bool canSwitch = false)
    {
      this._canSort = canSort;
      this._canSwitch = canSwitch;
    }

    public Section(
      string name,
      int oridinal,
      DateTime? sectionDate,
      string sectionId,
      string sectionUserId = "",
      bool canSort = false,
      bool canSwitch = false)
    {
      this.Name = name;
      this.Ordinal = (long) oridinal;
      this.SectionDate = new DateTime?(!sectionDate.HasValue || Utils.IsEmptyDate(sectionDate) ? new DateTime() : sectionDate.Value);
      this.SectionId = sectionId;
      this.SectionUserId = sectionUserId;
      this._canSort = canSort;
      this._canSwitch = canSwitch;
    }

    public string Name { get; set; }

    public long Ordinal { get; set; }

    public int Count { get; set; }

    public DateTime? SectionDate { get; set; }

    public string SectionId { get; set; }

    public string SectionUserId { get; set; }

    public bool Customized { get; set; }

    public string ProjectId { get; set; }

    public virtual bool CanSwitch(DisplayType displayType) => this._canSwitch;

    public virtual bool CanSort(string orderBy) => this._canSort;

    public List<DisplayItemModel> Children { get; set; } = new List<DisplayItemModel>();

    public string SectionEntityId { get; set; }

    public virtual int GetPriority() => TaskDefaultDao.GetDefaultSafely().Priority;

    public virtual string GetProjectId() => string.Empty;

    public DateTime? GetStartDate() => this.SectionDate;

    public virtual int GetTaskStatus() => 0;

    public virtual string GetAssignee() => string.Empty;

    public virtual string GetTag() => string.Empty;
  }
}
