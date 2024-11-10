// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.KanbanViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Kanban
{
  public class KanbanViewModel : BaseViewModel
  {
    private static readonly SemaphoreLocker LoadLock = new SemaphoreLocker();
    private bool _showAdd;
    private bool _enable = true;
    private string _name;
    public List<TaskBaseViewModel> SourceModels;
    public ProjectIdentity Identity;
    public readonly ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader CompletedTaskLoader = new ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader();
    private KanbanContainer _kanban;
    private bool _loading;

    public KanbanViewModel(ProjectIdentity identity, KanbanContainer kanban)
    {
      this._kanban = kanban;
      this.SetIdentity(identity);
    }

    public bool ShowAdd
    {
      get => this._showAdd && this._enable && this.SortOption.groupBy == "sortOrder";
      set
      {
        this._showAdd = value;
        this.OnPropertyChanged(nameof (ShowAdd));
      }
    }

    public bool Enable
    {
      get => this._enable;
      set
      {
        this._enable = value;
        this.OnPropertyChanged("ShowAdd");
        this.OnPropertyChanged(nameof (Enable));
      }
    }

    public string Name
    {
      get => this._name;
      set
      {
        this._name = value;
        this.OnPropertyChanged(nameof (Name));
      }
    }

    public SortOption SortOption => this.Identity.SortOption;

    public async Task LoadTasks(bool force = false, bool pullRemote = false)
    {
      if (this._loading && !force)
        return;
      this._loading = true;
      await KanbanViewModel.LoadLock.LockAsync((Func<Task>) (async () =>
      {
        this.SourceModels = await ProjectTaskDataProvider.GetDisplayModels(this.Identity);
        List<ColumnViewModel> columnViewModelList = await this.GetColumns(force, pullRemote) ?? new List<ColumnViewModel>();
        if (this.SortOption.groupBy != "sortOrder")
          columnViewModelList = columnViewModelList.Where<ColumnViewModel>((Func<ColumnViewModel, bool>) (c => c.SourceItems.Any<TaskBaseViewModel>())).ToList<ColumnViewModel>();
        columnViewModelList.Sort((Comparison<ColumnViewModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
        KanbanContainer kanban = this._kanban;
        if (kanban != null)
          await kanban.SetupColumn((IEnumerable<ColumnViewModel>) columnViewModelList);
        this._loading = false;
        if (!pullRemote)
          return;
        this.TryLoadCompletedTasks();
      }));
    }

    private async Task<List<ColumnViewModel>> GetColumns(bool force = false, bool pullRemote = false)
    {
      bool flag = this.Identity is NormalProjectIdentity;
      if (flag && (this.SortOption.groupBy == "none" || this.SortOption.groupBy == "sortOrder"))
        return await this.GetCustomColumns(force, pullRemote);
      if (!flag && this.SortOption.groupBy == "none" || this.SortOption.groupBy == "dueDate")
        return this.GetDateColumns();
      if (this.SortOption.groupBy == "priority")
        return this.GetPriorityColumns();
      if (this.SortOption.groupBy == "tag")
        return this.GetTagColumns();
      if (this.SortOption.groupBy == "project")
        return this.GetProjectColumns();
      return this.SortOption.groupBy == "assignee" ? this.GetAssignColumns() : await this.GetCustomColumns(force, pullRemote);
    }

    private List<ColumnViewModel> GetAssignColumns()
    {
      Dictionary<string, ColumnViewModel> dictionary = new Dictionary<string, ColumnViewModel>();
      dictionary.Add("assign:-1", new ColumnViewModel()
      {
        Name = Utils.GetString("NotAssigned"),
        SortOrder = long.MaxValue,
        ColumnId = "assign:-1",
        CanDropDown = true
      });
      List<Node<TaskBaseViewModel>> list = TaskCache.GetTaskNodeDict(this.SourceModels).Values.Where<Node<TaskBaseViewModel>>((Func<Node<TaskBaseViewModel>, bool>) (n => !n.HasParent)).ToList<Node<TaskBaseViewModel>>();
      List<ProjectModel> source1 = this.Identity is GroupProjectIdentity identity ? CacheManager.GetProjectsInGroup(identity.GroupId) : new List<ProjectModel>();
      List<AvatarViewModel> source2 = identity != null ? AvatarHelper.GetProjectAvatarsFromCacheInGroup(identity.Id) : AvatarHelper.GetProjectAvatarsFromCache(this.Identity?.CatId, true);
      foreach (Node<TaskBaseViewModel> node in list)
      {
        TaskBaseViewModel taskBaseViewModel = node.Value;
        if (taskBaseViewModel.Status == 0 || !LocalSettings.Settings.HideComplete)
        {
          string key = "assign:-1";
          string userId = taskBaseViewModel.Assignee;
          if (string.IsNullOrEmpty(taskBaseViewModel.Assignee) || taskBaseViewModel.Assignee == "-1")
            userId = "-1";
          if (!dictionary.ContainsKey("assign:" + userId))
          {
            AvatarViewModel avatarViewModel = userId == "-1" ? (AvatarViewModel) null : source2.FirstOrDefault<AvatarViewModel>((Func<AvatarViewModel, bool>) (u => (u.UserId ?? "") == userId));
            if (avatarViewModel != null)
            {
              bool flag = true;
              if (identity != null && source1.Any<ProjectModel>((Func<ProjectModel, bool>) (p => !p.IsEnable())))
                flag = ProjectDao.GetAssignProjectInGroup(identity.Id, userId) != null;
              key = "assign:" + avatarViewModel.UserId;
              dictionary.Add(key, new ColumnViewModel()
              {
                Name = avatarViewModel.Name,
                SortOrder = avatarViewModel.UserId == LocalSettings.Settings.LoginUserId ? long.MinValue : (long) source2.IndexOf(avatarViewModel),
                ColumnId = key,
                CanDropDown = flag,
                CanAdd = flag
              });
            }
          }
          else
            key = "assign:" + userId;
          ColumnViewModel columnViewModel = dictionary[key];
          columnViewModel.SourceItems.Add(taskBaseViewModel);
          List<Node<TaskBaseViewModel>> children = node.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
            columnViewModel.SourceItems.AddRange((IEnumerable<TaskBaseViewModel>) node.GetAllChildrenValue());
        }
      }
      foreach (ColumnViewModel columnViewModel in dictionary.Values)
        columnViewModel.SetColumnIdentity(this.Identity);
      return dictionary.Values.ToList<ColumnViewModel>();
    }

    private List<ColumnViewModel> GetTagColumns()
    {
      Dictionary<string, ColumnViewModel> dictionary = new Dictionary<string, ColumnViewModel>();
      dictionary.Add("tag:#notag", new ColumnViewModel()
      {
        Name = Utils.GetString("NoTags"),
        SortOrder = 9223372036854775806L,
        ColumnId = "tag:#notag",
        CanDropDown = false
      });
      ColumnViewModel habitColumn = ColumnViewModel.GetHabitColumn(this.Identity is WeekProjectIdentity);
      dictionary.Add(habitColumn.ColumnId, habitColumn);
      Dictionary<string, Node<TaskBaseViewModel>> taskNodeDict = TaskCache.GetTaskNodeDict(this.SourceModels);
      Dictionary<string, long> tagSortDict1 = TagDataHelper.GetTagSortDict();
      List<string> stringList = (List<string>) null;
      TagProjectIdentity tagId = this.Identity as TagProjectIdentity;
      if (tagId != null)
        stringList = TagDataHelper.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => t.parent == tagId.Tag || t.name == tagId.Tag)).Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>();
      foreach (Node<TaskBaseViewModel> node in taskNodeDict.Values)
      {
        TaskBaseViewModel taskBaseViewModel = node.Value;
        if (taskBaseViewModel.IsHabit)
        {
          if (taskBaseViewModel.Status == 0 || !LocalSettings.Settings.HideComplete)
            habitColumn.SourceItems.Add(taskBaseViewModel);
        }
        else if (!node.HasParent && (taskBaseViewModel.Status == 0 || !LocalSettings.Settings.HideComplete))
        {
          string key = "tag:#notag";
          Dictionary<string, long> tagSortDict2 = tagSortDict1;
          string[] tags = taskBaseViewModel.Tags;
          List<string> list = tags != null ? ((IEnumerable<string>) tags).ToList<string>() : (List<string>) null;
          List<string> limits = stringList;
          string lower = TagDataHelper.GetPrimaryTag(tagSortDict2, (IList<string>) list, (ICollection<string>) limits)?.ToLower();
          if (!string.IsNullOrEmpty(lower))
          {
            TagModel tagByName = CacheManager.GetTagByName(lower);
            if (tagByName != null)
            {
              key = lower;
              if (!dictionary.ContainsKey(key))
                dictionary.Add(key, new ColumnViewModel()
                {
                  Name = tagByName.GetDisplayName(),
                  SortOrder = tagByName.sortOrder,
                  ColumnId = "tag:" + key,
                  CanDropDown = false
                });
            }
          }
          ColumnViewModel columnViewModel = dictionary[key];
          columnViewModel.SourceItems.Add(taskBaseViewModel);
          List<Node<TaskBaseViewModel>> children = node.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
            columnViewModel.SourceItems.AddRange((IEnumerable<TaskBaseViewModel>) node.GetAllChildrenValue());
        }
      }
      foreach (ColumnViewModel columnViewModel in dictionary.Values)
        columnViewModel.SetColumnIdentity(this.Identity);
      return dictionary.Values.ToList<ColumnViewModel>();
    }

    private List<ColumnViewModel> GetDateColumns()
    {
      bool useInWeek = this.Identity is WeekProjectIdentity;
      bool flag1 = this.Identity is TomorrowProjectIdentity;
      bool flag2 = this.Identity is TodayProjectIdentity;
      List<ColumnViewModel> source = useInWeek ? ColumnViewModel.GetWeekDateColumnViewModels() : ColumnViewModel.GetDateColumnViewModels();
      ColumnViewModel noteColumn = ColumnViewModel.GetNoteColumn();
      source.Add(noteColumn);
      ColumnViewModel habitColumn = ColumnViewModel.GetHabitColumn(useInWeek);
      source.Add(habitColumn);
      foreach (ColumnViewModel columnViewModel in source)
        columnViewModel.SetColumnIdentity(this.Identity);
      Dictionary<string, ColumnViewModel> dictionary = source.ToDictionary<ColumnViewModel, string, ColumnViewModel>((Func<ColumnViewModel, string>) (vm => vm.ColumnId), (Func<ColumnViewModel, ColumnViewModel>) (vm => vm));
      if (useInWeek)
        dictionary["habit"].SortOrder = 2L;
      foreach (Node<TaskBaseViewModel> node in TaskCache.GetTaskNodeDict(this.SourceModels).Values)
      {
        TaskBaseViewModel taskBaseViewModel = node.Value;
        if (taskBaseViewModel.IsNote)
          noteColumn.SourceItems.Add(taskBaseViewModel);
        else if (taskBaseViewModel.IsHabit)
        {
          if (taskBaseViewModel.Status == 0 || !LocalSettings.Settings.HideComplete)
            habitColumn.SourceItems.Add(taskBaseViewModel);
        }
        else if (taskBaseViewModel.IsNote)
          noteColumn.SourceItems.Add(taskBaseViewModel);
        else if ((!taskBaseViewModel.IsTask || !node.HasParent) && (taskBaseViewModel.Status == 0 || !LocalSettings.Settings.HideComplete))
        {
          ColumnViewModel columnViewModel = flag2 | useInWeek ? dictionary["date:0"] : dictionary["date:no"];
          if (flag1)
          {
            columnViewModel = dictionary["date:1"];
          }
          else
          {
            switch (DateUtils.GetSectionCategory(taskBaseViewModel.StartDate, taskBaseViewModel.DueDate, taskBaseViewModel.IsAllDay))
            {
              case DateUtils.DateSectionCategory.OutDated:
                columnViewModel = dictionary["date:-1"];
                break;
              case DateUtils.DateSectionCategory.Today:
                columnViewModel = dictionary["date:0"];
                break;
              case DateUtils.DateSectionCategory.Tomorrow:
                columnViewModel = flag2 ? dictionary["date:0"] : dictionary["date:1"];
                break;
              case DateUtils.DateSectionCategory.ThisWeek:
                if (useInWeek)
                {
                  DateTime? startDate = taskBaseViewModel.StartDate;
                  ref DateTime? local1 = ref startDate;
                  DateTime? nullable1 = local1.HasValue ? new DateTime?(local1.GetValueOrDefault().Date) : new DateTime?();
                  DateTime today = DateTime.Today;
                  TimeSpan? nullable2;
                  TimeSpan? nullable3;
                  if (!nullable1.HasValue)
                  {
                    nullable2 = new TimeSpan?();
                    nullable3 = nullable2;
                  }
                  else
                    nullable3 = new TimeSpan?(nullable1.GetValueOrDefault() - today);
                  nullable2 = nullable3;
                  ref TimeSpan? local2 = ref nullable2;
                  int days = local2.HasValue ? local2.GetValueOrDefault().Days : 0;
                  switch (days)
                  {
                    case 0:
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                      columnViewModel = dictionary["date:" + days.ToString()];
                      break;
                    default:
                      columnViewModel = dictionary["date:-1"];
                      break;
                  }
                }
                else
                {
                  columnViewModel = flag2 ? dictionary["date:0"] : dictionary["date:week"];
                  break;
                }
                break;
              case DateUtils.DateSectionCategory.Future:
                columnViewModel = flag2 | useInWeek ? dictionary["date:0"] : dictionary["date:later"];
                break;
            }
          }
          columnViewModel.SourceItems.Add(taskBaseViewModel);
          columnViewModel.SourceItems.AddRange((IEnumerable<TaskBaseViewModel>) node.GetAllChildrenValue());
        }
      }
      return source;
    }

    private List<ColumnViewModel> GetPriorityColumns()
    {
      List<ColumnViewModel> columnViewModels = ColumnViewModel.GetPriorityColumnViewModels();
      ColumnViewModel noteColumn = ColumnViewModel.GetNoteColumn();
      columnViewModels.Add(noteColumn);
      ColumnViewModel habitColumn = ColumnViewModel.GetHabitColumn(this.Identity is WeekProjectIdentity);
      columnViewModels.Add(habitColumn);
      foreach (ColumnViewModel columnViewModel in columnViewModels)
        columnViewModel.SetColumnIdentity(this.Identity);
      Dictionary<string, ColumnViewModel> dictionary = columnViewModels.ToDictionary<ColumnViewModel, string, ColumnViewModel>((Func<ColumnViewModel, string>) (vm => vm.ColumnId), (Func<ColumnViewModel, ColumnViewModel>) (vm => vm));
      foreach (Node<TaskBaseViewModel> node in TaskCache.GetTaskNodeDict(this.SourceModels).Values)
      {
        TaskBaseViewModel taskBaseViewModel = node.Value;
        if (taskBaseViewModel.IsNote)
          noteColumn.SourceItems.Add(taskBaseViewModel);
        else if (taskBaseViewModel.IsHabit)
        {
          if (taskBaseViewModel.Status == 0 || !LocalSettings.Settings.HideComplete)
            habitColumn.SourceItems.Add(taskBaseViewModel);
        }
        else if (!node.HasParent && (taskBaseViewModel.Status == 0 || !LocalSettings.Settings.HideComplete))
        {
          string key = "priority:" + taskBaseViewModel.Priority.ToString();
          ColumnViewModel columnViewModel = dictionary.ContainsKey(key) ? dictionary[key] : dictionary["priority:" + 0.ToString()];
          columnViewModel.SourceItems.Add(taskBaseViewModel);
          columnViewModel.SourceItems.AddRange((IEnumerable<TaskBaseViewModel>) node.GetAllChildrenValue());
        }
      }
      return columnViewModels;
    }

    private async Task<List<ColumnViewModel>> GetCustomColumns(bool force = false, bool pullRemote = false)
    {
      Dictionary<string, ColumnViewModel> vms = new Dictionary<string, ColumnViewModel>();
      if (this.Identity is NormalProjectIdentity identity && identity.Project != null)
      {
        ProjectModel project = identity.Project;
        List<ColumnModel> columns = await ColumnDao.GetColumnsByProjectId(project.id);
        if (columns == null || !columns.Any<ColumnModel>())
        {
          int num1;
          if (!project.IsShareList())
          {
            bool? closed = project.closed;
            if (closed.HasValue)
            {
              closed = project.closed;
              num1 = closed.Value ? 1 : 0;
            }
            else
              num1 = 0;
          }
          else
            num1 = 1;
          int num2 = force ? 1 : 0;
          if ((num1 | num2) != 0)
          {
            if (await ColumnBatchHandler.MergeWithServer(project.id))
              return (List<ColumnViewModel>) null;
            pullRemote = false;
          }
          if ((columns == null || !columns.Any<ColumnModel>()) && this.SourceModels.Count > 0)
          {
            await ColumnDao.InitColumns(project.id);
            columns = await ColumnDao.GetColumnsByProjectId(project.id);
          }
        }
        if (columns == null || !columns.Any<ColumnModel>())
          return (List<ColumnViewModel>) null;
        columns.Sort((Comparison<ColumnModel>) ((a, b) => (a.sortOrder ?? long.MinValue).CompareTo(b.sortOrder ?? long.MinValue)));
        string id = columns[0].id;
        foreach (ColumnModel model in columns)
        {
          ColumnProjectIdentity columnProjectIdentity = new ColumnProjectIdentity(this.Identity, model.id);
          ColumnViewModel columnViewModel = new ColumnViewModel(model)
          {
            Identity = columnProjectIdentity,
            Enable = project.IsEnable(),
            CanAdd = project.IsEnable()
          };
          vms.Add(model.id, columnViewModel);
        }
        Dictionary<string, Node<TaskBaseViewModel>> taskNodeDict = TaskCache.GetTaskNodeDict(this.SourceModels);
        bool flag = false;
        foreach (Node<TaskBaseViewModel> node in taskNodeDict.Values)
        {
          if (node.HasParent && node.Value.ColumnId != node.Parent.Value.ColumnId && !string.IsNullOrEmpty(node.Parent.Value.ColumnId))
          {
            string cId = node.Parent.Value.ColumnId;
            node.GetAllChildrenValue().ForEach((Action<TaskBaseViewModel>) (m => m.ColumnId = cId));
            node.Value.ColumnId = cId;
          }
          TaskBaseViewModel taskBaseViewModel = node.Value;
          if (string.IsNullOrEmpty(taskBaseViewModel.ColumnId) || !vms.ContainsKey(taskBaseViewModel.ColumnId))
          {
            ColumnViewModel columnViewModel = vms[id];
            if (!string.IsNullOrEmpty(taskBaseViewModel.ColumnId) && !vms.ContainsKey(taskBaseViewModel.ColumnId))
            {
              UtilLog.Info("KanbanTaskNoColumn : t:" + taskBaseViewModel.Id + " c:" + taskBaseViewModel.ColumnId + " tp:" + taskBaseViewModel.ProjectId + " cp:" + columnViewModel.ProjectId);
              flag = true;
            }
            if (columnViewModel.ProjectId == taskBaseViewModel.ProjectId)
            {
              taskBaseViewModel.ColumnId = id;
              if (string.IsNullOrEmpty(taskBaseViewModel.ColumnId))
                TaskService.BatchSetColumn(new List<string>()
                {
                  taskBaseViewModel.Id
                }, id);
            }
            columnViewModel.SourceItems.Add(taskBaseViewModel);
          }
          else
            vms[taskBaseViewModel.ColumnId].SourceItems.Add(taskBaseViewModel);
        }
        if (flag)
          UtilLog.Info("KanbanTaskNoColumn : currentColumns " + vms.Keys.ToList<string>().Join<string>(","));
        if (force & pullRemote)
          Task.Run((Action) (() => ColumnBatchHandler.MergeWithServer(project.id)));
        columns = (List<ColumnModel>) null;
      }
      return vms.Values.ToList<ColumnViewModel>();
    }

    private List<ColumnViewModel> GetProjectColumns()
    {
      Dictionary<string, ColumnViewModel> dictionary = new Dictionary<string, ColumnViewModel>();
      ColumnViewModel habitColumn = ColumnViewModel.GetHabitColumn(this.Identity is WeekProjectIdentity);
      dictionary.Add(habitColumn.ColumnId, habitColumn);
      Dictionary<string, Node<TaskBaseViewModel>> taskNodeDict = TaskCache.GetTaskNodeDict(this.SourceModels);
      List<ProjectModel> projects = CacheManager.GetProjects();
      projects.Sort((Comparison<ProjectModel>) ((l, r) => l.CompareTo(r)));
      foreach (Node<TaskBaseViewModel> node in taskNodeDict.Values)
      {
        TaskBaseViewModel model = node.Value;
        if (model.IsHabit)
        {
          if (model.Status == 0 || !LocalSettings.Settings.HideComplete)
            habitColumn.SourceItems.Add(model);
        }
        else if (!node.HasParent && (model.Status == 0 || !LocalSettings.Settings.HideComplete))
        {
          string key = "project:" + model.ProjectId;
          if (model.IsEvent)
            key = "calendar";
          if (model.IsCourse)
            key = "course";
          if (model.IsHabit)
            key = "habit";
          if (!dictionary.ContainsKey(key))
          {
            ProjectModel projectModel = projects.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.ProjectId));
            bool flag = projectModel != null && projectModel.IsEnable();
            dictionary.Add(key, new ColumnViewModel()
            {
              Name = model.IsEvent ? Utils.GetString("SubscribeCalendar") : (model.IsCourse ? Utils.GetString("Timetable") : (model.IsHabit ? Utils.GetString("Habit") : projectModel?.name)),
              SortOrder = model.IsEvent ? 9223372036854775805L : (model.IsCourse ? 9223372036854775806L : (model.IsHabit ? long.MaxValue : (long) projects.IndexOf(projectModel))),
              ColumnId = key,
              CanDropDown = flag,
              CanAdd = flag
            });
          }
          ColumnViewModel columnViewModel = dictionary[key];
          columnViewModel.SourceItems.Add(model);
          List<Node<TaskBaseViewModel>> children = node.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
            columnViewModel.SourceItems.AddRange((IEnumerable<TaskBaseViewModel>) node.GetAllChildrenValue());
        }
      }
      List<ColumnViewModel> list = dictionary.Values.ToList<ColumnViewModel>();
      foreach (ColumnViewModel columnViewModel in list)
        columnViewModel.SetColumnIdentity(this.Identity);
      return list;
    }

    public void SetIdentity(ProjectIdentity identity)
    {
      this.Name = identity.GetDisplayTitle();
      if (identity is NormalProjectIdentity normalProjectIdentity)
      {
        ProjectModel project1 = normalProjectIdentity.Project;
        this.ShowAdd = project1 != null && project1.ShowAddColumn;
        ProjectModel project2 = normalProjectIdentity.Project;
        this.Enable = project2 != null && project2.IsEnable();
      }
      else
        this.ShowAdd = false;
      this.Identity = identity;
    }

    private async void TryLoadCompletedTasks()
    {
      if (!await this.CompletedTaskLoader.NeedPullCompletedTasks(this.Identity))
        return;
      if (!await this.CompletedTaskLoader.TryLoadCompletedTasks(this.Identity))
        return;
      await this.LoadTasks();
    }

    public void Dispose() => this._kanban = (KanbanContainer) null;
  }
}
