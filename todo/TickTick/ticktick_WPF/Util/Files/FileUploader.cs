// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Files.FileUploader
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Net;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Files
{
  public class FileUploader
  {
    private WebClient _client;
    private Uri _uri;
    private string _fileName;
    private bool _started;
    private object _lock = new object();

    public event UploadFileCompletedEventHandler UploadFileCompleted;

    public event UploadProgressChangedEventHandler UploadProgressChanged;

    public FileUploader(string uploadUri, string filePath, WebHeaderCollection headers)
      : this(new Uri(uploadUri), filePath, headers)
    {
    }

    public FileUploader(Uri uploadUri, string filePath, WebHeaderCollection headers)
    {
      this._client = new WebClient();
      this._client.Headers = headers;
      this._client.UploadFileCompleted += new UploadFileCompletedEventHandler(this.OnWebClientUploadFileCompleted);
      this._client.UploadProgressChanged += new UploadProgressChangedEventHandler(this.OnWebClientUploadProgressChanged);
      this._uri = uploadUri;
      this._fileName = filePath;
    }

    public bool StartAsync()
    {
      lock (this._lock)
      {
        if (this._started)
          return false;
        this._started = true;
      }
      if ((object) this._uri != null && !string.IsNullOrEmpty(this._fileName))
      {
        if (!string.IsNullOrWhiteSpace(this._fileName))
        {
          try
          {
            this._client.UploadFileAsync(this._uri, this._fileName);
          }
          catch (Exception ex)
          {
            UtilLog.Error("FileDownloader, UploadFileAsync Exception: " + ex.Message);
            this.ClearUp();
            return false;
          }
          return true;
        }
      }
      this.ClearUp();
      return false;
    }

    private void ClearUp()
    {
      this.UploadFileCompleted = (UploadFileCompletedEventHandler) null;
      this.UploadProgressChanged = (UploadProgressChangedEventHandler) null;
      if (this._client == null)
        return;
      this._client.UploadFileCompleted -= new UploadFileCompletedEventHandler(this.OnWebClientUploadFileCompleted);
      this._client.UploadProgressChanged -= new UploadProgressChangedEventHandler(this.OnWebClientUploadProgressChanged);
      this._client.CancelAsync();
      this._client.Dispose();
    }

    private void OnWebClientUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
    {
      UploadProgressChangedEventHandler uploadProgressChanged = this.UploadProgressChanged;
      if (uploadProgressChanged == null)
        return;
      uploadProgressChanged(sender, e);
    }

    private void OnWebClientUploadFileCompleted(object sender, UploadFileCompletedEventArgs e)
    {
      UploadFileCompletedEventHandler uploadFileCompleted = this.UploadFileCompleted;
      if (uploadFileCompleted != null)
        uploadFileCompleted(sender, e);
      this.ClearUp();
    }
  }
}
