// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.RecurrenceModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Util
{
  public class RecurrenceModel : RecurrencePattern
  {
    private RecurrenceModel(string flag)
      : base(flag)
    {
    }

    public static RecurrenceModel GetRecurrenceModel(string flag, DateTime until = default (DateTime), int count = -1)
    {
      try
      {
        return new RecurrenceModel(flag);
      }
      catch (Exception ex)
      {
        string flag1 = RecurrenceModel.HandleErrorFlag(flag);
        if (flag1 != flag)
          return RecurrenceModel.GetRecurrenceModel(flag1);
      }
      return new RecurrenceModel("");
    }

    private static string HandleErrorFlag(string flag)
    {
      if (flag != null && flag.Contains("COUNT"))
      {
        List<string> list = ((IEnumerable<string>) flag.Split(';')).ToList<string>();
        string str = list.FirstOrDefault<string>((Func<string, bool>) (r => r.Contains("COUNT")));
        int result;
        if (str != null && int.TryParse(str.Replace("COUNT=", ""), out result) && result <= 1)
        {
          list.Remove(str);
          return string.Join(";", (IEnumerable<string>) list);
        }
      }
      return flag;
    }
  }
}
