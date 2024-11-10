// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.AddTaskAttachmentInfo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Converter;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Files;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class AddTaskAttachmentInfo : BaseViewModel
  {
    public string Path { get; set; }

    public ImageSource Image { get; set; }

    public AddTaskAttachmentInfo(string path)
    {
      this.Path = path;
      string str = AttachmentProvider.GetFileType(System.IO.Path.GetFileName(path)).ToString();
      BitmapImage bitmapImage;
      if (!(str == "IMAGE"))
        bitmapImage = new BitmapImage(new Uri((string) (new AttachmentIconConverter().Convert((object) str, (Type) null, (object) new AttachmentConvertParam()
        {
          IsDark = ThemeUtil.IsDark(),
          IsFailed = false
        }, (CultureInfo) null) ?? (object) string.Empty)));
      else
        bitmapImage = ImageUtils.GetImageByUrl(path, 64, BitmapCacheOption.OnLoad);
      this.Image = (ImageSource) bitmapImage;
    }

    public void OnAttachmentClick()
    {
      if (FileUtils.FileEmptyOrNotExists(this.Path))
        return;
      try
      {
        Process.Start(this.Path);
      }
      catch (Exception ex)
      {
      }
    }
  }
}
