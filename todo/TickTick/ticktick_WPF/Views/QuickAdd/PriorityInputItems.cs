// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.PriorityInputItems
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.ObjectModel;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class PriorityInputItems : BaseInputItems<int>
  {
    private int _selected;

    public PriorityInputItems(int selected = -1)
    {
      this._selected = selected;
      this.LoadData();
    }

    protected override ObservableCollection<InputItemViewModel<int>> InitData()
    {
      ObservableCollection<InputItemViewModel<int>> observableCollection = new ObservableCollection<InputItemViewModel<int>>();
      InputItemViewModel<int> inputItemViewModel1 = new InputItemViewModel<int>(Utils.GetString("PriorityHigh"), 5);
      inputItemViewModel1.HighLightSelected = this._selected == 5;
      observableCollection.Add(inputItemViewModel1);
      InputItemViewModel<int> inputItemViewModel2 = new InputItemViewModel<int>(Utils.GetString("PriorityMedium"), 3);
      inputItemViewModel2.HighLightSelected = this._selected == 3;
      observableCollection.Add(inputItemViewModel2);
      InputItemViewModel<int> inputItemViewModel3 = new InputItemViewModel<int>(Utils.GetString("PriorityLow"), 1);
      inputItemViewModel3.HighLightSelected = this._selected == 1;
      observableCollection.Add(inputItemViewModel3);
      InputItemViewModel<int> inputItemViewModel4 = new InputItemViewModel<int>(Utils.GetString("PriorityNull"), 0);
      inputItemViewModel4.HighLightSelected = this._selected == 0;
      observableCollection.Add(inputItemViewModel4);
      return observableCollection;
    }

    internal void SetSelected(int priority)
    {
      this._selected = priority;
      this.LoadData();
    }
  }
}
