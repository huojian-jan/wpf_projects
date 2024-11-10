// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ArrangeSectionStatusDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class ArrangeSectionStatusDao
  {
    private static List<string> _listNames = new List<string>();
    private static List<string> _tagNames = new List<string>();
    private static List<string> _priorityNames = new List<string>();

    public static async Task InitClosedSectionNames()
    {
      List<ArrangeSectionStatusModel> listAsync = await App.Connection.Table<ArrangeSectionStatusModel>().Where((Expression<Func<ArrangeSectionStatusModel, bool>>) (m => m.UserId == LocalSettings.Settings.LoginUserId)).ToListAsync();
      ArrangeSectionStatusDao._listNames.AddRange((IEnumerable<string>) ((listAsync != null ? listAsync.Where<ArrangeSectionStatusModel>((Func<ArrangeSectionStatusModel, bool>) (m => m.Type == 0)).ToList<ArrangeSectionStatusModel>().Select<ArrangeSectionStatusModel, string>((Func<ArrangeSectionStatusModel, string>) (m => m.Name)).ToList<string>() : (List<string>) null) ?? new List<string>()));
      ArrangeSectionStatusDao._tagNames.AddRange((IEnumerable<string>) ((listAsync != null ? listAsync.Where<ArrangeSectionStatusModel>((Func<ArrangeSectionStatusModel, bool>) (m => m.Type == 1)).ToList<ArrangeSectionStatusModel>().Select<ArrangeSectionStatusModel, string>((Func<ArrangeSectionStatusModel, string>) (m => m.Name)).ToList<string>() : (List<string>) null) ?? new List<string>()));
      ArrangeSectionStatusDao._priorityNames.AddRange((IEnumerable<string>) ((listAsync != null ? listAsync.Where<ArrangeSectionStatusModel>((Func<ArrangeSectionStatusModel, bool>) (m => m.Type == 2)).ToList<ArrangeSectionStatusModel>().Select<ArrangeSectionStatusModel, string>((Func<ArrangeSectionStatusModel, string>) (m => m.Name)).ToList<string>() : (List<string>) null) ?? new List<string>()));
    }

    public static async Task AddSectionStatusModel(ArrangeSectionStatusModel model)
    {
      bool flag = false;
      switch (model.Type)
      {
        case 0:
          if (!ArrangeSectionStatusDao._listNames.Contains(model.Name))
          {
            ArrangeSectionStatusDao._listNames.Add(model.Name);
            flag = true;
            break;
          }
          break;
        case 1:
          if (!ArrangeSectionStatusDao._tagNames.Contains(model.Name))
          {
            ArrangeSectionStatusDao._tagNames.Add(model.Name);
            flag = true;
            break;
          }
          break;
        case 2:
          if (!ArrangeSectionStatusDao._priorityNames.Contains(model.Name))
          {
            ArrangeSectionStatusDao._priorityNames.Add(model.Name);
            flag = true;
            break;
          }
          break;
      }
      if (!flag)
        return;
      int num = await App.Connection.InsertAsync((object) model);
    }

    public static async Task DeleteSectionStatusModel(ArrangeSectionStatusModel model)
    {
      switch (model.Type)
      {
        case 0:
          if (ArrangeSectionStatusDao._listNames.Contains(model.Name))
          {
            ArrangeSectionStatusDao._listNames.Remove(model.Name);
            break;
          }
          break;
        case 1:
          if (ArrangeSectionStatusDao._tagNames.Contains(model.Name))
          {
            ArrangeSectionStatusDao._tagNames.Remove(model.Name);
            break;
          }
          break;
        case 2:
          if (ArrangeSectionStatusDao._priorityNames.Contains(model.Name))
          {
            ArrangeSectionStatusDao._priorityNames.Remove(model.Name);
            break;
          }
          break;
      }
      List<ArrangeSectionStatusModel> listAsync = await App.Connection.Table<ArrangeSectionStatusModel>().Where((Expression<Func<ArrangeSectionStatusModel, bool>>) (m => m.Name == model.Name && m.UserId == LocalSettings.Settings.LoginUserId && m.Type == model.Type)).ToListAsync();
      if (listAsync == null)
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static bool CheckSectionClosed(int type, string name)
    {
      switch (type)
      {
        case 0:
          return ArrangeSectionStatusDao._listNames.Contains(name);
        case 1:
          return ArrangeSectionStatusDao._tagNames.Contains(name);
        case 2:
          return ArrangeSectionStatusDao._priorityNames.Contains(name);
        default:
          return false;
      }
    }
  }
}
