// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TTAsyncLocker`4
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace ticktick_WPF.Util
{
  public class TTAsyncLocker<T1, T2, T3, TResult>
  {
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task<TResult> RunAsync(
      Func<T1, T2, T3, Task<TResult>> worker,
      T1 arg1,
      T2 arg2,
      T3 arg3)
    {
      await this._semaphore.WaitAsync();
      TResult result;
      try
      {
        result = await worker(arg1, arg2, arg3);
      }
      finally
      {
        this._semaphore.Release();
      }
      return result;
    }
  }
}
