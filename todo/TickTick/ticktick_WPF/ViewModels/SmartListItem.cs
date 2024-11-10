// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.SmartListItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.ObjectModel;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class SmartListItem
  {
    public int id { get; set; }

    public string smartListTitle { get; set; }

    public int smartListStatus { get; set; }

    public ObservableCollection<ComboBoxItem> comboBoxItemList { get; set; }
  }
}
