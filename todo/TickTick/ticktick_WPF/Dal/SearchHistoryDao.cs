// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.SearchHistoryDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class SearchHistoryDao
  {
    private static List<SearchExtra> _searchHistoryModels;
    private static List<SearchHistoryModel> _historyModles = new List<SearchHistoryModel>();

    public static async Task GetSearchHistoryModels()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      SearchHistoryDao._historyModles = await App.Connection.Table<SearchHistoryModel>().Where((Expression<Func<SearchHistoryModel, bool>>) (m => m.userId == userId)).OrderByDescending<long>((Expression<Func<SearchHistoryModel, long>>) (m => m.sortOrder)).ToListAsync();
      SearchHistoryDao._searchHistoryModels = new List<SearchExtra>();
      if (SearchHistoryDao._historyModles == null)
        return;
      for (int index = 0; index < SearchHistoryDao._historyModles.Count; ++index)
      {
        if (index < 5)
          SearchHistoryDao._searchHistoryModels.Add(new SearchExtra(SearchHistoryDao._historyModles[index]));
        else
          App.Connection.DeleteAsync((object) SearchHistoryDao._historyModles[index]);
      }
    }

    public static async Task AddHistoryModel(string key, List<string> tags)
    {
      if (SearchHistoryDao._searchHistoryModels == null)
        SearchHistoryDao._searchHistoryModels = new List<SearchExtra>();
      key = key ?? string.Empty;
      tags = tags ?? new List<string>();
      SearchExtra searchExtra = SearchHistoryDao._searchHistoryModels.FirstOrDefault<SearchExtra>((Func<SearchExtra, bool>) (m => (m.SearchKey?.Trim() ?? "") == key.Trim() && !m.Tags.Except<string>((IEnumerable<string>) tags).Any<string>() && !tags.Except<string>((IEnumerable<string>) m.Tags).Any<string>()));
      if (searchExtra != null)
      {
        int index = SearchHistoryDao._searchHistoryModels.IndexOf(searchExtra);
        SearchHistoryDao._searchHistoryModels.Remove(searchExtra);
        SearchHistoryDao._searchHistoryModels.Insert(0, searchExtra);
        SearchHistoryModel historyModle = SearchHistoryDao._historyModles[index];
        SearchHistoryDao._historyModles.Remove(historyModle);
        SearchHistoryDao._historyModles.Insert(0, historyModle);
        historyModle.sortOrder = Utils.GetNowTimeStamp();
        App.Connection.UpdateAsync((object) historyModle);
      }
      else
      {
        string str = Utils.GetCurrentUserIdInt().ToString();
        long nowTimeStamp = Utils.GetNowTimeStamp();
        SearchHistoryModel model = new SearchHistoryModel()
        {
          userId = str,
          keyText = key.Trim(),
          tags = tags.Join<string>("#"),
          sortOrder = nowTimeStamp
        };
        SearchHistoryDao._searchHistoryModels.Insert(0, new SearchExtra(model));
        SearchHistoryDao._historyModles.Insert(0, model);
        if (SearchHistoryDao._searchHistoryModels.Count > 5)
          SearchHistoryDao._searchHistoryModels.RemoveAt(5);
        App.Connection.InsertAsync((object) model);
      }
    }

    public static async Task ClearHistory()
    {
      SearchHistoryDao._searchHistoryModels.Clear();
      string userId = Utils.GetCurrentUserIdInt().ToString();
      AsyncTableQuery<SearchHistoryModel> asyncTableQuery = App.Connection.Table<SearchHistoryModel>().Where((Expression<Func<SearchHistoryModel, bool>>) (m => m.userId == userId));
      Expression<Func<SearchHistoryModel, long>> orderExpr = (Expression<Func<SearchHistoryModel, long>>) (m => m.sortOrder);
      foreach (object obj in await asyncTableQuery.OrderByDescending<long>(orderExpr).ToListAsync())
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task<List<SearchExtra>> GetHistoryViewModels()
    {
      if (SearchHistoryDao._searchHistoryModels == null || SearchHistoryDao._searchHistoryModels.Count == 0)
        await SearchHistoryDao.GetSearchHistoryModels();
      return SearchHistoryDao._searchHistoryModels;
    }

    public static void Clear() => SearchHistoryDao._searchHistoryModels?.Clear();
  }
}
