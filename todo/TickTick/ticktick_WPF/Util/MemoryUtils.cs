// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.MemoryUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.InteropServices;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class MemoryUtils
  {
    [DllImport("kernel32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GlobalMemoryStatusEx(ref MemoryUtils.MEMORY_INFO mi);

    public static string FormatSize(double size)
    {
      double num = size;
      int index;
      for (index = 0; num > 1024.0 && index < 5; ++index)
        num /= 1024.0;
      string[] strArray = new string[5]
      {
        "B",
        "KB",
        "MB",
        "GB",
        "TB"
      };
      return string.Format("{0} {1}", (object) Math.Round(num, 2), (object) strArray[index]);
    }

    public static MemoryUtils.MEMORY_INFO GetMemoryStatus()
    {
      MemoryUtils.MEMORY_INFO mi = new MemoryUtils.MEMORY_INFO();
      mi.dwLength = (uint) Marshal.SizeOf<MemoryUtils.MEMORY_INFO>(mi);
      MemoryUtils.GlobalMemoryStatusEx(ref mi);
      return mi;
    }

    public static ulong GetAvailPhys() => MemoryUtils.GetMemoryStatus().ullAvailPhys;

    public static ulong GetUsedPhys()
    {
      MemoryUtils.MEMORY_INFO memoryStatus = MemoryUtils.GetMemoryStatus();
      return memoryStatus.ullTotalPhys - memoryStatus.ullAvailPhys;
    }

    public static ulong GetTotalPhys() => MemoryUtils.GetMemoryStatus().ullTotalPhys;

    public struct MEMORY_INFO
    {
      public uint dwLength;
      public uint dwMemoryLoad;
      public ulong ullTotalPhys;
      public ulong ullAvailPhys;
      public ulong ullTotalPageFile;
      public ulong ullAvailPageFile;
      public ulong ullTotalVirtual;
      public ulong ullAvailVirtual;
      public ulong ullAvailExtendedVirtual;
    }
  }
}
