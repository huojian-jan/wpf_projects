// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.NewUser.GuideChooseViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Media;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.NewUser
{
  public class GuideChooseViewModel : BaseViewModel
  {
    private bool _selected;

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

    public string Name { get; set; }

    public string Emoji { get; set; }

    public Geometry Icon { get; set; }

    public Visibility ShowIcon { get; set; } = Visibility.Collapsed;

    public Visibility ShowEmoji { get; set; } = Visibility.Collapsed;

    public GuideChooseViewModel(string name, Geometry icon)
    {
      this.Name = name;
      this.Icon = icon;
      this.ShowIcon = Visibility.Visible;
    }

    public GuideChooseViewModel(string name, string emoji)
    {
      this.Name = name;
      this.Emoji = emoji;
      this.ShowEmoji = Visibility.Visible;
    }
  }
}
