// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.ITaskOperation
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Views.MainListView.TaskList;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public interface ITaskOperation
  {
    void SetPriority(int priority);

    void SetDate(string key);

    void ClearDate();

    void SelectDate(bool relative);

    void ToggleTaskCompleted();

    void Delete();

    TaskListView GetParentList();

    bool ParsingDate();

    bool IsNewAdd();

    void PinOrUnpinTask();

    void OpenSticky();
  }
}
