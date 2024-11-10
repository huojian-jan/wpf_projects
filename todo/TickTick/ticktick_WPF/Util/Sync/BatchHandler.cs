// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.BatchHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util.Sync.Model;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public abstract class BatchHandler
  {
    protected string userId;
    protected SyncResult syncResult;

    public BatchHandler(string userId, SyncResult syncResult)
    {
      this.userId = userId;
      this.syncResult = syncResult;
    }
  }
}
