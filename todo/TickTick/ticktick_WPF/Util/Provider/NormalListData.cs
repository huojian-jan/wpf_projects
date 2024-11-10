// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.NormalListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sync;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class NormalListData : SortProjectData
  {
    public NormalListData(ProjectModel project)
    {
      this.Project = project;
      bool isinbox = project.Isinbox;
      this.EmptyTitle = Utils.GetString(isinbox ? "InboxA" : (project.IsNote ? "NoteEmptyA" : "ProjectA"));
      this.EmptyContent = Utils.GetString(isinbox ? "InboxB" : (project.IsNote ? "NoteEmptyB" : "ProjectB"));
      this.EmptyPath = Utils.GetIconData(isinbox ? "IcEmptyInbox" : (project.IsNote ? "IcEmptyNoteList" : "IcEmptyProject"));
      this.AddTaskHint = project.IsEnable() ? string.Format(Utils.GetString("CenterAddTaskTextBoxPreviewText"), (object) this.GetTitle(), (object) Utils.GetString(project.IsNote ? "Notes" : "Task").ToLower()) : "";
      this.DefaultProjectModel = this.Project;
      bool? closed = project.closed;
      bool flag = true;
      this.IsProjectClosed = closed.GetValueOrDefault() == flag & closed.HasValue;
      this.IsTeamExpired = TeamDao.IsTeamExpired(project.teamId);
      this.ShowShare = !project.Isinbox;
      this.ShowAssignSort = project.IsShareList();
      this.ShowCustomSort = true;
      this.ShowLoadMore = true;
    }

    public ProjectModel Project { get; }

    public override sealed string GetTitle()
    {
      return !this.Project.Isinbox ? this.Project.name : Utils.GetString("Inbox");
    }

    public override async void SaveSortOption(SortOption sortOption)
    {
      if (!this.Project.Isinbox)
      {
        if (this.Project.SortOption != null && this.Project.SortOption.Equal(sortOption))
          return;
        this.Project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
        this.Project.SortOption = sortOption;
        this.Project.sortType = sortOption.groupBy == "none" ? sortOption.orderBy : sortOption.groupBy;
        int num = await ProjectDao.TryUpdateProject(this.Project);
      }
      else
      {
        if (sortOption.groupBy == Constants.SortType.sortOrder.ToString())
          ColumnBatchHandler.MergeWithServer(this.Project.id);
        SortProjectData.SaveSpecialProjectSortType("SortTypeOfInbox", sortOption);
        LocalSettings.Settings.SaveSmartProjectOptions("inbox", sortOption);
        ProjectDao.UpdateInboxSortType(sortOption);
      }
    }

    public override DrawingImage GetEmptyImage()
    {
      ProjectModel project1 = this.Project;
      bool flag1 = project1 != null && project1.Isinbox;
      ProjectModel project2 = this.Project;
      bool flag2 = project2 != null && project2.IsNote;
      return (DrawingImage) Application.Current?.FindResource(flag1 ? (object) "EmptyInboxDrawingImage" : (flag2 ? (object) "NoteEmptyDrawingImage" : (object) "EmptyProjectDrawingImage"));
    }

    public override Thickness GetEmptyMargin()
    {
      ProjectModel project = this.Project;
      return (project != null ? (project.IsNote ? 1 : 0) : 0) != 0 ? new Thickness(0.0, -46.0, 0.0, 0.0) : new Thickness(0.0, 0.0, 0.0, 0.0);
    }
  }
}
