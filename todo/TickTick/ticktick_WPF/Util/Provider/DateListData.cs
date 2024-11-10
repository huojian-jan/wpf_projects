// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.DateListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class DateListData : SortProjectData
  {
    public string DateStamp { get; set; }

    public DateListData(string stamp)
    {
      DateTime exact = DateTime.ParseExact(stamp, "yyyyMMdd", (IFormatProvider) null);
      this.AddTaskHint = string.Format(Utils.GetString("CenterAddTaskDateTextBoxPreviewText"), (object) DateUtils.FormatTimeDesc(exact, true, false), (object) Utils.GetString("Inbox"), (object) Utils.GetString("Task").ToLower());
      this.EmptyContent = Utils.GetString("empty_default_list_b");
      this.TitleInProjectGroup = Utils.GetString("Inbox");
      this.ShowProjectSort = false;
      this.ShowLoadMore = true;
      this.DateStamp = stamp;
    }

    public override DrawingImage GetEmptyImage()
    {
      return Application.Current?.FindResource((object) "EmptyAllDrawingImage") as DrawingImage;
    }

    public override Geometry GetEmptyPath() => Utils.GetIconData("IcEmptyProject");

    public override async Task<string> GetEmptyTitle() => Utils.GetString("no_tasks_here");

    public override string GetTitle() => new DateProjectIdentity(this.DateStamp).GetDisplayTitle();
  }
}
