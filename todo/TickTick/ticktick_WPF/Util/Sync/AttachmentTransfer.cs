﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.AttachmentTransfer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class AttachmentTransfer
  {
    public static AttachmentModel ConvertServerToLocal(AttachmentModel remote, string taskId)
    {
      return new AttachmentModel()
      {
        id = remote.id,
        taskId = taskId,
        fileType = remote.fileType,
        fileName = remote.fileName,
        status = remote.status,
        path = remote.path,
        createdTime = remote.createdTime,
        size = remote.size,
        refId = remote.refId,
        sync_status = 2.ToString()
      };
    }
  }
}