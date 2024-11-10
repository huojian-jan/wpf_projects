// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ProjectItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  [Serializable]
  public class ProjectItemViewModel : BaseViewModel
  {
    private int _count;
    private string _color;
    private bool _dragSelected;
    private bool _dropSelected;
    private bool _hover;
    private Geometry _icon;
    private Geometry _infoIcon;
    private string _info;
    private Brush _infoIconBrush = (Brush) new SolidColorBrush();
    private bool _isSubItem;
    private bool _isTabSelected;
    private bool _open = true;
    private bool _selected;
    private bool _isSplitLine;
    private bool _isGroupItem;
    private bool _isPtfAll;
    private bool _isPtfItem;
    private bool _isEmptySubItem;
    private bool _isEmptyProject;
    private string _title = string.Empty;
    public ProjectListType ListType;
    public string _emojiText;

    public string Id { get; set; }

    public bool CanDrag { get; protected set; }

    public bool CanDrop { get; protected set; }

    public bool IsAddProject { get; set; }

    public long SortOrder { get; set; }

    public bool IsNormalItem => !this.IsEmptyProject && !this.IsSplitLine && !this.IsEmptySubItem;

    public bool Expired { get; set; }

    public string ViewMode { get; set; }

    public bool ShowCount { get; protected set; } = true;

    public string IconText { get; set; }

    public string NotifyMessage { get; set; }

    public string TeamId { get; set; }

    public bool IsProjectPermit { get; set; } = true;

    public bool IsEmptyProject
    {
      get => this._isEmptyProject;
      set
      {
        this._isEmptyProject = value;
        this.OnPropertyChanged(nameof (IsEmptyProject));
        this.OnPropertyChanged("ItemVisible");
      }
    }

    public Visibility ItemVisible
    {
      get
      {
        if (this.DragSelected)
          return Visibility.Hidden;
        return !this.IsNormalItem ? Visibility.Collapsed : Visibility.Visible;
      }
    }

    public bool ShowIcon
    {
      get
      {
        if (this.IsPtfAll)
          return false;
        return string.IsNullOrEmpty(this._emojiText) || !this._title.StartsWith(this._emojiText);
      }
    }

    public bool ShowEmoji
    {
      get
      {
        return !this.IsPtfAll && !string.IsNullOrEmpty(this._emojiText) && this._title.StartsWith(this._emojiText);
      }
    }

    public bool IsEmptySubItem
    {
      get => this._isEmptySubItem;
      set
      {
        this._isEmptySubItem = value;
        this.OnPropertyChanged(nameof (IsEmptySubItem));
        this.OnPropertyChanged("ItemVisible");
      }
    }

    public bool IsPtfItem
    {
      get => this._isPtfItem;
      set
      {
        this._isPtfItem = value;
        this.OnPropertyChanged(nameof (IsPtfItem));
      }
    }

    public bool IsGroupItem
    {
      get => this._isGroupItem;
      set
      {
        this._isGroupItem = value;
        this.OnPropertyChanged(nameof (IsGroupItem));
      }
    }

    public bool IsPtfAll
    {
      get => this._isPtfAll;
      set
      {
        this._isPtfAll = value;
        this.OnPropertyChanged(nameof (IsPtfAll));
      }
    }

    public string Color
    {
      get => !string.IsNullOrEmpty(this._color) ? this._color : "Transparent";
      set
      {
        this._color = value;
        this.OnPropertyChanged(nameof (Color));
      }
    }

    public bool IsSplitLine
    {
      get => this._isSplitLine;
      set
      {
        this._isSplitLine = value;
        this.OnPropertyChanged(nameof (IsSplitLine));
        this.OnPropertyChanged("ItemVisible");
      }
    }

    public bool InSubSection { get; set; }

    public bool IsSubItem
    {
      get => this._isSubItem;
      set
      {
        this._isSubItem = value;
        this.OnPropertyChanged("Level");
      }
    }

    public int Level => (this.InSubSection ? 1 : 0) + (this.IsSubItem ? 1 : 0);

    public bool Open
    {
      get => this._open;
      set
      {
        this._open = value;
        this.OnPropertyChanged(nameof (Open));
      }
    }

    public List<ProjectItemViewModel> Children { get; set; } = new List<ProjectItemViewModel>();

    public string EmojiText => !this.ShowEmoji ? string.Empty : this._emojiText;

    public string TitleText
    {
      get
      {
        return !this.ShowEmoji ? this._title.Trim() : this._title.Substring(this._emojiText.Length).Trim();
      }
    }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged("EmojiText");
        this.OnPropertyChanged("TitleText");
      }
    }

    public Geometry Icon
    {
      get => this._icon;
      set
      {
        this._icon = value;
        this.OnPropertyChanged(nameof (Icon));
      }
    }

    public string Info
    {
      get => this._info;
      set
      {
        this._info = value;
        this.OnPropertyChanged("_info");
      }
    }

    public Geometry InfoIcon
    {
      get => this._infoIcon;
      set
      {
        this._infoIcon = value;
        this.OnPropertyChanged(nameof (InfoIcon));
      }
    }

    public Brush InfoIconBrush
    {
      get => this._infoIconBrush;
      set
      {
        this._infoIconBrush = value;
        this.OnPropertyChanged(nameof (InfoIconBrush));
      }
    }

    public int Count
    {
      get => this._infoIcon != null ? 0 : this._count;
      set
      {
        if (this._count == value)
          return;
        this._count = value;
        this.OnPropertyChanged("ShowCount");
        this.OnPropertyChanged(nameof (Count));
      }
    }

    public bool Hover
    {
      get => this._hover;
      set
      {
        this._hover = value;
        this.OnPropertyChanged("EmptySubItemVisibility");
      }
    }

    public Visibility EmptySubItemVisibility
    {
      get => !this.Hover ? Visibility.Collapsed : Visibility.Visible;
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public bool DragSelected
    {
      get => this._dragSelected;
      set
      {
        this._dragSelected = value;
        this.OnPropertyChanged(nameof (DragSelected));
      }
    }

    public bool DropSelected
    {
      get => this._dropSelected;
      set
      {
        this._dropSelected = value;
        this.OnPropertyChanged(nameof (DropSelected));
      }
    }

    public bool IsTabSelected
    {
      get => this._isTabSelected;
      set
      {
        this._isTabSelected = value;
        this.OnPropertyChanged(nameof (IsTabSelected));
      }
    }

    public virtual string GetSaveIdentity() => string.Empty;

    public virtual async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      return (IEnumerable<ContextAction>) null;
    }

    public virtual void LoadCount() => this.Count = 0;

    public virtual ProjectIdentity GetIdentity() => (ProjectIdentity) null;

    public virtual PtfType GetPtfType() => PtfType.Null;

    public static ProjectItemViewModel BuildProject(ProjectIdentity identity)
    {
      if (!(identity is NormalProjectIdentity))
      {
        if (identity is SmartProjectIdentity smartProjectIdentity)
          return (ProjectItemViewModel) SmartProjectViewModel.BuildModel(SpecialListUtils.GetSmartTypeById(smartProjectIdentity.Id));
        if (!(identity is GroupProjectIdentity))
        {
          if (!(identity is FilterProjectIdentity))
          {
            if (!(identity is TagProjectIdentity))
            {
              if (identity is DateProjectIdentity dateProjectIdentity)
                return (ProjectItemViewModel) new DateProjectViewModel(dateProjectIdentity.DateStamp);
              if (identity is SubscribeCalendarProjectIdentity calendarProjectIdentity1)
                return (ProjectItemViewModel) new SubscribeCalendarProjectViewModel(calendarProjectIdentity1.Profile);
              if (identity is BindAccountCalendarProjectIdentity calendarProjectIdentity2)
                return (ProjectItemViewModel) new BindCalendarAccountProjectViewModel(calendarProjectIdentity2.Account);
            }
            else
            {
              TagModel tag = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == identity.Id.ToLower()));
              if (tag != null)
                return (ProjectItemViewModel) new TagProjectViewModel(tag);
            }
          }
          else
          {
            FilterModel filter = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == identity.Id));
            if (filter != null)
              return (ProjectItemViewModel) new FilterProjectViewModel(filter);
          }
        }
        else
        {
          ProjectGroupModel itemGroup = CacheManager.GetProjectGroups().FirstOrDefault<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (p => p.id == identity.QueryId));
          if (itemGroup != null)
            return (ProjectItemViewModel) new ProjectGroupViewModel(itemGroup);
        }
      }
      else
      {
        ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == identity.Id));
        if (project != null)
          return (ProjectItemViewModel) new NormalProjectViewModel(project);
      }
      return (ProjectItemViewModel) new InboxProjectViewModel((SmartProject) new InboxProject());
    }

    public static ProjectItemViewModel BuildProject(string saveId)
    {
      if (!string.IsNullOrEmpty(saveId) && saveId.Contains(":"))
      {
        string[] strArray = saveId.Split(':');
        if (strArray.Length == 2)
        {
          string str = strArray[0];
          string identity = strArray[1];
          if (str != null)
          {
            switch (str.Length)
            {
              case 3:
                if (str == "tag")
                {
                  TagModel tag = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == identity));
                  if (tag != null)
                    return (ProjectItemViewModel) new TagProjectViewModel(tag);
                  break;
                }
                break;
              case 5:
                switch (str[0])
                {
                  case 'g':
                    if (str == "group")
                    {
                      ProjectGroupModel itemGroup = CacheManager.GetProjectGroups().FirstOrDefault<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (p => p.id == identity));
                      if (itemGroup != null)
                        return (ProjectItemViewModel) new ProjectGroupViewModel(itemGroup);
                      break;
                    }
                    break;
                  case 's':
                    if (str == "smart")
                      return (ProjectItemViewModel) SmartProjectViewModel.BuildModel(SpecialListUtils.GetSmartTypeById(identity));
                    break;
                }
                break;
              case 6:
                if (str == "filter")
                {
                  FilterModel filter = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == identity));
                  if (filter != null)
                    return (ProjectItemViewModel) new FilterProjectViewModel(filter);
                  break;
                }
                break;
              case 7:
                if (str == "project")
                {
                  ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == identity));
                  if (project != null)
                    return (ProjectItemViewModel) new NormalProjectViewModel(project);
                  break;
                }
                break;
              case 12:
                if (str == "bind_account")
                {
                  BindCalendarAccountModel account1 = CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (account => account.Id == identity));
                  if (account1 != null)
                    return (ProjectItemViewModel) new BindCalendarAccountProjectViewModel(account1);
                  break;
                }
                break;
              case 18:
                if (str == "subscribe_calendar")
                {
                  CalendarSubscribeProfileModel profile = CacheManager.GetSubscribeCalendars().FirstOrDefault<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (c => c.Id == identity));
                  if (profile != null)
                    return (ProjectItemViewModel) new SubscribeCalendarProjectViewModel(profile);
                  break;
                }
                break;
            }
          }
        }
      }
      return (ProjectItemViewModel) new InboxProjectViewModel((SmartProject) new InboxProject());
    }
  }
}
