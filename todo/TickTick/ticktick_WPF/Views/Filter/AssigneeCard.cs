// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.AssigneeCard
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class AssigneeCard : CardBase
  {
    public AssigneeCard(CardViewModel viewModel)
      : base(viewModel)
    {
      if (!(this.EditDialog is AssigneeEditDialog editDialog))
        return;
      editDialog.OnSelectedAssigneeChanged += (EventHandler<List<string>>) ((sender, assignees) =>
      {
        this.SelectedAssignee = assignees;
        ((AssigneeCardViewModel) this.ViewModel).Values = assignees;
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

    private List<string> SelectedAssignee { get; set; } = new List<string>();

    protected override void Init(CardViewModel model)
    {
      this.SelectedAssignee = ((AssigneeCardViewModel) model).Values;
    }

    protected override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new AssigneeEditDialog(this.SelectedAssignee, false);
    }

    protected override bool CheckFilterValid() => this.SelectedAssignee.Count > 0;
  }
}
