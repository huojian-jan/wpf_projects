// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ShortcutModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ticktick_WPF.Views;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Models
{
  public class ShortcutModel
  {
    public static Dictionary<SearchOperationType, string> GoShortcutDict = new Dictionary<SearchOperationType, string>()
    {
      {
        SearchOperationType.GoSettings,
        "S"
      },
      {
        SearchOperationType.GoAll,
        "A"
      },
      {
        SearchOperationType.GoToday,
        "T"
      },
      {
        SearchOperationType.GoTomorrow,
        "R"
      },
      {
        SearchOperationType.GoNext7Day,
        "N"
      },
      {
        SearchOperationType.GoAssignToMe,
        "M"
      },
      {
        SearchOperationType.GoInbox,
        "I"
      },
      {
        SearchOperationType.GoCompleted,
        "C"
      },
      {
        SearchOperationType.GoAbandoned,
        "W"
      },
      {
        SearchOperationType.GoSummary,
        "B"
      },
      {
        SearchOperationType.GoTrash,
        "G"
      }
    };
    [JsonIgnore]
    public bool CanNotify;
    private string _clearDate = "Ctrl+Shift+D";
    private string _setToday = "";
    private string _setTomorrow = "";
    private string _setNextWeek = "";
    private string _setNoPriority = "";
    private string _setLowPriority = "";
    private string _setMediumPriority = "";
    private string _setHighPriority = "";
    private string _print = "Ctrl+P";
    private string _printDetail = "";
    private string _jumpAll = "";
    private string _jumpToday = "Alt+Shift+T";
    private string _jumpTomorrow = "";
    private string _jumpWeek = "Alt+Shift+N";
    private string _jumpAssign = "";
    private string _jumpInbox = "Alt+Shift+I";
    private string _jumpComplete = "";
    private string _jumpTrash = "";
    private string _jumpSummary = "";
    private string _jumpAbandon = "";
    private string _sync = "Ctrl+S";
    private string _input = "Ctrl+N";
    private string _setDate = "Ctrl+D";
    private string _complete = "Ctrl+M";
    private string _deleteTask = "Alt+Delete";
    private string _expandAllTask = "";
    private string _expandAllSection = "";
    private string _omListView = "";
    private string _omKanbanView = "";
    private string _omTimelineView = "";
    private string _jumpCalendar = "";
    private string _jumpHabit = "";
    private string _search = "Ctrl+F";
    private string _pinTask = "";
    private string _openSticky = "";
    private string _stickyColor = "Ctrl+Shift+C";
    private string _pinSticky = "F12";
    private string _openSetting = "";
    private string _stickyAlignTop = "Alt+Up";
    private string _stickyAlignLeft = "Alt+Left";
    private string _stickyAlignRight = "Alt+Right";
    private string _stickyCollapse = "Ctrl+E";
    private string _continueAndPause = "";
    private string _startAndDrop = "";
    private string _tab01;
    private string _tab02;
    private string _tab03;
    private string _tab04;
    private string _tab05;
    private string _tab06;
    private string _tab07;
    private string _tab08;
    private string _tab09;

    [JsonIgnore]
    public string Tab01
    {
      get => this._tab01;
      set
      {
        this._tab01 = value;
        this.UpdateShortcut(nameof (Tab01), value);
      }
    }

    [JsonIgnore]
    public string Tab02
    {
      get => this._tab02;
      set
      {
        this._tab02 = value;
        this.UpdateShortcut(nameof (Tab02), value);
      }
    }

    [JsonIgnore]
    public string Tab03
    {
      get => this._tab03;
      set
      {
        this._tab03 = value;
        this.UpdateShortcut(nameof (Tab03), value);
      }
    }

    [JsonIgnore]
    public string Tab04
    {
      get => this._tab04;
      set
      {
        this._tab04 = value;
        this.UpdateShortcut(nameof (Tab04), value);
      }
    }

    [JsonIgnore]
    public string Tab05
    {
      get => this._tab05;
      set
      {
        this._tab05 = value;
        this.UpdateShortcut(nameof (Tab05), value);
      }
    }

    [JsonIgnore]
    public string Tab06
    {
      get => this._tab06;
      set
      {
        this._tab06 = value;
        this.UpdateShortcut(nameof (Tab06), value);
      }
    }

    [JsonIgnore]
    public string Tab07
    {
      get => this._tab07;
      set
      {
        this._tab07 = value;
        this.UpdateShortcut(nameof (Tab07), value);
      }
    }

    [JsonIgnore]
    public string Tab08
    {
      get => this._tab08;
      set
      {
        this._tab08 = value;
        this.UpdateShortcut(nameof (Tab08), value);
      }
    }

    [JsonIgnore]
    public string Tab09
    {
      get => this._tab09;
      set
      {
        this._tab09 = value;
        this.UpdateShortcut(nameof (Tab09), value);
      }
    }

    public string TaskSetFolded { get; set; } = "0";

    public string TaskOperateFolded { get; set; } = "0";

    public string AppGlobalOperateFolded { get; set; } = "0";

    public string GlobalOperateFolded { get; set; } = "0";

    public string SetTimeFolded { get; set; } = "0";

    public string SetPriorityFolded { get; set; } = "0";

    public string QuickJumpFolded { get; set; } = "0";

    public string ExpandAllTask
    {
      get => this._expandAllTask;
      set
      {
        this._expandAllTask = value;
        this.UpdateShortcut(nameof (ExpandAllTask), value);
      }
    }

    public string ExpandAllSection
    {
      get => this._expandAllSection;
      set
      {
        this._expandAllSection = value;
        this.UpdateShortcut(nameof (ExpandAllSection), value);
      }
    }

    public string OmListView
    {
      get => this._omListView;
      set
      {
        this._omListView = value;
        this.UpdateShortcut(nameof (OmListView), value);
      }
    }

    public string OmKanbanView
    {
      get => this._omKanbanView;
      set
      {
        this._omKanbanView = value;
        this.UpdateShortcut(nameof (OmKanbanView), value);
      }
    }

    public string OmTimelineView
    {
      get => this._omTimelineView;
      set
      {
        this._omTimelineView = value;
        this.UpdateShortcut(nameof (OmTimelineView), value);
      }
    }

    public string SyncTask
    {
      get => this._sync;
      set
      {
        this._sync = value;
        this.UpdateShortcut(nameof (SyncTask), value);
      }
    }

    public string OpenOperation { get; set; } = "Ctrl+K";

    public string SearchTask
    {
      get => this._search;
      set
      {
        this._search = value;
        this.UpdateShortcut(nameof (SearchTask), value);
      }
    }

    public string AddTask
    {
      get => this._input;
      set
      {
        this._input = value;
        this.UpdateShortcut(nameof (AddTask), value);
      }
    }

    public string CompleteTask
    {
      get => this._complete;
      set
      {
        this._complete = value;
        this.UpdateShortcut(nameof (CompleteTask), value);
      }
    }

    public string DeleteTask
    {
      get => this._deleteTask;
      set
      {
        this._deleteTask = value;
        this.UpdateShortcut(nameof (DeleteTask), value);
      }
    }

    public string SetDate
    {
      get => this._setDate;
      set
      {
        this._setDate = value;
        this.UpdateShortcut(nameof (SetDate), value);
      }
    }

    public string ClearDate
    {
      get => this._clearDate;
      set
      {
        this._clearDate = value;
        this.UpdateShortcut(nameof (ClearDate), value);
      }
    }

    public string SetToday
    {
      get => this._setToday;
      set
      {
        this._setToday = value;
        this.UpdateShortcut(nameof (SetToday), value);
      }
    }

    public string SetTomorrow
    {
      get => this._setTomorrow;
      set
      {
        this._setTomorrow = value;
        this.UpdateShortcut(nameof (SetTomorrow), value);
      }
    }

    public string SetNextWeek
    {
      get => this._setNextWeek;
      set
      {
        this._setNextWeek = value;
        this.UpdateShortcut(nameof (SetNextWeek), value);
      }
    }

    public string SetNoPriority
    {
      get => this._setNoPriority;
      set
      {
        this._setNoPriority = value;
        this.UpdateShortcut(nameof (SetNoPriority), value);
      }
    }

    public string SetLowPriority
    {
      get => this._setLowPriority;
      set
      {
        this._setLowPriority = value;
        this.UpdateShortcut(nameof (SetLowPriority), value);
      }
    }

    public string SetMediumPriority
    {
      get => this._setMediumPriority;
      set
      {
        this._setMediumPriority = value;
        this.UpdateShortcut(nameof (SetMediumPriority), value);
      }
    }

    public string SetHighPriority
    {
      get => this._setHighPriority;
      set
      {
        this._setHighPriority = value;
        this.UpdateShortcut(nameof (SetHighPriority), value);
      }
    }

    public string Print
    {
      get => this._print;
      set
      {
        this._print = value;
        this.UpdateShortcut(nameof (Print), value);
      }
    }

    public string PrintDetail
    {
      get => this._printDetail;
      set
      {
        this._printDetail = value;
        this.UpdateShortcut(nameof (PrintDetail), value);
      }
    }

    public string JumpAll
    {
      get => this._jumpAll;
      set
      {
        this._jumpAll = value;
        this.UpdateShortcut(nameof (JumpAll), value);
      }
    }

    public string JumpToday
    {
      get => this._jumpToday;
      set
      {
        this._jumpToday = value;
        this.UpdateShortcut(nameof (JumpToday), value);
      }
    }

    public string JumpTomorrow
    {
      get => this._jumpTomorrow;
      set
      {
        this._jumpTomorrow = value;
        this.UpdateShortcut(nameof (JumpTomorrow), value);
      }
    }

    public string JumpWeek
    {
      get => this._jumpWeek;
      set
      {
        this._jumpWeek = value;
        this.UpdateShortcut(nameof (JumpWeek), value);
      }
    }

    public string JumpCalendar
    {
      get => this._jumpCalendar;
      set
      {
        this._jumpCalendar = value;
        this.UpdateShortcut(nameof (JumpCalendar), value);
      }
    }

    public string JumpAssign
    {
      get => this._jumpAssign;
      set
      {
        this._jumpAssign = value;
        this.UpdateShortcut(nameof (JumpAssign), value);
      }
    }

    public string JumpInbox
    {
      get => this._jumpInbox;
      set
      {
        this._jumpInbox = value;
        this.UpdateShortcut(nameof (JumpInbox), value);
      }
    }

    public string JumpComplete
    {
      get => this._jumpComplete;
      set
      {
        this._jumpComplete = value;
        this.UpdateShortcut(nameof (JumpComplete), value);
      }
    }

    public string JumpSummary
    {
      get => this._jumpSummary;
      set
      {
        this._jumpSummary = value;
        this.UpdateShortcut(nameof (JumpSummary), value);
      }
    }

    public string JumpTrash
    {
      get => this._jumpTrash;
      set
      {
        this._jumpTrash = value;
        this.UpdateShortcut(nameof (JumpTrash), value);
      }
    }

    public string JumpAbandon
    {
      get => this._jumpAbandon;
      set
      {
        this._jumpAbandon = value;
        this.UpdateShortcut(nameof (JumpAbandon), value);
      }
    }

    public string JumpHabit
    {
      get => this._jumpHabit;
      set
      {
        this._jumpHabit = value;
        this.UpdateShortcut(nameof (JumpHabit), value);
      }
    }

    public string PinTask
    {
      get => this._pinTask;
      set
      {
        this._pinTask = value;
        this.UpdateShortcut(nameof (PinTask), value);
      }
    }

    public string OpenSticky
    {
      get => this._openSticky;
      set
      {
        this._openSticky = value;
        this.UpdateShortcut(nameof (OpenSticky), value);
      }
    }

    public string StickyColor
    {
      get => this._stickyColor;
      set
      {
        this._stickyColor = value;
        this.UpdateShortcut(nameof (StickyColor), value);
      }
    }

    public string StickyAlignTop
    {
      get => this._stickyAlignTop;
      set
      {
        this._stickyAlignTop = value;
        this.UpdateShortcut(nameof (StickyAlignTop), value);
      }
    }

    public string StickyAlignLeft
    {
      get => this._stickyAlignLeft;
      set
      {
        this._stickyAlignLeft = value;
        this.UpdateShortcut(nameof (StickyAlignLeft), value);
      }
    }

    public string StickyAlignRight
    {
      get => this._stickyAlignRight;
      set
      {
        this._stickyAlignRight = value;
        this.UpdateShortcut(nameof (StickyAlignRight), value);
      }
    }

    public string StickyCollapse
    {
      get => this._stickyCollapse;
      set
      {
        this._stickyCollapse = value;
        this.UpdateShortcut(nameof (StickyCollapse), value);
      }
    }

    public string PinSticky
    {
      get => this._pinSticky;
      set
      {
        this._pinSticky = value;
        this.UpdateShortcut(nameof (PinSticky), value);
      }
    }

    public string ContinueAndPause
    {
      get => this._continueAndPause;
      set
      {
        this._continueAndPause = value;
        this.UpdateShortcut(nameof (ContinueAndPause), value);
      }
    }

    public string StartAndDrop
    {
      get => this._startAndDrop;
      set
      {
        this._startAndDrop = value;
        this.UpdateShortcut(nameof (StartAndDrop), value);
      }
    }

    public string OpenSetting
    {
      get => this._openSetting;
      set
      {
        this._openSetting = value;
        this.UpdateShortcut(nameof (OpenSetting), value);
      }
    }

    private void UpdateShortcut(string property, string value)
    {
      if (!this.CanNotify)
        return;
      if (!property.StartsWith("Tab0"))
        KeyBindingManager.ResetTabShortCut();
      KeyBindingManager.ChangeShortCut(property, value);
    }

    [Obsolete]
    public static ShortcutModel GetRegeditModel()
    {
      ShortcutModel regeditModel = new ShortcutModel();
      RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software")?.OpenSubKey("Dida")?.OpenSubKey("Shortcuts");
      if (registryKey != null)
      {
        foreach (PropertyInfo propertyInfo in ((IEnumerable<PropertyInfo>) regeditModel.GetType().GetProperties()).ToList<PropertyInfo>())
        {
          object obj = registryKey.GetValue(propertyInfo.Name);
          if (obj != null)
            propertyInfo.SetValue((object) regeditModel, obj);
        }
      }
      return regeditModel;
    }

    public bool ExistKey(string hotkey)
    {
      return ((IEnumerable<PropertyInfo>) this.GetType().GetProperties()).Any<PropertyInfo>((Func<PropertyInfo, bool>) (property => !property.Name.StartsWith("Tab0") && (string) property.GetValue((object) this) == hotkey));
    }

    public void ClearShortcut(string name) => this.SetPropertyValue(name, "");

    public List<string> GetAllPropertyNames()
    {
      return ((IEnumerable<PropertyInfo>) this.GetType().GetProperties()).Select<PropertyInfo, string>((Func<PropertyInfo, string>) (i => i.Name)).ToList<string>();
    }

    public string GetPropertyValue(string key)
    {
      return (string) this.GetType().GetProperty(key)?.GetValue((object) this);
    }

    public void SetPropertyValue(string key, string value)
    {
      this.GetType().GetProperty(key)?.SetValue((object) this, (object) value);
    }

    public void Reset() => this.CopyProperties(new ShortcutModel());

    public void CopyProperties(ShortcutModel shortCut)
    {
      if (shortCut == null)
        return;
      PropertyInfo[] properties = this.GetType().GetProperties();
      foreach (PropertyInfo propertyInfo in properties)
      {
        if (!propertyInfo.Name.Contains("Tab0") && !propertyInfo.Name.Contains("Folded"))
          propertyInfo.SetValue((object) this, (object) "");
      }
      foreach (PropertyInfo propertyInfo in properties)
      {
        if (!propertyInfo.Name.Contains("Tab0") && !propertyInfo.Name.Contains("Folded"))
        {
          object obj = propertyInfo.GetValue((object) shortCut);
          if (propertyInfo.GetValue((object) this) != obj)
            propertyInfo.SetValue((object) this, obj);
        }
      }
    }
  }
}
