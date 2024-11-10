// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Completed.CompletedFilterControl
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync.ClosedLoader;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Views.Completed
{
  public class CompletedFilterControl : UserControl, IComponentConnector
  {
    private bool _isEditingTime;
    internal PopupPlacementBorder DateFilterText;
    internal EscPopup DateSelectPopup;
    internal PopupPlacementBorder ProjectOrGroupFilterText;
    internal EscPopup ProjectOrGroupFilterPopup;
    internal PopupPlacementBorder PersonalFilterText;
    internal EscPopup PersonalFilterPopup;
    private bool _contentLoaded;

    public event EventHandler<ClosedFilterViewModel> FilterChanged;

    private ClosedFilterViewModel ViewModel => this.DataContext as ClosedFilterViewModel;

    public CompletedFilterControl(bool isAbandon = false)
    {
      this.DataContext = isAbandon ? (object) AbandonedProjectIdentity.Filter : (object) CompletedProjectIdentity.Filter;
      this.ViewModel.IsCompleted = !isAbandon;
      this.InitializeComponent();
    }

    private void DateFilterClick(object sender, MouseButtonEventArgs e)
    {
      TimeFilterDialog timeFilterDialog = new TimeFilterDialog()
      {
        ItemsSource = new List<ListItemData>()
        {
          new ListItemData((object) DateFilter.All, Utils.GetString("AllDate")),
          new ListItemData((object) DateFilter.ThisWeek, Utils.GetString("ThisWeek")),
          new ListItemData((object) DateFilter.LastWeek, Utils.GetString("LastWeek")),
          new ListItemData((object) DateFilter.ThisMonth, Utils.GetString("ThisMonth")),
          new ListItemData((object) DateFilter.Custom, Utils.GetString("Custom"))
        }
      };
      timeFilterDialog.SetStartDate(this.ViewModel.StartDate);
      timeFilterDialog.SetEndDate(this.ViewModel.EndDate);
      timeFilterDialog.ItemsSource.ForEach((Action<ListItemData>) (item =>
      {
        if ((DateFilter) item.Key != this.ViewModel.DateFilter)
          return;
        item.Selected = true;
      }));
      timeFilterDialog.OnFilterSelect += (EventHandler<DateFilterData>) ((obj, item) =>
      {
        this.ViewModel.DateFilter = item.Type;
        this.ViewModel.StartDate = item.StartDate;
        this.ViewModel.EndDate = item.EndDate;
        this.DateSelectPopup.IsOpen = false;
        this.ViewModel.Changed = true;
        EventHandler<ClosedFilterViewModel> filterChanged = this.FilterChanged;
        if (filterChanged != null)
          filterChanged((object) this, this.ViewModel);
        UserActCollectUtils.AddClickEvent("tasklist", "completed_list", "filter_date");
      });
      timeFilterDialog.OnEndEditDate += (EventHandler) (async (obj, item) => this._isEditingTime = true);
      timeFilterDialog.OnCancel += (EventHandler) ((obj, arg) => this.DateSelectPopup.IsOpen = false);
      this.DateSelectPopup.Child = (UIElement) timeFilterDialog;
      this.DateSelectPopup.IsOpen = true;
      this.DateSelectPopup.Closed += (EventHandler) (async (obj, item) =>
      {
        if (!this._isEditingTime)
          return;
        await Task.Delay(1);
        this._isEditingTime = false;
      });
    }

    private void ProjectOrGroupFilterClick(object sender, MouseButtonEventArgs e)
    {
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) this.ProjectOrGroupFilterPopup, new ProjectExtra()
      {
        ProjectIds = this.ViewModel.SelectedProjectIds,
        GroupIds = this.ViewModel.SelectedProjectGroupIds
      }, new ProjectSelectorExtra());
      projectOrGroupPopup.Save += new EventHandler<ProjectExtra>(this.OnProjectSelect);
      projectOrGroupPopup.Show();
    }

    private async void OnProjectSelect(object sender, ProjectExtra data)
    {
      CompletedFilterControl sender1 = this;
      if (!data.IsAll)
      {
        sender1.ViewModel.SelectedProjectIds = data.ProjectIds;
        data.ProjectIds.ForEach((Action<string>) (id => CompletionLoadStatusDao.DeleteStatusByEntityId(id)));
        sender1.ViewModel.SelectedProjectGroupIds = data.GroupIds;
        sender1.ViewModel.SelectedProjectDisplayText = ProjectExtra.FormatDisplayText(ProjectExtra.Serialize(data));
        sender1.ViewModel.ProjectOrGroupFilter = ProjectOrGroupFilter.Custom;
        UserActCollectUtils.AddClickEvent("tasklist", "completed_list", "filter_list");
      }
      else
      {
        sender1.ViewModel.SelectedProjectIds = new List<string>();
        sender1.ViewModel.SelectedProjectGroupIds = new List<string>();
        sender1.ViewModel.SelectedProjectDisplayText = Utils.GetString("AllList");
        sender1.ViewModel.ProjectOrGroupFilter = ProjectOrGroupFilter.All;
      }
      sender1.ViewModel.Changed = true;
      EventHandler<ClosedFilterViewModel> filterChanged = sender1.FilterChanged;
      if (filterChanged == null)
        return;
      filterChanged((object) sender1, sender1.ViewModel);
    }

    private void PersonalOrAllFilterClick(object sender, MouseButtonEventArgs e)
    {
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "personal", Utils.GetString(this.ViewModel.IsCompleted ? "CompletedbyMe" : "WontDobyMe"), (Geometry) null);
      menuItemViewModel1.Selected = !this.ViewModel.ShowAll;
      types.Add(menuItemViewModel1);
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "all", Utils.GetString(this.ViewModel.IsCompleted ? "AllCompleted" : "AllWontDo"), (Geometry) null);
      menuItemViewModel2.Selected = this.ViewModel.ShowAll;
      types.Add(menuItemViewModel2);
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this.PersonalFilterPopup);
      customMenuList.Operated += new EventHandler<object>(this.OnShowAllSelected);
      customMenuList.Show();
    }

    private void OnShowAllSelected(object sender, object e)
    {
      this.PersonalFilterPopup.IsOpen = false;
      this.ViewModel.ShowAll = e == (object) "all";
      EventHandler<ClosedFilterViewModel> filterChanged = this.FilterChanged;
      if (filterChanged == null)
        return;
      filterChanged((object) this, this.ViewModel);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/completed/completedfiltercontrol.xaml", UriKind.Relative));
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
          this.DateFilterText = (PopupPlacementBorder) target;
          break;
        case 2:
          this.DateSelectPopup = (EscPopup) target;
          break;
        case 3:
          this.ProjectOrGroupFilterText = (PopupPlacementBorder) target;
          break;
        case 4:
          this.ProjectOrGroupFilterPopup = (EscPopup) target;
          break;
        case 5:
          this.PersonalFilterText = (PopupPlacementBorder) target;
          break;
        case 6:
          this.PersonalFilterPopup = (EscPopup) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
