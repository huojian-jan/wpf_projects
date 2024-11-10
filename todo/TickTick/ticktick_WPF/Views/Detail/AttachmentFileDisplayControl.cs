// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.AttachmentFileDisplayControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Converter;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Files;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class AttachmentFileDisplayControl : Border, IComponentConnector
  {
    private static readonly AttachmentIconConverter _attachmentIconConverter = new AttachmentIconConverter();
    private static readonly AttachmentSizedConverter _attachmentSizedConverter = new AttachmentSizedConverter();
    private static readonly AttachmentBackgroundConverter _attachmentBackgroundConverter = new AttachmentBackgroundConverter();
    private string _fileType;
    private AttachmentConvertParam _convertParam;
    private Image _optSyncError;
    private LoadingIndicator _optSyncing;
    private Image _optUpload;
    private Image _optDownload;
    private bool _optSynced;
    private Border _optBorder;
    private Grid _mainGrid;
    private Image _iconImage;
    private bool _isDark;
    private bool _contentLoaded;

    public bool OptSyncing
    {
      get
      {
        LoadingIndicator optSyncing = this._optSyncing;
        return optSyncing != null && optSyncing.Visibility == Visibility.Visible;
      }
      set
      {
        if (this._optSyncing == null & value)
        {
          LoadingIndicator loadingIndicator = new LoadingIndicator();
          loadingIndicator.SpeedRatio = 3.0;
          loadingIndicator.IsActive = true;
          loadingIndicator.Style = ThemeUtil.GetStyle("LoadingIndicatorRingStyle");
          loadingIndicator.HorizontalAlignment = HorizontalAlignment.Center;
          loadingIndicator.VerticalAlignment = VerticalAlignment.Center;
          this._optSyncing = loadingIndicator;
          this._optSyncing.SetResourceReference(Control.ForegroundProperty, (object) "BaseColorOpacity100_80");
        }
        if (value)
        {
          this.HideAllOption();
          this._optBorder.Child = (UIElement) this._optSyncing;
        }
        if (this._optSyncing == null)
          return;
        this._optSyncing.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    public bool OptSynced
    {
      get => this._optSynced;
      set
      {
        this._optSynced = value;
        if (!value)
          return;
        this.HideAllOption();
        this._convertParam.IsFailed = false;
        this.ResetIconAndBackground();
      }
    }

    public bool OptSyncError
    {
      get
      {
        Image optSyncError = this._optSyncError;
        return optSyncError != null && optSyncError.Visibility == Visibility.Visible;
      }
      set => this.SetOptPath(ref this._optSyncError, "AttSyncErrorDrawingImage", value);
    }

    public bool OptDownload
    {
      get
      {
        Image optDownload = this._optDownload;
        return optDownload != null && optDownload.Visibility == Visibility.Visible;
      }
      set => this.SetOptPath(ref this._optDownload, "AttDownloadDrawingImage", value);
    }

    public bool OptUpload
    {
      get
      {
        Image optUpload = this._optUpload;
        return optUpload != null && optUpload.Visibility == Visibility.Visible;
      }
      set => this.SetOptPath(ref this._optUpload, "AttFileUploadDrawingImage", value);
    }

    private void SetOptPath(ref Image path, string sourceKey, bool vis)
    {
      if (path == null & vis)
      {
        ResourceDictionary imageDictionary = ThemeUtil.GetImageDictionary(this._isDark);
        DrawingImage imageSource = Utils.GetImageSource(sourceKey, imageDictionary);
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
        this._optBorder.Child = (UIElement) path;
      }
      if (path == null)
        return;
      path.Visibility = vis ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ResetIconAndBackground()
    {
      this._iconImage.Source = (ImageSource) new BitmapImage(new Uri((string) (AttachmentFileDisplayControl._attachmentIconConverter.Convert((object) this._fileType, (Type) null, (object) this._convertParam, (CultureInfo) null) ?? (object) string.Empty)));
      this.Background = (Brush) (AttachmentFileDisplayControl._attachmentBackgroundConverter.Convert((object) this._fileType, (Type) null, (object) this._convertParam, (CultureInfo) null) as SolidColorBrush);
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

    public AttachmentFileDisplayControl(
      AttachmentViewModel model,
      AttachmentImageDisplayControl.OnAttachementMouseEnter mouseEnter,
      bool isDark)
    {
      AttachmentFileDisplayControl fileDisplayControl = this;
      this.InitializeComponent();
      this._isDark = isDark;
      this.HorizontalAlignment = HorizontalAlignment.Left;
      if (model == null)
        return;
      model.PropertyChanged += (PropertyChangedEventHandler) ((s, e) =>
      {
        if (!(s is AttachmentViewModel attachmentViewModel2))
          return;
        switch (e.PropertyName)
        {
          case "FileStatus":
            fileDisplayControl._convertParam.IsFailed = FileUtils.FileEmptyOrNotExists(model.LocalPath);
            fileDisplayControl.SetOpt(attachmentViewModel2.FileStatus);
            break;
          case "LocalPath":
            fileDisplayControl._convertParam.IsFailed = FileUtils.FileEmptyOrNotExists(model.LocalPath);
            fileDisplayControl.ResetIconAndBackground();
            break;
        }
      });
      this.DataContext = (object) model;
      this.InitViews(model);
      this.MouseEnter += (MouseEventHandler) (async (s, e) =>
      {
        await Task.Delay(100);
        if (!(s is Border border2) || !border2.IsMouseOver)
          return;
        AttachmentImageDisplayControl.OnAttachementMouseEnter attachementMouseEnter = mouseEnter;
        if (attachementMouseEnter == null)
          return;
        attachementMouseEnter(s);
      });
      this._convertParam.IsDark = isDark;
      this._convertParam.IsFailed = FileUtils.FileEmptyOrNotExists(model.LocalPath);
      this._fileType = model.FileType;
      this.SetOpt(model.FileStatus);
    }

    private void SetOpt(OptTypeEnum opt)
    {
      switch (opt)
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
          this._convertParam.IsFailed = false;
          break;
      }
      this.ResetIconAndBackground();
    }

    private void InitViews(AttachmentViewModel model)
    {
      Border border1 = new Border()
      {
        CornerRadius = new CornerRadius(4.0)
      };
      this._mainGrid = new Grid();
      this._mainGrid.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this._mainGrid.ColumnDefinitions.Add(new ColumnDefinition());
      this._mainGrid.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      Image image = new Image();
      image.Width = 32.0;
      image.Height = 32.0;
      image.Source = (ImageSource) new BitmapImage(new Uri((string) (AttachmentFileDisplayControl._attachmentIconConverter.Convert((object) model.FileType, (Type) null, (object) this._convertParam, (CultureInfo) null) ?? (object) string.Empty)));
      this._iconImage = image;
      UIElementCollection children = this._mainGrid.Children;
      Border element1 = new Border();
      element1.Margin = new Thickness(12.0, 0.0, 0.0, 0.0);
      element1.VerticalAlignment = VerticalAlignment.Center;
      element1.Child = (UIElement) this._iconImage;
      children.Add((UIElement) element1);
      Grid grid = new Grid();
      grid.Height = 56.0;
      grid.VerticalAlignment = VerticalAlignment.Center;
      grid.Margin = new Thickness(12.0, 0.0, 10.0, 0.0);
      Grid element2 = grid;
      element2.RowDefinitions.Add(new RowDefinition());
      element2.RowDefinitions.Add(new RowDefinition());
      TextBlock textBlock1 = new TextBlock();
      textBlock1.Tag = (object) model.Id;
      textBlock1.Text = model.FileName;
      textBlock1.TextTrimming = TextTrimming.CharacterEllipsis;
      textBlock1.VerticalAlignment = VerticalAlignment.Bottom;
      textBlock1.Style = ThemeUtil.GetStyle("Body01");
      textBlock1.Cursor = Cursors.Hand;
      textBlock1.Padding = new Thickness(0.0, 0.0, 30.0, 0.0);
      textBlock1.LineHeight = 14.0;
      textBlock1.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
      TextBlock element3 = textBlock1;
      element3.SetResourceReference(Control.ForegroundProperty, (object) "BaseColorOpacity100_80");
      TextBlock textBlock2 = new TextBlock();
      textBlock2.Text = AttachmentFileDisplayControl._attachmentSizedConverter.Convert((object) model.Size, (Type) null, (object) null, (CultureInfo) null) as string;
      textBlock2.FontSize = 12.0;
      textBlock2.LineHeight = 12.0;
      textBlock2.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
      textBlock2.VerticalAlignment = VerticalAlignment.Center;
      TextBlock element4 = textBlock2;
      element4.SetResourceReference(Control.ForegroundProperty, (object) "BaseColorOpacity40");
      element2.Children.Add((UIElement) element3);
      Grid.SetRow((UIElement) element4, 1);
      element2.Children.Add((UIElement) element4);
      Grid.SetColumn((UIElement) element2, 1);
      this._mainGrid.Children.Add((UIElement) element2);
      Border border2 = new Border();
      border2.Width = 24.0;
      border2.Height = 24.0;
      border2.VerticalAlignment = VerticalAlignment.Center;
      border2.Margin = new Thickness(0.0, 0.0, 12.0, 0.0);
      border2.Background = (Brush) new SolidColorBrush(Colors.Transparent);
      this._optBorder = border2;
      Grid.SetColumn((UIElement) this._optBorder, 2);
      this._mainGrid.Children.Add((UIElement) this._optBorder);
      border1.Child = (UIElement) this._mainGrid;
      this.Child = (UIElement) border1;
      this.Height = 54.0;
      this.CornerRadius = new CornerRadius(4.0);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/attachmentfiledisplaycontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target) => this._contentLoaded = true;
  }
}
