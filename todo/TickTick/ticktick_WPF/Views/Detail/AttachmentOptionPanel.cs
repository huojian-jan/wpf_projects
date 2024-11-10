// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.AttachmentOptionPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Attachment;
using ticktick_WPF.Util.Files;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.DetailView;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class AttachmentOptionPanel : System.Windows.Controls.UserControl, IComponentConnector
  {
    private AttachmentInfo _modelInfo;
    private AttachmentModel _model;
    private bool _enable;
    private OptTypeEnum _optType = OptTypeEnum.None;
    internal Grid ImageOption;
    internal Border ImageOptionMenuBorder;
    internal Popup ImageOptionMenu;
    internal System.Windows.Controls.Button SwitchToSmallSizeButton;
    internal System.Windows.Controls.Button SwitchToNormalSizeButton;
    internal StackPanel FileOption;
    internal Popup AttachmentOptionMenu;
    internal System.Windows.Controls.ProgressBar DownloadProgress;
    private bool _contentLoaded;

    public event EventHandler<AttachmentInfo> Delete;

    public event EventHandler<AttachmentModel> CheckListDelete;

    public event EventHandler<int> ImageModeChanged;

    private OptTypeEnum OptType
    {
      get => this._optType;
      set
      {
        this._optType = value;
        if (this._optType != OptTypeEnum.Menu)
        {
          this.ImageOption.Opacity = 0.0;
          this.FileOption.Opacity = 0.0;
        }
        else
        {
          this.ImageOption.Opacity = 1.0;
          this.FileOption.Opacity = 1.0;
        }
      }
    }

    public AttachmentOptionPanel() => this.InitializeComponent();

    private void OnFileDownLoadClick(object sender, RoutedEventArgs e)
    {
      this.AttachmentOptionMenu.IsOpen = false;
      this.SaveAs();
    }

    private void OnFileDeleteClick(object sender, RoutedEventArgs e)
    {
      this.AttachmentOptionMenu.IsOpen = false;
      this.FileDelete();
    }

    private void FileDelete()
    {
      if (this._modelInfo == null)
      {
        EventHandler<AttachmentModel> checkListDelete = this.CheckListDelete;
        if (checkListDelete == null)
          return;
        checkListDelete((object) this, this._model);
      }
      else
      {
        EventHandler<AttachmentInfo> delete = this.Delete;
        if (delete == null)
          return;
        delete((object) this, this._modelInfo);
      }
    }

    private async Task Download()
    {
      await AttachmentDownloadUtils.DownloadFileAttachment(this._model.id);
    }

    private async Task SaveAs()
    {
      AttachmentOptionPanel child = this;
      if (child._model == null)
        return;
      AttachmentModel attachment = child._model;
      TaskModel task = await TaskDao.GetThinTaskById(child._model.taskId);
      if (task == null)
        return;
      string str = FileUtils.TrimFileName(attachment.fileName, 200);
      SaveFileDialog saveFileDialog1 = new SaveFileDialog();
      saveFileDialog1.FileName = str;
      SaveFileDialog saveFileDialog2 = saveFileDialog1;
      TaskDetailView detailView = Utils.FindParent<TaskDetailView>((DependencyObject) child);
      detailView?.OnShowDialog();
      if (saveFileDialog2.ShowDialog() == DialogResult.OK)
      {
        string foldPath = saveFileDialog2.FileName;
        try
        {
          if (!string.IsNullOrEmpty(attachment.localPath) && File.Exists(attachment.localPath))
          {
            File.Copy(attachment.localPath, foldPath);
          }
          else
          {
            attachment.localPath = foldPath;
            await AttachmentDao.UpdateAttachment(attachment);
            AttachmentDownloadUtils.DownloadFile(task.projectId, task.id, attachment.id, foldPath);
          }
        }
        catch (Exception ex)
        {
        }
        foldPath = (string) null;
      }
      detailView?.OnCloseDialog();
      attachment = (AttachmentModel) null;
      task = (TaskModel) null;
      detailView = (TaskDetailView) null;
    }

    private async Task Upload()
    {
      AttachmentOptionPanel child = this;
      if (child._model == null)
        return;
      if (AttachmentLoadHelper.IsLoading(child._model.id))
      {
        child.OptType = OptTypeEnum.Loading;
      }
      else
      {
        if (AttachmentCache.GetAttachmentById(child._model.id) == null)
          return;
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(child._model.taskId);
        if (thinTaskById == null)
          return;
        AttachmentUploadUtils.UpFile(thinTaskById.projectId, thinTaskById.id, child._model, Utils.FindParent<IToastShowWindow>((DependencyObject) child));
      }
    }

    public void SetPanel(
      AttachmentViewModel model,
      double width,
      double height,
      bool enable,
      int imageMode = 0)
    {
      this._model = AttachmentCache.GetAttachmentById(model.Id);
      this._modelInfo = (AttachmentInfo) null;
      this._enable = enable && model != null;
      this.DataContext = (object) new AttachmentOptionPanelModel()
      {
        FileName = model?.FileName
      };
      this.DownloadProgress.Visibility = Visibility.Collapsed;
      this.OptType = AttachmentLoadHelper.AttachmentStatus(this._model, enable);
      this.ImageOption.Visibility = Visibility.Collapsed;
      this.FileOption.Visibility = Visibility.Collapsed;
      if (model != null & enable)
      {
        this.OptType = AttachmentLoadHelper.AttachmentStatus(this._model, enable);
        if (model.FileType == "IMAGE")
        {
          this.ImageOption.Visibility = Visibility.Visible;
          this.SwitchToSmallSizeButton.Visibility = imageMode == 0 ? Visibility.Visible : Visibility.Collapsed;
          this.SwitchToNormalSizeButton.Visibility = imageMode == 0 ? Visibility.Collapsed : Visibility.Visible;
          this.ImageOptionMenuBorder.Margin = new Thickness(imageMode == 0 ? 6.0 : 3.0);
        }
        else
          this.FileOption.Visibility = Visibility.Visible;
      }
      this.Width = width;
      this.Height = height;
    }

    public void SetPanel(
      AttachmentMDDisplayModel model,
      double width,
      double height,
      bool enable,
      int imageMode = 0)
    {
      if (model.Attachment != null)
        model.Attachment = AttachmentCache.GetAttachmentById(model.Attachment.id);
      this._model = model.Attachment;
      this._modelInfo = model.Info;
      this._enable = enable && model.Attachment != null;
      this.DataContext = (object) new AttachmentOptionPanelModel()
      {
        FileName = model?.Attachment?.fileName
      };
      this.DownloadProgress.Visibility = Visibility.Collapsed;
      this.OptType = AttachmentLoadHelper.AttachmentStatus(this._model, enable);
      this.ImageOption.Visibility = Visibility.Collapsed;
      this.FileOption.Visibility = Visibility.Collapsed;
      if (model.Attachment != null & enable)
      {
        if (model.Info?.Kind == "image")
        {
          this.ImageOption.Visibility = Visibility.Visible;
          this.SwitchToSmallSizeButton.Visibility = imageMode == 0 ? Visibility.Visible : Visibility.Collapsed;
          this.SwitchToNormalSizeButton.Visibility = imageMode == 0 ? Visibility.Collapsed : Visibility.Visible;
          this.ImageOptionMenuBorder.Margin = new Thickness(imageMode == 0 ? 6.0 : 3.0);
        }
        else
          this.FileOption.Visibility = Visibility.Visible;
      }
      this.Width = width;
      this.Height = height;
    }

    private async void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this.FileOption.IsMouseOver || this.ImageOption.IsMouseOver || this.AttachmentOptionMenu.IsMouseOver || this.ImageOptionMenu.IsMouseOver || this._model == null)
        return;
      e.Handled = true;
      if (!FileUtils.FileEmptyOrNotExists(this._model.localPath))
      {
        try
        {
          await Task.Delay(50);
          Process.Start(this._model.localPath);
        }
        catch (Exception ex)
        {
        }
      }
      else
      {
        if (this._model == null || !(this._model.fileType != "IMAGE"))
          return;
        this.Download();
      }
    }

    private void OnOptionRightMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this.OptType != OptTypeEnum.Menu)
        return;
      if (this.ImageOption.IsVisible)
      {
        this.ImageOptionMenu.Placement = PlacementMode.Mouse;
        this.ImageOptionMenu.IsOpen = true;
      }
      else
      {
        this.AttachmentOptionMenu.Placement = PlacementMode.Mouse;
        this.AttachmentOptionMenu.IsOpen = true;
      }
    }

    private void OnImageMorePreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
      switch (this.OptType)
      {
        case OptTypeEnum.Menu:
          this.ImageOptionMenu.Placement = PlacementMode.Bottom;
          this.ImageOptionMenu.PlacementTarget = sender as UIElement;
          this.ImageOptionMenu.IsOpen = true;
          break;
        case OptTypeEnum.Download:
        case OptTypeEnum.DownloadFailed:
          this.Download();
          break;
        case OptTypeEnum.Upload:
        case OptTypeEnum.UploadFailed:
          this.Upload();
          break;
      }
    }

    private void OnImageDeleteClick(object sender, RoutedEventArgs e)
    {
      this.ImageOptionMenu.IsOpen = false;
      this.FileDelete();
    }

    private void OnImageDownloadClick(object sender, RoutedEventArgs e)
    {
      this.ImageOptionMenu.IsOpen = false;
      this.SaveAs();
    }

    private void OnNormalSizeClick(object sender, RoutedEventArgs e)
    {
      this.ImageOptionMenu.IsOpen = false;
      EventHandler<int> imageModeChanged = this.ImageModeChanged;
      if (imageModeChanged == null)
        return;
      imageModeChanged((object) this, 0);
    }

    private void OnSmallSizeClick(object sender, RoutedEventArgs e)
    {
      this.ImageOptionMenu.IsOpen = false;
      EventHandler<int> imageModeChanged = this.ImageModeChanged;
      if (imageModeChanged == null)
        return;
      imageModeChanged((object) this, 1);
    }

    private void OnAttachmentOptionClick(object sender, MouseButtonEventArgs e)
    {
      switch (this.OptType)
      {
        case OptTypeEnum.Menu:
          this.AttachmentOptionMenu.Placement = PlacementMode.Bottom;
          this.AttachmentOptionMenu.PlacementTarget = sender as UIElement;
          this.AttachmentOptionMenu.IsOpen = true;
          break;
        case OptTypeEnum.Download:
        case OptTypeEnum.DownloadFailed:
          this.Download();
          break;
        case OptTypeEnum.Upload:
        case OptTypeEnum.UploadFailed:
          this.Upload();
          break;
      }
    }

    private void OnImageCopyClick(object sender, RoutedEventArgs e)
    {
      if (this._model == null)
        return;
      if (!File.Exists(this._model.localPath))
        return;
      try
      {
        System.Windows.Clipboard.SetFileDropList(new StringCollection()
        {
          this._model.localPath
        });
        Utils.FindParent<IToastShowWindow>((DependencyObject) this)?.TryToastString((object) null, Utils.GetString("Copied"));
      }
      catch (Exception ex)
      {
        Utils.FindParent<IToastShowWindow>((DependencyObject) this)?.TryToastString((object) null, Utils.GetString("OperateFailed"));
        UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
      }
      this.ImageOptionMenu.IsOpen = false;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/attachmentoptionpanel.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
          ((UIElement) target).PreviewMouseRightButtonUp += new MouseButtonEventHandler(this.OnOptionRightMouseUp);
          break;
        case 2:
          this.ImageOption = (Grid) target;
          break;
        case 3:
          this.ImageOptionMenuBorder = (Border) target;
          this.ImageOptionMenuBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnImageMorePreviewMouseUp);
          break;
        case 4:
          this.ImageOptionMenu = (Popup) target;
          break;
        case 5:
          this.SwitchToSmallSizeButton = (System.Windows.Controls.Button) target;
          this.SwitchToSmallSizeButton.Click += new RoutedEventHandler(this.OnSmallSizeClick);
          break;
        case 6:
          this.SwitchToNormalSizeButton = (System.Windows.Controls.Button) target;
          this.SwitchToNormalSizeButton.Click += new RoutedEventHandler(this.OnNormalSizeClick);
          break;
        case 7:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.OnImageCopyClick);
          break;
        case 8:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.OnImageDownloadClick);
          break;
        case 9:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.OnImageDeleteClick);
          break;
        case 10:
          this.FileOption = (StackPanel) target;
          break;
        case 11:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnAttachmentOptionClick);
          break;
        case 12:
          this.AttachmentOptionMenu = (Popup) target;
          break;
        case 13:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.OnFileDownLoadClick);
          break;
        case 14:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.OnFileDeleteClick);
          break;
        case 15:
          this.DownloadProgress = (System.Windows.Controls.ProgressBar) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
