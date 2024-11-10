// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.Item.MatrixListItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;

#nullable disable
namespace ticktick_WPF.Views.TaskList.Item
{
  public class MatrixListItem : TaskListItem
  {
    public MatrixListItem()
      : base(true, false)
    {
      this.RequestBringIntoView += new RequestBringIntoViewEventHandler(this.OnBringIntoView);
    }

    private void OnBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }
  }
}
