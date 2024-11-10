// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.FeatureInfoWindowViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Views
{
  public class FeatureInfoWindowViewModel
  {
    public string Title { get; set; }

    public string Info1 { get; set; }

    public string Info2 { get; set; }

    public string GifPath { get; set; }

    public string CommandTitle { get; set; }

    public Action Action { get; set; }

    public FeatureInfoWindowViewModel(
      string title,
      string info1,
      string info2,
      string path,
      string cTitle,
      Action act)
    {
      this.Title = title;
      this.Info1 = info1;
      this.Info2 = info2;
      this.GifPath = path;
      this.CommandTitle = cTitle;
      this.Action = act;
    }

    public void DoAction()
    {
      if (this.Action == null)
        return;
      this.Action();
    }
  }
}
