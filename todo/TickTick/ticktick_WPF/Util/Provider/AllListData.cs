// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.AllListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class AllListData : SortProjectData
  {
    public AllListData()
    {
      this.EmptyContent = Utils.GetString("AllB");
      this.AddTaskHint = Utils.GetString("CenterAddTaskTextBoxPreviewTextInbox");
      this.TitleInProjectGroup = Utils.GetString("Inbox");
      this.ShowProjectSort = true;
      this.ShowLoadMore = true;
    }

    public override DrawingImage GetEmptyImage()
    {
      return LocalSettings.Settings.HideComplete ? Application.Current?.FindResource((object) "EmptyProjectDrawingImage") as DrawingImage : Application.Current?.FindResource((object) "EmptyAllDrawingImage") as DrawingImage;
    }

    public override Geometry GetEmptyPath()
    {
      return !LocalSettings.Settings.HideComplete ? Utils.GetIconData("IcEmptyCompleted") : Utils.GetIconData("IcEmptyProject");
    }

    public override async Task<string> GetEmptyTitle()
    {
      return !LocalSettings.Settings.HideComplete ? Utils.GetString("AllA1") : Utils.GetString("AllA2");
    }

    public override string GetTitle() => Utils.GetString("All");

    public override void SaveSortOption(SortOption sortOption)
    {
      SortProjectData.SaveSpecialProjectSortType("SortTypeOfAllProject", sortOption);
      LocalSettings.Settings.SaveSmartProjectOptions("all", sortOption);
      DataChangedNotifier.NotifySortOptionChanged("all");
    }
  }
}
