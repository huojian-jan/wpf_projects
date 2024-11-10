// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.TaskCheckIcon
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public sealed class TaskCheckIcon : Canvas
  {
    private readonly Path _icon;
    private Rectangle _backBorder;
    private bool _showBack;

    private Brush IconFill { get; set; }

    public TaskCheckIcon()
    {
      this.Height = 14.0;
      this.SetBinding(FrameworkElement.WidthProperty, "IconWidth");
      this.Background = (Brush) Brushes.Transparent;
      Path path = new Path();
      path.Width = 14.0;
      path.Height = 14.0;
      path.Stretch = Stretch.Uniform;
      this._icon = path;
      this.Children.Add((UIElement) this._icon);
      this._icon.SetBinding(Path.DataProperty, "Icon");
      this._icon.SetBinding(FrameworkElement.WidthProperty, "IconWidth");
      this.MouseEnter += new MouseEventHandler(this.OnMouseOverChange);
      this.MouseLeave += new MouseEventHandler(this.OnMouseOverChange);
    }

    private void OnMouseOverChange(object sender, MouseEventArgs e) => this.SetColor();

    private void SetColor()
    {
      if (this.IconFill == null)
      {
        this._icon.Fill = (Brush) ThemeUtil.GetColor(this.IsMouseOver ? "BaseColorOpacity40" : "BaseColorOpacity20", (FrameworkElement) this);
        if (this._backBorder == null)
          return;
        this.Children.Remove((UIElement) this._backBorder);
      }
      else
      {
        this._icon.Fill = this.IconFill;
        if (this.IsMouseOver && this._showBack)
        {
          if (this._backBorder == null)
          {
            Rectangle rectangle = new Rectangle();
            rectangle.Width = 14.0;
            rectangle.Height = 14.0;
            rectangle.RadiusX = 2.0;
            rectangle.RadiusY = 2.0;
            rectangle.Opacity = 0.1;
            this._backBorder = rectangle;
            this.Children.Add((UIElement) this._backBorder);
          }
          this._backBorder.Fill = this.IconFill;
        }
        else
        {
          if (this._backBorder == null)
            return;
          this.Children.Remove((UIElement) this._backBorder);
          this._backBorder = (Rectangle) null;
        }
      }
    }

    public void SetIconColor(DisplayType type, int priority, int status)
    {
      this._showBack = type != DisplayType.Course && type != DisplayType.Event && type != DisplayType.Note && status == 0;
      if (status == 0 && (type == DisplayType.Task || type == DisplayType.CheckItem || type == DisplayType.Agenda))
      {
        switch (priority)
        {
          case 0:
            this.IconFill = (Brush) ThemeUtil.GetColor("BaseColorOpacity40", (FrameworkElement) this);
            break;
          case 1:
            this.IconFill = (Brush) ThemeUtil.GetColor("PriorityLowColor");
            break;
          case 3:
            this.IconFill = (Brush) ThemeUtil.GetColor("PriorityMiddleColor");
            break;
          case 5:
            this.IconFill = (Brush) ThemeUtil.GetColor("PriorityHighColor");
            break;
        }
      }
      else
        this.IconFill = status == 0 ? (Brush) ThemeUtil.GetColor("BaseColorOpacity40", (FrameworkElement) this) : (Brush) null;
      this.SetColor();
    }

    public void SetSize(int size)
    {
      this.Height = (double) size;
      this._icon.Width = (double) size;
      this._icon.Height = (double) size;
    }
  }
}
