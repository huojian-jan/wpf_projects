// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.TaskTypeEditDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class TaskTypeEditDialog : ConditionEditDialogBase<string>
  {
    private List<string> _originTaskTypes;

    public TaskTypeEditDialog(ICollection<string> selected)
    {
      this.InitializeComponent();
      this.InitData(selected);
      this.OnSelectedValueChanged += (EventHandler<FilterConditionViewModel>) ((sender, e) =>
      {
        EventHandler<List<string>> selectedTypesChanged = this.OnSelectedTypesChanged;
        if (selectedTypesChanged == null)
          return;
        selectedTypesChanged((object) this, this.GetSelectedValues());
      });
      this._originTaskTypes = selected.ToList<string>();
    }

    public TaskTypeEditDialog()
      : this((ICollection<string>) new List<string>())
    {
    }

    public event EventHandler<List<string>> OnSelectedTypesChanged;

    protected virtual void InitData(ICollection<string> selected)
    {
      FilterConditionViewModel conditionViewModel = new FilterConditionViewModel();
      conditionViewModel.Type = CondType.TaskType;
      ObservableCollection<FilterListItemViewModel> observableCollection = new ObservableCollection<FilterListItemViewModel>();
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "task",
        Title = Utils.GetString("Task"),
        Icon = "IcNormalProject",
        Selected = selected.Contains("task"),
        ShowIcon = true
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) "note",
        Title = Utils.GetString("Notes"),
        Icon = "IcNoteProject",
        Selected = selected.Contains("note"),
        ShowIcon = true
      });
      conditionViewModel.ItemsSource = observableCollection;
      conditionViewModel.SupportedLogic = new List<LogicType>()
      {
        LogicType.Or
      };
      this.ViewModel = conditionViewModel;
    }

    protected override void SyncOriginal()
    {
      this._originTaskTypes = this.GetSelectedValues().ToList<string>();
    }

    protected override bool CanSave() => this.GetSelectedValues().Count > 0 || base.CanSave();

    public override void Restore()
    {
      this.RestoreSetSelected();
      EventHandler<List<string>> selectedTypesChanged = this.OnSelectedTypesChanged;
      if (selectedTypesChanged == null)
        return;
      selectedTypesChanged((object) this, this._originTaskTypes);
    }

    private void RestoreSetSelected()
    {
      foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this.ViewModel.ItemsSource)
        listItemViewModel.Selected = this._originTaskTypes.Contains((string) listItemViewModel.Value);
    }
  }
}
