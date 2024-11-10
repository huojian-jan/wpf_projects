// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ProjectGroupViewModel
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
  public class ProjectGroupViewModel : ProjectItemViewModel
  {
    public bool IsNew;
    private bool _editing;

    public ProjectGroupModel ProjectGroup { get; private set; }

    public ProjectGroupViewModel(ProjectGroupModel itemGroup)
    {
      this.Id = itemGroup.id;
      this.ProjectGroup = itemGroup;
      this.Icon = Utils.GetIcon(itemGroup.open ? "IcOpenedFolder" : "IcClosedFolder");
      this.Title = itemGroup.name;
      this.SortOrder = itemGroup.sortOrder.GetValueOrDefault();
      this.Open = itemGroup.open;
      this.CanDrag = !TeamDao.IsTeamExpired(itemGroup.teamId);
      this.ViewMode = itemGroup.viewMode;
      this.TeamId = itemGroup.teamId;
      string emojiIcon = EmojiHelper.GetEmojiIcon(itemGroup.name);
      this.IsPtfItem = true;
      if (!string.IsNullOrEmpty(emojiIcon) && itemGroup.name.StartsWith(emojiIcon))
        this._emojiText = emojiIcon;
      this.IsGroupItem = true;
    }

    public bool Editing
    {
      get => this._editing;
      set
      {
        this._editing = true;
        this.OnPropertyChanged(nameof (Editing));
      }
    }

    public override string GetSaveIdentity() => "group:" + this.ProjectGroup.id;

    public override async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      ProjectGroupViewModel projectGroupViewModel = this;
      string saveIdentity = projectGroupViewModel.GetSaveIdentity();
      bool windowShowing = !string.IsNullOrEmpty(saveIdentity) && TaskListWindow.Windows.ContainsKey(saveIdentity);
      List<ContextAction> contextActions = new List<ContextAction>();
      contextActions.Add(new ContextAction(ContextActionKey.AddProject));
      contextActions.Add(new ContextAction(ContextActionKey.Edit));
      List<ContextAction> contextActionList = contextActions;
      contextActionList.Add(new ContextAction(await ProjectPinSortOrderService.CheckIsProjectPinned(projectGroupViewModel.ProjectGroup.id, 6) ? ContextActionKey.Unpin : ContextActionKey.Pin));
      contextActions.Add(new ContextAction(windowShowing ? ContextActionKey.CloseWindow : ContextActionKey.OpenNewWindow));
      contextActions.Add(new ContextAction(ContextActionKey.Ungroup));
      return (IEnumerable<ContextAction>) contextActions;
    }

    public override async void LoadCount()
    {
      ProjectGroupViewModel projectGroupViewModel = this;
      if (projectGroupViewModel.IsNew)
        await Task.Delay(500);
      // ISSUE: reference to a compiler-generated method
      int num = await Task.Run<int>(new Func<Task<int>>(projectGroupViewModel.\u003CLoadCount\u003Eb__12_0));
      projectGroupViewModel.Count = num;
    }

    public override ProjectIdentity GetIdentity()
    {
      List<ProjectModel> projectsInGroup = CacheManager.GetProjectsInGroup(this.ProjectGroup.id);
      this.ProjectGroup = CacheManager.GetGroupById(this.ProjectGroup.id) ?? this.ProjectGroup;
      return (ProjectIdentity) new GroupProjectIdentity(this.ProjectGroup, projectsInGroup);
    }

    public override PtfType GetPtfType() => PtfType.Project;
  }
}
