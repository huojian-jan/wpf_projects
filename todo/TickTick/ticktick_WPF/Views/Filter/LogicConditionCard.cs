// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.LogicConditionCard
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class LogicConditionCard : ConditionCard
  {
    public LogicConditionCard(CardViewModel viewModel)
      : base(viewModel)
    {
    }

    protected override void OnCardClick() => this.ShowSelectLogicDialog();

    protected override void OnDeleteClick() => this.ViewModel.Type = CardType.InitLogic;
  }
}
