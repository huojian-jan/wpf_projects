// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.HabitSectionCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class HabitSectionCache
  {
    private static List<HabitSectionModel> _sections = new List<HabitSectionModel>();

    public static async Task SetSections()
    {
      HabitSectionCache._sections = await HabitSectionDao.GetAllHabitSections();
    }

    public static List<HabitSectionModel> GetSections()
    {
      return HabitSectionCache._sections.Where<HabitSectionModel>((Func<HabitSectionModel, bool>) (s => s.SyncStatus != -1)).ToList<HabitSectionModel>();
    }

    public static Dictionary<string, HabitSectionModel> GetSectionDict()
    {
      Dictionary<string, HabitSectionModel> sectionDict = new Dictionary<string, HabitSectionModel>();
      foreach (HabitSectionModel section in HabitSectionCache._sections)
      {
        if (!string.IsNullOrEmpty(section.Id) && section.SyncStatus != -1)
          sectionDict[section.Id] = section;
      }
      return sectionDict;
    }
  }
}
