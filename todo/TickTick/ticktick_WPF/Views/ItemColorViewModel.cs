// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.ItemColorViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views
{
  public class ItemColorViewModel : BaseViewModel
  {
    private string _color;
    private bool _selected;

    public bool IsAddCustom { get; set; }

    public bool NoColor { get; set; }

    public string Color
    {
      get => this._color;
      set
      {
        this._color = value;
        this.OnPropertyChanged(nameof (Color));
      }
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        if (this._selected == value)
          return;
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }
  }
}
