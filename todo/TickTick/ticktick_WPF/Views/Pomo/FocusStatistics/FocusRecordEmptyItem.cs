// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.FocusRecordEmptyItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class FocusRecordEmptyItem : StackPanel
  {
    public FocusRecordEmptyItem()
    {
      this.Margin = new Thickness(20.0);
      Grid grid = new Grid();
      grid.Height = 200.0;
      grid.Width = 200.0;
      Grid element1 = grid;
      Image image = new Image();
      image.Height = 200.0;
      image.Width = 200.0;
      Image element2 = image;
      element2.SetResourceReference(Image.SourceProperty, (object) "FocusRecordEmptyImage");
      element2.Stretch = Stretch.Uniform;
      Path path = new Path();
      path.Height = 200.0;
      path.Width = 200.0;
      path.Stretch = Stretch.Uniform;
      Path element3 = path;
      element3.Data = Utils.GetIcon("FocusRecordEmptyIcon");
      element3.SetResourceReference(Shape.FillProperty, (object) "EmptyPathColor");
      Ellipse ellipse = new Ellipse();
      ellipse.Width = 112.0;
      ellipse.Height = 112.0;
      ellipse.VerticalAlignment = VerticalAlignment.Center;
      ellipse.HorizontalAlignment = HorizontalAlignment.Center;
      Ellipse element4 = ellipse;
      element4.SetResourceReference(Shape.FillProperty, (object) "BaseColorOpacity5");
      element1.Children.Add((UIElement) element4);
      element1.Children.Add((UIElement) element2);
      element1.Children.Add((UIElement) element3);
      this.Children.Add((UIElement) element1);
      TextBlock textBlock = new TextBlock();
      textBlock.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock.VerticalAlignment = VerticalAlignment.Center;
      textBlock.TextAlignment = TextAlignment.Center;
      textBlock.FontSize = 14.0;
      TextBlock element5 = textBlock;
      element5.Text = Utils.GetString("FocusRecordEmpty");
      element5.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
      this.Children.Add((UIElement) element5);
    }
  }
}
