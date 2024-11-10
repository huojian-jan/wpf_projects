// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ContextAction
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class ContextAction : BaseViewModel
  {
    public ContextAction()
    {
    }

    public ContextAction(ContextActionKey actionKey)
    {
      this.ActionKey = actionKey;
      if (actionKey != ContextActionKey.OpenNewWindow)
      {
        if (actionKey == ContextActionKey.CloseWindow)
          this.Title = Utils.GetString("CloseWindow");
        else
          this.Title = Utils.GetString(actionKey.ToString());
      }
      else
        this.Title = Utils.GetString("OpenNewWindow");
    }

    public ContextActionKey ActionKey { get; set; }

    public string Title { get; set; }

    public string Key { get; set; }

    public List<ContextAction> SubActions { get; set; }

    public bool Selected { get; set; }
  }
}
