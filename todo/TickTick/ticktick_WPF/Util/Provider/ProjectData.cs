// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.ProjectData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public abstract class ProjectData
  {
    public string TitleInProjectGroup;

    public ProjectIdentity ProjectIdentity { get; set; }

    public bool IsCompleted { get; set; }

    public bool IsTrash { get; set; }

    public bool IsProjectClosed { get; set; }

    public bool IsTeamExpired { get; set; }

    public string EmptyTitle { get; protected set; }

    public string EmptyContent { get; protected set; }

    public string AddTaskHint { get; protected set; }

    public DrawingImage EmptyImage { get; protected set; }

    public Geometry EmptyPath { get; protected set; }

    public DateTime? DefaultTaskDate { get; protected set; }

    public ProjectModel DefaultProjectModel { get; protected set; }

    public bool ShowShare { get; protected set; }

    public bool ShowProjectSort { get; protected set; }

    public bool ShowCustomSort { get; protected set; }

    public bool ShowAssignSort { get; protected set; }

    private static DrawingImage GetDefaultAvatar()
    {
      return Application.Current?.FindResource((object) "EmptyAllDrawingImage") as DrawingImage;
    }

    protected bool IgnoreTodaySection { get; set; }

    protected ProjectData()
    {
      ProjectModel projectModel = new ProjectModel();
      projectModel._Id = LocalSettings.Settings.InBoxId;
      projectModel.id = "inbox" + LocalSettings.Settings.LoginUserId;
      projectModel.name = Utils.GetString("Inbox");
      // ISSUE: reference to a compiler-generated field
      this.\u003CDefaultProjectModel\u003Ek__BackingField = projectModel;
      // ISSUE: explicit constructor call
      base.\u002Ector();
    }
  }
}
