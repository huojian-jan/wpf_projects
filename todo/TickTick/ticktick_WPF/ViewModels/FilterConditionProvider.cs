// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.FilterConditionProvider
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public static class FilterConditionProvider
  {
    public static List<CardViewModel> BuildInitData()
    {
      return new List<CardViewModel>()
      {
        new CardViewModel() { Type = CardType.InitFilter },
        new CardViewModel() { Type = CardType.InitLogic },
        new CardViewModel() { Type = CardType.InitFilter },
        new CardViewModel() { Type = CardType.AddMore }
      };
    }

    public static List<CardViewModel> AddMoreCondition(List<CardViewModel> conditions)
    {
      conditions.ForEach((Action<CardViewModel>) (con => con.ShowDropdown = false));
      CardViewModel cardViewModel = conditions.Find((Predicate<CardViewModel>) (x => x.Type == CardType.AddMore));
      if (cardViewModel != null)
        conditions.Remove(cardViewModel);
      conditions.Add(new CardViewModel()
      {
        Type = CardType.InitLogic
      });
      conditions.Add(new CardViewModel()
      {
        Type = CardType.InitFilter
      });
      if (conditions.Count <= 3)
        conditions.Add(new CardViewModel()
        {
          Type = CardType.AddMore
        });
      return conditions;
    }
  }
}
