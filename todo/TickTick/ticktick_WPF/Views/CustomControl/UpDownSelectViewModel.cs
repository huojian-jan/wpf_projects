// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomControl.UpDownSelectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.CustomControl
{
  public class UpDownSelectViewModel : BaseViewModel
  {
    private bool _selected;
    private bool _hoverSelected;

    public bool Selectable { get; set; } = true;

    public bool Selected
    {
      get => this.ShowSelected && this._selected;
      set
      {
        if (this._selected == value)
          return;
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
        this.OnSelectedChanged();
      }
    }

    public bool HoverSelected
    {
      get => this._hoverSelected;
      set
      {
        if (this._hoverSelected == value)
          return;
        this._hoverSelected = value;
        this.OnPropertyChanged(nameof (HoverSelected));
      }
    }

    public bool SubOpened { get; set; }

    public virtual bool HasChildren { get; set; }

    public bool IsEnable { get; set; } = true;

    public bool ShowSelected { get; set; } = true;

    public virtual void OnSelectedChanged()
    {
    }
  }
}
