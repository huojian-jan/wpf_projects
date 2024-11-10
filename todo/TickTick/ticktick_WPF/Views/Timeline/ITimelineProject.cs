// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.ITimelineProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  internal interface ITimelineProject
  {
    TimelineModel GetTimelineModel();

    void CommitTimeline(TimelineModel model);

    List<SortTypeViewModel> GetTimelineSortTypes();
  }
}
