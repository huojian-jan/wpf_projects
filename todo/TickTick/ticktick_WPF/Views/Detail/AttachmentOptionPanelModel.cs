// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.AttachmentOptionPanelModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class AttachmentOptionPanelModel : BaseViewModel
  {
    private string _fileName;

    public string FileName
    {
      get => this._fileName;
      set
      {
        this._fileName = value;
        this.OnPropertyChanged(nameof (FileName));
      }
    }
  }
}
