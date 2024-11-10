// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineThemes
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineThemes : BaseViewModel
  {
    static TimelineThemes()
    {
      SolidColorBrush solidColorBrush = new SolidColorBrush(Colors.White);
      solidColorBrush.Opacity = 0.1;
      TimelineThemes.BackgroundBrush = solidColorBrush;
    }

    public static SolidColorBrush GridBrush => ThemeUtil.GetColor("BaseColorOpacity5");

    public static SolidColorBrush WeekendBrush => ThemeUtil.GetColor("BaseColorOpacity2");

    public static SolidColorBrush BackgroundBrush { get; private set; }

    public static Color DayLineColor => ThemeUtil.GetColorValue("TimelineDayLineColor");

    public static SolidColorBrush WeekendDayBrush => ThemeUtil.GetColor("BaseColorOpacity40_20");

    public static SolidColorBrush NormalDayBrush => ThemeUtil.GetColor("BaseColorOpacity80");

    public static SolidColorBrush PrimaryColorBrush => ThemeUtil.GetColor("PrimaryColor");

    public static SolidColorBrush ContainerBackgroundBrush
    {
      get => ThemeUtil.GetColor("TimelineContainerBackground");
    }
  }
}
