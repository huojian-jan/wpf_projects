// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ProjectList.ProjectPinItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.MainListView.ProjectList
{
  public class ProjectPinItemViewModel : BaseViewModel
  {
    public string EntityId;
    public int Type;
    public long SortOrder;
    private Geometry _icon;
    private string _emoji;
    private string _color;
    private string _title;
    private bool _selected;
    private bool _isMore;
    private bool _dragging;
    private double _itemWidth = 46.0;

    public ProjectPinItemViewModel()
    {
    }

    public ProjectPinItemViewModel(SyncSortOrderModel pinSortOrderModel, ProjectModel project)
    {
      this.EntityId = project.id;
      this.Type = pinSortOrderModel.Type;
      this.SortOrder = pinSortOrderModel.SortOrder;
      this.Color = project.color;
      string emojiIcon = EmojiHelper.GetEmojiIcon(project.name);
      if (!string.IsNullOrEmpty(emojiIcon) && project.name.StartsWith(emojiIcon))
      {
        this.Emoji = emojiIcon;
        this.Title = project.name.Substring(emojiIcon.Length);
      }
      else
      {
        this.Icon = project.IsNote ? Utils.GetIcon(project.IsShareList() ? "IcShareNoteProject" : "IcNoteProject") : Utils.GetIcon(project.IsShareList() ? "IcSharedProject" : "IcNormalProject");
        this.Title = project.name;
      }
    }

    public ProjectPinItemViewModel(SyncSortOrderModel pinSortOrderModel, ProjectGroupModel group)
    {
      this.EntityId = group.id;
      this.Type = pinSortOrderModel.Type;
      this.SortOrder = pinSortOrderModel.SortOrder;
      string emojiIcon = EmojiHelper.GetEmojiIcon(group.name);
      if (!string.IsNullOrEmpty(emojiIcon) && group.name.StartsWith(emojiIcon))
      {
        this.Emoji = emojiIcon;
        this.Title = group.name.Substring(emojiIcon.Length);
      }
      else
      {
        this.Icon = Utils.GetIcon("IcClosedFolder");
        this.Title = group.name;
      }
    }

    public ProjectPinItemViewModel(SyncSortOrderModel pinSortOrderModel, TagModel tag)
    {
      this.EntityId = tag.name;
      this.Type = pinSortOrderModel.Type;
      this.SortOrder = pinSortOrderModel.SortOrder;
      this.Icon = Utils.GetIcon("IcTagLine");
      this.Color = tag.color;
      this.Title = tag.GetDisplayName();
    }

    public ProjectPinItemViewModel(SyncSortOrderModel pinSortOrderModel, FilterModel filter)
    {
      this.EntityId = filter.id;
      this.Type = pinSortOrderModel.Type;
      this.SortOrder = pinSortOrderModel.SortOrder;
      string emojiIcon = EmojiHelper.GetEmojiIcon(filter.name);
      if (!string.IsNullOrEmpty(emojiIcon) && filter.name.StartsWith(emojiIcon))
      {
        this.Emoji = emojiIcon;
        this.Title = filter.name.Substring(emojiIcon.Length);
      }
      else
      {
        this.Icon = Utils.GetIcon("IcFilterProject");
        this.Title = filter.name;
      }
    }

    public ProjectPinItemViewModel(
      SyncSortOrderModel pinSortOrderModel,
      CalendarSubscribeProfileModel subscribe)
    {
      this.EntityId = pinSortOrderModel.EntityId;
      this.Type = pinSortOrderModel.Type;
      this.Color = subscribe.Color;
      this.SortOrder = pinSortOrderModel.SortOrder;
      this.Icon = Utils.GetIcon("IcSubscribeCalendar");
      this.Title = subscribe.Name;
    }

    public ProjectPinItemViewModel(
      SyncSortOrderModel pinSortOrderModel,
      BindCalendarAccountModel account)
    {
      this.EntityId = pinSortOrderModel.EntityId;
      this.Type = pinSortOrderModel.Type;
      this.SortOrder = pinSortOrderModel.SortOrder;
      this.Title = string.IsNullOrEmpty(account.Description) ? account.Account : account.Description;
      if (account.Site == "feishu")
        this.Title = Utils.GetString("FeishuCalendar");
      this.Icon = SubscribeCalendarHelper.GetCalendarProjectIcon(account);
      this.IsBindAccount = true;
    }

    public ProjectPinItemViewModel(SyncSortOrderModel pinSortOrderModel, SmartProject smart)
    {
      this.EntityId = smart.Id;
      this.Type = pinSortOrderModel.Type;
      this.SortOrder = pinSortOrderModel.SortOrder;
      this.Title = smart.Name;
      this.Icon = smart.Icon;
    }

    public bool IsBindAccount { get; set; }

    public string SyncEntityId
    {
      get
      {
        if (this.Type != 11)
          return this.EntityId;
        return this.EntityId?.Replace("_special_id_", "");
      }
    }

    public string DisplayTitle
    {
      get
      {
        string text1 = this.Title.Substring(0, Math.Min(this.Title.Length, 9));
        double width = Utils.MeasureString(text1, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 10.0).Width;
        if (width < 42.0)
        {
          string text2 = text1;
          for (; width < 42.0 && text1.Length < this.Title.Length; width = Utils.MeasureString(text2, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 10.0).Width)
          {
            text1 = text2;
            if (text2.Length != this.Title.Length)
              text2 = this.Title.Substring(0, text2.Length + 1);
            else
              break;
          }
        }
        else
        {
          for (; width > 42.0 && text1.Length > 0; width = Utils.MeasureString(text1, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 10.0).Width)
            text1 = text1.Substring(0, text1.Length - 1);
        }
        return text1;
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

    public string Emoji
    {
      get => this._emoji;
      set
      {
        this._emoji = value;
        this.OnPropertyChanged(nameof (Emoji));
      }
    }

    public string Color
    {
      get => this._color;
      set
      {
        this._color = value;
        this.OnPropertyChanged(nameof (Color));
      }
    }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
        this.OnPropertyChanged("DisplayTitle");
      }
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

    public bool IsMore
    {
      get => this._isMore;
      set
      {
        this._isMore = value;
        this.OnPropertyChanged(nameof (IsMore));
      }
    }

    public bool Dragging
    {
      get => this._dragging;
      set
      {
        this._dragging = value;
        this.OnPropertyChanged(nameof (Dragging));
      }
    }

    public double ItemWidth
    {
      get => this._itemWidth;
      set
      {
        this._itemWidth = value;
        this.OnPropertyChanged(nameof (ItemWidth));
      }
    }

    public SolidColorBrush IconBackground
    {
      get
      {
        string themeId = LocalSettings.Settings.ThemeId;
        if (!(themeId == "White") && !(themeId == "Blue"))
          return ThemeUtil.GetAlphaColor("#FFFFFF", 80);
        string color = this.Color;
        if (string.IsNullOrEmpty(color) || color == "transparent")
        {
          System.Windows.Media.Color? resource = Application.Current?.FindResource((object) "ColorPrimary") as System.Windows.Media.Color?;
          ref System.Windows.Media.Color? local = ref resource;
          color = (local.HasValue ? local.GetValueOrDefault().ToString() : (string) null) ?? "#000000";
        }
        return ThemeUtil.GetAlphaColor(color, 10);
      }
    }

    public SolidColorBrush IconColor
    {
      get
      {
        string color = this.Color;
        if (string.IsNullOrEmpty(color) || color == "transparent")
        {
          System.Windows.Media.Color? resource = Application.Current?.FindResource((object) "ColorPrimary") as System.Windows.Media.Color?;
          ref System.Windows.Media.Color? local = ref resource;
          color = (local.HasValue ? local.GetValueOrDefault().ToString() : (string) null) ?? "#66000000";
        }
        return ThemeUtil.GetColorInString(color);
      }
    }

    public static List<ProjectPinItemViewModel> BuildModels(List<SyncSortOrderModel> pinnedModel)
    {
      List<ProjectPinItemViewModel> pinItemViewModelList = new List<ProjectPinItemViewModel>();
      ConcurrentDictionary<string, ProjectModel> projectDict = CacheManager.GetProjectDict();
      ConcurrentDictionary<string, ProjectGroupModel> groupDict = CacheManager.GetGroupDict();
      ConcurrentDictionary<string, TagModel> tagDict = CacheManager.GetTagDict();
      ConcurrentDictionary<string, FilterModel> filterDict = CacheManager.GetFilterDict();
      ConcurrentDictionary<string, CalendarSubscribeProfileModel> subscribeDict = CacheManager.GetSubscribeDict();
      ConcurrentDictionary<string, BindCalendarAccountModel> accountCalDict = CacheManager.GetAccountCalDict();
      foreach (SyncSortOrderModel pinSortOrderModel in pinnedModel)
      {
        if (!string.IsNullOrEmpty(pinSortOrderModel.EntityId))
        {
          switch (pinSortOrderModel.Type)
          {
            case 5:
              if (projectDict.ContainsKey(pinSortOrderModel.EntityId))
              {
                ProjectModel project = projectDict[pinSortOrderModel.EntityId];
                if (!project.delete_status)
                {
                  pinItemViewModelList.Add(new ProjectPinItemViewModel(pinSortOrderModel, project));
                  continue;
                }
                continue;
              }
              continue;
            case 6:
              if (groupDict.ContainsKey(pinSortOrderModel.EntityId))
              {
                ProjectGroupModel group = groupDict[pinSortOrderModel.EntityId];
                if (group.deleted == 0)
                {
                  pinItemViewModelList.Add(new ProjectPinItemViewModel(pinSortOrderModel, group));
                  continue;
                }
                continue;
              }
              continue;
            case 7:
              if (LocalSettings.Settings.SmartListTag != 1 && tagDict.ContainsKey(pinSortOrderModel.EntityId))
              {
                TagModel tag = tagDict[pinSortOrderModel.EntityId];
                if (tag.deleted == 0)
                {
                  pinItemViewModelList.Add(new ProjectPinItemViewModel(pinSortOrderModel, tag));
                  continue;
                }
                continue;
              }
              continue;
            case 8:
              if (LocalSettings.Settings.ShowCustomSmartList == 0 && filterDict.ContainsKey(pinSortOrderModel.EntityId))
              {
                FilterModel filter = filterDict[pinSortOrderModel.EntityId];
                if (filter.deleted == 0)
                {
                  pinItemViewModelList.Add(new ProjectPinItemViewModel(pinSortOrderModel, filter));
                  continue;
                }
                continue;
              }
              continue;
            case 9:
              if (subscribeDict.ContainsKey(pinSortOrderModel.EntityId))
              {
                CalendarSubscribeProfileModel subscribe = subscribeDict[pinSortOrderModel.EntityId];
                if (subscribe.Show != "hidden")
                {
                  pinItemViewModelList.Add(new ProjectPinItemViewModel(pinSortOrderModel, subscribe));
                  continue;
                }
                continue;
              }
              if (accountCalDict.ContainsKey(pinSortOrderModel.EntityId))
              {
                BindCalendarAccountModel account = accountCalDict[pinSortOrderModel.EntityId];
                pinItemViewModelList.Add(new ProjectPinItemViewModel(pinSortOrderModel, account));
                continue;
              }
              continue;
            case 11:
              SmartProject smart = (SmartProject) null;
              switch (pinSortOrderModel.EntityId)
              {
                case "completed":
                  if (LocalSettings.Settings.SmartListComplete != 1)
                  {
                    smart = (SmartProject) new CompletedProject();
                    break;
                  }
                  break;
                case "abandoned":
                  if (LocalSettings.Settings.SmartListAbandoned != 1)
                  {
                    smart = (SmartProject) new AbandonedProject();
                    break;
                  }
                  break;
                case "trash":
                  if (LocalSettings.Settings.SmartListTrash != 1)
                  {
                    smart = (SmartProject) new TrashProject();
                    break;
                  }
                  break;
              }
              if (smart != null)
              {
                pinItemViewModelList.Add(new ProjectPinItemViewModel(pinSortOrderModel, smart));
                continue;
              }
              continue;
            default:
              continue;
          }
        }
      }
      return pinItemViewModelList;
    }
  }
}
