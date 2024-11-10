// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitIconViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitIconViewModel : BaseViewModel
  {
    private bool _selected;

    public string IconName { get; set; }

    public string IconUrl { get; set; }

    public string Color { get; set; }

    public string DescText { get; set; }

    public HabitIconViewModel()
    {
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public HabitIconViewModel(string iconRes)
    {
      this.IconName = iconRes;
      this.IconUrl = "../../Assets/Habits/" + iconRes.ToLower() + ".png";
      this.DescText = HabitUtils.GetHabitHintText(iconRes);
      this.Color = HabitUtils.IconColorDict[iconRes];
    }

    public static List<HabitIconViewModel> BuildItems()
    {
      List<HabitIconViewModel> items = new List<HabitIconViewModel>();
      HabitUtils.HabitIcons.ForEach((Action<string>) (icon =>
      {
        if (!(icon != "habit_control_temper"))
          return;
        items.Add(new HabitIconViewModel(icon));
      }));
      return items;
    }
  }
}
