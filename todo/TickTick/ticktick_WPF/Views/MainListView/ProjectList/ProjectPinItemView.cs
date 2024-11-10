// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ProjectList.ProjectPinItemView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.MainListView.ProjectList
{
  public class ProjectPinItemView : Border
  {
    private bool _mouseDown;

    public bool InPopup { get; set; }

    public ProjectPinItemView()
    {
      this.Height = 56.0;
      this.SetBinding(FrameworkElement.WidthProperty, "ItemWidth");
      this.Background = (Brush) Brushes.Transparent;
      this.CornerRadius = new CornerRadius(6.0);
      this.Margin = new Thickness(1.0, 2.0, 1.0, 2.0);
      this.Cursor = Cursors.Hand;
      this.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
      this.MouseRightButtonUp += new MouseButtonEventHandler(this.OnItemRightClick);
      this.Unloaded += new RoutedEventHandler(this.OnUnload);
    }

    private void OnItemRightClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Border border))
        return;
      ProjectPinItemViewModel model = border.DataContext as ProjectPinItemViewModel;
      if (model == null)
        return;
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      types.Add(new CustomMenuItemViewModel((object) "unpin", Utils.GetString("Unpin"), (DrawingImage) null)
      {
        ImageWidth = 0.0
      });
      EscPopup escPopup = new EscPopup();
      escPopup.PlacementTarget = (UIElement) this;
      escPopup.Placement = PlacementMode.Mouse;
      escPopup.StaysOpen = false;
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) escPopup);
      customMenuList.Operated += (EventHandler<object>) (async (o, a) =>
      {
        await ProjectPinSortOrderService.Delete(model.SyncEntityId, model.Type);
        Utils.FindParent<ProjectPinView>((DependencyObject) this)?.OnItemUnpin(model);
      });
      customMenuList.Show();
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!this._mouseDown)
        return;
      this._mouseDown = false;
      if (!(this.DataContext is ProjectPinItemViewModel dataContext))
        return;
      Utils.FindParent<ProjectPinView>((DependencyObject) this)?.OnItemSelect(dataContext);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._mouseDown = true;
      if (!(this.DataContext is ProjectPinItemViewModel dataContext))
        return;
      Utils.FindParent<ProjectPinView>((DependencyObject) this)?.OnItemMouseDown(dataContext);
    }

    private void OnUnload(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext is ProjectPinItemViewModel dataContext))
        return;
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) dataContext, new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), "");
    }

    private void OnMouseLeave(object sender, MouseEventArgs e) => this.SetBackGround();

    private void OnMouseEnter(object sender, MouseEventArgs e) => this.SetBackGround();

    private void SetBackGround()
    {
      if (this.DataContext is ProjectPinItemViewModel dataContext)
      {
        if (dataContext.IsMore)
          return;
        if (dataContext.Selected)
        {
          this.SetResourceReference(Border.BackgroundProperty, (object) "ProjectSelectedBackground");
          return;
        }
      }
      if (this.IsMouseOver || this.InPopup)
        this.SetResourceReference(Border.BackgroundProperty, (object) "ProjectHoverBackGround");
      else
        this.Background = (Brush) Brushes.Transparent;
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is ProjectPinItemViewModel oldValue)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), "");
      if (!(e.NewValue is ProjectPinItemViewModel newValue))
        return;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnModelPropertyChanged), "");
      this.SetBackGround();
      if (newValue.IsMore)
      {
        this.ToolTip = (object) null;
        this.SetMoreContent();
      }
      else if (oldValue == null || oldValue.IsMore || oldValue.Icon == null != (newValue.Icon == null))
      {
        System.Windows.Controls.ToolTip toolTip = new System.Windows.Controls.ToolTip();
        toolTip.SetResourceReference(FrameworkElement.StyleProperty, (object) "PinProjectTooltipStyle");
        toolTip.SetBinding(ContentControl.ContentProperty, "Title");
        this.ToolTip = (object) toolTip;
        this.SetItemContent(newValue);
      }
      this.Opacity = this.InPopup || !newValue.Dragging ? 1.0 : 0.0;
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Selected")
        this.SetBackGround();
      if (!(e.PropertyName == "Dragging"))
        return;
      this.Opacity = this.InPopup || (this.DataContext is ProjectPinItemViewModel dataContext ? (dataContext.Dragging ? 1 : 0) : 0) == 0 ? 1.0 : 0.0;
    }

    private void SetItemContent(ProjectPinItemViewModel model)
    {
      Grid grid = new Grid();
      Border border = new Border();
      border.Height = 28.0;
      border.Width = 28.0;
      border.CornerRadius = new CornerRadius(6.0);
      border.VerticalAlignment = VerticalAlignment.Top;
      border.HorizontalAlignment = HorizontalAlignment.Center;
      border.Margin = new Thickness(0.0, 4.0, 0.0, 0.0);
      Border element1 = border;
      element1.SetBinding(Border.BackgroundProperty, "IconBackground");
      grid.Children.Add((UIElement) element1);
      if (model.Icon != null)
      {
        Path path = new Path();
        path.VerticalAlignment = VerticalAlignment.Top;
        Path element2 = path;
        element2.SetResourceReference(FrameworkElement.StyleProperty, (object) "Path01");
        element2.Width = 14.0;
        element2.Height = 14.0;
        element2.SetBinding(Path.DataProperty, "Icon");
        element2.SetBinding(Shape.FillProperty, "IconColor");
        element2.Margin = new Thickness(0.0, 11.0, 0.0, 0.0);
        grid.Children.Add((UIElement) element2);
      }
      else
      {
        EmjTextBlock emjTextBlock = new EmjTextBlock();
        emjTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
        emjTextBlock.VerticalAlignment = VerticalAlignment.Top;
        emjTextBlock.Margin = new Thickness(0.0, 9.0, 0.0, 0.0);
        EmjTextBlock element3 = emjTextBlock;
        element3.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body01");
        element3.SetBinding(EmjTextBlock.TextProperty, "Emoji");
        grid.Children.Add((UIElement) element3);
      }
      EmjTextBlock emjTextBlock1 = new EmjTextBlock();
      emjTextBlock1.Name = "PTitle";
      emjTextBlock1.TextTrimming = TextTrimming.WordEllipsis;
      emjTextBlock1.HorizontalAlignment = HorizontalAlignment.Center;
      emjTextBlock1.VerticalAlignment = VerticalAlignment.Bottom;
      emjTextBlock1.Margin = new Thickness(0.0, 0.0, 0.0, 6.0);
      emjTextBlock1.FontSize = 10.0;
      EmjTextBlock element4 = emjTextBlock1;
      element4.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body01");
      element4.SetResourceReference(TextBlock.ForegroundProperty, (object) "ProjectMenuColorOpacity80");
      element4.SetBinding(EmjTextBlock.TextProperty, "DisplayTitle");
      grid.Children.Add((UIElement) element4);
      this.Child = (UIElement) grid;
    }

    private void SetMoreContent()
    {
      Grid grid1 = new Grid();
      Border border = new Border();
      border.Height = 28.0;
      border.Width = 28.0;
      border.VerticalAlignment = VerticalAlignment.Top;
      border.Background = (Brush) Brushes.Transparent;
      border.Margin = new Thickness(0.0, 4.0, 0.0, 0.0);
      Border element1 = border;
      element1.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle60_100");
      Rectangle rectangle = new Rectangle();
      rectangle.Height = 28.0;
      rectangle.Width = 28.0;
      rectangle.RadiusX = 6.0;
      rectangle.RadiusY = 6.0;
      rectangle.StrokeThickness = 1.0;
      Rectangle element2 = rectangle;
      Grid grid2 = new Grid();
      grid2.Width = 28.0;
      grid2.Height = 28.0;
      Grid grid3 = grid2;
      grid3.Children.Add((UIElement) element2);
      element2.SetResourceReference(Shape.StrokeProperty, (object) "ProjectMenuColorOpacity40");
      Path path = new Path();
      path.Width = 12.0;
      path.Height = 12.0;
      path.Data = Utils.GetIcon("ArrowLine");
      Path element3 = path;
      element3.SetResourceReference(Shape.FillProperty, (object) "ProjectMenuColorOpacity60");
      element3.SetResourceReference(FrameworkElement.StyleProperty, (object) "Path01");
      grid3.Children.Add((UIElement) element3);
      element1.Child = (UIElement) grid3;
      TextBlock textBlock = new TextBlock();
      textBlock.TextTrimming = TextTrimming.WordEllipsis;
      textBlock.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock.VerticalAlignment = VerticalAlignment.Bottom;
      textBlock.Margin = new Thickness(0.0, 0.0, 0.0, 6.0);
      textBlock.FontSize = 10.0;
      textBlock.Text = Utils.GetString("Expand");
      TextBlock element4 = textBlock;
      element4.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body01");
      element4.SetResourceReference(TextBlock.ForegroundProperty, (object) "ProjectMenuColorOpacity80");
      grid1.Children.Add((UIElement) element1);
      grid1.Children.Add((UIElement) element4);
      this.Child = (UIElement) grid1;
    }
  }
}
