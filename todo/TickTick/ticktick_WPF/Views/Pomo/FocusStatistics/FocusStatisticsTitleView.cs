// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.FocusStatisticsTitleView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class FocusStatisticsTitleView : DockPanel
  {
    public FocusStatisticsTitleView()
      : this(false)
    {
    }

    public FocusStatisticsTitleView(bool showBottomLine)
    {
      if (showBottomLine)
      {
        Border border = new Border();
        border.Height = 10.0;
        border.BorderThickness = new Thickness(0.0, 0.0, 0.0, 1.0);
        Border element = border;
        element.SetResourceReference(Border.BorderBrushProperty, (object) "BaseColorOpacity5");
        this.Children.Add((UIElement) element);
        element.SetValue(DockPanel.DockProperty, (object) Dock.Bottom);
      }
      HoverIconButton hoverIconButton = new HoverIconButton();
      hoverIconButton.Margin = new Thickness(0.0, 0.0, 18.0, 0.0);
      HoverIconButton icon = hoverIconButton;
      icon.SetValue(DockPanel.DockProperty, (object) Dock.Right);
      icon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnIconClick);
      this.Children.Add((UIElement) icon);
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 18.0;
      textBlock.Margin = new Thickness(20.0, 0.0, 0.0, 0.0);
      TextBlock element1 = textBlock;
      element1.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      element1.SetValue(DockPanel.DockProperty, (object) Dock.Left);
      element1.SetBinding(TextBlock.TextProperty, "Title");
      this.Children.Add((UIElement) element1);
      this.DataContextChanged += (DependencyPropertyChangedEventHandler) ((o, e) =>
      {
        if (!(e.NewValue is FocusStatisticsTitleItemViewModel newValue2))
          return;
        icon.Visibility = string.IsNullOrEmpty(newValue2.ImageName) ? Visibility.Collapsed : Visibility.Visible;
        icon.SetResourceReference(HoverIconButton.ImageSourceProperty, (object) newValue2.ImageName);
      });
    }

    private void OnIconClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is FocusStatisticsTitleItemViewModel dataContext))
        return;
      switch (dataContext.ImageTag)
      {
        case "Statistics":
          TickFocusManager.OpenStatisticsInWeb();
          break;
        case "Add":
          AddFocusRecordWindow focusRecordWindow = new AddFocusRecordWindow(LocalSettings.Settings.PomoLocalSetting.AddRecordType);
          focusRecordWindow.Owner = Window.GetWindow((DependencyObject) this);
          focusRecordWindow.ShowDialog();
          break;
      }
    }
  }
}
