// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.AttachmentDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync.Model;
using TickTickDao;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class AttachmentDao : BaseDao<AttachmentModel>
  {
    private static TTAsyncLocker _asyncLocker = new TTAsyncLocker(1, 1);

    public static async Task InsertOrUpdateAttachment(AttachmentModel attachment)
    {
      AttachmentModel attachmentById = await AttachmentDao.GetAttachmentById(attachment.id);
      if (attachmentById == null)
      {
        int num = await App.Connection.InsertAsync((object) attachment);
        AttachmentCache.SetAttachment(await AttachmentDao.GetAttachmentById(attachment.id));
      }
      else
      {
        attachment._Id = attachmentById._Id;
        attachment.localPath = attachmentById.localPath;
        AttachmentCache.SetAttachment(attachment);
        int num = await App.Connection.UpdateAsync((object) attachment);
      }
    }

    public static async Task UpdateAttachment(AttachmentModel attachment)
    {
      AttachmentModel attachmentById = await AttachmentDao.GetAttachmentById(attachment.id);
      if (attachmentById == null)
        return;
      attachment._Id = attachmentById._Id;
      AttachmentCache.SetAttachment(attachment);
      int num = await App.Connection.UpdateAsync((object) attachment);
    }

    public static async Task UpdateUploadAttachment(AttachmentModel attachment)
    {
      AttachmentModel attachmentById = await AttachmentDao.GetAttachmentById(attachment.id);
      if (attachmentById == null)
        return;
      attachmentById.path = attachment.path;
      AttachmentCache.SetAttachment(attachmentById);
      int num = await App.Connection.UpdateAsync((object) attachmentById);
    }

    public static async Task<AttachmentModel> GetAttachmentById(string id)
    {
      return await App.Connection.Table<AttachmentModel>().Where((Expression<Func<AttachmentModel, bool>>) (t => t.id == id)).FirstOrDefaultAsync();
    }

    private static async Task<List<AttachmentModel>> GetAttachmentsById(string id)
    {
      return await App.Connection.Table<AttachmentModel>().Where((Expression<Func<AttachmentModel, bool>>) (t => t.id == id)).ToListAsync();
    }

    public static async Task<List<AttachmentModel>> GetTaskAttachments(string taskId, bool active = false)
    {
      return active ? await Task.Run<List<AttachmentModel>>((Func<Task<List<AttachmentModel>>>) (async () => await App.Connection.Table<AttachmentModel>().Where((Expression<Func<AttachmentModel, bool>>) (v => v.taskId == taskId && v.deleted != true && v.status == 0)).ToListAsync())) : await Task.Run<List<AttachmentModel>>((Func<Task<List<AttachmentModel>>>) (async () => await App.Connection.Table<AttachmentModel>().Where((Expression<Func<AttachmentModel, bool>>) (v => v.taskId == taskId && v.deleted != true)).ToListAsync()));
    }

    public static async Task<int> GetTaskAttachmentCount(string taskId)
    {
      return await Task.Run<int>((Func<Task<int>>) (async () => await App.Connection.Table<AttachmentModel>().Where((Expression<Func<AttachmentModel, bool>>) (v => v.taskId == taskId && v.deleted != true && v.status == 0)).CountAsync()));
    }

    public static async Task<List<AttachmentModel>> GetAllAttachments()
    {
      return await Task.Run<List<AttachmentModel>>((Func<Task<List<AttachmentModel>>>) (async () => await App.Connection.Table<AttachmentModel>().Where((Expression<Func<AttachmentModel, bool>>) (v => v.deleted != true)).ToListAsync()));
    }

    public static async Task<List<AttachmentModel>> GetNeedUpdateAttachments()
    {
      return await Task.Run<List<AttachmentModel>>((Func<Task<List<AttachmentModel>>>) (async () => await App.Connection.Table<AttachmentModel>().Where((Expression<Func<AttachmentModel, bool>>) (v => v.deleted != true && (v.sync_status == "0" || v.sync_status == "1"))).ToListAsync()));
    }

    public static async Task FakeDeleteAttachment(string id)
    {
      List<AttachmentModel> attachmentsById = await AttachmentDao.GetAttachmentsById(id);
      if (attachmentsById == null || attachmentsById.Count <= 0)
        return;
      foreach (AttachmentModel attachment in attachmentsById)
      {
        attachment.deleted = true;
        AttachmentCache.SetAttachment(attachment);
        int num = await App.Connection.UpdateAsync((object) attachment);
      }
    }

    public static async Task SaveServerMergeToDb(AttachmentSyncBean attachmentSyncBean)
    {
      foreach (AttachmentModel attachmentModel in attachmentSyncBean.Deleted)
        await AttachmentDao.FakeDeleteAttachment(attachmentModel.id);
      foreach (AttachmentModel attachment in attachmentSyncBean.Added)
        await AttachmentDao.InsertOrUpdateAttachment(attachment);
      foreach (AttachmentModel attachment in attachmentSyncBean.Updated)
        await AttachmentDao.UpdateAttachment(attachment);
      AttachmentCache.ResetDictItems();
    }

    public static async void CheckTaskAttachment(
      string taskId,
      List<AttachmentInfo> attachmentInfos)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      await AttachmentDao._asyncLocker.RunAsync((Func<Task>) (async () =>
      {
        string log = string.Empty;
        if (task != null && task.kind != "CHECKLIST")
        {
          List<AttachmentModel> attachments = await AttachmentDao.GetTaskAttachments(taskId);
          bool flag1 = false;
          foreach (AttachmentModel attachmentModel1 in attachments)
          {
            AttachmentModel attachment = attachmentModel1;
            if (!attachment.deleted)
            {
              string attachmentText = attachment.id;
              bool flag2 = attachmentInfos.Any<AttachmentInfo>((Func<AttachmentInfo, bool>) (info => info.Url.Contains(attachmentText)));
              if (attachment.status == 0 && !flag2)
              {
                attachment.status = 1;
                AttachmentModel attachmentModel2 = await Communicator.UpdateAttachment(attachment, task.id, task.projectId);
                if (attachmentModel2 != null)
                  attachment.path = attachmentModel2.path;
                else if (attachment.sync_status != "0")
                  attachment.sync_status = 1.ToString();
                log = log + "inactive attachmentId: " + attachment.id + " taskId:" + attachment.taskId + " \r\n";
                await AttachmentDao.InsertOrUpdateAttachment(attachment);
                flag1 = true;
              }
              else if (attachment.status == 1 & flag2)
              {
                attachment.status = 0;
                AttachmentModel attachmentModel3 = await Communicator.UpdateAttachment(attachment, task.id, task.projectId);
                if (attachmentModel3 != null)
                  attachment.path = attachmentModel3.path;
                else if (attachment.sync_status != "0")
                  attachment.sync_status = 1.ToString();
                log = log + "active attachmentId: " + attachment.id + " taskId:" + attachment.taskId + " \r\n";
                await AttachmentDao.InsertOrUpdateAttachment(attachment);
                flag1 = true;
              }
              attachment = (AttachmentModel) null;
            }
          }
          if (flag1)
          {
            await AttachmentCache.ResetDictItems();
            TaskChangeNotifier.NotifyAttachmentChanged(taskId, (object) null);
            int count = attachments.Count<AttachmentModel>((Func<AttachmentModel, bool>) (a => a.IsActive()));
            TaskDao.UpdateTaskAttachmentCount(task.id, count);
          }
          attachments = (List<AttachmentModel>) null;
        }
        if (string.IsNullOrEmpty(log))
        {
          log = (string) null;
        }
        else
        {
          UtilLog.Info(log);
          log = (string) null;
        }
      }));
    }

    public static async Task CopyAttachment(
      AttachmentModel otherAttachment,
      string newId,
      string editingTaskId)
    {
      AttachmentModel attachment = AttachmentModel.Copy(otherAttachment);
      attachment.taskId = editingTaskId;
      attachment.id = newId;
      attachment._Id = 0;
      attachment.deleted = false;
      attachment.status = 0;
      attachment.sync_status = 0.ToString();
      attachment.createdTime = new DateTime?(DateTime.Now);
      await AttachmentDao.InsertOrUpdateAttachment(attachment);
      await SyncStatusDao.AddSyncStatus(editingTaskId, 0);
    }

    public static async Task<bool> DownloadAttachmentDone(string id, AsyncCompletedEventArgs e)
    {
      AttachmentModel attachmentById = await AttachmentDao.GetAttachmentById(id);
      if (e.Error != null)
      {
        try
        {
          if (File.Exists(attachmentById.localPath))
          {
            FileInfo fileInfo = new FileInfo(attachmentById.localPath);
            fileInfo.Attributes = FileAttributes.Normal;
            fileInfo.Delete();
          }
        }
        catch (Exception ex)
        {
        }
        attachmentById.sync_status = 2.ToString();
        attachmentById.localPath = string.Empty;
        await AttachmentDao.UpdateAttachment(attachmentById);
        return false;
      }
      attachmentById.sync_status = 2.ToString();
      await AttachmentDao.UpdateAttachment(attachmentById);
      return true;
    }

    public static async Task<string> AddAttachmentStrings(string taskTaskId, string content)
    {
      if (TaskCache.GetTaskById(taskTaskId) == null)
        return content;
      List<AttachmentModel> taskAttachments = await AttachmentDao.GetTaskAttachments(taskTaskId, true);
      // ISSUE: explicit non-virtual call
      if (taskAttachments != null && __nonvirtual (taskAttachments.Count) > 0)
        content = AttachmentDao.AddAttachmentStrings(content, taskAttachments);
      return content;
    }

    public static string AddAttachmentStrings(string content, List<AttachmentModel> attachments)
    {
      content = content ?? string.Empty;
      attachments.Sort((Comparison<AttachmentModel>) ((a, b) => a.createdTime.HasValue && b.createdTime.HasValue ? b.createdTime.Value.CompareTo(a.createdTime.Value) : 0));
      content = content.Replace("\r\n", "\n").Replace("\r", "\n");
      foreach (AttachmentModel attachment in attachments)
      {
        if (!attachment.deleted && attachment.status == 0)
        {
          string msg = "![" + ((string.IsNullOrEmpty(attachment.fileType) ? AttachmentProvider.GetFileType(attachment.fileName).ToString() : attachment.fileType) == Constants.AttachmentKind.IMAGE.ToString() ? "image" : "file") + "](" + attachment.id + "/" + Utils.UrlEncode(attachment.fileName) + ")";
          if (!content.StartsWith(msg + "\n") && !content.EndsWith("\n" + msg) && !content.Contains("\n" + msg + "\n"))
          {
            UtilLog.Info(content.Replace("\n", "==="));
            UtilLog.Info(msg);
            content = string.IsNullOrEmpty(content) || content.EndsWith("\n") ? content + msg : content + "\n" + msg;
          }
        }
      }
      return content;
    }

    public static async Task HandleCommitResult(
      List<TaskModel> tasks,
      BatchUpdateResult updatedResult)
    {
      if (updatedResult == null)
        return;
      foreach (TaskModel task in tasks)
      {
        if (!updatedResult.Id2error.ContainsKey(task.id))
        {
          foreach (AttachmentModel attachment in task.Attachments)
            attachment.sync_status = "2";
          int num = await App.Connection.UpdateAllAsync((IEnumerable) task.Attachments);
        }
      }
    }

    public static AttachmentModel GetAttachmentInUrl(string url)
    {
      string[] strArray = url.Split('/');
      return strArray.Length != 0 ? AttachmentCache.GetAttachmentById(strArray[0]) : (AttachmentModel) null;
    }

    public static async Task SetAttachemntActive(
      string projectId,
      string taskId,
      string attachmentId,
      int active)
    {
      AttachmentModel attachmentById = await AttachmentDao.GetAttachmentById(attachmentId);
      attachmentById.status = active;
      AttachmentModel attachmentModel = await Communicator.UpdateAttachment(attachmentById, taskId, projectId);
    }

    public static async void DeleteAttachmentByTaskIdAsync(string taskId)
    {
    }
  }
}
