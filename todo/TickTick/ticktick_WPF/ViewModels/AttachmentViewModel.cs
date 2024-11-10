// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.AttachmentViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Attachment;
using ticktick_WPF.Views.Detail;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class AttachmentViewModel : BaseViewModel
  {
    private object _tag;
    private Thickness _margin;
    private Thickness _statusMargin;
    private Thickness _cornerRadius;
    private double _width;
    private double _height;
    private Brush _selectedBrush;
    private int _imageMode;
    private string _path;
    private string _localPath;
    private int _size;
    private string _fileName;
    private string _fileType;
    private DateTime? _createdTime;
    private int _ordinal;
    private OptTypeEnum _fileStatus;

    public AttachmentModel AttachmentModel { get; set; }

    public AttachmentViewModel(string filePath, int imageMode = 0)
    {
      this.LocalPath = filePath;
      this.ImageMode = imageMode;
    }

    public AttachmentViewModel(AttachmentModel attachment, int imageMode = 0)
    {
      this.UpdateProperty(attachment, imageMode);
    }

    public void UpdateProperty(AttachmentModel model, int imageMode)
    {
      this.Id = model.id;
      this.Path = model.path;
      this.LocalPath = model.localPath;
      this.RefId = model.refId;
      this.TaskId = model.taskId;
      this.FileName = model.fileName;
      this.FileType = model.fileType;
      this.CreatedTime = model.createdTime;
      this.Size = model.size;
      this.SyncStatus = model.sync_status;
      this.Deleted = model.deleted;
      this.CanDelete = model.canDelete;
      this.ImageMode = imageMode;
      this.FileStatus = AttachmentLoadHelper.AttachmentStatus(model);
      this.AttachmentModel = model;
      try
      {
        this.Ordinal = (int) Enum.Parse(typeof (Constants.AttachmentKind), this.FileType.ToUpper());
      }
      catch (Exception ex)
      {
        this.Ordinal = 10;
      }
    }

    public string Id { get; set; }

    public string RefId { get; set; }

    public string TaskId { get; set; }

    public bool Deleted { get; set; }

    public bool CanDelete { get; set; }

    public string SyncStatus { get; set; }

    public object Tag
    {
      get => this._tag;
      set
      {
        if (this._tag == value)
          return;
        this._tag = value;
        this.OnPropertyChanged(nameof (Tag));
      }
    }

    public Thickness Margin
    {
      get => this._margin;
      set
      {
        if (this._margin == value)
          return;
        this._margin = value;
        this.OnPropertyChanged(nameof (Margin));
      }
    }

    public Thickness StatusMargin
    {
      get => this._statusMargin;
      set
      {
        if (this._statusMargin == value)
          return;
        this._statusMargin = value;
        this.OnPropertyChanged(nameof (StatusMargin));
      }
    }

    public double RadiusX => this._cornerRadius.Top;

    public double RadiusY => this._cornerRadius.Left;

    public Thickness CornerRadius
    {
      get => this._cornerRadius;
      set
      {
        this._cornerRadius = value;
        this.OnPropertyChanged(nameof (CornerRadius));
        this.OnPropertyChanged("RadiusX");
        this.OnPropertyChanged("RadiusY");
      }
    }

    public double Width
    {
      get => this._width;
      set
      {
        if (Math.Abs(this._width - value) < 0.1)
          return;
        this._width = value;
        this.OnPropertyChanged(nameof (Width));
      }
    }

    public double Height
    {
      get => this._height;
      set
      {
        if (Math.Abs(this._height - value) < 0.1)
          return;
        this._height = value;
        this.OnPropertyChanged(nameof (Height));
      }
    }

    public Brush SelectedBrush
    {
      get => this._selectedBrush;
      set
      {
        if (this._selectedBrush == value)
          return;
        this._selectedBrush = value;
        this.OnPropertyChanged(nameof (SelectedBrush));
      }
    }

    public int ImageMode
    {
      get => this._imageMode;
      set
      {
        if (this._imageMode == value)
          return;
        this._imageMode = value;
        this.OnPropertyChanged(nameof (ImageMode));
      }
    }

    public string Path
    {
      get => this._path;
      set
      {
        if (this._path == value)
          return;
        this._path = value;
        this.OnPropertyChanged(nameof (Path));
      }
    }

    public string LocalPath
    {
      get => this._localPath;
      set
      {
        if (this._localPath == value)
          return;
        this._localPath = value;
        this.OnPropertyChanged(nameof (LocalPath));
      }
    }

    public int Size
    {
      get => this._size;
      set
      {
        if (this._size == value)
          return;
        this._size = value;
        this.OnPropertyChanged(nameof (Size));
      }
    }

    public string FileName
    {
      get => this._fileName;
      set
      {
        if (this._fileName == value)
          return;
        this._fileName = value;
        this.OnPropertyChanged(nameof (FileName));
      }
    }

    public string FileType
    {
      get => this._fileType;
      set
      {
        if (this._fileType == value)
          return;
        this._fileType = value;
        this.OnPropertyChanged(nameof (FileType));
      }
    }

    public DateTime? CreatedTime
    {
      get => this._createdTime;
      set
      {
        DateTime? createdTime = this._createdTime;
        DateTime? nullable = value;
        if ((createdTime.HasValue == nullable.HasValue ? (createdTime.HasValue ? (createdTime.GetValueOrDefault() == nullable.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
          return;
        this._createdTime = value;
        this.OnPropertyChanged(nameof (CreatedTime));
      }
    }

    public int Ordinal
    {
      get => this._ordinal;
      set
      {
        if (this._ordinal == value)
          return;
        this._ordinal = value;
        this.OnPropertyChanged(nameof (Ordinal));
      }
    }

    public OptTypeEnum FileStatus
    {
      get => this._fileStatus;
      set
      {
        if (this._fileStatus == value)
          return;
        this._fileStatus = value;
        this.OnPropertyChanged(nameof (FileStatus));
      }
    }

    public void SetFileStatus(OptTypeEnum status) => this.FileStatus = status;
  }
}
