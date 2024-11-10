// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.PtfAllViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class PtfAllViewModel : ProjectItemViewModel
  {
    public PtfType Type { get; set; }

    public bool IsTag => this.Type == PtfType.Tag;

    public bool IsProject => this.Type == PtfType.Project;

    public bool IsFilter => this.Type == PtfType.Filter;

    public bool IsSubscribe => this.Type == PtfType.Subscribe;

    public bool IsTeam
    {
      get
      {
        return this.IsProject && !string.IsNullOrEmpty(this.TeamId) && this.TeamId != "c1a7e08345e444dea187e21a692f0d7a";
      }
    }

    public PtfAllViewModel(PtfType type)
    {
      this.Id = type.ToString();
      this.IsGroupItem = true;
      this.IsPtfItem = true;
      this.Type = type;
      this.CanDrag = true;
      switch (type)
      {
        case PtfType.Project:
          this.Title = Utils.GetString("lists");
          this.Open = LocalSettings.Settings.AllProjectOpened;
          break;
        case PtfType.Tag:
          this.Title = Utils.GetString("tag");
          this.Open = LocalSettings.Settings.AllTagOpened;
          break;
        case PtfType.Filter:
          this.Title = Utils.GetString("Filter");
          this.Open = LocalSettings.Settings.AllFilterOpened;
          break;
        case PtfType.Subscribe:
          this.Title = Utils.GetString("SubscribeCalendar");
          this.Open = LocalSettings.Settings.AllSubscribeOpened;
          break;
      }
      this.IsPtfAll = true;
      this.ShowCount = false;
    }

    public override async Task<IEnumerable<ContextAction>> GetContextActions()
    {
      int num = this.IsTag ? 1 : 0;
      List<ContextAction> contextActions = new List<ContextAction>()
      {
        new ContextAction(ContextActionKey.Show),
        new ContextAction(ContextActionKey.Hide)
      };
      if (num != 0)
        contextActions.Insert(0, new ContextAction(ContextActionKey.ShowIfNotEmpty)
        {
          Title = Utils.GetString("ShowOmitEmptyTags")
        });
      return (IEnumerable<ContextAction>) contextActions;
    }
  }
}
