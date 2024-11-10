// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchHistoryViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchHistoryViewModel : BaseViewModel
  {
    private bool _selected;

    public SearchHistoryViewModel(SearchExtra model)
    {
      this.SearchKey = model.SearchKey;
      this.Tags = model.Tags;
      this.FirstTag = this.Tags == null || this.Tags.Count <= 0 ? (string) null : this.Tags[0];
      this.SecondTag = this.Tags == null || this.Tags.Count <= 1 ? (string) null : this.Tags[1];
      this.MoreNum = this.Tags == null || this.Tags.Count <= 2 ? 0 : this.Tags.Count - 2;
      this.Selected = false;
    }

    public string SearchKey { get; set; }

    public List<string> Tags { get; set; }

    public string FirstTag { get; set; }

    public string SecondTag { get; set; }

    public int MoreNum { get; set; }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }
  }
}
