// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.AttachmentSyncCollector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync.Model;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class AttachmentSyncCollector
  {
    public static async Task CollectRemoteAttachments(
      TaskModel serverTask,
      AttachmentSyncBean attachmentSyncBean)
    {
      List<AttachmentModel> taskAttachments = await AttachmentDao.GetTaskAttachments(serverTask.id);
      bool flag1 = serverTask.Attachments != null && serverTask.Attachments.Length != 0;
      bool flag2 = taskAttachments != null && taskAttachments.Count > 0;
      if (flag1 && !flag2)
      {
        foreach (AttachmentModel attachment in serverTask.Attachments)
        {
          if (string.IsNullOrEmpty(attachment.path))
            UtilLog.Info("NullPathRemoteAttachment : " + attachment.id);
          attachmentSyncBean.Added.Add(AttachmentTransfer.ConvertServerToLocal(attachment, serverTask.id));
        }
      }
      else if (!flag1 & flag2)
      {
        foreach (AttachmentModel attachmentModel in taskAttachments)
        {
          if (attachmentModel.sync_status == 2.ToString())
            attachmentSyncBean.Deleted.Add(attachmentModel);
        }
      }
      else
      {
        if (!flag1)
          return;
        Dictionary<string, AttachmentModel> localAttachmentMap = AttachmentSyncCollector.GetLocalAttachmentMap(taskAttachments);
        foreach (AttachmentModel attachment in serverTask.Attachments)
        {
          if (attachment != null)
          {
            if (localAttachmentMap.ContainsKey(attachment.id))
            {
              AttachmentModel attachmentModel = localAttachmentMap[attachment.id];
              if (attachmentModel.status != attachment.status || attachmentModel.path != attachment.path)
                attachmentSyncBean.Updated.Add(AttachmentTransfer.ConvertServerToLocal(attachment, serverTask.id));
              localAttachmentMap.Remove(attachment.id);
            }
            else
            {
              attachment.fileType = AttachmentProvider.GetFileType(attachment.fileName).ToString();
              attachmentSyncBean.Added.Add(AttachmentTransfer.ConvertServerToLocal(attachment, serverTask.id));
            }
          }
        }
        foreach (AttachmentModel attachmentModel in localAttachmentMap.Values)
        {
          if (attachmentModel.sync_status == 2.ToString())
            attachmentSyncBean.Deleted.Add(attachmentModel);
        }
      }
    }

    public static Dictionary<string, AttachmentModel> GetLocalAttachmentMap(
      List<AttachmentModel> attachments)
    {
      Dictionary<string, AttachmentModel> localAttachmentMap = new Dictionary<string, AttachmentModel>();
      if (attachments != null && attachments.Count > 0)
      {
        foreach (AttachmentModel attachment in attachments)
        {
          if (!localAttachmentMap.ContainsKey(attachment.id))
            localAttachmentMap.Add(attachment.id, attachment);
        }
      }
      return localAttachmentMap;
    }
  }
}
