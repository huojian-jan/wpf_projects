// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskSortOrderInDateModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TaskSortOrderInDateModel : BaseModel
  {
    public string userid { get; set; }

    public string date { get; set; }

    public string projectid { get; set; }

    public string taskid { get; set; }

    public long sortOrder { get; set; }

    public long modifiedTime { get; set; }

    public int status { get; set; }

    public int type { get; set; }

    public int deleted { get; set; }
  }
}
