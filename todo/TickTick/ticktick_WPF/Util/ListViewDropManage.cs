// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ListViewDropManage
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Util
{
  public class ListViewDropManage
  {
    public static int GetCurrentIndex(
      ListView listview,
      ListViewDropManage.GetPositionDelegate getPosition)
    {
      int currentIndex = -1;
      for (int index = 0; index < listview.Items.Count; ++index)
      {
        ListViewItem listViewItem = Utils.GetListViewItem(listview, index);
        if (listViewItem != null && ListViewDropManage.IsMouseOverTarget((Visual) listViewItem, getPosition))
        {
          currentIndex = index;
          break;
        }
      }
      return currentIndex;
    }

    public static bool IsMouseOverTarget(
      Visual target,
      ListViewDropManage.GetPositionDelegate getPosition)
    {
      return VisualTreeHelper.GetDescendantBounds(target).Contains(getPosition((IInputElement) target));
    }

    public delegate System.Windows.Point GetPositionDelegate(IInputElement element);
  }
}
