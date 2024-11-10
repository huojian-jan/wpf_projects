// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.ProjectWidgetsHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Provider;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public static class ProjectWidgetsHelper
  {
    private static List<WidgetWindow> _widgets = new List<WidgetWindow>();

    static ProjectWidgetsHelper()
    {
      DataChangedNotifier.ListSectionOpenChanged += new EventHandler<(string, SortOption)>(ProjectWidgetsHelper.OnListSectionOpenChanged);
    }

    private static void OnListSectionOpenChanged(object sender, (string, SortOption) e)
    {
      List<WidgetWindow> widgets = ProjectWidgetsHelper._widgets;
      // ISSUE: explicit non-virtual call
      if ((widgets != null ? (__nonvirtual (widgets.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      foreach (WidgetWindow widget in ProjectWidgetsHelper._widgets)
      {
        ProjectIdentity projectIdentity = widget?.ProjectWidget?.ProjectIdentity;
        if (projectIdentity != null && !object.Equals((object) widget.ProjectWidget, sender) && projectIdentity.CatId == e.Item1 && projectIdentity.SortOption.Equal(e.Item2))
          widget.ProjectWidget.Reload();
      }
    }

    public static async Task AddDefaultWidget()
    {
      await ProjectWidgetDao.CreateInboxWidget();
      ProjectWidgetsHelper.LoadWidgets();
    }

    public static void CloseAll()
    {
      if (ProjectWidgetsHelper._widgets.Count > 0)
      {
        for (int index = 0; index < ProjectWidgetsHelper._widgets.Count; ++index)
          ProjectWidgetsHelper._widgets[index].CloseWidget();
      }
      ProjectWidgetsHelper._widgets = new List<WidgetWindow>();
    }

    public static async Task CloseProject(string widgetId)
    {
      ProjectWidgetsHelper._widgets = ProjectWidgetsHelper._widgets.Where<WidgetWindow>((Func<WidgetWindow, bool>) (widget => widget.Model.Id != widgetId)).ToList<WidgetWindow>();
      await ProjectWidgetDao.DeleteWidgetById(widgetId);
    }

    public static async Task LoadWidgets()
    {
      List<WidgetSettingModel> widgets = await ProjectWidgetDao.GetWidgets();
      UtilLog.Info("LoadProjectWidgetsCount: " + widgets?.Count.ToString());
      if (widgets == null || widgets.Count <= 0)
        return;
      foreach (WidgetSettingModel settingModel in widgets)
      {
        if (!ProjectWidgetsHelper.IsWidgetAdded(settingModel.id))
        {
          WidgetWindow widgetWindow = new WidgetWindow(new WidgetViewModel(settingModel));
          ProjectWidgetsHelper._widgets.Add(widgetWindow);
          widgetWindow.ShowWidget();
        }
      }
    }

    public static async Task<WidgetWindow> ReopenWidget(string widgetId)
    {
      if (ProjectWidgetsHelper.IsWidgetAdded(widgetId))
      {
        WidgetWindow widgetWindow1 = ProjectWidgetsHelper._widgets.First<WidgetWindow>((Func<WidgetWindow, bool>) (widget => widget.Model.Id == widgetId));
        if (widgetWindow1 != null)
        {
          widgetWindow1.Close();
          ProjectWidgetsHelper._widgets.Remove(widgetWindow1);
        }
        WidgetSettingModel widgetById = await ProjectWidgetDao.GetWidgetById(widgetId);
        if (widgetById != null)
        {
          WidgetWindow widgetWindow2 = new WidgetWindow(new WidgetViewModel(widgetById));
          ProjectWidgetsHelper._widgets.Add(widgetWindow2);
          widgetWindow2.ShowWidget();
          return widgetWindow2;
        }
      }
      return (WidgetWindow) null;
    }

    private static bool IsWidgetAdded(string widgetId)
    {
      return ProjectWidgetsHelper._widgets.Exists((Predicate<WidgetWindow>) (widget => widget.Model.Id == widgetId));
    }

    public static void Reload(bool loadDate = false)
    {
      if (ProjectWidgetsHelper._widgets == null || ProjectWidgetsHelper._widgets.Count <= 0)
        return;
      foreach (WidgetWindow widget in ProjectWidgetsHelper._widgets)
        widget.Reload(loadDate);
    }

    public static void OnShowCountDownChanged(string id, bool showCountDown)
    {
      if (ProjectWidgetsHelper._widgets == null || ProjectWidgetsHelper._widgets.Count <= 0)
        return;
      foreach (WidgetWindow widget in ProjectWidgetsHelper._widgets)
        widget.ProjectWidget?.SetShowCountDown(LocalSettings.Settings.ShowCountDown);
    }

    public static void OnProjectChanged(ProjectIdentity project)
    {
      if (project == null)
        return;
      List<WidgetWindow> widgets = ProjectWidgetsHelper._widgets;
      // ISSUE: explicit non-virtual call
      if ((widgets != null ? (__nonvirtual (widgets.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      foreach (WidgetWindow widget in ProjectWidgetsHelper._widgets)
      {
        if (widget?.ProjectWidget?.ProjectIdentity?.QueryId == project.QueryId)
        {
          widget?.ProjectWidget?.ResetProject();
          if (!(project is NormalProjectIdentity) && widget != null)
            widget.ProjectWidget?.Reload();
        }
      }
    }

    public static void OnShowAddChanged()
    {
      List<WidgetWindow> widgets = ProjectWidgetsHelper._widgets;
      // ISSUE: explicit non-virtual call
      if ((widgets != null ? (__nonvirtual (widgets.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      foreach (WidgetWindow widget in ProjectWidgetsHelper._widgets)
        widget?.ProjectWidget?.SetShowAdd();
    }
  }
}
