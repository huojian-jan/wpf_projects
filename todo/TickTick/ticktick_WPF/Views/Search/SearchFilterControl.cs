// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchFilterControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Filter;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchFilterControl : UserControl, IComponentConnector
  {
    private bool _isEditingTime;
    private AssigneeEditDialog _assignEditDialog;
    private PriorityEditDialog _priorityEditDialog;
    internal Grid TopContainer;
    internal PopupPlacementBorder DateFilterText;
    internal EscPopup DateSelectPopup;
    internal PopupPlacementBorder ProjectOrGroupFilterText;
    internal EscPopup ListFilterPopup;
    internal PopupPlacementBorder TagFilterText;
    internal EscPopup TagFilterPopup;
    internal PopupPlacementBorder StatusFilterText;
    internal EscPopup StatusFilterPopup;
    internal PopupPlacementBorder PriorityFilter;
    internal EscPopup PriorityFilterPopup;
    internal PopupPlacementBorder AssignFilter;
    internal EscPopup AssignFilterPopup;
    internal PopupPlacementBorder TaskTypeFilter;
    internal EscPopup TaskTypeFilterPopup;
    internal PopupPlacementBorder MoreFilter;
    internal EscPopup MoreFilterPopup;
    private bool _contentLoaded;

    public SearchFilterControl()
    {
      this.InitializeComponent();
      SearchFilterControl.ViewModel.AssignFilterEnable = CacheManager.GetProjects().Any<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsShareList()));
      SearchFilterControl.ViewModel.TaskTypeFilterEnable = LocalSettings.Settings.IsNoteEnabled;
      this.DataContext = (object) SearchFilterControl.ViewModel;
    }

    private static SearchFilterViewModel ViewModel => SearchHelper.SearchFilter;

    public bool PopopOpened
    {
      get
      {
        return this.DateSelectPopup.IsOpen || this.ListFilterPopup.IsOpen || this.TagFilterPopup.IsOpen || this.StatusFilterPopup.IsOpen || this.PriorityFilterPopup.IsOpen || this.AssignFilterPopup.IsOpen || this.TaskTypeFilterPopup.IsOpen || this.MoreFilterPopup.IsOpen;
      }
    }

    public event EventHandler FilterChanged;

    private void StatusFilterClick(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.AdvancedSearch))
        return;
      DropdownDialog dropdownDialog = new DropdownDialog()
      {
        ItemsSource = new List<ListItemData>()
        {
          new ListItemData((object) StatusFilter.All, Utils.GetString("AllStatus")),
          new ListItemData((object) StatusFilter.Uncompleted, Utils.GetString("Uncompleted")),
          new ListItemData((object) StatusFilter.Completed, Utils.GetString("Completed")),
          new ListItemData((object) StatusFilter.Abandoned, Utils.GetString("Abandoned"))
        }
      };
      foreach (ListItemData listItemData in dropdownDialog.ItemsSource)
      {
        if ((StatusFilter) listItemData.Key == SearchFilterControl.ViewModel.StatusFilter)
          listItemData.Selected = true;
      }
      dropdownDialog.OnItemSelected += (EventHandler<ListItemData>) ((obj, item) =>
      {
        SearchFilterControl.ViewModel.StatusFilter = (StatusFilter) item.Key;
        this.StatusFilterPopup.IsOpen = false;
        SearchFilterControl.ViewModel.Searched = false;
        SearchHelper.ClearTaskSearchModels();
        EventHandler filterChanged = this.FilterChanged;
        if (filterChanged != null)
          filterChanged((object) this, (EventArgs) null);
        UserActCollectUtils.AddClickEvent("search", "filter", "status");
      });
      this.StatusFilterPopup.Child = (UIElement) dropdownDialog;
      this.StatusFilterPopup.IsOpen = true;
    }

    private void DateFilterClick(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.AdvancedSearch))
        return;
      TimeFilterDialog timeFilterDialog = new TimeFilterDialog()
      {
        ItemsSource = new List<ListItemData>()
        {
          new ListItemData((object) DateFilter.All, Utils.GetString("AllDate")),
          new ListItemData((object) DateFilter.ThisWeek, Utils.GetString("ThisWeek")),
          new ListItemData((object) DateFilter.ThisMonth, Utils.GetString("ThisMonth")),
          new ListItemData((object) DateFilter.Custom, Utils.GetString("Custom"))
        }
      };
      timeFilterDialog.SetStartDate(SearchFilterControl.ViewModel.StartDate);
      timeFilterDialog.SetEndDate(SearchFilterControl.ViewModel.EndDate);
      foreach (ListItemData listItemData in timeFilterDialog.ItemsSource)
      {
        if ((DateFilter) listItemData.Key == SearchFilterControl.ViewModel.DateFilter)
          listItemData.Selected = true;
      }
      timeFilterDialog.OnFilterSelect += (EventHandler<DateFilterData>) ((obj, item) =>
      {
        SearchFilterControl.ViewModel.DateFilter = item.Type;
        if (item.StartDate.HasValue)
          SearchFilterControl.ViewModel.StartDate = new DateTime?(item.StartDate.Value);
        if (item.EndDate.HasValue)
          SearchFilterControl.ViewModel.EndDate = new DateTime?(item.EndDate.Value);
        this.DateSelectPopup.IsOpen = false;
        SearchHelper.ClearTaskSearchModels();
        SearchFilterControl.ViewModel.Searched = false;
        UserActCollectUtils.AddClickEvent("search", "filter", "date");
        EventHandler filterChanged = this.FilterChanged;
        if (filterChanged == null)
          return;
        filterChanged((object) this, (EventArgs) null);
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

    private async void ProjectOrGroupFilterClick(object sender, MouseButtonEventArgs e)
    {
      SearchFilterControl searchFilterControl = this;
      if (!ProChecker.CheckPro(ProType.AdvancedSearch))
        return;
      ProjectExtra data = new ProjectExtra()
      {
        ProjectIds = SearchFilterControl.ViewModel.SelectedProjectIds,
        GroupIds = SearchFilterControl.ViewModel.SelectedProjectGroupIds
      };
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) searchFilterControl.ListFilterPopup, data, new ProjectSelectorExtra());
      projectOrGroupPopup.Save += new EventHandler<ProjectExtra>(searchFilterControl.OnProjectSelect);
      projectOrGroupPopup.Show();
    }

    private async void OnProjectSelect(object sender, ProjectExtra data)
    {
      SearchFilterControl sender1 = this;
      if (!data.IsAll)
      {
        SearchFilterControl.ViewModel.SelectedProjectIds = data.ProjectIds;
        SearchFilterControl.ViewModel.SelectedProjectGroupIds = data.GroupIds;
        SearchFilterControl.ViewModel.SelectedProjectDisplayText = ProjectExtra.FormatDisplayText(ProjectExtra.Serialize(data));
        SearchFilterControl.ViewModel.ProjectOrGroupFilter = ProjectOrGroupFilter.Custom;
        UserActCollectUtils.AddClickEvent("search", "filter", "list");
      }
      else
      {
        SearchFilterControl.ViewModel.SelectedProjectIds = new List<string>();
        SearchFilterControl.ViewModel.SelectedProjectGroupIds = new List<string>();
        SearchFilterControl.ViewModel.SelectedProjectDisplayText = Utils.GetString("AllList");
        SearchFilterControl.ViewModel.ProjectOrGroupFilter = ProjectOrGroupFilter.All;
      }
      SearchHelper.ClearTaskSearchModels();
      SearchFilterControl.ViewModel.Searched = false;
      EventHandler filterChanged = sender1.FilterChanged;
      if (filterChanged == null)
        return;
      filterChanged((object) sender1, (EventArgs) null);
    }

    public void Reset()
    {
      SearchFilterControl.ViewModel.AssignFilterEnable = CacheManager.GetProjects().Any<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsShareList()));
      SearchFilterControl.ViewModel.TaskTypeFilterEnable = LocalSettings.Settings.IsNoteEnabled;
      this.DataContext = (object) SearchFilterControl.ViewModel;
    }

    private void TagFilterClick(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.AdvancedSearch))
        return;
      TagSearchFilterControl searchFilterControl = new TagSearchFilterControl(SearchFilterControl.ViewModel.SelectedTags);
      searchFilterControl.Cancel -= new EventHandler(this.OnTagFilterCancel);
      searchFilterControl.Cancel += new EventHandler(this.OnTagFilterCancel);
      searchFilterControl.Save -= new EventHandler<List<string>>(this.OnTagFilterSelected);
      searchFilterControl.Save += new EventHandler<List<string>>(this.OnTagFilterSelected);
      this.TagFilterPopup.Child = (UIElement) searchFilterControl;
      this.TagFilterPopup.IsOpen = true;
    }

    private void OnTagFilterSelected(object sender, List<string> tags)
    {
      this.TagFilterPopup.IsOpen = false;
      SearchFilterControl.ViewModel.SelectedTags = tags.Select<string, string>((Func<string, string>) (t => TagDataHelper.GetTagDisplayName(t))).ToList<string>();
      SearchFilterControl.ViewModel.SelectedTagDisplayText = TagCardViewModel.ToNormalDisplayText(tags);
      SearchHelper.ClearTaskSearchModels();
      SearchFilterControl.ViewModel.Searched = false;
      UserActCollectUtils.AddClickEvent("search", "filter", "tag");
      EventHandler filterChanged = this.FilterChanged;
      if (filterChanged == null)
        return;
      filterChanged((object) this, (EventArgs) null);
    }

    private void OnTagFilterCancel(object sender, EventArgs e)
    {
      this.TagFilterPopup.IsOpen = false;
      SearchFilterControl.ViewModel.SelectedTags = new List<string>();
      SearchFilterControl.ViewModel.SelectedTagDisplayText = Utils.GetString("AllTags");
      SearchHelper.ClearTaskSearchModels();
      SearchFilterControl.ViewModel.Searched = false;
      EventHandler filterChanged = this.FilterChanged;
      if (filterChanged == null)
        return;
      filterChanged((object) this, (EventArgs) null);
    }

    private void EditSearchClick(object sender, MouseButtonEventArgs e)
    {
      App.Window.StartSearch(true);
    }

    private void CloseSearch(object sender, MouseButtonEventArgs e) => App.Window.StopSearch();

    private void PriorityFilterClick(object sender, MouseButtonEventArgs e)
    {
      this._priorityEditDialog = new PriorityEditDialog(SearchFilterControl.ViewModel.SelectedPriorities ?? new List<int>(), true);
      this._priorityEditDialog.OnSelectedPriorityChanged += (EventHandler<List<int>>) ((s, priorities) => SearchFilterControl.ViewModel.SelectedPriorities = priorities);
      this.PriorityFilterPopup.Closed -= new EventHandler(this.PriorityPopup_Closed);
      this.PriorityFilterPopup.Closed += new EventHandler(this.PriorityPopup_Closed);
      this.PriorityFilterPopup.Child = (UIElement) this._priorityEditDialog;
      this._priorityEditDialog.OnCancel += (EventHandler) ((s, arg) => this.PriorityFilterPopup.IsOpen = false);
      this._priorityEditDialog.OnSave += (EventHandler<FilterConditionViewModel>) ((s, model) =>
      {
        this.PriorityFilterPopup.IsOpen = false;
        SearchHelper.ClearTaskSearchModels();
        UserActCollectUtils.AddClickEvent("search", "filter", "priority");
        EventHandler filterChanged = this.FilterChanged;
        if (filterChanged == null)
          return;
        filterChanged((object) this, (EventArgs) null);
      });
      this.PriorityFilterPopup.IsOpen = true;
    }

    private void PriorityPopup_Closed(object sender, EventArgs e)
    {
      if (!this._priorityEditDialog.IsSave)
        this._priorityEditDialog.Restore();
      this._priorityEditDialog.IsSave = false;
    }

    private void AssignFilterClick(object sender, MouseButtonEventArgs e)
    {
      this._assignEditDialog = new AssigneeEditDialog(SearchFilterControl.ViewModel.SelectedAssignees ?? new List<string>(), true);
      this._assignEditDialog.OnSelectedAssigneeChanged += (EventHandler<List<string>>) ((s, assignees) => SearchFilterControl.ViewModel.SelectedAssignees = assignees);
      this.AssignFilterPopup.Closed -= new EventHandler(this.AssigneePopup_Closed);
      this.AssignFilterPopup.Closed += new EventHandler(this.AssigneePopup_Closed);
      this.AssignFilterPopup.Child = (UIElement) this._assignEditDialog;
      this._assignEditDialog.OnCancel += (EventHandler) ((s, arg) => this.AssignFilterPopup.IsOpen = false);
      this._assignEditDialog.OnSave += (EventHandler<FilterConditionViewModel>) ((s, model) =>
      {
        this.AssignFilterPopup.IsOpen = false;
        SearchHelper.ClearTaskSearchModels();
        UserActCollectUtils.AddClickEvent("search", "filter", "assignee");
        EventHandler filterChanged = this.FilterChanged;
        if (filterChanged == null)
          return;
        filterChanged((object) this, (EventArgs) null);
      });
      this.AssignFilterPopup.IsOpen = true;
    }

    private void AssigneePopup_Closed(object sender, EventArgs e)
    {
      if (!this._assignEditDialog.IsSave)
        this._assignEditDialog.Restore();
      this._assignEditDialog.IsSave = false;
    }

    private void TaskTypeFilterClick(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.AdvancedSearch))
        return;
      DropdownDialog dropdownDialog = new DropdownDialog()
      {
        ItemsSource = new List<ListItemData>()
        {
          new ListItemData((object) TaskType.TaskAndNote, Utils.GetString("All")),
          new ListItemData((object) TaskType.Task, Utils.GetString("Task")),
          new ListItemData((object) TaskType.Note, Utils.GetString("Notes"))
        }
      };
      foreach (ListItemData listItemData in dropdownDialog.ItemsSource)
      {
        if ((TaskType) listItemData.Key == SearchFilterControl.ViewModel.SelectedType)
          listItemData.Selected = true;
      }
      dropdownDialog.OnItemSelected += (EventHandler<ListItemData>) ((obj, item) =>
      {
        SearchFilterControl.ViewModel.SelectedType = (TaskType) item.Key;
        this.TaskTypeFilterPopup.IsOpen = false;
        SearchHelper.ClearTaskSearchModels();
        UserActCollectUtils.AddClickEvent("search", "filter", "type");
        EventHandler filterChanged = this.FilterChanged;
        if (filterChanged == null)
          return;
        filterChanged((object) this, (EventArgs) null);
      });
      this.TaskTypeFilterPopup.Child = (UIElement) dropdownDialog;
      this.TaskTypeFilterPopup.IsOpen = true;
    }

    private void ShowFilterClick(object sender, object e)
    {
      if (!(e is string str))
        return;
      switch (str)
      {
        case "priority":
          SearchFilterControl.ViewModel.ShowPriorityFilter = true;
          break;
        case "assign":
          SearchFilterControl.ViewModel.ShowAssignFilter = true;
          break;
        case "taskType":
          SearchFilterControl.ViewModel.ShowTaskTypeFilter = true;
          break;
      }
    }

    private void MoreFilterClick(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.AdvancedSearch))
        return;
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      if (!SearchFilterControl.ViewModel.ShowPriorityFilter)
        types.Add(new CustomMenuItemViewModel((object) "priority", Utils.GetString("priority"), (Geometry) null)
        {
          ImageWidth = 0.0
        });
      if (SearchFilterControl.ViewModel.AssignFilterEnable && !SearchFilterControl.ViewModel.ShowAssignFilter)
        types.Add(new CustomMenuItemViewModel((object) "assign", Utils.GetString("assignee"), (Geometry) null)
        {
          ImageWidth = 0.0
        });
      if (SearchFilterControl.ViewModel.TaskTypeFilterEnable && !SearchFilterControl.ViewModel.ShowTaskTypeFilter)
        types.Add(new CustomMenuItemViewModel((object) "taskType", Utils.GetString("TaskType"), (Geometry) null)
        {
          ImageWidth = 0.0
        });
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this.MoreFilterPopup);
      customMenuList.Operated += new EventHandler<object>(this.ShowFilterClick);
      customMenuList.Show();
    }

    private void SaveAsFilterClick(object sender, RoutedEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.Filter))
        return;
      FilterEditDialog filterEditDialog = new FilterEditDialog(SearchHelper.SearchFilter.ToSearchFilter());
      filterEditDialog.Owner = Window.GetWindow((DependencyObject) this);
      filterEditDialog.ShowDialog();
      UserActCollectUtils.AddClickEvent("search", "action", "save_as_filter");
      if (!filterEditDialog.Saved)
        return;
      Utils.Toast(Utils.GetString("Saved"));
      SyncManager.Sync();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/search/searchfiltercontrol.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.CloseSearch);
          break;
        case 2:
          this.TopContainer = (Grid) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.EditSearchClick);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.SaveAsFilterClick);
          break;
        case 5:
          this.DateFilterText = (PopupPlacementBorder) target;
          break;
        case 6:
          this.DateSelectPopup = (EscPopup) target;
          break;
        case 7:
          this.ProjectOrGroupFilterText = (PopupPlacementBorder) target;
          break;
        case 8:
          this.ListFilterPopup = (EscPopup) target;
          break;
        case 9:
          this.TagFilterText = (PopupPlacementBorder) target;
          break;
        case 10:
          this.TagFilterPopup = (EscPopup) target;
          break;
        case 11:
          this.StatusFilterText = (PopupPlacementBorder) target;
          break;
        case 12:
          this.StatusFilterPopup = (EscPopup) target;
          break;
        case 13:
          this.PriorityFilter = (PopupPlacementBorder) target;
          break;
        case 14:
          this.PriorityFilterPopup = (EscPopup) target;
          break;
        case 15:
          this.AssignFilter = (PopupPlacementBorder) target;
          break;
        case 16:
          this.AssignFilterPopup = (EscPopup) target;
          break;
        case 17:
          this.TaskTypeFilter = (PopupPlacementBorder) target;
          break;
        case 18:
          this.TaskTypeFilterPopup = (EscPopup) target;
          break;
        case 19:
          this.MoreFilter = (PopupPlacementBorder) target;
          break;
        case 20:
          this.MoreFilterPopup = (EscPopup) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
