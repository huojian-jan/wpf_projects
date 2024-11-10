// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.SortOption
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class SortOption
  {
    public string groupBy { get; set; }

    public string orderBy { get; set; }

    public override string ToString() => "g:" + this.groupBy + ",o:" + this.orderBy;

    public string ToSmartString() => this.groupBy + "," + this.orderBy;

    public static SortOption GetOptionInSmartString(string optionString)
    {
      if (string.IsNullOrEmpty(optionString))
        return new SortOption()
        {
          groupBy = "dueDate",
          orderBy = "dueDate"
        };
      string[] strArray = optionString.Split(',');
      if (strArray.Length == 1)
        return SortOptionUtils.GetSortOptionBySortType(Utils.LoadSortType(strArray[0]), false);
      if (strArray.Length >= 2)
        return new SortOption()
        {
          groupBy = strArray[0],
          orderBy = strArray[1]
        };
      return new SortOption()
      {
        groupBy = "dueDate",
        orderBy = "dueDate"
      };
    }

    public bool Equal(SortOption b)
    {
      return b != null && this.groupBy == b.groupBy && this.orderBy == b.orderBy;
    }

    public override bool Equals(object obj)
    {
      return obj is SortOption sortOption && this.groupBy == sortOption.groupBy && this.orderBy == sortOption.orderBy;
    }

    public bool ContainsSortType(string sortType)
    {
      return this.groupBy == sortType || this.orderBy == sortType;
    }

    public string GetOldSortKey() => "taskBy#" + this.groupBy + "_" + this.orderBy;

    public string GetSortKey()
    {
      return this.orderBy == "title" || this.orderBy == "assignee" || this.orderBy == "modifiedTime" || this.orderBy == "createdTime" ? (string) null : "taskBy#" + this.groupBy + "|{0}_" + this.orderBy;
    }

    public bool IsNone()
    {
      if (!string.IsNullOrEmpty(this.groupBy) && !(this.groupBy == "none"))
        return false;
      return string.IsNullOrEmpty(this.orderBy) || this.orderBy == "none";
    }

    public SortOption Copy()
    {
      return new SortOption()
      {
        groupBy = this.groupBy,
        orderBy = this.orderBy
      };
    }

    public bool SpecialSort()
    {
      return (!(this.groupBy == "sortOrder") || !(this.orderBy == "sortOrder")) && !(this.groupBy == "none") && !(this.groupBy == "assign");
    }
  }
}
