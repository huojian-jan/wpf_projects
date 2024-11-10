// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.GroupTitleViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class GroupTitleViewModel : BaseViewModel
  {
    private string _title;
    public object _tag;
    private string _content = string.Empty;
    private bool _isSelected;
    private bool _isTabSelected;
    private double _maxWidth = 1000.0;

    public int Index { get; set; }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public object Tag
    {
      get => this._tag;
      set
      {
        this._tag = value;
        this.OnPropertyChanged(nameof (Tag));
      }
    }

    public string Content
    {
      get => this._content;
      set
      {
        this._content = value;
        this.OnPropertyChanged(nameof (Content));
      }
    }

    public bool IsSelected
    {
      get => this._isSelected;
      set
      {
        this._isSelected = value;
        this.OnPropertyChanged(nameof (IsSelected));
      }
    }

    public bool IsTabSelected
    {
      get => this._isTabSelected;
      set
      {
        this._isTabSelected = value;
        this.OnPropertyChanged(nameof (IsTabSelected));
      }
    }

    public double MaxWidth
    {
      get => this._maxWidth;
      set
      {
        this._maxWidth = value;
        this.OnPropertyChanged(nameof (MaxWidth));
      }
    }
  }
}
