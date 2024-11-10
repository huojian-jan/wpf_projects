// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ExDateSerilizer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public static class ExDateSerilizer
  {
    public static string ToString(string[] exDates)
    {
      return exDates != null && ((IEnumerable<string>) exDates).Any<string>() ? JsonConvert.SerializeObject((object) ((IEnumerable<string>) exDates).ToList<string>()) : string.Empty;
    }

    public static string[] ToArray(string exDate)
    {
      return !string.IsNullOrEmpty(exDate) ? JsonConvert.DeserializeObject<string[]>(exDate) : (string[]) null;
    }
  }
}
