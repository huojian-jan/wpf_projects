// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.ISectionList
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public interface ISectionList
  {
    void OnSectionStatusChanged(SectionStatus status);

    Task OnTaskOpenClick(DisplayItemModel model);

    Task OnAddTaskInSectionClick(DisplayItemModel model);

    void SelectOrDeselectAll(DisplayItemModel model, bool selectAll);
  }
}
