// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.Item.SubTaskListItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView.TaskList;

#nullable disable
namespace ticktick_WPF.Views.TaskList.Item
{
  public class SubTaskListItem : UserControl, ITaskOperation, IComponentConnector
  {
    private DisplayItemController _controller;
    private bool _startDrag;
    private System.Windows.Point _startPoint;
    private bool _mouseDown;
    private DisplayItemModel _model;
    internal Grid ItemGrid;
    internal Grid DragBar;
    internal Grid ItemControl;
    internal ListItemContent TitleContent;
    internal TextBlock TimeText;
    internal Border NavigateGrid;
    internal Grid LineGrid;
    private bool _contentLoaded;

    public SubTaskListItem() => this.InitializeComponent();

    public void SetPriority(int priority) => this._controller?.SetPriority(priority);

    public void SetDate(string key)
    {
      this.TitleContent.TryClearParseDate();
      this._controller.SetDate(key);
    }

    public async void ClearDate()
    {
      this.TitleContent.TitleTextBox.SetCanParseDate(false);
      this._controller.ClearDate();
    }

    public void SelectDate(bool relative) => this._controller.SelectDate();

    public void ToggleTaskCompleted()
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      this._controller.ToggleItemCompleted(dataContext);
    }

    public void Delete() => this._controller?.DeleteSelectedTasks();

    public TaskListView GetParentList() => this._controller?.GetTaskListView();

    public bool ParsingDate() => this.TitleContent.TitleTextBox.CanParseDate;

    public bool IsNewAdd()
    {
      return this.DataContext is DisplayItemModel dataContext && dataContext.IsNewAdd;
    }

    public void PinOrUnpinTask()
    {
    }

    public void OpenSticky()
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      TaskStickyWindow.ShowTaskSticky(new List<string>()
      {
        dataContext.TaskId
      });
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      this.SetModel(dataContext);
      this.TimeText.Cursor = !dataContext.Enable ? Cursors.Arrow : Cursors.Hand;
      if (this._controller == null)
        this._controller = new DisplayItemController((UIElement) this, dataContext);
      else
        this._controller.Reset((UIElement) this, dataContext);
      if (!dataContext.InSticky)
        return;
      this.SetStickyMode();
    }

    private void SetStickyMode()
    {
      this.LineGrid.Visibility = Visibility.Collapsed;
      this.NavigateGrid.Visibility = Visibility.Collapsed;
      this.ItemControl.SetResourceReference(FrameworkElement.HeightProperty, (object) "StickyHeight30");
      this.DragBar.SetResourceReference(FrameworkElement.HeightProperty, (object) "StickyHeight30");
      this.TimeText.SetResourceReference(Control.FontSizeProperty, (object) "StickyFont12");
      this.TimeText.Margin = new Thickness(12.0, 0.0, 0.0, 0.0);
      this.TitleContent.Margin = new Thickness(0.0, -4.0, 0.0, 0.0);
      this.TitleContent.SetStickyMode();
    }

    private void SetModel(DisplayItemModel model)
    {
      model.SetPropertyChangedEvent();
      if (this._model != null)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnTaskTitleChanged), "Title");
      this._model = model;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this._model, new EventHandler<PropertyChangedEventArgs>(this.OnTaskTitleChanged), "Title");
    }

    private void OnTaskTitleChanged(object sender, PropertyChangedEventArgs e)
    {
      if (this.TitleContent.TitleTextBox.KeyboardFocused || !(this.DataContext is DisplayItemModel dataContext) || !(dataContext.Title != this.TitleContent.TitleTextBox.Text))
        return;
      this.TitleContent.TitleTextBox.SetTextOffset(dataContext.Title, true, true);
      this.TitleContent.SetHintText(string.IsNullOrEmpty(dataContext.Title));
    }

    protected virtual void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext is DisplayItemModel dataContext && dataContext.InSticky)
        return;
      this._controller?.ShowOperationDialogSafely();
    }

    private void TimeTextClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || !dataContext.Enable)
        return;
      Window window = Window.GetWindow((DependencyObject) this);
      System.Windows.Point point = this.TranslatePoint(new System.Windows.Point(this.ActualWidth - 180.0, 30.0), (UIElement) window);
      this._controller?.SelectDate((FrameworkElement) window, point.X, point.Y);
    }

    private void NavigateTaskClick(object sender, RoutedEventArgs e)
    {
      this._controller?.NavigateTaskClick();
    }

    private void OnDragMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || dataContext.InSticky)
        return;
      this._startDrag = true;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (this._startDrag && e.LeftButton == MouseButtonState.Pressed)
        this._controller?.OnDragMouseDown(sender, e);
      this._startDrag = false;
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this.DragBar.IsMouseOver || this.TitleContent.IsIconMouseOver || this.NavigateGrid.IsMouseOver)
        return;
      if (!Utils.IfCtrlPressed() && !Utils.IfShiftPressed())
      {
        this._startPoint = e.GetPosition((IInputElement) this);
        this._startDrag = true;
      }
      this._mouseDown = true;
    }

    private async void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      SubTaskListItem subTaskListItem = this;
      if (!(subTaskListItem.DataContext is DisplayItemModel dataContext) || !dataContext.HitVisible)
        return;
      if (subTaskListItem.TitleContent.IsIconMouseOver || subTaskListItem.TimeText.IsMouseOver)
        subTaskListItem._mouseDown = false;
      if (!subTaskListItem._mouseDown)
        return;
      subTaskListItem._mouseDown = false;
      subTaskListItem._controller?.SelectItem(ignoreBatch: dataContext.InSticky);
      if (subTaskListItem.TitleContent.TitleTextBox.CurrentFocused || Utils.IfCtrlPressed() || Utils.IfShiftPressed())
        return;
      await Task.Delay(20);
      subTaskListItem.TitleContent.TitleTextBox.FocusEnd();
    }

    private void OnItemMouseMove(object sender, MouseEventArgs e)
    {
      if (!this.TitleContent.TitleTextBox.CurrentFocused && this._startDrag && e.LeftButton == MouseButtonState.Pressed)
      {
        System.Windows.Point position = e.GetPosition((IInputElement) this);
        if (Math.Abs(position.X - this._startPoint.X) <= 5.0 && Math.Abs(position.Y - this._startPoint.Y) <= 2.0)
          return;
        this._controller?.OnDragMouseDown(sender, e);
      }
      this._startDrag = false;
    }

    private void OnDragMouseUp(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private void OnDoubleClick(object sender, MouseButtonEventArgs e)
    {
      if (this.TitleContent.TitleTextBox.TextArea.Selection.Length > 0 || this.DragBar.IsMouseOver || this.TitleContent.IsIconMouseOver || !(this.DataContext is DisplayItemModel dataContext) || dataContext.InSticky)
        return;
      TaskDetailWindows.ShowTaskWindows(dataContext.Id);
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tasklist/item/subtasklistitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
          ((UIElement) target).MouseMove += new MouseEventHandler(this.OnItemMouseMove);
          ((UIElement) target).MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
          ((Control) target).MouseDoubleClick += new MouseButtonEventHandler(this.OnDoubleClick);
          ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 2:
          this.ItemGrid = (Grid) target;
          break;
        case 3:
          this.DragBar = (Grid) target;
          this.DragBar.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnDragMouseDown);
          this.DragBar.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnDragMouseUp);
          this.DragBar.MouseMove += new MouseEventHandler(this.OnMouseMove);
          break;
        case 4:
          this.ItemControl = (Grid) target;
          break;
        case 5:
          this.TitleContent = (ListItemContent) target;
          break;
        case 6:
          this.TimeText = (TextBlock) target;
          this.TimeText.MouseLeftButtonUp += new MouseButtonEventHandler(this.TimeTextClick);
          break;
        case 7:
          this.NavigateGrid = (Border) target;
          break;
        case 8:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.NavigateTaskClick);
          break;
        case 9:
          this.LineGrid = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
