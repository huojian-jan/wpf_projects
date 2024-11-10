// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.AvatarComboBoxViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media.Imaging;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class AvatarComboBoxViewModel : AvatarViewModel
  {
    public new string AvatarUrl
    {
      set => this.SetAvatar(value);
    }

    private async void SetAvatar(string avatarUrl)
    {
      AvatarComboBoxViewModel comboBoxViewModel = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(avatarUrl);
      comboBoxViewModel.Avatar = avatarByUrlAsync;
      comboBoxViewModel.OnPropertyChanged("Avatar");
    }

    public new string Name { get; set; }

    public new string UserId { get; set; }

    public new string UserCode { get; set; }

    public BitmapImage Avatar { get; set; }
  }
}
