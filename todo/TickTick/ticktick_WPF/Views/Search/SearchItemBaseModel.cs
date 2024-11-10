// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchItemBaseModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchItemBaseModel : BaseViewModel
  {
    private bool _selected;

    public virtual string Id { get; set; }

    public virtual bool CanSelect { get; set; }

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
