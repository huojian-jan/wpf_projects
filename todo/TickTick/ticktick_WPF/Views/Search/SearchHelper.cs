// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public static class SearchHelper
  {
    private static string _searchKey;
    public static SearchFilterViewModel SearchFilter = new SearchFilterViewModel();
    private static SolidColorBrush _searchHighlightColor;
    private static SolidColorBrush _darkSearchHighlightColor;
    public static List<string> Tags;
    public static Regex PreSearchRegex = new Regex("");
    public static Regex SearchRegex = new Regex("");
    private static BlockingList<TaskSearchModel> _tempSearchModel;

    public static event EventHandler SearchKeyChanged;

    public static string SearchKey
    {
      get => SearchHelper._searchKey;
      set
      {
        SearchHelper._searchKey = value;
        List<string> stringList1;
        if (value == null)
          stringList1 = (List<string>) null;
        else
          stringList1 = ((IEnumerable<string>) value.ToLower().Split(' ')).ToList<string>();
        List<string> stringList2 = stringList1;
        stringList2?.Remove("");
        string pattern = "(" + Utils.HandleReg(SearchHelper.SearchKey?.ToLower()) + ")";
        if (stringList2 != null && __nonvirtual (stringList2.Count) > 1)
        {
          foreach (string original in stringList2)
            pattern = pattern + "|(" + Utils.HandleReg(original) + ")";
        }
        SearchHelper.SearchRegex = new Regex(pattern);
        EventHandler searchKeyChanged = SearchHelper.SearchKeyChanged;
        if (searchKeyChanged == null)
          return;
        searchKeyChanged((object) null, (EventArgs) null);
      }
    }

    public static bool DetailChanged { get; set; }

    public static int KeyWordMatch(
      string text,
      string keyword,
      List<string> keys,
      bool ignoreCase = false)
    {
      if (ignoreCase)
      {
        text = text.ToLower();
        keyword = keyword.ToLower();
        keys = keys.Select<string, string>((Func<string, string>) (it => it.ToLower())).ToList<string>();
      }
      if (string.IsNullOrEmpty(text))
        return 0;
      if (text.Contains(keyword))
        return 1;
      if (keys.Count > 0)
      {
        bool flag = true;
        foreach (string key in keys)
        {
          if (text.Contains(key))
          {
            Utils.ReplaceFirst(ref text, key, "");
          }
          else
          {
            flag = false;
            break;
          }
        }
        if (flag)
          return 2;
      }
      return 0;
    }

    public static bool KeyWordMatched(string text, string keyword, List<string> keys)
    {
      return SearchHelper.KeyWordMatch(text, keyword, keys) > 0;
    }

    public static async void TrySearchFromService(string searchKey, SearchFilterModel filterModel)
    {
      SearchResultModel searchedResult = await Communicator.GetSearchedResult(searchKey, filterModel);
      bool flag1 = searchedResult?.Tasks != null;
      if (flag1)
        flag1 = await TaskService.MergeTasks((IEnumerable<TaskModel>) searchedResult.Tasks);
      bool taskMerged = flag1;
      bool flag2 = searchedResult?.Comments != null;
      if (flag2)
        flag2 = await CommentService.MergeComments((IEnumerable<CommentModel>) searchedResult.Comments);
      if (!(taskMerged | flag2))
      {
        searchedResult = (SearchResultModel) null;
      }
      else
      {
        SearchHelper.ClearTaskSearchModels();
        ListViewContainer listView = App.Window.ListView;
        if (listView == null)
        {
          searchedResult = (SearchResultModel) null;
        }
        else
        {
          listView.ReSearch();
          searchedResult = (SearchResultModel) null;
        }
      }
    }

    public static Brush GetSearchHighlightColor()
    {
      if (LocalSettings.Settings.IsDarkTheme)
      {
        if (SearchHelper._darkSearchHighlightColor == null)
          SearchHelper._darkSearchHighlightColor = ThemeUtil.GetColorInString("#CCFFD54D");
        return (Brush) SearchHelper._darkSearchHighlightColor ?? (Brush) Brushes.Transparent;
      }
      if (SearchHelper._searchHighlightColor == null)
        SearchHelper._searchHighlightColor = ThemeUtil.GetColorInString("#FFD54D");
      return (Brush) SearchHelper._searchHighlightColor ?? (Brush) Brushes.Transparent;
    }

    public static async Task<List<TaskSearchModel>> GetTaskSearchModels(
      SearchFilterModel filter,
      List<string> tags)
    {
      if (SearchHelper._tempSearchModel == null)
        SearchHelper._tempSearchModel = new BlockingList<TaskSearchModel>((IEnumerable<TaskSearchModel>) await TaskViewModelHelper.GetTaskSearchModels(filter, tags));
      return SearchHelper._tempSearchModel.Value;
    }

    public static async Task OnEventArchiveChanged(string eventId)
    {
      TaskSearchModel model = SearchHelper._tempSearchModel?.FirstOrDefault((Func<TaskSearchModel, bool>) (m => m.Id == eventId));
      if (model == null)
        model = (TaskSearchModel) null;
      else if (!string.IsNullOrEmpty(model.SourceModel?.RepeatFlag))
      {
        CalendarEventModel eve = await CalendarEventDao.GetEventById(eventId);
        List<CalendarEventModel> skipEvents = await CalendarEventDao.GetSkipEvents();
        DateTime? eventNextRepeat = CalendarEventDao.GetEventNextRepeat(eve, skipEvents, await ArchivedDao.GetArchivedKeys());
        if (eventNextRepeat.HasValue)
        {
          DateTime? dueEnd = eve.DueEnd;
          DateTime? dueStart = eve.DueStart;
          TimeSpan? nullable1 = dueEnd.HasValue & dueStart.HasValue ? new TimeSpan?(dueEnd.GetValueOrDefault() - dueStart.GetValueOrDefault()) : new TimeSpan?();
          ref TimeSpan? local = ref nullable1;
          double? nullable2 = local.HasValue ? new double?(local.GetValueOrDefault().TotalMinutes) : new double?();
          eve.DueStart = eventNextRepeat;
          if (nullable2.HasValue)
            eve.DueEnd = new DateTime?(eventNextRepeat.Value.AddMinutes(nullable2.Value));
          model.SourceModel = new TaskBaseViewModel(eve);
          model.Content = eve.Content;
        }
        else
          SearchHelper._tempSearchModel?.Remove(model);
        eve = (CalendarEventModel) null;
        skipEvents = (List<CalendarEventModel>) null;
        model = (TaskSearchModel) null;
      }
      else
      {
        BlockingList<TaskSearchModel> tempSearchModel = SearchHelper._tempSearchModel;
        if (tempSearchModel == null)
        {
          model = (TaskSearchModel) null;
        }
        else
        {
          tempSearchModel.Remove(model);
          model = (TaskSearchModel) null;
        }
      }
    }

    public static void ClearTaskSearchModels()
    {
      SearchHelper._tempSearchModel = (BlockingList<TaskSearchModel>) null;
    }
  }
}
