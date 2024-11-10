// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.IListViewParent
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public interface IListViewParent
  {
    void OnProjectWidthChanged(double width);

    void OnDetailWidthChanged(double width);

    void SaveSelectedProject(string saveProjectId);

    double GetProjectWidth();

    double GetDetailWidth();

    string GetSelectedProject();

    Task NavigateTask(ProjectTask task);

    void EnterImmersiveMode(string taskId, int caretIndex);

    void ExitImmersiveMode();

    void SetMinSize(int width, int height);
  }
}
