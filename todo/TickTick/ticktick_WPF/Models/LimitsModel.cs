// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.LimitsModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class LimitsModel : BaseModel
  {
    public string type { get; set; }

    public int projectNumber { get; set; }

    public int projectTaskNumber { get; set; }

    public int subtaskNumber { get; set; }

    public int shareUserNumber { get; set; }

    public int dailyUploadNumber { get; set; }

    public int taskAttachmentNumber { get; set; }

    public int reminderNumber { get; set; }

    public int dailyReminderNumber { get; set; }

    public int attachmentSize { get; set; }

    public int kanbanNumber { get; set; }

    public int habitNumber { get; set; }

    public int timerNumber { get; set; }

    public int visitorNumber { get; set; }
  }
}
