// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TrashListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class TrashListData : SortProjectData
  {
    public TrashListData()
    {
      this.EmptyTitle = Utils.GetString("TrashA");
      this.EmptyContent = Utils.GetString("TrashB");
      this.EmptyPath = Utils.GetIconData("IcEmptyTrash");
      this.AddTaskHint = string.Empty;
      this.IsCompleted = false;
      this.IsTrash = true;
    }

    public override Thickness GetEmptyMargin() => new Thickness(118.0, 44.0, 0.0, 0.0);

    public override string GetTitle() => Utils.GetString("Trash");

    public override DrawingImage GetEmptyImage()
    {
      return Application.Current?.FindResource((object) "EmptyTrashDrawingImage") as DrawingImage;
    }
  }
}
