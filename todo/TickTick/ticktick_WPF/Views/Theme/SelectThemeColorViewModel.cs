// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.SelectThemeColorViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class SelectThemeColorViewModel : BaseViewModel
  {
    private string _color;
    private bool _selected;

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
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }
  }
}
