// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.HabitItemBaseViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class HabitItemBaseViewModel : BaseHidableViewModel
  {
    private bool _selected;
    private long _sortOrder;
    private bool _dragging;

    public virtual string Id { get; set; }

    public virtual bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public virtual bool Archived { get; set; }

    public virtual long SortOrder
    {
      get => this._sortOrder;
      set
      {
        this._sortOrder = value;
        this.OnPropertyChanged(nameof (SortOrder));
      }
    }

    public bool Dragging
    {
      get => this._dragging;
      set
      {
        this._dragging = value;
        this.OnPropertyChanged(nameof (Dragging));
      }
    }

    public bool Enable { get; set; } = true;

    public bool IsSection { get; set; }

    public HabitItemBaseViewModel Clone() => (HabitItemBaseViewModel) this.MemberwiseClone();
  }
}
