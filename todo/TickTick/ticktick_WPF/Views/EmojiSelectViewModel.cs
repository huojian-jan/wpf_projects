// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.EmojiSelectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views
{
  public class EmojiSelectViewModel : BaseViewModel
  {
    private bool _folded;

    public string Text { get; set; }

    public string Key { get; set; }

    public bool IsSection { get; set; }

    public bool IsEmojiItems
    {
      get
      {
        List<EmojiSelectViewModel> children = this.Children;
        return children != null && __nonvirtual (children.Count) > 0;
      }
    }

    public bool Folded
    {
      get => this._folded;
      set
      {
        this._folded = value;
        this.OnPropertyChanged(nameof (Folded));
      }
    }

    public List<EmojiSelectViewModel> Children { get; set; }
  }
}
