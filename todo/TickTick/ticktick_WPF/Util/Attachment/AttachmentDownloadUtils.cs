// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Attachment.AttachmentDownloadUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Files;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util.Attachment
{
  public static class AttachmentDownloadUtils
  {
    public static string GetLocalPath(AttachmentModel attachment)
    {
      if (attachment == null)
        return string.Empty;
      string localPath;
      if (attachment.fileType == Constants.AttachmentKind.IMAGE.ToString())
      {
        string str = ((IEnumerable<string>) attachment.fileName.Split('.')).Last<string>();
        localPath = AppPaths.ImageDir + attachment.refId + "." + str;
      }
      else
        localPath = AppPaths.ImageDir + attachment.refId + FileUtils.ToValidFileName(attachment.fileName);
      return localPath;
    }

    public static async Task AutoDownloadImgAttachment(AttachmentModel item)
    {
      TaskModel taskById = await TaskDao.GetTaskById(item.taskId);
      if (taskById == null)
        return;
      await AttachmentDownloadUtils.CheckThenDownload(item, taskById, AttachmentDownloadUtils.GetLocalPath(item));
    }

    public static async Task DownloadFileAttachment(string fileId)
    {
      AttachmentModel fileModel;
      if (string.IsNullOrEmpty(fileId))
      {
        fileModel = (AttachmentModel) null;
      }
      else
      {
        fileModel = AttachmentCache.GetAttachmentById(fileId);
        if (fileModel == null)
        {
          fileModel = (AttachmentModel) null;
        }
        else
        {
          if (!string.IsNullOrEmpty(fileModel.refId) && fileModel.refId != fileModel.id)
          {
            AttachmentModel attachmentById = await AttachmentDao.GetAttachmentById(fileModel.refId);
            if (attachmentById != null)
              fileModel.path = attachmentById.path;
          }
          if (string.IsNullOrEmpty(fileModel.path))
          {
            Utils.Toast(Utils.GetString("DownloadFailedCauseFileNotUpload"));
            fileModel = (AttachmentModel) null;
          }
          else
          {
            TaskModel taskById = await TaskDao.GetTaskById(fileModel.taskId);
            if (taskById == null)
            {
              fileModel = (AttachmentModel) null;
            }
            else
            {
              await AttachmentDownloadUtils.CheckThenDownload(fileModel, taskById, AttachmentDownloadUtils.GetLocalPath(fileModel));
              fileModel = (AttachmentModel) null;
            }
          }
        }
      }
    }

    private static async Task CheckThenDownload(
      AttachmentModel item,
      TaskModel task,
      string filePath)
    {
      if (item == null || task == null)
        return;
      if (!Utils.IsNetworkAvailable())
      {
        AttachmentLoadHelper.NotifyDownloadFailed(item.id);
        Utils.Toast(Utils.GetString("DownloadAttachmentNetworkError"));
      }
      else
      {
        if (!string.IsNullOrEmpty(item.refId) && item.refId != item.id)
        {
          AttachmentModel attachmentById = await AttachmentDao.GetAttachmentById(item.refId);
          if (attachmentById != null)
            item.path = attachmentById.path;
        }
        if (string.IsNullOrEmpty(item.path))
        {
          AttachmentLoadHelper.NotifyDownloadFailed(item.id);
          Utils.Toast(Utils.GetString("DownloadFailedCauseFileNotUpload"));
        }
        else if (!FileUtils.FileEmptyOrNotExists(filePath))
          AttachmentLoadHelper.OnDownloadCompleted((object) null, new AttachmentDownloadSuccessModel()
          {
            AttachmentId = item.id,
            Success = true
          });
        else
          await AttachmentDownloadUtils.DownloadFile(task.projectId, task.id, item.id, filePath);
      }
    }

    public static async Task DownloadFile(
      string projectId,
      string taskId,
      string attachmentId,
      string locationPath)
    {
      if (!Utils.IsNetworkAvailable())
      {
        AttachmentLoadHelper.NotifyDownloadFailed(attachmentId);
        Utils.Toast(Utils.GetString("DownloadAttachmentNetworkError"));
      }
      else
      {
        try
        {
          if (!AttachmentLoadHelper.StartDownload(attachmentId))
            return;
          FileDownloader fileDownloader = new FileDownloader(BaseUrl.GetApiDomain() + string.Format("/api/v1/attachment/{0}/{1}/{2}", (object) projectId, (object) taskId, (object) attachmentId), locationPath, attachmentId);
          fileDownloader.DownloadFileCompleted += new EventHandler<AttachmentDownloadSuccessModel>(AttachmentLoadHelper.OnDownloadCompleted);
          fileDownloader.DownloadFileCompleted += (EventHandler<AttachmentDownloadSuccessModel>) ((s, e) =>
          {
            if (e.Success)
              return;
            Utils.Toast(Utils.GetString("DownloadAttachmentNetworkError"));
          });
          fileDownloader.DownloadProgressChanged += new EventHandler<AttachmentProgressModel>(AttachmentLoadHelper.OnDownloadProgressChanged);
          fileDownloader.StartDownloadAsync();
        }
        catch (Exception ex)
        {
          AttachmentLoadHelper.NotifyDownloadFailed(attachmentId);
          int num = (int) MessageBox.Show(ex.Message);
        }
      }
    }
  }
}
