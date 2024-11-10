// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TTAsyncLocker`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace ticktick_WPF.Util
{
  public class TTAsyncLocker<TResult>
  {
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task<TResult> RunAsync(Func<Task<TResult>> worker)
    {
      await this._semaphore.WaitAsync();
      TResult result;
      try
      {
        result = await worker();
      }
      finally
      {
        this._semaphore.Release();
      }
      return result;
    }
  }
}
