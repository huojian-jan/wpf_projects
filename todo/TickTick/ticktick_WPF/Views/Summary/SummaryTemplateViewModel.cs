// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Summary.SummaryTemplateViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Summary
{
  public class SummaryTemplateViewModel : BaseViewModel
  {
    private bool _isSelected;

    public string Id { get; set; }

    public string Name { get; set; }

    public bool IsDefault => this.Id == "defaultId";

    public bool IsSelected
    {
      get => this._isSelected;
      set
      {
        this._isSelected = value;
        this.OnPropertyChanged(nameof (IsSelected));
      }
    }
  }
}
