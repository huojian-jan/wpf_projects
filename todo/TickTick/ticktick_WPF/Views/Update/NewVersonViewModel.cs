// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Update.NewVersonViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Update
{
  public class NewVersonViewModel : BaseViewModel
  {
    public string NewVersion { get; set; }

    public string OldVersion { get; set; }

    public string ReleaseNote { get; set; }

    public string DownloadPath { get; set; }

    public PackageSizeModel SizeModel { get; set; }
  }
}
