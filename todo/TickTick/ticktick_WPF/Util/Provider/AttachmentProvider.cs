// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.AttachmentProvider
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Attachment;
using ticktick_WPF.Util.Files;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.QuickAdd;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class AttachmentProvider : IDisposable
  {
    private readonly List<AttachmentViewModel> _displayAttachmentList = new List<AttachmentViewModel>();
    private readonly AttachmentComparer _attachmentComparer = new AttachmentComparer();

    public string TaskId { get; set; }

    public void UpdateModels(
      List<AttachmentModel> models,
      int imageMode,
      out List<string> addIds,
      out List<string> outRemoveIds)
    {
      List<string> removeIds = this._displayAttachmentList.Where<AttachmentViewModel>((Func<AttachmentViewModel, bool>) (vm => !models.Exists((Predicate<AttachmentModel>) (model => vm.Id == model.id)))).Select<AttachmentViewModel, string>((Func<AttachmentViewModel, string>) (vm => vm.Id)).ToList<string>();
      this._displayAttachmentList.RemoveAll((Predicate<AttachmentViewModel>) (vm => removeIds.Contains(vm.Id)));
      List<AttachmentModel> source = new List<AttachmentModel>();
      foreach (AttachmentModel model1 in models)
      {
        AttachmentModel model = model1;
        AttachmentViewModel attachmentViewModel = this._displayAttachmentList.FirstOrDefault<AttachmentViewModel>((Func<AttachmentViewModel, bool>) (item => item.Id == model.id));
        if (attachmentViewModel != null)
          attachmentViewModel.UpdateProperty(model, imageMode);
        else
          source.Add(model);
      }
      source.ForEach(new Action<AttachmentModel>(this.DealWithAttachment));
      this._displayAttachmentList.AddRange(source.Select<AttachmentModel, AttachmentViewModel>((Func<AttachmentModel, AttachmentViewModel>) (model => new AttachmentViewModel(model, imageMode))));
      this._displayAttachmentList.Sort((IComparer<AttachmentViewModel>) this._attachmentComparer);
      addIds = source.Select<AttachmentModel, string>((Func<AttachmentModel, string>) (item => item.id)).ToList<string>();
      outRemoveIds = removeIds;
    }

    public AttachmentProvider(IEnumerable<AttachmentModel> attachmentModelList, int imageMode)
    {
      foreach (AttachmentModel attachmentModel in attachmentModelList)
      {
        this.DealWithAttachment(attachmentModel);
        this._displayAttachmentList.Add(new AttachmentViewModel(attachmentModel, imageMode));
      }
      this._displayAttachmentList.Sort((IComparer<AttachmentViewModel>) this._attachmentComparer);
      AttachmentLoadHelper.Downloaded += new EventHandler<string>(this.OnAttachmentDownloaded);
      AttachmentLoadHelper.DownloadFailed += new EventHandler<string>(this.OnAttachmentDownloadFailed);
      AttachmentLoadHelper.DownloadProgressChanged += new EventHandler<AttachmentProgressModel>(this.OnDownloadProgressChanged);
      AttachmentLoadHelper.Uploaded += new EventHandler<string>(this.OnAttachmentUploaded);
      AttachmentLoadHelper.UploadFailed += new EventHandler<string>(this.OnAttachmentUploadFailed);
      AttachmentLoadHelper.UploadProgressChanged += new EventHandler<AttachmentProgressModel>(this.OnUploadProgressChanged);
    }

    private void DealWithAttachment(AttachmentModel item)
    {
      try
      {
        if (item.fileType == "IMAGE")
        {
          if (FileUtils.FileEmptyOrNotExists(item.localPath))
            AttachmentDownloadUtils.AutoDownloadImgAttachment(item);
        }
      }
      catch (Exception ex)
      {
      }
      if (!(item.fileType == "IMAGE") || !FileUtils.FileExists(item.localPath) || FileUtils.FileInFolder(item.localPath, AppPaths.ImageDir))
        return;
      string str = ((IEnumerable<string>) item.fileName.Split('.')).Last<string>();
      string destFileName = AppPaths.ImageDir + item.id + "." + str;
      try
      {
        File.Copy(item.localPath, destFileName, true);
        item.localPath = destFileName;
        AttachmentDao.UpdateAttachment(item);
      }
      catch (Exception ex)
      {
      }
    }

    public void Dispose()
    {
      AttachmentLoadHelper.Downloaded -= new EventHandler<string>(this.OnAttachmentDownloaded);
      AttachmentLoadHelper.DownloadFailed -= new EventHandler<string>(this.OnAttachmentDownloadFailed);
      AttachmentLoadHelper.DownloadProgressChanged -= new EventHandler<AttachmentProgressModel>(this.OnDownloadProgressChanged);
      AttachmentLoadHelper.Uploaded -= new EventHandler<string>(this.OnAttachmentUploaded);
      AttachmentLoadHelper.UploadFailed -= new EventHandler<string>(this.OnAttachmentUploadFailed);
      AttachmentLoadHelper.UploadProgressChanged -= new EventHandler<AttachmentProgressModel>(this.OnUploadProgressChanged);
    }

    public List<AttachmentViewModel> GetModels()
    {
      return this._displayAttachmentList.ToList<AttachmentViewModel>();
    }

    private AttachmentViewModel GetModelById(string id)
    {
      return this._displayAttachmentList.FirstOrDefault<AttachmentViewModel>((Func<AttachmentViewModel, bool>) (m => m.Id == id));
    }

    private void OnUploadProgressChanged(object sender, AttachmentProgressModel e)
    {
      this.GetModelById(e.AttachmentId)?.SetFileStatus(e.ProgressPercentage >= 100 ? OptTypeEnum.None : OptTypeEnum.Loading);
    }

    private void OnAttachmentUploadFailed(object sender, string e)
    {
      this.GetModelById(e)?.SetFileStatus(OptTypeEnum.UploadFailed);
    }

    private void OnAttachmentUploaded(object sender, string e)
    {
      this.GetModelById(e)?.SetFileStatus(OptTypeEnum.None);
    }

    private void OnDownloadProgressChanged(object sender, AttachmentProgressModel e)
    {
      this.GetModelById(e.AttachmentId)?.SetFileStatus(e.ProgressPercentage >= 100 ? OptTypeEnum.None : OptTypeEnum.Loading);
    }

    private void OnAttachmentDownloadFailed(object sender, string e)
    {
      this.GetModelById(e)?.SetFileStatus(OptTypeEnum.Loading);
    }

    private void OnAttachmentDownloaded(object sender, string e)
    {
      AttachmentViewModel attachmentViewModel = this._displayAttachmentList.FirstOrDefault<AttachmentViewModel>((Func<AttachmentViewModel, bool>) (a => a.Id == e));
      if (attachmentViewModel == null || e == null)
        return;
      AttachmentModel attachmentById = AttachmentCache.GetAttachmentById(e);
      attachmentViewModel.LocalPath = attachmentById.localPath;
      attachmentViewModel.SetFileStatus(OptTypeEnum.Menu);
    }

    public static Constants.AttachmentKind GetFileType(string fileName)
    {
      string lower = ((IEnumerable<string>) fileName.Split('.')).Last<string>().ToLower();
      if (lower != null)
      {
        switch (lower.Length)
        {
          case 2:
            if (lower == "7z")
              goto label_40;
            else
              goto label_41;
          case 3:
            switch (lower[0])
            {
              case '3':
                if (lower == "3gp")
                  goto label_32;
                else
                  goto label_41;
              case 'a':
                switch (lower)
                {
                  case "amr":
                    goto label_32;
                  case "avi":
                    break;
                  default:
                    goto label_41;
                }
                break;
              case 'b':
                if (lower == "bmp")
                  goto label_31;
                else
                  goto label_41;
              case 'c':
                if (lower == "csv")
                  return Constants.AttachmentKind.CSV;
                goto label_41;
              case 'd':
                if (lower == "doc")
                  goto label_35;
                else
                  goto label_41;
              case 'g':
                if (lower == "gif")
                  goto label_31;
                else
                  goto label_41;
              case 'j':
                if (lower == "jpg")
                  goto label_31;
                else
                  goto label_41;
              case 'm':
                switch (lower)
                {
                  case "mp3":
                  case "m4a":
                  case "m4r":
                    goto label_32;
                  case "mov":
                  case "mp4":
                    break;
                  default:
                    goto label_41;
                }
                break;
              case 'o':
                switch (lower)
                {
                  case "odt":
                    goto label_35;
                  case "ods":
                    goto label_37;
                  case "odp":
                    goto label_39;
                  default:
                    goto label_41;
                }
              case 'p':
                switch (lower)
                {
                  case "png":
                    goto label_31;
                  case "pdf":
                    return Constants.AttachmentKind.PDF;
                  case "ppt":
                  case "pps":
                    goto label_39;
                  default:
                    goto label_41;
                }
              case 'r':
                switch (lower)
                {
                  case "rtf":
                    goto label_36;
                  case "rar":
                    goto label_40;
                  default:
                    goto label_41;
                }
              case 't':
                if (lower == "txt")
                  goto label_36;
                else
                  goto label_41;
              case 'w':
                if (lower == "wps")
                  goto label_35;
                else
                  goto label_41;
              case 'x':
                if (lower == "xls")
                  goto label_37;
                else
                  goto label_41;
              case 'z':
                if (lower == "zip")
                  goto label_40;
                else
                  goto label_41;
              default:
                goto label_41;
            }
            return Constants.AttachmentKind.VIDEO;
label_36:
            return Constants.AttachmentKind.TEXT;
          case 4:
            switch (lower[0])
            {
              case '3':
                if (lower == "3gpp")
                  goto label_32;
                else
                  goto label_41;
              case 'd':
                if (lower == "docx")
                  goto label_35;
                else
                  goto label_41;
              case 'j':
                if (lower == "jpeg")
                  break;
                goto label_41;
              case 'p':
                if (lower == "pptx" || lower == "ppsx")
                  goto label_39;
                else
                  goto label_41;
              case 't':
                if (lower == "tiff")
                  break;
                goto label_41;
              case 'w':
                if (lower == "webp")
                  break;
                goto label_41;
              case 'x':
                if (lower == "xlsx")
                  goto label_37;
                else
                  goto label_41;
              default:
                goto label_41;
            }
            break;
          case 5:
            if (lower == "pages")
              goto label_35;
            else
              goto label_41;
          case 7:
            switch (lower[0])
            {
              case 'k':
                if (lower == "keynote")
                  goto label_39;
                else
                  goto label_41;
              case 'n':
                if (lower == "numbers")
                  goto label_37;
                else
                  goto label_41;
              default:
                goto label_41;
            }
          default:
            goto label_41;
        }
label_31:
        return Constants.AttachmentKind.IMAGE;
label_32:
        return Constants.AttachmentKind.AUDIO;
label_35:
        return Constants.AttachmentKind.DOC;
label_37:
        return Constants.AttachmentKind.XLS;
label_39:
        return Constants.AttachmentKind.PPT;
label_40:
        return Constants.AttachmentKind.ZIP;
      }
label_41:
      return Constants.AttachmentKind.OTHER;
    }

    public static string GetFileTypeName(string fileName)
    {
      return AttachmentProvider.GetFileType(fileName) == Constants.AttachmentKind.IMAGE ? Utils.GetString("Image").ToLower() : Utils.GetString("File").ToLower();
    }

    public void Clear() => this._displayAttachmentList.Clear();

    public static async Task<List<AttachmentModel>> AddAttachmentModels(
      string taskId,
      List<AddTaskAttachmentInfo> files)
    {
      List<AttachmentModel> result = new List<AttachmentModel>();
      if (files == null || files.Count == 0)
        return result;
      foreach (AddTaskAttachmentInfo file in files)
      {
        string path = file.Path;
        if (FileUtils.FileEmptyOrNotExists(path))
        {
          UtilLog.Warn("UploadAttachment empty file " + path);
        }
        else
        {
          string fileName = Path.GetFileName(file.Path);
          Constants.AttachmentKind fileType = AttachmentProvider.GetFileType(fileName);
          FileInfo fileInfo = new FileInfo(path);
          AttachmentModel uploadAttachmentModel = new AttachmentModel()
          {
            id = Utils.GetGuid()
          };
          uploadAttachmentModel.refId = uploadAttachmentModel.id;
          uploadAttachmentModel.taskId = taskId;
          uploadAttachmentModel.fileName = fileName;
          uploadAttachmentModel.localPath = path;
          uploadAttachmentModel.fileType = fileType.ToString();
          uploadAttachmentModel.createdTime = new DateTime?(DateTime.Now);
          uploadAttachmentModel.sync_status = 0.ToString();
          uploadAttachmentModel.size = (int) fileInfo.Length;
          if (uploadAttachmentModel.fileType == Constants.AttachmentKind.IMAGE.ToString())
            AttachmentUploadUtils.CompressImage(uploadAttachmentModel);
          FileUtils.CollectFileSize(uploadAttachmentModel.localPath);
          if (FileUtils.FileOverSize(uploadAttachmentModel.localPath))
          {
            Utils.Toast(LocalSettings.Settings.IsPro ? Utils.GetString("AttachmentSizeLimitPro") : Utils.GetString("AttachmentSizeLimit"));
          }
          else
          {
            await AttachmentDao.InsertOrUpdateAttachment(uploadAttachmentModel);
            result.Add(uploadAttachmentModel);
            uploadAttachmentModel = (AttachmentModel) null;
          }
        }
      }
      return result;
    }

    public static void AppendTaskContent(TaskModel task, List<AttachmentModel> attachments)
    {
      if (!(task.kind != "CHECKLIST"))
        return;
      task.content = AttachmentDao.AddAttachmentStrings(task.content, attachments);
    }
  }
}
