// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.ProjectOrGroupCard
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class ProjectOrGroupCard : CardBase
  {
    public ProjectOrGroupCard(CardViewModel viewModel)
      : base(viewModel)
    {
      this.InitEditDialog();
    }

    private List<string> SelectedProjectIds { get; set; } = new List<string>();

    private List<string> SelectedGroupIds { get; set; } = new List<string>();

    private void InitEditDialog()
    {
      if (!(this.EditDialog is ProjectOrGroupEditDialog editDialog))
        return;
      editDialog.OnSelectedProjectChanged += (EventHandler<List<string>>) ((sender, projectIds) =>
      {
        this.SelectedProjectIds = projectIds;
        ((ProjectOrGroupCardViewModel) this.ViewModel).Values = projectIds;
        this.ViewModel.Content = this.ViewModel.ToCardText();
      });
      editDialog.OnSelectedGroupChanged += (EventHandler<List<string>>) ((sender, groupIds) =>
      {
        this.SelectedGroupIds = groupIds;
        ((ProjectOrGroupCardViewModel) this.ViewModel).GroupIds = groupIds;
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

    protected override void Init(CardViewModel model)
    {
      this.SelectedProjectIds = ((ProjectOrGroupCardViewModel) model).Values;
      this.SelectedGroupIds = ((ProjectOrGroupCardViewModel) model).GroupIds;
    }

    protected override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new ProjectOrGroupEditDialog(false, this.SelectedProjectIds, this.SelectedGroupIds, this.ViewModel.LogicType);
    }

    protected override bool CheckFilterValid()
    {
      return this.SelectedProjectIds.Count > 0 || this.SelectedGroupIds.Count > 0;
    }
  }
}
