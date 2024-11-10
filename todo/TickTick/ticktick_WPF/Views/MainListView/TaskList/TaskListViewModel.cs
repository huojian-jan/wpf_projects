// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.TaskList.TaskListViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Kanban;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.MainListView.TaskList
{
  public class TaskListViewModel : BaseViewModel
  {
    private string _title;
    public readonly TrashSyncService TrashTaskLoader = new TrashSyncService();
    public List<TaskBaseViewModel> SourceModels = new List<TaskBaseViewModel>();
    private bool _loading;
    private KanbanViewModel _kanban;
    private string _previewLoadId;

    public TaskListViewModel()
    {
    }

    public TaskListViewModel(ProjectIdentity projectIdentity)
    {
      this.ProjectIdentity = projectIdentity;
      this.Title = projectIdentity.GetDisplayTitle();
      this.CompletedTaskLoader.SetIdentity(projectIdentity);
    }

    public void SetIdentity(ProjectIdentity projectIdentity, TaskListDisplayType displayType = TaskListDisplayType.Normal)
    {
      if (projectIdentity.Id != this.ProjectIdentity?.Id)
        this._loading = false;
      this.ProjectIdentity = projectIdentity;
      this.Title = projectIdentity.GetDisplayTitle();
      this.ListDisplayType = displayType;
      if (displayType != TaskListDisplayType.Kanban && displayType != TaskListDisplayType.Matrix)
      {
        this.CompletedTaskLoader.SetIdentity(projectIdentity);
      }
      else
      {
        SortProjectData sortProjectData = new SortProjectData();
        sortProjectData.ProjectIdentity = projectIdentity;
        sortProjectData.ShowLoadMore = true;
        this.ProjectData = sortProjectData;
      }
    }

    public SortOption SortOption => this.ProjectIdentity.SortOption;

    public string Title
    {
      get => this._title;
      set
      {
        if (!(this._title != value))
          return;
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public ProjectIdentity ProjectIdentity { get; set; } = (ProjectIdentity) ProjectIdentity.CreateInboxProject();

    public ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader CompletedTaskLoader { get; private set; } = new ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader();

    public ObservableCollection<DisplayItemModel> Items { get; set; } = new ObservableCollection<DisplayItemModel>();

    public TaskListDisplayType ListDisplayType { get; set; }

    public SortProjectData ProjectData { get; set; }

    public bool InKanban => this.ListDisplayType == TaskListDisplayType.Kanban;

    public bool InMatrix => this.ListDisplayType == TaskListDisplayType.Matrix;

    public bool InDetail => this.ListDisplayType == TaskListDisplayType.SubTask;

    public bool IsList => this.ListDisplayType == TaskListDisplayType.Normal;

    public List<string> SelectedTaskIds { get; set; }

    public IBatchEditable BatchEditor { get; set; }

    public bool AddingTask { get; set; }

    public bool InBatchSelect { get; set; }

    public string FirstBatchSelectId { get; set; }

    public async Task LoadModels(bool refreshData, bool loadMore)
    {
      if (this.ProjectIdentity is ColumnProjectIdentity)
      {
        await this._kanban.LoadTasks();
      }
      else
      {
        bool forceLoad = this._previewLoadId != this.ProjectIdentity.CatId;
        if (refreshData)
        {
          this._previewLoadId = this.ProjectIdentity.CatId;
          this.SourceModels = await TaskListRefreshHelper.InitData(this.ProjectIdentity);
        }
        await this.LoadSortedItems(loadMore, force: forceLoad);
      }
    }

    public async Task<List<DisplayItemModel>> BuildItems(
      bool loadMore,
      List<TaskBaseViewModel> models)
    {
      this._loading = true;
      List<ColumnModel> columns = (List<ColumnModel>) null;
      if (this.ProjectIdentity is NormalProjectIdentity projectIdentity && projectIdentity.SortOption?.groupBy == Constants.SortType.sortOrder.ToString() && !this.InKanban)
        columns = await ColumnDao.GetColumnsByProjectId(projectIdentity.Project.id);
      return await TaskListRefreshHelper.GetSortedDisplayItems(this.ProjectIdentity, models, loadMore, columns, this.CompletedTaskLoader) ?? new List<DisplayItemModel>();
    }

    public async Task LoadSortedItems(
      bool loadMore,
      SectionAddTaskViewModel addingModel = null,
      bool force = false)
    {
      if (this.SourceModels == null)
        this.SourceModels = new List<TaskBaseViewModel>();
      List<DisplayItemModel> revisedItems;
      if (this.SourceModels.Count == 0 && this.SortOption?.groupBy != "sortOrder")
      {
        this.Items.Clear();
        revisedItems = (List<DisplayItemModel>) null;
      }
      else if (this._loading && !force)
      {
        revisedItems = (List<DisplayItemModel>) null;
      }
      else
      {
        revisedItems = await this.BuildItems(loadMore, this.SourceModels);
        List<string> selectedTaskIds = this.BatchEditor?.GetSelectedTaskIds();
        // ISSUE: explicit non-virtual call
        if (selectedTaskIds != null && __nonvirtual (selectedTaskIds.Count) > 0)
        {
          bool flag = selectedTaskIds.Count > 0 && this.InBatchSelect;
          foreach (DisplayItemModel displayItemModel in revisedItems)
          {
            if (!displayItemModel.IsOpen)
            {
              foreach (DisplayItemModel childrenModel in displayItemModel.GetChildrenModels(true))
              {
                childrenModel.Selected = selectedTaskIds.Contains(childrenModel.Id);
                childrenModel.InBatchSelected = childrenModel.Selected & flag;
              }
            }
            if (displayItemModel.IsSection)
            {
              displayItemModel.InBatchSelected = this.InBatchSelect;
            }
            else
            {
              displayItemModel.Selected = selectedTaskIds.Contains(displayItemModel.Id);
              displayItemModel.InBatchSelected = displayItemModel.Selected & flag;
            }
          }
        }
        if (LocalSettings.Settings.ShowDetails && !this.InMatrix && !this.InKanban)
        {
          revisedItems.ForEach((Action<DisplayItemModel>) (item =>
          {
            if (!item.IsTask)
              return;
            item.CompletionRate = TaskCompletionRateDao.GetRateStrById(item.Id);
          }));
          List<DisplayItemModel> taskModels = revisedItems.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.IsTask)).ToList<DisplayItemModel>();
          TaskCompletionRateDao.FetchRateByModels(taskModels).ContinueWith((Action<Task<List<string>>>) (task =>
          {
            if (!task.IsCompleted || task.Exception != null || task.Result == null || !task.Result.Any<string>())
              return;
            taskModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => task.Result.Contains(item.Id))).ToList<DisplayItemModel>().ForEach((Action<DisplayItemModel>) (item => item.CompletionRate = TaskCompletionRateDao.GetRateStrById(item.Id)));
          }));
        }
        this.SetKanbanProperty(revisedItems);
        await TaskItemLoadHelper.LoadIndicator(revisedItems, LocalSettings.Settings.ShowDetails || this.InKanban);
        if (addingModel != null)
        {
          for (int index = 0; index < revisedItems.Count; ++index)
          {
            if (revisedItems[index].Section?.SectionId == addingModel.SectionId)
            {
              revisedItems.Insert(index + 1, new DisplayItemModel()
              {
                SourceViewModel = new TaskBaseViewModel()
                {
                  Id = string.Empty
                },
                AddViewModel = addingModel,
                Section = revisedItems[index].Section
              });
              this.AddingTask = true;
              break;
            }
          }
        }
        if (revisedItems.Count > 0 && revisedItems[0].IsSection)
          revisedItems[0].UnderTaskItem = false;
        ItemsSourceHelper.CopyTo<DisplayItemModel>(revisedItems, this.Items);
        this._loading = false;
        revisedItems = (List<DisplayItemModel>) null;
      }
    }

    public void SetKanbanProperty(List<DisplayItemModel> models)
    {
      if (!this.InKanban)
        return;
      for (int index = 0; index < models.Count; ++index)
      {
        DisplayItemModel model = models[index];
        model.ShowBottomMargin = index >= models.Count - 1 || models[index + 1].Level <= 0;
        model.ShowTopMargin = model.Level <= 0;
        model.InKanban = true;
      }
    }

    public void RemoveSelectedId(string id)
    {
      this.SelectedTaskIds?.Remove(id);
      this.BatchEditor?.RemoveSelectedId(id);
    }

    public DisplayItemModel SetSelectedTaskId(string id)
    {
      bool flag = string.IsNullOrEmpty(id);
      DisplayItemModel displayItemModel1 = (DisplayItemModel) null;
      foreach (DisplayItemModel displayItemModel2 in (Collection<DisplayItemModel>) this.Items)
      {
        if (!flag && displayItemModel2.Id == id)
        {
          displayItemModel2.Selected = true;
          displayItemModel1 = displayItemModel2;
          if (!displayItemModel2.CanBatchSelect)
            this.InBatchSelect = false;
          displayItemModel2.InBatchSelected = this.InBatchSelect;
          this.FirstBatchSelectId = this.InBatchSelect ? id : "";
        }
        else
        {
          displayItemModel2.Selected = false;
          displayItemModel2.InBatchSelected = false;
        }
      }
      if (flag)
      {
        this.InBatchSelect = false;
        this.FirstBatchSelectId = "";
      }
      List<string> ids;
      if (!flag)
        ids = new List<string>() { id };
      else
        ids = new List<string>();
      this.SetSelectedTaskIds(ids);
      return displayItemModel1;
    }

    public void SetSelectedTaskIds(List<string> ids)
    {
      this.SelectedTaskIds = ids;
      this.BatchEditor?.SetSelectedTaskIds(ids);
    }

    public async Task CheckSearchMatched(List<string> ids)
    {
      List<TaskBaseViewModel> tasks = TaskCache.GetTasksByIds(ids);
      ConcurrentDictionary<string, List<CommentModel>> taskCommentDict = await TaskCommentCache.GetTaskCommentDict();
      List<TaskBaseViewModel> source1 = this.SourceModels;
      string keyword = SearchHelper.SearchKey;
      if (string.IsNullOrEmpty(keyword))
      {
        tasks = (List<TaskBaseViewModel>) null;
      }
      else
      {
        string str1 = keyword;
        List<string> stringList;
        if (str1 == null)
        {
          stringList = (List<string>) null;
        }
        else
        {
          string[] source2 = str1.Split(' ');
          stringList = source2 != null ? ((IEnumerable<string>) source2).ToList<string>() : (List<string>) null;
        }
        List<string> keys = stringList;
        keys?.Remove("");
        bool flag1 = false;
        bool flag2 = false;
        foreach (TaskBaseViewModel taskBaseViewModel in tasks)
        {
          TaskBaseViewModel task = taskBaseViewModel;
          string title = task.Title;
          string str2;
          if (task.Kind == "TEXT")
          {
            str2 = title + "\n" + task.Content;
          }
          else
          {
            str2 = title + "\n" + task.Desc;
            BlockingList<TaskBaseViewModel> checkItems = task.CheckItems;
            if ((checkItems != null ? (checkItems.Count > 0 ? 1 : 0) : 0) != 0)
              str2 = task.CheckItems.ToList().Aggregate<TaskBaseViewModel, string>(str2, (Func<string, TaskBaseViewModel, string>) ((current, checkItem) => current + "\n" + checkItem.Title));
          }
          if (!string.IsNullOrEmpty(task.Id) && taskCommentDict.ContainsKey(task.Id))
            str2 = taskCommentDict[task.Id].Aggregate<CommentModel, string>(str2, (Func<string, CommentModel, string>) ((current, comment) => current + "\r\n" + comment.title?.ToLower()));
          if (SearchHelper.KeyWordMatched(str2, keyword, keys))
          {
            if (source1.All<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m => m.Id != task.Id)))
            {
              source1.Add(task);
              flag1 = true;
              flag2 = true;
            }
          }
          else
          {
            source1.Remove(task);
            flag1 = true;
          }
        }
        if (flag2)
        {
          List<TaskBaseViewModel> exactSearchTasks = new List<TaskBaseViewModel>();
          List<TaskBaseViewModel> fussySearchTasks = new List<TaskBaseViewModel>();
          foreach (TaskBaseViewModel taskBaseViewModel in source1)
          {
            if (taskBaseViewModel.Title != null && taskBaseViewModel.Title.Contains(keyword))
            {
              exactSearchTasks.Add(taskBaseViewModel);
            }
            else
            {
              if (taskBaseViewModel.Kind == "TEXT")
              {
                if (taskBaseViewModel.Content != null && taskBaseViewModel.Content.Contains(keyword))
                {
                  exactSearchTasks.Add(taskBaseViewModel);
                  continue;
                }
              }
              else
              {
                if (taskBaseViewModel.Desc == null || !taskBaseViewModel.Desc.Contains(keyword))
                {
                  BlockingList<TaskBaseViewModel> checkItems = taskBaseViewModel.CheckItems;
                  bool? nullable;
                  if (checkItems == null)
                  {
                    nullable = new bool?();
                  }
                  else
                  {
                    List<TaskBaseViewModel> list = checkItems.ToList();
                    nullable = list != null ? new bool?(list.Any<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (c => c.Title.Contains(keyword)))) : new bool?();
                  }
                  if (!nullable.GetValueOrDefault())
                    goto label_36;
                }
                exactSearchTasks.Add(taskBaseViewModel);
                continue;
              }
label_36:
              if (!string.IsNullOrEmpty(taskBaseViewModel.Id) && taskCommentDict.ContainsKey(taskBaseViewModel.Id))
              {
                List<CommentModel> source3 = taskCommentDict[taskBaseViewModel.Id];
                if ((source3 != null ? (source3.Any<CommentModel>((Func<CommentModel, bool>) (c => c.title.Contains(keyword))) ? 1 : 0) : 0) != 0)
                {
                  exactSearchTasks.Add(taskBaseViewModel);
                  continue;
                }
              }
              fussySearchTasks.Add(taskBaseViewModel);
            }
          }
          source1 = TaskDisplayService.GetSearchModels(exactSearchTasks, fussySearchTasks);
        }
        if (!flag1)
        {
          tasks = (List<TaskBaseViewModel>) null;
        }
        else
        {
          this.SourceModels = source1;
          this.LoadSortedItems(false);
          tasks = (List<TaskBaseViewModel>) null;
        }
      }
    }

    public void CheckOpenChangedIds(HashSet<string> ids, bool useTaskOpen)
    {
      List<DisplayItemModel> models = this.Items.ToList<DisplayItemModel>();
      List<DisplayItemModel> list = models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>();
      if (list.Count == 0)
        return;
      HashSet<string> openedTaskIds = (HashSet<string>) null;
      if (!useTaskOpen)
        openedTaskIds = SmartListTaskFoldHelper.GetOpenedTaskIds(this.ProjectIdentity.CatId);
      bool flag1 = false;
      foreach (DisplayItemModel displayItemModel in list)
      {
        bool flag2 = useTaskOpen ? displayItemModel.SourceViewModel.IsOpen : openedTaskIds.Contains(displayItemModel.Id);
        if (displayItemModel.IsOpen != flag2)
        {
          if (displayItemModel.IsOpen)
          {
            displayItemModel.GetChildrenModels(true).ForEach((Action<DisplayItemModel>) (m => models.Remove(m)));
          }
          else
          {
            List<DisplayItemModel> childrenModels = displayItemModel.GetChildrenModels(false, openedTaskIds);
            int num = models.IndexOf(displayItemModel);
            if (num >= 0)
            {
              for (int index = childrenModels.Count - 1; index >= 0; --index)
                models.Insert(num + 1, childrenModels[index]);
            }
          }
          flag1 = true;
          displayItemModel.IsOpen = flag2;
        }
      }
      if (!flag1)
        return;
      ItemsSourceHelper.CopyTo<DisplayItemModel>(models, this.Items);
    }

    public async Task OpenOrCloseAllSections(List<DisplayItemModel> sections, bool isOpen)
    {
      List<DisplayItemModel> models = this.Items.ToList<DisplayItemModel>();
      foreach (DisplayItemModel section1 in sections)
      {
        if (section1.IsOpen != isOpen)
        {
          string name = section1.Title;
          if (section1.Section is AssigneeSection section2)
            name = section2.Assingee;
          if (this.InKanban)
          {
            if (isOpen)
              await SectionStatusDao.OpenProjectSection(this.ProjectIdentity?.Id, name);
            else
              await SectionStatusDao.CloseProjectSection(this.ProjectIdentity?.Id, name);
          }
          else
          {
            section1.IsOpen = isOpen;
            List<DisplayItemModel> displayItemModelList = new List<DisplayItemModel>();
            foreach (DisplayItemModel child in section1.Section.Children)
            {
              displayItemModelList.Add(child);
              if (child.IsOpen && child.IsTask)
              {
                List<DisplayItemModel> childrenModels = child.GetChildrenModels(false);
                displayItemModelList.AddRange((IEnumerable<DisplayItemModel>) childrenModels);
              }
            }
            if (isOpen)
            {
              SectionStatusDao.OpenProjectSection(this.ProjectIdentity?.Id, name);
              int num = models.IndexOf(section1);
              if (num >= 0)
              {
                for (int index = 0; index < displayItemModelList.Count; ++index)
                  models.Insert(num + 1 + index, displayItemModelList[index]);
                if (section1.Section?.SectionId == new CompletedSection().SectionId)
                {
                  DisplayItemModel displayItemModel = DisplayItemModel.BuildLoadMore();
                  displayItemModel.Section = section1.Section;
                  models.Add(displayItemModel);
                }
              }
            }
            else
            {
              SectionStatusDao.CloseProjectSection(this.ProjectIdentity?.Id, name);
              displayItemModelList.ForEach((Action<DisplayItemModel>) (child => models.Remove(child)));
              if (section1.Section?.SectionId == new CompletedSection().SectionId && models[models.Count - 1].Type == DisplayType.LoadMore)
                models.RemoveAt(models.Count - 1);
            }
          }
        }
      }
      if (this.InKanban)
        ;
      else
        this.SetItems(models);
    }

    public void OpenOrCloseSection(SectionStatus status, bool storeStatus)
    {
      List<DisplayItemModel> models = this.Items.ToList<DisplayItemModel>();
      DisplayItemModel displayItemModel1 = models.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.Type == DisplayType.Section && item.Section.SectionId == status.SectionId));
      if (displayItemModel1 == null)
        return;
      List<DisplayItemModel> displayItemModelList = new List<DisplayItemModel>();
      foreach (DisplayItemModel child in displayItemModel1.Section.Children)
      {
        displayItemModelList.Add(child);
        if (child.IsOpen && child.IsTask)
        {
          List<DisplayItemModel> childrenModels = child.GetChildrenModels(false);
          displayItemModelList.AddRange((IEnumerable<DisplayItemModel>) childrenModels);
        }
      }
      string name = displayItemModel1.Title;
      if (displayItemModel1.Section is AssigneeSection section)
        name = section.Assingee;
      if (status.IsOpen)
      {
        int index1 = models.IndexOf(displayItemModel1);
        if (storeStatus)
          SectionStatusDao.OpenProjectSection(this.ProjectIdentity.Id, name);
        if (index1 >= 0)
        {
          DisplayItemModel displayItemModel2 = models[index1];
          if (displayItemModel2 != null)
          {
            for (int index2 = 0; index2 < displayItemModelList.Count; ++index2)
              models.Insert(index1 + 1 + index2, displayItemModelList[index2]);
            if (status.SectionId == new CompletedSection().SectionId)
            {
              DisplayItemModel displayItemModel3 = DisplayItemModel.BuildLoadMore();
              displayItemModel3.Section = displayItemModel2.Section;
              models.Add(displayItemModel3);
            }
          }
        }
      }
      else
      {
        if (storeStatus && !string.IsNullOrEmpty(this.ProjectIdentity.Id))
          SectionStatusDao.CloseProjectSection(this.ProjectIdentity.Id, name);
        displayItemModelList.ForEach((Action<DisplayItemModel>) (child => models.Remove(child)));
        if (status.SectionId == new CompletedSection().SectionId && models[models.Count - 1].Type == DisplayType.LoadMore)
          models.RemoveAt(models.Count - 1);
      }
      this.SetItems(models);
    }

    public void ChangeModelOpenStatus(DisplayItemModel model, bool useTaskOpen)
    {
      if (model == null)
        return;
      List<DisplayItemModel> models = this.Items.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m != null)).ToList<DisplayItemModel>();
      if (model.IsOpen)
      {
        model.GetChildrenModels(true).ForEach((Action<DisplayItemModel>) (m => models.Remove(m)));
      }
      else
      {
        HashSet<string> openedTaskIds = (HashSet<string>) null;
        if (!useTaskOpen)
          openedTaskIds = SmartListTaskFoldHelper.GetOpenedTaskIds(this.ProjectIdentity.CatId);
        List<DisplayItemModel> childrenModels = model.GetChildrenModels(false, openedTaskIds);
        int num = models.IndexOf(model);
        if (num >= 0)
        {
          for (int index = childrenModels.Count - 1; index >= 0; --index)
            models.Insert(num + 1, childrenModels[index]);
        }
      }
      model.IsOpen = !model.IsOpen;
      this.SetItems(models);
    }

    public List<DisplayItemModel> RemoveItemByIds(List<string> taskIds, string except = null)
    {
      List<DisplayItemModel> result = new List<DisplayItemModel>();
      if (this.Items.Any<DisplayItemModel>())
      {
        List<DisplayItemModel> models = this.Items.ToList<DisplayItemModel>();
        models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (i =>
        {
          if (taskIds.Contains(i.Id))
            return true;
          return except == null && taskIds.Contains(i.TaskId);
        })).ToList<DisplayItemModel>().ForEach((Action<DisplayItemModel>) (item =>
        {
          DisplayItemModel displayItemModel1 = (DisplayItemModel) null;
          int num = models.IndexOf(item);
          if (num > 0)
            displayItemModel1 = models.ElementAt<DisplayItemModel>(num - 1);
          if (displayItemModel1 != null && item.Selected)
            displayItemModel1.LineVisible = true;
          List<DisplayItemModel> childrenModels = item.GetChildrenModels(false);
          if (item.Id != except)
          {
            models.Remove(item);
            result.Add(item);
            TaskListViewModel.RemoveItemInParent(item, models);
          }
          else if (item.Parent == null || item.Parent.IsSection)
            result.Add(item);
          if (childrenModels == null)
            return;
          foreach (DisplayItemModel displayItemModel2 in childrenModels)
          {
            models.Remove(displayItemModel2);
            result.Add(displayItemModel2);
          }
        }));
        this.SetItems(models);
      }
      return result;
    }

    private static void RemoveItemInParent(DisplayItemModel item, List<DisplayItemModel> models)
    {
      if (item.Parent == null)
        return;
      item.Parent.RemoveItem(item);
      if (!item.Parent.IsSection || item.Parent.IsCustomizedSection || item.Parent.Section.Children.Count != 0)
        return;
      models.Remove(item.Parent);
    }

    private async Task RemoveSubItemByTaskId(string taskId, List<DisplayItemModel> models)
    {
      (await TaskDetailItemDao.GetCheckItemsByTaskId(taskId)).Select<TaskDetailItemModel, string>((Func<TaskDetailItemModel, string>) (task => task.id)).ToList<string>().ForEach((Action<string>) (id =>
      {
        List<DisplayItemModel> source = models;
        DisplayItemModel displayItemModel = source != null ? source.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => model.Type == DisplayType.CheckItem && model.Id == id)) : (DisplayItemModel) null;
        if (displayItemModel == null)
          return;
        models.Remove(displayItemModel);
        TaskListViewModel.RemoveItemInParent(displayItemModel, models);
      }));
    }

    public void RemoveItemById(string id)
    {
      if (!this.Items.Any<DisplayItemModel>())
        return;
      List<DisplayItemModel> list = this.Items.ToList<DisplayItemModel>();
      DisplayItemModel displayItemModel1 = list.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.Id == id));
      if (displayItemModel1 == null)
        return;
      DisplayItemModel displayItemModel2 = (DisplayItemModel) null;
      int num = list.IndexOf(displayItemModel1);
      if (num > 0)
        displayItemModel2 = list.ElementAt<DisplayItemModel>(num - 1);
      if (displayItemModel2 != null && displayItemModel1.Selected)
        displayItemModel2.LineVisible = true;
      list.Remove(displayItemModel1);
      TaskListViewModel.RemoveItemInParent(displayItemModel1, list);
      this.SourceModels.Remove(displayItemModel1.SourceViewModel);
      if (LocalSettings.Settings.ShowSubtasks)
        this.RemoveSubItemByTaskId(id, list);
      this.SetItems(list);
    }

    public void SetItems(List<DisplayItemModel> models)
    {
      this._loading = true;
      if (this.SourceModels == null)
        this.SourceModels = new List<TaskBaseViewModel>();
      this.SetKanbanProperty(models);
      ItemsSourceHelper.CopyTo<DisplayItemModel>(models, this.Items);
      this._loading = false;
    }

    public DisplayItemModel GetSelectedItem()
    {
      return this.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.Selected));
    }

    public void SetKanbanParent(KanbanViewModel kanban)
    {
      this._kanban = kanban;
      this.CompletedTaskLoader = this._kanban.CompletedTaskLoader;
    }

    public void CheckTaskChanged(TasksChangeEventArgs e)
    {
      if (e.PomoChangedIds.Any() && (SettingsHelper.GetShowDetail() || this.InKanban))
      {
        List<DisplayItemModel> list = this.Items.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => e.PomoChangedIds.Contains(m.Id))).ToList<DisplayItemModel>();
        if (list.Count > 0)
        {
          foreach (DisplayItemModel displayItemModel in list)
            displayItemModel.LoadPomo();
        }
      }
      if (e.AttachmentChangedIds.Any())
      {
        List<DisplayItemModel> list = this.Items.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => e.AttachmentChangedIds.Contains(m.Id))).ToList<DisplayItemModel>();
        if (list.Count > 0)
        {
          foreach (DisplayItemModel displayItemModel in list)
          {
            displayItemModel.ShowAttachment = new bool?(AttachmentCache.GetTaskExistAttachment(displayItemModel.Id));
            displayItemModel.NotifyShowIconsChanged();
          }
        }
      }
      if (e.DateChangedIds.Any())
      {
        List<DisplayItemModel> list = this.Items.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => e.DateChangedIds.Value.Contains(m.TaskId))).ToList<DisplayItemModel>();
        if (list.Count > 0)
        {
          foreach (DisplayItemModel model in list)
            TaskItemLoadHelper.LoadShowReminder(model);
        }
      }
      if (e.TaskTextChangedIds.Any() && this.ProjectIdentity is SearchProjectIdentity)
        this.CheckSearchMatched(e.TaskTextChangedIds.ToList());
      if (!e.TasksOpenChangedIds.Any())
        return;
      ProjectIdentity projectIdentity = this.ProjectIdentity;
      if (projectIdentity is ColumnProjectIdentity columnProjectIdentity)
        projectIdentity = columnProjectIdentity.Project;
      bool useTaskOpen = projectIdentity is NormalProjectIdentity || projectIdentity is GroupProjectIdentity || projectIdentity is ParentTaskIdentity || projectIdentity is MatrixQuadrantIdentity || projectIdentity is CompletedProjectIdentity || projectIdentity is ClosedProjectIdentity;
      this.CheckOpenChangedIds(e.TasksOpenChangedIds.Value, useTaskOpen);
    }

    public void Dispose()
    {
      this.SourceModels?.Clear();
      this.Items.Clear();
      this.BatchEditor = (IBatchEditable) null;
      this.CompletedTaskLoader = (ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader) null;
      this._kanban = (KanbanViewModel) null;
    }

    public void Clear()
    {
      this.SourceModels?.Clear();
      this.Items.Clear();
    }

    public void SetBatchEditor(IBatchEditable editor)
    {
      if (this.BatchEditor == null)
        editor.SetSelectedTaskIds(this.SelectedTaskIds);
      this.BatchEditor = editor;
    }

    public bool IsAllSectionOpen()
    {
      return this.Items.ToList<DisplayItemModel>().All<DisplayItemModel>((Func<DisplayItemModel, bool>) (m =>
      {
        if (!m.IsSection)
          return true;
        return m.IsSection && m.IsOpen;
      }));
    }

    public bool IsAllTaskOpen()
    {
      return this.Items.ToList<DisplayItemModel>().All<DisplayItemModel>((Func<DisplayItemModel, bool>) (m =>
      {
        if (!m.IsTask)
          return true;
        if (!m.IsTask)
          return false;
        return !m.HasChildren || m.IsOpen;
      }));
    }

    public async Task OpenOrCloseAllTasksInKanban(bool isOpen)
    {
      List<DisplayItemModel> source = new List<DisplayItemModel>();
      foreach (DisplayItemModel displayItemModel in this.Items.ToList<DisplayItemModel>())
      {
        if (displayItemModel.IsTask && displayItemModel.Level == 0 && displayItemModel.HasChildren)
        {
          source.Add(displayItemModel);
          source.AddRange(displayItemModel.GetChildrenModels(true).Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (c => c.HasChildren)));
        }
      }
      if (source.Count <= 0)
        return;
      List<string> list = source.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (m => m.Id)).ToList<string>();
      if (this.ProjectIdentity is ColumnProjectIdentity projectIdentity)
        this.ProjectIdentity = projectIdentity.Project;
      if (this.ProjectIdentity is NormalProjectIdentity || this.ProjectIdentity is GroupProjectIdentity || this.ProjectIdentity is ParentTaskIdentity)
        await TaskDao.FoldOrOpenTasks(list, isOpen);
      else
        SmartListTaskFoldHelper.ResetStatus(this.ProjectIdentity.CatId, isOpen ? list : (List<string>) null);
    }

    public void ExitBatchSelect(string selectId = null)
    {
      List<string> ids;
      if (!string.IsNullOrEmpty(selectId))
        ids = new List<string>() { selectId };
      else
        ids = new List<string>();
      this.SetSelectedTaskIds(ids);
      this.InBatchSelect = false;
      this.FirstBatchSelectId = "";
      foreach (DisplayItemModel displayItemModel in (Collection<DisplayItemModel>) this.Items)
      {
        SetItemProperties(displayItemModel);
        if (!displayItemModel.IsOpen)
        {
          foreach (DisplayItemModel childrenModel in displayItemModel.GetChildrenModels(true))
            SetItemProperties(childrenModel);
        }
      }

      void SetItemProperties(DisplayItemModel item)
      {
        item.InBatchSelected = false;
        if (!item.Selected)
          return;
        if (!item.InKanban)
        {
          item.ShowTopMargin = true;
          item.ShowBottomMargin = true;
        }
        if (item.Id == selectId)
        {
          item.Selected = true;
          item.InBatchSelected = false;
        }
        else
        {
          item.Selected = false;
          item.InBatchSelected = false;
        }
      }
    }

    public void SetFirstBatchSelectId(string id)
    {
      if (!string.IsNullOrEmpty(this.FirstBatchSelectId))
        return;
      this.FirstBatchSelectId = id;
    }

    public int GetFirstBatchSelectIndex()
    {
      List<DisplayItemModel> list = this.Items.ToList<DisplayItemModel>();
      DisplayItemModel displayItemModel = list.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.Id == this.FirstBatchSelectId && i.Selected)) ?? list.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.Selected));
      this.FirstBatchSelectId = displayItemModel?.Id;
      return displayItemModel == null ? -1 : list.IndexOf(displayItemModel);
    }

    public void ClearBatchSelect()
    {
      this.ExitBatchSelect();
      this.OnBatchSelectChanged();
    }

    public List<DisplayItemModel> GetSelectedItems(bool forceBatch = false)
    {
      List<DisplayItemModel> selectedTasks = new List<DisplayItemModel>();
      List<DisplayItemModel> sections = new List<DisplayItemModel>();
      DisplayItemModel frontOne = (DisplayItemModel) null;
      foreach (DisplayItemModel displayItemModel in (Collection<DisplayItemModel>) this.Items)
      {
        SetItemProperties(displayItemModel);
        if (!displayItemModel.IsOpen)
        {
          foreach (DisplayItemModel childrenModel in displayItemModel.GetChildrenModels(true))
            SetItemProperties(childrenModel);
          if (!this.InKanban)
            frontOne = displayItemModel.Selected ? displayItemModel : (DisplayItemModel) null;
        }
      }
      if (frontOne != null)
        frontOne.ShowBottomMargin = true;
      bool inBatch = this.InBatchSelect || selectedTasks.Count > 1 || forceBatch && selectedTasks.Count >= 1;
      foreach (DisplayItemModel displayItemModel in selectedTasks)
        displayItemModel.InBatchSelected = inBatch;
      sections.ForEach((Action<DisplayItemModel>) (s => s.InBatchSelected = inBatch));
      return selectedTasks;

      void SetItemProperties(DisplayItemModel item)
      {
        if (item.IsSection)
          sections.Add(item);
        else if (item.Selected)
        {
          if (!this.InKanban)
          {
            if (frontOne == null)
            {
              item.ShowTopMargin = true;
            }
            else
            {
              item.ShowTopMargin = false;
              frontOne.ShowBottomMargin = false;
            }
            frontOne = item;
          }
          selectedTasks.Add(item);
        }
        else
        {
          if (!this.InKanban && frontOne != null)
          {
            frontOne.ShowBottomMargin = true;
            frontOne = (DisplayItemModel) null;
          }
          item.InBatchSelected = false;
        }
      }
    }

    public bool OnBatchSelectChanged()
    {
      List<DisplayItemModel> selectedItems = this.GetSelectedItems(true);
      bool flag1 = selectedItems.Count >= 1;
      bool flag2 = flag1 != this.InBatchSelect;
      selectedItems.ForEach((Action<DisplayItemModel>) (item => item.InBatchSelected = item.Selected));
      this.InBatchSelect = flag1;
      List<string> list = selectedItems.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (t => t.Id)).ToList<string>();
      int num1 = this.SelectedTaskIds == null || list.Count != this.SelectedTaskIds.Count ? 1 : (list.Union<string>((IEnumerable<string>) this.SelectedTaskIds).Count<string>() != this.SelectedTaskIds.Count ? 1 : 0);
      this.SetSelectedTaskIds(list);
      int num2 = flag2 ? 1 : 0;
      return (num1 | num2) != 0;
    }

    public int GetCurrentIndex(string taskId)
    {
      for (int index = 0; index < this.Items.Count; ++index)
      {
        if (this.Items[index].Id == taskId)
          return index;
      }
      return -1;
    }
  }
}
