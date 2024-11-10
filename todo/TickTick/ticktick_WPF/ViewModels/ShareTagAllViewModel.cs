// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ShareTagAllViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class ShareTagAllViewModel : PtfAllViewModel
  {
    public ShareTagAllViewModel()
      : base(PtfType.Tag)
    {
      this.Title = Utils.GetString("SharedTags");
      this.Open = LocalSettings.Settings.AllShareTagOpened;
      this.InSubSection = true;
      this.Id = "ShareTag";
    }
  }
}
