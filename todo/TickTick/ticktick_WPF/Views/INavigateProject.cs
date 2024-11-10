// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.INavigateProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views
{
  public interface INavigateProject
  {
    void NavigateProjectById(string projectId);

    void NavigateTodayProject();

    void NavigateTomorrowProject();
  }
}
