// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Coaches.CoachCommands.TimelineCoachCommand
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Coaches.CoachCommands
{
  public class TimelineCoachCommand : ICommand
  {
    public bool CanExecute(object parameter) => true;

    public void Execute(object parameter)
    {
      UserActCollectUtils.AddClickEvent("timeline", "user_guide", "learn_more");
      if (Utils.IsDida())
        Utils.TryProcessStartUrl("https://help.dida365.com/articles/6950656549178048512");
      else
        Utils.TryProcessStartUrl("https://help.ticktick.com/articles/7055782331050622976");
    }

    public event EventHandler CanExecuteChanged;
  }
}
