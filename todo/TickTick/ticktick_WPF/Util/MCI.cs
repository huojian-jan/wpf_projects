// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.MCI
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

#nullable disable
namespace ticktick_WPF.Util
{
  public class MCI
  {
    private Thread thread;

    [DllImport("winmm.dll")]
    private static extern long mciSendString(
      string command,
      string returnString,
      int returnSize,
      IntPtr hwndCallback);

    private async Task PlayOnce(string file)
    {
      MCI.mciSendString(string.Format("open \"{0}\" type mpegvideo alias tickMedia", (object) file), (string) null, 0, IntPtr.Zero);
      MCI.mciSendString("play tickMedia from 0", (string) null, 0, IntPtr.Zero);
      await Task.Delay(10000);
      MCI.mciSendString("close tickMedia", (string) null, 0, IntPtr.Zero);
    }

    private void PlayRepeat(string file)
    {
      MCI.mciSendString(string.Format("open \"{0}\" type mpegvideo alias tickMedia", (object) file), (string) null, 0, IntPtr.Zero);
      MCI.mciSendString("play tickMedia repeat", (string) null, 0, IntPtr.Zero);
    }

    public void Stop() => MCI.mciSendString("close tickMedia", (string) null, 0, IntPtr.Zero);

    public void Play(string file, int times)
    {
      if (times == 0)
      {
        this.PlayRepeat(file);
      }
      else
      {
        if (times <= 0)
          return;
        this.PlayOnce(file);
      }
    }

    public void Exit()
    {
      if (this.thread == null)
        return;
      try
      {
        this.thread.Abort();
      }
      catch
      {
      }
      this.thread = (Thread) null;
    }
  }
}
