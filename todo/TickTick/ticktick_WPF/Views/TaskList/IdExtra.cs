// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.IdExtra
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class IdExtra
  {
    public readonly string ItemId;
    public readonly string TaskId;

    public IdExtra(string id, string taskId)
    {
      this.ItemId = id;
      this.TaskId = taskId;
    }
  }
}
