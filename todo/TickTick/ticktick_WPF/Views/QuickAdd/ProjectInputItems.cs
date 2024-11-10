// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.ProjectInputItems
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class ProjectInputItems : BaseInputItems<ProjectModel>
  {
    private string _selected;

    public ProjectInputItems(string projectId)
    {
      this._selected = projectId;
      this.LoadData();
    }

    protected override ObservableCollection<InputItemViewModel<ProjectModel>> InitData()
    {
      List<ProjectModel> projects = CacheManager.GetProjects();
      ObservableCollection<InputItemViewModel<ProjectModel>> observableCollection1 = new ObservableCollection<InputItemViewModel<ProjectModel>>();
      List<string> list = ProjectDataAssembler.AssembleProjects(true).Where<SelectableItemViewModel>((Func<SelectableItemViewModel, bool>) (item => item is ProjectViewModel)).Select<SelectableItemViewModel, string>((Func<SelectableItemViewModel, string>) (item => item.Id)).ToList<string>();
      if (LocalSettings.Settings.SmartListInbox != 1)
      {
        ProjectModel entity = projects.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.Isinbox));
        if (entity != null)
        {
          entity.name = Utils.GetString("Inbox");
          observableCollection1.Add(new InputItemViewModel<ProjectModel>(entity.name, entity));
        }
      }
      foreach (string str in list)
      {
        string id = str;
        ProjectModel entity = projects.FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == id));
        if (entity != null)
        {
          ObservableCollection<InputItemViewModel<ProjectModel>> observableCollection2 = observableCollection1;
          InputItemViewModel<ProjectModel> inputItemViewModel = new InputItemViewModel<ProjectModel>(entity.name, entity);
          inputItemViewModel.HighLightSelected = entity.id == this._selected;
          observableCollection2.Add(inputItemViewModel);
        }
      }
      return observableCollection1;
    }

    internal void SetSelected(string projectId)
    {
      this._selected = projectId;
      this.LoadData();
    }
  }
}
