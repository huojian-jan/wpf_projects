// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.TaskSyncModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class TaskSyncModel
  {
    private AttachmentSyncBean attachmentSyncBean = new AttachmentSyncBean();
    private TaskSyncBean taskSyncBean = new TaskSyncBean();
    private TaskSyncedJsonBean taskSyncedJsonBean = new TaskSyncedJsonBean();

    public TaskSyncBean TaskSyncBean
    {
      get => this.taskSyncBean;
      set => this.taskSyncBean = value;
    }

    public TaskSyncedJsonBean TaskSyncedJsonBean
    {
      get => this.taskSyncedJsonBean;
      set => this.taskSyncedJsonBean = value;
    }

    public AttachmentSyncBean AttachmentSyncBean
    {
      get => this.attachmentSyncBean;
      set => this.attachmentSyncBean = value;
    }
  }
}
