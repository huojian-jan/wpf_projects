// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.ParentTaskIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public sealed class ParentTaskIdentity : ProjectIdentity
  {
    private TaskBaseViewModel _task;
    public string ProjectId;

    public override string SortProjectId => (string) null;

    public ParentTaskIdentity(TaskBaseViewModel task)
    {
      this._task = task;
      this.Id = task.Id;
      this.ProjectId = task.ProjectId;
      this.SortOption = new SortOption()
      {
        groupBy = "none",
        orderBy = "sortOrder"
      };
    }

    public override ProjectIdentity Copy(ProjectIdentity project)
    {
      return (ProjectIdentity) new ParentTaskIdentity(((ParentTaskIdentity) project)._task);
    }
  }
}
