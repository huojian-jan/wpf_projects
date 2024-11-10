// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.ConditionEditDialogBase`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public abstract class ConditionEditDialogBase<T> : ConditionEditDialog
  {
    protected virtual List<T> GetSelectedValues()
    {
      if (this.ViewModel == null)
        return new List<T>();
      if (this.ViewModel.IsAllSelected)
        return new List<T>();
      List<T> selectedValues = new List<T>();
      foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this.ViewModel.ItemsSource)
      {
        if (listItemViewModel.IsAssignee)
        {
          if (listItemViewModel.Selected)
          {
            T obj = (T) listItemViewModel.Value;
            selectedValues.Add(obj);
            if (obj.ToString() == "other")
              break;
          }
        }
        else
        {
          if (listItemViewModel.Selected && !listItemViewModel.IsSecondLevel && (!listItemViewModel.IsProjectGroup || listItemViewModel.IsTagParent))
            selectedValues.Add((T) listItemViewModel.Value);
          if (listItemViewModel.IsProjectGroup)
            listItemViewModel.Children?.ForEach((Action<FilterListItemViewModel>) (item =>
            {
              if (!item.Selected)
                return;
              selectedValues.Add((T) item.Value);
            }));
        }
      }
      return selectedValues;
    }
  }
}
