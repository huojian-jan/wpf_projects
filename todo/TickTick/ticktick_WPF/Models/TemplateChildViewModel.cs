// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TemplateChildViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TemplateChildViewModel : BaseViewModel
  {
    public string Id { get; set; }

    public string Title { get; set; }

    public int Level { get; set; }

    public bool IsTextMode { get; set; }
  }
}
