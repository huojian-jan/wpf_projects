// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.ProjectOrGroupEditDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class ProjectOrGroupEditDialog : ConditionEditDialogBase<string>
  {
    private readonly LogicType _logic;
    private readonly List<ProjectModel> _projects;
    private readonly bool _showAll;
    private List<string> _originalGroupIds;
    private List<string> _originalProjectIds;

    public ProjectOrGroupEditDialog(
      bool showAll,
      List<string> projectIds,
      List<string> groupIds,
      LogicType logicType = LogicType.Or)
    {
      this.InitializeComponent();
      this._showAll = showAll;
      this._projects = CacheManager.GetProjects();
      this._originalProjectIds = projectIds.ToList<string>();
      this._originalGroupIds = groupIds.ToList<string>();
      this._logic = logicType;
      this.InitData(projectIds, groupIds);
    }

    private ProjectOrGroupEditDialog(bool showAll)
      : this(showAll, new List<string>(), new List<string>())
    {
    }

    public ProjectOrGroupEditDialog()
      : this(false)
    {
    }

    public event EventHandler<List<string>> OnSelectedProjectChanged;

    public event EventHandler<List<string>> OnSelectedGroupChanged;

    private void InitData(List<string> projectIds, List<string> groupIds)
    {
      string str = "inbox" + LocalSettings.Settings.LoginUserId;
      List<FilterListItemViewModel> listItemViewModelList = new List<FilterListItemViewModel>()
      {
        new FilterListItemViewModel()
        {
          Title = Utils.GetString("Inbox"),
          Icon = "IcInboxProject",
          IsSecondLevel = false,
          SortOrder = long.MinValue,
          Value = (object) str,
          GroupId = "NONE",
          ShowIcon = true,
          Selected = projectIds.Contains(str)
        }
      };
      List<SelectableItemViewModel> source = ProjectDataAssembler.AssembleProjects();
      if (source.Any<SelectableItemViewModel>())
        listItemViewModelList.AddRange(source.Where<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (project => !(project is ticktick_WPF.Views.Misc.SubProjectViewModel))).Select<SelectableItemViewModel, FilterListItemViewModel>((Func<SelectableItemViewModel, FilterListItemViewModel>) (project => FilterListItemViewModel.BuildItem(project, projectIds, groupIds))));
      ObservableCollection<FilterListItemViewModel> observableCollection = new ObservableCollection<FilterListItemViewModel>();
      foreach (FilterListItemViewModel listItemViewModel in listItemViewModelList)
      {
        observableCollection.Add(listItemViewModel);
        if (listItemViewModel.IsProjectGroup && listItemViewModel.Unfold)
          listItemViewModel.Children?.ForEach(new Action<FilterListItemViewModel>(((Collection<FilterListItemViewModel>) observableCollection).Add));
      }
      bool flag = projectIds.Contains("Calendar5959a2259161d16d23a4f272");
      if (!flag)
        flag = CacheManager.GetSubscribeCalendars().Any<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => cal.Show != "hidden"));
      if (!flag)
        flag = CacheManager.GetBindCalendars().Any<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Show != "hidden"));
      if (flag)
      {
        observableCollection.Add(new FilterListItemViewModel()
        {
          IsSplit = true
        });
        observableCollection.Add(FilterListItemViewModel.BuildItem((SelectableItemViewModel) new FilterCalendarViewModel(), projectIds, groupIds));
      }
      this.ViewModel = new FilterConditionViewModel()
      {
        Type = CondType.Lists,
        ItemsSource = observableCollection,
        ShowAll = this._showAll,
        IsAllSelected = projectIds.Count == 0 && groupIds.Count == 0,
        Logic = this._logic,
        SupportedLogic = new List<LogicType>()
        {
          LogicType.Or,
          LogicType.Not
        }
      };
      this.OnSelectedValueChanged += (EventHandler<FilterConditionViewModel>) ((sender, obj) =>
      {
        List<string> selectedGroupIds = this.GetSelectedGroups();
        List<string> selectedValues = this.GetSelectedValues();
        List<string> list = selectedValues.Select<string, ProjectModel>((Func<string, ProjectModel>) (projectId => this._projects.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId)))).Where<ProjectModel>((Func<ProjectModel, bool>) (project => project != null)).Where<ProjectModel>((Func<ProjectModel, bool>) (project => !selectedGroupIds.Contains(project.groupId))).Select<ProjectModel, string>((Func<ProjectModel, string>) (project => project.id)).ToList<string>();
        if (selectedValues.Contains("Calendar5959a2259161d16d23a4f272"))
          list.Add("Calendar5959a2259161d16d23a4f272");
        EventHandler<List<string>> selectedProjectChanged = this.OnSelectedProjectChanged;
        if (selectedProjectChanged != null)
          selectedProjectChanged((object) this, list);
        EventHandler<List<string>> selectedGroupChanged = this.OnSelectedGroupChanged;
        if (selectedGroupChanged == null)
          return;
        selectedGroupChanged((object) this, selectedGroupIds);
      });
    }

    protected override bool CanSave()
    {
      return this.GetSelectedValues().Count > 0 || this.GetSelectedGroups().Count > 0 || base.CanSave();
    }

    protected override void SyncOriginal()
    {
      this._originalGroupIds = this.GetSelectedGroups().ToList<string>();
      this._originalProjectIds = this.GetSelectedValues().ToList<string>();
    }

    public override void Restore()
    {
      this.RestoreSetSelected();
      EventHandler<List<string>> selectedProjectChanged = this.OnSelectedProjectChanged;
      if (selectedProjectChanged != null)
        selectedProjectChanged((object) this, this._originalProjectIds);
      EventHandler<List<string>> selectedGroupChanged = this.OnSelectedGroupChanged;
      if (selectedGroupChanged == null)
        return;
      selectedGroupChanged((object) this, this._originalGroupIds);
    }

    private void RestoreSetSelected()
    {
      foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this.ViewModel.ItemsSource)
        listItemViewModel.Selected = this._originalProjectIds.Contains(listItemViewModel.Value?.ToString()) || this._originalGroupIds.Contains(listItemViewModel.GroupId) || this._originalGroupIds.Contains(listItemViewModel.Value?.ToString());
    }

    private List<string> GetSelectedGroups()
    {
      List<string> selectedGroups = new List<string>();
      foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this.ViewModel.ItemsSource)
      {
        if (listItemViewModel.Selected && listItemViewModel.IsProjectGroup)
          selectedGroups.Add(listItemViewModel.Value.ToString());
      }
      return selectedGroups;
    }

    public static string GetDisplayProjectText(
      List<string> selectedProjectIds,
      List<string> selectedGroupIds)
    {
      if (selectedProjectIds.Count == 0 && selectedGroupIds.Count == 0)
        return Utils.GetString("All");
      List<ProjectModel> projects = CacheManager.GetProjects();
      if (projects == null || projects.Count <= 0)
        return Utils.GetString("All");
      List<ProjectModel> list = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => selectedProjectIds.Contains(p.id) || selectedGroupIds.Contains(p.groupId))).Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsValid())).OrderBy<ProjectModel, long>((Func<ProjectModel, long>) (p => p.sortOrder)).ToList<ProjectModel>();
      if (selectedProjectIds.Contains("Calendar5959a2259161d16d23a4f272"))
        list.Add(new ProjectModel()
        {
          id = "Calendar5959a2259161d16d23a4f272",
          name = Utils.GetString("Calendar")
        });
      if (list.Count > 3)
      {
        string str = (list.Count - 3).ToString();
        return string.Format(Utils.GetString("ProjectDisplayMessage"), (object) ProjectOrGroupEditDialog.GetDisplayProjectText(list.Take<ProjectModel>(3).ToList<ProjectModel>()), (object) str);
      }
      return list.Count <= 0 ? Utils.GetString("Expired") : ProjectOrGroupEditDialog.GetDisplayProjectText(list);
    }

    private static string GetDisplayProjectText(List<ProjectModel> projects)
    {
      if (projects.Count == 0)
        return Utils.GetString("All");
      string displayProjectText = string.Empty;
      for (int index = 0; index < projects.Count; ++index)
        displayProjectText = index >= projects.Count - 1 ? displayProjectText + ProjectOrGroupEditDialog.GetProjectName(projects[index]) : displayProjectText + ProjectOrGroupEditDialog.GetProjectName(projects[index]) + ", ";
      return displayProjectText;
    }

    private static string GetProjectName(ProjectModel project)
    {
      if (project == null)
        return string.Empty;
      return !project.Isinbox ? project.name : Utils.GetString("Inbox");
    }
  }
}
