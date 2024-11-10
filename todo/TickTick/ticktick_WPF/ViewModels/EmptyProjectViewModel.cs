// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.EmptyProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class EmptyProjectViewModel : ProjectItemViewModel
  {
    public EmptyProjectViewModel() => this.IsEmptyProject = true;

    public PtfType Type { get; set; }
  }
}
