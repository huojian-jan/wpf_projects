// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ReleaseNoteModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class ReleaseNoteModel
  {
    public string vsersion { get; set; }

    public List<NoteItemModel> data { get; set; }

    public int release_date { get; set; }

    public string release_type { get; set; } = "grey";

    public PackageSizeModel size { get; set; }

    public bool IsForced() => this.release_type == "rollout";

    public bool IsGreyForced() => this.IsForced() || this.release_type == "grey_force";
  }
}
