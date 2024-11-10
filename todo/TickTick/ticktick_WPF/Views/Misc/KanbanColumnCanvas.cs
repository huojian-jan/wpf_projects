// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.KanbanColumnCanvas
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Kanban;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class KanbanColumnCanvas : Canvas
  {
    private readonly SemaphoreSlim _locker = new SemaphoreSlim(1, 1);
    private System.Windows.Point _uiStartPoint;
    private bool _isDragging;
    public double ColumnWidth = 282.0;
    private readonly Storyboard _board = new Storyboard();
    private BlockingList<ColumnViewModel> _columnViewModels = new BlockingList<ColumnViewModel>();
    public readonly BlockingList<KanbanColumnView> ChildrenList = new BlockingList<KanbanColumnView>();
    private double _horizonOffset;
    private double _scrollWidth;
    private List<ColumnViewModel> _displayModels;
    private bool _moveChanged;

    public bool IsDragging => this._isDragging;

    public event EventHandler<Tuple<ColumnViewModel, ColumnViewModel>> Switched;

    public KanbanColumnCanvas()
    {
      this.InitStoryboard();
      this.ColumnWidth = (double) this.FindResource((object) "KanbanColumnWidth");
    }

    public void SetChild(int index, object dataContext)
    {
      if (this.ChildrenList.Count <= index)
        return;
      this.ChildrenList[index].DataContext = dataContext;
    }

    public void RemoveItem(ColumnViewModel model)
    {
      this._columnViewModels.Remove(model);
      this.Width = (double) this._columnViewModels.Count * this.ColumnWidth;
      this.SetColumns();
    }

    public void Insert(int index, ColumnViewModel model)
    {
      this._columnViewModels.Insert(index, model);
      this.Width = (double) this._columnViewModels.Count * this.ColumnWidth;
      this.SetColumns();
    }

    private void OnChildMouseMove(object sender, MouseEventArgs e)
    {
      if (!(sender is KanbanColumnView element) || e.LeftButton != MouseButtonState.Pressed || !(element.DataContext is ColumnViewModel dataContext) || this._uiStartPoint == new System.Windows.Point())
        return;
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      double length = position.X - this._uiStartPoint.X;
      if (!this._isDragging)
      {
        Panel.SetZIndex((UIElement) element, this.Children.Count);
        RotateTransform rotateTransform = new RotateTransform(-3.0);
        element.RenderTransform = (Transform) rotateTransform;
        element.CaptureMouse();
        element.SetColumnDragging(true);
      }
      this.Cursor = Cursors.Hand;
      this._isDragging = true;
      element.DragMouseDown = false;
      double x = position.X;
      Canvas.SetLeft((UIElement) element, length);
      int num = this._columnViewModels.IndexOf(dataContext);
      int index = this.LeftToIndex(x, num);
      if (index < num)
      {
        if (num < 1)
          return;
        ColumnViewModel preVm = this._columnViewModels[num - 1];
        KanbanColumnView kanbanColumnView = this.ChildrenList.FirstOrDefault((Func<KanbanColumnView, bool>) (m => m.DataContext == preVm));
        if (kanbanColumnView == null)
          return;
        double to = (double) num * this.ColumnWidth;
        this.MoveChild(Canvas.GetLeft((UIElement) kanbanColumnView), to, kanbanColumnView);
        this._columnViewModels[num - 1] = dataContext;
        this._columnViewModels[num] = preVm;
      }
      else
      {
        if (index <= num || num > this._columnViewModels.Count - 2)
          return;
        ColumnViewModel nextVm = this._columnViewModels[num + 1];
        KanbanColumnView kanbanColumnView = this.ChildrenList.FirstOrDefault((Func<KanbanColumnView, bool>) (m => m.DataContext == nextVm));
        if (kanbanColumnView == null)
          return;
        double to = (double) num * this.ColumnWidth;
        this.MoveChild(Canvas.GetLeft((UIElement) kanbanColumnView), to, kanbanColumnView);
        this._columnViewModels[num + 1] = dataContext;
        this._columnViewModels[num] = nextVm;
      }
    }

    private void InitStoryboard()
    {
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(200.0));
      DoubleAnimation element = doubleAnimation;
      Storyboard.SetTargetProperty((DependencyObject) element, new PropertyPath("(Canvas.Left)", Array.Empty<object>()));
      this._board.FillBehavior = FillBehavior.Stop;
      this._board.Children.Add((Timeline) element);
    }

    private void MoveChild(double from, double to, KanbanColumnView child)
    {
      this._moveChanged = true;
      this._board.SeekAlignedToLastTick(TimeSpan.FromMilliseconds(200.0));
      this._board.Stop();
      if (!this._board.Children.Any<Timeline>() || !(this._board.Children.First<Timeline>() is DoubleAnimation element))
        return;
      element.To = new double?(to);
      element.From = new double?(from);
      Storyboard.SetTarget((DependencyObject) element, (DependencyObject) child);
      this._board.Completed += new EventHandler(OnCompleted);
      this._board.Begin();

      void OnCompleted(object sender, EventArgs e)
      {
        this._board.Completed -= new EventHandler(OnCompleted);
        Canvas.SetLeft((UIElement) child, to);
      }
    }

    private int LeftToIndex(double left, int currentIndex) => (int) (left / this.ColumnWidth);

    private async void OnChildLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      KanbanColumnCanvas kanbanColumnCanvas = this;
      ColumnViewModel model;
      if (sender is KanbanColumnView ui)
      {
        model = ui.DataContext as ColumnViewModel;
        if (model != null && kanbanColumnCanvas._isDragging)
        {
          kanbanColumnCanvas._isDragging = false;
          kanbanColumnCanvas.Cursor = Cursors.Arrow;
          kanbanColumnCanvas._uiStartPoint = new System.Windows.Point();
          int num = kanbanColumnCanvas._columnViewModels.IndexOf(model);
          if (num >= 0)
          {
            Canvas.SetLeft((UIElement) ui, (double) num * kanbanColumnCanvas.ColumnWidth);
            if (kanbanColumnCanvas._moveChanged)
            {
              ColumnViewModel columnViewModel1 = num > 0 ? kanbanColumnCanvas._columnViewModels[num - 1] : (ColumnViewModel) null;
              ColumnViewModel columnViewModel2 = num < kanbanColumnCanvas._columnViewModels.Count - 1 ? kanbanColumnCanvas._columnViewModels[num + 1] : (ColumnViewModel) null;
              string dropId = columnViewModel1?.ColumnId ?? columnViewModel2?.ColumnId;
              if (!string.IsNullOrEmpty(dropId))
              {
                long? nullable = await ColumnDao.SaveSortOrder(model.ColumnId, dropId, columnViewModel1 == null);
                SyncManager.TryDelaySync();
                DataChangedNotifier.NotifyColumnChanged(model.Identity?.GetProjectId());
                model.SortOrder = nullable ?? model.SortOrder;
              }
            }
          }
          kanbanColumnCanvas._moveChanged = false;
          Panel.SetZIndex((UIElement) ui, 0);
          ui.ReleaseMouseCapture();
          ui.MouseMove -= new MouseEventHandler(kanbanColumnCanvas.OnChildMouseMove);
          ui.RenderTransform = (Transform) null;
          ui.SetColumnDragging(false);
        }
      }
      kanbanColumnCanvas._isDragging = false;
      kanbanColumnCanvas.Cursor = Cursors.Arrow;
      kanbanColumnCanvas._uiStartPoint = new System.Windows.Point();
      ui = (KanbanColumnView) null;
      model = (ColumnViewModel) null;
    }

    private async void OnChildLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      KanbanColumnCanvas kanbanColumnCanvas = this;
      if (!(sender is KanbanColumnView relativeTo) || !relativeTo.IsMouseDragOver || !(relativeTo.DataContext is ColumnViewModel dataContext) || !dataContext.Enable)
        return;
      kanbanColumnCanvas._uiStartPoint = e.GetPosition((IInputElement) relativeTo);
      relativeTo.MouseMove -= new MouseEventHandler(kanbanColumnCanvas.OnChildMouseMove);
      relativeTo.MouseMove += new MouseEventHandler(kanbanColumnCanvas.OnChildMouseMove);
      relativeTo.DragMouseDown = true;
    }

    public void Clear()
    {
      this.ChildrenList.Clear();
      this.Children.Clear();
    }

    public void ReloadQuickAddView()
    {
      foreach (object child in this.Children)
      {
        if (child is KanbanColumnView kanbanColumnView)
          kanbanColumnView.ResetQuickAddView();
      }
    }

    public void SetItemModels(BlockingList<ColumnViewModel> columnViewModels)
    {
      this._columnViewModels = columnViewModels;
      this.Width = (double) this._columnViewModels.Count * this.ColumnWidth;
      this.SetColumns();
    }

    private async Task SetColumns()
    {
      KanbanColumnCanvas kanbanColumnCanvas = this;
      await kanbanColumnCanvas._locker.WaitAsync();
      try
      {
        if (kanbanColumnCanvas._scrollWidth <= 0.0)
          return;
        int num1 = (int) (kanbanColumnCanvas._horizonOffset / kanbanColumnCanvas.ColumnWidth) - 1;
        if (num1 >= kanbanColumnCanvas._columnViewModels.Count)
          return;
        if (num1 < 0)
          num1 = 0;
        int num2 = num1 + (int) (kanbanColumnCanvas._scrollWidth / kanbanColumnCanvas.ColumnWidth) + 2;
        if (num2 >= kanbanColumnCanvas._columnViewModels.Count)
          num2 = kanbanColumnCanvas._columnViewModels.Count - 1;
        List<KanbanColumnView> list = kanbanColumnCanvas.ChildrenList.ToList();
        List<ColumnViewModel> models = new List<ColumnViewModel>();
        for (int index = num1; index <= num2; ++index)
          models.Add(kanbanColumnCanvas._columnViewModels[index]);
        if (kanbanColumnCanvas._displayModels != null && kanbanColumnCanvas._displayModels.Count == models.Count)
        {
          bool flag = true;
          for (int index = 0; index < models.Count; ++index)
          {
            flag = kanbanColumnCanvas._displayModels[index] == models[index];
            if (!flag)
              break;
          }
          if (flag)
            return;
        }
        kanbanColumnCanvas._displayModels = models;
        for (int index = 0; index < models.Count; ++index)
        {
          ColumnViewModel model = models[index];
          KanbanColumnView kanbanColumnView = list.FirstOrDefault<KanbanColumnView>((Func<KanbanColumnView, bool>) (c => c.ColumnId == model.ColumnId)) ?? list.FirstOrDefault<KanbanColumnView>((Func<KanbanColumnView, bool>) (c => models.All<ColumnViewModel>((Func<ColumnViewModel, bool>) (m => m.ColumnId != c.ColumnId))));
          if (kanbanColumnView != null)
          {
            list.Remove(kanbanColumnView);
            if (kanbanColumnView.DataContext != model)
              kanbanColumnView.DataContext = (object) model;
          }
          else
          {
            kanbanColumnView = new KanbanColumnView(model);
            if (kanbanColumnCanvas.ActualHeight > 18.0)
              kanbanColumnView.SetMaxHeight(kanbanColumnCanvas.ActualHeight - 18.0);
            kanbanColumnCanvas.Children.Add((UIElement) kanbanColumnView);
            kanbanColumnCanvas.ChildrenList.Add(kanbanColumnView);
            kanbanColumnView.MouseLeftButtonDown += new MouseButtonEventHandler(kanbanColumnCanvas.OnChildLeftButtonDown);
            kanbanColumnView.MouseLeftButtonUp += new MouseButtonEventHandler(kanbanColumnCanvas.OnChildLeftButtonUp);
          }
          Canvas.SetLeft((UIElement) kanbanColumnView, (double) (num1 + index) * kanbanColumnCanvas.ColumnWidth);
          Canvas.SetTop((UIElement) kanbanColumnView, 16.0);
        }
        foreach (KanbanColumnView kanbanColumnView in list)
        {
          kanbanColumnCanvas.Children.Remove((UIElement) kanbanColumnView);
          kanbanColumnCanvas.ChildrenList.Remove(kanbanColumnView);
          kanbanColumnView.MouseLeftButtonDown -= new MouseButtonEventHandler(kanbanColumnCanvas.OnChildLeftButtonDown);
          kanbanColumnView.MouseLeftButtonUp -= new MouseButtonEventHandler(kanbanColumnCanvas.OnChildLeftButtonUp);
          kanbanColumnView.UnbindEvents();
        }
      }
      finally
      {
        kanbanColumnCanvas._locker.Release();
      }
    }

    public void SetScrollOffset(double horizonOffset, double scrollWidth)
    {
      this._horizonOffset = horizonOffset;
      this._scrollWidth = scrollWidth;
      this.SetColumns();
    }

    public KanbanColumnView GetColumnById(string columnId)
    {
      return this.ChildrenList.FirstOrDefault((Func<KanbanColumnView, bool>) (c => c.GetColumnId() == columnId));
    }

    public void SetItemMaxHeight()
    {
      foreach (KanbanColumnView kanbanColumnView in this.ChildrenList.Value)
      {
        if (this.ActualHeight > 18.0)
          kanbanColumnView.SetMaxHeight(this.ActualHeight - 18.0);
      }
    }

    public void Dispose()
    {
      this.Children.Clear();
      this.ChildrenList.Clear();
    }

    public void SetShowAddInColumn()
    {
      foreach (KanbanColumnView kanbanColumnView in this.ChildrenList.Value)
        kanbanColumnView.SetShowAdd();
    }

    public void ColumnWidthChanged()
    {
      this.ColumnWidth = (double) this.FindResource((object) "KanbanColumnWidth");
      BlockingList<ColumnViewModel> columnViewModels = this._columnViewModels;
      if ((columnViewModels != null ? (columnViewModels.Count > 0 ? 1 : 0) : 0) == 0)
        return;
      this.Width = (double) this._columnViewModels.Count * this.ColumnWidth;
      List<KanbanColumnView> list = this.ChildrenList.Value.ToList<KanbanColumnView>();
      for (int index = 0; index < list.Count; ++index)
        Canvas.SetLeft((UIElement) list[index], (double) index * this.ColumnWidth);
    }
  }
}
