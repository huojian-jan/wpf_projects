// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.UiUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

#nullable disable
namespace ticktick_WPF.Util
{
  public class UiUtils
  {
    public static void SetSaveButtonTabSelected(Button button, bool tabSelected)
    {
      if (tabSelected)
      {
        button.BorderThickness = new Thickness(1.0);
        button.SetResourceReference(Control.BorderBrushProperty, (object) "TabBorderColor");
        button.SetResourceReference(Control.BackgroundProperty, (object) "PrimaryColor60");
      }
      else
      {
        button.BorderThickness = new Thickness(0.0);
        button.SetResourceReference(Control.BackgroundProperty, (object) "TabBorderColor");
      }
    }

    public static void SetCancelButtonTabSelected(Button button, bool tabSelected)
    {
      button.SetResourceReference(Control.BorderBrushProperty, tabSelected ? (object) "TabBorderColor" : (object) "BaseColorOpacity20");
    }

    public static void SetBorderTabSelected(Border border, bool tabSelected)
    {
      if (tabSelected)
        border.SetResourceReference(Control.BorderBrushProperty, (object) "TabBorderColor");
      else
        border.BorderBrush = (Brush) Brushes.Transparent;
    }

    public static Path GetArrow(double size, double angle, string color)
    {
      Path path = new Path();
      path.Height = size;
      path.Width = size;
      path.Data = Utils.GetIcon("ArrowLine");
      path.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
      Path arrow = path;
      if (angle != 0.0)
        arrow.RenderTransform = (Transform) new RotateTransform(angle);
      arrow.SetResourceReference(FrameworkElement.StyleProperty, (object) "Path01");
      arrow.SetResourceReference(Shape.FillProperty, (object) color);
      return arrow;
    }

    public static void SetTimeTextRun(TextBlock text, long minutes, int size, string foreground)
    {
      long num1 = minutes / 60L;
      long num2 = minutes % 60L;
      text.Inlines.Clear();
      if (num1 > 0L)
      {
        text.Inlines.Add((Inline) new Run(num1.ToString() ?? ""));
        Run run1 = new Run(" h ");
        run1.FontSize = (double) size;
        Run run2 = run1;
        run2.SetResourceReference(TextElement.ForegroundProperty, (object) foreground);
        text.Inlines.Add((Inline) run2);
      }
      if (num1 != 0L && num2 <= 0L)
        return;
      text.Inlines.Add((Inline) new Run(num2.ToString() ?? ""));
      Run run3 = new Run(" m ");
      run3.FontSize = (double) size;
      Run run4 = run3;
      run4.SetResourceReference(TextElement.ForegroundProperty, (object) foreground);
      text.Inlines.Add((Inline) run4);
    }

    public static Path CreatePath(string geoKey, string colorKey, string styleKey)
    {
      Path path = new Path();
      if (!string.IsNullOrEmpty(geoKey))
        path.Data = Utils.GetIcon(geoKey);
      if (!string.IsNullOrEmpty(colorKey))
        path.SetResourceReference(Shape.FillProperty, (object) colorKey);
      if (!string.IsNullOrEmpty(styleKey))
        path.SetResourceReference(FrameworkElement.StyleProperty, (object) styleKey);
      return path;
    }
  }
}
