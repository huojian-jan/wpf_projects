// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.JumpHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Widget;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class JumpHelper
  {
    public static void ClearJumpList()
    {
      JumpList.SetJumpList(Application.Current, new JumpList((IEnumerable<JumpItem>) new List<JumpItem>(), false, false));
      JumpHelper.RemoveChangeEvents();
    }

    public static async void InitJumpList()
    {
      await Task.Delay(400);
      ThreadUtil.DetachedRunOnUiBackThread((Action) (async () => await JumpHelper.SetJumpList()));
    }

    private static async Task SetJumpList()
    {
      string path = AppPaths.ExeDir + "\\JumpIcons.dll";
      JumpList jumpList = new JumpList();
      JumpTask jumpTask1 = new JumpTask();
      jumpTask1.Title = Utils.GetString("AddListWidget");
      jumpTask1.Arguments = "other_add_list_widget";
      jumpTask1.CustomCategory = Utils.GetString("Others");
      jumpTask1.IconResourcePath = path;
      jumpTask1.IconResourceIndex = 3;
      JumpTask addProjectWidget = jumpTask1;
      JumpTask jumpTask2 = new JumpTask();
      jumpTask2.Title = Utils.GetString("AddCalendarWidget");
      jumpTask2.Arguments = "other_add_calendar_widget";
      jumpTask2.CustomCategory = Utils.GetString("Others");
      jumpTask2.IconResourcePath = path;
      jumpTask2.IconResourceIndex = 1;
      JumpTask addCalendarWidget = jumpTask2;
      JumpTask jumpTask3 = new JumpTask();
      jumpTask3.Title = Utils.GetString("CloseCalendarWidget");
      jumpTask3.Arguments = "other_remove_calendar_widget";
      jumpTask3.CustomCategory = Utils.GetString("Others");
      jumpTask3.IconResourcePath = path;
      jumpTask3.IconResourceIndex = 1;
      JumpTask removeCalendarWidget = jumpTask3;
      if (!await AppLockCache.GetAppLocked())
      {
        jumpList.JumpItems.Add((JumpItem) addProjectWidget);
        jumpList.JumpItems.Add(CalendarWidgetHelper.CanAddProject() ? (JumpItem) addCalendarWidget : (JumpItem) removeCalendarWidget);
        bool flag = MatrixWidgetHelper.CanAddWidget();
        List<JumpItem> jumpItems = jumpList.JumpItems;
        JumpTask jumpTask4 = new JumpTask();
        jumpTask4.Title = Utils.GetString(flag ? "AddMatrixWidget" : "CloseMatrixWidget");
        jumpTask4.Arguments = flag ? "other_add_matrix_widget" : "other_remove_matrix_widget";
        jumpTask4.CustomCategory = Utils.GetString("Others");
        jumpTask4.IconResourcePath = path;
        jumpTask4.IconResourceIndex = 4;
        jumpItems.Add((JumpItem) jumpTask4);
      }
      if (LocalSettings.Settings.EnableFocus)
      {
        if (TickFocusManager.Working)
        {
          bool flag = LocalSettings.Settings.PomoType == FocusConstance.Focus;
          List<JumpItem> jumpItems = jumpList.JumpItems;
          JumpTask jumpTask5 = new JumpTask();
          jumpTask5.Title = Utils.GetString(flag ? "ShowPomo" : "ShowTiming");
          jumpTask5.Arguments = flag ? "other_show_pomo" : "other_show_timing";
          jumpTask5.CustomCategory = Utils.GetString("Others");
          jumpTask5.IconResourcePath = path;
          jumpTask5.IconResourceIndex = flag ? 5 : 6;
          jumpItems.Add((JumpItem) jumpTask5);
        }
        else
        {
          List<JumpItem> jumpItems1 = jumpList.JumpItems;
          JumpTask jumpTask6 = new JumpTask();
          jumpTask6.Title = Utils.GetString("StartPomo");
          jumpTask6.Arguments = "other_start_pomo";
          jumpTask6.CustomCategory = Utils.GetString("Others");
          jumpTask6.IconResourcePath = path;
          jumpTask6.IconResourceIndex = 5;
          jumpItems1.Add((JumpItem) jumpTask6);
          List<JumpItem> jumpItems2 = jumpList.JumpItems;
          JumpTask jumpTask7 = new JumpTask();
          jumpTask7.Title = Utils.GetString("StartTiming");
          jumpTask7.Arguments = "other_start_timing";
          jumpTask7.CustomCategory = Utils.GetString("Others");
          jumpTask7.IconResourcePath = path;
          jumpTask7.IconResourceIndex = 6;
          jumpItems2.Add((JumpItem) jumpTask7);
        }
      }
      JumpTask jumpTask8 = new JumpTask();
      jumpTask8.Title = Utils.GetString("AddaTask");
      jumpTask8.Arguments = "add_task";
      jumpTask8.CustomCategory = Utils.GetString("Task");
      jumpTask8.IconResourcePath = path;
      jumpTask8.IconResourceIndex = 0;
      JumpTask jumpTask9 = jumpTask8;
      JumpTask jumpTask10 = new JumpTask();
      jumpTask10.Title = Utils.GetString("Calendar");
      jumpTask10.Arguments = "task_show_calendar";
      jumpTask10.CustomCategory = Utils.GetString("Task");
      jumpTask10.IconResourcePath = path;
      jumpTask10.IconResourceIndex = 1;
      JumpTask jumpTask11 = jumpTask10;
      JumpTask jumpTask12 = new JumpTask();
      jumpTask12.Title = Utils.GetString("Today");
      jumpTask12.Arguments = "task_show_today";
      jumpTask12.CustomCategory = Utils.GetString("Task");
      jumpTask12.IconResourcePath = path;
      jumpTask12.IconResourceIndex = 7;
      JumpTask jumpTask13 = jumpTask12;
      JumpTask jumpTask14 = new JumpTask();
      jumpTask14.Title = Utils.GetString("Inbox");
      jumpTask14.Arguments = "task_show_inbox";
      jumpTask14.CustomCategory = Utils.GetString("Task");
      jumpTask14.IconResourcePath = path;
      jumpTask14.IconResourceIndex = 2;
      JumpTask jumpTask15 = jumpTask14;
      jumpList.JumpItems.Add((JumpItem) jumpTask9);
      jumpList.JumpItems.Add((JumpItem) jumpTask13);
      jumpList.JumpItems.Add((JumpItem) jumpTask11);
      jumpList.JumpItems.Add((JumpItem) jumpTask15);
      List<JumpTask> jumpTaskList = JumpHelper.InitCustomList();
      if (jumpTaskList.Any<JumpTask>())
        jumpList.JumpItems.AddRange((IEnumerable<JumpItem>) jumpTaskList);
      jumpList.ShowFrequentCategory = false;
      jumpList.ShowRecentCategory = false;
      try
      {
        JumpList.SetJumpList(Application.Current, jumpList);
        path = (string) null;
        jumpList = (JumpList) null;
        addProjectWidget = (JumpTask) null;
        addCalendarWidget = (JumpTask) null;
        removeCalendarWidget = (JumpTask) null;
      }
      catch (Exception ex)
      {
        path = (string) null;
        jumpList = (JumpList) null;
        addProjectWidget = (JumpTask) null;
        addCalendarWidget = (JumpTask) null;
        removeCalendarWidget = (JumpTask) null;
      }
    }

    private static List<JumpTask> InitCustomList()
    {
      List<JumpTask> jumpTaskList1 = new List<JumpTask>();
      string recentProjects = LocalSettings.Settings.RecentProjects;
      string str1 = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\JumpIcons.dll";
      if (!string.IsNullOrEmpty(recentProjects))
      {
        List<string> list = ((IEnumerable<string>) recentProjects.Split(',')).ToList<string>();
        if (!list.Any<string>())
          return jumpTaskList1;
        foreach (string str2 in list)
        {
          if (str2.Contains(":"))
          {
            string[] strArray = str2.Split(':');
            if (strArray.Length == 2)
            {
              string str3 = strArray[0];
              string value = strArray[1];
              switch (str3)
              {
                case "project":
                  ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == value));
                  if (projectModel != null)
                  {
                    List<JumpTask> jumpTaskList2 = jumpTaskList1;
                    JumpTask jumpTask = new JumpTask();
                    jumpTask.Title = projectModel.name;
                    jumpTask.Arguments = str2;
                    jumpTask.CustomCategory = Utils.GetString("Recent");
                    jumpTask.IconResourcePath = str1;
                    jumpTask.IconResourceIndex = 3;
                    jumpTaskList2.Add(jumpTask);
                    break;
                  }
                  break;
                case "tag":
                  TagModel tagModel = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (p => p.name == value));
                  if (tagModel != null)
                  {
                    List<JumpTask> jumpTaskList3 = jumpTaskList1;
                    JumpTask jumpTask = new JumpTask();
                    jumpTask.Title = tagModel.GetDisplayName();
                    jumpTask.Arguments = str2;
                    jumpTask.CustomCategory = Utils.GetString("Recent");
                    jumpTask.IconResourcePath = str1;
                    jumpTask.IconResourceIndex = 9;
                    jumpTaskList3.Add(jumpTask);
                    break;
                  }
                  break;
                case "filter":
                  FilterModel filterModel = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (p => p.id == value));
                  if (filterModel != null)
                  {
                    List<JumpTask> jumpTaskList4 = jumpTaskList1;
                    JumpTask jumpTask = new JumpTask();
                    jumpTask.Title = filterModel.name;
                    jumpTask.Arguments = str2;
                    jumpTask.CustomCategory = Utils.GetString("Recent");
                    jumpTask.IconResourcePath = str1;
                    jumpTask.IconResourceIndex = 8;
                    jumpTaskList4.Add(jumpTask);
                    break;
                  }
                  break;
              }
            }
            else
              continue;
          }
          if (jumpTaskList1.Count == 2)
            break;
        }
      }
      return jumpTaskList1;
    }

    private static void RemoveChangeEvents()
    {
      DataChangedNotifier.ProjectChanged -= new EventHandler(JumpHelper.OnProjectChanged);
      DataChangedNotifier.FilterChanged -= new EventHandler<FilterChangeArgs>(JumpHelper.OnFilterChanged);
      DataChangedNotifier.FilterChanged -= new EventHandler<FilterChangeArgs>(JumpHelper.OnFilterChanged);
    }

    private static void OnFilterChanged(object sender, FilterChangeArgs e)
    {
      JumpHelper.InitJumpList();
    }

    public static void InitChangeEvents()
    {
      DataChangedNotifier.ProjectChanged += new EventHandler(JumpHelper.OnProjectChanged);
      DataChangedNotifier.FilterChanged += new EventHandler<FilterChangeArgs>(JumpHelper.OnFilterChanged);
      DataChangedNotifier.FilterChanged += new EventHandler<FilterChangeArgs>(JumpHelper.OnFilterChanged);
    }

    private static void OnProjectChanged(object sender, EventArgs e) => JumpHelper.InitJumpList();
  }
}
