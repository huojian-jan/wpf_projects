// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.DateCard
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class DateCard : CardBase
  {
    public DateCard(CardViewModel viewModel)
      : base(viewModel)
    {
      if (!(this.EditDialog is DateEditDialog editDialog))
        return;
      editDialog.OnSelectedDateChanged += (EventHandler<List<string>>) ((sender, dates) =>
      {
        this.SelectedDates = dates;
        ((DateCardViewModel) this.ViewModel).Values = dates;
        this.ViewModel.Content = this.ViewModel.ToCardText();
      });
      editDialog.OnLogicChanged += (EventHandler<LogicType>) ((sender, logicType) =>
      {
        this.ViewModel.LogicType = logicType;
        this.ViewModel.Content = this.ViewModel.ToCardText();
      });
      editDialog.OnCancel += (EventHandler) ((sender, e) => this.Popup.IsOpen = false);
      editDialog.OnSave += (EventHandler<FilterConditionViewModel>) ((sender, e) =>
      {
        this.IsSave = true;
        this.Popup.IsOpen = false;
      });
      this.Popup.Closed -= new EventHandler(((CardBase) this).CardPopupClosed);
      this.Popup.Closed += new EventHandler(((CardBase) this).CardPopupClosed);
    }

    private List<string> SelectedDates { get; set; } = new List<string>();

    protected override bool CheckFilterValid() => this.SelectedDates.Count > 0;

    protected override void Init(CardViewModel model)
    {
      this.SelectedDates = ((DateCardViewModel) model).Values;
    }

    protected override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new DateEditDialog(false, this.SelectedDates, this.ViewModel.LogicType, this.ViewModel.Version);
    }
  }
}
