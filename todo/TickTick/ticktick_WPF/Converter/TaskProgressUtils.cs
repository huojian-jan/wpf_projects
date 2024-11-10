// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskProgressUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Converter
{
  internal class TaskProgressUtils
  {
    public static double GetPercentWidth(int percent, double width)
    {
      return percent < 0 || percent > 100 ? 0.0 : width * ((double) percent / 100.0);
    }
  }
}
