// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ClosedProjectGroupViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class ClosedProjectGroupViewModel : ProjectGroupViewModel
  {
    public ClosedProjectGroupViewModel(ProjectGroupModel itemGroup)
      : base(itemGroup)
    {
      this.Icon = Utils.GetIcon("IcClosedGroup");
      this.CanDrag = false;
      this.IsGroupItem = true;
      this.ShowCount = false;
    }

    public override async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      return (IEnumerable<ContextAction>) null;
    }
  }
}
