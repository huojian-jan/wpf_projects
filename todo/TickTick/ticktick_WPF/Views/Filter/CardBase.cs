// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.CardBase
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class CardBase : ConditionCard
  {
    protected bool IsSave;

    protected CardBase(CardViewModel viewModel)
      : base(viewModel)
    {
      this.ViewModel = viewModel;
      this.ViewModel.Content = viewModel.ToCardText();
      this.Init(viewModel);
      this.EditDialog = this.GenerateEditDialog();
      this.Popup.Child = (UIElement) this.EditDialog;
      this.Popup.Closed += new EventHandler(this.Popup_Closed);
      if (!this.ViewModel.ShowDropdown)
        return;
      this.Popup.IsOpen = true;
    }

    protected virtual void Init(CardViewModel model)
    {
    }

    protected override void OnCardClick()
    {
      if (this.Popup.IsOpen)
        return;
      this.Popup.IsOpen = true;
    }

    private void Popup_Closed(object sender, EventArgs e) => this.PopupClosed();

    private void PopupClosed()
    {
      if (!this.EditDialog.IsSave)
        this.EditDialog.Restore();
      this.EditDialog.IsSave = false;
    }

    protected override void OnDeleteClick() => this.ReturnToInitFilter();

    private void ReturnToInitFilter()
    {
      this.ViewModel.ShowDropdown = false;
      this.FireEvent(new ChangeData()
      {
        Type = ChangeType.Filter,
        From = this.ViewModel,
        To = new CardViewModel()
        {
          Type = CardType.InitFilter
        }
      });
    }

    protected void CardPopupClosed(object sender, EventArgs e)
    {
      if (this.EditDialog == null)
        return;
      if (!this.IsSave)
      {
        this.EditDialog.Restore();
        if (!this.CheckFilterValid())
          this.ReturnToInitFilter();
      }
      this.IsSave = false;
    }

    protected virtual bool CheckFilterValid() => true;
  }
}
