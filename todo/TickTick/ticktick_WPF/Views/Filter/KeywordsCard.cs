// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.KeywordsCard
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class KeywordsCard : ConditionCard
  {
    public KeywordsCard(CardViewModel viewModel)
      : base(viewModel)
    {
    }

    protected override void OnDeleteClick()
    {
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
  }
}
