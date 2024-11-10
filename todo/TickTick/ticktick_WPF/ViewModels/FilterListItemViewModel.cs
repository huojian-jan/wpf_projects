// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.FilterListItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class FilterListItemViewModel : BaseViewModel
  {
    private string _dateText = string.Empty;
    private string _groupId;
    private string _icon;
    private bool _canSelect = true;
    private bool _isProjectGroup;
    private bool _isAssignee;
    private bool _showFeishu;
    private bool _isSecondLevel;
    private string _nDaysLaterValue;
    private string _nextNDaysValue;
    private string _projectId;
    private bool _selected;
    private bool _showIcon;
    private long _sortOrder;
    private string _title;
    private object _value;
    private bool _unfold;
    private bool _isTeam;
    private bool _partSelected;
    private bool _isSplit;
    private string _imgaeUrl;
    private bool _isAllItem;

    public bool IsTagParent { get; set; }

    public List<FilterListItemViewModel> Children { get; set; }

    public bool Highlighted => this._partSelected && !this._unfold || this._selected;

    public bool ShowFoldIcon => this.IsProjectGroup || this.IsTeam;

    public int NDaysValue { get; set; }

    public FilterItemDisplayType DisplayType { get; set; }

    public string Emoji { get; set; } = string.Empty;

    public bool ShowEmoji => !string.IsNullOrEmpty(this.Emoji);

    public bool IsTeam
    {
      get => this._isTeam;
      set
      {
        this._isTeam = value;
        this.OnPropertyChanged("ShowFoldIcon");
      }
    }

    public bool IsSplit
    {
      get => this._isSplit;
      set
      {
        this._isSplit = value;
        this.OnPropertyChanged(nameof (IsSplit));
      }
    }

    public bool PartSelected
    {
      get => this._partSelected;
      set
      {
        this._partSelected = value;
        this.OnPropertyChanged(nameof (PartSelected));
        this.OnPropertyChanged("Highlighted");
      }
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
        this.OnPropertyChanged("Highlighted");
      }
    }

    public object Value
    {
      get => this._value;
      set
      {
        this._value = value;
        this.OnPropertyChanged(nameof (Value));
      }
    }

    public string ImageUrl
    {
      get => this._imgaeUrl;
      set
      {
        this._imgaeUrl = value;
        this.OnPropertyChanged(nameof (ImageUrl));
        this.SetAvatar();
      }
    }

    private async void SetAvatar()
    {
      FilterListItemViewModel listItemViewModel = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(listItemViewModel._imgaeUrl);
      listItemViewModel.Avatar = avatarByUrlAsync;
      listItemViewModel.OnPropertyChanged("Avatar");
    }

    public BitmapImage Avatar { get; set; }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public bool IsAllItem
    {
      get => this._isAllItem;
      set
      {
        this._isAllItem = value;
        this.OnPropertyChanged(nameof (IsAllItem));
      }
    }

    public string Icon
    {
      get => this._icon;
      set
      {
        this._icon = value;
        this.OnPropertyChanged(nameof (Icon));
      }
    }

    public bool ShowIcon
    {
      get => this._showIcon;
      set
      {
        this._showIcon = value;
        this.OnPropertyChanged(nameof (ShowIcon));
      }
    }

    public string ProjectId
    {
      get => this._projectId;
      set => this._projectId = value;
    }

    public string GroupId
    {
      get => this._groupId;
      set => this._groupId = value;
    }

    public bool IsSecondLevel
    {
      get => this._isSecondLevel;
      set
      {
        this._isSecondLevel = value;
        this.OnPropertyChanged(nameof (IsSecondLevel));
      }
    }

    public bool IsProjectGroup
    {
      get => this._isProjectGroup;
      set
      {
        this._isProjectGroup = value;
        this.OnPropertyChanged("ShowFoldIcon");
      }
    }

    public bool IsAssignee
    {
      get => this._isAssignee;
      set
      {
        this._isAssignee = value;
        this.OnPropertyChanged(nameof (IsAssignee));
      }
    }

    public bool ShowFeishu
    {
      get => this._showFeishu;
      set
      {
        this._showFeishu = value;
        this.OnPropertyChanged(nameof (ShowFeishu));
      }
    }

    public long SortOrder
    {
      get => this._sortOrder;
      set => this._sortOrder = value;
    }

    public string DateText
    {
      get => this._dateText;
      set
      {
        this._dateText = value;
        this.OnPropertyChanged(nameof (DateText));
      }
    }

    public bool CanSelect
    {
      get => this._canSelect;
      set
      {
        this._canSelect = value;
        this.OnPropertyChanged("IsNormal");
      }
    }

    public string NextNDaysValue
    {
      get => this._nextNDaysValue;
      set
      {
        this._nextNDaysValue = value;
        this.OnPropertyChanged(nameof (NextNDaysValue));
      }
    }

    public string NDaysLaterValue
    {
      get => this._nDaysLaterValue;
      set
      {
        this._nDaysLaterValue = value;
        this.OnPropertyChanged(nameof (NDaysLaterValue));
      }
    }

    public bool Unfold
    {
      get => this._unfold;
      set
      {
        this._unfold = value;
        this.OnPropertyChanged("UnFold");
        this.OnPropertyChanged("Highlighted");
      }
    }

    public int? DaysFrom { get; set; }

    public int? DaysTo { get; set; }

    public FilterListItemViewModel()
    {
    }

    public FilterListItemViewModel(TagModel tag, ICollection<string> selectedTags)
    {
      this.Title = tag.GetDisplayName();
      this.Value = (object) tag.name;
      this.Icon = "IcTagLine";
      this.ShowIcon = true;
      this.IsProjectGroup = tag.IsParent();
      this.IsSecondLevel = tag.IsChild();
      this.GroupId = tag.parent;
      this.IsTagParent = this.IsProjectGroup;
      this.Unfold = this.IsProjectGroup && !tag.collapsed;
      this.Selected = selectedTags.Contains(tag.name) || selectedTags.Contains(tag.parent);
    }

    public static FilterListItemViewModel BuildItem(
      SelectableItemViewModel model,
      List<string> projectIds,
      List<string> groupIds)
    {
      FilterListItemViewModel listItemViewModel = new FilterListItemViewModel();
      ticktick_WPF.Views.Misc.ProjectViewModel pm = model as ticktick_WPF.Views.Misc.ProjectViewModel;
      if (pm != null)
      {
        ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == pm.Id));
        if (project != null)
        {
          string str1 = EmojiHelper.GetEmojiIcon(project.name);
          string str2;
          if (!string.IsNullOrEmpty(str1) && project.name.StartsWith(str1))
          {
            str2 = project.name.Remove(0, str1.Length);
          }
          else
          {
            str1 = string.Empty;
            str2 = project.name;
          }
          return new FilterListItemViewModel()
          {
            Emoji = str1,
            Title = str2,
            Icon = FilterListItemViewModel.GetProjectIcon(project),
            IsSecondLevel = !string.IsNullOrEmpty(project.groupId) && project.groupId != "NONE",
            SortOrder = project.sortOrder,
            Value = (object) project.id,
            GroupId = project.groupId,
            Selected = projectIds.Contains(project.id) || groupIds.Contains(project.groupId),
            ShowIcon = true
          };
        }
      }
      else
      {
        ticktick_WPF.Views.Misc.ProjectGroupViewModel pg = model as ticktick_WPF.Views.Misc.ProjectGroupViewModel;
        if (pg != null)
        {
          ProjectGroupModel projectGroupModel = CacheManager.GetProjectGroups().FirstOrDefault<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (p => p.id == pg.Id));
          if (projectGroupModel != null)
            return new FilterListItemViewModel()
            {
              Title = projectGroupModel.name,
              Icon = "IcOpenedFolder",
              SortOrder = projectGroupModel.sortOrder.GetValueOrDefault(),
              Value = (object) projectGroupModel.id,
              Selected = groupIds.Contains(projectGroupModel.id),
              ShowIcon = true,
              IsProjectGroup = true,
              Unfold = projectGroupModel.open,
              Children = model.Children.Select<SelectableItemViewModel, FilterListItemViewModel>((Func<SelectableItemViewModel, FilterListItemViewModel>) (child => FilterListItemViewModel.BuildItem(child, projectIds, groupIds))).ToList<FilterListItemViewModel>()
            };
        }
        else
        {
          switch (model)
          {
            case TeamSectionViewModel sectionViewModel:
              return new FilterListItemViewModel()
              {
                Title = sectionViewModel.Title,
                Value = (object) sectionViewModel.TeamId,
                Selected = false,
                ShowIcon = false,
                IsProjectGroup = false,
                CanSelect = false,
                IsTeam = true
              };
            case FilterCalendarViewModel calendarViewModel:
              return new FilterListItemViewModel()
              {
                Title = calendarViewModel.Title,
                Value = (object) calendarViewModel.Id,
                Selected = projectIds.Contains(calendarViewModel.Id),
                ShowIcon = true,
                IsProjectGroup = false,
                CanSelect = true,
                Icon = "IcCalendar"
              };
          }
        }
      }
      return listItemViewModel;
    }

    private static string GetProjectIcon(ProjectModel project)
    {
      if (project.IsNote)
        return !project.IsShareList() ? "IcNoteProject" : "IcShareNoteProject";
      if (project.Isinbox)
        return "IcInboxProject";
      return project.IsShareList() ? "IcSharedProject" : "IcNormalProject";
    }

    public static FilterListItemViewModel BuildDateItem(string title, string value, string icon)
    {
      return new FilterListItemViewModel()
      {
        Title = title,
        Value = (object) value,
        ShowIcon = true,
        Icon = icon
      };
    }

    public static FilterListItemViewModel BuildDateItem(
      string title,
      string value,
      string dateText,
      string icon)
    {
      return new FilterListItemViewModel()
      {
        Title = title,
        Value = (object) value,
        ShowIcon = true,
        DateText = dateText,
        Icon = icon
      };
    }

    public void SetNDaysValue(int days)
    {
      this.NDaysValue = days;
      switch (this.DisplayType)
      {
        case FilterItemDisplayType.NextNDays:
          this.Value = (object) string.Format("{0}days", (object) days);
          break;
        case FilterItemDisplayType.NDaysLater:
          this.Value = (object) string.Format("{0}dayslater", (object) days);
          break;
        case FilterItemDisplayType.AfterNDays:
          if (days <= 0)
            days = 7;
          this.Value = (object) string.Format("{0}daysfromtoday", (object) days);
          break;
        case FilterItemDisplayType.NDaysAgo:
          this.Value = (object) string.Format("-{0}daysfromtoday", (object) days);
          break;
      }
    }
  }
}
