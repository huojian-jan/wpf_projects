// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.SelectedTagViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class SelectedTagViewModel : BaseViewModel
  {
    private string _tag;
    private bool _isAdd;

    public string Tag
    {
      get => this._tag;
      set
      {
        this._tag = value;
        this.OnPropertyChanged(nameof (Tag));
      }
    }

    public bool IsAdd
    {
      get => this._isAdd;
      set
      {
        this._isAdd = value;
        this.OnPropertyChanged(nameof (IsAdd));
      }
    }
  }
}
