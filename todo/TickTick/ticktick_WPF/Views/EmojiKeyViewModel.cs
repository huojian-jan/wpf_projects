// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.EmojiKeyViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views
{
  public class EmojiKeyViewModel : BaseViewModel
  {
    public string Key { get; set; }

    public string Text { get; set; }

    private bool _isSelected { get; set; }

    public bool IsSelected
    {
      get => this._isSelected;
      set
      {
        this._isSelected = value;
        this.OnPropertyChanged(nameof (IsSelected));
      }
    }

    public string EmojiPathName { get; set; }
  }
}
