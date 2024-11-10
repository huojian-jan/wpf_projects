// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.TaskLinkInputItems
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Collections.ObjectModel;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class TaskLinkInputItems : BaseInputItems<TaskBaseViewModel>
  {
    public List<TaskBaseViewModel> Models;

    public TaskLinkInputItems(List<TaskBaseViewModel> models, bool selectFirst = true)
    {
      this.Models = models;
      this.LoadData(selectFirst);
    }

    protected override ObservableCollection<InputItemViewModel<TaskBaseViewModel>> InitData()
    {
      if (this.Models == null)
        return (ObservableCollection<InputItemViewModel<TaskBaseViewModel>>) null;
      ObservableCollection<InputItemViewModel<TaskBaseViewModel>> observableCollection = new ObservableCollection<InputItemViewModel<TaskBaseViewModel>>();
      foreach (TaskBaseViewModel model in this.Models)
        observableCollection.Add(new InputItemViewModel<TaskBaseViewModel>(model.Title, model.Id, model, false));
      return observableCollection;
    }

    public void SetModels(List<TaskBaseViewModel> tasks)
    {
      this.Models = tasks;
      this.LoadData();
    }
  }
}
