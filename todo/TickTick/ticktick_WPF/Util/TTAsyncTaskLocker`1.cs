// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TTAsyncTaskLocker`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace ticktick_WPF.Util
{
  public class TTAsyncTaskLocker<T>
  {
    private readonly SemaphoreSlim _semaphore;

    public TTAsyncTaskLocker(int i, int j) => this._semaphore = new SemaphoreSlim(i, j);

    public async Task RunAsync(Func<T, Task> worker, T obj)
    {
      await this._semaphore.WaitAsync();
      try
      {
        await worker(obj);
      }
      finally
      {
        this._semaphore.Release();
      }
    }
  }
}
