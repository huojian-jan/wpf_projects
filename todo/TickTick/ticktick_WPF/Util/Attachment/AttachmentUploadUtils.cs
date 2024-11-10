// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Attachment.AttachmentUploadUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Files;
using ticktick_WPF.Util.Network;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Attachment
{
  public static class AttachmentUploadUtils
  {
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(5);
    private static readonly HashSet<string> UploadedIds = new HashSet<string>();

    static AttachmentUploadUtils()
    {
      NetworkChange.NetworkAvailabilityChanged -= new NetworkAvailabilityChangedEventHandler(AttachmentUploadUtils.OnNetWorkChanged);
      NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(AttachmentUploadUtils.OnNetWorkChanged);
    }

    public static async Task UpFile(
      string projectId,
      string taskId,
      AttachmentModel attachment,
      IToastShowWindow toastWindow,
      bool ignoreUploadCollect = false)
    {
      if (!Utils.IsNetworkAvailable())
      {
        AttachmentLoadHelper.NotifyUploadFailed(attachment.id);
        toastWindow?.TryToastString((object) null, Utils.GetString("UploadAttachmentNetworkError"));
      }
      else
      {
        if (!ignoreUploadCollect)
          FileUtils.CollectFileSize(attachment.localPath);
        if (FileUtils.FileOverSize(attachment.localPath))
        {
          toastWindow?.TryToastString((object) null, LocalSettings.Settings.IsPro ? Utils.GetString("AttachmentSizeLimitPro") : Utils.GetString("AttachmentSizeLimit"));
          AttachmentLoadHelper.NotifyUploadFailed(attachment.id);
        }
        else
        {
          string attachmentId = attachment.id;
          if (!AttachmentLoadHelper.StartUpload(attachmentId))
            return;
          string uploadUri = BaseUrl.GetApiDomain() + string.Format("/api/v1/attachment/upload/{0}/{1}/{2}", (object) projectId, (object) taskId, (object) attachmentId);
          string file = attachment.localPath;
          string fileName = string.IsNullOrEmpty(attachment.fileName) ? Path.GetFileName(file) : attachment.fileName;
          Task.Run((Func<Task>) (async () =>
          {
            Stopwatch watch = Stopwatch.StartNew();
            try
            {
              await AttachmentUploadUtils._semaphore.WaitAsync();
              Task<AttachmentModel> task = ApiClient.SendFileAsync<AttachmentModel>(uploadUri, (file, fileName));
              UtilRun.OnProgressAsync((Task) task, (Action<ProgressEventArgs>) (args => AttachmentLoadHelper.NotifyUploading(new AttachmentProgressModel()
              {
                AttachmentId = attachment.id,
                ProgressPercentage = args.Progress
              })));
              await AttachmentDao.UpdateUploadAttachment(await task);
              watch.Stop();
              UtilLog.Info("upload success (" + watch.ElapsedMilliseconds.ToString() + "ms) uri=" + uploadUri);
              AttachmentLoadHelper.NotifyUploadCompleted(attachmentId);
            }
            catch (ApiException ex)
            {
              UtilLog.Error("upload UploadFailed (" + watch.ElapsedMilliseconds.ToString() + "ms) uri=" + uploadUri + ", ex: " + ex?.ToString());
              if (ex.ErrorCode.ToLower() == "no_project_permission")
              {
                UtilLog.Info("upload no_project_permission uri=" + uploadUri);
                attachment.sync_status = 2.ToString();
                await AttachmentDao.UpdateAttachment(attachment);
                AttachmentCache.SetTodayAttachmentCount(AttachmentCache.TodayAttachmentCount() - 1);
                AttachmentLoadHelper.NotifyUploadCompleted(attachmentId);
              }
              else
                AttachmentLoadHelper.NotifyUploadFailed(attachmentId);
              if (ex.ErrorCode.ToLower() == "exceed_quota")
                AttachmentCache.SetTodayAttachmentCount((int) LimitCache.GetLimitByKey(Constants.LimitKind.DailyUploadNumber));
            }
            catch (Exception ex)
            {
              UtilLog.Error("upload UploadFailed (" + watch.ElapsedMilliseconds.ToString() + "ms) uri=" + uploadUri + ", ex: " + ex?.ToString());
              AttachmentLoadHelper.NotifyUploadFailed(attachmentId);
            }
            finally
            {
              watch.Stop();
              AttachmentUploadUtils._semaphore.Release();
            }
            watch = (Stopwatch) null;
          }));
        }
      }
    }

    public static bool CompressImage(AttachmentModel attachment)
    {
      if (!(attachment.fileType == Constants.AttachmentKind.IMAGE.ToString()))
        return true;
      string oldValue = "." + ((IEnumerable<string>) attachment.localPath.Split('.')).Last<string>().ToLower();
      int flag = oldValue.EndsWith("png") ? 90 : 80;
      string str = attachment.localPath.Replace(oldValue, "_compress" + oldValue);
      FileInfo fileInfo1 = new FileInfo(attachment.localPath);
      long userLimit = Utils.GetUserLimit(Constants.LimitKind.AttachmentSize);
      if (fileInfo1.Length <= userLimit / 10L || !(str != attachment.localPath) || !AttachmentUploadUtils.CompressImage(attachment.localPath, str, flag, 1024) || !File.Exists(str))
        return true;
      FileInfo fileInfo2 = new FileInfo(str);
      if (fileInfo2.Length <= 0L || fileInfo2.Length > fileInfo1.Length)
      {
        try
        {
          fileInfo2.Attributes = FileAttributes.Normal;
          fileInfo2.Delete();
        }
        catch (Exception ex)
        {
        }
      }
      else
      {
        attachment.size = (int) fileInfo2.Length;
        try
        {
          fileInfo1.Attributes = FileAttributes.Normal;
          fileInfo1.Delete();
          fileInfo2.CopyTo(attachment.localPath);
          fileInfo2.Attributes = FileAttributes.Normal;
          fileInfo2.Delete();
        }
        catch (Exception ex)
        {
          attachment.localPath = str;
        }
      }
      return new FileInfo(attachment.localPath).Length <= userLimit;
    }

    private static bool CompressImage(string sFile, string dFile, int flag = 95, int size = 512)
    {
      Image image = Image.FromFile(sFile);
      ImageFormat rawFormat = image.RawFormat;
      int width = image.Width;
      int height = image.Height;
      Bitmap bitmap = new Bitmap(width, height);
      Graphics graphics = Graphics.FromImage((Image) bitmap);
      graphics.Clear(Color.WhiteSmoke);
      graphics.CompositingQuality = CompositingQuality.HighQuality;
      graphics.SmoothingMode = SmoothingMode.HighQuality;
      graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
      graphics.DrawImage(image, new Rectangle(0, 0, width, height), 0, 0, width, height, GraphicsUnit.Pixel);
      graphics.Dispose();
      EncoderParameters encoderParams = new EncoderParameters();
      long[] numArray = new long[1]{ (long) flag };
      EncoderParameter encoderParameter = new EncoderParameter(Encoder.Quality, numArray);
      encoderParams.Param[0] = encoderParameter;
      try
      {
        ImageCodecInfo[] imageEncoders = ImageCodecInfo.GetImageEncoders();
        string suf = ((IEnumerable<string>) sFile.Split('.')).Last<string>().ToLower();
        string str = suf;
        if (str != null)
        {
          switch (str.Length)
          {
            case 3:
              switch (str[0])
              {
                case 'b':
                  if (str == "bmp")
                  {
                    suf = "BMP";
                    goto label_18;
                  }
                  else
                    goto label_18;
                case 'g':
                  if (str == "gif")
                    return true;
                  goto label_18;
                case 'j':
                  if (str == "jpg")
                    break;
                  goto label_18;
                case 'p':
                  if (str == "png")
                  {
                    suf = "PNG";
                    goto label_18;
                  }
                  else
                    goto label_18;
                default:
                  goto label_18;
              }
              break;
            case 4:
              switch (str[0])
              {
                case 'j':
                  if (str == "jpeg")
                    break;
                  goto label_18;
                case 't':
                  if (str == "tiff")
                  {
                    suf = "TIFF";
                    goto label_18;
                  }
                  else
                    goto label_18;
                case 'w':
                  if (str == "webp")
                  {
                    suf = "WEBP";
                    goto label_18;
                  }
                  else
                    goto label_18;
                default:
                  goto label_18;
              }
              break;
            default:
              goto label_18;
          }
          suf = "JPEG";
        }
label_18:
        ImageCodecInfo encoder = ((IEnumerable<ImageCodecInfo>) imageEncoders).FirstOrDefault<ImageCodecInfo>((Func<ImageCodecInfo, bool>) (t => t.FormatDescription.Equals(suf)));
        if (encoder != null)
          bitmap.Save(dFile, encoder, encoderParams);
        else
          bitmap.Save(dFile, rawFormat);
        return true;
      }
      catch
      {
        return false;
      }
      finally
      {
        image.Dispose();
        bitmap.Dispose();
        GC.Collect();
      }
    }

    public static async Task UploadAllFiles()
    {
      List<AttachmentModel> allAttachments = await AttachmentDao.GetAllAttachments();
      List<AttachmentModel> list = allAttachments != null ? allAttachments.Where<AttachmentModel>((Func<AttachmentModel, bool>) (att => att.status == 0 && !AttachmentUploadUtils.UploadedIds.Contains(att.id) && !string.IsNullOrEmpty(att.localPath) && File.Exists(att.localPath) && string.IsNullOrEmpty(att.path) && !AttachmentLoadHelper.IsLoading(att.id))).ToList<AttachmentModel>() : (List<AttachmentModel>) null;
      if (list == null || list.Count == 0)
        return;
      foreach (AttachmentModel attachmentModel in list)
      {
        AttachmentModel attachment = attachmentModel;
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(attachment.taskId);
        if (thinTaskById != null && thinTaskById.deleted == 0)
        {
          AttachmentUploadUtils.UploadedIds.Add(attachment.id);
          AttachmentUploadUtils.UpFile(thinTaskById.projectId, attachment.taskId, attachment, (IToastShowWindow) null);
          attachment = (AttachmentModel) null;
        }
      }
    }

    private static async void OnNetWorkChanged(object sender, NetworkAvailabilityEventArgs e)
    {
      if (!e.IsAvailable)
        return;
      await Task.Delay(6000);
      AttachmentUploadUtils.UploadedIds.Clear();
    }
  }
}
