// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Summary.SummaryFilterModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Search;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Summary
{
  public class SummaryFilterModel
  {
    public DateFilter DateFilter { get; set; } = DateFilter.ThisWeek;

    public List<string> SelectedTags { get; set; } = new List<string>();

    public List<string> SelectedProjectIds { get; set; } = new List<string>();

    public List<string> SelectedProjectGroupIds { get; set; } = new List<string>();

    public List<string> SelectedPriorities { get; set; } = new List<string>();

    public List<string> SelectedStatus { get; set; } = new List<string>();

    public List<string> Assigns { get; set; } = new List<string>();

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? StartDate { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? EndDate { get; set; }

    public SummarySortType SortBy { get; set; }

    public bool ShowCompleteDate { get; set; } = true;

    public bool ShowDetail { get; set; }

    public bool ShowPomo { get; set; }

    public bool ShowProgress { get; set; } = true;

    public bool ShowBelongList { get; set; }

    public List<SummaryDisplayItemModel> DisplayItems { get; set; } = new List<SummaryDisplayItemModel>();

    public void Save()
    {
      LocalSettings.Settings.SetSummaryFilter(this);
      LocalSettings.Settings.Save();
    }
  }
}
