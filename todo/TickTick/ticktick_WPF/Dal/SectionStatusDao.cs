// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.SectionStatusDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class SectionStatusDao
  {
    private static async Task<bool> IsProjectSectionClosed(string identity, string name)
    {
      return await App.Connection.Table<SectionStatusModel>().Where((Expression<Func<SectionStatusModel, bool>>) (v => v.Identity == identity && v.Name == name)).CountAsync() > 0;
    }

    public static async Task OpenProjectSection(string identity, string name)
    {
      List<SectionStatusModel> listAsync = await App.Connection.Table<SectionStatusModel>().Where((Expression<Func<SectionStatusModel, bool>>) (v => v.Identity == identity && v.Name == name)).ToListAsync();
      if (listAsync == null)
        ;
      else if (listAsync.Count <= 0)
        ;
      else
      {
        foreach (object obj in listAsync)
        {
          int num = await App.Connection.DeleteAsync(obj);
        }
        CacheManager.DeleteSectionStatus(new SectionStatusModel()
        {
          Identity = identity,
          Name = name
        });
      }
    }

    public static async Task CloseProjectSection(string identity, string name)
    {
      if (await SectionStatusDao.IsProjectSectionClosed(identity, name))
        return;
      int num = await App.Connection.InsertAsync((object) new SectionStatusModel()
      {
        Identity = identity,
        Name = name
      });
      CacheManager.AddSectionStatus(new SectionStatusModel()
      {
        Identity = identity,
        Name = name
      });
    }

    public static async Task<List<SectionStatusModel>> GetSectionStatus()
    {
      return await App.Connection.Table<SectionStatusModel>().ToListAsync();
    }

    public static async Task<List<SectionStatusModel>> GetSectionStatus(List<string> identities)
    {
      return await App.Connection.Table<SectionStatusModel>().Where((Expression<Func<SectionStatusModel, bool>>) (v => identities.Contains(v.Identity))).ToListAsync();
    }

    public static async Task<List<SectionStatusModel>> GetSectionStatus(string projectId)
    {
      return await App.Connection.Table<SectionStatusModel>().Where((Expression<Func<SectionStatusModel, bool>>) (v => v.Identity == projectId)).ToListAsync();
    }
  }
}
