// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.ProjectPinnedControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.MainListView.ProjectList;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class ProjectPinnedControl : UserControl, IComponentConnector, IStyleConnector
  {
    private readonly ProjectPinSortOrderModel _selectedData = new ProjectPinSortOrderModel();
    private List<ProjectPinItemViewModel> _data;
    private Border _mouseDownBd;
    private int _startDragIndex;
    private ProjectPinItemViewModel _needUnpinModel;
    private int _itemWidth = 46;
    private int _countInLine;
    private ProjectPinItemViewModel _moreItem;
    internal ListView ProjectPinnedList;
    internal Popup ProjectDragPopup;
    internal Grid BottomGrid;
    internal Border BottomBorder;
    internal Border FoldBorder;
    internal EscPopup UnPinPopup;
    private bool _contentLoaded;

    public ProjectPinnedControl()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
    }

    public void AddPinModel<T>(ProjectPinSortOrderModel pinSortOrderModel, T model)
    {
    }

    public void RemovePinModel(string id, int type)
    {
    }

    public void SetSelectedProject(ProjectIdentity identity)
    {
      if (identity == null || this._selectedData == null)
        return;
      switch (identity)
      {
        case NormalProjectIdentity normalProjectIdentity:
          this._selectedData.EntityId = normalProjectIdentity.Id;
          this._selectedData.Type = 5;
          break;
        case GroupProjectIdentity groupProjectIdentity:
          this._selectedData.EntityId = groupProjectIdentity.Id;
          this._selectedData.Type = 6;
          break;
        case TagProjectIdentity tagProjectIdentity:
          this._selectedData.EntityId = tagProjectIdentity.Id;
          this._selectedData.Type = 7;
          break;
        case FilterProjectIdentity filterProjectIdentity:
          this._selectedData.EntityId = filterProjectIdentity.Id;
          this._selectedData.Type = 8;
          break;
        case BindAccountCalendarProjectIdentity _:
        case SubscribeCalendarProjectIdentity _:
          this._selectedData.EntityId = identity.Id;
          this._selectedData.Type = 9;
          break;
        case CompletedProjectIdentity _:
        case AbandonedProjectIdentity _:
        case TrashProjectIdentity _:
          this._selectedData.EntityId = identity.Id;
          this._selectedData.Type = 11;
          break;
        default:
          this._selectedData.EntityId = string.Empty;
          break;
      }
      if (this._data == null)
        return;
      foreach (ProjectPinItemViewModel pinItemViewModel in this._data.Where<ProjectPinItemViewModel>((Func<ProjectPinItemViewModel, bool>) (model => model != null)))
        pinItemViewModel.Selected = pinItemViewModel.EntityId == this._selectedData.EntityId && pinItemViewModel.Type == this._selectedData.Type;
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Border border) || !(border.DataContext is ProjectPinItemViewModel dataContext) || this._mouseDownBd == null || !border.Equals((object) this._mouseDownBd))
        return;
      this._mouseDownBd = (Border) null;
      string type = "project";
      string str = "project";
      switch (dataContext.Type)
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
          str = dataContext.IsBindAccount ? "bind_account" : "subscribe_calendar";
          type = "calendar";
          break;
        case 11:
          str = "smart";
          type = "calendar";
          break;
      }
      App.Window.NavigateProject(type, dataContext.EntityId);
      Utils.FindParent<IListViewParent>((DependencyObject) this).SaveSelectedProject(str + ":" + dataContext.EntityId);
    }

    private void OnFoldClick(object sender, MouseButtonEventArgs e)
    {
    }

    private void DragMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!this.ProjectDragPopup.IsOpen || !(this.ProjectDragPopup.DataContext is ProjectPinItemViewModel dataContext))
        return;
      dataContext.Dragging = false;
      this.ProjectDragPopup.DataContext = (object) null;
      this.ProjectDragPopup.IsOpen = false;
      this._mouseDownBd = (Border) null;
      int num = this._data.IndexOf(dataContext);
      if (this._startDragIndex == num)
        return;
      ProjectPinItemViewModel pinItemViewModel1 = num > 0 ? this._data[num - 1] : (ProjectPinItemViewModel) null;
      ProjectPinItemViewModel pinItemViewModel2 = num < this._data.Count - 1 ? this._data[num + 1] : (ProjectPinItemViewModel) null;
      if (pinItemViewModel1 == null && pinItemViewModel2 == null)
        return;
      dataContext.SortOrder = pinItemViewModel1 != null ? (pinItemViewModel2 != null ? pinItemViewModel1.SortOrder + (pinItemViewModel2.SortOrder - pinItemViewModel1.SortOrder) / 2L : pinItemViewModel1.SortOrder + 268435456L) : pinItemViewModel2.SortOrder - 268435456L;
      ProjectPinSortOrderService.UpdateProjectPinModel(dataContext.EntityId, dataContext.Type, dataContext.SortOrder);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/project/projectpinnedcontrol.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.DragMouseUp);
          break;
        case 2:
          this.ProjectPinnedList = (ListView) target;
          break;
        case 5:
          this.ProjectDragPopup = (Popup) target;
          break;
        case 6:
          this.BottomGrid = (Grid) target;
          break;
        case 7:
          this.BottomBorder = (Border) target;
          break;
        case 8:
          this.FoldBorder = (Border) target;
          this.FoldBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFoldClick);
          break;
        case 9:
          this.UnPinPopup = (EscPopup) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 3)
      {
        if (connectionId != 4)
          return;
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFoldClick);
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
    }
  }
}
