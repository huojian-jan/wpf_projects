// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.CompleteListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class CompleteListData : SortProjectData
  {
    public CompleteListData()
    {
      this.EmptyTitle = Utils.GetString("CompletedA");
      this.EmptyContent = Utils.GetString("CompletedB");
      this.EmptyPath = Utils.GetIconData("IcEmptyCompleted");
      this.AddTaskHint = string.Empty;
      this.IsCompleted = true;
    }

    public override string GetTitle() => Utils.GetString("Completed");

    public override DrawingImage GetEmptyImage()
    {
      return Application.Current?.FindResource((object) "EmptyCompletedDrawingImage") as DrawingImage;
    }
  }
}
