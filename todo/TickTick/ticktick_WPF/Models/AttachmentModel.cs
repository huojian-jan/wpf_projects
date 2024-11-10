// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.AttachmentModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class AttachmentModel : BaseModel
  {
    public string id { get; set; }

    public string refId { get; set; }

    public string path { get; set; }

    public string localPath { get; set; }

    public int size { get; set; }

    public string fileName { get; set; }

    public string fileType { get; set; }

    public DateTime? createdTime { get; set; }

    public string taskId { get; set; }

    public bool deleted { get; set; }

    public bool canDelete { get; set; }

    public string sync_status { get; set; }

    public int status { get; set; }

    public bool IsActive() => !this.deleted && this.status == 0;

    public static AttachmentModel Copy(AttachmentModel other)
    {
      return other.MemberwiseClone() as AttachmentModel;
    }
  }
}
