// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchProjectHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public static class SearchProjectHelper
  {
    private static readonly BlockingList<SearchTagAndProjectModel> ProjectModels = new BlockingList<SearchTagAndProjectModel>();

    public static void InitModels()
    {
      SearchProjectHelper.ProjectModels.Clear();
      CacheManager.GetTags().ForEach((Action<TagModel>) (tag => SearchProjectHelper.ProjectModels.Add(new SearchTagAndProjectModel(tag))));
      List<ProjectModel> projects = CacheManager.GetProjects();
      IEnumerable<ProjectModel> projectModels = projects != null ? projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsValid())) : (IEnumerable<ProjectModel>) null;
      if (projectModels != null)
      {
        foreach (ProjectModel project in projectModels)
          SearchProjectHelper.ProjectModels.Add(new SearchTagAndProjectModel(project));
      }
      List<FilterModel> filters = CacheManager.GetFilters();
      IEnumerable<FilterModel> filterModels = filters != null ? filters.Where<FilterModel>((Func<FilterModel, bool>) (f => f.IsAvailable())) : (IEnumerable<FilterModel>) null;
      if (filterModels == null)
        return;
      foreach (FilterModel filter in filterModels)
        SearchProjectHelper.ProjectModels.Add(new SearchTagAndProjectModel(filter));
    }

    public static void UpdateModels(FilterModel filter)
    {
      SearchTagAndProjectModel m = SearchProjectHelper.ProjectModels.FirstOrDefault((Func<SearchTagAndProjectModel, bool>) (model => model.IsFilter && model.Id == filter.id));
      if (m != null)
        SearchProjectHelper.ProjectModels.Remove(m);
      if (!filter.IsAvailable())
        return;
      SearchProjectHelper.ProjectModels.Add(new SearchTagAndProjectModel(filter));
    }

    public static void DeleteFilter(string id)
    {
      SearchTagAndProjectModel m = SearchProjectHelper.ProjectModels.FirstOrDefault((Func<SearchTagAndProjectModel, bool>) (model => model.IsFilter && model.Id == id));
      if (m == null)
        return;
      SearchProjectHelper.ProjectModels.Remove(m);
    }

    public static void UpdateModels(ProjectModel project)
    {
      SearchTagAndProjectModel m = SearchProjectHelper.ProjectModels.FirstOrDefault((Func<SearchTagAndProjectModel, bool>) (model => model.Id == project.id && !model.IsTag));
      if (m != null)
        SearchProjectHelper.ProjectModels.Remove(m);
      if (!project.IsEnable())
        return;
      SearchProjectHelper.ProjectModels.Add(new SearchTagAndProjectModel(project));
    }

    public static void DeleteModels(ProjectModel project)
    {
      SearchTagAndProjectModel m1 = SearchProjectHelper.ProjectModels.FirstOrDefault((Func<SearchTagAndProjectModel, bool>) (m => m.Id == project.id && !m.IsTag));
      if (m1 == null)
        return;
      SearchProjectHelper.ProjectModels.Remove(m1);
    }

    public static void DeleteModels(string tag)
    {
      SearchTagAndProjectModel m1 = SearchProjectHelper.ProjectModels.FirstOrDefault((Func<SearchTagAndProjectModel, bool>) (m => m.Name == tag && m.IsTag));
      if (m1 == null)
        return;
      SearchProjectHelper.ProjectModels.Remove(m1);
    }

    public static void UpdateModels(TagModel tag)
    {
      SearchTagAndProjectModel m = SearchProjectHelper.ProjectModels.FirstOrDefault((Func<SearchTagAndProjectModel, bool>) (model => model.Id == tag.name && model.IsTag));
      SearchProjectHelper.ProjectModels.Remove(m);
      SearchProjectHelper.ProjectModels.Add(new SearchTagAndProjectModel(tag));
    }

    private static List<SearchTagAndProjectModel> GetModels(bool onlyProject, bool withInbox = false)
    {
      List<SearchTagAndProjectModel> models = new List<SearchTagAndProjectModel>();
      foreach (SearchTagAndProjectModel tagAndProjectModel in SearchProjectHelper.ProjectModels.Value)
      {
        if (tagAndProjectModel.IsProject)
        {
          if (withInbox || !tagAndProjectModel.IsInbox)
            models.Add(tagAndProjectModel);
        }
        else if (!onlyProject)
        {
          if (tagAndProjectModel.IsTag)
          {
            switch (LocalSettings.Settings.SmartListTag)
            {
              case 0:
                models.Add(tagAndProjectModel);
                continue;
              case 2:
                int num;
                if (TaskCountCache.CountData.TryGetValue(tagAndProjectModel.Name.ToLower(), out num) && num > 0)
                {
                  models.Add(tagAndProjectModel);
                  continue;
                }
                continue;
              default:
                continue;
            }
          }
          else
            models.Add(tagAndProjectModel);
        }
      }
      return models;
    }

    public static void Clear()
    {
      SearchProjectHelper.ProjectModels?.Clear();
      SearchHistoryDao.Clear();
    }

    public static List<SearchTagAndProjectModel> GetModelsMatched(
      string text,
      bool onlyProject = false,
      bool withInbox = false)
    {
      List<SearchTagAndProjectModel> source = SearchProjectHelper.GetModels(onlyProject, withInbox);
      if (!string.IsNullOrWhiteSpace(text))
      {
        List<string> keys = ((IEnumerable<string>) text.Split(' ')).ToList<string>();
        keys.RemoveAll((Predicate<string>) (key => key == ""));
        source = source.Where<SearchTagAndProjectModel>((Func<SearchTagAndProjectModel, bool>) (m => m.Pinyin.Contains(text) || SearchHelper.KeyWordMatch(m.Emoji + m.Name.ToLower(), text.Trim(), keys, true) != 0)).ToList<SearchTagAndProjectModel>();
      }
      return source;
    }

    private static string Replace(string name, string old, string newStr)
    {
      int startIndex = name.IndexOf(old, StringComparison.Ordinal);
      if (startIndex < 0)
        return name;
      name = name.Remove(startIndex, old.Length);
      return name.Insert(startIndex, newStr);
    }

    public static void UpdateSortOrder()
    {
      SearchProjectHelper.ProjectModels.Foreach((Action<SearchTagAndProjectModel>) (m => m.UpdateGroupSortOrder()));
      lock (SearchProjectHelper.ProjectModels.Value)
        SearchProjectHelper.ProjectModels.Value.Sort((Comparison<SearchTagAndProjectModel>) ((a, b) =>
        {
          if (a.IsTag && !b.IsTag)
            return -1;
          if (b.IsTag && !a.IsTag)
            return 1;
          if (a.IsProject && b.IsFilter)
            return -1;
          if (b.IsProject && a.IsFilter)
            return 1;
          if (a.IsTag && b.IsTag || a.IsFilter && b.IsFilter)
            return a.SortOrder.CompareTo(b.SortOrder);
          if (!a.IsProject || !b.IsProject)
            return 0;
          if (a.TeamProject != b.TeamProject)
            return a.TeamProject.CompareTo(b.TeamProject);
          if (a.ParentSortOrder.HasValue && !b.ParentSortOrder.HasValue)
            return a.ParentSortOrder.Value.CompareTo(b.SortOrder);
          if (!a.ParentSortOrder.HasValue && b.ParentSortOrder.HasValue)
            return a.SortOrder.CompareTo(b.ParentSortOrder.Value);
          if (a.ParentSortOrder.HasValue && b.ParentSortOrder.HasValue)
          {
            long? parentSortOrder1 = a.ParentSortOrder;
            long? parentSortOrder2 = b.ParentSortOrder;
            if (!(parentSortOrder1.GetValueOrDefault() == parentSortOrder2.GetValueOrDefault() & parentSortOrder1.HasValue == parentSortOrder2.HasValue))
            {
              parentSortOrder2 = a.ParentSortOrder;
              long num1 = parentSortOrder2.Value;
              ref long local = ref num1;
              parentSortOrder2 = b.ParentSortOrder;
              long num2 = parentSortOrder2.Value;
              return local.CompareTo(num2);
            }
          }
          return a.SortOrder.CompareTo(b.SortOrder);
        }));
    }
  }
}
