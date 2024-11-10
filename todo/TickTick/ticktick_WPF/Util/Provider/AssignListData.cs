// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.AssignListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class AssignListData : SortProjectData
  {
    public AssignListData()
    {
      this.EmptyTitle = Utils.GetString("AssignToMeA");
      this.EmptyContent = Utils.GetString("AssignToMeB");
      this.EmptyPath = Utils.GetIconData("IcAssignToMeEmpty");
      this.AddTaskHint = string.Empty;
      this.ShowProjectSort = true;
    }

    public override string GetTitle() => Utils.GetString("AssignToMe");

    public override void SaveSortOption(SortOption sortOption)
    {
      SortProjectData.SaveSpecialProjectSortType("SortTypeOfAssignMe", sortOption);
      LocalSettings.Settings.SaveSmartProjectOptions("assignToMe", sortOption);
      DataChangedNotifier.NotifySortOptionChanged("assigned");
    }

    public override DrawingImage GetEmptyImage()
    {
      return Application.Current?.FindResource((object) "EmptyAssignToMeDrawingImage") as DrawingImage;
    }
  }
}
