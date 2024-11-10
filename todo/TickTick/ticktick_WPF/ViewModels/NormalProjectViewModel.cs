// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.NormalProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;
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
  public class NormalProjectViewModel : ProjectItemViewModel, IDroppable
  {
    public ProjectModel Project { get; private set; }

    public NormalProjectViewModel()
    {
    }

    public NormalProjectViewModel(ProjectModel project)
    {
      this.Id = project.id;
      this.Project = project;
      this.Color = project.color;
      this.Icon = project.IsNote ? Utils.GetIcon(project.IsShareList() ? "IcShareNoteProject" : "IcNoteProject") : Utils.GetIcon(project.IsShareList() ? "IcSharedProject" : "IcNormalProject");
      this.Title = project.name;
      this.SortOrder = project.sortOrder;
      this.CanDrag = !TeamDao.IsTeamExpired(project.teamId);
      this.CanDrop = this.CanDrag && project.IsProjectPermit();
      this.TeamId = CacheManager.GetTeamById(project.teamId)?.id;
      this.IsProjectPermit = project.IsProjectPermit();
      this.IsPtfItem = true;
      this.ViewMode = project.viewMode;
      this.ListType = ProjectListType.Project;
      string emojiIcon = EmojiHelper.GetEmojiIcon(project.name);
      if (!string.IsNullOrEmpty(emojiIcon) && project.name.StartsWith(emojiIcon))
        this._emojiText = emojiIcon;
      if (!project.IsOverLimit())
        return;
      this.InfoIcon = Utils.GetIcon("IcExpired");
      this.Count = 0;
      this.InfoIconBrush = (Brush) ThemeUtil.GetColorInString("#FFB000");
      this.Info = Utils.GetString("UnsyncedLists");
    }

    public string ProjectId => this.Project.id;

    public DateTime? DefaultDate => new DateTime?();

    public bool IsCompleted => false;

    public bool IsAbandoned => false;

    public bool IsDeleted => false;

    public List<string> Tags => new List<string>();

    public int Priority => 0;

    public bool Multiple => false;

    public override async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      NormalProjectViewModel projectViewModel = this;
      string saveIdentity = projectViewModel.GetSaveIdentity();
      bool windowShowing = !string.IsNullOrEmpty(saveIdentity) && TaskListWindow.Windows.ContainsKey(saveIdentity);
      bool? closed = projectViewModel.Project.closed;
      int num;
      if (closed.HasValue)
      {
        closed = projectViewModel.Project.closed;
        num = closed.Value ? 1 : 0;
      }
      else
        num = 0;
      bool flag = num != 0;
      ContextAction exitOrDelete = new ContextAction(!projectViewModel.Project.IsShareList() || projectViewModel.Project.isOwner ? ContextActionKey.Delete : ContextActionKey.Exit);
      ContextAction openOrClose = new ContextAction(flag ? ContextActionKey.UnArchiveList : ContextActionKey.Archive);
      List<ContextAction> contextActionList;
      if (flag)
      {
        contextActionList = new List<ContextAction>()
        {
          openOrClose,
          new ContextAction(ContextActionKey.Edit),
          exitOrDelete
        };
        if (projectViewModel.IsProjectPermit)
          contextActionList.Insert(1, new ContextAction(ContextActionKey.Duplicate));
      }
      else
      {
        try
        {
          List<ContextAction> contextActionList1 = new List<ContextAction>();
          contextActionList1.Add(new ContextAction(ContextActionKey.Edit));
          List<ContextAction> contextActionList2 = contextActionList1;
          contextActionList2.Add(new ContextAction(await ProjectPinSortOrderService.CheckIsProjectPinned(projectViewModel.Project.id, 5) ? ContextActionKey.Unpin : ContextActionKey.Pin));
          contextActionList1.Add(new ContextAction(ContextActionKey.Share));
          contextActionList1.Add(new ContextAction(windowShowing ? ContextActionKey.CloseWindow : ContextActionKey.OpenNewWindow));
          contextActionList1.Add(openOrClose);
          contextActionList1.Add(exitOrDelete);
          contextActionList = contextActionList1;
          contextActionList2 = (List<ContextAction>) null;
          contextActionList1 = (List<ContextAction>) null;
          if (projectViewModel.IsProjectPermit)
            contextActionList.Insert(2, new ContextAction(ContextActionKey.Duplicate));
        }
        catch (Exception ex)
        {
          contextActionList = new List<ContextAction>();
        }
      }
      IEnumerable<ContextAction> contextActions = (IEnumerable<ContextAction>) contextActionList;
      exitOrDelete = (ContextAction) null;
      openOrClose = (ContextAction) null;
      return contextActions;
    }

    public override string GetSaveIdentity() => "project:" + this.Project.id;

    public override ProjectIdentity GetIdentity()
    {
      this.Project = CacheManager.GetProjectById(this.Project.id) ?? this.Project;
      return (ProjectIdentity) new NormalProjectIdentity(this.Project);
    }

    public override async void LoadCount()
    {
      NormalProjectViewModel projectViewModel = this;
      // ISSUE: reference to a compiler-generated method
      int num = await Task.Run<int>(new Func<Task<int>>(projectViewModel.\u003CLoadCount\u003Eb__25_0));
      projectViewModel.Count = num;
    }

    public override PtfType GetPtfType() => PtfType.Project;
  }
}
