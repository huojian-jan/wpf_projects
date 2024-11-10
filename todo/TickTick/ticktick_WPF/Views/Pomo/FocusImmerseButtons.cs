// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusImmerseButtons
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusImmerseButtons : FocusOptionButtons
  {
    public FocusImmerseButtons() => this.Orientation = Orientation.Horizontal;

    protected override void AddButton(string option, string textKey, bool isFill = false, bool isRest = false)
    {
      if (option == "Exit")
        return;
      Button button = new Button();
      button.Margin = new Thickness(6.0, 0.0, 6.0, 0.0);
      button.Tag = (object) option;
      button.Content = (object) Utils.GetString(textKey);
      button.Height = 48.0;
      button.Width = 148.0;
      Button element = button;
      element.SetResourceReference(FrameworkElement.StyleProperty, (object) "FocusOptionButtonStyle");
      if (!isFill)
        element.Background = (Brush) ThemeUtil.GetColorInString("#1F1F1F");
      else if (isRest)
        element.SetResourceReference(Control.BackgroundProperty, (object) "PomoGreen");
      element.Click += new RoutedEventHandler(((FocusOptionButtons) this).OnOptionClick);
      this.Children.Add((UIElement) element);
    }

    protected override async void AddTodayStatistic()
    {
    }
  }
}
