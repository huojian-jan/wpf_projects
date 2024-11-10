// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TagProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class TagProjectViewModel : ProjectItemViewModel, IDroppable
  {
    public TagModel TagModel;
    public bool IsParent;
    public bool IsNew;

    public TagProjectViewModel(TagModel tag)
    {
      this.Id = tag.name;
      this.Icon = Utils.GetIcon("IcTagLine");
      this.TagModel = tag;
      this.Title = tag.GetDisplayName();
      this.Color = tag.color;
      this.CanDrag = true;
      this.CanDrop = true;
      this.Open = !tag.collapsed;
      this.IsSubItem = tag.IsChild();
      this.IsPtfItem = true;
      this.IsParent = tag.IsParent();
      this.IsGroupItem = this.IsParent;
      this.ListType = ProjectListType.Tag;
      this.ViewMode = tag.viewMode;
    }

    public string ProjectId => string.Empty;

    public DateTime? DefaultDate => new DateTime?();

    public bool IsCompleted => false;

    public bool IsAbandoned => false;

    public bool IsDeleted => false;

    public List<string> Tags
    {
      get => new List<string>() { this.TagModel.name };
    }

    public int Priority => 0;

    public bool Multiple => false;

    public override async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      TagProjectViewModel projectViewModel = this;
      int count = 0;
      count = projectViewModel.IsParent ? CacheManager.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => !t.IsChild())).ToList<TagModel>().Count : CacheManager.GetTags().Count;
      string saveIdentity = projectViewModel.GetSaveIdentity();
      bool windowShowing = !string.IsNullOrEmpty(saveIdentity) && TaskListWindow.Windows.ContainsKey(saveIdentity);
      List<ContextAction> contextActionList1 = new List<ContextAction>();
      contextActionList1.Add(new ContextAction(ContextActionKey.Edit));
      List<ContextAction> contextActionList2 = contextActionList1;
      contextActionList2.Add(new ContextAction(await ProjectPinSortOrderService.CheckIsProjectPinned(projectViewModel.TagModel.name, 7) ? ContextActionKey.Unpin : ContextActionKey.Pin));
      contextActionList1.Add(new ContextAction(windowShowing ? ContextActionKey.CloseWindow : ContextActionKey.OpenNewWindow));
      contextActionList1.Add(new ContextAction(ContextActionKey.Delete));
      List<ContextAction> contextActions = contextActionList1;
      contextActionList2 = (List<ContextAction>) null;
      contextActionList1 = (List<ContextAction>) null;
      if (TagService.ExistShareTag())
      {
        List<ContextAction> contextActionList = contextActions;
        TagModel tagModel = projectViewModel.TagModel;
        ContextAction contextAction = (tagModel != null ? (tagModel.type == 2 ? 1 : 0) : 0) != 0 ? new ContextAction(ContextActionKey.ToPersonalTag) : new ContextAction(ContextActionKey.ToShareTag);
        contextActionList.Insert(2, contextAction);
      }
      if (count > 1)
        contextActions.Insert(2, new ContextAction(ContextActionKey.MergeTags));
      return (IEnumerable<ContextAction>) contextActions;
    }

    public override string GetSaveIdentity() => "tag:" + this.TagModel.name;

    public override async void LoadCount()
    {
      TagProjectViewModel projectViewModel = this;
      if (projectViewModel.IsNew)
        return;
      // ISSUE: reference to a compiler-generated method
      int num = await Task.Run<int>(new Func<Task<int>>(projectViewModel.\u003CLoadCount\u003Eb__22_0));
      projectViewModel.Count = num;
    }

    public override ProjectIdentity GetIdentity()
    {
      this.TagModel = CacheManager.GetTagByName(this.Id) ?? this.TagModel;
      return (ProjectIdentity) new TagProjectIdentity(this.TagModel);
    }

    public override PtfType GetPtfType() => PtfType.Tag;
  }
}
