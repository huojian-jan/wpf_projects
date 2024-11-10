// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.DefaultDateModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class DefaultDateModel
  {
    public DateTime? DefaultDate;
    public string Key;
    public int Priority;

    public static List<DefaultDateModel> GetDefaultDateMap()
    {
      return new List<DefaultDateModel>()
      {
        new DefaultDateModel()
        {
          Key = "nodue",
          Priority = 56,
          DefaultDate = new DateTime?()
        },
        new DefaultDateModel()
        {
          Key = "today",
          Priority = 0,
          DefaultDate = new DateTime?(DateTime.Today)
        },
        new DefaultDateModel()
        {
          Key = "thisweek",
          Priority = 0,
          DefaultDate = new DateTime?(DateTime.Today)
        },
        new DefaultDateModel()
        {
          Key = "thismonth",
          Priority = 0,
          DefaultDate = new DateTime?(DateTime.Today)
        },
        new DefaultDateModel()
        {
          Key = "tomorrow",
          Priority = 1,
          DefaultDate = new DateTime?(DateTime.Today.AddDays(1.0))
        },
        new DefaultDateModel()
        {
          Key = "overdue",
          Priority = -1,
          DefaultDate = new DateTime?(DateTime.Today.AddDays(-1.0))
        }
      };
    }
  }
}
