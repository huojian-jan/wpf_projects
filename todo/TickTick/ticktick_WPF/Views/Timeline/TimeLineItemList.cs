// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimeLineItemList
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimeLineItemList : BlockingList<TimelineCellViewModel>
  {
    private readonly List<TimelineCellViewModel> _changedModels = new List<TimelineCellViewModel>();
    private string _uid;

    public TimeLineItemList() => this._uid = Utils.GetGuid();

    public void OnItemPosChanged(TimelineCellViewModel cellModel)
    {
      lock (this._changedModels)
        this._changedModels.Add(cellModel);
      DelayActionHandlerCenter.TryDoAction(this._uid + "NotifyTimelineItemsPosChanged", new EventHandler(this.DoNotify), 10);
    }

    private void DoNotify(object sender, EventArgs e)
    {
      Application.Current?.Dispatcher?.Invoke((Action) (() =>
      {
        List<TimelineCellViewModel> list;
        lock (this._changedModels)
        {
          list = this._changedModels.ToList<TimelineCellViewModel>();
          this._changedModels.Clear();
        }
        this.NotifyItemsChanged(list);
      }));
    }
  }
}
