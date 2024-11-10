// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.CarouselItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class CarouselItemViewModel : BaseViewModel
  {
    private string _title;
    private string _subTitle;
    private bool _selected;
    public string ImageUrl = "pack://application:,,,/Assets/LoginImage/LoginTask.png";

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public string SubTitle
    {
      get => this._subTitle;
      set
      {
        this._subTitle = value;
        this.OnPropertyChanged(nameof (SubTitle));
      }
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public static List<CarouselItemViewModel> GetLoginCarouselModels()
    {
      return new List<CarouselItemViewModel>()
      {
        new CarouselItemViewModel()
        {
          _title = Utils.GetString("LoginTitleTask"),
          _subTitle = Utils.GetString("LoginSubtitleTask"),
          ImageUrl = "pack://application:,,,/Assets/LoginImage/LoginTask.png"
        },
        new CarouselItemViewModel()
        {
          _title = Utils.GetString("LoginTitleCal"),
          _subTitle = Utils.GetString("LoginSubtitleCal"),
          ImageUrl = "pack://application:,,,/Assets/LoginImage/LoginDate.png"
        },
        new CarouselItemViewModel()
        {
          _title = Utils.GetString("LoginTitlePomo"),
          _subTitle = Utils.GetString("LoginSubtitlePomo"),
          ImageUrl = "pack://application:,,,/Assets/LoginImage/LoginPomo.png"
        },
        new CarouselItemViewModel()
        {
          _title = Utils.GetString("LoginTitleHabit"),
          _subTitle = Utils.GetString("LoginSubtitleHabit"),
          ImageUrl = "pack://application:,,,/Assets/LoginImage/LoginHabit.png"
        }
      };
    }
  }
}
