// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.InboxProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class InboxProjectViewModel : SmartProjectViewModel, IDroppable
  {
    public InboxProjectViewModel(SmartProject project)
      : base(project)
    {
      this.CanDrop = true;
      this.Id = this.ProjectId;
      ProjectModel projectModel = CacheManager.GetProjects().ToList<ProjectModel>().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.Isinbox));
      if (projectModel == null)
        return;
      this.Color = projectModel.color;
    }

    public new string ProjectId => Utils.GetInboxId();

    public override async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      string saveIdentity = this.GetSaveIdentity();
      bool flag1 = !string.IsNullOrEmpty(saveIdentity) && TaskListWindow.Windows.ContainsKey(saveIdentity);
      bool flag2 = SmartProjectViewModel.IsProjectAuto("SmartListInbox");
      return (IEnumerable<ContextAction>) new List<ContextAction>()
      {
        new ContextAction(ContextActionKey.Edit),
        new ContextAction(ContextActionKey.DisplayOption)
        {
          SubActions = new List<ContextAction>()
          {
            new ContextAction(ContextActionKey.Show)
            {
              Selected = !flag2
            },
            new ContextAction(ContextActionKey.ShowIfNotEmpty)
            {
              Selected = flag2
            },
            new ContextAction(ContextActionKey.Hide)
          }
        },
        new ContextAction(flag1 ? ContextActionKey.CloseWindow : ContextActionKey.OpenNewWindow)
      };
    }

    public override async void LoadCount()
    {
      this.Count = await Task.Run<int>((Func<Task<int>>) (async () => await TaskCountCache.TryGetCount((ProjectIdentity) ProjectIdentity.CreateInboxProject())));
    }

    public override ProjectIdentity GetIdentity()
    {
      return (ProjectIdentity) ProjectIdentity.CreateInboxProject();
    }
  }
}
