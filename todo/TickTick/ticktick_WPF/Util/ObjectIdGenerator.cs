// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ObjectIdGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Diagnostics;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ObjectIdGenerator
  {
    private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly object _innerLock = new object();
    private static int _counter;
    private static readonly byte[] _machineHash = ObjectIdGenerator.GenerateHostHash();
    private static readonly byte[] _processId = ObjectIdGenerator.GenerateProcessId();
    private static readonly object lockObject = new object();

    public static byte[] Generate()
    {
      lock (ObjectIdGenerator.lockObject)
      {
        byte[] destinationArray = new byte[12];
        int num = 0;
        byte[] time = ObjectIdGenerator.GenerateTime();
        Array.Reverse((Array) time);
        Array.Copy((Array) time, 0, (Array) destinationArray, 0, 4);
        int destinationIndex1 = num + 4;
        Array.Copy((Array) ObjectIdGenerator._machineHash, 0, (Array) destinationArray, destinationIndex1, 3);
        int destinationIndex2 = destinationIndex1 + 3;
        Array.Copy((Array) ObjectIdGenerator._processId, 0, (Array) destinationArray, destinationIndex2, 2);
        int destinationIndex3 = destinationIndex2 + 2;
        byte[] bytes = BitConverter.GetBytes(ObjectIdGenerator.GenerateCounter());
        Array.Reverse((Array) bytes);
        Array.Copy((Array) bytes, 1, (Array) destinationArray, destinationIndex3, 3);
        return destinationArray;
      }
    }

    private static byte[] GenerateTime()
    {
      return BitConverter.GetBytes((int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
    }

    private static byte[] GenerateHostHash() => Convert.FromBase64String(App.UId ?? "xxxxxxxx");

    private static byte[] GenerateProcessId()
    {
      byte[] bytes = BitConverter.GetBytes(Process.GetCurrentProcess().Id);
      Array.Reverse((Array) bytes);
      return bytes;
    }

    private static int GenerateCounter()
    {
      lock (ObjectIdGenerator._innerLock)
        return ObjectIdGenerator._counter++;
    }

    public static string GenerateString()
    {
      return BitConverter.ToString(ObjectIdGenerator.Generate()).Replace("-", "").ToLower();
    }
  }
}
