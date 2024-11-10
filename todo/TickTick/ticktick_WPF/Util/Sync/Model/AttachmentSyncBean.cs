// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.AttachmentSyncBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class AttachmentSyncBean
  {
    private List<AttachmentModel> added = new List<AttachmentModel>();
    private List<AttachmentModel> updated = new List<AttachmentModel>();
    private List<AttachmentModel> deleted = new List<AttachmentModel>();

    public bool Empty
    {
      get => this.added.Count == 0 && this.updated.Count == 0 && this.deleted.Count == 0;
    }

    public List<AttachmentModel> Added
    {
      get => this.added;
      set => this.added = value;
    }

    public List<AttachmentModel> Updated
    {
      get => this.updated;
      set => this.updated = value;
    }

    public List<AttachmentModel> Deleted
    {
      get => this.deleted;
      set => this.deleted = value;
    }
  }
}
