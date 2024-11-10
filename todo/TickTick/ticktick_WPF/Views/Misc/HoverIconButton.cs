// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.HoverIconButton
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class HoverIconButton : Grid
  {
    public readonly Border BackBorder;
    public string IconColor = "BaseColorOpacity100";
    private Path _icon;
    private Image _image;
    public static readonly DependencyProperty IconDataProperty = DependencyProperty.Register(nameof (IconData), typeof (Geometry), typeof (HoverIconButton), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty IsImageProperty = DependencyProperty.Register(nameof (IsImage), typeof (bool), typeof (HoverIconButton), new PropertyMetadata((object) true));
    public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(nameof (ImageSource), typeof (ImageSource), typeof (HoverIconButton), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty ImageWidthProperty = DependencyProperty.Register(nameof (ImageWidth), typeof (double), typeof (HoverIconButton), new PropertyMetadata((object) 18.0));
    public static readonly DependencyProperty ImageOpacityProperty = DependencyProperty.Register(nameof (ImageOpacity), typeof (double), typeof (HoverIconButton), new PropertyMetadata((object) 0.6));

    public HoverIconButton()
    {
      this.Cursor = Cursors.Hand;
      this.HorizontalAlignment = HorizontalAlignment.Center;
      this.VerticalAlignment = VerticalAlignment.Center;
      this.BackBorder = new Border();
      this.BackBorder.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle");
      this.Children.Add((UIElement) this.BackBorder);
      this.Width = 28.0;
      this.Height = 28.0;
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.SetValue(ToolTipService.BetweenShowDelayProperty, (object) 0);
      this.SetValue(ToolTipService.InitialShowDelayProperty, (object) 500);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      if (this.IsImage)
      {
        Image image = new Image();
        image.Width = this.ImageWidth;
        image.Height = this.ImageWidth;
        image.Opacity = this.ImageOpacity;
        image.Margin = new Thickness(2.0);
        image.IsHitTestVisible = false;
        this._image = image;
        this._image.SetBinding(Image.SourceProperty, (BindingBase) new Binding("ImageSource")
        {
          Source = (object) this
        });
        this._image.SetResourceReference(FrameworkElement.StyleProperty, (object) "Icon01");
        this.Children.Add((UIElement) this._image);
      }
      else
      {
        Path path = new Path();
        path.Data = this.IconData;
        path.Width = this.ImageWidth;
        path.Height = this.ImageWidth;
        path.Opacity = this.ImageOpacity;
        path.Margin = new Thickness(2.0);
        path.IsHitTestVisible = false;
        path.Stretch = Stretch.Uniform;
        this._icon = path;
        this._icon.SetResourceReference(Shape.FillProperty, (object) this.IconColor);
        this.Children.Add((UIElement) this._icon);
      }
    }

    public Geometry IconData
    {
      get => (Geometry) this.GetValue(HoverIconButton.IconDataProperty);
      set
      {
        this.SetValue(HoverIconButton.IconDataProperty, (object) value);
        if (this._icon == null)
          return;
        this._icon.Data = value;
      }
    }

    public bool IsImage
    {
      get => (bool) this.GetValue(HoverIconButton.IsImageProperty);
      set => this.SetValue(HoverIconButton.IsImageProperty, (object) value);
    }

    public ImageSource ImageSource
    {
      get => (ImageSource) this.GetValue(HoverIconButton.ImageSourceProperty);
      set => this.SetValue(HoverIconButton.ImageSourceProperty, (object) value);
    }

    public double ImageWidth
    {
      get => (double) this.GetValue(HoverIconButton.ImageWidthProperty);
      set => this.SetValue(HoverIconButton.ImageWidthProperty, (object) value);
    }

    public double ImageOpacity
    {
      get => (double) this.GetValue(HoverIconButton.ImageOpacityProperty);
      set
      {
        this.SetValue(HoverIconButton.ImageOpacityProperty, (object) value);
        if (this._icon != null)
          this._icon.Opacity = value;
        if (this._image == null)
          return;
        this._image.Opacity = value;
      }
    }

    public void SetIconColor(string color)
    {
      this.IconColor = color;
      this._icon?.SetResourceReference(Shape.FillProperty, (object) color);
    }
  }
}
