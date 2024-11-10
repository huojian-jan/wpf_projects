// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Files.FileDownloader
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ticktick_WPF.Util.Attachment;
using ticktick_WPF.Util.Network;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Files
{
  public class FileDownloader
  {
    private Uri _uri;
    private string _fileName;
    private bool _started;
    private object _lock = new object();
    private HttpClient _httpClient;
    private string _attachmentId;

    public event EventHandler<AttachmentDownloadSuccessModel> DownloadFileCompleted;

    public event EventHandler<AttachmentProgressModel> DownloadProgressChanged;

    public FileDownloader(string downloadUri, string filePath, string attachmentId)
      : this(new Uri(downloadUri), filePath, attachmentId)
    {
    }

    public FileDownloader(Uri downloadUri, string filePath, string attachmentId)
    {
      this._httpClient = ApiClient.GetDownloadClient();
      this._httpClient.DefaultRequestHeaders.Add(nameof (attachmentId), attachmentId);
      this._uri = downloadUri;
      this._fileName = FileUtils.ToValidFileName(filePath);
      this._attachmentId = attachmentId;
    }

    public async Task StartDownloadAsync()
    {
      AttachmentDownloadSuccessModel completeModel;
      lock (this._lock)
      {
        if (this._started)
        {
          completeModel = (AttachmentDownloadSuccessModel) null;
          return;
        }
        this._started = true;
      }
      if ((object) this._uri == null || string.IsNullOrEmpty(this._fileName) || string.IsNullOrWhiteSpace(this._fileName))
      {
        this.ClearUp();
        completeModel = (AttachmentDownloadSuccessModel) null;
      }
      else
      {
        completeModel = new AttachmentDownloadSuccessModel()
        {
          AttachmentId = this._attachmentId,
          Success = true
        };
        try
        {
          HttpResponseMessage async = await this._httpClient.GetAsync(this._uri);
          long? contentLength = async.Content.Headers.ContentLength;
          FileInfo fileInfo = new FileInfo(this._fileName);
          FileInfo bakFile = new FileInfo(Path.GetDirectoryName(this._fileName) + "\\bak_" + fileInfo.Name);
          using (FileStream fileStream = bakFile.Create())
          {
            using (Stream stream = await async.Content.ReadAsStreamAsync())
              await stream.CopyToAsync((Stream) fileStream);
          }
          bakFile.CopyTo(this._fileName);
          bakFile.Attributes = FileAttributes.Normal;
          bakFile.Delete();
          EventHandler<AttachmentProgressModel> downloadProgressChanged = this.DownloadProgressChanged;
          if (downloadProgressChanged != null)
            downloadProgressChanged((object) null, new AttachmentProgressModel()
            {
              AttachmentId = this._attachmentId,
              ProgressPercentage = 100
            });
          bakFile = (FileInfo) null;
        }
        catch (Exception ex)
        {
          UtilLog.Error("FileDownloader, DownloadFileAsync Exception: " + ex.Message);
          completeModel.Success = false;
        }
        EventHandler<AttachmentDownloadSuccessModel> downloadFileCompleted = this.DownloadFileCompleted;
        if (downloadFileCompleted != null)
          downloadFileCompleted((object) null, completeModel);
        this.ClearUp();
        completeModel = (AttachmentDownloadSuccessModel) null;
      }
    }

    private void ClearUp()
    {
      this.DownloadFileCompleted = (EventHandler<AttachmentDownloadSuccessModel>) null;
      this.DownloadProgressChanged = (EventHandler<AttachmentProgressModel>) null;
    }
  }
}
