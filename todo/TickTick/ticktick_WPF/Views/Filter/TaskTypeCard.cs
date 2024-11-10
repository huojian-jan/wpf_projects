// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.TaskTypeCard
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class TaskTypeCard : CardBase
  {
    public TaskTypeCard(CardViewModel viewModel)
      : base(viewModel)
    {
      if (!(this.EditDialog is TaskTypeEditDialog editDialog))
        return;
      editDialog.OnSelectedTypesChanged += (EventHandler<List<string>>) ((sender, selected) =>
      {
        this.SelectedTypes = selected;
        ((TaskTypeViewModel) this.ViewModel).Values = selected;
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

    private List<string> SelectedTypes { get; set; } = new List<string>();

    protected override void Init(CardViewModel model)
    {
      this.SelectedTypes = ((TaskTypeViewModel) model).Values;
    }

    protected override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new TaskTypeEditDialog((ICollection<string>) this.SelectedTypes);
    }

    protected override bool CheckFilterValid() => this.SelectedTypes.Count > 0;
  }
}
