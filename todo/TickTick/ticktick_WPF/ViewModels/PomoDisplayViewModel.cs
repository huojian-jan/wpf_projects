// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.PomoDisplayViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class PomoDisplayViewModel : BaseViewModel
  {
    public string Id { get; set; }

    public string TimerId { get; set; }

    public bool Enable { get; set; }

    public string Note { get; set; }

    public List<PomoTaskDisplayViewModel> PomoTaskModels { get; set; }

    public PomoDisplayViewModel(PomodoroModel pomo, PomoTask[] pomoTasks)
    {
      this.Id = pomo.Id;
      this.Enable = (DateTime.Today - pomo.StartTime.Date).TotalDays <= 30.0;
      this.PomoTaskModels = new List<PomoTaskDisplayViewModel>();
      this.Note = pomo.Note;
      if (pomoTasks != null && ((IEnumerable<PomoTask>) pomoTasks).Any<PomoTask>())
      {
        foreach (PomoTask pomoTask in pomoTasks)
          this.PomoTaskModels.Add(new PomoTaskDisplayViewModel(pomoTask, pomo)
          {
            Enable = this.Enable
          });
      }
      else
      {
        long duration = Utils.GetTotalSecond(pomo.StartTime, pomo.EndTime) - pomo.PauseDuration;
        List<PomoTaskDisplayViewModel> pomoTaskModels = this.PomoTaskModels;
        PomoTaskDisplayViewModel displayViewModel = new PomoTaskDisplayViewModel();
        displayViewModel.PomoId = pomo.Id;
        displayViewModel.StartTime = pomo.StartTime;
        displayViewModel.Duration = duration;
        displayViewModel.Enable = this.Enable;
        displayViewModel.DurationString = Utils.GetDurationString(duration, true, true, false);
        DateTime dateTime = pomo.StartTime;
        string str1 = dateTime.ToString("yyyy'/'MM'/'dd HH':'mm");
        dateTime = pomo.StartTime;
        dateTime = dateTime.AddSeconds((double) duration);
        string str2 = dateTime.ToString("yyyy'/'MM'/'dd HH':'mm");
        displayViewModel.TimeString = str1 + " - " + str2;
        displayViewModel.Icon = Utils.GetIcon(pomo.Type == 0 ? "IcPomo" : "IcPomoTimer");
        pomoTaskModels.Add(displayViewModel);
      }
    }
  }
}
