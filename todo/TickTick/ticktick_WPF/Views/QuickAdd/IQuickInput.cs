// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.IQuickInput
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public interface IQuickInput
  {
    bool Filter(string key, List<string> selected = null);

    bool IsTag();

    void Move(bool forward);

    void TrySelectItem(bool exactly = false);
  }
}
