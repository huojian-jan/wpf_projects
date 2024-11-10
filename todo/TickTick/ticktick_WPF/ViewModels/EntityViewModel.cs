// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.EntityViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class EntityViewModel : BaseViewModel
  {
    public EntityViewModel()
    {
    }

    public EntityViewModel(string title, string key)
    {
      this.Title = title;
      this.Key = key;
    }

    public string Title { get; set; }

    public string Key { get; set; }
  }
}
