// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitItemDisplayModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.ComponentModel;
using System.Runtime.CompilerServices;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitItemDisplayModel : INotifyPropertyChanged
  {
    public static readonly HabitItemDisplayModel DisplayModel = new HabitItemDisplayModel();
    private bool _showCurrentStreak;

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
      PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
      if (propertyChanged == null)
        return;
      propertyChanged((object) this, new PropertyChangedEventArgs(propertyName));
    }

    public bool ShowCurrentStreak
    {
      get => this._showCurrentStreak;
      set
      {
        this._showCurrentStreak = value;
        this.OnPropertyChanged(nameof (ShowCurrentStreak));
      }
    }
  }
}
