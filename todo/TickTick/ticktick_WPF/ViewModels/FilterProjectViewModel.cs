// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.FilterProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class FilterProjectViewModel : ProjectItemViewModel, IDroppable
  {
    private readonly FilterTaskDefault _filterTaskDefault;

    public FilterModel Filter { get; private set; }

    public FilterProjectViewModel(FilterModel filter)
    {
      this.Id = filter.id;
      this.Filter = filter;
      this.Icon = Utils.GetIcon("IcFilterProject");
      this.Title = filter.name;
      this.IsPtfItem = true;
      this.CanDrag = true;
      this.CanDrop = false;
      this.ViewMode = filter.viewMode;
      this.ListType = ProjectListType.Filter;
      this._filterTaskDefault = FilterViewModel.CalculateTaskDefault(filter.rule, true);
      string emojiIcon = EmojiHelper.GetEmojiIcon(filter.name);
      if (string.IsNullOrEmpty(emojiIcon) || !filter.name.StartsWith(emojiIcon))
        return;
      this._emojiText = emojiIcon;
    }

    public string ProjectId => this._filterTaskDefault?.ProjectModel?.id;

    public DateTime? DefaultDate => this._filterTaskDefault?.DefaultDate;

    public bool IsCompleted => false;

    public bool IsAbandoned => false;

    public bool IsDeleted => false;

    public List<string> Tags => this._filterTaskDefault?.DefaultTags;

    public int Priority
    {
      get => (int?) this._filterTaskDefault?.Priority ?? TaskDefaultDao.GetDefaultSafely().Priority;
    }

    public bool Multiple => true;

    public override async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      FilterProjectViewModel projectViewModel = this;
      string saveIdentity = projectViewModel.GetSaveIdentity();
      bool windowShowing = !string.IsNullOrEmpty(saveIdentity) && TaskListWindow.Windows.ContainsKey(saveIdentity);
      List<ContextAction> contextActions = new List<ContextAction>();
      contextActions.Add(new ContextAction(ContextActionKey.Edit));
      List<ContextAction> contextActionList = contextActions;
      contextActionList.Add(new ContextAction(await ProjectPinSortOrderService.CheckIsProjectPinned(projectViewModel.Filter.id, 8) ? ContextActionKey.Unpin : ContextActionKey.Pin));
      contextActions.Add(new ContextAction(ContextActionKey.Duplicate));
      contextActions.Add(new ContextAction(windowShowing ? ContextActionKey.CloseWindow : ContextActionKey.OpenNewWindow));
      contextActions.Add(new ContextAction(ContextActionKey.Delete));
      return (IEnumerable<ContextAction>) contextActions;
    }

    public override string GetSaveIdentity() => "filter:" + this.Filter.id;

    public override async void LoadCount()
    {
      FilterProjectViewModel projectViewModel = this;
      projectViewModel.Count = TaskCountCache.TryGetCount(projectViewModel.Filter.id);
      await Task.Delay(new Random().Next(1000, 3000));
      // ISSUE: reference to a compiler-generated method
      Task.Run<int>(new Func<Task<int>>(projectViewModel.\u003CLoadCount\u003Eb__24_0));
      await Task.Delay(3000);
      projectViewModel.Count = TaskCountCache.TryGetCount(projectViewModel.Filter.id);
    }

    public override ProjectIdentity GetIdentity()
    {
      this.Filter = CacheManager.GetFilterById(this.Filter.id) ?? this.Filter;
      return (ProjectIdentity) new FilterProjectIdentity(this.Filter);
    }

    public override PtfType GetPtfType() => PtfType.Filter;
  }
}
