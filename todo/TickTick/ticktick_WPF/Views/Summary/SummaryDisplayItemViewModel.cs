// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Summary.SummaryDisplayItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Summary
{
  public class SummaryDisplayItemViewModel : BaseViewModel
  {
    private bool _enabled;
    private bool _isDragging;
    private bool _isDraggingOver;
    private readonly List<string> _proItems = new List<string>()
    {
      "status",
      "dueDate",
      "parentTask",
      "tag"
    };
    private static readonly Dictionary<string, string> Key2NameMap = new Dictionary<string, string>()
    {
      {
        "status",
        "task_status"
      },
      {
        "completedTime",
        "summary_completed_time"
      },
      {
        "progress",
        "Progress"
      },
      {
        "focus",
        "FocusData"
      },
      {
        "project",
        "BelongList"
      },
      {
        "dueDate",
        "task_time"
      },
      {
        "title",
        "task_title"
      },
      {
        "parentTask",
        "parent_task"
      },
      {
        "tag",
        "tag"
      },
      {
        "detail",
        "DetailContent"
      }
    };
    private static readonly Dictionary<string, List<string>> Key2StylesMap = new Dictionary<string, List<string>>()
    {
      {
        "completedTime",
        new List<string>() { "date", "time" }
      },
      {
        "dueDate",
        new List<string>() { "date", "time" }
      },
      {
        "project",
        new List<string>() { "project", "column" }
      }
    };
    private static readonly Dictionary<string, string> Style2NameMap = new Dictionary<string, string>()
    {
      {
        "date",
        "date"
      },
      {
        "time",
        "date_plus_time"
      },
      {
        "project",
        "List"
      },
      {
        "column",
        "project_column"
      },
      {
        "markdown",
        "Markdown"
      },
      {
        "text",
        "Text"
      }
    };

    public string Key { get; set; }

    public long SortOrder { get; set; }

    public string Style { get; set; }

    public string Name { get; set; }

    public bool HasStyleOption { get; set; }

    public List<string> SupportedStyles { get; set; }

    public bool IsDraggingOver
    {
      get => this._isDraggingOver;
      set
      {
        this._isDraggingOver = value;
        this.OnPropertyChanged(nameof (IsDraggingOver));
      }
    }

    public bool IsDragging
    {
      get => this._isDragging;
      set
      {
        this._isDragging = value;
        this.OnPropertyChanged(nameof (IsDragging));
      }
    }

    public bool Draggable => this.Key != "status" && this.Key != "detail";

    public bool ForceEnabled => this.Key == "title";

    public bool Enabled
    {
      get => this._enabled;
      set
      {
        this._enabled = value;
        this.OnPropertyChanged(nameof (Enabled));
      }
    }

    public bool IsProItem => this._proItems.Contains(this.Key);

    public SummaryDisplayItemModel ToModel()
    {
      return new SummaryDisplayItemModel(this.Key, this.SortOrder, this.Style, this.Enabled);
    }

    public SummaryDisplayItemViewModel(SummaryDisplayItemModel model)
    {
      this.Key = model.Key;
      this.SortOrder = model.SortOrder;
      this.Enabled = model.Enabled;
      this.Style = model.Enabled ? model.Style : (string) null;
      this.Name = this.GetNameByKey();
      List<string> stringList;
      this.SupportedStyles = SummaryDisplayItemViewModel.Key2StylesMap.TryGetValue(model.Key, out stringList) ? stringList : new List<string>();
      this.HasStyleOption = this.SupportedStyles != null && this.SupportedStyles.Count > 0;
    }

    public SummaryDisplayItemViewModel(SummaryDisplayItem model)
    {
      this.Key = model.key;
      this.SortOrder = model.sortOrder;
      this.Enabled = model.enabled;
      this.Style = model.enabled ? model.style : (string) null;
      this.Name = this.GetNameByKey();
      List<string> stringList;
      this.SupportedStyles = SummaryDisplayItemViewModel.Key2StylesMap.TryGetValue(model.key, out stringList) ? stringList : new List<string>();
      this.HasStyleOption = this.SupportedStyles != null && this.SupportedStyles.Count > 0;
    }

    public SummaryDisplayItem ToItemModel()
    {
      return new SummaryDisplayItem()
      {
        key = this.Key,
        sortOrder = this.SortOrder,
        enabled = this.Enabled,
        style = this.GetStyle()
      };
    }

    private string GetStyle()
    {
      if (this.Key == "detail" && string.IsNullOrEmpty(this.Style))
        return "text";
      return string.IsNullOrEmpty(this.Style) ? (string) null : this.Style;
    }

    public SummaryDisplayItemViewModel()
    {
    }

    public static string GetStyleName(string style)
    {
      string key;
      return !SummaryDisplayItemViewModel.Style2NameMap.TryGetValue(style, out key) ? style : Utils.GetString(key);
    }

    public SummaryDisplayItemViewModel(string key, long sortOrder, string style, bool enabled)
    {
      this.Key = key;
      this.SortOrder = sortOrder;
      this.Style = style;
      this.Enabled = enabled;
      this.Name = this.GetNameByKey();
      this.HasStyleOption = !string.IsNullOrEmpty(style);
    }

    private string GetNameByKey()
    {
      string key;
      return !SummaryDisplayItemViewModel.Key2NameMap.TryGetValue(this.Key, out key) ? this.Key : Utils.GetString(key);
    }
  }
}
