// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TagSortTypeDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class TagSortTypeDao
  {
    public static async Task<TagSortTypeModel> GetTagSortTypeDbByuserIdAndTagAsync(
      string userid,
      string tag)
    {
      return await Task.Run<TagSortTypeModel>((Func<Task<TagSortTypeModel>>) (async () =>
      {
        List<TagSortTypeModel> listAsync = await App.Connection.Table<TagSortTypeModel>().Where((Expression<Func<TagSortTypeModel, bool>>) (v => v.userid == userid && v.tag == tag)).ToListAsync();
        return listAsync.Count == 0 ? (TagSortTypeModel) null : listAsync[0];
      }));
    }

    public static async Task UpdateOrInsertTagSortTypeAsync(TagSortTypeModel tagSortTypeModel)
    {
      List<TagSortTypeModel> listAsync = await App.Connection.Table<TagSortTypeModel>().Where((Expression<Func<TagSortTypeModel, bool>>) (v => v.userid == tagSortTypeModel.userid && v.tag == tagSortTypeModel.tag)).ToListAsync();
      if (listAsync.Count != 0)
      {
        tagSortTypeModel._Id = listAsync[0]._Id;
        int num = await App.Connection.UpdateAsync((object) tagSortTypeModel);
      }
      else
      {
        int num1 = await App.Connection.InsertAsync((object) tagSortTypeModel);
      }
    }
  }
}
