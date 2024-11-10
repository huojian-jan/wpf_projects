// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.DayCellControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class DayCellControl : UserControl, IComponentConnector
  {
    internal Grid DayCellGrid;
    internal Border RangeBorder;
    internal Image WorkRestImage;
    internal TextBlock DateText;
    private bool _contentLoaded;

    public event EventHandler<DayViewModel> DayClicked;

    public event EventHandler<DayViewModel> DayDoubleClicked;

    public DayCellControl()
    {
      this.InitializeComponent();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is DayViewModel oldValue)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnPropertyChanged), "IsRest");
      if (!(e.NewValue is DayViewModel newValue))
        return;
      if (newValue.IsRest)
        this.WorkRestImage.SetResourceReference(Image.SourceProperty, (object) "RestDrawingImage");
      else if (newValue.IsWork)
        this.WorkRestImage.SetResourceReference(Image.SourceProperty, (object) "WorkDrawingImage");
      else
        this.WorkRestImage.Source = (ImageSource) null;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnPropertyChanged), "IsRest");
      if (!this.IsMouseOver)
        return;
      newValue.Hover = true;
    }

    private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(this.DataContext is DayViewModel dataContext))
        return;
      if (dataContext.IsRest)
        this.WorkRestImage.SetResourceReference(Image.SourceProperty, (object) "RestDrawingImage");
      else if (dataContext.IsWork)
        this.WorkRestImage.SetResourceReference(Image.SourceProperty, (object) "WorkDrawingImage");
      else
        this.WorkRestImage.Source = (ImageSource) null;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      if (this.DataContext == null || !(this.DataContext is DayViewModel dataContext))
        return;
      dataContext.Hover = true;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      if (this.DataContext == null || !(this.DataContext is DayViewModel dataContext))
        return;
      dataContext.Hover = false;
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (this.DataContext == null || !(this.DataContext is DayViewModel dataContext) || !dataContext.CanSelect || dataContext.IsFixed)
        return;
      EventHandler<DayViewModel> dayClicked = this.DayClicked;
      if (dayClicked == null)
        return;
      dayClicked((object) this, dataContext);
    }

    private void OnCellDoubleClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (this.DataContext == null || !(this.DataContext is DayViewModel dataContext) || !dataContext.CanSelect || dataContext.IsFixed || !dataContext.CanDoubleSelect)
        return;
      dataContext.Selected = !dataContext.Selected;
      EventHandler<DayViewModel> dayDoubleClicked = this.DayDoubleClicked;
      if (dayDoubleClicked == null)
        return;
      dayDoubleClicked((object) this, dataContext);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/daycellcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnMouseEnter);
          ((UIElement) target).MouseLeave += new MouseEventHandler(this.OnMouseLeave);
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClick);
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.OnCellDoubleClick);
          break;
        case 2:
          this.DayCellGrid = (Grid) target;
          break;
        case 3:
          this.RangeBorder = (Border) target;
          break;
        case 4:
          this.WorkRestImage = (Image) target;
          break;
        case 5:
          this.DateText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
