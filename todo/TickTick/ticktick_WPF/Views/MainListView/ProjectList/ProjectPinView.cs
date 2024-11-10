// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ProjectList.ProjectPinView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.MainListView.ProjectList
{
  public class ProjectPinView : StackPanel
  {
    private List<ProjectPinItemViewModel> _data;
    private int _countInLine;
    private readonly ProjectPinSortOrderModel _selectedData;
    private ProjectPinItemViewModel _moreItem;
    private double _itemWidth;
    private readonly ListView _listView;
    private readonly Border _bottomBorder;
    private Popup _projectDragPopup;
    private int _startDragIndex;
    private ProjectPinItemViewModel _mouseDownModel;

    public ProjectPinView()
    {
      ListView listView = new ListView();
      listView.MaxHeight = LocalSettings.Settings.ProjectPinFolded ? 120.0 : 240.0;
      listView.HorizontalAlignment = HorizontalAlignment.Left;
      this._listView = listView;
      this._bottomBorder = new Border();
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this.InitUiChildren();
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.DragMouseUp);
      this.MouseMove += new MouseEventHandler(this.OnMouseMove);
    }

    private void InitUiChildren()
    {
      this._listView.SetResourceReference(FrameworkElement.StyleProperty, (object) "ListViewStyle");
      this._listView.SetResourceReference(ItemsControl.ItemContainerStyleProperty, (object) "ListViewItemContainerStyle");
      this._listView.ItemTemplate = (DataTemplate) this.FindResource((object) "ProjectPinItemTemplate");
      this._listView.ItemsPanel = (ItemsPanelTemplate) this.FindResource((object) "WrapPanelTemplate");
      this.Children.Add((UIElement) this._listView);
      Grid grid = new Grid();
      grid.MinHeight = 11.0;
      grid.Margin = new Thickness(0.0, 0.0, 12.0, 6.0);
      Grid element1 = grid;
      Line line = new Line();
      line.X1 = 0.0;
      line.X2 = 1.0;
      line.Stretch = Stretch.Fill;
      line.VerticalAlignment = VerticalAlignment.Bottom;
      line.StrokeThickness = 1.0;
      Line element2 = line;
      element2.SetResourceReference(Shape.StrokeProperty, (object) "ProjectMenuColorOpacity5");
      element1.Children.Add((UIElement) this._bottomBorder);
      element1.Children.Add((UIElement) element2);
      this.Children.Add((UIElement) element1);
      EscPopup escPopup = new EscPopup();
      escPopup.PlacementTarget = (UIElement) this;
      escPopup.StaysOpen = true;
      escPopup.Placement = PlacementMode.Relative;
      this._projectDragPopup = (Popup) escPopup;
    }

    private void ShowOrHideCollapse(bool show)
    {
      if (show)
      {
        if (this._bottomBorder.Child != null)
          return;
        Border border1 = new Border();
        border1.Cursor = Cursors.Hand;
        border1.Height = 18.0;
        border1.Margin = new Thickness(0.0, -2.0, 0.0, 2.0);
        Border border2 = border1;
        border2.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle60_100");
        border2.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCollapseClick);
        Path path = UiUtils.CreatePath("ArrowLine", "ProjectMenuColorOpacity100_80", "Path01");
        path.Width = 12.0;
        path.Height = 12.0;
        path.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
        path.RenderTransform = (Transform) new RotateTransform()
        {
          Angle = 180.0
        };
        border2.Child = (UIElement) path;
        this._bottomBorder.Child = (UIElement) border2;
      }
      else
        this._bottomBorder.Child = (UIElement) null;
    }

    public void SetData(List<ProjectPinItemViewModel> data)
    {
      if (this._projectDragPopup.IsOpen)
        return;
      this._data = data;
      foreach (ProjectPinItemViewModel pinItemViewModel in data)
        pinItemViewModel.Selected = pinItemViewModel.EntityId == this._selectedData.EntityId && pinItemViewModel.Type == this._selectedData.Type;
      this.SetModels();
    }

    private void SetModels()
    {
      if (this._countInLine == 0)
        this._countInLine = ((int) this.ActualWidth - 14) / 46;
      List<ProjectPinItemViewModel> list = this._data.ToList<ProjectPinItemViewModel>();
      if (LocalSettings.Settings.ProjectPinFolded && this._data.Count<ProjectPinItemViewModel>() > this._countInLine * 2)
      {
        list = this._data.Take<ProjectPinItemViewModel>(this._countInLine * 2 - 1).ToList<ProjectPinItemViewModel>();
        this._moreItem = new ProjectPinItemViewModel()
        {
          IsMore = true,
          ItemWidth = this._data.Count > this._countInLine ? this._itemWidth : 46.0
        };
        list.Add(this._moreItem);
      }
      else
        this._moreItem = (ProjectPinItemViewModel) null;
      foreach (ProjectPinItemViewModel pinItemViewModel in this._data)
        pinItemViewModel.ItemWidth = this._data.Count > this._countInLine ? this._itemWidth : 46.0;
      this.ShowOrHideCollapse(!LocalSettings.Settings.ProjectPinFolded && this._data.Count > this._countInLine * 2);
      ItemsSourceHelper.SetItemsSource<ProjectPinItemViewModel>((ItemsControl) this._listView, list);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      this._listView.ItemsSource = (IEnumerable) null;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this._countInLine = (int) ((this.ActualWidth - 14.0) / 46.0);
      this._itemWidth = (double) (int) ((e.NewSize.Width - (double) (this._countInLine * 2) - 14.0) / (double) this._countInLine);
      if (this._itemWidth * (double) this._countInLine > e.NewSize.Width)
        --this._itemWidth;
      if (this._data == null || this._data.Count == 0)
        return;
      if (this._countInLine != (int) Math.Floor((e.PreviousSize.Width - 14.0) / 46.0))
      {
        this.SetModels();
      }
      else
      {
        foreach (ProjectPinItemViewModel pinItemViewModel in this._data)
          pinItemViewModel.ItemWidth = this._data.Count > this._countInLine ? this._itemWidth : 46.0;
        if (this._moreItem == null)
          return;
        this._moreItem.ItemWidth = this._data.Count > this._countInLine ? this._itemWidth : 46.0;
      }
    }

    private void OnCollapseClick(object sender, MouseButtonEventArgs e)
    {
      this.SetCollapseStatus(true);
      e.Handled = true;
    }

    private void SetCollapseStatus(bool collapse)
    {
      LocalSettings.Settings.ProjectPinFolded = collapse;
      this._listView.MaxHeight = LocalSettings.Settings.ProjectPinFolded ? 120.0 : 240.0;
      this.SetModels();
    }

    private void DragMouseUp(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      if (!this._projectDragPopup.IsOpen || !(this._projectDragPopup.DataContext is ProjectPinItemViewModel dataContext))
        return;
      dataContext.Dragging = false;
      this._projectDragPopup.DataContext = (object) null;
      this._projectDragPopup.IsOpen = false;
      this._mouseDownModel = (ProjectPinItemViewModel) null;
      int num = this._data.IndexOf(dataContext);
      if (this._startDragIndex == num)
        return;
      ProjectPinItemViewModel pinItemViewModel1 = num > 0 ? this._data[num - 1] : (ProjectPinItemViewModel) null;
      ProjectPinItemViewModel pinItemViewModel2 = num < this._data.Count - 1 ? this._data[num + 1] : (ProjectPinItemViewModel) null;
      if (pinItemViewModel1 == null && pinItemViewModel2 == null)
        return;
      dataContext.SortOrder = pinItemViewModel1 != null ? (pinItemViewModel2 != null ? pinItemViewModel1.SortOrder + (pinItemViewModel2.SortOrder - pinItemViewModel1.SortOrder) / 2L : pinItemViewModel1.SortOrder + 268435456L) : pinItemViewModel2.SortOrder - 268435456L;
      ProjectPinSortOrderService.UpdateProjectPinModel(dataContext.SyncEntityId, dataContext.Type, dataContext.SortOrder);
    }

    public void OnItemMouseDown(ProjectPinItemViewModel model) => this._mouseDownModel = model;

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (!this._projectDragPopup.IsOpen && e.LeftButton == MouseButtonState.Pressed)
      {
        if (this._mouseDownModel == null || this._mouseDownModel.IsMore)
          return;
        this._mouseDownModel.Dragging = true;
        this._projectDragPopup.DataContext = (object) this._mouseDownModel;
        this._projectDragPopup.Child = (UIElement) new ProjectPinItemView()
        {
          InPopup = true
        };
        this._projectDragPopup.IsOpen = true;
        System.Windows.Point position = e.GetPosition((IInputElement) this);
        this._projectDragPopup.HorizontalOffset = position.X - 18.0;
        this._projectDragPopup.VerticalOffset = position.Y - 24.0;
        this._startDragIndex = this._data.IndexOf(this._mouseDownModel);
        Mouse.Capture((IInputElement) this);
      }
      else if (this._projectDragPopup.IsOpen && e.LeftButton == MouseButtonState.Pressed)
      {
        System.Windows.Point position1 = e.GetPosition((IInputElement) this);
        this._projectDragPopup.HorizontalOffset = position1.X - 28.0;
        this._projectDragPopup.VerticalOffset = position1.Y - 34.0;
        ListBoxItem mousePointElement = Utils.GetMousePointElement<ListBoxItem>(e, (FrameworkElement) this);
        if (mousePointElement == null || !(mousePointElement.DataContext is ProjectPinItemViewModel dataContext1) || dataContext1.IsMore || !(this._projectDragPopup.DataContext is ProjectPinItemViewModel dataContext2) || dataContext1.Equals((object) dataContext2) || !(this._listView.ItemsSource is ObservableCollection<ProjectPinItemViewModel> itemsSource))
          return;
        System.Windows.Point position2 = e.GetPosition((IInputElement) mousePointElement);
        itemsSource.Remove(dataContext2);
        this._data.Remove(dataContext2);
        int num = itemsSource.IndexOf(dataContext1);
        if (num >= 0)
        {
          itemsSource.Insert(position2.X > 23.0 ? num + 1 : num, dataContext2);
          this._data.Insert(position2.X > 23.0 ? num + 1 : num, dataContext2);
        }
        else
        {
          itemsSource.Insert(0, dataContext2);
          this._data.Insert(0, dataContext2);
        }
      }
      else
        this._mouseDownModel = (ProjectPinItemViewModel) null;
    }

    public void SetSelectedProject(ProjectItemViewModel vm)
    {
      if (vm == null || this._selectedData == null)
        return;
      this._selectedData.EntityId = vm.Id;
      switch (vm.ListType)
      {
        case ProjectListType.Smart:
          this._selectedData.Type = 11;
          break;
        case ProjectListType.Project:
          this._selectedData.Type = 5;
          break;
        case ProjectListType.Group:
          this._selectedData.Type = 6;
          break;
        case ProjectListType.Tag:
          this._selectedData.Type = 7;
          break;
        case ProjectListType.Filter:
          this._selectedData.Type = 8;
          break;
        case ProjectListType.Calendar:
          this._selectedData.Type = 9;
          break;
      }
      if (this._data == null)
        return;
      foreach (ProjectPinItemViewModel pinItemViewModel in this._data.Where<ProjectPinItemViewModel>((Func<ProjectPinItemViewModel, bool>) (model => model != null)))
      {
        if (pinItemViewModel.EntityId == this._selectedData.EntityId && pinItemViewModel.Type == this._selectedData.Type)
          pinItemViewModel.Selected = true;
        else if (pinItemViewModel.Selected)
          pinItemViewModel.Selected = false;
      }
    }

    public void OnItemSelect(ProjectPinItemViewModel model)
    {
      if (model.IsMore)
      {
        this.SetCollapseStatus(false);
      }
      else
      {
        string type = "project";
        string str = "project";
        switch (model.Type)
        {
          case 6:
            str = "group";
            type = "group";
            break;
          case 7:
            str = "tag";
            type = "tag";
            break;
          case 8:
            str = "filter";
            type = "filter";
            break;
          case 9:
            str = model.IsBindAccount ? "bind_account" : "subscribe_calendar";
            type = "calendar";
            break;
          case 11:
            str = "smart";
            type = "smart";
            break;
        }
        Utils.FindParent<ProjectListView>((DependencyObject) this)?.NavigateProject(type, model.EntityId);
        Utils.FindParent<IListViewParent>((DependencyObject) this).SaveSelectedProject(str + ":" + model.EntityId);
        foreach (ProjectPinItemViewModel pinItemViewModel in this._data)
        {
          if (pinItemViewModel.Selected)
            pinItemViewModel.Selected = false;
        }
        model.Selected = true;
      }
    }

    public void OnItemUnpin(ProjectPinItemViewModel model)
    {
      this._data.Remove(model);
      this.SetModels();
      Utils.FindParent<ProjectListView>((DependencyObject) this)?.LoadPinModels();
    }

    public bool IsEmpty() => this._data == null || this._data.Count == 0;
  }
}
