// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.SmartProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class SmartProjectViewModel : ProjectItemViewModel, IDroppable
  {
    public readonly SmartProject Project;

    public SmartProjectViewModel(SmartProject smartProject)
    {
      this.Id = smartProject.Id;
      this.Icon = smartProject.Icon;
      this.Title = smartProject.Name;
      this.Count = smartProject.Count;
      this.IconText = smartProject.IconText;
      this.Project = smartProject;
      int num1;
      switch (smartProject)
      {
        case CompletedProject _:
        case TrashProject _:
          num1 = 0;
          break;
        default:
          num1 = !(smartProject is AbandonedProject) ? 1 : 0;
          break;
      }
      this.CanDrag = num1 != 0;
      int num2;
      switch (smartProject)
      {
        case TodayProject _:
        case TomorrowProject _:
        case WeekProject _:
        case CompletedProject _:
        case AbandonedProject _:
          num2 = 1;
          break;
        default:
          num2 = smartProject is TrashProject ? 1 : 0;
          break;
      }
      this.CanDrop = num2 != 0;
      this.SortOrder = (long) smartProject.SortOrder;
      int num3;
      switch (smartProject)
      {
        case CompletedProject _:
        case AbandonedProject _:
          num3 = 0;
          break;
        default:
          num3 = !(smartProject is TrashProject) ? 1 : 0;
          break;
      }
      this.ShowCount = num3 != 0;
      this.ListType = ProjectListType.Smart;
      this.ViewMode = SmartProjectService.GetSmartProjectViewMode(smartProject.Id);
    }

    public string ProjectId => string.Empty;

    public DateTime? DefaultDate => this.Project.DefaultDate;

    public bool IsCompleted => this.Project is CompletedProject;

    public bool IsAbandoned => this.Project is AbandonedProject;

    public List<string> Tags => new List<string>();

    public int Priority => 0;

    public bool Multiple => false;

    public bool IsDeleted => this.Project is TrashProject;

    public override async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      SmartProjectViewModel projectViewModel = this;
      if (projectViewModel.Project is SummaryProject)
        return (IEnumerable<ContextAction>) new List<ContextAction>()
        {
          new ContextAction(ContextActionKey.Hide)
        };
      if (projectViewModel.Project is InboxProject)
        return (IEnumerable<ContextAction>) null;
      bool withAuto = false;
      bool auto = false;
      bool flag1 = false;
      bool flag2 = false;
      if (projectViewModel.Project == null)
        return (IEnumerable<ContextAction>) null;
      switch (projectViewModel.Project)
      {
        case TodayProject _:
          auto = SmartProjectViewModel.IsProjectAuto("SmartListToday");
          withAuto = true;
          break;
        case TomorrowProject _:
          auto = SmartProjectViewModel.IsProjectAuto("SmartListTomorrow");
          withAuto = true;
          break;
        case WeekProject _:
          auto = SmartProjectViewModel.IsProjectAuto("SmartList7Day");
          withAuto = true;
          break;
        case AssignProject _:
          auto = SmartProjectViewModel.IsProjectAuto("SmartListForMe");
          withAuto = true;
          break;
        case AbandonedProject _:
          auto = SmartProjectViewModel.IsProjectAuto("SmartListAbandoned");
          withAuto = true;
          break;
      }
      switch (projectViewModel.Project)
      {
        case AbandonedProject _:
        case CompletedProject _:
        case TrashProject _:
          flag2 = await ProjectPinSortOrderService.CheckIsProjectPinned(projectViewModel.Project.UserEventId, 11);
          flag1 = true;
          break;
      }
      List<ContextAction> contextActions = new List<ContextAction>();
      List<ContextAction> contextActionList = new List<ContextAction>();
      if (flag1)
        contextActions.Add(flag2 ? new ContextAction(ContextActionKey.Unpin) : new ContextAction(ContextActionKey.Pin));
      contextActions.Add(new ContextAction(ContextActionKey.DisplayOption)
      {
        SubActions = contextActionList
      });
      contextActionList.Add(new ContextAction(ContextActionKey.Show)
      {
        Selected = !withAuto || !auto
      });
      if (withAuto)
        contextActionList.Add(new ContextAction(ContextActionKey.ShowIfNotEmpty)
        {
          Selected = auto
        });
      contextActionList.Add(new ContextAction(ContextActionKey.Hide));
      string saveIdentity = projectViewModel.GetSaveIdentity();
      bool flag3 = !string.IsNullOrEmpty(saveIdentity) && TaskListWindow.Windows.ContainsKey(saveIdentity);
      contextActions.Add(new ContextAction(flag3 ? ContextActionKey.CloseWindow : ContextActionKey.OpenNewWindow));
      return (IEnumerable<ContextAction>) contextActions;
    }

    protected static bool IsProjectAuto(string key) => (int) LocalSettings.Settings[key] == 2;

    public override string GetSaveIdentity() => "smart:" + this.Project.Id;

    public override ProjectIdentity GetIdentity()
    {
      return (ProjectIdentity) SmartProjectIdentity.BuildSmartProject(this.Project.Id);
    }

    public static SmartProjectViewModel BuildModel(SmartListType smartType)
    {
      switch (smartType)
      {
        case SmartListType.All:
          return new SmartProjectViewModel((SmartProject) new AllProject());
        case SmartListType.Today:
          return new SmartProjectViewModel((SmartProject) new TodayProject());
        case SmartListType.Tomorrow:
          return new SmartProjectViewModel((SmartProject) new TomorrowProject());
        case SmartListType.Week:
          return new SmartProjectViewModel((SmartProject) new WeekProject());
        case SmartListType.Assign:
          return new SmartProjectViewModel((SmartProject) new AssignProject());
        case SmartListType.Completed:
          return new SmartProjectViewModel((SmartProject) new CompletedProject());
        case SmartListType.Trash:
          return new SmartProjectViewModel((SmartProject) new TrashProject());
        case SmartListType.Summary:
          return new SmartProjectViewModel((SmartProject) new SummaryProject());
        case SmartListType.Inbox:
          return (SmartProjectViewModel) new InboxProjectViewModel((SmartProject) new InboxProject());
        case SmartListType.Abandoned:
          return new SmartProjectViewModel((SmartProject) new AbandonedProject());
        default:
          return (SmartProjectViewModel) null;
      }
    }

    public override async void LoadCount()
    {
      SmartProjectViewModel projectViewModel = this;
      // ISSUE: reference to a compiler-generated method
      int num = await Task.Run<int>(new Func<Task<int>>(projectViewModel.\u003CLoadCount\u003Eb__23_0));
      projectViewModel.Count = num;
    }
  }
}
