// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ThemeUtil
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Files;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc.ColorSelector;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ThemeUtil
  {
    private static readonly Dictionary<string, (BitmapImage, DateTime, DateTime)> LoadedImages = new Dictionary<string, (BitmapImage, DateTime, DateTime)>();
    private static readonly Dictionary<string, BitmapImage> LargeImages = new Dictionary<string, BitmapImage>();
    private static readonly BitmapImage ImageLoading = new BitmapImage(new Uri("pack://application:,,,/Assets/Attachment_Icon/ImageLoading.png"));
    private static readonly BitmapImage ImageLoadFailed = new BitmapImage(new Uri("pack://application:,,,/Assets/Attachment_Icon/ImageLoadFailed.png"));
    private static readonly BitmapImage ImageLoadingDark = new BitmapImage(new Uri("pack://application:,,,/Assets/Attachment_Icon/ImageLoadingDark.png"));
    private static readonly BitmapImage ImageLoadFailedDark = new BitmapImage(new Uri("pack://application:,,,/Assets/Attachment_Icon/ImageLoadFailedDark.png"));
    private static readonly HashSet<string> Themes = new HashSet<string>()
    {
      "white",
      "blue",
      "pink",
      "black",
      "green",
      "gray",
      "yellow",
      "dark",
      "spring",
      "summer",
      "autumn",
      "winter",
      "beijing",
      "hangzhou",
      "london",
      "moscow",
      "sanfrancisco",
      "seoul",
      "shanghai",
      "sydney",
      "tokyo",
      "newyork",
      "desert",
      "structure",
      "blacksea",
      "leaves",
      "birds",
      "guangzhou",
      "shenzhen",
      "cairo",
      "losangeles",
      "custom",
      "dawn",
      "blossom",
      "frozen",
      "meadow",
      "silence"
    };
    private static Dictionary<string, SolidColorBrush> _brushes = new Dictionary<string, SolidColorBrush>();

    static ThemeUtil() => ThemeUtil.InitPlaceholder();

    public static SolidColorBrush GetPrimaryColor(double opacity)
    {
      object resource = Application.Current?.FindResource((object) "ColorPrimary");
      if (resource == null || !(resource is System.Windows.Media.Color color))
        return new SolidColorBrush(Colors.Transparent);
      SolidColorBrush primaryColor = new SolidColorBrush(color);
      primaryColor.Opacity = opacity;
      return primaryColor;
    }

    public static Size GetImageSize(string path)
    {
      double width = 1080.0;
      double height = 608.0;
      if (FileUtils.FileEmptyOrNotExists(path))
        return new Size(width, height);
      try
      {
        if (ThemeUtil.LoadedImages.ContainsKey(path))
        {
          BitmapImage bitmapImage = ThemeUtil.LoadedImages[path].Item1;
          width = bitmapImage.Width;
          height = bitmapImage.Height;
        }
        else
        {
          using (FileStream bitmapStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
          {
            BitmapDecoder bitmapDecoder = BitmapDecoder.Create((Stream) bitmapStream, BitmapCreateOptions.IgnoreColorProfile, BitmapCacheOption.Default);
            height = bitmapDecoder.Frames[0].Height;
            width = bitmapDecoder.Frames[0].Width;
          }
        }
      }
      catch (Exception ex)
      {
        UtilLog.Warn(string.Format("ThemeUtil.GetImageSize Exception {0}", (object) ex));
      }
      return new Size(width, height);
    }

    public static void TryClearImageCached(bool force = false)
    {
      List<string> stringList = new List<string>();
      DateTime dateTime = DateTime.Now.AddMinutes(force ? 0.0 : -2.0);
      foreach (KeyValuePair<string, (BitmapImage, DateTime, DateTime)> loadedImage in ThemeUtil.LoadedImages)
      {
        if (loadedImage.Value.Item3 < dateTime)
          stringList.Add(loadedImage.Key);
      }
      stringList.ForEach((Action<string>) (x => ThemeUtil.LoadedImages.Remove(x)));
      ThemeUtil.LargeImages.Clear();
    }

    public static BitmapImage GetImage(string path, int width = -1, BitmapCacheOption option = BitmapCacheOption.Default)
    {
      if (FileUtils.FileEmptyOrNotExists(path))
        return ThemeUtil.GetPlaceholderAttachmentImage();
      DateTime dateTime1 = FileUtils.FileModifyTime(path);
      BitmapImage image1;
      if (ThemeUtil.LargeImages.TryGetValue(path, out image1))
        return image1;
      string key = path + width.ToString();
      if (ThemeUtil.LoadedImages.ContainsKey(key))
      {
        (BitmapImage image2, DateTime dateTime2, DateTime _) = ThemeUtil.LoadedImages[key];
        if (dateTime2 == dateTime1)
        {
          ThemeUtil.LoadedImages[key] = (image2, dateTime2, DateTime.Now);
          return image2;
        }
      }
      BitmapImage image3 = ImageUtils.GetImageByUrl(path, width, option);
      double num1 = image3.Width * image3.Height;
      if (num1 > 10000000.0)
      {
        double num2 = Math.Sqrt(num1 / 10000000.0);
        width = (int) (image3.Width / num2);
        image3 = new BitmapImage();
        image3.BeginInit();
        image3.UriSource = new Uri(path);
        image3.DecodePixelWidth = width;
        image3.EndInit();
        image3.Freeze();
        ThemeUtil.LargeImages[path] = image3;
      }
      else
        ThemeUtil.LoadedImages[key] = (image3, dateTime1, DateTime.Now);
      return image3;
    }

    private static void InitPlaceholder()
    {
      if (ThemeUtil.ImageLoading.CanFreeze)
        ThemeUtil.ImageLoading.Freeze();
      if (ThemeUtil.ImageLoadFailed.CanFreeze)
        ThemeUtil.ImageLoadFailed.Freeze();
      if (ThemeUtil.ImageLoadingDark.CanFreeze)
        ThemeUtil.ImageLoadingDark.Freeze();
      if (!ThemeUtil.ImageLoadFailed.CanFreeze)
        return;
      ThemeUtil.ImageLoadFailedDark.Freeze();
    }

    public static BitmapImage GetPlaceholderAttachmentImage(bool isFailed = false)
    {
      return string.Compare(LocalSettings.Settings.ThemeId, "dark", StringComparison.OrdinalIgnoreCase) != 0 ? (isFailed ? ThemeUtil.ImageLoadFailed : ThemeUtil.ImageLoading) : (isFailed ? ThemeUtil.ImageLoadFailedDark : ThemeUtil.ImageLoadingDark);
    }

    public static string GetRandomColor()
    {
      int count = ColorItemSelector.MoreColorList.Count;
      int index = Math.Min(Math.Max(new Random().Next(0, count - 1), 0), count - 1);
      return ColorItemSelector.MoreColorList[index];
    }

    public static System.Windows.Media.Color GetColorValue(string key)
    {
      object resource = Application.Current?.TryFindResource((object) key);
      return resource != null ? (System.Windows.Media.Color) resource : Colors.Transparent;
    }

    public static ThemeUtil.Dpi GetDpiFromVisual(Visual visual)
    {
      PresentationSource presentationSource = PresentationSource.FromVisual((Visual) App.Window);
      double x = 96.0;
      double y = 96.0;
      if (presentationSource?.CompositionTarget != null)
      {
        x = 96.0 * presentationSource.CompositionTarget.TransformToDevice.M11;
        y = 96.0 * presentationSource.CompositionTarget.TransformToDevice.M22;
      }
      return new ThemeUtil.Dpi(x, y);
    }

    public static WriteableBitmap SaveAsWriteableBitmap(FrameworkElement element)
    {
      if (element == null)
        return (WriteableBitmap) null;
      ThemeUtil.Dpi dpiFromVisual = ThemeUtil.GetDpiFromVisual((Visual) element);
      double num1 = dpiFromVisual.X / 96.0;
      double num2 = dpiFromVisual.X / 96.0;
      Size size = double.IsNaN(element.ActualHeight) || double.IsNaN(element.ActualWidth) ? new Size(element.Width, element.Height) : new Size(element.Width, element.Height);
      element.Measure(size);
      element.Arrange(new Rect(size));
      element.UpdateLayout();
      RenderTargetBitmap source = new RenderTargetBitmap((int) (size.Width * num1), (int) (size.Height * num2), dpiFromVisual.X, dpiFromVisual.Y, PixelFormats.Pbgra32);
      source.Render((Visual) element);
      return new WriteableBitmap((BitmapSource) source);
    }

    public static bool SaveBitmapToPng(string filePath, BitmapSource bitmapSource)
    {
      try
      {
        PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
        pngBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
          pngBitmapEncoder.Save((Stream) fileStream);
      }
      catch (Exception ex)
      {
        UtilLog.Error("ThemeUtil.SaveBitmapToJpg failed" + ex.Message);
        return false;
      }
      return true;
    }

    public static SolidColorBrush GetColorInString(string color)
    {
      try
      {
        if (color != null && (color.Length == 6 || color.Length == 8) && !color.StartsWith("#"))
          color = "#" + color;
        return new BrushConverter().ConvertFromString(color) as SolidColorBrush;
      }
      catch (Exception ex)
      {
        UtilLog.Info("GetColorInString Error + color:" + color);
        return Brushes.Black;
      }
    }

    public static SolidColorBrush GetAlphaColor(string color, int percent)
    {
      if (color.StartsWith("#"))
      {
        string str1 = Convert.ToString(256 * percent / 100, 16).ToUpper();
        if (str1.Length == 1)
          str1 = "0" + str1;
        string str2 = "#" + str1;
        if (str2.Length == 3)
        {
          color = color.Replace("#", "");
          if (color.Length > 6)
            color = color.Substring(color.Length - 6, 6);
          color = str2 + color;
          return ThemeUtil.GetColorInString(color);
        }
      }
      return ThemeUtil.GetColorInString(color);
    }

    public static bool IsEmptyColor(string color)
    {
      return string.IsNullOrEmpty(color) || color.ToLowerInvariant() == "transparent";
    }

    public static Style GetStyle(string key)
    {
      return Application.Current != null && Application.Current?.TryFindResource((object) key) is Style resource ? resource : (Style) null;
    }

    public static SolidColorBrush GetColor(ResourceDictionary dict, string key)
    {
      return dict.Contains((object) key) ? dict[(object) key] as SolidColorBrush : ThemeUtil.GetColor(key);
    }

    public static SolidColorBrush TryGetColor(string key, SolidColorBrush defaultColor = null)
    {
      try
      {
        return Application.Current != null && Application.Current?.TryFindResource((object) key) is SolidColorBrush resource ? resource : defaultColor ?? new SolidColorBrush(Colors.Black);
      }
      catch (Exception ex)
      {
        return (SolidColorBrush) null;
      }
    }

    public static SolidColorBrush GetColor(string key, SolidColorBrush defaultColor = null)
    {
      return Application.Current != null && Application.Current?.TryFindResource((object) key) is SolidColorBrush resource ? resource : defaultColor ?? new SolidColorBrush(Colors.Black);
    }

    public static SolidColorBrush GetTodayColorInMenu(FrameworkElement context)
    {
      string themeId = LocalSettings.Settings.ThemeId;
      return themeId == "Yellow" || themeId == "Pink" || themeId == "Green" ? new SolidColorBrush(Colors.White) : ThemeUtil.GetColor("TextAccentColor");
    }

    public static SolidColorBrush GetDateSelectedColorInMenu(FrameworkElement context)
    {
      string themeId = LocalSettings.Settings.ThemeId;
      return themeId == "Yellow" || themeId == "Pink" || themeId == "Green" ? ThemeUtil.GetColor("LightBorderColor", context) : ThemeUtil.GetPrimaryColor(1.0);
    }

    public static SolidColorBrush GetColor(
      string key,
      FrameworkElement context,
      SolidColorBrush defaultColor = null)
    {
      if (context != null)
      {
        if (context.TryFindResource((object) key) is SolidColorBrush resource1)
          return resource1;
      }
      else if (Application.Current != null && Application.Current?.TryFindResource((object) key) is SolidColorBrush resource2)
        return resource2;
      return defaultColor ?? new SolidColorBrush(Colors.Black);
    }

    public static SolidColorBrush GetColor(
      string key,
      ResourceDictionary dict,
      SolidColorBrush defaultColor = null)
    {
      if (dict != null && dict.Contains((object) key) && dict[(object) key] is SolidColorBrush color)
        return color;
      return Application.Current != null && Application.Current?.TryFindResource((object) key) is SolidColorBrush resource ? resource : defaultColor ?? new SolidColorBrush(Colors.Black);
    }

    public static void SetTheme(string themeId, FrameworkElement context, bool isWhite = false)
    {
      if (themeId == "light")
        ThemeUtil.SetThemeLight(context, isWhite);
      else
        ThemeUtil.SetThemeDark(context);
    }

    private static void SetThemeLight(FrameworkElement context, bool isWhite = false)
    {
      ThemeUtil.AddResourceDictionary(context, "Resource/Themes/Light.xaml");
      if (isWhite)
        ThemeUtil.AddResourceDictionary(context, "Resource/Themes/White.xaml");
      else if (!string.IsNullOrWhiteSpace(LocalSettings.Settings.ThemeId) && LocalSettings.Settings.ThemeId != "Dark" && LocalSettings.Settings.ThemeId != "Custom")
      {
        string src = "Resource/Themes/" + LocalSettings.Settings.ThemeId + ".xaml";
        ThemeUtil.AddResourceDictionary(context, src);
      }
      ThemeUtil.AddResourceDictionary(context, "Resource/Themes/LightNecessaryColor.xaml");
      ThemeUtil.AddResourceDictionary(context, "Resource/icons_light.xaml");
    }

    private static void SetThemeDark(FrameworkElement context)
    {
      ThemeUtil.AddResourceDictionary(context, "Resource/Themes/Dark.xaml");
      ThemeUtil.AddResourceDictionary(context, "Resource/icons_dark.xaml");
    }

    private static void AddResourceDictionary(FrameworkElement context, string src)
    {
      context.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri(src, UriKind.Relative)
      });
    }

    public static Geometry GetTaskIconGeometry(
      DisplayType type,
      int status,
      string kind,
      int calKind)
    {
      switch (status)
      {
        case -1:
          return Utils.GetIconData("IcAbandoned");
        case 0:
          switch (type)
          {
            case DisplayType.Task:
              switch (kind)
              {
                case "TEXT":
                  return Utils.GetIconData("IcCheckBox");
                case "CHECKLIST":
                  return Utils.GetIconData("IcCheckList");
              }
              break;
            case DisplayType.CheckItem:
              return Utils.GetIconData("IcCheckItem");
            case DisplayType.Agenda:
              return Utils.GetIconData("IcAgendaItem");
            case DisplayType.Event:
              switch (calKind)
              {
                case 1:
                  return Utils.GetIconData("IcGoogleItem");
                case 2:
                  return Utils.GetIconData("IcOutlookItem");
                case 3:
                  return Utils.GetIconData("IcExchangeItem");
                case 4:
                  return Utils.GetIconData("IcICloudItem");
                case 5:
                  return Utils.GetIconData("IcCalDavItem");
                default:
                  return Utils.GetIconData("IcSubscribeItem");
              }
            case DisplayType.Note:
              return Utils.GetIconData("IcNoteItem");
            case DisplayType.Course:
              return Utils.GetIconData("IcCourseItem");
          }
          return Utils.GetIconData("IcCheckBox");
        default:
          return Utils.GetIconData("IcChecked");
      }
    }

    public static bool ExistTheme(string themeId) => ThemeUtil.Themes.Contains(themeId.ToLower());

    public static string GetDarkColorString(string key)
    {
      ResourceDictionary resourceDictionary = new ResourceDictionary()
      {
        Source = new Uri("Resource/Themes/Light.xaml", UriKind.Relative)
      };
      if (!resourceDictionary.Contains((object) key) || !(resourceDictionary[(object) key] is System.Windows.Media.Color _))
        ;
      return (string) null;
    }

    public static string GetColorString(string key, bool inDark)
    {
      System.Windows.Media.Color? nullable = new System.Windows.Media.Color?();
      if (inDark)
      {
        ResourceDictionary resourceDictionary = new ResourceDictionary()
        {
          Source = new Uri("Resource/Themes/Light.xaml", UriKind.Relative)
        };
        if (resourceDictionary.Contains((object) key))
          nullable = resourceDictionary[(object) key] as System.Windows.Media.Color?;
      }
      else if (Application.Current != null && Application.Current?.TryFindResource((object) key) is System.Windows.Media.Color resource)
        nullable = new System.Windows.Media.Color?(resource);
      if (!nullable.HasValue)
        return (string) null;
      string str = nullable.Value.ToString();
      return "#" + str.Substring(str.Length - 6, 6);
    }

    public static SolidColorBrush GetColorInDict(string color, int percent)
    {
      string key = string.IsNullOrEmpty(color) ? "transparent" : color + percent.ToString();
      if (ThemeUtil._brushes.ContainsKey(key))
        return ThemeUtil._brushes[key];
      if (color != null && (color.Length == 6 || color.Length == 8) && !color.StartsWith("#"))
        color = "#" + color;
      SolidColorBrush solidColorBrush = key == "transparent" ? new SolidColorBrush(Colors.Transparent) : ThemeUtil.GetAlphaColor(color, percent);
      ThemeUtil._brushes[key] = solidColorBrush;
      return ThemeUtil._brushes[key];
    }

    public static SolidColorBrush GetBlendColor(
      string color1,
      string color2,
      float alpha1,
      float alpha2)
    {
      string key = color1 + color2 + alpha1.ToString() + alpha2.ToString();
      if (ThemeUtil._brushes.ContainsKey(key))
        return ThemeUtil._brushes[key];
      if (color1.Length > 6)
      {
        if (color2.Length > 6)
        {
          try
          {
            SolidColorBrush blendColor = ColorUtils.GetBlendColor(color1, color2, alpha1, alpha2);
            ThemeUtil._brushes[key] = blendColor;
            return blendColor;
          }
          catch (Exception ex)
          {
          }
        }
      }
      return ThemeUtil.GetColorInString(color1);
    }

    public static void TrySetSystemTheme(bool force = false)
    {
      bool flag = ThemeUtil.IsSystemDark();
      if (!force && LocalSettings.Settings.ThemeId == "Dark" == flag)
        return;
      if (flag)
      {
        App.Instance.LoadTheme(LocalSettings.Settings.ThemeId, "Dark");
        LocalSettings.Settings.ThemeId = "Dark";
      }
      else
        App.Instance.LoadTargetTheme(LocalSettings.Settings.ExtraSettings.AppTheme == "Dark" ? "White" : LocalSettings.Settings.ExtraSettings.AppTheme, true);
      DataChangedNotifier.NotifyIsDarkChanged();
    }

    public static bool IsSystemDark()
    {
      object obj = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize")?.GetValue("AppsUseLightTheme");
      return obj != null && (int) obj <= 0;
    }

    public static ResourceDictionary GetImageDictionary(bool isDark)
    {
      return new ResourceDictionary()
      {
        Source = new Uri("../Resource/" + (isDark ? "icons_dark" : "icons_light") + ".xaml", UriKind.Relative)
      };
    }

    internal static string GetNoAlphaColorString(string colorString)
    {
      if (string.IsNullOrEmpty(colorString) || colorString.ToLower(CultureInfo.InvariantCulture) == "transparent")
        return colorString;
      if ((colorString.Length == 8 || colorString.Length == 6) && !colorString.StartsWith("#"))
        colorString = "#" + colorString;
      return colorString.Length == 9 && colorString.StartsWith("#") ? "#" + colorString.Substring(3) : colorString;
    }

    public static bool IsDark() => LocalSettings.Settings.ThemeId == "Dark";

    public struct Dpi
    {
      public double X { get; set; }

      public double Y { get; set; }

      public Dpi(double x, double y)
      {
        this.X = x;
        this.Y = y;
      }
    }
  }
}
