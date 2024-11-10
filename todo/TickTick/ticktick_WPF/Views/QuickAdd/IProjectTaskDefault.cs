// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.IProjectTaskDefault
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public interface IProjectTaskDefault
  {
    string GetProjectId();

    string GetProjectName();

    string GetColumnId();

    TimeData GetTimeData();

    int GetPriority();

    List<string> GetTags();

    bool UseDefaultTags();

    bool IsCalendar();

    string GetAccountId();

    bool GetIsNote();

    string GetAssignee();
  }
}
