// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.StickyColorViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class StickyColorViewModel : BaseViewModel
  {
    private static readonly Dictionary<StickyColorKey, string> BackColorDict = new Dictionary<StickyColorKey, string>()
    {
      {
        StickyColorKey.Default,
        "#FFF2AA"
      },
      {
        StickyColorKey.Apricot,
        "#FFD4C4"
      },
      {
        StickyColorKey.Watermelon,
        "#FFADAD"
      },
      {
        StickyColorKey.LakeBlue,
        "#CAF1FF"
      },
      {
        StickyColorKey.Blue,
        "#C4D4FF"
      },
      {
        StickyColorKey.Purple,
        "#D4C9FC"
      },
      {
        StickyColorKey.BlueGreen,
        "#AFEBBB"
      },
      {
        StickyColorKey.Gray,
        "#EEEEEE"
      },
      {
        StickyColorKey.Dark,
        "#1F2126"
      },
      {
        StickyColorKey.DarkBlue,
        "#152240"
      }
    };
    private static readonly Dictionary<StickyColorKey, string> TopColorDict = new Dictionary<StickyColorKey, string>()
    {
      {
        StickyColorKey.Default,
        "#FFED92"
      },
      {
        StickyColorKey.Apricot,
        "#FFCEBE"
      },
      {
        StickyColorKey.Watermelon,
        "#FFA5A5"
      },
      {
        StickyColorKey.LakeBlue,
        "#BEEDFF"
      },
      {
        StickyColorKey.Blue,
        "#BBCEFF"
      },
      {
        StickyColorKey.Purple,
        "#D0C3FF"
      },
      {
        StickyColorKey.BlueGreen,
        "#A0E7AF"
      },
      {
        StickyColorKey.Gray,
        "#E8E8E8"
      },
      {
        StickyColorKey.Dark,
        "#1A1C20"
      },
      {
        StickyColorKey.DarkBlue,
        "#131F3B"
      }
    };
    private bool _selected;

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public string Key { get; set; }

    public string Tooltip { get; set; }

    public bool NeedPro { get; set; }

    public SolidColorBrush BorderColor { get; set; }

    public SolidColorBrush FillColor { get; set; }

    public Geometry Icon { get; set; }

    public StickyColorViewModel(StickyColorKey key)
    {
      this.Key = key.ToString();
      this.NeedPro = key != 0;
      this.FillColor = StickyColorViewModel.GetBackColor(key);
      this.BorderColor = StickyColorViewModel.GetTopColor(key);
      if (key != StickyColorKey.Random)
        return;
      this.FillColor = Brushes.Transparent;
      this.BorderColor = ThemeUtil.GetColor("BaseColorOpacity10");
      this.Icon = Utils.GetIcon("RandomIcon");
      this.Tooltip = Utils.GetString("Random");
    }

    public static List<StickyColorViewModel> GetColorModels()
    {
      return Enum.GetValues(typeof (StickyColorKey)).Cast<StickyColorKey>().Select<StickyColorKey, StickyColorViewModel>((Func<StickyColorKey, StickyColorViewModel>) (e => new StickyColorViewModel(e))).ToList<StickyColorViewModel>();
    }

    public static SolidColorBrush GetBackColor(StickyColorKey key)
    {
      return StickyColorViewModel.BackColorDict.ContainsKey(key) ? ThemeUtil.GetColorInString(StickyColorViewModel.BackColorDict[key]) : ThemeUtil.GetColorInString("#FFF2AA");
    }

    public static SolidColorBrush GetTopColor(StickyColorKey key)
    {
      return StickyColorViewModel.TopColorDict.ContainsKey(key) ? ThemeUtil.GetColorInString(StickyColorViewModel.TopColorDict[key]) : ThemeUtil.GetColorInString("#FFF2AA");
    }
  }
}
