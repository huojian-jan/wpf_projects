// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.FocusStatisticsTitleItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class FocusStatisticsTitleItemViewModel : FocusStatisticsPanelItemViewModel
  {
    public string Title { get; set; }

    public string ImageName { get; set; }

    public string ImageTag { get; set; }

    public FocusStatisticsTitleItemViewModel(string title, string imageName, string imageTag)
    {
      this.Title = title;
      this.ImageName = imageName;
      this.ImageTag = imageTag;
      this.IsTitle = true;
    }
  }
}
