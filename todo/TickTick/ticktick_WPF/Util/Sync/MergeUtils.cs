// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.MergeUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Tag;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public static class MergeUtils
  {
    public static void Merge(ProjectModel original, ProjectModel delta, ProjectModel revised)
    {
      revised.name = MergeUtils.GetRevisedValue<string>(original.name, delta.name, revised.name);
      revised.color = MergeUtils.GetRevisedValue<string>(original.color, delta.color, revised.color);
      revised.muted = MergeUtils.GetRevisedValue<bool>(original.muted, delta.muted, revised.muted);
      revised.inAll = MergeUtils.GetRevisedValue<bool>(original.inAll, delta.inAll, revised.inAll);
      revised.sortOrder = MergeUtils.GetRevisedValue<long>(original.sortOrder, delta.sortOrder, revised.sortOrder);
      revised.sortType = MergeUtils.GetRevisedValue<string>(original.sortType, delta.sortType, revised.sortType);
      revised.groupId = MergeUtils.GetRevisedValue<string>(original.groupId, delta.groupId, revised.groupId);
      revised.closed = MergeUtils.GetRevisedValue<bool?>(original.closed, delta.closed, revised.closed);
      revised.teamId = MergeUtils.GetRevisedValue<string>(original.teamId, delta.teamId, revised.teamId);
      revised.permission = MergeUtils.GetRevisedValue<string>(original.permission, delta.permission, revised.permission);
      revised.viewMode = MergeUtils.GetRevisedValue<string>(original.viewMode, delta.viewMode, revised.viewMode);
      revised.kind = MergeUtils.GetRevisedValue<string>(original.kind, delta.kind, revised.kind);
      bool isInit = revised.sync_status == Constants.SyncStatus.SYNC_INIT.ToString();
      MergeUtils.MergeTimeline((ITimeline) original, (ITimeline) delta, (ITimeline) revised, isInit);
      if (delta.SortOption == null)
        return;
      if (!isInit && original.SortOption != null && revised.SortOption != null)
      {
        revised.SortOption.groupBy = MergeUtils.GetRevisedValue<string>(original.SortOption.groupBy, delta.SortOption.groupBy, revised.SortOption.groupBy);
        revised.SortOption.orderBy = MergeUtils.GetRevisedValue<string>(original.SortOption.orderBy, delta.SortOption.orderBy, revised.SortOption.orderBy);
      }
      else
        revised.SortOption = delta.SortOption;
    }

    public static void Merge(
      ProjectGroupModel original,
      ProjectGroupModel delta,
      ProjectGroupModel revised)
    {
      revised.name = MergeUtils.GetRevisedValue<string>(original.name, delta.name, revised.name);
      revised.sortOrder = MergeUtils.GetRevisedValue<long?>(original.sortOrder, delta.sortOrder, revised.sortOrder);
      revised.sortType = MergeUtils.GetRevisedValue<string>(original.sortType, delta.sortType, revised.sortType);
      revised.teamId = MergeUtils.GetRevisedValue<string>(original.teamId, delta.teamId, revised.teamId);
      revised.showAll = MergeUtils.GetRevisedValue<bool>(original.showAll, delta.showAll, revised.showAll);
      revised.viewMode = MergeUtils.GetRevisedValue<string>(original.viewMode, delta.viewMode, revised.viewMode);
      bool isInit = revised.sync_status == Constants.SyncStatus.SYNC_INIT.ToString();
      MergeUtils.MergeTimeline((ITimeline) original, (ITimeline) delta, (ITimeline) revised, isInit);
      if (delta.SortOption == null)
        return;
      if (!isInit && original.SortOption != null)
      {
        revised.SortOption.groupBy = MergeUtils.GetRevisedValue<string>(original.SortOption.groupBy, delta.SortOption.groupBy, revised.SortOption.groupBy);
        revised.SortOption.orderBy = MergeUtils.GetRevisedValue<string>(original.SortOption.orderBy, delta.SortOption.orderBy, revised.SortOption.orderBy);
      }
      else
        revised.SortOption = delta.SortOption;
    }

    private static void MergeTimeline(
      ITimeline original,
      ITimeline delta,
      ITimeline revised,
      bool isInit)
    {
      if (revised.SyncTimeline != null)
      {
        revised.SyncTimeline.SortType = MergeUtils.GetRevisedValue<string>(original.SyncTimeline?.SortType, delta.SyncTimeline?.SortType, revised.SyncTimeline?.SortType);
        if (delta.SyncTimeline?.SortOption != null)
        {
          if (!isInit && original.SyncTimeline?.SortOption != null && revised.SyncTimeline.SortOption != null)
          {
            revised.SyncTimeline.SortOption.groupBy = MergeUtils.GetRevisedValue<string>(original.SyncTimeline.SortOption.groupBy, delta.SyncTimeline.SortOption.groupBy, revised.SyncTimeline.SortOption.groupBy);
            revised.SyncTimeline.SortOption.orderBy = MergeUtils.GetRevisedValue<string>(original.SyncTimeline.SortOption.orderBy, delta.SyncTimeline.SortOption.orderBy, revised.SyncTimeline.SortOption.orderBy);
          }
          else
            revised.SyncTimeline.SortOption = delta.SyncTimeline.SortOption;
        }
      }
      if (revised.Timeline == null)
        return;
      revised.Timeline.SortType = revised.SyncTimeline?.SortType ?? revised.Timeline.SortType;
      revised.Timeline.sortOption = revised.SyncTimeline?.SortOption ?? revised.Timeline.sortOption;
    }

    public static void Merge(FilterModel original, FilterModel delta, FilterModel revised)
    {
      revised.name = MergeUtils.GetRevisedValue<string>(original.name, delta.name, revised.name);
      revised.rule = MergeUtils.GetRevisedValue<string>(original.rule, delta.rule, revised.rule);
      revised.sortOrder = MergeUtils.GetRevisedValue<long>(original.sortOrder, delta.sortOrder, revised.sortOrder);
      revised.sortType = MergeUtils.GetRevisedValue<string>(original.sortType, delta.sortType, revised.sortType);
      revised.viewMode = MergeUtils.GetRevisedValue<string>(original.viewMode, delta.viewMode, revised.viewMode);
      bool isInit = revised.syncStatus == 3;
      MergeUtils.MergeTimeline((ITimeline) original, (ITimeline) delta, (ITimeline) revised, isInit);
      if (delta.SortOption == null)
        return;
      if (!isInit && original.SortOption != null)
      {
        revised.SortOption.groupBy = MergeUtils.GetRevisedValue<string>(original.SortOption.groupBy, delta.SortOption.groupBy, revised.SortOption.groupBy);
        revised.SortOption.orderBy = MergeUtils.GetRevisedValue<string>(original.SortOption.orderBy, delta.SortOption.orderBy, revised.SortOption.orderBy);
      }
      else
        revised.SortOption = delta.SortOption;
    }

    public static void Merge(TagModel original, TagModel delta, TagModel revised)
    {
      revised.sortOrder = MergeUtils.GetRevisedValue<long>(original.sortOrder, delta.sortOrder, revised.sortOrder);
      revised.sortType = MergeUtils.GetRevisedValue<string>(original.sortType, delta.sortType, revised.sortType);
      revised.color = MergeUtils.GetRevisedValue<string>(original.color, delta.color, revised.color);
      revised.label = MergeUtils.GetRevisedValue<string>(original.label, delta.label, revised.label);
      revised.parent = MergeUtils.GetRevisedValue<string>(original.parent, delta.parent, revised.parent);
      MergeUtils.MergeTimeline((ITimeline) original, (ITimeline) delta, (ITimeline) revised, false);
      if (delta.SortOption == null)
        return;
      if (revised.status != 3 && original.SortOption != null)
      {
        revised.SortOption.groupBy = MergeUtils.GetRevisedValue<string>(original.SortOption.groupBy, delta.SortOption.groupBy, revised.SortOption.groupBy);
        revised.SortOption.orderBy = MergeUtils.GetRevisedValue<string>(original.SortOption.orderBy, delta.SortOption.orderBy, revised.SortOption.orderBy);
      }
      else
        revised.SortOption = delta.SortOption;
    }

    public static void Merge(TaskModel original, TaskModel delta, TaskModel revised)
    {
      string revisedDiffText = MergeUtils.GetRevisedDiffText(original.title, delta.title, revised.title);
      MergeUtils.Log("title", (object) original.title, (object) delta.title, (object) revised.title, (object) revisedDiffText);
      revised.title = revisedDiffText;
      int revisedValue1 = MergeUtils.GetRevisedValue<int>(original.priority, delta.priority, revised.priority);
      MergeUtils.Log("priority", (object) original.priority, (object) delta.priority, (object) revised.priority, (object) revisedValue1);
      revised.priority = revisedValue1;
      int revisedValue2 = MergeUtils.GetRevisedValue<int>(original.status, delta.status, revised.status);
      MergeUtils.Log("status", (object) original.status, (object) delta.status, (object) revised.status, (object) revisedValue2);
      revised.status = revisedValue2;
      long revisedValue3 = MergeUtils.GetRevisedValue<long>(original.sortOrder, delta.sortOrder, revised.sortOrder);
      MergeUtils.Log("sortOrder", (object) original.sortOrder, (object) delta.sortOrder, (object) revised.sortOrder, (object) revisedValue3);
      revised.sortOrder = revisedValue3;
      int? revisedValue4 = MergeUtils.GetRevisedValue<int?>(original.progress, delta.progress, revised.progress);
      MergeUtils.Log("progress", (object) original.progress, (object) delta.progress, (object) revised.progress, (object) revisedValue4);
      revised.progress = revisedValue4;
      revised.startDate = MergeUtils.GetRevisedValue<DateTime?>(original.startDate, delta.startDate, revised.startDate);
      revised.dueDate = MergeUtils.GetRevisedValue<DateTime?>(original.dueDate, delta.dueDate, revised.dueDate);
      revised.isAllDay = MergeUtils.GetRevisedValue<bool?>(original.isAllDay, delta.isAllDay, revised.isAllDay);
      revised.timeZone = MergeUtils.GetRevisedValue<string>(original.timeZone, delta.timeZone, revised.timeZone);
      revised.isFloating = MergeUtils.GetRevisedValue<bool?>(original.isFloating, delta.isFloating, revised.isFloating);
      revised.parentId = MergeUtils.GetRevisedValue<string>(original.parentId, delta.parentId, revised.parentId);
      string[] tags1 = original.tags;
      List<string> original1 = (tags1 != null ? ((IEnumerable<string>) tags1).ToList<string>() : (List<string>) null) ?? new List<string>();
      string[] tags2 = delta.tags;
      List<string> delta1 = (tags2 != null ? ((IEnumerable<string>) tags2).ToList<string>() : (List<string>) null) ?? new List<string>();
      string[] tags3 = revised.tags;
      List<string> revised1 = (tags3 != null ? ((IEnumerable<string>) tags3).ToList<string>() : (List<string>) null) ?? new List<string>();
      List<string> revisedTags = MergeUtils.GetRevisedTags((IEnumerable<string>) original1, (ICollection<string>) delta1, (ICollection<string>) revised1);
      string[] tags4 = original.tags;
      string original2 = string.Join(",", (IEnumerable<string>) ((tags4 != null ? ((IEnumerable<string>) tags4).ToList<string>() : (List<string>) null) ?? new List<string>()));
      string[] tags5 = delta.tags;
      string delta2 = string.Join(",", (IEnumerable<string>) ((tags5 != null ? ((IEnumerable<string>) tags5).ToList<string>() : (List<string>) null) ?? new List<string>()));
      string[] tags6 = revised.tags;
      string revised2 = string.Join(",", (IEnumerable<string>) ((tags6 != null ? ((IEnumerable<string>) tags6).ToList<string>() : (List<string>) null) ?? new List<string>()));
      string merged = string.Join(",", (IEnumerable<string>) revisedTags);
      MergeUtils.Log("tag", (object) original2, (object) delta2, (object) revised2, (object) merged);
      revised.tags = revisedTags.ToArray();
      revised.tag = TagSerializer.ToJsonContent(revisedTags);
      revised.exDate = MergeUtils.GetRevisedValue<string[]>(original.exDate, delta.exDate, revised.exDate);
      MergeUtils.MergeChecklistItems(original, delta, revised);
      MergeUtils.MergeReminders(original, delta, revised);
      MergeUtils.MergePomoSummaries(original, delta, revised);
    }

    private static void MergePomoSummaries(TaskModel original, TaskModel delta, TaskModel revised)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      PomodoroSummaryModel[] focusSummaries1 = original.FocusSummaries;
      PomodoroSummaryModel pomodoroSummaryModel1 = focusSummaries1 != null ? ((IEnumerable<PomodoroSummaryModel>) focusSummaries1).FirstOrDefault<PomodoroSummaryModel>((Func<PomodoroSummaryModel, bool>) (f => f.userId == userId)) : (PomodoroSummaryModel) null;
      PomodoroSummaryModel[] focusSummaries2 = delta.FocusSummaries;
      PomodoroSummaryModel pomodoroSummaryModel2 = focusSummaries2 != null ? ((IEnumerable<PomodoroSummaryModel>) focusSummaries2).FirstOrDefault<PomodoroSummaryModel>((Func<PomodoroSummaryModel, bool>) (f => f.userId == userId)) : (PomodoroSummaryModel) null;
      PomodoroSummaryModel[] focusSummaries3 = revised.FocusSummaries;
      PomodoroSummaryModel pomodoroSummaryModel3 = focusSummaries3 != null ? ((IEnumerable<PomodoroSummaryModel>) focusSummaries3).FirstOrDefault<PomodoroSummaryModel>((Func<PomodoroSummaryModel, bool>) (f => f.userId == userId)) : (PomodoroSummaryModel) null;
      if (pomodoroSummaryModel3 == null)
      {
        if (pomodoroSummaryModel2 == null)
          return;
        PomodoroSummaryModel[] focusSummaries4 = revised.FocusSummaries;
        List<PomodoroSummaryModel> pomodoroSummaryModelList = (focusSummaries4 != null ? ((IEnumerable<PomodoroSummaryModel>) focusSummaries4).ToList<PomodoroSummaryModel>() : (List<PomodoroSummaryModel>) null) ?? new List<PomodoroSummaryModel>();
        pomodoroSummaryModelList.Add(pomodoroSummaryModel2);
        revised.FocusSummaries = pomodoroSummaryModelList.ToArray();
      }
      else
      {
        if (pomodoroSummaryModel2 == null || pomodoroSummaryModel1 == null)
          return;
        pomodoroSummaryModel3.count = MergeUtils.GetRevisedValue<int>(pomodoroSummaryModel1.count, pomodoroSummaryModel2.count, pomodoroSummaryModel3.count);
        pomodoroSummaryModel3.PomoDuration = MergeUtils.GetRevisedValue<long>(pomodoroSummaryModel1.PomoDuration, pomodoroSummaryModel2.PomoDuration, pomodoroSummaryModel3.PomoDuration);
        pomodoroSummaryModel3.StopwatchDuration = MergeUtils.GetRevisedValue<long>(pomodoroSummaryModel1.StopwatchDuration, pomodoroSummaryModel2.StopwatchDuration, pomodoroSummaryModel3.StopwatchDuration);
        pomodoroSummaryModel3.estimatedPomo = MergeUtils.GetRevisedValue<int>(pomodoroSummaryModel1.estimatedPomo, pomodoroSummaryModel2.estimatedPomo, pomodoroSummaryModel3.estimatedPomo);
        pomodoroSummaryModel3.EstimatedDuration = MergeUtils.GetRevisedValue<long>(pomodoroSummaryModel1.EstimatedDuration, pomodoroSummaryModel2.EstimatedDuration, pomodoroSummaryModel3.EstimatedDuration);
        pomodoroSummaryModel3.focusesString = MergeUtils.MergeFocuses(pomodoroSummaryModel1.focusesString, pomodoroSummaryModel2.focusesString, pomodoroSummaryModel3.focusesString);
      }
    }

    private static string MergeFocuses(string original, string delta, string revised)
    {
      List<object[]> focuses1 = JsonConvert.DeserializeObject<List<object[]>>(delta) ?? new List<object[]>();
      List<object[]> focuses2 = JsonConvert.DeserializeObject<List<object[]>>(revised) ?? new List<object[]>();
      Dictionary<string, object[]> dict1 = GetDict(JsonConvert.DeserializeObject<List<object[]>>(original));
      Dictionary<string, object[]> dict2 = GetDict(focuses1);
      Dictionary<string, object[]> dict3 = GetDict(focuses2);
      foreach (object[] objArray1 in focuses2)
      {
        object[] objArray2;
        if (objArray1[0] is string key && dict1.TryGetValue(key, out objArray2) && objArray1[1] == objArray2[1] && objArray1[2] == objArray2[2])
        {
          if (dict2.ContainsKey(key))
          {
            objArray1[1] = dict2[key][1];
            objArray1[2] = dict2[key][2];
          }
          else
            dict3.Remove(key);
        }
      }
      foreach (object[] objArray in focuses1)
      {
        if (objArray[0] is string key && !dict3.ContainsKey(key))
          dict3[key] = objArray;
      }
      return JsonConvert.SerializeObject((object) dict3.Values.ToList<object[]>());

      static Dictionary<string, object[]> GetDict(List<object[]> focuses)
      {
        Dictionary<string, object[]> dict = new Dictionary<string, object[]>();
        if (focuses == null)
          return dict;
        foreach (object[] focuse in focuses)
        {
          if (focuse[0] is string key1)
            dict[key1] = focuse;
        }
        return dict;
      }
    }

    private static void MergeReminders(TaskModel original, TaskModel delta, TaskModel revised)
    {
      Dictionary<string, TaskReminderModel> itemsDict1 = MergeUtils.CreateItemsDict<TaskReminderModel>(original.reminders, (Func<TaskReminderModel, string>) (item => item.id));
      Dictionary<string, TaskReminderModel> itemsDict2 = MergeUtils.CreateItemsDict<TaskReminderModel>(revised.reminders, (Func<TaskReminderModel, string>) (item => item.id));
      Dictionary<string, TaskReminderModel> dictionary = new Dictionary<string, TaskReminderModel>();
      if (delta.reminders != null && delta.reminders.Length != 0)
      {
        foreach (TaskReminderModel reminder in delta.reminders)
        {
          if (!string.IsNullOrEmpty(reminder.id) && !dictionary.ContainsKey(reminder.id))
          {
            dictionary.Add(reminder.id, reminder);
            if (itemsDict1.ContainsKey(reminder.id) && itemsDict2.ContainsKey(reminder.id))
            {
              TaskReminderModel taskReminderModel1 = itemsDict1[reminder.id];
              TaskReminderModel taskReminderModel2 = itemsDict2[reminder.id];
              taskReminderModel2.trigger = MergeUtils.GetRevisedValue<string>(taskReminderModel1.trigger, reminder.trigger, taskReminderModel2.trigger);
            }
            if (itemsDict1.ContainsKey(reminder.id) && !itemsDict2.ContainsKey(reminder.id) && itemsDict1[reminder.id].trigger != reminder.trigger)
              itemsDict2.Add(reminder.id, reminder);
            if (!itemsDict1.ContainsKey(reminder.id) && !itemsDict2.ContainsKey(reminder.id))
              itemsDict2.Add(reminder.id, reminder);
          }
        }
      }
      foreach (TaskReminderModel taskReminderModel3 in itemsDict1.Values)
      {
        if (!dictionary.ContainsKey(taskReminderModel3.id) && itemsDict2.ContainsKey(taskReminderModel3.id))
        {
          TaskReminderModel taskReminderModel4 = itemsDict2[taskReminderModel3.id];
          if (taskReminderModel3.trigger == taskReminderModel4.trigger)
            itemsDict2.Remove(taskReminderModel3.id);
        }
      }
      revised.reminders = MergeUtils.FilterDuplicateReminders((IEnumerable<TaskReminderModel>) itemsDict2.Values.ToList<TaskReminderModel>()).ToArray();
    }

    private static List<TaskReminderModel> FilterDuplicateReminders(
      IEnumerable<TaskReminderModel> reminders)
    {
      List<TaskReminderModel> source = new List<TaskReminderModel>();
      List<string> stringList = new List<string>();
      foreach (TaskReminderModel reminder in reminders)
      {
        if (!stringList.Contains(reminder.trigger))
        {
          stringList.Add(reminder.trigger);
          source.Add(reminder);
        }
      }
      return source.Count > 5 ? source.Take<TaskReminderModel>(5).ToList<TaskReminderModel>() : source;
    }

    private static void MergeChecklistItems(TaskModel original, TaskModel delta, TaskModel revised)
    {
      string revisedValue = MergeUtils.GetRevisedValue<string>(original.kind, delta.kind, revised.kind);
      MergeUtils.Log("kind", (object) original.kind, (object) delta.kind, (object) revised.kind, (object) revisedValue);
      if (delta.kind == revised.kind)
      {
        switch (revisedValue)
        {
          case "CHECKLIST":
            string revisedDiffText = MergeUtils.GetRevisedDiffText(original.desc, delta.desc, revised.desc);
            MergeUtils.Log("desc", (object) original.desc, (object) delta.desc, (object) revised.desc, (object) revisedDiffText);
            revised.desc = revisedDiffText;
            revised.items = MergeUtils.MergeChecklistItems(original.items, delta.items, revised.items);
            break;
          case "TEXT":
          case "NOTE":
            string str = MergeUtils.GetRevisedDiffText(original.content, delta.content, revised.content) ?? string.Empty;
            MergeUtils.Log("content", (object) original.content, (object) delta.content, (object) revised.content, (object) str);
            MatchCollection matchCollection = MarkDownEditor.AttachmentRegex.Matches(str);
            if (matchCollection.Count > 0 && matchCollection.Count > 0)
            {
              Dictionary<int, Match> dictionary = new Dictionary<int, Match>();
              HashSet<string> stringSet = new HashSet<string>();
              for (int i = 0; i < matchCollection.Count; ++i)
              {
                Match match = matchCollection[i];
                string[] strArray = match.Groups[2].Value.Split('/');
                if (strArray.Length == 2)
                {
                  if (stringSet.Contains(strArray[0]))
                    dictionary[match.Index] = match;
                  else
                    stringSet.Add(strArray[0]);
                }
              }
              foreach (int num in dictionary.Keys.OrderByDescending<int, int>((Func<int, int>) (i => i)).ToList<int>())
              {
                Match match;
                if (dictionary.TryGetValue(num, out match))
                  str = str.Remove(num, Math.Min(str.Length - num, match.Length));
              }
            }
            revised.content = str;
            revised.desc = string.Empty;
            revised.items = (TaskDetailItemModel[]) null;
            break;
        }
      }
      else if (revisedValue == revised.kind)
      {
        switch (revisedValue)
        {
          case "NOTE":
          case "TEXT":
            revised.desc = string.Empty;
            revised.items = (TaskDetailItemModel[]) null;
            break;
        }
      }
      else
      {
        switch (revisedValue)
        {
          case "CHECKLIST":
            revised.desc = delta.desc;
            revised.items = delta.items;
            break;
          case "NOTE":
          case "TEXT":
            revised.content = delta.content;
            revised.desc = string.Empty;
            revised.items = (TaskDetailItemModel[]) null;
            break;
        }
      }
      revised.kind = revisedValue;
    }

    private static TaskDetailItemModel[] MergeChecklistItems(
      TaskDetailItemModel[] original,
      TaskDetailItemModel[] delta,
      TaskDetailItemModel[] revised)
    {
      Dictionary<string, TaskDetailItemModel> itemsDict1 = MergeUtils.CreateItemsDict<TaskDetailItemModel>(original, (Func<TaskDetailItemModel, string>) (item => item.id));
      Dictionary<string, TaskDetailItemModel> itemsDict2 = MergeUtils.CreateItemsDict<TaskDetailItemModel>(revised, (Func<TaskDetailItemModel, string>) (item => item.id));
      Dictionary<string, TaskDetailItemModel> dictionary = new Dictionary<string, TaskDetailItemModel>();
      if (delta != null && delta.Length != 0)
      {
        foreach (TaskDetailItemModel taskDetailItemModel1 in delta)
        {
          string id = taskDetailItemModel1.id;
          if (!string.IsNullOrEmpty(id))
          {
            dictionary.Add(id, taskDetailItemModel1);
            if (itemsDict1.ContainsKey(id) && itemsDict2.ContainsKey(id))
            {
              TaskDetailItemModel taskDetailItemModel2 = itemsDict1[id];
              TaskDetailItemModel taskDetailItemModel3 = itemsDict2[id];
              string revisedDiffText = MergeUtils.GetRevisedDiffText(taskDetailItemModel2.title, taskDetailItemModel1.title, taskDetailItemModel3.title);
              MergeUtils.Log("item_title", (object) taskDetailItemModel2.title, (object) taskDetailItemModel1.title, (object) taskDetailItemModel3.title, (object) revisedDiffText);
              taskDetailItemModel3.title = revisedDiffText;
              taskDetailItemModel3.sortOrder = MergeUtils.GetRevisedValue<long>(taskDetailItemModel2.sortOrder, taskDetailItemModel1.sortOrder, taskDetailItemModel3.sortOrder);
              taskDetailItemModel3.status = MergeUtils.GetRevisedValue<int>(taskDetailItemModel2.status, taskDetailItemModel1.status, taskDetailItemModel3.status);
              taskDetailItemModel3.completedTime = MergeUtils.GetRevisedValue<DateTime?>(taskDetailItemModel2.completedTime, taskDetailItemModel1.completedTime, taskDetailItemModel3.completedTime);
              taskDetailItemModel3.snoozeReminderTime = MergeUtils.GetRevisedValue<DateTime?>(taskDetailItemModel2.snoozeReminderTime, taskDetailItemModel1.snoozeReminderTime, taskDetailItemModel3.snoozeReminderTime);
            }
            else if (itemsDict1.ContainsKey(id))
            {
              if (MergeUtils.DiffCheckItemExcludeSortOrder(itemsDict1[id], taskDetailItemModel1) || revised == null || revised.Length == 0)
                itemsDict2.Add(id, taskDetailItemModel1);
            }
            else
              itemsDict2.Add(id, taskDetailItemModel1);
          }
        }
      }
      foreach (TaskDetailItemModel taskDetailItemModel4 in itemsDict1.Values)
      {
        if (!dictionary.ContainsKey(taskDetailItemModel4.id) && itemsDict2.ContainsKey(taskDetailItemModel4.id))
        {
          TaskDetailItemModel taskDetailItemModel5 = itemsDict2[taskDetailItemModel4.id];
          if (!MergeUtils.DiffCheckItemExcludeSortOrder(taskDetailItemModel4, taskDetailItemModel5))
            itemsDict2.Remove(taskDetailItemModel5.id);
        }
      }
      return itemsDict2.Values.ToArray<TaskDetailItemModel>();
    }

    private static bool DiffCheckItemExcludeSortOrder(
      TaskDetailItemModel item1,
      TaskDetailItemModel item2)
    {
      if (item1.title != item2.title || item1.status != item2.status)
        return true;
      bool? isAllDay1 = item1.isAllDay;
      bool? isAllDay2 = item2.isAllDay;
      if (!(isAllDay1.GetValueOrDefault() == isAllDay2.GetValueOrDefault() & isAllDay1.HasValue == isAllDay2.HasValue))
        return true;
      DateTime? startDate1 = item1.startDate;
      DateTime? startDate2 = item2.startDate;
      return (startDate1.HasValue == startDate2.HasValue ? (startDate1.HasValue ? (startDate1.GetValueOrDefault() != startDate2.GetValueOrDefault() ? 1 : 0) : 0) : 1) != 0;
    }

    private static Dictionary<string, T> CreateItemsDict<T>(T[] models, Func<T, string> getId)
    {
      List<T> objList = (models != null ? ((IEnumerable<T>) models).ToList<T>() : (List<T>) null) ?? new List<T>();
      Dictionary<string, T> itemsDict = new Dictionary<string, T>();
      foreach (T obj in objList)
      {
        if (!string.IsNullOrEmpty(getId(obj)) && !itemsDict.ContainsKey(getId(obj)))
          itemsDict.Add(getId(obj), obj);
      }
      return itemsDict;
    }

    private static List<string> GetRevisedTags(
      IEnumerable<string> original,
      ICollection<string> delta,
      ICollection<string> revised)
    {
      List<string> list = revised.ToList<string>();
      list.AddRange(delta.Except<string>((IEnumerable<string>) revised));
      foreach (string str in original)
      {
        if (!delta.Contains(str) || !revised.Contains(str))
          list.Remove(str);
      }
      return list;
    }

    public static string GetRevisedDiffText(string original, string delta, string revised)
    {
      if (string.Equals(original, revised))
        return delta;
      if (string.Equals(original, delta))
        return revised;
      diff_match_patch diffMatchPatch = new diff_match_patch();
      return diffMatchPatch.patch_apply(diffMatchPatch.patch_make(original ?? string.Empty, delta ?? string.Empty), revised ?? string.Empty)[0].ToString();
    }

    private static T GetRevisedValue<T>(T original, T delta, T revised)
    {
      string empty1 = string.Empty;
      if ((object) original != null)
        empty1 = original.ToString();
      string empty2 = string.Empty;
      if ((object) delta != null)
        empty2 = delta.ToString();
      string empty3 = string.Empty;
      if ((object) revised != null)
        empty3 = revised.ToString();
      if (empty1.Equals(empty3))
        return delta;
      empty1.Equals(empty2);
      return revised;
    }

    private static void Log(
      string key,
      object original,
      object delta,
      object revised,
      object merged)
    {
      if (!App.IsAdmin)
        return;
      UtilLog.Info("\n******************** " + key + " ******************** \n" + string.Format("original (backup):{0}\n", original) + string.Format("delta (server)   :{0}\n", delta) + string.Format("revised (local)  :{0}\n", revised) + string.Format("merge (final)    :{0}\n", merged));
    }
  }
}
