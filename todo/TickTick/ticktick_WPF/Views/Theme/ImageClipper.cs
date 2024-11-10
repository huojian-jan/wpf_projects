// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.ImageClipper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class ImageClipper : UserControl, IComponentConnector
  {
    public ImageClipper.CutImageDelegate OnCutImage;
    private BitmapImage _bitSource;
    private double BorderWidth = 1.0;
    private bool _clipBorderMouseDown;
    private ImageClipper.DragPointMode _pointMode;
    private bool _imageAreaMouseDown;
    private System.Windows.Point _imageAreaMouseDownPoint = new System.Windows.Point(-1.0, -1.0);
    private BitmapImage _bitmapImage;
    private PixelColor[,] _colors;
    internal Image SourceImage;
    internal System.Windows.Shapes.Path MaskPath;
    internal RectangleGeometry RectOutside;
    internal RectangleGeometry RectInside;
    internal Canvas Container;
    internal Border ImageArea;
    internal Border ClipBorder;
    private bool _contentLoaded;

    public double MaxImageWidth { get; set; } = 640.0;

    public double MaxImageHeight { get; set; } = 400.0;

    public BitmapImage BitSource
    {
      get => this._bitSource;
      set
      {
        this._bitSource = value;
        this.SourceImage.Source = (ImageSource) value;
      }
    }

    public event EventHandler<List<SelectThemeColorViewModel>> OnThemeColorsChanged;

    public ImageClipper() => this.InitializeComponent();

    public void SetImage(string imageUrl, string location = null)
    {
      try
      {
        this.LoadBitmap(imageUrl);
      }
      catch (Exception ex)
      {
      }
      if (this._bitmapImage == null)
        return;
      double width = this.MaxImageWidth;
      double height = this.MaxImageHeight;
      if (this._bitmapImage != null && this._bitmapImage.Height > 0.0)
      {
        double num = this._bitmapImage.Width / this._bitmapImage.Height;
        if (num > 1.6)
        {
          width = this.MaxImageWidth;
          height = this.MaxImageWidth / num;
        }
        else if (num < 1.6)
        {
          width = this.MaxImageHeight * num;
          height = this.MaxImageHeight;
        }
        else
        {
          width = this.MaxImageWidth;
          height = this.MaxImageHeight;
        }
      }
      this.SourceImage.Width = width;
      this.SourceImage.Height = height;
      this.SourceImage.Source = (ImageSource) this._bitmapImage;
      this.Container.Width = width;
      this.Container.Height = height;
      Rect imageRect = this.GetImageRect(width, height, location);
      this.ImageArea.Width = imageRect.Width;
      this.ImageArea.Height = imageRect.Height;
      Canvas.SetLeft((UIElement) this.ImageArea, imageRect.Left);
      Canvas.SetTop((UIElement) this.ImageArea, imageRect.Top);
      this.MaskPath.Margin = new Thickness((this.MaxImageWidth - width) / 2.0, 0.0, 0.0, 0.0);
      this.RectOutside.Rect = new Rect(0.0, 0.0, width, height);
      this.RectInside.Rect = imageRect;
      this.GetThemeColor();
      this.TrySetTheme(true);
    }

    private void GetThemeColor()
    {
      List<SelectThemeColorViewModel> themeColorViewModelList = new List<SelectThemeColorViewModel>();
      if (this._colors != null && this._colors.Length > 0)
      {
        Rect currentLocation = this.GetCurrentLocation();
        currentLocation.Width /= 3.0;
        Dictionary<PixelColor, long> source = new Dictionary<PixelColor, long>();
        int num1 = Math.Max(0, (int) (currentLocation.Left * (double) this._colors.GetLength(1)));
        int num2 = Math.Min(this._colors.GetLength(1), (int) (currentLocation.Right * (double) this._colors.GetLength(1)));
        int num3 = Math.Max(0, (int) (currentLocation.Top * (double) this._colors.GetLength(0)));
        int num4 = Math.Min(this._colors.GetLength(0), (int) (currentLocation.Bottom * (double) this._colors.GetLength(0)));
        for (int index1 = num1; index1 < num2; ++index1)
        {
          for (int index2 = num3; index2 < num4; ++index2)
          {
            PixelColor color = this._colors[index2, index1];
            if (!color.IsTooLight())
            {
              if (source.ContainsKey(color))
                ++source[color];
              else
                source[color] = 1L;
            }
          }
        }
        List<KeyValuePair<PixelColor, long>> list = source.ToList<KeyValuePair<PixelColor, long>>();
        list.Sort((Comparison<KeyValuePair<PixelColor, long>>) ((a, b) => b.Value.CompareTo(a.Value)));
        if (list.Count > 3)
          list = list.Take<KeyValuePair<PixelColor, long>>(3).ToList<KeyValuePair<PixelColor, long>>();
        foreach (KeyValuePair<PixelColor, long> keyValuePair in list)
        {
          SolidColorBrush colorBrush = keyValuePair.Key.ToColorBrush();
          themeColorViewModelList.Add(new SelectThemeColorViewModel()
          {
            Color = colorBrush.Color.ToString()
          });
        }
      }
      foreach (string str in new List<string>()
      {
        "#FF4772FA",
        "#FFFF739F",
        "#FFFFA14D",
        "#FF00C598",
        "#FF000000",
        "#FF7D7F84"
      })
      {
        string color = str;
        if (themeColorViewModelList.All<SelectThemeColorViewModel>((Func<SelectThemeColorViewModel, bool>) (c => c.Color != color)))
          themeColorViewModelList.Add(new SelectThemeColorViewModel()
          {
            Color = color
          });
      }
      EventHandler<List<SelectThemeColorViewModel>> themeColorsChanged = this.OnThemeColorsChanged;
      if (themeColorsChanged == null)
        return;
      themeColorsChanged((object) this, themeColorViewModelList);
    }

    private Rect GetImageRect(double width, double height, string location)
    {
      double num = width / height;
      Rect rectByString = Utils.GetRectByString(location);
      double val2_1;
      double val2_2;
      double x;
      double y;
      if (rectByString.Height == 0.0 || rectByString.Width == 0.0)
      {
        val2_1 = num > 1.6 ? height * 1.6 : width;
        val2_2 = num > 1.6 ? height : width / 1.6;
        x = (width - val2_1) / 2.0;
        y = (height - val2_2) / 2.0;
      }
      else
      {
        x = width * rectByString.Left;
        y = height * rectByString.Top;
        val2_1 = width * rectByString.Width;
        val2_2 = height * rectByString.Height;
      }
      double width1 = Math.Min(width - x, val2_1);
      double height1 = Math.Min(height - y, val2_2);
      return new Rect(x, y, width1, height1);
    }

    private void OnClipMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Released)
      {
        if (this._imageAreaMouseDownPoint.X > 0.0)
          this._imageAreaMouseDownPoint = new System.Windows.Point(-1.0, -1.0);
        if (this.ClipBorder.IsMouseOver)
        {
          System.Windows.Point position = e.GetPosition((IInputElement) this.ClipBorder);
          bool flag1 = position.Y <= 6.0;
          bool flag2 = position.X <= 6.0;
          bool flag3 = position.Y >= this.ClipBorder.ActualHeight - 6.0;
          bool flag4 = position.X >= this.ClipBorder.ActualWidth - 6.0;
          if (flag1 & flag2 || flag3 & flag4)
          {
            Mouse.OverrideCursor = Cursors.SizeNWSE;
            this._pointMode = flag1 ? ImageClipper.DragPointMode.TopLeft : ImageClipper.DragPointMode.BottomRight;
          }
          else if (flag1 & flag4 || flag3 & flag2)
          {
            Mouse.OverrideCursor = Cursors.SizeNESW;
            this._pointMode = flag1 ? ImageClipper.DragPointMode.TopRight : ImageClipper.DragPointMode.BottomLeft;
          }
          else if (flag1 | flag3)
          {
            Mouse.OverrideCursor = Cursors.SizeNS;
            this._pointMode = flag1 ? ImageClipper.DragPointMode.Top : ImageClipper.DragPointMode.Bottom;
          }
          else
          {
            if (!(flag4 | flag2))
              return;
            Mouse.OverrideCursor = Cursors.SizeWE;
            this._pointMode = flag2 ? ImageClipper.DragPointMode.Left : ImageClipper.DragPointMode.Right;
          }
        }
        else
          Mouse.OverrideCursor = (Cursor) null;
      }
      else if (this._clipBorderMouseDown)
      {
        System.Windows.Point position = e.GetPosition((IInputElement) this.Container);
        switch (this._pointMode)
        {
          case ImageClipper.DragPointMode.TopLeft:
            System.Windows.Point targetPoint1 = new System.Windows.Point(Canvas.GetLeft((UIElement) this.ImageArea) + this.ImageArea.Width, Canvas.GetTop((UIElement) this.ImageArea) + this.ImageArea.Height);
            position.X = Math.Min(position.X, targetPoint1.X);
            position.Y = Math.Min(position.Y, targetPoint1.Y);
            this.SetImageArea(targetPoint1, position, this._pointMode);
            break;
          case ImageClipper.DragPointMode.TopRight:
            System.Windows.Point targetPoint2 = new System.Windows.Point(Canvas.GetLeft((UIElement) this.ImageArea), Canvas.GetTop((UIElement) this.ImageArea) + this.ImageArea.Height);
            position.X = Math.Max(position.X, targetPoint2.X);
            position.Y = Math.Min(position.Y, targetPoint2.Y);
            this.SetImageArea(targetPoint2, position, this._pointMode);
            break;
          case ImageClipper.DragPointMode.BottomLeft:
            System.Windows.Point targetPoint3 = new System.Windows.Point(Canvas.GetLeft((UIElement) this.ImageArea) + this.ImageArea.Width, Canvas.GetTop((UIElement) this.ImageArea));
            position.X = Math.Min(position.X, targetPoint3.X);
            position.Y = Math.Max(position.Y, targetPoint3.Y);
            this.SetImageArea(targetPoint3, position, this._pointMode);
            break;
          case ImageClipper.DragPointMode.BottomRight:
            System.Windows.Point targetPoint4 = new System.Windows.Point(Canvas.GetLeft((UIElement) this.ImageArea), Canvas.GetTop((UIElement) this.ImageArea));
            position.X = Math.Max(position.X, targetPoint4.X);
            position.Y = Math.Max(position.Y, targetPoint4.Y);
            this.SetImageArea(targetPoint4, position, this._pointMode);
            break;
          case ImageClipper.DragPointMode.Top:
            System.Windows.Point targetPoint5 = new System.Windows.Point(Canvas.GetLeft((UIElement) this.ImageArea), Canvas.GetTop((UIElement) this.ImageArea) + this.ImageArea.Height);
            position.X = targetPoint5.X;
            position.Y = Math.Min(position.Y, targetPoint5.Y);
            this.SetImageArea(targetPoint5, position, this._pointMode);
            break;
          case ImageClipper.DragPointMode.Bottom:
            System.Windows.Point targetPoint6 = new System.Windows.Point(Canvas.GetLeft((UIElement) this.ImageArea), Canvas.GetTop((UIElement) this.ImageArea));
            position.X = targetPoint6.X;
            position.Y = Math.Max(position.Y, targetPoint6.Y);
            this.SetImageArea(targetPoint6, position, this._pointMode);
            break;
          case ImageClipper.DragPointMode.Left:
            System.Windows.Point targetPoint7 = new System.Windows.Point(Canvas.GetLeft((UIElement) this.ImageArea) + this.ImageArea.Width, Canvas.GetTop((UIElement) this.ImageArea));
            position.X = Math.Min(position.X, targetPoint7.X);
            position.Y = targetPoint7.Y;
            this.SetImageArea(targetPoint7, position, this._pointMode);
            break;
          case ImageClipper.DragPointMode.Right:
            System.Windows.Point targetPoint8 = new System.Windows.Point(Canvas.GetLeft((UIElement) this.ImageArea), Canvas.GetTop((UIElement) this.ImageArea));
            position.X = Math.Max(position.X, targetPoint8.X);
            position.Y = targetPoint8.Y;
            this.SetImageArea(targetPoint8, position, this._pointMode);
            break;
        }
        this.TrySetTheme();
      }
      else
      {
        if (this._imageAreaMouseDownPoint.X <= 0.0 || this._imageAreaMouseDownPoint.Y <= 0.0)
          return;
        double val1_1 = this.Container.Width - this.ImageArea.Width;
        double val1_2 = this.Container.Height - this.ImageArea.Height;
        System.Windows.Point position = e.GetPosition((IInputElement) this.Container);
        double val2 = position.X - this._imageAreaMouseDownPoint.X;
        double length1 = Math.Max(Math.Min(val1_1, val2), 0.0);
        double length2 = Math.Max(Math.Min(val1_2, position.Y - this._imageAreaMouseDownPoint.Y), 0.0);
        Canvas.SetLeft((UIElement) this.ImageArea, length1);
        Canvas.SetTop((UIElement) this.ImageArea, length2);
        this.GetInsideRect();
        this.TrySetTheme();
      }
    }

    private void SetImageArea(System.Windows.Point targetPoint, System.Windows.Point point, ImageClipper.DragPointMode pointMode)
    {
      double x = targetPoint.X;
      double num1 = Math.Min(this.Container.Width, Math.Max(point.X, 0.0));
      bool flag1 = pointMode == ImageClipper.DragPointMode.TopLeft || pointMode == ImageClipper.DragPointMode.BottomLeft || pointMode == ImageClipper.DragPointMode.Left;
      double y = targetPoint.Y;
      double num2 = Math.Min(this.Container.Height, Math.Max(point.Y, 0.0));
      bool flag2 = pointMode == ImageClipper.DragPointMode.TopLeft || pointMode == ImageClipper.DragPointMode.TopRight || pointMode == ImageClipper.DragPointMode.Top;
      double num3 = flag1 ? targetPoint.X : this.Container.Width - targetPoint.X;
      double val2 = Math.Min(flag2 ? targetPoint.Y : this.Container.Height - targetPoint.Y, num3 / 1.6);
      double num4 = Math.Max(16.0, Math.Min(Math.Abs(num1 - x) / 1.6, val2));
      double num5 = Math.Max(10.0, Math.Min(Math.Abs(num2 - y), val2));
      if (num4 > num5)
      {
        double num6 = num4 * 1.6;
        this.ImageArea.Width = num6;
        this.ImageArea.Height = num4;
        if (flag1)
          Canvas.SetLeft((UIElement) this.ImageArea, x - num6);
        if (flag2)
          Canvas.SetTop((UIElement) this.ImageArea, Math.Max(y - num4, 0.0));
      }
      else
      {
        double num7 = num5 * 1.6;
        this.ImageArea.Width = num7;
        this.ImageArea.Height = num5;
        if (flag1)
          Canvas.SetLeft((UIElement) this.ImageArea, Math.Max(x - num7, 0.0));
        if (flag2)
          Canvas.SetTop((UIElement) this.ImageArea, y - num5);
      }
      this.GetInsideRect();
    }

    private void GetInsideRect()
    {
      this.RectInside.Rect = new Rect(Canvas.GetLeft((UIElement) this.ImageArea), Canvas.GetTop((UIElement) this.ImageArea), this.ImageArea.Width, this.ImageArea.Height);
    }

    private void OnClipBorderMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._clipBorderMouseDown = true;
      this.CaptureMouse();
    }

    private void OnImageAreaMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._imageAreaMouseDownPoint = e.GetPosition((IInputElement) this.ImageArea);
    }

    private void OnClipMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this._imageAreaMouseDownPoint.X > 0.0 || this._imageAreaMouseDownPoint.Y > 0.0 || this.IsMouseCaptured)
        this.GetThemeColor();
      this._clipBorderMouseDown = false;
      this._imageAreaMouseDownPoint = new System.Windows.Point(-1.0, -1.0);
      if (!this.IsMouseCaptured)
        return;
      this.ReleaseMouseCapture();
    }

    private void TrySetTheme(bool widthImage = false)
    {
      Rect currentLocation = this.GetCurrentLocation();
      App.Window.SetCustomTheme(widthImage ? this._bitmapImage : (BitmapImage) null, currentLocation);
    }

    public Rect GetCurrentLocation()
    {
      double width1 = this.Container.Width;
      double height1 = this.Container.Height;
      double x = Canvas.GetLeft((UIElement) this.ImageArea) / width1;
      double num1 = Canvas.GetTop((UIElement) this.ImageArea) / height1;
      double num2 = this.ImageArea.Width / width1;
      double num3 = this.ImageArea.Height / height1;
      double y = num1;
      double width2 = num2;
      double height2 = num3;
      return new Rect(x, y, width2, height2);
    }

    private void LoadBitmap(string imagePath)
    {
      this.SourceImage.Source = (ImageSource) null;
      this._bitmapImage = (BitmapImage) null;
      try
      {
        if (!File.Exists(imagePath))
          return;
        BitmapImage source = new BitmapImage();
        source.BeginInit();
        source.CacheOption = BitmapCacheOption.OnLoad;
        source.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
        using (Stream stream = (Stream) new MemoryStream(File.ReadAllBytes(imagePath)))
        {
          source.StreamSource = stream;
          source.EndInit();
          source.Freeze();
          this._bitmapImage = source;
          this._colors = ImageColorUtils.GetPixels((BitmapSource) source);
        }
      }
      catch (ArgumentException ex)
      {
      }
      catch (IOException ex)
      {
      }
    }

    private void ImageClipper_OnMouseLeave(object sender, MouseEventArgs e)
    {
      Mouse.OverrideCursor = (Cursor) null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/theme/imageclipper.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseMove += new MouseEventHandler(this.OnClipMouseMove);
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClipMouseUp);
          ((UIElement) target).MouseLeave += new MouseEventHandler(this.ImageClipper_OnMouseLeave);
          break;
        case 2:
          this.SourceImage = (Image) target;
          break;
        case 3:
          this.MaskPath = (System.Windows.Shapes.Path) target;
          break;
        case 4:
          this.RectOutside = (RectangleGeometry) target;
          break;
        case 5:
          this.RectInside = (RectangleGeometry) target;
          break;
        case 6:
          this.Container = (Canvas) target;
          break;
        case 7:
          this.ImageArea = (Border) target;
          this.ImageArea.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnImageAreaMouseDown);
          break;
        case 8:
          this.ClipBorder = (Border) target;
          this.ClipBorder.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnClipBorderMouseDown);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate void CutImageDelegate(BitmapSource bit);

    private enum DragPointMode
    {
      TopLeft,
      TopRight,
      BottomLeft,
      BottomRight,
      Top,
      Bottom,
      Left,
      Right,
    }
  }
}
