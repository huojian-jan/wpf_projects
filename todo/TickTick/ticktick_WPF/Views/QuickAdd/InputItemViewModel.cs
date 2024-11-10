// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.InputItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media.Imaging;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class InputItemViewModel : BaseViewModel
  {
    private string _imageUrl;
    private bool _selected;
    private bool _highLightSelected;

    public string Title { get; set; }

    public string Value { get; set; }

    public string Pinyin { get; set; }

    public string Inits { get; set; }

    public BitmapImage Avatar { get; set; }

    private async void SetAvatar()
    {
      InputItemViewModel inputItemViewModel = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(inputItemViewModel.ImageUrl);
      inputItemViewModel.Avatar = avatarByUrlAsync;
      inputItemViewModel.OnPropertyChanged("Avatar");
    }

    public string ImageUrl
    {
      get => this._imageUrl;
      set
      {
        this._imageUrl = value;
        this.SetAvatar();
      }
    }

    public bool IsNoAvatar { get; set; }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public bool HighLightSelected
    {
      get => this._highLightSelected;
      set
      {
        this._highLightSelected = value;
        this.OnPropertyChanged(nameof (HighLightSelected));
      }
    }
  }
}
