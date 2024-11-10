// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.SortOptionUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util
{
  public class SortOptionUtils
  {
    public static SortOption GetSortOptionBySortType(Constants.SortType sortType, bool inKanban)
    {
      SortOption optionBySortType = new SortOption();
      if (inKanban)
      {
        optionBySortType.groupBy = "sortOrder";
        optionBySortType.orderBy = sortType.ToString();
      }
      else if (sortType == Constants.SortType.title || sortType == Constants.SortType.createdTime || sortType == Constants.SortType.modifiedTime)
      {
        optionBySortType.groupBy = "none";
        optionBySortType.orderBy = sortType.ToString();
      }
      else
      {
        optionBySortType.groupBy = sortType.ToString();
        optionBySortType.orderBy = sortType == Constants.SortType.sortOrder ? sortType.ToString() : "dueDate";
      }
      return optionBySortType;
    }
  }
}
