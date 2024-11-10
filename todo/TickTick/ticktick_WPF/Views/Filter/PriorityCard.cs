// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.PriorityCard
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class PriorityCard : CardBase
  {
    public PriorityCard(CardViewModel viewModel)
      : base(viewModel)
    {
      if (!(this.EditDialog is PriorityEditDialog editDialog))
        return;
      editDialog.OnSelectedPriorityChanged += (EventHandler<List<int>>) ((sender, priorities) =>
      {
        this.SelectedPriority = priorities;
        ((PriorityCardViewModel) this.ViewModel).Values = priorities;
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

    private List<int> SelectedPriority { get; set; } = new List<int>();

    protected override void Init(CardViewModel model)
    {
      this.SelectedPriority = ((PriorityCardViewModel) model).Values;
    }

    protected override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new PriorityEditDialog(this.SelectedPriority);
    }

    protected override bool CheckFilterValid() => this.SelectedPriority.Count > 0;
  }
}
