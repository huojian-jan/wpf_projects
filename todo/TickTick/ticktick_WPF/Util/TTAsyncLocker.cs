// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TTAsyncLocker
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace ticktick_WPF.Util
{
  public class TTAsyncLocker
  {
    private readonly SemaphoreSlim _semaphore;

    public TTAsyncLocker(int i, int j) => this._semaphore = new SemaphoreSlim(i, j);

    public async Task RunAsync(Action worker)
    {
      await this._semaphore.WaitAsync();
      try
      {
        worker();
      }
      finally
      {
        this._semaphore.Release();
      }
    }

    public async Task RunAsync(Func<Task> worker)
    {
      await this._semaphore.WaitAsync();
      try
      {
        await worker();
      }
      finally
      {
        this._semaphore.Release();
      }
    }

    public async Task RunAsync(Func<object, Task> worker, object obj)
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
