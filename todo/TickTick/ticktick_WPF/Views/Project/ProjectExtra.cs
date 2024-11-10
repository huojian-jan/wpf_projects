// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.ProjectExtra
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class ProjectExtra
  {
    private const char TopDivider = '|';
    private const char InternalDivider = ',';
    private const string ProjectIdentify = "project:";
    private const string GroupIdentify = "group:";
    private const string FilterIdentify = "filter:";
    private const string TagIdentify = "tag:";
    private const string BindAccountIdentify = "bind_account:";
    private const string SubscribeCalendarIdentity = "subscribe_calendar:";
    public List<string> BindAccounts = new List<string>();
    public List<string> FilterIds = new List<string>();
    public List<string> GroupIds = new List<string>();
    public List<string> ProjectIds = new List<string>();
    public List<string> SubscribeCalendars = new List<string>();
    public List<string> Tags = new List<string>();
    public List<string> SmartIds = new List<string>();
    public bool NeedSave;

    public bool IsAll
    {
      get
      {
        return this.ProjectIds.Count == 0 && this.GroupIds.Count == 0 && this.FilterIds.Count == 0 && this.Tags.Count == 0 && this.BindAccounts.Count == 0 && this.SubscribeCalendars.Count == 0;
      }
    }

    public static ProjectExtra Deserialize(string data, bool withChild = true, bool needFix = true)
    {
      ProjectExtra result = new ProjectExtra();
      if (string.IsNullOrEmpty(data))
        return result;
      string[] strArray = data.Split('|');
      if (strArray.Length != 0)
      {
        foreach (string data1 in strArray)
        {
          if (!string.IsNullOrEmpty(data1))
          {
            if (data1.StartsWith("project:"))
            {
              result.ProjectIds = ProjectExtra.DeserializeParts(data1, "project:");
              if (needFix)
                result.ProjectIds.RemoveAll((Predicate<string>) (id => id != "Calendar5959a2259161d16d23a4f272" && id != "Habit2e4c103c57ef480997943206" && id != "ProjectAll2e4c103c57ef480997943206" && !ProjectDao.IsValidProject(id)));
            }
            if (data1.StartsWith("group:"))
            {
              result.GroupIds = ProjectExtra.DeserializeParts(data1, "group:");
              if (result.GroupIds.Any<string>())
              {
                foreach (ProjectModel projectModel in CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (project => project.IsValid() && !string.IsNullOrEmpty(project.groupId) && result.GroupIds.Contains(project.groupId))))
                {
                  if (!result.ProjectIds.Contains(projectModel.id))
                    result.ProjectIds.Add(projectModel.id);
                }
              }
            }
            if (data1.StartsWith("filter:"))
              result.FilterIds = ProjectExtra.DeserializeParts(data1, "filter:");
            if (data1.StartsWith("tag:"))
            {
              List<string> tagNames = ProjectExtra.DeserializeParts(data1, "tag:");
              if (tagNames.Contains("*withtags"))
              {
                result.Tags.Add("*withtags");
              }
              else
              {
                List<TagModel> tags = CacheManager.GetTags();
                foreach (TagModel tagModel in tags.Where<TagModel>((Func<TagModel, bool>) (tag => tagNames.Contains(tag.name))))
                {
                  TagModel tag = tagModel;
                  result.Tags.Add(tag.name);
                  if (withChild && tag.IsParent())
                    result.Tags.AddRange(tags.Where<TagModel>((Func<TagModel, bool>) (t => t.parent == tag.name && !tagNames.Contains(t.name))).Select<TagModel, string>((Func<TagModel, string>) (t => t.name)));
                }
              }
            }
            if (data1.StartsWith("bind_account:"))
              result.BindAccounts = ProjectExtra.DeserializeParts(data1, "bind_account:");
            if (data1.StartsWith("subscribe_calendar:"))
              result.SubscribeCalendars = ProjectExtra.DeserializeParts(data1, "subscribe_calendar:");
          }
        }
      }
      return result;
    }

    private static List<string> DeserializeParts(string data, string identify)
    {
      if (!data.StartsWith(identify))
        return new List<string>();
      return ((IEnumerable<string>) data.Substring(identify.Length).Split(',')).ToList<string>();
    }

    public static string Serialize(ProjectExtra data)
    {
      StringBuilder result = new StringBuilder();
      if (data.ProjectIds.Count > 0)
        ProjectExtra.SerializeItems(result, "project:", (IEnumerable<string>) data.ProjectIds);
      if (data.GroupIds.Count > 0)
        ProjectExtra.SerializeItems(result, "group:", (IEnumerable<string>) data.GroupIds);
      if (data.FilterIds.Count > 0)
        ProjectExtra.SerializeItems(result, "filter:", (IEnumerable<string>) data.FilterIds);
      if (data.Tags.Count > 0)
        ProjectExtra.SerializeItems(result, "tag:", (IEnumerable<string>) data.Tags);
      if (data.BindAccounts.Count > 0)
        ProjectExtra.SerializeItems(result, "bind_account:", (IEnumerable<string>) data.BindAccounts);
      if (data.SubscribeCalendars.Count > 0)
        ProjectExtra.SerializeItems(result, "subscribe_calendar:", (IEnumerable<string>) data.SubscribeCalendars);
      return result.ToString();
    }

    private static void SerializeItems(
      StringBuilder result,
      string identity,
      IEnumerable<string> items)
    {
      result.Append(identity);
      result.Append(string.Join(','.ToString(), items));
      result.Append('|');
    }

    public static string FormatDisplayText(string dataString, bool onlyShowPermission = false)
    {
      ProjectExtra projectExtra = ProjectExtra.Deserialize(dataString);
      if (projectExtra.IsAll)
        return Utils.GetString("All");
      List<string> stringList = new List<string>();
      List<string> filterIds = projectExtra.FilterIds;
      // ISSUE: explicit non-virtual call
      if ((filterIds != null ? (__nonvirtual (filterIds.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        string filterId = projectExtra.FilterIds[0];
        FilterModel filterModel = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == filterId));
        if (filterModel != null)
          return filterModel.name;
      }
      return ProjectExtra.FormatDisplayText(projectExtra.ProjectIds, projectExtra.GroupIds, onlyShowPermission);
    }

    public static string FixExtraOnProjectChanged(ProjectModel project, string dataString)
    {
      ProjectExtra data = ProjectExtra.Deserialize(dataString);
      if (!project.IsValid() && data.ProjectIds.Contains(project.id))
      {
        data.ProjectIds.Remove(project.id);
        dataString = ProjectExtra.Serialize(data);
      }
      return dataString;
    }

    public static string FormatDisplayText(
      List<string> pIds,
      List<string> gIds,
      bool onlyShowPermission = false)
    {
      List<string> values = new List<string>();
      if ((pIds == null || !pIds.Any<string>()) && (gIds == null || !gIds.Any<string>()))
        return Utils.GetString("All");
      if (pIds != null && pIds.Contains("ProjectAll2e4c103c57ef480997943206"))
      {
        values.Add(Utils.GetString("AllList"));
      }
      else
      {
        List<ProjectModel> source = ProjectDao.GetProjectsInIdsOrGroupIds(pIds, gIds);
        if (onlyShowPermission)
          source = source != null ? source.Where<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsProjectPermit())).ToList<ProjectModel>() : (List<ProjectModel>) null;
        // ISSUE: explicit non-virtual call
        if (source != null && __nonvirtual (source.Count) == 1)
          values.Add(source[0].name);
        // ISSUE: explicit non-virtual call
        if (source != null && __nonvirtual (source.Count) > 1)
          values.Add(string.Format(Utils.GetString("ListDisplayText"), (object) source[0].name, (object) (source.Count - 1)));
      }
      if (pIds != null && pIds.Contains("Calendar5959a2259161d16d23a4f272"))
        values.Add(Utils.GetString("Calendar"));
      if (pIds != null && pIds.Contains("Habit2e4c103c57ef480997943206"))
        values.Add(Utils.GetString("statistics_habit"));
      return string.Join(", ", (IEnumerable<string>) values);
    }
  }
}
