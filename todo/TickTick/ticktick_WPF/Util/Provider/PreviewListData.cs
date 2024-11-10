// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.PreviewListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class PreviewListData : SortProjectData
  {
    public PreviewListData(FilterPreviewIdentity filter)
    {
      this.ShowCustomSort = false;
      this.AddTaskHint = string.Empty;
      this.ShowLoadMore = false;
      this.EmptyTitle = Utils.GetString("SearchA");
      this.EmptyContent = Utils.GetString("SearchB");
      this.EmptyPath = Utils.GetIconData("IcSearchResult");
      this.ShowShare = false;
    }

    public override string GetTitle() => string.Empty;

    public override DrawingImage GetEmptyImage()
    {
      return Application.Current?.FindResource((object) "EmptySearchDrawingImage") as DrawingImage;
    }
  }
}
