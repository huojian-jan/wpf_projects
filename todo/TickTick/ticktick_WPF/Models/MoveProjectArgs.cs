// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.MoveProjectArgs
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Models
{
  public class MoveProjectArgs
  {
    public MoveProjectArgs(ProjectModel project)
    {
      this.Project = project;
      this.ProjectId = project?.id;
    }

    public MoveProjectArgs(string taskId, ProjectModel project, bool keepAssignee = false, string undoId = null)
    {
      this.TaskId = taskId;
      this.Project = project;
      this.ProjectId = project?.id;
      this.KeepAssignee = keepAssignee;
      this.UndoId = undoId;
    }

    public string TaskId { get; set; }

    public string ProjectId { get; set; }

    public string UndoId { get; set; }

    public string ColumnId { get; set; }

    public ProjectModel Project { get; set; }

    public bool KeepAssignee { get; set; }

    public bool? IsTop { get; set; }

    public bool Notify { get; set; } = true;
  }
}
