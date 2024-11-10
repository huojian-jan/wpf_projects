// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Attachment.AttachmentLoadHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Files;
using ticktick_WPF.Views.Detail;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Attachment
{
  public static class AttachmentLoadHelper
  {
    private static readonly HashSet<string> UploadingIds = new HashSet<string>();
    private static readonly HashSet<string> DownloadingIds = new HashSet<string>();
    private static readonly HashSet<string> DownloadFailedIds = new HashSet<string>();
    private static readonly HashSet<string> UploadFailedIds = new HashSet<string>();
    private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

    public static event EventHandler<string> Downloaded;

    public static event EventHandler<string> DownloadFailed;

    public static event EventHandler<string> Uploaded;

    public static event EventHandler<string> UploadFailed;

    public static event EventHandler<AttachmentProgressModel> DownloadProgressChanged;

    public static event EventHandler<AttachmentProgressModel> UploadProgressChanged;

    public static OptTypeEnum AttachmentStatus(AttachmentModel attachment, bool enable = true)
    {
      return ThreadUtil.ReadLock<OptTypeEnum>(AttachmentLoadHelper._lock, (Func<OptTypeEnum>) (() =>
      {
        if (attachment == null || !enable)
          return OptTypeEnum.None;
        if (AttachmentLoadHelper.DownloadFailedIds.Contains(attachment.id))
          return OptTypeEnum.DownloadFailed;
        if (AttachmentLoadHelper.UploadFailedIds.Contains(attachment.id))
          return OptTypeEnum.UploadFailed;
        if (AttachmentLoadHelper.DownloadingIds.Contains(attachment.id) || AttachmentLoadHelper.UploadingIds.Contains(attachment.id))
          return OptTypeEnum.Loading;
        if (string.IsNullOrWhiteSpace(attachment.path) && !FileUtils.FileEmptyOrNotExists(attachment.localPath) && !AttachmentLoadHelper.UploadingIds.Contains(attachment.id))
          return OptTypeEnum.Upload;
        return !AttachmentLoadHelper.DownloadingIds.Contains(attachment.id) && FileUtils.FileEmptyOrNotExists(attachment.localPath) ? OptTypeEnum.Download : OptTypeEnum.Menu;
      }));
    }

    public static bool IsFailed(string id)
    {
      return ThreadUtil.ReadLock<bool>(AttachmentLoadHelper._lock, (Func<bool>) (() => AttachmentLoadHelper.IsFailedInternal(id)));
    }

    public static bool IsLoading(string id)
    {
      return ThreadUtil.ReadLock<bool>(AttachmentLoadHelper._lock, (Func<bool>) (() => AttachmentLoadHelper.IsLoadingInternal(id)));
    }

    private static bool IsFailedInternal(string id)
    {
      return AttachmentLoadHelper.DownloadFailedIds.Contains(id) || AttachmentLoadHelper.UploadFailedIds.Contains(id);
    }

    private static bool IsLoadingInternal(string id)
    {
      return AttachmentLoadHelper.DownloadingIds.Contains(id) || AttachmentLoadHelper.UploadingIds.Contains(id);
    }

    public static async void OnDownloadCompleted(
      object sender,
      AttachmentDownloadSuccessModel model)
    {
      AttachmentModel attachment = AttachmentCache.GetAttachmentById(model.AttachmentId);
      string localPath = AttachmentDownloadUtils.GetLocalPath(attachment);
      if (attachment == null)
        attachment = (AttachmentModel) null;
      else if (!model.Success)
      {
        try
        {
          if (System.IO.File.Exists(localPath))
          {
            FileInfo fileInfo = new FileInfo(localPath);
            fileInfo.Attributes = FileAttributes.Normal;
            fileInfo.Delete();
          }
        }
        catch (Exception ex)
        {
        }
        finally
        {
          attachment.localPath = (string) null;
          await AttachmentDao.UpdateAttachment(attachment);
          AttachmentLoadHelper.NotifyDownloadFailed(attachment.id);
        }
        attachment = (AttachmentModel) null;
      }
      else
      {
        attachment.localPath = localPath;
        attachment.sync_status = 2.ToString();
        await AttachmentDao.UpdateAttachment(attachment);
        AttachmentLoadHelper.NotifyDownloadCompleted(attachment.id);
        attachment = (AttachmentModel) null;
      }
    }

    public static void OnDownloadProgressChanged(object sender, AttachmentProgressModel e)
    {
      if (AttachmentCache.GetAttachmentById(e.AttachmentId) == null)
        return;
      AttachmentLoadHelper.NotifyDownloading(e);
    }

    public static void OnUploadCompleted(object sender, UploadFileCompletedEventArgs e)
    {
      App.Instance.Dispatcher.Invoke<Task>((Func<Task>) (async () =>
      {
        if (!(sender is WebClient webClient2))
          return;
        string[] values = webClient2.Headers.GetValues("attachmentId");
        if (values == null || values.Length == 0)
          return;
        AttachmentLoadHelper.UploadingIds.Remove(values[0]);
        if (e.Error != null)
        {
          Utils.Toast(Utils.GetString("UploadFailed"));
          SyncManager.Sync();
          AttachmentLoadHelper.NotifyUploadFailed(values[0]);
          UtilLog.Info("AttachmentUploadFailed " + values[0]);
        }
        else
        {
          UtilLog.Info("AttachmentUploadSuccess " + values[0]);
          AttachmentLoadHelper.NotifyUploadCompleted(values[0]);
        }
      }));
    }

    public static void OnUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
    {
      App.Instance.Dispatcher.Invoke((Action) (() =>
      {
        if (!(sender is WebClient webClient2))
          return;
        string[] values = webClient2.Headers.GetValues("attachmentId");
        if (values == null || values.Length == 0)
          return;
        AttachmentModel attachmentById = AttachmentCache.GetAttachmentById(values[0]);
        if (attachmentById == null)
          return;
        AttachmentLoadHelper.NotifyUploading(new AttachmentProgressModel()
        {
          AttachmentId = attachmentById.id,
          ProgressPercentage = e.ProgressPercentage
        });
      }));
    }

    private static void RemoveAll(string id, AttachmentLoadingType type)
    {
      if (type != AttachmentLoadingType.UploadFailed && AttachmentLoadHelper.UploadFailedIds.Contains(id))
        AttachmentLoadHelper.UploadFailedIds.Remove(id);
      if (type != AttachmentLoadingType.DownloadingFailed && AttachmentLoadHelper.DownloadFailedIds.Contains(id))
        AttachmentLoadHelper.DownloadFailedIds.Remove(id);
      if (type != AttachmentLoadingType.Uploading && AttachmentLoadHelper.UploadingIds.Contains(id))
        AttachmentLoadHelper.UploadingIds.Remove(id);
      if (type == AttachmentLoadingType.Downloading || !AttachmentLoadHelper.DownloadingIds.Contains(id))
        return;
      AttachmentLoadHelper.DownloadingIds.Remove(id);
    }

    public static void ClearFailed()
    {
      ThreadUtil.WriteLock(AttachmentLoadHelper._lock, (Action) (() =>
      {
        AttachmentLoadHelper.UploadFailedIds.Clear();
        AttachmentLoadHelper.DownloadFailedIds.Clear();
      }));
    }

    public static bool StartDownload(string id)
    {
      int num = ThreadUtil.WriteLock<bool>(AttachmentLoadHelper._lock, (Func<bool>) (() =>
      {
        if (AttachmentLoadHelper.IsLoadingInternal(id))
          return false;
        AttachmentLoadHelper.RemoveAll(id, AttachmentLoadingType.Downloading);
        AttachmentLoadHelper.DownloadingIds.Add(id);
        return true;
      })) ? 1 : 0;
      if (num == 0)
        return num != 0;
      ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        EventHandler<AttachmentProgressModel> downloadProgressChanged = AttachmentLoadHelper.DownloadProgressChanged;
        if (downloadProgressChanged == null)
          return;
        downloadProgressChanged((object) null, new AttachmentProgressModel()
        {
          AttachmentId = id,
          ProgressPercentage = 0
        });
      }));
      return num != 0;
    }

    public static bool StartUpload(string id)
    {
      int num = ThreadUtil.WriteLock<bool>(AttachmentLoadHelper._lock, (Func<bool>) (() =>
      {
        if (AttachmentLoadHelper.IsLoadingInternal(id))
          return false;
        AttachmentLoadHelper.RemoveAll(id, AttachmentLoadingType.Uploading);
        AttachmentLoadHelper.UploadingIds.Add(id);
        return true;
      })) ? 1 : 0;
      if (num == 0)
        return num != 0;
      App.Instance.Dispatcher.Invoke((Action) (() =>
      {
        EventHandler<AttachmentProgressModel> uploadProgressChanged = AttachmentLoadHelper.UploadProgressChanged;
        if (uploadProgressChanged == null)
          return;
        uploadProgressChanged((object) null, new AttachmentProgressModel()
        {
          AttachmentId = id,
          ProgressPercentage = 0
        });
      }));
      return num != 0;
    }

    public static void NotifyUploadFailed(string id)
    {
      ThreadUtil.WriteLock(AttachmentLoadHelper._lock, (Action) (() =>
      {
        AttachmentLoadHelper.RemoveAll(id, AttachmentLoadingType.UploadFailed);
        AttachmentLoadHelper.UploadFailedIds.Add(id);
      }));
      ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        EventHandler<string> uploadFailed = AttachmentLoadHelper.UploadFailed;
        if (uploadFailed == null)
          return;
        uploadFailed((object) null, id);
      }));
    }

    public static void NotifyDownloadFailed(string id)
    {
      ThreadUtil.WriteLock(AttachmentLoadHelper._lock, (Action) (() =>
      {
        AttachmentLoadHelper.RemoveAll(id, AttachmentLoadingType.DownloadingFailed);
        AttachmentLoadHelper.DownloadFailedIds.Add(id);
      }));
      ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        EventHandler<string> downloadFailed = AttachmentLoadHelper.DownloadFailed;
        if (downloadFailed == null)
          return;
        downloadFailed((object) null, id);
      }));
    }

    public static void NotifyDownloadCompleted(string id)
    {
      ThreadUtil.WriteLock(AttachmentLoadHelper._lock, (Action) (() => AttachmentLoadHelper.RemoveAll(id, AttachmentLoadingType.None)));
      ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        EventHandler<string> downloaded = AttachmentLoadHelper.Downloaded;
        if (downloaded == null)
          return;
        downloaded((object) null, id);
      }));
    }

    public static void NotifyUploadCompleted(string id)
    {
      ThreadUtil.WriteLock(AttachmentLoadHelper._lock, (Action) (() => AttachmentLoadHelper.RemoveAll(id, AttachmentLoadingType.None)));
      ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        EventHandler<string> uploaded = AttachmentLoadHelper.Uploaded;
        if (uploaded == null)
          return;
        uploaded((object) null, id);
      }));
    }

    public static void NotifyUploading(AttachmentProgressModel model)
    {
      ThreadUtil.WriteLock(AttachmentLoadHelper._lock, (Action) (() => AttachmentLoadHelper.RemoveAll(model.AttachmentId, AttachmentLoadingType.Uploading)));
      ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        EventHandler<AttachmentProgressModel> uploadProgressChanged = AttachmentLoadHelper.UploadProgressChanged;
        if (uploadProgressChanged == null)
          return;
        uploadProgressChanged((object) null, model);
      }));
    }

    public static void NotifyDownloading(AttachmentProgressModel model)
    {
      ThreadUtil.WriteLock(AttachmentLoadHelper._lock, (Action) (() => AttachmentLoadHelper.RemoveAll(model.AttachmentId, AttachmentLoadingType.Downloading)));
      ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        EventHandler<AttachmentProgressModel> downloadProgressChanged = AttachmentLoadHelper.DownloadProgressChanged;
        if (downloadProgressChanged == null)
          return;
        downloadProgressChanged((object) null, model);
      }));
    }
  }
}
