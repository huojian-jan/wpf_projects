// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.HabitSectionListViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class HabitSectionListViewModel : HabitItemBaseViewModel
  {
    public HabitSectionModel Section;
    public readonly List<HabitItemViewModel> Children = new List<HabitItemViewModel>();
    public long SortOrder;
    private string _title;

    public HabitSectionListViewModel(HabitSectionModel section)
    {
      this.Title = section.DisplayName;
      this.Section = section;
      HabitSectionModel section1 = this.Section;
      this.SortOrder = section1 != null ? section1.SortOrder : 0L;
      this.IsSection = true;
    }

    public override string Id => this.Section?.Id;

    public bool IsOpen
    {
      get
      {
        HabitSectionModel section = this.Section;
        return section == null || section.IsOpen;
      }
      set
      {
        if (this.Section != null)
          this.Section.IsOpen = value;
        this.OnPropertyChanged(nameof (IsOpen));
      }
    }

    public HabitSectionListViewModel()
    {
    }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public bool IsOther { get; set; }

    public bool IsAdd { get; set; }

    public int Num { get; set; }
  }
}
