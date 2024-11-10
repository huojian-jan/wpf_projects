// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.ShortcutViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using NHotkey;
using NHotkey.Wpf;
using System;
using System.Collections.Generic;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MarkDown;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class ShortcutViewModel : BaseViewModel
  {
    private static readonly string SetShortcut = Utils.GetString(nameof (SetShortcut));
    private string _originHotKey;
    private string _newHotKey;
    private string _shortCut;
    private string _toolTip;

    public ShortcutViewModel()
    {
    }

    private ShortcutViewModel(string name, string shortcut, bool editable = true)
    {
      this.Name = name;
      this.Title = Utils.GetString(name);
      this.Shortcut = shortcut;
      this.Editable = editable;
      this.OriginHotKey = string.IsNullOrEmpty(this.DisplayShortcut) ? ShortcutViewModel.SetShortcut : this.DisplayShortcut;
    }

    private ShortcutViewModel(
      string name,
      string title,
      string shortcut,
      bool editable,
      bool isGlobal)
    {
      this.Name = name;
      this.Title = title;
      this.Shortcut = shortcut;
      this.Editable = editable;
      this.OriginHotKey = string.IsNullOrEmpty(this.DisplayShortcut) ? ShortcutViewModel.SetShortcut : this.DisplayShortcut;
      this.IsGlobal = isGlobal;
      if (!isGlobal)
        return;
      this.CheckShortKey();
    }

    public string OriginHotKey
    {
      get => this._originHotKey;
      set
      {
        this._originHotKey = value;
        this.OnPropertyChanged(nameof (OriginHotKey));
      }
    }

    public string NewHotKey
    {
      get => this._newHotKey;
      set
      {
        this._newHotKey = value;
        this.OnPropertyChanged(nameof (NewHotKey));
      }
    }

    public string Name { get; set; }

    public string Title { get; set; }

    public bool IsSection { get; set; }

    public string Description { get; set; }

    public string DisplayShortcut { get; set; }

    public CornerRadius ItemCornerRadius { get; set; } = new CornerRadius(0.0);

    public Thickness BorderMargin { get; set; } = new Thickness(0.0);

    public Thickness SectionMargin { get; set; } = new Thickness(18.0, 32.0, 4.0, 0.0);

    public bool EnableOneWord { get; set; }

    public string Shortcut
    {
      get => this._shortCut;
      set
      {
        this._shortCut = value;
        if (string.IsNullOrEmpty(value))
          return;
        int startIndex = value.IndexOf('D');
        if (startIndex != -1)
        {
          int? length = this.Shortcut?.Length;
          int num = startIndex + 1;
          if (length.GetValueOrDefault() > num & length.HasValue && '0' <= this.Shortcut[startIndex + 1] && this.Shortcut[startIndex + 1] <= '9')
          {
            this.DisplayShortcut = HotkeyModel.HandleUpDownText(value.Remove(startIndex, 1));
            return;
          }
        }
        this.DisplayShortcut = HotkeyModel.HandleUpDownText(value);
      }
    }

    public string ExtraShortCut { get; set; }

    public bool Editable { get; set; }

    public bool IsGlobal { get; set; }

    public string ToolTip
    {
      get => this._toolTip;
      set
      {
        this._toolTip = value;
        this.IsKeyValid = string.IsNullOrEmpty(this._toolTip);
        this.OnPropertyChanged(nameof (ToolTip));
        this.OnPropertyChanged("IsKeyValid");
      }
    }

    public bool IsKeyValid { get; set; } = true;

    public static IEnumerable<ShortcutViewModel> BuildShortcuts()
    {
      ShortcutModel shortCutModel = LocalSettings.Settings.ShortCutModel;
      ShortcutViewModel shortcutViewModel = new ShortcutViewModel("SwitchTabShortcut", "Ctrl+1,2...")
      {
        Editable = false,
        ItemCornerRadius = new CornerRadius(8.0, 8.0, 0.0, 0.0),
        BorderMargin = new Thickness(0.0, -4.0, 0.0, 0.0)
      };
      bool flag = !KeyBindingManager.HasCtrlNumKeyBinding();
      List<ShortcutViewModel> shortcutViewModelList = new List<ShortcutViewModel>()
      {
        new ShortcutViewModel()
        {
          IsSection = true,
          Title = Utils.GetString("General"),
          SectionMargin = new Thickness(18.0, 18.0, 4.0, 0.0)
        },
        new ShortcutViewModel("SyncTask", shortCutModel.GetPropertyValue("SyncTask"))
        {
          ItemCornerRadius = new CornerRadius(8.0, 8.0, 0.0, 0.0),
          BorderMargin = new Thickness(0.0, -4.0, 0.0, 0.0)
        },
        new ShortcutViewModel("Cancel", "Esc", false),
        new ShortcutViewModel("Undo", "Ctrl+Z", false),
        new ShortcutViewModel("TextRedo", "Ctrl+Shift+Z", false),
        new ShortcutViewModel("Print", shortCutModel.GetPropertyValue("Print")),
        new ShortcutViewModel("PrintDetail", shortCutModel.GetPropertyValue("PrintDetail")),
        new ShortcutViewModel("OpenOperationMenu", "Ctrl+K", false),
        new ShortcutViewModel("Shortcut", "?", false)
        {
          ItemCornerRadius = new CornerRadius(0.0, 0.0, 8.0, 8.0),
          BorderMargin = new Thickness(0.0, 0.0, 0.0, -4.0)
        },
        new ShortcutViewModel()
        {
          IsSection = true,
          Title = Utils.GetString("GlobalOperate"),
          Description = Utils.GetString("GlobalOperateMessage")
        },
        new ShortcutViewModel("ShortcutAddTask", Utils.GetString("GlobalAddTask"), LocalSettings.Settings.ShortcutAddTask, true, true)
        {
          ItemCornerRadius = new CornerRadius(8.0, 8.0, 0.0, 0.0),
          BorderMargin = new Thickness(0.0, -4.0, 0.0, 0.0)
        },
        new ShortcutViewModel("ShortcutOpenOrClose", Utils.GetString("ShowOrHideApp"), LocalSettings.Settings.ShortcutOpenOrClose, true, true),
        new ShortcutViewModel("PomoShortcut", Utils.GetString("OpenOrCloseFocusWindow"), LocalSettings.Settings.PomoShortcut, LocalSettings.Settings.EnableFocus, true),
        new ShortcutViewModel("ShortcutPin", Utils.GetString("PinOrUnPin"), LocalSettings.Settings.ShortcutPin, true, true),
        new ShortcutViewModel("LockShortcut", Utils.GetString("LockUnlockTickTick"), LocalSettings.Settings.LockShortcut, true, true),
        new ShortcutViewModel("CreateStickyShortcut", Utils.GetString("NewSticky"), LocalSettings.Settings.CreateStickyShortCut, true, true),
        new ShortcutViewModel("ShowHideStickyShortcut", Utils.GetString("ShowHideSticky"), LocalSettings.Settings.ShowHideStickyShortCut, true, true)
        {
          ItemCornerRadius = new CornerRadius(0.0, 0.0, 8.0, 8.0),
          BorderMargin = new Thickness(0.0, 0.0, 0.0, -4.0)
        },
        new ShortcutViewModel()
        {
          IsSection = true,
          Title = Utils.GetString("Task")
        },
        new ShortcutViewModel("AddTask", shortCutModel.GetPropertyValue("AddTask"))
        {
          ItemCornerRadius = new CornerRadius(8.0, 8.0, 0.0, 0.0),
          BorderMargin = new Thickness(0.0, -4.0, 0.0, 0.0)
        },
        new ShortcutViewModel("AddTaskUnder", "Enter", false),
        new ShortcutViewModel("AddSubTask", "Shift+Enter", false)
        {
          Title = Utils.GetString("AddSubTask").UpCaseFirst()
        },
        new ShortcutViewModel("ExpandAllSection", Utils.GetString("ToggleAllGroups"), shortCutModel.GetPropertyValue("ExpandAllSection"), true, false),
        new ShortcutViewModel("ExpandAllTask", Utils.GetString("ExpandOrCollapseSubtasks"), shortCutModel.GetPropertyValue("ExpandAllTask"), true, false)
        {
          ItemCornerRadius = new CornerRadius(0.0, 0.0, 8.0, 8.0)
        },
        new ShortcutViewModel()
        {
          IsSection = true,
          Title = Utils.GetString("QuickSetTask"),
          Description = Utils.GetString("QuickSetTaskMessage")
        },
        new ShortcutViewModel("QuickSetDate", "*", false)
        {
          ItemCornerRadius = new CornerRadius(8.0, 8.0, 0.0, 0.0)
        },
        new ShortcutViewModel("QuickSetPriority", "!", false),
        new ShortcutViewModel("QuickSetTag", "#", false),
        new ShortcutViewModel("QuickSetProject", "~/^", false),
        new ShortcutViewModel("QuickSetAssign", "@", false)
        {
          ItemCornerRadius = new CornerRadius(0.0, 0.0, 8.0, 8.0),
          BorderMargin = new Thickness(0.0, 0.0, 0.0, -4.0)
        },
        new ShortcutViewModel()
        {
          IsSection = true,
          Title = Utils.GetString("EditTask"),
          Description = Utils.GetString("EditTaskMessage")
        },
        new ShortcutViewModel("CompleteTask", shortCutModel.GetPropertyValue("CompleteTask"))
        {
          ItemCornerRadius = new CornerRadius(8.0, 8.0, 0.0, 0.0),
          BorderMargin = new Thickness(0.0, -4.0, 0.0, 0.0)
        },
        new ShortcutViewModel("PinTask", shortCutModel.GetPropertyValue("PinTask")),
        new ShortcutViewModel("DeleteTask", shortCutModel.GetPropertyValue("DeleteTask")),
        new ShortcutViewModel("SetDate", shortCutModel.GetPropertyValue("SetDate")),
        new ShortcutViewModel("ClearDate", shortCutModel.GetPropertyValue("ClearDate")),
        new ShortcutViewModel("SetToday", shortCutModel.GetPropertyValue("SetToday")),
        new ShortcutViewModel("SetTomorrow", shortCutModel.GetPropertyValue("SetTomorrow")),
        new ShortcutViewModel("SetNextWeek", shortCutModel.GetPropertyValue("SetNextWeek")),
        new ShortcutViewModel("SetNoPriority", shortCutModel.GetPropertyValue("SetNoPriority")),
        new ShortcutViewModel("SetLowPriority", shortCutModel.GetPropertyValue("SetLowPriority")),
        new ShortcutViewModel("SetMediumPriority", shortCutModel.GetPropertyValue("SetMediumPriority")),
        new ShortcutViewModel("SetHighPriority", shortCutModel.GetPropertyValue("SetHighPriority")),
        new ShortcutViewModel("OpenSticky", shortCutModel.GetPropertyValue("OpenSticky"))
        {
          Title = Utils.GetString("OpenAsSticky"),
          ItemCornerRadius = new CornerRadius(0.0, 0.0, 8.0, 8.0),
          BorderMargin = new Thickness(0.0, 0.0, 0.0, -4.0)
        },
        new ShortcutViewModel()
        {
          IsSection = true,
          Title = Utils.GetString("Navigation"),
          Description = Utils.GetString("NavigationMessage")
        },
        shortcutViewModel,
        new ShortcutViewModel("SearchTask", shortCutModel.GetPropertyValue("SearchTask"))
        {
          ItemCornerRadius = flag ? new CornerRadius(0.0) : new CornerRadius(8.0, 8.0, 0.0, 0.0),
          BorderMargin = new Thickness(0.0, flag ? 0.0 : -4.0, 0.0, 0.0)
        },
        new ShortcutViewModel("OpenSetting", shortCutModel.GetPropertyValue("OpenSetting"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("Settings")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "S")
        },
        new ShortcutViewModel("JumpAll", shortCutModel.GetPropertyValue("JumpAll"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("All")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "A")
        },
        new ShortcutViewModel("JumpToday", shortCutModel.GetPropertyValue("JumpToday"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("Today")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "T")
        },
        new ShortcutViewModel("JumpTomorrow", shortCutModel.GetPropertyValue("JumpTomorrow"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("Tomorrow")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "R")
        },
        new ShortcutViewModel("JumpWeek", shortCutModel.GetPropertyValue("JumpWeek"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("Next7Day")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "N")
        },
        new ShortcutViewModel("JumpAssign", shortCutModel.GetPropertyValue("JumpAssign"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("AssignToMe")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "M")
        },
        new ShortcutViewModel("JumpInbox", shortCutModel.GetPropertyValue("JumpInbox"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("Inbox")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "I")
        },
        new ShortcutViewModel("JumpComplete", shortCutModel.GetPropertyValue("JumpComplete"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("Completed")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "C")
        },
        new ShortcutViewModel("JumpAbandon", shortCutModel.GetPropertyValue("JumpAbandon"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("Abandoned")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "W")
        },
        new ShortcutViewModel("JumpTrash", shortCutModel.GetPropertyValue("JumpTrash"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("Trash")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "G")
        },
        new ShortcutViewModel("JumpSummary", shortCutModel.GetPropertyValue("JumpSummary"))
        {
          Title = string.Format(Utils.GetString("GoList"), (object) Utils.GetString("Summary")),
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "G", (object) "B"),
          ItemCornerRadius = new CornerRadius(0.0, 0.0, 8.0, 8.0)
        },
        new ShortcutViewModel()
        {
          IsSection = true,
          Title = Utils.GetString("SwitchView"),
          Description = Utils.GetString("SwitchViewMessage")
        },
        new ShortcutViewModel("OmListView", shortCutModel.GetPropertyValue("OmListView"))
        {
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "V", (object) "L"),
          ItemCornerRadius = new CornerRadius(8.0, 8.0, 0.0, 0.0)
        },
        new ShortcutViewModel("OmKanbanView", shortCutModel.GetPropertyValue("OmKanbanView"))
        {
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "V", (object) "K")
        },
        new ShortcutViewModel("OmTimelineView", shortCutModel.GetPropertyValue("OmTimelineView"))
        {
          ExtraShortCut = string.Format(Utils.GetString("AThenB"), (object) "V", (object) "T")
        },
        new ShortcutViewModel("DayView", "D / 1", false),
        new ShortcutViewModel("WeekView", "W / 2", false),
        new ShortcutViewModel("MonthView", "M / 3", false)
        {
          ItemCornerRadius = new CornerRadius(0.0, 0.0, 8.0, 8.0)
        },
        new ShortcutViewModel()
        {
          IsSection = true,
          Title = Utils.GetString("Focus")
        },
        new ShortcutViewModel("ContinueAndPause", shortCutModel.GetPropertyValue("ContinueAndPause"))
        {
          ItemCornerRadius = new CornerRadius(8.0, 8.0, 0.0, 0.0),
          IsGlobal = true
        },
        new ShortcutViewModel("StartAndDrop", shortCutModel.GetPropertyValue("StartAndDrop"))
        {
          ItemCornerRadius = new CornerRadius(0.0, 0.0, 8.0, 8.0),
          IsGlobal = true
        },
        new ShortcutViewModel()
        {
          IsSection = true,
          Title = Utils.GetString("StickyNote")
        },
        new ShortcutViewModel("StickyColor", shortCutModel.GetPropertyValue("StickyColor"))
        {
          Title = Utils.GetString("Color"),
          ItemCornerRadius = new CornerRadius(8.0, 8.0, 0.0, 0.0)
        },
        new ShortcutViewModel("PinSticky", shortCutModel.GetPropertyValue("PinSticky"))
        {
          Title = Utils.GetString("Pin")
        },
        new ShortcutViewModel("StickyCollapse", shortCutModel.GetPropertyValue("StickyCollapse"))
        {
          Title = Utils.GetString("CollapseOrExpandAll")
        },
        new ShortcutViewModel("StickyAlignTop", shortCutModel.GetPropertyValue("StickyAlignTop"))
        {
          Title = Utils.GetString("TopAlignment")
        },
        new ShortcutViewModel("StickyAlignLeft", shortCutModel.GetPropertyValue("StickyAlignLeft"))
        {
          Title = Utils.GetString("LeftAlignment")
        },
        new ShortcutViewModel("StickyAlignRight", shortCutModel.GetPropertyValue("StickyAlignRight"))
        {
          Title = Utils.GetString("RightAlignment"),
          ItemCornerRadius = new CornerRadius(0.0, 0.0, 8.0, 8.0),
          BorderMargin = new Thickness(0.0, 0.0, 0.0, -4.0)
        },
        new ShortcutViewModel() { IsSection = true }
      };
      if (!flag)
        shortcutViewModelList.Remove(shortcutViewModel);
      return (IEnumerable<ShortcutViewModel>) shortcutViewModelList;
    }

    public void CheckShortKey()
    {
      try
      {
        HotkeyManager.Current.Remove(this.Name);
        ShortcutViewModel.CheckHotkeyAvailability(this);
        HotKeyUtils.ResignHotKey(this.Name);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(ExceptionUtils.BuildExceptionMessage(ex));
      }
    }

    private static bool CheckHotkeyAvailability(ShortcutViewModel model)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(model.Shortcut))
        {
          model.ToolTip = (string) null;
          return true;
        }
        HotKeyUtils hotKeyUtils = new HotKeyUtils(model.Shortcut.Replace(" ", ""));
        HotkeyManager.Current.AddOrReplace("HotkeyAvailabilityTest", hotKeyUtils.Key, hotKeyUtils.Modifiers, (EventHandler<HotkeyEventArgs>) ((sender, e) => { }));
        model.ToolTip = (string) null;
        return true;
      }
      catch (Exception ex)
      {
        model.ToolTip = !(ex is HotkeyAlreadyRegisteredException) ? Utils.GetString("ShortCutOccupied") : Utils.GetString("ShortCutOccupied");
      }
      finally
      {
        HotkeyManager.Current.Remove("HotkeyAvailabilityTest");
      }
      return false;
    }

    public void ClearShortCut()
    {
      if (this.IsGlobal)
      {
        string name = this.Name;
        if (name == "ContinueAndPause" || name == "StartAndDrop")
          LocalSettings.Settings.ShortCutModel.ClearShortcut(this.Name);
        else
          LocalSettings.Settings[this.Name] = (object) string.Empty;
        HotkeyManager.Current.Remove(this.Name);
      }
      else
        LocalSettings.Settings.ShortCutModel.ClearShortcut(this.Name);
    }

    public void SetShortCut(string shortcut)
    {
      this.Shortcut = shortcut;
      if (this.IsGlobal)
      {
        string name = this.Name;
        if (name == "ContinueAndPause" || name == "StartAndDrop")
          LocalSettings.Settings.ShortCutModel.SetPropertyValue(this.Name, this.Shortcut);
        else
          LocalSettings.Settings[this.Name] = (object) this.Shortcut;
        this.CheckShortKey();
      }
      else
        LocalSettings.Settings.ShortCutModel.SetPropertyValue(this.Name, this.Shortcut);
      SettingsHelper.ShortCutChanged = true;
    }
  }
}
