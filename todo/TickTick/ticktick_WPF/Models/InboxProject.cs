// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.InboxProject
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class InboxProject : SmartProject
  {
    public override string Id => Utils.GetInboxId();

    public override string Name => Utils.GetString("Inbox");

    public override Geometry Icon => Utils.GetIcon("IcInboxProject");

    public override string ProjectId => this.Id;

    public override int SortOrder
    {
      get => LocalSettings.Settings.SortOrderOfInbox;
      set => LocalSettings.Settings.SortOrderOfInbox = value;
    }
  }
}
