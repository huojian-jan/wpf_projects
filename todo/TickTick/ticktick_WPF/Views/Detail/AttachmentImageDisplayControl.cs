// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.AttachmentImageDisplayControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public sealed class AttachmentImageDisplayControl : Border, IComponentConnector
  {
    private static readonly double _resolution = 76.0 / 135.0;
    private string _attachementId;
    private Image _optSyncError;
    private LoadingIndicator _optSyncing;
    private Image _optUpload;
    private Image _optDownload;
    private AttachmentImageDisplayControl.OnAttachementMouseEnter _onMouseEnter;
    internal AttachmentImageDisplayControl Root;
    internal Border StatusBorder;
    private bool _contentLoaded;

    public AttachmentImageDisplayControl(
      AttachmentViewModel viewModel,
      AttachmentImageDisplayControl.OnAttachementMouseEnter onMouseEnter)
    {
      this.InitializeComponent();
      this.HorizontalAlignment = HorizontalAlignment.Left;
      if (viewModel == null)
        return;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) viewModel, (EventHandler<PropertyChangedEventArgs>) ((sender, args) =>
      {
        if (!(sender is AttachmentViewModel attachmentViewModel2))
          return;
        this.SwitchType(attachmentViewModel2.FileStatus);
      }), "FileStatus");
      this.DataContext = (object) viewModel;
      this._attachementId = viewModel.Id;
      this._onMouseEnter = onMouseEnter;
      this.SwitchType(viewModel.FileStatus);
    }

    private void SwitchType(OptTypeEnum type)
    {
      switch (type)
      {
        case OptTypeEnum.Download:
          this.OptDownload = true;
          break;
        case OptTypeEnum.Upload:
          this.OptUpload = true;
          break;
        case OptTypeEnum.DownloadFailed:
        case OptTypeEnum.UploadFailed:
          this.OptSyncError = true;
          break;
        case OptTypeEnum.Loading:
          this.OptSyncing = true;
          break;
        default:
          this.HideAllOption();
          this.SetOptBorderVisibility();
          break;
      }
    }

    public bool OptSyncing
    {
      get
      {
        LoadingIndicator optSyncing = this._optSyncing;
        return optSyncing != null && optSyncing.Visibility == Visibility.Visible;
      }
      set
      {
        if (this._optSyncing == null)
        {
          LoadingIndicator loadingIndicator = new LoadingIndicator();
          loadingIndicator.SpeedRatio = 3.0;
          loadingIndicator.IsActive = true;
          loadingIndicator.Style = ThemeUtil.GetStyle("LoadingIndicatorRingStyle");
          loadingIndicator.Foreground = (Brush) ThemeUtil.GetColor("BaseColorOpacity100_80");
          loadingIndicator.HorizontalAlignment = HorizontalAlignment.Center;
          loadingIndicator.VerticalAlignment = VerticalAlignment.Center;
          this._optSyncing = loadingIndicator;
        }
        if (value)
        {
          this.HideAllOption();
          this.StatusBorder.Child = (UIElement) this._optSyncing;
          this.StatusBorder.Visibility = Visibility.Visible;
        }
        this._optSyncing.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        this.SetOptBorderVisibility();
      }
    }

    public bool OptSyncError
    {
      get => this._optSyncError.Visibility == Visibility.Visible;
      set => this.SetOptPath(ref this._optSyncError, "AttSyncErrorDrawingImage", value);
    }

    public bool OptDownload
    {
      get => this._optDownload.Visibility == Visibility.Visible;
      set => this.SetOptPath(ref this._optDownload, "AttSyncErrorDrawingImage", value);
    }

    public bool OptUpload
    {
      get => this._optUpload.Visibility == Visibility.Visible;
      set => this.SetOptPath(ref this._optUpload, "AttImageUploadDrawingImage", value);
    }

    private void SetOptPath(ref Image path, string sourceKey, bool vis)
    {
      if (path == null)
      {
        DrawingImage imageSource = Utils.GetImageSource(sourceKey);
        if (imageSource.CanFreeze)
          imageSource.Freeze();
        ref Image local = ref path;
        Image image = new Image();
        image.Source = (ImageSource) imageSource;
        image.HorizontalAlignment = HorizontalAlignment.Center;
        image.VerticalAlignment = VerticalAlignment.Center;
        local = image;
      }
      if (vis)
      {
        this.HideAllOption();
        this.StatusBorder.Child = (UIElement) path;
        this.StatusBorder.Visibility = Visibility.Visible;
      }
      path.Visibility = vis ? Visibility.Visible : Visibility.Collapsed;
      this.SetOptBorderVisibility();
    }

    private void SetOptBorderVisibility()
    {
      if (this._optUpload != null && this._optUpload.Visibility == Visibility.Visible)
      {
        Border statusBorder = this.StatusBorder;
        SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromRgb((byte) 19, (byte) 19, (byte) 19));
        solidColorBrush.Opacity = 0.3;
        statusBorder.Background = (Brush) solidColorBrush;
      }
      else
        this.StatusBorder.Background = (Brush) Brushes.Transparent;
    }

    private void HideAllOption()
    {
      if (this._optDownload != null)
        this._optDownload.Visibility = Visibility.Collapsed;
      if (this._optUpload != null)
        this._optUpload.Visibility = Visibility.Collapsed;
      if (this._optSyncError != null)
        this._optSyncError.Visibility = Visibility.Collapsed;
      if (this._optSyncing == null)
        return;
      this._optSyncing.Visibility = Visibility.Collapsed;
    }

    private async void OnMouseEnter(object sender, MouseEventArgs e)
    {
      await Task.Delay(100);
      if (!(sender is Border border) || !border.IsMouseOver)
        return;
      AttachmentImageDisplayControl.OnAttachementMouseEnter onMouseEnter = this._onMouseEnter;
      if (onMouseEnter == null)
        return;
      onMouseEnter(sender);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/attachmentimagedisplaycontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.StatusBorder = (Border) target;
        else
          this._contentLoaded = true;
      }
      else
      {
        this.Root = (AttachmentImageDisplayControl) target;
        this.Root.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      }
    }

    public delegate void OnAttachementMouseEnter(object sender);
  }
}
