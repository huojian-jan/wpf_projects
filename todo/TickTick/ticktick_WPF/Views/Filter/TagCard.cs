// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.TagCard
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class TagCard : CardBase
  {
    public TagCard(CardViewModel viewModel)
      : base(viewModel)
    {
      this.InitEditDialog();
    }

    private List<string> SelectedTags { get; set; } = new List<string>();

    private LogicType LogicType { get; set; } = LogicType.Or;

    private void InitEditDialog()
    {
      if (!(this.EditDialog is TagEditDialog editDialog))
        return;
      editDialog.OnSelectedTagChanged += (EventHandler<List<string>>) ((sender, tags) =>
      {
        this.SelectedTags = tags;
        ((TagCardViewModel) this.ViewModel).Values = tags;
        this.ViewModel.Content = this.ViewModel.ToCardText();
      });
      editDialog.OnLogicChanged += (EventHandler<LogicType>) ((sender, logicType) =>
      {
        this.LogicType = logicType;
        this.ViewModel.LogicType = this.LogicType;
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
      this.SelectedTags = ((TagCardViewModel) model).Values;
      this.LogicType = model.LogicType;
    }

    protected override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new TagEditDialog(false, this.SelectedTags, this.LogicType);
    }

    protected override bool CheckFilterValid() => this.SelectedTags.Count > 0;
  }
}
