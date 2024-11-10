// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.CustomThemeViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class CustomThemeViewModel : ThemeBaseModel
  {
    private ImageSource _image;
    private Rect _viewBox = new Rect(0.0, 0.0, 1.0, 1.0);

    public bool IsEmpty => this.Image == null;

    public ImageSource Image
    {
      get => this._image;
      set
      {
        this._image = value;
        this.OnPropertyChanged(nameof (Image));
        this.OnPropertyChanged("IsEmpty");
      }
    }

    public Rect ViewBox
    {
      get => this._viewBox;
      set
      {
        this._viewBox = value;
        this.OnPropertyChanged(nameof (ViewBox));
      }
    }

    public CustomThemeViewModel()
    {
      this.Key = "Custom";
      this.Selected = "Custom" == LocalSettings.Settings.ThemeId;
      this.Name = Utils.GetString("Custom");
      if (!Directory.Exists(AppPaths.ThemeDir))
        Directory.CreateDirectory(AppPaths.ThemeDir);
      this.SetImage();
    }

    public void SetImage()
    {
      if (!UserDao.IsPro())
      {
        this.Image = (ImageSource) null;
      }
      else
      {
        List<string> list = ((IEnumerable<string>) Directory.GetFiles(AppPaths.ThemeDir, "custom*.*")).ToList<string>();
        this._viewBox = Utils.GetRectByString(LocalSettings.Settings.Common.CustomThemeLocation);
        if (this._viewBox.Width == 0.0 || this._viewBox.Height == 0.0)
          this._viewBox = new Rect(0.0, 0.0, 1.0, 1.0);
        this.OnPropertyChanged("ViewBox");
        if (list.Count <= 0 || new FileInfo(list[0]).Length <= 0L)
          return;
        this.Image = (ImageSource) Utils.LoadBitmap(list[0], 320, 200);
      }
    }
  }
}
