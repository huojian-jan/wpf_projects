// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.AssignInputItems
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Collections.ObjectModel;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class AssignInputItems : BaseInputItems<AvatarViewModel>
  {
    public List<AvatarViewModel> Models;
    private string _selected;

    public AssignInputItems(List<AvatarViewModel> models, bool selectFirst = true, string selected = null)
    {
      this.Models = models;
      this._selected = selected;
      this.LoadData(selectFirst);
    }

    protected override ObservableCollection<InputItemViewModel<AvatarViewModel>> InitData()
    {
      if (this.Models == null)
        return (ObservableCollection<InputItemViewModel<AvatarViewModel>>) null;
      ObservableCollection<InputItemViewModel<AvatarViewModel>> observableCollection1 = new ObservableCollection<InputItemViewModel<AvatarViewModel>>();
      foreach (AvatarViewModel model in this.Models)
      {
        ObservableCollection<InputItemViewModel<AvatarViewModel>> observableCollection2 = observableCollection1;
        InputItemViewModel<AvatarViewModel> inputItemViewModel = new InputItemViewModel<AvatarViewModel>(model.Name, model.UserId, model.AvatarUrl, model);
        inputItemViewModel.IsNoAvatar = model.IsNoAvatar;
        inputItemViewModel.HighLightSelected = model.UserId == this._selected || string.IsNullOrEmpty(this._selected) && model.UserId == "-1";
        observableCollection2.Add(inputItemViewModel);
      }
      return observableCollection1;
    }

    public void SetModels(List<AvatarViewModel> avatars)
    {
      this.Models = avatars;
      this.LoadData();
    }

    internal void SetSelected(string assignee)
    {
      this._selected = assignee;
      this.LoadData();
    }
  }
}
