// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.AttachmentCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Attachment;
using ticktick_WPF.Util.Files;

#nullable disable
namespace ticktick_WPF.Cache
{
  public static class AttachmentCache
  {
    private static ConcurrentDictionary<string, AttachmentModel> _attachmentModelDict = new ConcurrentDictionary<string, AttachmentModel>();

    public static async Task ResetDictItems()
    {
      List<AttachmentModel> listAsync = await App.Connection.Table<AttachmentModel>().ToListAsync();
      ConcurrentDictionary<string, AttachmentModel> concurrentDictionary = new ConcurrentDictionary<string, AttachmentModel>();
      foreach (AttachmentModel attachmentModel in listAsync)
      {
        if (!string.IsNullOrEmpty(attachmentModel.id) && !string.IsNullOrEmpty(attachmentModel.taskId) && TaskCache.ExistTask(attachmentModel.taskId))
        {
          if (concurrentDictionary.ContainsKey(attachmentModel.id))
            App.Connection.DeleteAsync((object) attachmentModel);
          else
            concurrentDictionary[attachmentModel.id] = attachmentModel;
        }
      }
      AttachmentCache._attachmentModelDict = concurrentDictionary;
    }

    public static AttachmentModel GetAttachmentById(string id)
    {
      if (string.IsNullOrEmpty(id) || !AttachmentCache._attachmentModelDict.ContainsKey(id))
        return (AttachmentModel) null;
      AttachmentModel attachment = AttachmentCache._attachmentModelDict[id];
      if (string.IsNullOrEmpty(attachment.localPath) && attachment.id != attachment.refId)
      {
        string localPath = AttachmentDownloadUtils.GetLocalPath(attachment);
        if (!FileUtils.FileEmptyOrNotExists(localPath))
        {
          attachment.localPath = localPath;
          AttachmentCache.TryUpdate(attachment);
        }
      }
      return attachment;
    }

    public static string GetFileName(string id)
    {
      return !string.IsNullOrEmpty(id) && AttachmentCache._attachmentModelDict.ContainsKey(id) ? AttachmentCache._attachmentModelDict[id].fileName : (string) null;
    }

    private static async void TryUpdate(AttachmentModel attachment)
    {
      AttachmentModel attachmentById = await AttachmentDao.GetAttachmentById(attachment.id);
      if (attachmentById != null)
      {
        attachment._Id = attachmentById._Id;
        int num = await App.Connection.UpdateAsync((object) attachment);
      }
      else
      {
        int num1 = await App.Connection.InsertAsync((object) attachment);
      }
    }

    public static List<AttachmentModel> GetAllAttachments()
    {
      return AttachmentCache._attachmentModelDict.Values.ToList<AttachmentModel>();
    }

    public static List<AttachmentModel> GetAttachmentsByTaskId(string taskId)
    {
      return AttachmentCache.GetAllAttachments().Where<AttachmentModel>((Func<AttachmentModel, bool>) (a => a.taskId == taskId && !a.deleted && a.status == 0)).ToList<AttachmentModel>();
    }

    public static bool GetTaskExistAttachment(string taskId)
    {
      return AttachmentCache.GetAllAttachments().Any<AttachmentModel>((Func<AttachmentModel, bool>) (a => a.taskId == taskId && !a.deleted && a.status == 0));
    }

    public static void SetAttachment(AttachmentModel attachment)
    {
      if (string.IsNullOrEmpty(attachment?.id))
        return;
      AttachmentCache._attachmentModelDict[attachment.id] = attachment;
    }

    public static int TodayAttachmentCount()
    {
      string todayAttachmentCount = LocalSettings.Settings.ExtraSettings.TodayAttachmentCount;
      if (string.IsNullOrEmpty(todayAttachmentCount))
        return 0;
      string oldValue = DateTime.Now.ToUniversalTime().ToString("yyyyMMdd':'");
      int result;
      return todayAttachmentCount.StartsWith(oldValue) && int.TryParse(todayAttachmentCount.Replace(oldValue, ""), out result) ? result : 0;
    }

    public static void SetTodayAttachmentCount(int num)
    {
      DateTime dateTime = DateTime.Now;
      dateTime = dateTime.ToUniversalTime();
      string str = dateTime.ToString("yyyyMMdd':'");
      LocalSettings.Settings.ExtraSettings.TodayAttachmentCount = str + num.ToString();
      LocalSettings.Settings.Save(true);
    }
  }
}
