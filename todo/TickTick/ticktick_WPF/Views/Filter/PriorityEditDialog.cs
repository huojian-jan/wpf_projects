// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.PriorityEditDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class PriorityEditDialog : ConditionEditDialogBase<int>
  {
    private List<int> _originPriorities;

    public PriorityEditDialog(List<int> selectedPriorities, bool showAll = false)
    {
      this.InitializeComponent();
      this.InitData((ICollection<int>) selectedPriorities, showAll);
      this.OnSelectedValueChanged += (EventHandler<FilterConditionViewModel>) ((sender, e) =>
      {
        if (e != null && e.ItemsSource.All<FilterListItemViewModel>((Func<FilterListItemViewModel, bool>) (item => item.Selected)))
        {
          foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) e.ItemsSource)
            listItemViewModel.Selected = false;
          e.IsAllSelected = true;
        }
        EventHandler<List<int>> selectedPriorityChanged = this.OnSelectedPriorityChanged;
        if (selectedPriorityChanged == null)
          return;
        selectedPriorityChanged((object) this, this.GetSelectedValues());
      });
      this._originPriorities = selectedPriorities.ToList<int>();
    }

    public PriorityEditDialog()
      : this(new List<int>())
    {
    }

    public event EventHandler<List<int>> OnSelectedPriorityChanged;

    private void InitData(ICollection<int> selectedPriorities, bool showAll)
    {
      FilterConditionViewModel conditionViewModel = new FilterConditionViewModel();
      conditionViewModel.Type = CondType.Priority;
      ObservableCollection<FilterListItemViewModel> observableCollection = new ObservableCollection<FilterListItemViewModel>();
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) 5,
        Title = Utils.GetString("PriorityHigh"),
        Selected = selectedPriorities.Contains(5)
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) 3,
        Title = Utils.GetString("PriorityMedium"),
        Selected = selectedPriorities.Contains(3)
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) 1,
        Title = Utils.GetString("PriorityLow"),
        Selected = selectedPriorities.Contains(1)
      });
      observableCollection.Add(new FilterListItemViewModel()
      {
        Value = (object) 0,
        Title = Utils.GetString("PriorityNull"),
        Selected = selectedPriorities.Contains(0)
      });
      conditionViewModel.ItemsSource = observableCollection;
      conditionViewModel.SupportedLogic = new List<LogicType>()
      {
        LogicType.Or
      };
      conditionViewModel.ShowAll = showAll;
      conditionViewModel.AllIconVisible = Visibility.Collapsed;
      conditionViewModel.IsAllSelected = !selectedPriorities.Any<int>();
      this.ViewModel = conditionViewModel;
    }

    protected override void SyncOriginal()
    {
      this._originPriorities = this.GetSelectedValues().ToList<int>();
    }

    protected override bool CanSave() => this.GetSelectedValues().Count > 0 || base.CanSave();

    public override void Restore()
    {
      this.RestoreSetSelected();
      EventHandler<List<int>> selectedPriorityChanged = this.OnSelectedPriorityChanged;
      if (selectedPriorityChanged == null)
        return;
      selectedPriorityChanged((object) this, this._originPriorities);
    }

    private void RestoreSetSelected()
    {
      foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this.ViewModel.ItemsSource)
        listItemViewModel.Selected = this._originPriorities.Contains((int) listItemViewModel.Value);
    }
  }
}
