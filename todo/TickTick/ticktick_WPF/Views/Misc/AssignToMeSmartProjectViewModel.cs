// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.AssignToMeSmartProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class AssignToMeSmartProjectViewModel : SmartProjectViewModel
  {
    public AssignToMeSmartProjectViewModel()
    {
      this.Title = Utils.GetString("AssignToMe");
      this.Icon = Utils.GetIconData("IcAssignToMe");
      this.Id = "_special_id_assigned";
    }
  }
}
