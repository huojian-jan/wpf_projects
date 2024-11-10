// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.SortableListItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class SortableListItemViewModel : BaseViewModel
  {
    private bool _open;
    private bool _dragging;

    public bool IsSection { get; set; }

    public string Id { get; set; }

    public string SectionId { get; set; }

    public long SortOrder { get; set; }

    public bool Dragging
    {
      get => this._dragging;
      set
      {
        this._dragging = value;
        this.OnPropertyChanged(nameof (Dragging));
      }
    }

    public bool IsOpen
    {
      get => this._open;
      set
      {
        this._open = value;
        this.OnPropertyChanged(nameof (IsOpen));
      }
    }

    public virtual void SaveSortOrder()
    {
    }
  }
}
