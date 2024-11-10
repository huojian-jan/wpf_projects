// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sort.SortHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync.ClosedLoader;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Timeline;

#nullable disable
namespace ticktick_WPF.Util.Sort
{
  public static class SortHelper
  {
    public static async Task<ObservableCollection<DisplayItemModel>> Sort(
      ObservableCollection<DisplayItemModel> models,
      ProjectIdentity identity,
      bool showFold = true,
      bool showLoadMore = false,
      List<ColumnModel> columns = null,
      ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader closedLoader = null,
      bool? showComplete = null)
    {
      List<SectionStatusModel> sectionStatusModels = showFold ? CacheManager.GetSectionStatus().Where<SectionStatusModel>((Func<SectionStatusModel, bool>) (item => item.Identity == identity.Id)).ToList<SectionStatusModel>() : new List<SectionStatusModel>();
      ProjectSortExtra extra = ProjectSortExtra.Build(identity);
      if (showComplete.HasValue)
        extra.ShowCompleted = showComplete.Value;
      SortOption sortOption = extra.SortOption;
      extra.ShowFold = showFold;
      List<SyncSortOrderModel> asyncList1 = await TaskSortOrderService.GetAsyncList("taskPinned", identity.CatId);
      string tagParent = (string) null;
      if (identity is TagProjectIdentity tagProjectIdentity)
        tagParent = tagProjectIdentity.Tag;
      HashSet<string> openedTaskIds = (HashSet<string>) null;
      ProjectIdentity projectIdentity = identity is ColumnProjectIdentity columnProjectIdentity ? columnProjectIdentity.Project : identity;
      switch (projectIdentity)
      {
        case NormalProjectIdentity _:
        case GroupProjectIdentity _:
        case MatrixQuadrantIdentity _:
label_7:
          ObservableCollection<DisplayItemModel> results = new ObservableCollection<DisplayItemModel>();
          List<DisplayItemModel> uncompleted = models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (p => p.Status == 0)).ToList<DisplayItemModel>();
          if (uncompleted.Any<DisplayItemModel>() || sortOption.groupBy == Constants.SortType.sortOrder.ToString())
          {
            bool existStarred = false;
            if (!sortOption.IsNone())
            {
              List<DisplayItemModel> list = uncompleted.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.IsPinned)).ToList<DisplayItemModel>();
              if (list.Count > 0)
              {
                existStarred = true;
                await SortHelper.AssembleStarredSection(list, sectionStatusModels, results, openedTaskIds, asyncList1);
              }
              uncompleted.RemoveAll((Predicate<DisplayItemModel>) (item => item.IsPinned));
              if (extra.InKanban && !string.IsNullOrEmpty(extra.SortKey))
              {
                List<SyncSortOrderModel> asyncList2 = await TaskSortOrderService.GetAsyncList(extra.SortKey, identity.GetSortProjectId());
                long num1 = TaskDefaultDao.GetDefaultSafely().AddTo == 0 ? long.MinValue : long.MaxValue;
                foreach (DisplayItemModel displayItemModel1 in uncompleted)
                {
                  DisplayItemModel model = displayItemModel1;
                  long? nullable1;
                  long? nullable2;
                  if (asyncList2 == null)
                  {
                    nullable1 = new long?();
                    nullable2 = nullable1;
                  }
                  else
                  {
                    SyncSortOrderModel syncSortOrderModel = asyncList2.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == model.EntityId));
                    if (syncSortOrderModel == null)
                    {
                      nullable1 = new long?();
                      nullable2 = nullable1;
                    }
                    else
                      nullable2 = new long?(syncSortOrderModel.SortOrder);
                  }
                  long? nullable3 = nullable2;
                  DisplayItemModel displayItemModel2 = model;
                  nullable1 = nullable3;
                  long num2 = nullable1 ?? num1;
                  displayItemModel2.SpecialOrder = num2;
                  if (nullable3.HasValue)
                    extra.ExistSpecialOrderItem = true;
                }
              }
              if (existStarred && extra.InKanban && extra.SortOption.groupBy == "none")
                extra.SortOption.groupBy = extra.SortOption.orderBy;
              IComparer<DisplayItemModel> compareByOrderBy1 = SortHelper.GetCompareByOrderBy(sortOption.orderBy, extra.ProjectId, extra.InNoteColumn);
              string groupBy = sortOption.groupBy;
              if (groupBy != null)
              {
                switch (groupBy.Length)
                {
                  case 3:
                    if (groupBy == "tag")
                    {
                      await SortHelper.GroupAsTag((IReadOnlyCollection<DisplayItemModel>) uncompleted, sectionStatusModels, results, tagParent, openedTaskIds, compareByOrderBy1, extra);
                      goto label_52;
                    }
                    else
                      goto label_50;
                  case 4:
                    if (groupBy == "none")
                      break;
                    goto label_50;
                  case 5:
                    if (groupBy == "title")
                      break;
                    goto label_50;
                  case 7:
                    switch (groupBy[0])
                    {
                      case 'd':
                        if (groupBy == "dueDate")
                        {
                          IComparer<DisplayItemModel> compareByOrderBy2 = SortHelper.GetCompareByOrderBy(sortOption.orderBy, extra.ProjectId, true);
                          await SortHelper.GroupAsDate((IReadOnlyCollection<DisplayItemModel>) uncompleted, sectionStatusModels, results, openedTaskIds, compareByOrderBy1, compareByOrderBy2, extra);
                          goto label_52;
                        }
                        else
                          goto label_50;
                      case 'p':
                        if (groupBy == "project")
                        {
                          await SortHelper.GroupAsProject((IReadOnlyCollection<DisplayItemModel>) uncompleted, sectionStatusModels, results, openedTaskIds, compareByOrderBy1, extra);
                          goto label_52;
                        }
                        else
                          goto label_50;
                      default:
                        goto label_50;
                    }
                  case 8:
                    switch (groupBy[0])
                    {
                      case 'a':
                        if (groupBy == "assignee")
                        {
                          await SortHelper.GroupAsAssignee((IReadOnlyCollection<DisplayItemModel>) uncompleted, sectionStatusModels, results, openedTaskIds, compareByOrderBy1, extra);
                          goto label_52;
                        }
                        else
                          goto label_50;
                      case 'p':
                        if (groupBy == "priority")
                        {
                          IComparer<DisplayItemModel> compareByOrderBy3 = SortHelper.GetCompareByOrderBy(sortOption.orderBy, extra.ProjectId, true);
                          await SortHelper.GroupAsPriority((IReadOnlyCollection<DisplayItemModel>) uncompleted, sectionStatusModels, results, openedTaskIds, compareByOrderBy1, compareByOrderBy3, extra);
                          goto label_52;
                        }
                        else
                          goto label_50;
                      default:
                        goto label_50;
                    }
                  case 9:
                    if (groupBy == "sortOrder")
                    {
                      await SortHelper.GroupAsSortOrder(uncompleted, results, columns, sectionStatusModels, openedTaskIds, existStarred, compareByOrderBy1, extra);
                      goto label_52;
                    }
                    else
                      goto label_50;
                  case 11:
                    if (groupBy == "createdTime")
                      break;
                    goto label_50;
                  case 12:
                    if (groupBy == "modifiedTime")
                      break;
                    goto label_50;
                  default:
                    goto label_50;
                }
                await SortHelper.GroupAsNone((IReadOnlyCollection<DisplayItemModel>) uncompleted, sectionStatusModels, results, openedTaskIds, existStarred, compareByOrderBy1, extra);
                goto label_52;
              }
label_50:
              SortHelper.SortAsNone((IReadOnlyCollection<DisplayItemModel>) uncompleted, results, "none");
            }
            else
              SortHelper.SortAsNone((IReadOnlyCollection<DisplayItemModel>) uncompleted, results, "none");
          }
label_52:
          if (extra.IsTrash)
          {
            List<DisplayItemModel> list = models.ToList<DisplayItemModel>();
            list.Sort((Comparison<DisplayItemModel>) ((a, b) => b.ModifiedTime.CompareTo(a.ModifiedTime) == 0 ? string.Compare(a.Title, b.Title, CultureInfo.CurrentCulture, CompareOptions.None) : b.ModifiedTime.CompareTo(a.ModifiedTime)));
            Dictionary<string, Node<DisplayItemModel>> nodeDict = SortHelper.GetNodeDict(list);
            TaskNodeUtils.BuildNodeTree<DisplayItemModel>(nodeDict);
            ObservableCollection<DisplayItemModel> observableCollection = new ObservableCollection<DisplayItemModel>();
            foreach (Node<DisplayItemModel> node in nodeDict.Values)
            {
              if (!node.HasParent)
              {
                if (openedTaskIds != null)
                  node.Value.IsOpen = openedTaskIds.Contains(node.Value.Id);
                observableCollection.Add(node.Value);
                List<DisplayItemModel> allChildrenValue = node.GetAllChildrenValue();
                if (node.Value.IsOpen)
                {
                  foreach (DisplayItemModel displayItemModel in allChildrenValue)
                  {
                    if (openedTaskIds != null)
                      displayItemModel.IsOpen = openedTaskIds.Contains(displayItemModel.Id);
                    observableCollection.Add(displayItemModel);
                  }
                }
              }
            }
            DisplayItemModel displayItemModel3 = DisplayItemModel.BuildLoadMore();
            if (showLoadMore && observableCollection.Count >= 50)
              observableCollection.Add(displayItemModel3);
            return observableCollection;
          }
          if (extra.IsSearch || extra.ShowCompleted || extra.IsClosed)
          {
            IEnumerable<DisplayItemModel> displayItemModels = models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (p => p.Status != 0));
            ObservableCollection<DisplayItemModel> observableCollection = sortOption.IsNone() ? new ObservableCollection<DisplayItemModel>(displayItemModels) : new ObservableCollection<DisplayItemModel>((IEnumerable<DisplayItemModel>) displayItemModels.OrderByDescending<DisplayItemModel, DateTime?>((Func<DisplayItemModel, DateTime?>) (p => p.CompletedTime)));
            if (observableCollection.Any<DisplayItemModel>())
            {
              if (extra.IsClosed)
              {
                if (extra.IsCompleted)
                  showLoadMore = ClosedTaskWithFilterLoader.CompletionLoader.NeedShowLoadMore(observableCollection.Count);
                if (extra.IsAbandoned)
                  showLoadMore = ClosedTaskWithFilterLoader.AbandonedLoader.NeedShowLoadMore(observableCollection.Count);
                foreach (Section section in SortHelper.GetCompletedSection((IEnumerable<DisplayItemModel>) observableCollection))
                  SortHelper.AssembleSection(section, (IComparer<DisplayItemModel>) new SortHelper.SpecialOrderComparer(), (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
                DisplayItemModel displayItemModel = DisplayItemModel.BuildLoadMore();
                if (showLoadMore)
                  results.Add(displayItemModel);
              }
              else
                SortHelper.AppendCompletedSection((IReadOnlyList<DisplayItemModel>) observableCollection, (IEnumerable<SectionStatusModel>) sectionStatusModels, (ICollection<DisplayItemModel>) results, showLoadMore, loadAll: extra.LoadAll, closedLoader: closedLoader, openedTaskIds: openedTaskIds);
            }
          }
          if (!extra.Editable)
          {
            foreach (DisplayItemModel displayItemModel in (Collection<DisplayItemModel>) results)
              displayItemModel.SourceViewModel.Editable = false;
          }
          return results;
        default:
          openedTaskIds = SmartListTaskFoldHelper.GetOpenedTaskIds(projectIdentity.CatId);
          goto label_7;
      }
    }

    private static IEnumerable<Section> GetCompletedSection(IEnumerable<DisplayItemModel> models)
    {
      List<Section> completedSection = new List<Section>();
      List<DateTime> dateTimeList = new List<DateTime>();
      List<DisplayItemModel> list = models.ToList<DisplayItemModel>();
      list.Sort((Comparison<DisplayItemModel>) ((a, b) => b.CompletedTime.HasValue && a.CompletedTime.HasValue ? b.CompletedTime.Value.CompareTo(a.CompletedTime.Value) : 0));
      int num = 0;
      foreach (DisplayItemModel displayItemModel in list)
      {
        displayItemModel.SpecialOrder = (long) num;
        DateTime? completedTime = displayItemModel.CompletedTime;
        if (completedTime.HasValue)
        {
          completedTime = displayItemModel.CompletedTime;
          DateTime date = completedTime.Value.Date;
          if (!dateTimeList.Contains(date))
          {
            dateTimeList.Add(date);
            string name = DateUtils.FormatTimeDesc(date, true) + date.ToString(" ddd");
            string str = DateUtils.FormatTimeDesc(date, true);
            int oridinal = num++;
            DateTime? sectionDate = new DateTime?(date);
            string sectionId = str;
            completedSection.Add(new Section(name, oridinal, sectionDate, sectionId)
            {
              Children = {
                displayItemModel
              }
            });
          }
          else
          {
            int index = dateTimeList.IndexOf(date);
            completedSection[index].Children.Add(displayItemModel);
          }
        }
      }
      return (IEnumerable<Section>) completedSection;
    }

    private static async Task GroupAsTag(
      IReadOnlyCollection<DisplayItemModel> models,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      string tagParent,
      HashSet<string> openedTaskIds,
      IComparer<DisplayItemModel> comparer,
      ProjectSortExtra extra)
    {
      if (models == null || models.Count <= 0)
        return;
      List<TagModel> tags = TagDataHelper.GetTags();
      Dictionary<string, TagSection> tagSections = new Dictionary<string, TagSection>();
      HabitSection habits = new HabitSection(extra.IsWeek);
      Dictionary<string, TagModel> dictionary = new Dictionary<string, TagModel>();
      Dictionary<string, long> tagSortDict = TagDataHelper.GetTagSortDict();
      foreach (DisplayItemModel model in (IEnumerable<DisplayItemModel>) models)
      {
        if (model.Habit == null)
        {
          if (model.Tags != null && model.Tags.Length != 0)
          {
            List<string> limits = (List<string>) null;
            if (!string.IsNullOrEmpty(tagParent))
              limits = tags.Where<TagModel>((Func<TagModel, bool>) (t => t.parent == tagParent || t.name == tagParent)).Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>();
            string primaryTag = TagDataHelper.GetPrimaryTag(tagSortDict, (IList<string>) ((IEnumerable<string>) model.Tags).ToList<string>(), (ICollection<string>) limits);
            TagModel tagModel = tags.FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == primaryTag));
            dictionary[model.Id] = tagModel;
          }
          else
            dictionary[model.Id] = (TagModel) null;
        }
      }
      Dictionary<string, Node<DisplayItemModel>> nodeDict = SortHelper.GetNodeDict(models.ToList<DisplayItemModel>());
      TaskNodeUtils.BuildNodeTree<DisplayItemModel>(nodeDict);
      foreach (Node<DisplayItemModel> node in nodeDict.Values.Where<Node<DisplayItemModel>>((Func<Node<DisplayItemModel>, bool>) (node => !node.HasParent)))
      {
        node.GetAllChildrenValue();
        DisplayItemModel model = node.Value;
        if (model.Habit != null)
        {
          habits.Children.Add(model);
        }
        else
        {
          if (openedTaskIds != null)
            model.IsOpen = openedTaskIds.Contains(model.Id);
          TagModel tagModel = dictionary.ContainsKey(model.Id) ? dictionary[model.Id] : (TagModel) null;
          if (tagModel != null && model.Tags != null && model.Tags.Length != 0)
          {
            if (!tagSections.ContainsKey(tagModel.name))
            {
              TagSection tagSection1 = new TagSection(tagModel.GetDisplayName());
              tagSection1.Ordinal = tagModel.sortOrder;
              tagSection1.SectionId = tagModel.name;
              tagSection1.SectionEntityId = tagModel.name;
              TagSection tagSection2 = tagSection1;
              tagSections.Add(tagModel.name, tagSection2);
              tagSections[tagModel.name].Children.Add(model);
            }
            else
              tagSections[tagModel.name].Children.Add(model);
          }
          else
            SortHelper.TryAddNoTagSection(tagSections, model, node);
        }
      }
      foreach (TagSection section in tagSections.Values.ToList<TagSection>().OrderBy<TagSection, long>((Func<TagSection, long>) (p => p.Ordinal)).ToList<TagSection>())
      {
        if (!extra.InKanban)
          await SortHelper.SetItemSpecialOrder((Section) section, extra.SortKey, extra.SortCatId, extra.DefaultOrder);
        SortHelper.AssembleSection((Section) section, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      }
      SortHelper.AssembleSection((Section) habits, (IComparer<DisplayItemModel>) null, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
      habits = (HabitSection) null;
    }

    private static async Task SortByOrders(
      IList<DisplayItemModel> models,
      List<SyncSortOrderModel> orders,
      IEqualityComparer<SyncSortOrderModel> comparer = null)
    {
      comparer = comparer ?? (IEqualityComparer<SyncSortOrderModel>) new DefaultSortOrderModelComparer();
      List<DisplayItemModel> sortedModels = new List<DisplayItemModel>();
      if (orders != null && orders.Any<SyncSortOrderModel>())
      {
        orders.Sort((Comparison<SyncSortOrderModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
        foreach (SyncSortOrderModel syncSortOrderModel in orders.Distinct<SyncSortOrderModel>(comparer))
        {
          SyncSortOrderModel sortModel = syncSortOrderModel;
          DisplayItemModel displayItemModel = models.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.Id == sortModel.EntityId));
          if (displayItemModel != null)
          {
            sortedModels.Add(displayItemModel);
            models.Remove(displayItemModel);
            displayItemModel.SpecialOrder = sortModel.SortOrder;
          }
        }
        if (models.Count > 0 && sortedModels.Count > 0)
        {
          SyncSortOrderModel minOrder = orders[0];
          long sortOrder = minOrder.SortOrder;
          List<SyncSortOrderModel> newOrders = new List<SyncSortOrderModel>();
          for (int index = models.Count - 1; index >= 0; --index)
          {
            sortOrder -= 268435456L;
            DisplayItemModel model = models[index];
            model.SpecialOrder = sortOrder;
            sortedModels.Insert(0, model);
            SyncSortOrderModel dest = new SyncSortOrderModel();
            SyncSortOrderModel.Copy(dest, minOrder);
            dest.SyncStatus = 1;
            dest.EntityId = model.Id;
            dest.SortOrder = model.SpecialOrder;
            newOrders.Add(dest);
          }
          models.Clear();
          int num = await SyncSortOrderDao.InsertAllAsync(newOrders);
          TaskSortOrderService.AddSortModels(minOrder.SortOrderType, minOrder.GroupId, newOrders);
          SyncManager.TryDelaySync();
          minOrder = (SyncSortOrderModel) null;
          newOrders = (List<SyncSortOrderModel>) null;
        }
      }
      sortedModels.ForEach((Action<DisplayItemModel>) (item => models.Add(item)));
      sortedModels = (List<DisplayItemModel>) null;
    }

    private static async Task AssembleStarredSection(
      List<DisplayItemModel> starred,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds = null,
      List<SyncSortOrderModel> sortOrderPinned = null)
    {
      PinnedSection starredSection = new PinnedSection();
      starred.Sort((Comparison<DisplayItemModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
      starred.Sort((Comparison<DisplayItemModel>) ((a, b) => b.PinnedTime.CompareTo(a.PinnedTime)));
      await SortHelper.SortByOrders((IList<DisplayItemModel>) starred, sortOrderPinned, (IEqualityComparer<SyncSortOrderModel>) new TaskPinnedSortOrderModelComparer());
      List<Node<DisplayItemModel>> sortedTaskNodeTree = TaskNodeUtils.GetSortedTaskNodeTree((IEnumerable<DisplayItemModel>) starred);
      if (sortedTaskNodeTree != null && sortedTaskNodeTree.Any<Node<DisplayItemModel>>())
      {
        foreach (Node<DisplayItemModel> node in sortedTaskNodeTree)
        {
          node.GetAllChildrenValue();
          starredSection.Children.Add(node.Value);
        }
      }
      if (starredSection.Children.Count <= 0)
      {
        starredSection = (PinnedSection) null;
      }
      else
      {
        SortHelper.AssembleSection((Section) starredSection, (IComparer<DisplayItemModel>) null, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, false);
        starredSection = (PinnedSection) null;
      }
    }

    private static void TryAddNoTagSection(
      Dictionary<string, TagSection> tagSections,
      DisplayItemModel model,
      Node<DisplayItemModel> node)
    {
      string str = Utils.GetString("NoTags");
      if (!tagSections.ContainsKey(str))
      {
        Dictionary<string, TagSection> dictionary = tagSections;
        string key = str;
        TagSection tagSection = new TagSection(str);
        tagSection.Ordinal = 9223372036854775806L;
        tagSection.SectionId = "notag";
        tagSection.SectionEntityId = "noTag";
        dictionary.Add(key, tagSection);
      }
      tagSections[str].Children.Add(model);
    }

    private static async Task GroupAsAssignee(
      IReadOnlyCollection<DisplayItemModel> models,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds,
      IComparer<DisplayItemModel> comparer,
      ProjectSortExtra extra)
    {
      if (models == null || models.Count <= 0)
        return;
      Dictionary<string, Node<DisplayItemModel>> nodeDict = SortHelper.GetNodeDict(models.ToList<DisplayItemModel>());
      TaskNodeUtils.BuildNodeTree<DisplayItemModel>(nodeDict);
      Dictionary<string, AssigneeSection> dictionary1 = new Dictionary<string, AssigneeSection>();
      List<Node<DisplayItemModel>> list1 = nodeDict.Values.Where<Node<DisplayItemModel>>((Func<Node<DisplayItemModel>, bool>) (n => !n.HasParent)).ToList<Node<DisplayItemModel>>();
      List<AvatarViewModel> source = extra.InGroup ? AvatarHelper.GetProjectAvatarsFromCacheInGroup(extra.SortCatId) : AvatarHelper.GetProjectAvatarsFromCache(extra.SortCatId, true);
      for (int index = 0; index < list1.Count; ++index)
      {
        list1[index].GetAllChildrenValue();
        DisplayItemModel displayItemModel = list1[index].Value;
        if (openedTaskIds != null)
          displayItemModel.IsOpen = openedTaskIds.Contains(displayItemModel.Id);
        string userId = displayItemModel.Assignee;
        if (string.IsNullOrEmpty(displayItemModel.Assignee) || displayItemModel.Assignee == "-1")
          userId = "-1";
        if (!dictionary1.ContainsKey(userId))
        {
          string str = string.Empty;
          if (index + 1 < list1.Count)
            str = list1[index + 1].Value.ProjectId;
          AvatarViewModel avatarViewModel = userId == "-1" ? (AvatarViewModel) null : source.FirstOrDefault<AvatarViewModel>((Func<AvatarViewModel, bool>) (u => (u.UserId ?? "") == userId));
          Dictionary<string, AssigneeSection> dictionary2 = dictionary1;
          string key = userId;
          AssigneeSection assigneeSection1;
          if (!(userId == "-1"))
          {
            AssigneeSection assigneeSection2 = new AssigneeSection();
            assigneeSection2.SectionId = userId;
            assigneeSection2.Name = avatarViewModel?.Name;
            assigneeSection2.Assingee = userId;
            assigneeSection2.Ordinal = avatarViewModel?.UserId == LocalSettings.Settings.LoginUserId ? long.MinValue : (long) source.IndexOf(avatarViewModel);
            assigneeSection2.ProjectId = str;
            assigneeSection2.SectionEntityId = userId;
            assigneeSection1 = assigneeSection2;
          }
          else
            assigneeSection1 = (AssigneeSection) new NoAssingeeSection();
          dictionary2.Add(key, assigneeSection1);
        }
        dictionary1[userId].Children.Add(displayItemModel);
      }
      List<AssigneeSection> list2 = dictionary1.Values.ToList<AssigneeSection>();
      list2.Sort((Comparison<AssigneeSection>) ((a, b) => a.Ordinal.CompareTo(b.Ordinal)));
      foreach (AssigneeSection section in list2)
      {
        if (!extra.InKanban)
          await SortHelper.SetItemSpecialOrder((Section) section, extra.SortKey, extra.SortCatId, extra.DefaultOrder);
        SortHelper.AssembleSection((Section) section, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      }
    }

    private static async Task GroupAsPriority(
      IReadOnlyCollection<DisplayItemModel> models,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds,
      IComparer<DisplayItemModel> comparer,
      IComparer<DisplayItemModel> noteComparer,
      ProjectSortExtra extra)
    {
      if (models == null || models.Count <= 0)
        return;
      List<Node<DisplayItemModel>> sortedTaskNodeTree = TaskNodeUtils.GetSortedTaskNodeTree((IEnumerable<DisplayItemModel>) models);
      HighPrioritySection high = new HighPrioritySection();
      MediumPrioritySection medium = new MediumPrioritySection();
      LowPrioritySection low = new LowPrioritySection();
      NoPrioritySection none = new NoPrioritySection();
      HabitSection habits = new HabitSection(extra.IsWeek);
      NoteSection notes = new NoteSection();
      foreach (Node<DisplayItemModel> node in sortedTaskNodeTree)
      {
        DisplayItemModel displayItemModel = node.Value;
        node.GetAllChildrenValue();
        if (displayItemModel.IsNote && displayItemModel.Priority != 0)
          displayItemModel.SourceViewModel.Priority = 0;
        if (displayItemModel.Habit != null)
          habits.Children.Add(displayItemModel);
        else if (displayItemModel.IsNote)
        {
          notes.Children.Add(displayItemModel);
        }
        else
        {
          if (openedTaskIds != null)
            displayItemModel.IsOpen = openedTaskIds.Contains(displayItemModel.Id);
          switch (displayItemModel.Priority)
          {
            case 1:
              low.Children.Add(displayItemModel);
              continue;
            case 3:
              medium.Children.Add(displayItemModel);
              continue;
            case 5:
              high.Children.Add(displayItemModel);
              continue;
            default:
              none.Children.Add(displayItemModel);
              continue;
          }
        }
      }
      if (!extra.InKanban)
        await Task.WhenAll(SortHelper.SetItemSpecialOrder((Section) high, extra.SortKey, extra.SortCatId, extra.DefaultOrder), SortHelper.SetItemSpecialOrder((Section) medium, extra.SortKey, extra.SortCatId, extra.DefaultOrder), SortHelper.SetItemSpecialOrder((Section) low, extra.SortKey, extra.SortCatId, extra.DefaultOrder), SortHelper.SetItemSpecialOrder((Section) none, extra.SortKey, extra.SortCatId, extra.DefaultOrder));
      SortHelper.AssembleSection((Section) high, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      SortHelper.AssembleSection((Section) medium, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      SortHelper.AssembleSection((Section) low, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      SortHelper.AssembleSection((Section) none, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      SortHelper.AssembleSection((Section) notes, noteComparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
      SortHelper.AssembleSection((Section) habits, (IComparer<DisplayItemModel>) null, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
      high = (HighPrioritySection) null;
      medium = (MediumPrioritySection) null;
      low = (LowPrioritySection) null;
      none = (NoPrioritySection) null;
      habits = (HabitSection) null;
      notes = (NoteSection) null;
    }

    private static async Task SetItemSpecialOrder(
      Section section,
      string key,
      string catId,
      long def)
    {
      List<DisplayItemModel> children = section.Children;
      // ISSUE: explicit non-virtual call
      if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) == 0 || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(section.SectionEntityId))
        return;
      List<SyncSortOrderModel> asyncList = await TaskSortOrderService.GetAsyncList(string.Format(key, (object) section.SectionEntityId), catId);
      foreach (DisplayItemModel child1 in section.Children)
      {
        DisplayItemModel model = child1;
        long? nullable1;
        long? nullable2;
        if (asyncList == null)
        {
          nullable1 = new long?();
          nullable2 = nullable1;
        }
        else
        {
          SyncSortOrderModel syncSortOrderModel = asyncList.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == model.EntityId));
          if (syncSortOrderModel == null)
          {
            nullable1 = new long?();
            nullable2 = nullable1;
          }
          else
            nullable2 = new long?(syncSortOrderModel.SortOrder);
        }
        long? nullable3 = nullable2;
        DisplayItemModel displayItemModel1 = model;
        nullable1 = nullable3;
        long num1 = nullable1 ?? def;
        displayItemModel1.SpecialOrder = num1;
        List<DisplayItemModel> childrenModels = model.GetChildrenModels(true);
        // ISSUE: explicit non-virtual call
        if (childrenModels != null && __nonvirtual (childrenModels.Count) > 0)
        {
          foreach (DisplayItemModel displayItemModel2 in childrenModels)
          {
            DisplayItemModel child = displayItemModel2;
            long? nullable4;
            if (asyncList == null)
            {
              nullable1 = new long?();
              nullable4 = nullable1;
            }
            else
            {
              SyncSortOrderModel syncSortOrderModel = asyncList.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == child.EntityId));
              if (syncSortOrderModel == null)
              {
                nullable1 = new long?();
                nullable4 = nullable1;
              }
              else
                nullable4 = new long?(syncSortOrderModel.SortOrder);
            }
            long? nullable5 = nullable4;
            DisplayItemModel displayItemModel3 = child;
            nullable1 = nullable5;
            long num2 = nullable1 ?? def;
            displayItemModel3.SpecialOrder = num2;
          }
        }
      }
    }

    private static async Task GroupAsNone(
      IReadOnlyCollection<DisplayItemModel> models,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds,
      bool existStarred,
      IComparer<DisplayItemModel> comparer,
      ProjectSortExtra extra)
    {
      if (!extra.InKanban && extra.SortKey != null)
      {
        List<SyncSortOrderModel> asyncList = await TaskSortOrderService.GetAsyncList(string.Format(extra.SortKey, (object) "none"), extra.SortCatId);
        long num1 = TaskDefaultDao.GetDefaultSafely().AddTo == 0 ? long.MinValue : long.MaxValue;
        foreach (DisplayItemModel model1 in (IEnumerable<DisplayItemModel>) models)
        {
          DisplayItemModel model = model1;
          long? nullable1;
          long? nullable2;
          if (asyncList == null)
          {
            nullable1 = new long?();
            nullable2 = nullable1;
          }
          else
          {
            SyncSortOrderModel syncSortOrderModel = asyncList.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == model.EntityId));
            if (syncSortOrderModel == null)
            {
              nullable1 = new long?();
              nullable2 = nullable1;
            }
            else
              nullable2 = new long?(syncSortOrderModel.SortOrder);
          }
          long? nullable3 = nullable2;
          DisplayItemModel displayItemModel = model;
          nullable1 = nullable3;
          long num2 = nullable1 ?? num1;
          displayItemModel.SpecialOrder = num2;
        }
      }
      HabitSection habitSection = new HabitSection(extra.IsWeek);
      List<DisplayItemModel> list = TaskNodeUtils.GetModelsFromNodes<DisplayItemModel>(TaskNodeUtils.GetSortedTaskNodeTree((IEnumerable<DisplayItemModel>) models)).Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Level == 0)).ToList<DisplayItemModel>();
      UnpinnedSection unpinnedSection1 = new UnpinnedSection();
      unpinnedSection1.SectionEntityId = "none";
      UnpinnedSection unpinnedSection2 = unpinnedSection1;
      foreach (DisplayItemModel displayItemModel in list)
      {
        if (displayItemModel.Habit != null)
        {
          habitSection.Children.Add(displayItemModel);
        }
        else
        {
          if (openedTaskIds != null)
            displayItemModel.IsOpen = openedTaskIds.Contains(displayItemModel.Id);
          unpinnedSection2.Children.Add(displayItemModel);
        }
      }
      if (unpinnedSection2.Children.Count > 0)
        SortHelper.AssembleSection((Section) unpinnedSection2, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, extra: extra, showSection: existStarred);
      SortHelper.AssembleSection((Section) habitSection, (IComparer<DisplayItemModel>) null, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, showSection: !extra.InKanban);
    }

    private static async Task GroupAsDate(
      IReadOnlyCollection<DisplayItemModel> models,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds,
      IComparer<DisplayItemModel> comparer,
      IComparer<DisplayItemModel> noteComparer,
      ProjectSortExtra extra)
    {
      if (extra.IsToday)
        await SortHelper.SortAsDateInToday((IEnumerable<DisplayItemModel>) models, sectionStatusModels, results, openedTaskIds, comparer, noteComparer, extra);
      else if (extra.IsTomorrow)
        await SortHelper.SortAsDateInTomorrow((IEnumerable<DisplayItemModel>) models, sectionStatusModels, results, openedTaskIds, comparer, noteComparer, extra);
      else if (extra.IsWeek)
        await SortHelper.SortAsDateInWeek((IEnumerable<DisplayItemModel>) models, sectionStatusModels, results, openedTaskIds, comparer, noteComparer, extra);
      else
        await SortHelper.SortAsDateNormal((IEnumerable<DisplayItemModel>) models, sectionStatusModels, results, openedTaskIds, comparer, noteComparer, extra);
    }

    private static async Task SortAsDateNormal(
      IEnumerable<DisplayItemModel> models,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds,
      IComparer<DisplayItemModel> comparer,
      IComparer<DisplayItemModel> noteComparer,
      ProjectSortExtra extra)
    {
      DateSection section = new DateSection();
      Dictionary<string, Node<DisplayItemModel>> nodeDict = SortHelper.GetNodeDict(models.ToList<DisplayItemModel>());
      NoteSection notes = new NoteSection();
      TaskNodeUtils.BuildNodeTree<DisplayItemModel>(nodeDict);
      foreach (Node<DisplayItemModel> node in nodeDict.Values.Where<Node<DisplayItemModel>>((Func<Node<DisplayItemModel>, bool>) (n => !n.HasParent)))
      {
        DisplayItemModel displayItemModel = node.Value;
        node.GetAllChildrenValue();
        if (openedTaskIds != null)
          displayItemModel.IsOpen = openedTaskIds.Contains(displayItemModel.Id);
        if (displayItemModel.IsNote)
        {
          notes.Children.Add(displayItemModel);
        }
        else
        {
          switch (DateUtils.GetSectionCategory(displayItemModel.StartDate, displayItemModel.DueDate, displayItemModel.IsAllDay))
          {
            case DateUtils.DateSectionCategory.NoDate:
              section.Nodate.Children.Add(displayItemModel);
              continue;
            case DateUtils.DateSectionCategory.OutDated:
              section.Outdated.Children.Add(displayItemModel);
              continue;
            case DateUtils.DateSectionCategory.Today:
              section.Today.Children.Add(displayItemModel);
              continue;
            case DateUtils.DateSectionCategory.Tomorrow:
              section.Tomorrow.Children.Add(displayItemModel);
              continue;
            case DateUtils.DateSectionCategory.ThisWeek:
              section.Week.Children.Add(displayItemModel);
              continue;
            case DateUtils.DateSectionCategory.Future:
              section.Later.Children.Add(displayItemModel);
              continue;
            default:
              section.Nodate.Children.Add(displayItemModel);
              continue;
          }
        }
      }
      if (section.Outdated is OutdatedSection outdated)
        outdated.ShowPostpone = extra.IsAll;
      if (LocalSettings.Settings.PosOfOverdue == 0)
      {
        if (comparer is SortHelper.DateComparer dateComparer)
          dateComparer.SetCheckSpecial(false);
        SortHelper.AssembleSection(section.Outdated, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra.ExistSpecialOrderItem ? extra : (ProjectSortExtra) null);
        dateComparer?.SetCheckSpecial(true);
      }
      if (!extra.InKanban)
        await Task.WhenAll(SortHelper.SetItemSpecialOrder(section.Today, extra.SortKey, extra.SortCatId, extra.DefaultOrder), SortHelper.SetItemSpecialOrder(section.Tomorrow, extra.SortKey, extra.SortCatId, extra.DefaultOrder), SortHelper.SetItemSpecialOrder(section.Nodate, extra.SortKey, extra.SortCatId, extra.DefaultOrder));
      SortHelper.AssembleSection(section.Today, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      SortHelper.AssembleSection(section.Tomorrow, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      SortHelper.AssembleSection(section.Week, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra.ExistSpecialOrderItem ? extra : (ProjectSortExtra) null);
      SortHelper.AssembleSection(section.Later, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra.ExistSpecialOrderItem ? extra : (ProjectSortExtra) null);
      SortHelper.AssembleSection(section.Nodate, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      if (LocalSettings.Settings.PosOfOverdue == 1)
      {
        if (comparer is SortHelper.DateComparer dateComparer)
          dateComparer.SetCheckSpecial(false);
        SortHelper.AssembleSection(section.Outdated, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra.ExistSpecialOrderItem ? extra : (ProjectSortExtra) null);
        dateComparer?.SetCheckSpecial(true);
      }
      SortHelper.AssembleSection((Section) notes, noteComparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds);
      section = (DateSection) null;
      notes = (NoteSection) null;
    }

    private static async Task SortAsDateInWeek(
      IEnumerable<DisplayItemModel> models,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds,
      IComparer<DisplayItemModel> comparer,
      IComparer<DisplayItemModel> noteComparer,
      ProjectSortExtra extra)
    {
      List<Section> weekSections = SortHelper.GenerateWeekSections();
      HabitSection habits = new HabitSection(true, true);
      Dictionary<string, Node<DisplayItemModel>> nodeDict = SortHelper.GetNodeDict(models.ToList<DisplayItemModel>());
      NoteSection notes = new NoteSection();
      TaskNodeUtils.BuildNodeTree<DisplayItemModel>(nodeDict);
      foreach (Node<DisplayItemModel> node in nodeDict.Values.Where<Node<DisplayItemModel>>((Func<Node<DisplayItemModel>, bool>) (n => !n.HasParent)))
      {
        DisplayItemModel displayItemModel = node.Value;
        if (displayItemModel.Habit != null)
          habits.Children.Add(displayItemModel);
        else if (displayItemModel.IsNote)
        {
          notes.Children.Add(displayItemModel);
        }
        else
        {
          node.GetAllChildrenValue();
          if (displayItemModel.StartDate.HasValue)
            SortHelper.GetWeekSection(displayItemModel.StartDate, displayItemModel.DueDate, displayItemModel.IsAllDay, (IReadOnlyList<Section>) weekSections)?.Children.Add(displayItemModel);
        }
      }
      Section outDated = weekSections.FirstOrDefault<Section>((Func<Section, bool>) (section => section.SectionId == "outdated"));
      if (LocalSettings.Settings.PosOfOverdue == 0)
      {
        if (comparer is SortHelper.DateComparer dateComparer)
          dateComparer.SetCheckSpecial(false);
        SortHelper.AssembleSection(outDated, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds);
        dateComparer?.SetCheckSpecial(true);
      }
      foreach (Section section in weekSections.Where<Section>((Func<Section, bool>) (section => section.SectionId != "outdated")))
      {
        if (!extra.InKanban)
          await SortHelper.SetItemSpecialOrder(section, extra.SortKey, extra.SortCatId, extra.DefaultOrder);
        SortHelper.AssembleSection(section, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
        if (extra.ShowFold)
        {
          DateTime? sectionDate = section.SectionDate;
          DateTime today = DateTime.Today;
          if ((sectionDate.HasValue ? (sectionDate.HasValue ? (sectionDate.GetValueOrDefault() == today ? 1 : 0) : 1) : 0) != 0)
            SortHelper.AssembleSection((Section) habits, (IComparer<DisplayItemModel>) null, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
        }
      }
      if (LocalSettings.Settings.PosOfOverdue == 1)
      {
        if (comparer is SortHelper.DateComparer dateComparer)
          dateComparer.SetCheckSpecial(false);
        SortHelper.AssembleSection(outDated, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds);
        dateComparer?.SetCheckSpecial(true);
      }
      SortHelper.AssembleSection((Section) notes, noteComparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds);
      if (extra.ShowFold)
      {
        habits = (HabitSection) null;
        notes = (NoteSection) null;
        outDated = (Section) null;
      }
      else
      {
        SortHelper.AssembleSection((Section) habits, (IComparer<DisplayItemModel>) null, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
        habits = (HabitSection) null;
        notes = (NoteSection) null;
        outDated = (Section) null;
      }
    }

    private static Dictionary<string, Node<DisplayItemModel>> GetNodeDict(
      List<DisplayItemModel> displayItemModels)
    {
      Dictionary<string, Node<DisplayItemModel>> nodeDict = new Dictionary<string, Node<DisplayItemModel>>();
      foreach (DisplayItemModel displayItemModel in displayItemModels)
      {
        if (!string.IsNullOrEmpty(displayItemModel.Id) && !nodeDict.ContainsKey(displayItemModel.Id))
          nodeDict.Add(displayItemModel.Id, (Node<DisplayItemModel>) new DisplayItemNode(displayItemModel));
      }
      return nodeDict;
    }

    private static Section GetWeekSection(
      DateTime? startDate,
      DateTime? dueDate,
      bool? isAllDay,
      IReadOnlyList<Section> weekSections)
    {
      DateTime checkDate = startDate ?? DateTime.Today;
      if (startDate.HasValue)
      {
        DateTime date = startDate.Value.Date;
        DateTime today = DateTime.Today;
        DateTime dateTime = today.AddDays(1.0);
        if (date == dateTime)
        {
          today = DateTime.Today;
          checkDate = today.AddDays(1.0);
          goto label_5;
        }
      }
      if (DateUtils.GetSectionCategory(startDate, dueDate, isAllDay) == DateUtils.DateSectionCategory.Today)
        checkDate = DateTime.Today;
label_5:
      return weekSections.FirstOrDefault<Section>((Func<Section, bool>) (section => section.SectionDate.HasValue && section.SectionDate.Value.Date == checkDate.Date)) ?? weekSections[0];
    }

    private static List<Section> GenerateWeekSections()
    {
      List<Section> sectionList = new List<Section>();
      OutdatedSection outdatedSection = new OutdatedSection();
      outdatedSection.Ordinal = 8L;
      outdatedSection.ShowPostpone = true;
      sectionList.Add((Section) outdatedSection);
      sectionList.Add(new Section(string.Format(Utils.GetString("7DayThisWeek"), (object) Utils.GetString("Today"), (object) DateTime.Now.ToString("ddd", (IFormatProvider) App.Ci)), 7, new DateTime?(DateTime.Now.Date), "today", canSort: true, canSwitch: true)
      {
        SectionEntityId = DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) DateTimeFormatInfo.InvariantInfo)
      });
      string format = Utils.GetString("7DayThisWeek");
      string str1 = Utils.GetString("Tomorrow");
      DateTime dateTime1 = DateTime.Now;
      dateTime1 = dateTime1.AddDays(1.0);
      string str2 = dateTime1.ToString("ddd", (IFormatProvider) App.Ci);
      string name = string.Format(format, (object) str1, (object) str2);
      DateTime dateTime2 = DateTime.Now;
      dateTime2 = dateTime2.AddDays(1.0);
      DateTime? sectionDate = new DateTime?(dateTime2.Date);
      Section section1 = new Section(name, 6, sectionDate, "tomorrow", canSort: true, canSwitch: true);
      DateTime dateTime3 = DateTime.Today;
      dateTime3 = dateTime3.AddDays(1.0);
      section1.SectionEntityId = dateTime3.ToString("yyyyMMdd", (IFormatProvider) DateTimeFormatInfo.InvariantInfo);
      sectionList.Add(section1);
      List<Section> weekSections = sectionList;
      DayOfWeek nextDayOfWeek = Utils.GetNextDayOfWeek();
      DateTime dateTime4 = DateTime.Today;
      for (int index = 0; index < 7; ++index)
      {
        dateTime4 = dateTime4.AddDays(1.0);
        if (dateTime4.DayOfWeek == nextDayOfWeek)
          break;
      }
      for (int index = 2; index < 7; ++index)
      {
        DateTime dateTime5 = DateTime.Today;
        DateTime date = dateTime5.AddDays((double) index);
        Section section2 = new Section(true, true);
        dateTime5 = date.Date;
        section2.SectionId = dateTime5.ToString("yyyyMMdd", (IFormatProvider) App.Ci);
        section2.Ordinal = (long) (7 - index);
        section2.SectionDate = new DateTime?(date.Date);
        section2.Name = string.Format(date >= dateTime4 ? Utils.GetString("7DayTodayNextWeek") : Utils.GetString("7DayThisWeek"), (object) DateUtils.FormatShortDate(date), (object) DateUtils.FormatWeekDayName(date));
        dateTime5 = DateTime.Today;
        dateTime5 = dateTime5.AddDays((double) index);
        section2.SectionEntityId = dateTime5.ToString("yyyyMMdd", (IFormatProvider) DateTimeFormatInfo.InvariantInfo);
        Section section3 = section2;
        weekSections.Add(section3);
      }
      return weekSections;
    }

    private static async Task SortAsDateInTomorrow(
      IEnumerable<DisplayItemModel> models,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds,
      IComparer<DisplayItemModel> comparer,
      IComparer<DisplayItemModel> noteComparer,
      ProjectSortExtra extra)
    {
      TomorrowSection section = new TomorrowSection();
      Dictionary<string, Node<DisplayItemModel>> nodeDict = SortHelper.GetNodeDict(models.ToList<DisplayItemModel>());
      NoteSection notes = new NoteSection();
      TaskNodeUtils.BuildNodeTree<DisplayItemModel>(nodeDict);
      foreach (Node<DisplayItemModel> node in nodeDict.Values.Where<Node<DisplayItemModel>>((Func<Node<DisplayItemModel>, bool>) (n => !n.HasParent)))
      {
        DisplayItemModel displayItemModel = node.Value;
        if (displayItemModel.IsNote)
        {
          notes.Children.Add(displayItemModel);
        }
        else
        {
          node.GetAllChildrenValue();
          section.Children.Add(displayItemModel);
        }
      }
      if (!extra.InKanban)
        await SortHelper.SetItemSpecialOrder((Section) section, extra.SortKey, extra.SortCatId, extra.DefaultOrder);
      SortHelper.AssembleSection((Section) section, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      SortHelper.AssembleSection((Section) notes, noteComparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds);
      section = (TomorrowSection) null;
      notes = (NoteSection) null;
    }

    private static void AssembleSection(
      Section section,
      IComparer<DisplayItemModel> comparer,
      IEnumerable<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds = null,
      bool compareChild = true,
      ProjectSortExtra extra = null,
      bool showSection = true)
    {
      if (!section.Customized && section.Children.Count <= 0)
        return;
      DisplayItemModel displayItemModel = DisplayItemModel.BuildSection(section);
      displayItemModel.IsOpen = !SortHelper.IsSectionClosed(section, sectionStatusModels);
      if (showSection)
        results.Add(displayItemModel);
      if (comparer != null)
      {
        List<DisplayItemModel> list = section.Children.OrderBy<DisplayItemModel, DisplayItemModel>((Func<DisplayItemModel, DisplayItemModel>) (x => x), comparer).ToList<DisplayItemModel>();
        section.Children = list;
      }
      bool flag = extra != null && !string.IsNullOrEmpty(extra.SortKey) && (extra.SortOption?.orderBy != "sortOrder" && !string.IsNullOrEmpty(section.SectionEntityId) || extra.ExistSpecialOrderItem);
      List<DisplayItemModel> allItems = new List<DisplayItemModel>();
      foreach (DisplayItemModel child1 in section.Children)
      {
        allItems.Add(child1);
        child1.Section = section;
        child1.Parent = displayItemModel;
        if (openedTaskIds != null)
          child1.IsOpen = openedTaskIds.Contains(child1.Id);
        if (compareChild)
          child1.SortChildren(comparer);
        if (displayItemModel.IsOpen || !showSection)
        {
          results.Add(child1);
          if (child1.IsOpen)
            child1.GetChildrenModels(false, openedTaskIds).ForEach((Action<DisplayItemModel>) (child =>
            {
              child.Section = section;
              if (openedTaskIds != null)
                child.IsOpen = openedTaskIds.Contains(child.Id);
              results.Add(child);
            }));
        }
        if (flag)
          allItems.AddRange((IEnumerable<DisplayItemModel>) child1.GetChildrenModels(true));
      }
      if (!flag)
        return;
      SortHelper.CheckSpecialOrder(allItems, section, extra);
    }

    private static void CheckSpecialOrder(
      List<DisplayItemModel> allItems,
      Section section,
      ProjectSortExtra extra)
    {
      string str = extra.InKanban ? extra.SortKey : string.Format(extra.SortKey, (object) section.SectionEntityId);
      long num1 = long.MinValue;
      long minValue = long.MinValue;
      List<SyncSortOrderModel> syncSortOrderModelList = new List<SyncSortOrderModel>();
      List<DisplayItemModel> displayItemModelList = new List<DisplayItemModel>();
      for (int index1 = 0; index1 < allItems.Count; ++index1)
      {
        DisplayItemModel allItem = allItems[index1];
        if (allItem.SpecialOrder == extra.DefaultOrder)
          displayItemModelList.Add(allItem);
        else if (displayItemModelList.Count > 0)
        {
          long specialOrder = allItem.SpecialOrder;
          long num2 = num1 == long.MinValue ? 268435456L : (specialOrder - num1) / (long) (displayItemModelList.Count + 1);
          for (int index2 = displayItemModelList.Count - 1; index2 >= 0; --index2)
          {
            DisplayItemModel displayItemModel = displayItemModelList[index2];
            displayItemModel.SpecialOrder = specialOrder - num2 * (long) (index2 + 1);
            SyncSortOrderModel syncSortOrderModel = new SyncSortOrderModel(str)
            {
              Type = EntityType.GetEntityTypeNum(displayItemModel.Type),
              SortOrder = displayItemModel.SpecialOrder,
              GroupId = extra.SortCatId,
              EntityId = displayItemModel.EntityId,
              SyncStatus = 1
            };
            syncSortOrderModelList.Add(syncSortOrderModel);
          }
          displayItemModelList.Clear();
          num1 = long.MinValue;
          minValue = long.MinValue;
        }
        else
          num1 = allItem.SpecialOrder;
      }
      if ((num1 != long.MinValue || extra.ExistSpecialOrderItem) && minValue == long.MinValue && displayItemModelList.Count > 0)
      {
        if (num1 == long.MinValue)
          num1 = (long) new Random().Next(1, 512);
        long num3 = 268435456;
        for (int index = displayItemModelList.Count - 1; index >= 0; --index)
        {
          DisplayItemModel displayItemModel = displayItemModelList[index];
          displayItemModel.SpecialOrder = num1 + num3 * (long) (index + 1);
          SyncSortOrderModel syncSortOrderModel = new SyncSortOrderModel(str)
          {
            Type = EntityType.GetEntityTypeNum(displayItemModel.Type),
            SortOrder = displayItemModel.SpecialOrder,
            GroupId = extra.SortCatId,
            EntityId = displayItemModel.EntityId,
            SyncStatus = 1
          };
          syncSortOrderModelList.Add(syncSortOrderModel);
        }
        displayItemModelList.Clear();
      }
      TaskSortOrderService.AddSortModels(str, extra.SortCatId, syncSortOrderModelList);
      SyncSortOrderDao.InsertAllAsync(syncSortOrderModelList);
      if (syncSortOrderModelList.Count <= 0)
        return;
      SyncManager.TryDelaySync(2000);
    }

    private static bool IsSectionClosed(
      Section section,
      IEnumerable<SectionStatusModel> sectionStatusModels)
    {
      AssigneeSection assigneeSection = section as AssigneeSection;
      return assigneeSection != null ? sectionStatusModels.Any<SectionStatusModel>((Func<SectionStatusModel, bool>) (model => model.Name == assigneeSection.Assingee)) : sectionStatusModels.Any<SectionStatusModel>((Func<SectionStatusModel, bool>) (model => model.Name == section.Name));
    }

    private static async Task SortAsDateInToday(
      IEnumerable<DisplayItemModel> models,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds,
      IComparer<DisplayItemModel> comparer,
      IComparer<DisplayItemModel> noteComparer,
      ProjectSortExtra extra)
    {
      TodaySection today = new TodaySection();
      OutdatedSection outDated = new OutdatedSection()
      {
        ShowPostpone = true
      };
      HabitSection habits = new HabitSection();
      NoteSection notes = new NoteSection();
      Dictionary<string, Node<DisplayItemModel>> nodeDict = SortHelper.GetNodeDict(models.ToList<DisplayItemModel>());
      TaskNodeUtils.BuildNodeTree<DisplayItemModel>(nodeDict);
      foreach (Node<DisplayItemModel> node in nodeDict.Values.Where<Node<DisplayItemModel>>((Func<Node<DisplayItemModel>, bool>) (n => !n.HasParent)))
      {
        DisplayItemModel displayItemModel = node.Value;
        if (displayItemModel.Habit != null)
          habits.Children.Add(displayItemModel);
        else if (displayItemModel.IsNote)
        {
          notes.Children.Add(displayItemModel);
        }
        else
        {
          if (openedTaskIds != null)
            displayItemModel.IsOpen = openedTaskIds.Contains(displayItemModel.Id);
          DateUtils.DateSectionCategory sectionCategory = DateUtils.GetSectionCategory(displayItemModel.StartDate, displayItemModel.DueDate, displayItemModel.IsAllDay);
          node.GetAllChildrenValue();
          switch (sectionCategory)
          {
            case DateUtils.DateSectionCategory.OutDated:
              outDated.Children.Add(displayItemModel);
              continue;
            case DateUtils.DateSectionCategory.Today:
              today.Children.Add(displayItemModel);
              continue;
            default:
              continue;
          }
        }
      }
      if (!extra.InKanban)
        await SortHelper.SetItemSpecialOrder((Section) today, extra.SortKey, extra.SortCatId, extra.DefaultOrder);
      if (LocalSettings.Settings.PosOfOverdue == 0)
      {
        SortHelper.AssembleSection((Section) outDated, comparer is SortHelper.SortOrderComparer ? comparer : (IComparer<DisplayItemModel>) new SortHelper.DateComparer(false, false), (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds);
        SortHelper.AssembleSection((Section) today, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
        SortHelper.AssembleSection((Section) notes, noteComparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
        SortHelper.AssembleSection((Section) habits, (IComparer<DisplayItemModel>) null, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
        today = (TodaySection) null;
        outDated = (OutdatedSection) null;
        habits = (HabitSection) null;
        notes = (NoteSection) null;
      }
      else
      {
        SortHelper.AssembleSection((Section) today, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
        SortHelper.AssembleSection((Section) outDated, comparer is SortHelper.SortOrderComparer ? comparer : (IComparer<DisplayItemModel>) new SortHelper.DateComparer(false, false), (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds);
        SortHelper.AssembleSection((Section) notes, noteComparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
        SortHelper.AssembleSection((Section) habits, (IComparer<DisplayItemModel>) null, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
        today = (TodaySection) null;
        outDated = (OutdatedSection) null;
        habits = (HabitSection) null;
        notes = (NoteSection) null;
      }
    }

    private static async Task GroupAsProject(
      IReadOnlyCollection<DisplayItemModel> models,
      List<SectionStatusModel> sectionStatusModels,
      ObservableCollection<DisplayItemModel> results,
      HashSet<string> openedTaskIds,
      IComparer<DisplayItemModel> comparer,
      ProjectSortExtra extra)
    {
      if (models == null || models.Count <= 0)
        return;
      Dictionary<string, ProjectSection> dictionary1 = new Dictionary<string, ProjectSection>();
      Dictionary<string, long> projectSortOrders = CacheManager.GetProjectSortOrders();
      List<ProjectModel> projects = CacheManager.GetProjects();
      HabitSection habits = new HabitSection(extra.IsWeek);
      TimetableSection courses = new TimetableSection();
      foreach (Node<DisplayItemModel> node in TaskNodeUtils.GetSortedTaskNodeTree((IEnumerable<DisplayItemModel>) models).Where<Node<DisplayItemModel>>((Func<Node<DisplayItemModel>, bool>) (n => !n.HasParent)))
      {
        DisplayItemModel model = node.Value;
        node.GetAllChildrenValue();
        if (model.Habit != null)
          habits.Children.Add(model);
        else if (model.IsCourse)
          courses.Children.Add(model);
        else if (model.ProjectId != null)
        {
          if (openedTaskIds != null)
            model.IsOpen = openedTaskIds.Contains(model.Id);
          if (!dictionary1.ContainsKey(model.ProjectId))
          {
            ProjectModel projectModel = projects != null ? projects.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.ProjectId)) : (ProjectModel) null;
            Dictionary<string, ProjectSection> dictionary2 = dictionary1;
            string projectId = model.ProjectId;
            ProjectSection projectSection = new ProjectSection(projectModel != null && projectModel.IsProjectPermit());
            projectSection.SectionId = model.ProjectId;
            projectSection.ProjectId = model.ProjectId;
            projectSection.Name = model.ProjectName;
            projectSection.Ordinal = SortHelper.GetProjectOrdinal(model, (IReadOnlyDictionary<string, long>) projectSortOrders);
            projectSection.SectionEntityId = model.ProjectId;
            dictionary2.Add(projectId, projectSection);
          }
          dictionary1[model.ProjectId].Children.Add(model);
        }
      }
      foreach (ProjectSection section in dictionary1.Values.ToList<ProjectSection>().OrderBy<ProjectSection, long>((Func<ProjectSection, long>) (p => p.Ordinal)).ToList<ProjectSection>())
      {
        if (!extra.InKanban)
          await SortHelper.SetItemSpecialOrder((Section) section, extra.SortKey, extra.SortCatId, extra.DefaultOrder);
        if (section.SectionId == "8ac3038d93c54b80a67321b6a03df066")
        {
          section.Name = Utils.GetString("SubscribeCalendar");
          SortHelper.AssembleSection((Section) section, (IComparer<DisplayItemModel>) new SortHelper.DateComparer(false), (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds);
        }
        else
          SortHelper.AssembleSection((Section) section, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
      }
      courses.Children.Sort((Comparison<DisplayItemModel>) ((a, b) =>
      {
        if (a.StartDate.HasValue && b.StartDate.HasValue)
          return a.StartDate.Value.CompareTo(b.StartDate.Value);
        return a.StartDate.HasValue ? -1 : 1;
      }));
      SortHelper.AssembleSection((Section) courses, (IComparer<DisplayItemModel>) null, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
      SortHelper.AssembleSection((Section) habits, (IComparer<DisplayItemModel>) null, (IEnumerable<SectionStatusModel>) sectionStatusModels, results);
      habits = (HabitSection) null;
      courses = (TimetableSection) null;
    }

    private static long GetProjectOrdinal(
      DisplayItemModel model,
      IReadOnlyDictionary<string, long> sorts)
    {
      if (model.IsEvent)
        return long.MaxValue;
      if (model.ProjectId == Utils.GetInboxId())
        return long.MinValue;
      return !sorts.ContainsKey(model.ProjectId) ? 0L : sorts[model.ProjectId];
    }

    private static async Task GroupAsSortOrder(
      List<DisplayItemModel> models,
      ObservableCollection<DisplayItemModel> results,
      List<ColumnModel> columns,
      List<SectionStatusModel> sectionStatusModels,
      HashSet<string> openedTaskIds,
      bool existStarred,
      IComparer<DisplayItemModel> comparer,
      ProjectSortExtra extra)
    {
      List<DisplayItemModel> sortedTreeModels = TaskNodeUtils.GetModelsFromNodes<DisplayItemModel>(TaskNodeUtils.GetSortedTaskNodeTree((IEnumerable<DisplayItemModel>) models));
      List<CustomizedSection> sections = new List<CustomizedSection>();
      List<ColumnModel> columnModelList = columns;
      // ISSUE: explicit non-virtual call
      if ((columnModelList != null ? (__nonvirtual (columnModelList.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        columns.Sort((Comparison<ColumnModel>) ((a, b) =>
        {
          if (a.sortOrder.HasValue && b.sortOrder.HasValue)
            return a.sortOrder.Value.CompareTo(b.sortOrder.Value);
          return b.sortOrder.HasValue ? -1 : 0;
        }));
        sections.AddRange(columns.Select<ColumnModel, CustomizedSection>((Func<ColumnModel, CustomizedSection>) (column => new CustomizedSection(column)
        {
          CanDelete = columns.Count > 1
        })));
      }
      Dictionary<string, List<SyncSortOrderModel>> orderDict = new Dictionary<string, List<SyncSortOrderModel>>();
      if (!string.IsNullOrEmpty(extra.SortKey))
      {
        if (sections.Count <= 1)
        {
          List<SyncSortOrderModel> asyncList = await TaskSortOrderService.GetAsyncList(string.Format(extra.SortKey, sections.Count == 1 ? (object) sections[0].SectionId : (object) "none"), extra.SortCatId);
          foreach (DisplayItemModel model1 in models)
          {
            DisplayItemModel model = model1;
            long? nullable1;
            long? nullable2;
            if (asyncList == null)
            {
              nullable1 = new long?();
              nullable2 = nullable1;
            }
            else
            {
              SyncSortOrderModel syncSortOrderModel = asyncList.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == model.EntityId));
              if (syncSortOrderModel == null)
              {
                nullable1 = new long?();
                nullable2 = nullable1;
              }
              else
                nullable2 = new long?(syncSortOrderModel.SortOrder);
            }
            long? nullable3 = nullable2;
            DisplayItemModel displayItemModel = model;
            nullable1 = nullable3;
            long num = nullable1 ?? extra.DefaultOrder;
            displayItemModel.SpecialOrder = num;
          }
        }
        else
        {
          foreach (CustomizedSection section in sections)
            orderDict[section.SectionId] = await TaskSortOrderService.GetAsyncList(string.Format(extra.SortKey, (object) section.SectionId), extra.SortCatId);
        }
      }
      if (sections.Count > 1 | existStarred)
      {
        foreach (DisplayItemModel displayItemModel1 in sortedTreeModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => model.Level == 0)))
        {
          DisplayItemModel model = displayItemModel1;
          long? nullable4;
          if (existStarred)
          {
            if (sections.Count > 0)
            {
              if (sections.Count == 1)
                sections[0].CanDelete = false;
              CustomizedSection customizedSection = sections.FirstOrDefault<CustomizedSection>((Func<CustomizedSection, bool>) (s => s.SectionId == model.ColumnId)) ?? sections[0];
              if (model.Status == 0 && model.ColumnId != customizedSection.SectionId && customizedSection.SectionId != "NotPinned" && model.ProjectId == customizedSection.ProjectId)
                TaskService.SaveTaskColumnId(model.Id, customizedSection.SectionId, true);
              customizedSection.Children.Add(model);
              if (sections.Count > 1 && !string.IsNullOrEmpty(extra.SortKey))
              {
                List<SyncSortOrderModel> source = customizedSection.SectionId == null || !orderDict.ContainsKey(customizedSection.SectionId) ? (List<SyncSortOrderModel>) null : orderDict[customizedSection.SectionId];
                long? nullable5;
                if (source == null)
                {
                  nullable4 = new long?();
                  nullable5 = nullable4;
                }
                else
                {
                  SyncSortOrderModel syncSortOrderModel = source.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == model.EntityId));
                  if (syncSortOrderModel == null)
                  {
                    nullable4 = new long?();
                    nullable5 = nullable4;
                  }
                  else
                    nullable5 = new long?(syncSortOrderModel.SortOrder);
                }
                long? nullable6 = nullable5;
                DisplayItemModel displayItemModel2 = model;
                nullable4 = nullable6;
                long num1 = nullable4 ?? extra.DefaultOrder;
                displayItemModel2.SpecialOrder = num1;
                List<DisplayItemModel> childrenModels = model.GetChildrenModels(true);
                // ISSUE: explicit non-virtual call
                if (childrenModels != null && __nonvirtual (childrenModels.Count) > 0)
                {
                  foreach (DisplayItemModel displayItemModel3 in childrenModels)
                  {
                    DisplayItemModel child = displayItemModel3;
                    long? nullable7;
                    if (source == null)
                    {
                      nullable4 = new long?();
                      nullable7 = nullable4;
                    }
                    else
                    {
                      SyncSortOrderModel syncSortOrderModel = source.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == child.EntityId));
                      if (syncSortOrderModel == null)
                      {
                        nullable4 = new long?();
                        nullable7 = nullable4;
                      }
                      else
                        nullable7 = new long?(syncSortOrderModel.SortOrder);
                    }
                    long? nullable8 = nullable7;
                    DisplayItemModel displayItemModel4 = child;
                    nullable4 = nullable8;
                    long num2 = nullable4 ?? extra.DefaultOrder;
                    displayItemModel4.SpecialOrder = num2;
                  }
                }
              }
            }
            else
            {
              string projectId = model.ProjectId;
              CustomizedSection customizedSection1 = new CustomizedSection();
              customizedSection1.Name = Utils.GetString(projectId == Utils.GetInboxId() || extra.InKanban ? "NotPinned" : "NotSectioned");
              customizedSection1.SectionId = "NotPinned";
              customizedSection1.SectionEntityId = "none";
              customizedSection1.Ordinal = 0L;
              customizedSection1.ProjectId = projectId;
              customizedSection1.Customized = false;
              CustomizedSection customizedSection2 = customizedSection1;
              sections.Add(customizedSection2);
              customizedSection2.Children.Add(model);
            }
          }
          else
          {
            CustomizedSection customizedSection = sections.FirstOrDefault<CustomizedSection>((Func<CustomizedSection, bool>) (s => s.SectionId == model.ColumnId)) ?? sections[0];
            if (model.Status == 0 && model.ColumnId != customizedSection.SectionId && model.ProjectId == customizedSection.ProjectId)
              TaskService.SaveTaskColumnId(model.Id, customizedSection.SectionId, true);
            customizedSection.Children.Add(model);
            if (!string.IsNullOrEmpty(extra.SortKey))
            {
              List<SyncSortOrderModel> source = customizedSection.SectionId == null || !orderDict.ContainsKey(customizedSection.SectionId) ? (List<SyncSortOrderModel>) null : orderDict[customizedSection.SectionId];
              long? nullable9;
              if (source == null)
              {
                nullable4 = new long?();
                nullable9 = nullable4;
              }
              else
              {
                SyncSortOrderModel syncSortOrderModel = source.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == model.EntityId));
                if (syncSortOrderModel == null)
                {
                  nullable4 = new long?();
                  nullable9 = nullable4;
                }
                else
                  nullable9 = new long?(syncSortOrderModel.SortOrder);
              }
              long? nullable10 = nullable9;
              DisplayItemModel displayItemModel5 = model;
              nullable4 = nullable10;
              long num3 = nullable4 ?? extra.DefaultOrder;
              displayItemModel5.SpecialOrder = num3;
              List<DisplayItemModel> childrenModels = model.GetChildrenModels(true);
              // ISSUE: explicit non-virtual call
              if (childrenModels != null && __nonvirtual (childrenModels.Count) > 0)
              {
                foreach (DisplayItemModel displayItemModel6 in childrenModels)
                {
                  DisplayItemModel child = displayItemModel6;
                  long? nullable11;
                  if (source == null)
                  {
                    nullable4 = new long?();
                    nullable11 = nullable4;
                  }
                  else
                  {
                    SyncSortOrderModel syncSortOrderModel = source.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.EntityId == child.EntityId));
                    if (syncSortOrderModel == null)
                    {
                      nullable4 = new long?();
                      nullable11 = nullable4;
                    }
                    else
                      nullable11 = new long?(syncSortOrderModel.SortOrder);
                  }
                  long? nullable12 = nullable11;
                  DisplayItemModel displayItemModel7 = child;
                  nullable4 = nullable12;
                  long num4 = nullable4 ?? extra.DefaultOrder;
                  displayItemModel7.SpecialOrder = num4;
                }
              }
            }
          }
        }
        using (List<CustomizedSection>.Enumerator enumerator = sections.GetEnumerator())
        {
          while (enumerator.MoveNext())
            SortHelper.AssembleSection((Section) enumerator.Current, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra);
          sortedTreeModels = (List<DisplayItemModel>) null;
          sections = (List<CustomizedSection>) null;
          orderDict = (Dictionary<string, List<SyncSortOrderModel>>) null;
        }
      }
      else
      {
        CustomizedSection customizedSection3;
        if (sections.Count != 1)
        {
          CustomizedSection customizedSection4 = new CustomizedSection();
          customizedSection4.SectionEntityId = "none";
          customizedSection4.SectionId = "none";
          customizedSection4.Customized = false;
          customizedSection3 = customizedSection4;
        }
        else
          customizedSection3 = sections[0];
        CustomizedSection customizedSection5 = customizedSection3;
        sortedTreeModels = sortedTreeModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => model.Level == 0)).ToList<DisplayItemModel>();
        foreach (DisplayItemModel displayItemModel in sortedTreeModels)
        {
          displayItemModel.Section = (Section) customizedSection5;
          customizedSection5.Children.Add(displayItemModel);
        }
        SortHelper.AssembleSection((Section) customizedSection5, comparer, (IEnumerable<SectionStatusModel>) sectionStatusModels, results, openedTaskIds, extra: extra, showSection: false);
        sortedTreeModels = (List<DisplayItemModel>) null;
        sections = (List<CustomizedSection>) null;
        orderDict = (Dictionary<string, List<SyncSortOrderModel>>) null;
      }
    }

    private static IComparer<DisplayItemModel> GetCompareByOrderBy(
      string orderBy,
      string projectId,
      bool isNote = false)
    {
      if (orderBy != null)
      {
        switch (orderBy.Length)
        {
          case 3:
            if (orderBy == "tag")
            {
              List<TagModel> tags = TagDataHelper.GetTags();
              return (IComparer<DisplayItemModel>) new SortHelper.TagComparer(tags != null ? tags.Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>() : (List<string>) null, isNote);
            }
            break;
          case 5:
            if (orderBy == "title")
              return (IComparer<DisplayItemModel>) new SortHelper.TitleComparer(isNote);
            break;
          case 7:
            switch (orderBy[0])
            {
              case 'd':
                if (orderBy == "dueDate")
                  return (IComparer<DisplayItemModel>) new SortHelper.DateComparer(isNote);
                break;
              case 'p':
                if (orderBy == "project")
                  return (IComparer<DisplayItemModel>) new SortHelper.ProjectDateComparer(isNote);
                break;
            }
            break;
          case 8:
            switch (orderBy[0])
            {
              case 'a':
                if (orderBy == "assignee")
                  return (IComparer<DisplayItemModel>) new SortHelper.AssigneeComparer(projectId, isNote);
                break;
              case 'p':
                if (orderBy == "priority")
                  return (IComparer<DisplayItemModel>) new SortHelper.PriorityDateComparer(isNote);
                break;
            }
            break;
          case 9:
            if (orderBy == "sortOrder")
              return (IComparer<DisplayItemModel>) new SortHelper.SortOrderComparer();
            break;
          case 11:
            if (orderBy == "createdTime")
              return (IComparer<DisplayItemModel>) new SortHelper.CreatedTimeComparer();
            break;
          case 12:
            if (orderBy == "modifiedTime")
              return (IComparer<DisplayItemModel>) new SortHelper.ModifiedTimeComparer();
            break;
        }
      }
      return (IComparer<DisplayItemModel>) new SortHelper.DateComparer(isNote);
    }

    private static void SortAsNone(
      IReadOnlyCollection<DisplayItemModel> models,
      ObservableCollection<DisplayItemModel> results,
      string orderBy)
    {
      if (!(orderBy == "none"))
        return;
      if (results == null)
        throw new ArgumentNullException(nameof (results));
      if (models == null || models.Count <= 0)
        return;
      foreach (DisplayItemModel model in (IEnumerable<DisplayItemModel>) models)
        results.Add(model);
    }

    private static void AppendCompletedSection(
      IReadOnlyList<DisplayItemModel> completed,
      IEnumerable<SectionStatusModel> sectionStatusModels,
      ICollection<DisplayItemModel> results,
      bool showLoadMore,
      bool showCompletedSection = true,
      bool loadAll = false,
      ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader closedLoader = null,
      bool forceShowLoadMore = false,
      HashSet<string> openedTaskIds = null)
    {
      int num1 = completed.Count<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.IsAbandoned));
      CompletedSection completedSection = new CompletedSection();
      if (num1 == completed.Count)
        completedSection.SetAbandonedOnly();
      else if (num1 <= 0)
        completedSection.SetCompletedOnly();
      completedSection.Count = completed.Count;
      DisplayItemModel displayItemModel = DisplayItemModel.BuildSection((Section) completedSection);
      if (completed.Count <= 0)
        return;
      displayItemModel.IsOpen = !SortHelper.IsSectionClosed((Section) completedSection, sectionStatusModels);
      displayItemModel.Num = completed.Count;
      displayItemModel.Section = (Section) completedSection;
      if (showCompletedSection)
        results.Add(displayItemModel);
      completedSection.Children.Clear();
      int num2 = completed.Count;
      if (!loadAll && closedLoader != null && closedLoader.GetCompletedLimit() < num2)
        num2 = closedLoader.GetCompletedLimit();
      List<DisplayItemModel> modelsFromNodes1 = TaskNodeUtils.GetModelsFromNodes<DisplayItemModel>(TaskNodeUtils.GetSortedTaskNodeTree((IEnumerable<DisplayItemModel>) completed));
      List<DisplayItemModel> models = new List<DisplayItemModel>();
      for (int index = 0; index < Math.Min(num2, modelsFromNodes1.Count); ++index)
        models.Add(modelsFromNodes1[index]);
      List<DisplayItemModel> modelsFromNodes2 = TaskNodeUtils.GetModelsFromNodes<DisplayItemModel>(TaskNodeUtils.GetSortedTaskNodeTree((IEnumerable<DisplayItemModel>) models));
      completedSection.Children = modelsFromNodes2.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (child => child.Level == 0)).ToList<DisplayItemModel>();
      foreach (DisplayItemModel child1 in completedSection.Children)
      {
        child1.Section = (Section) completedSection;
        child1.Parent = displayItemModel;
        if (openedTaskIds != null)
          child1.IsOpen = openedTaskIds.Contains(child1.Id);
        if (displayItemModel.IsOpen)
        {
          results.Add(child1);
          if (child1.IsOpen)
            child1.GetChildrenModels(false, openedTaskIds).ForEach((Action<DisplayItemModel>) (child =>
            {
              child.Section = (Section) completedSection;
              if (openedTaskIds != null)
                child.IsOpen = openedTaskIds.Contains(child.Id);
              results.Add(child);
            }));
        }
      }
      if (completed.Count > 0 & forceShowLoadMore && displayItemModel.IsOpen)
      {
        SortHelper.AddLoadMore(results, completedSection);
      }
      else
      {
        if (closedLoader != null && closedLoader.CheckDrainOff(num2) || ((!showLoadMore ? 1 : (completed.Count < 5 ? 1 : 0)) | (loadAll ? 1 : 0)) != 0 || !displayItemModel.IsOpen)
          return;
        SortHelper.AddLoadMore(results, completedSection);
      }
    }

    private static void AddLoadMore(
      ICollection<DisplayItemModel> results,
      CompletedSection completedSection)
    {
      DisplayItemModel displayItemModel = DisplayItemModel.BuildLoadMore();
      displayItemModel.Section = (Section) completedSection;
      results.Add(displayItemModel);
    }

    private static int CompareTaskDisplayType(DisplayType left, DisplayType right)
    {
      return (left == DisplayType.Task || left == DisplayType.Note) && (right == DisplayType.Task || right == DisplayType.Note) || left == right ? 0 : ((int) left).CompareTo((int) right);
    }

    private static int CompareModelOutDate(DisplayItemModel left, DisplayItemModel right)
    {
      if (left == null || right == null)
        return 0;
      int num1 = SortHelper.CompareOutDate(left, right);
      if (num1 != 0)
        return num1;
      int num2 = SortHelper.ComparePriority(left.Priority, right.Priority);
      if (num2 != 0)
        return num2;
      int num3 = SortHelper.CompareProjectOrder(left, right);
      return num3 != 0 ? num3 : SortHelper.CompareCustomSortOrder(left, right);
    }

    private static int CompareModelDate(DisplayItemModel left, DisplayItemModel right)
    {
      if (left == null || right == null)
        return 0;
      bool flag1 = left.OutDate();
      bool flag2 = left.OutDate();
      int num1 = !(flag1 & flag2) ? (flag1 ? -1 : (flag2 ? 1 : SortHelper.CompareDate(left, right))) : SortHelper.CompareOutDate(left, right);
      if (num1 != 0)
        return num1;
      int num2 = SortHelper.ComparePriority(left.Priority, right.Priority);
      if (num2 != 0)
        return num2;
      int num3 = SortHelper.CompareProjectOrder(left, right);
      return num3 != 0 ? num3 : SortHelper.CompareCustomSortOrder(left, right);
    }

    private static int CompareCustomSortOrder(DisplayItemModel left, DisplayItemModel right)
    {
      if (left.IsItem && right.IsItem && left.ParentOrder != right.ParentOrder)
        return left.ParentOrder.CompareTo(right.ParentOrder);
      long sortOrder1 = left.SortOrder;
      long sortOrder2 = right.SortOrder;
      if (sortOrder1 < sortOrder2)
        return -1;
      return sortOrder1 > sortOrder2 ? 1 : 0;
    }

    private static int CompareProjectOrder(DisplayItemModel left, DisplayItemModel right)
    {
      long sortProjectOrder1 = SortHelper.GetSortProjectOrder(left);
      long sortProjectOrder2 = SortHelper.GetSortProjectOrder(right);
      if (sortProjectOrder1 < sortProjectOrder2)
        return -1;
      return sortProjectOrder1 > sortProjectOrder2 ? 1 : 0;
    }

    private static long GetSortProjectOrder(DisplayItemModel model)
    {
      return model.ProjectId == LocalSettings.Settings.InServerBoxId ? long.MinValue : model.ProjectOrder;
    }

    private static int ComparePriority(int left, int right)
    {
      if (left > right)
        return -1;
      return left < right ? 1 : 0;
    }

    private static int CompareOutDate(DisplayItemModel left, DisplayItemModel right)
    {
      if (!left.StartDate.HasValue || !right.StartDate.HasValue)
        return 0;
      DateTime dateTime1 = left.StartDate.Value;
      DateTime dateTime2 = right.StartDate.Value;
      bool flag1 = SortHelper.CheckAllDay(left);
      bool flag2 = SortHelper.CheckAllDay(right);
      if (left.DueDate.HasValue)
      {
        dateTime1 = left.DueDate.Value;
        if (flag1)
          dateTime1 = dateTime1.Date.AddDays(-1.0);
      }
      if (right.DueDate.HasValue)
      {
        dateTime2 = right.DueDate.Value;
        if (flag2)
          dateTime2 = dateTime2.Date.AddDays(-1.0);
      }
      if (dateTime1.Date < dateTime2.Date)
        return -1;
      if (dateTime1.Date > dateTime2.Date || flag1 && !flag2)
        return 1;
      if (!flag1 & flag2 || dateTime1 < dateTime2)
        return -1;
      if (dateTime1 > dateTime2)
        return 1;
      DateTime dateTime3 = left.StartDate.Value;
      DateTime dateTime4 = right.StartDate.Value;
      if (dateTime3 < dateTime4)
        return -1;
      return dateTime3 > dateTime4 ? 1 : SortHelper.CompareTimeOrDuration(left, right);
    }

    private static int CompareDate(DisplayItemModel left, DisplayItemModel right)
    {
      if (left.StartDate.HasValue && right.StartDate.HasValue)
      {
        DateTime date1 = left.StartDate.Value.Date;
        DateTime date2 = right.StartDate.Value.Date;
        if (date1 < date2)
          return -1;
        if (date1 > date2)
          return 1;
        bool flag1 = SortHelper.CheckAllDay(left);
        bool flag2 = SortHelper.CheckAllDay(right);
        if (flag1 && !flag2)
          return 1;
        if (!flag1 & flag2)
          return -1;
        int num1 = left.StartDate.Value.CompareTo(right.StartDate.Value);
        if (num1 != 0)
          return num1;
        DateTime dateTime1 = left.StartDate.Value.Date;
        DateTime dateTime2 = right.StartDate.Value.Date;
        if (left.DueDate.HasValue)
        {
          dateTime1 = left.DueDate.Value.Date;
          if (flag1)
            dateTime1 = dateTime1.AddDays(-1.0);
        }
        if (right.DueDate.HasValue)
        {
          dateTime2 = right.DueDate.Value.Date;
          if (flag2)
            dateTime2 = dateTime2.AddDays(-1.0);
        }
        if (dateTime1 < dateTime2)
          return -1;
        if (dateTime1 > dateTime2)
          return 1;
        int num2 = SortHelper.CompareTimeOrDuration(left, right);
        if (num2 != 0)
          return num2;
        int num3 = SortHelper.ComparePriority(left.Priority, right.Priority);
        if (num3 != 0)
          return num3;
        int num4 = SortHelper.CompareCustomSortOrder(left, right);
        if (num4 != 0)
          return num4;
        int num5 = SortHelper.CompareTaskDisplayType(left.Type, right.Type);
        if (num5 != 0)
          return num5;
      }
      return 0;
    }

    private static bool CheckAllDay(DisplayItemModel model)
    {
      return model.IsAllDay.HasValue && model.IsAllDay.Value;
    }

    private static int CompareTimeOrDuration(DisplayItemModel left, DisplayItemModel right)
    {
      if (left.StartDate.HasValue && right.StartDate.HasValue)
      {
        DateTime date1 = left.StartDate.Value;
        DateTime date2 = right.StartDate.Value;
        if (left.IsAllDay.HasValue && left.IsAllDay.Value)
          date1 = left.StartDate.Value.Date;
        if (right.IsAllDay.HasValue && right.IsAllDay.Value)
          date2 = right.StartDate.Value.Date;
        if (date1 < date2)
          return -1;
        if (date1 > date2)
          return 1;
        int num1 = 0;
        int num2 = 0;
        DateTime? nullable;
        if (left.DueDate.HasValue)
        {
          DateTime dateTime1 = left.DueDate.Value;
          nullable = left.StartDate;
          DateTime dateTime2 = nullable.Value;
          num1 = (int) (dateTime1 - dateTime2).TotalMinutes;
        }
        nullable = right.DueDate;
        if (nullable.HasValue)
        {
          nullable = right.DueDate;
          DateTime dateTime3 = nullable.Value;
          nullable = right.StartDate;
          DateTime dateTime4 = nullable.Value;
          num2 = (int) (dateTime3 - dateTime4).TotalMinutes;
        }
        if (num1 < num2)
          return -1;
        if (num1 > num2)
          return 1;
      }
      return 0;
    }

    public static async Task<bool> ShowResetDate(string projectId)
    {
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
      if (projectModel != null && projectModel.sortType == Constants.SortType.dueDate.ToString())
      {
        List<TaskSortOrderInDateModel> inDateByProjectId = await TaskSortOrderInDateDao.GetSortOrderInDateByProjectId(projectId);
        if (inDateByProjectId != null && inDateByProjectId.Any<TaskSortOrderInDateModel>())
          return true;
      }
      return false;
    }

    public static async Task<bool> ShowResetPriority(string projectId)
    {
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
      if (projectModel != null && projectModel.sortType == Constants.SortType.priority.ToString())
      {
        List<TaskSortOrderInPriorityModel> sortOrders = await TaskSortOrderInPriorityDao.GetSortOrders(projectId);
        if (sortOrders != null && sortOrders.Any<TaskSortOrderInPriorityModel>())
          return true;
      }
      return false;
    }

    public static async Task<bool> ShowResetTag(string projectId)
    {
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
      return projectModel != null && projectModel.sortType == Constants.SortType.tag.ToString() && await SyncSortOrderDao.CountTagSortOrderByGroupId(projectId) > 0;
    }

    public class TagCompare : IComparer<DisplayItemModel>
    {
      private readonly List<SyncSortOrderModel> _sortOrders;

      public TagCompare(List<SyncSortOrderModel> sortOrders)
      {
        this._sortOrders = sortOrders.OrderBy<SyncSortOrderModel, long>((Func<SyncSortOrderModel, long>) (x => x.SortOrder)).ToList<SyncSortOrderModel>();
      }

      public int Compare(DisplayItemModel x, DisplayItemModel y)
      {
        return x == null || y == null ? 0 : this._sortOrders.FindIndex((Predicate<SyncSortOrderModel>) (o => o.EntityId == x.Id)).CompareTo(this._sortOrders.FindIndex((Predicate<SyncSortOrderModel>) (o => o.EntityId == y.Id)));
      }
    }

    private class TitleComparer : SortHelper.PriorityDateComparer
    {
      public TitleComparer(bool inNote)
        : base(inNote)
      {
        this.InNoteSection = inNote;
        this.CheckSpecial = false;
      }

      public override int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        if (left != null && right != null)
        {
          List<TitleChunk> titleNum1 = left.GetTitleNum();
          List<TitleChunk> titleNum2 = right.GetTitleNum();
          // ISSUE: explicit non-virtual call
          // ISSUE: explicit non-virtual call
          if (titleNum1 != null && __nonvirtual (titleNum1.Count) > 0 && titleNum2 != null && __nonvirtual (titleNum2.Count) > 0)
          {
            int num = this.CompareChunks(titleNum1, titleNum2);
            if (num != 0)
              return num;
          }
          else
          {
            int num = string.Compare(left.Title, right.Title, StringComparison.CurrentCulture);
            if (num != 0)
              return num;
          }
        }
        return this.InNoteSection ? SortHelper.CreatedTimeComparer.CompareCreatedTime(left, right) : base.Compare(left, right);
      }

      private int CompareChunks(List<TitleChunk> leftChunks, List<TitleChunk> rightChunks)
      {
        int num1 = Math.Min(leftChunks.Count, rightChunks.Count);
        for (int index = 0; index < num1; ++index)
        {
          TitleChunk leftChunk = leftChunks[index];
          TitleChunk rightChunk = rightChunks[index];
          if (!leftChunk.IsNum || !rightChunk.IsNum)
          {
            int num2 = string.Compare(leftChunk.Text, rightChunk.Text, StringComparison.CurrentCulture);
            if (num2 != 0)
              return num2;
          }
          else
          {
            int num3 = leftChunk.Num.CompareTo(rightChunk.Num);
            if (num3 != 0)
              return num3;
            int num4 = rightChunk.RealLength.CompareTo(leftChunk.RealLength);
            if (leftChunk.RealLength != rightChunk.RealLength)
              return num4;
          }
        }
        return 0;
      }
    }

    private class SortByTagComparaer : IComparer<DisplayItemModel>
    {
      private List<string> _sortTags;
      private Dictionary<string, TagModel> _modelTagDict;

      public SortByTagComparaer(List<string> sortTags, Dictionary<string, TagModel> modelTagDict)
      {
        this._sortTags = sortTags;
        this._modelTagDict = modelTagDict;
      }

      public int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        if (left == null || right == null)
          return 0;
        if (this._modelTagDict.ContainsKey(left.Id) && this._modelTagDict.ContainsKey(right.Id))
        {
          TagModel tagModel1 = this._modelTagDict[left.Id];
          long num1 = tagModel1 != null ? tagModel1.sortOrder : long.MaxValue;
          TagModel tagModel2 = this._modelTagDict[right.Id];
          long num2 = tagModel2 != null ? tagModel2.sortOrder : long.MaxValue;
          if (num1 != num2)
            return num1.CompareTo(num2);
        }
        if (left.Tags != null && right.Tags != null)
        {
          foreach (string sortTag in this._sortTags)
          {
            if (((IEnumerable<string>) left.Tags).Contains<string>(sortTag) && !((IEnumerable<string>) right.Tags).Contains<string>(sortTag))
              return -1;
            if (!((IEnumerable<string>) left.Tags).Contains<string>(sortTag) && ((IEnumerable<string>) right.Tags).Contains<string>(sortTag))
              return 1;
          }
        }
        return 0;
      }
    }

    private class PinnedTaskComparer : IComparer<DisplayItemModel>
    {
      private List<SyncSortOrderModel> _sortOrder;

      public PinnedTaskComparer(List<SyncSortOrderModel> sortOrder) => this._sortOrder = sortOrder;

      public int Compare(DisplayItemModel x, DisplayItemModel y)
      {
        if (x == null || y == null || this._sortOrder == null || !this._sortOrder.Any<SyncSortOrderModel>())
          return 0;
        SyncSortOrderModel syncSortOrderModel1 = this._sortOrder.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (s => s.EntityId == x.Id));
        long sortOrder1 = syncSortOrderModel1 != null ? syncSortOrderModel1.SortOrder : 0L;
        SyncSortOrderModel syncSortOrderModel2 = this._sortOrder.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (s => s.EntityId == y.Id));
        long sortOrder2 = syncSortOrderModel2 != null ? syncSortOrderModel2.SortOrder : 0L;
        return sortOrder1.CompareTo(sortOrder2);
      }
    }

    private class TagComparer : SortHelper.DateComparer
    {
      private List<string> _sortTags;

      public TagComparer(List<string> sortTags, bool inNote)
        : base(inNote)
      {
        this._sortTags = sortTags;
      }

      public override int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        if (this.CheckSpecial && left != null && right != null)
        {
          int num = left.SpecialOrder.CompareTo(right.SpecialOrder);
          if (num != 0)
            return num;
        }
        int? length;
        if (left != null)
        {
          length = left.Tags?.Length;
          int num = 0;
          if (length.GetValueOrDefault() > num & length.HasValue && (right?.Tags == null || right.Tags.Length == 0))
            return -1;
        }
        if (right != null)
        {
          length = right.Tags?.Length;
          int num = 0;
          if (length.GetValueOrDefault() > num & length.HasValue && (left?.Tags == null || left.Tags.Length == 0))
            return 1;
        }
        if (left?.Tags != null && right?.Tags != null)
        {
          foreach (string sortTag in this._sortTags)
          {
            if (((IEnumerable<string>) left.Tags).Contains<string>(sortTag) && !((IEnumerable<string>) right.Tags).Contains<string>(sortTag))
              return -1;
            if (!((IEnumerable<string>) left.Tags).Contains<string>(sortTag) && ((IEnumerable<string>) right.Tags).Contains<string>(sortTag))
              return 1;
          }
        }
        if (this.InNoteSection)
          return SortHelper.CreatedTimeComparer.CompareCreatedTime(left, right);
        int num1 = base.Compare(left, right);
        return num1 != 0 ? num1 : 0;
      }
    }

    private class OutDateComparaer : IComparer<DisplayItemModel>
    {
      public int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        if (left == null || right == null)
          return 0;
        DateTime? startDate = left.StartDate;
        if (!startDate.HasValue)
        {
          startDate = right.StartDate;
          if (startDate.HasValue)
            return 1;
        }
        startDate = left.StartDate;
        if (startDate.HasValue)
        {
          startDate = right.StartDate;
          if (!startDate.HasValue)
            return -1;
        }
        startDate = left.StartDate;
        if (startDate.HasValue)
        {
          startDate = right.StartDate;
          if (startDate.HasValue)
          {
            int num = SortHelper.CompareModelOutDate(left, right);
            if (num != 0)
              return num;
          }
        }
        return string.Compare(left.Title, right.Title, StringComparison.Ordinal);
      }
    }

    public class ProjectDateComparer : SortHelper.DateComparer
    {
      public ProjectDateComparer(bool inNote)
        : base(inNote)
      {
      }

      public override int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        if (left != null && right != null)
        {
          if (this.CheckSpecial && left.SpecialOrder != right.SpecialOrder)
            return left.SpecialOrder > right.SpecialOrder ? 1 : -1;
          int num = SortHelper.CompareDate(left, right);
          if (num != 0)
            return num;
        }
        return base.Compare(left, right);
      }
    }

    private class PriorityDateComparer : SortHelper.DateComparer
    {
      public PriorityDateComparer(bool inNote)
        : base(inNote)
      {
      }

      public override int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        if (left != null && right != null)
        {
          if (this.CheckSpecial && left.SpecialOrder != right.SpecialOrder)
            return left.SpecialOrder > right.SpecialOrder ? 1 : -1;
          int num1 = right.Priority.CompareTo(left.Priority);
          if (num1 != 0)
            return num1;
          if (this.InNoteSection)
            return SortHelper.CreatedTimeComparer.CompareCreatedTime(left, right);
          int num2 = SortHelper.CompareDate(left, right);
          if (num2 != 0)
            return num2;
        }
        return base.Compare(left, right);
      }
    }

    public class SortOrderComparer : IComparer<DisplayItemModel>
    {
      public virtual int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        return SortHelper.CompareCustomSortOrder(left, right);
      }
    }

    private class AssigneeComparer : SortHelper.DateComparer
    {
      private Dictionary<string, int> _dict = new Dictionary<string, int>();

      public AssigneeComparer(string projectId, bool inNote)
        : base(inNote)
      {
        List<ShareUserModel> projectUsers = AvatarHelper.GetProjectUsers(projectId);
        if (projectUsers == null)
          return;
        for (int index = 0; index < projectUsers.Count; ++index)
          this._dict[projectUsers[index].userId.ToString()] = index;
      }

      public override int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        if (left != null && right != null)
        {
          int num1 = string.IsNullOrEmpty(left.Assignee) || !this._dict.ContainsKey(left.Assignee) ? 100000 : this._dict[left.Assignee];
          int num2 = string.IsNullOrEmpty(right.Assignee) || !this._dict.ContainsKey(right.Assignee) ? 100000 : this._dict[right.Assignee];
          if (num1 != num2)
            return num1.CompareTo(num2);
        }
        return this.InNoteSection ? SortHelper.CreatedTimeComparer.CompareCreatedTime(left, right) : base.Compare(left, right);
      }
    }

    public class DateComparer : IComparer<DisplayItemModel>
    {
      protected bool InNoteSection;
      protected bool CheckSpecial;

      public DateComparer(bool inNote, bool checkSpecial = true)
      {
        this.InNoteSection = inNote;
        this.CheckSpecial = checkSpecial;
      }

      public virtual int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        if (left == null || right == null)
          return 0;
        if (this.CheckSpecial)
        {
          int num = left.SpecialOrder.CompareTo(right.SpecialOrder);
          if (num != 0)
            return num;
        }
        DateTime? startDate = left.StartDate;
        if (!startDate.HasValue)
        {
          startDate = right.StartDate;
          if (startDate.HasValue)
            return 1;
        }
        startDate = left.StartDate;
        if (startDate.HasValue)
        {
          startDate = right.StartDate;
          if (!startDate.HasValue)
            return -1;
        }
        startDate = left.StartDate;
        if (startDate.HasValue)
        {
          startDate = right.StartDate;
          if (startDate.HasValue)
          {
            int num = SortHelper.CompareModelDate(left, right);
            if (num != 0)
              return num;
          }
        }
        if (this.InNoteSection)
          return SortHelper.CreatedTimeComparer.CompareCreatedTime(left, right);
        if (left.Priority != right.Priority)
          return right.Priority.CompareTo(left.Priority);
        return left.ProjectId != right.ProjectId ? left.ProjectOrder.CompareTo(right.ProjectOrder) : SortHelper.CompareCustomSortOrder(left, right);
      }

      public void SetCheckSpecial(bool checkSpecial) => this.CheckSpecial = checkSpecial;
    }

    private class SpecialOrderComparer : IComparer<DisplayItemModel>
    {
      public int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        return left != null && right != null ? left.SpecialOrder.CompareTo(right.SpecialOrder) : 0;
      }
    }

    public class CreatedTimeComparer : IComparer<DisplayItemModel>
    {
      public int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        return SortHelper.CreatedTimeComparer.CompareCreatedTime(left, right);
      }

      public static int CompareCreatedTime(DisplayItemModel left, DisplayItemModel right)
      {
        if (left == null || right == null)
          return 0;
        DateTime createdTime = right.CreatedTime;
        if (createdTime.CompareTo(left.CreatedTime) == 0)
          return left.SortOrder.CompareTo(right.SortOrder);
        createdTime = right.CreatedTime;
        return createdTime.CompareTo(left.CreatedTime);
      }
    }

    private class ModifiedTimeComparer : IComparer<DisplayItemModel>
    {
      public int Compare(DisplayItemModel left, DisplayItemModel right)
      {
        if (left == null || right == null)
          return 0;
        DateTime modifiedTime = right.ModifiedTime;
        if (modifiedTime.CompareTo(left.ModifiedTime) == 0)
          return left.SortOrder.CompareTo(right.SortOrder);
        modifiedTime = right.ModifiedTime;
        return modifiedTime.CompareTo(left.ModifiedTime);
      }
    }

    public class TimelineTagComparer : IComparer<TimelineCellViewModel>
    {
      private List<string> _sortTags;

      public TimelineTagComparer()
      {
        List<TagModel> tags = TagDataHelper.GetTags();
        this._sortTags = tags != null ? tags.Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>() : (List<string>) null;
      }

      public int Compare(TimelineCellViewModel a, TimelineCellViewModel b)
      {
        if (a == null || b == null)
        {
          if (b != null)
            return 1;
          return a != null ? -1 : 0;
        }
        DateTime startDate = a.StartDate;
        DateTime date1 = startDate.Date;
        startDate = b.StartDate;
        DateTime date2 = startDate.Date;
        if (date1 != date2)
        {
          startDate = a.StartDate;
          return startDate.CompareTo(b.StartDate);
        }
        if (Math.Abs(b.Width - a.Width) > 0.01)
          return b.Width.CompareTo(a.Width);
        if (a.IsAllDay != b.IsAllDay)
          return a.IsAllDay.CompareTo(b.IsAllDay);
        startDate = a.StartDate;
        TimeSpan timeOfDay1 = startDate.TimeOfDay;
        ref TimeSpan local = ref timeOfDay1;
        startDate = b.StartDate;
        TimeSpan timeOfDay2 = startDate.TimeOfDay;
        int num = local.CompareTo(timeOfDay2);
        if (num != 0)
          return num;
        string[] tags1 = a.DisplayModel.Tags;
        if ((tags1 != null ? (tags1.Length != 0 ? 1 : 0) : 0) != 0 && (b.DisplayModel.Tags == null || b.DisplayModel.Tags.Length == 0))
          return -1;
        string[] tags2 = b.DisplayModel.Tags;
        if ((tags2 != null ? (tags2.Length != 0 ? 1 : 0) : 0) != 0 && (a.DisplayModel.Tags == null || a.DisplayModel.Tags.Length == 0))
          return 1;
        if (a.DisplayModel.Tags != null && b.DisplayModel.Tags != null)
        {
          foreach (string sortTag in this._sortTags)
          {
            if (((IEnumerable<string>) a.DisplayModel.Tags).Contains<string>(sortTag) && !((IEnumerable<string>) b.DisplayModel.Tags).Contains<string>(sortTag))
              return -1;
            if (!((IEnumerable<string>) a.DisplayModel.Tags).Contains<string>(sortTag) && ((IEnumerable<string>) b.DisplayModel.Tags).Contains<string>(sortTag))
              return 1;
          }
        }
        if (a.DisplayModel.Priority != b.DisplayModel.Priority)
          return b.DisplayModel.Priority.CompareTo(a.DisplayModel.Priority);
        if (a.DisplayModel.Type != b.DisplayModel.Type && (a.DisplayModel.IsCheckItem || b.DisplayModel.IsCheckItem) && a.DisplayModel.CreatedTime.HasValue && b.DisplayModel.CreatedTime.HasValue)
        {
          startDate = a.DisplayModel.CreatedTime.Value;
          return startDate.CompareTo(b.DisplayModel.CreatedTime.Value);
        }
        if (a.DisplayModel.ProjectOrder != b.DisplayModel.ProjectOrder)
          return a.DisplayModel.ProjectOrder.CompareTo(b.DisplayModel.ProjectOrder);
        return a.SortOrder != b.SortOrder ? a.SortOrder.CompareTo(b.SortOrder) : string.Compare(a.Id, b.Id, StringComparison.Ordinal);
      }
    }
  }
}
