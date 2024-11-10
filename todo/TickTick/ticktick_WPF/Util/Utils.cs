// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Utils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Network;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Widget;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public class Utils
  {
    public const int AM = 0;
    public const int PM = 1;
    public static IToastShowWindow ToastWindow;

    public static bool IsChinese(string content)
    {
      return new Regex("^[\\u4e00-\\u9fa5]{0,}$").IsMatch(content);
    }

    public static bool ContainsChineseInText(string content)
    {
      return !string.IsNullOrEmpty(content) && content.Any<char>((Func<char, bool>) (ch => Utils.IsChinese(ch.ToString())));
    }

    public static async Task<string> GetAvatarUrl(string tempUserCode)
    {
      return await Task.Run<string>((Func<Task<string>>) (async () =>
      {
        if (tempUserCode == null)
          return "../Assets/logo.png";
        UserPublicProfilesModel userInfoByUserCode = await UserPublicProfilesDao.GetUserInfoByUserCode(tempUserCode);
        return userInfoByUserCode == null ? "../Assets/avatar-new.png" : userInfoByUserCode.avatarUrl;
      }));
    }

    public static async Task LogotEmptyDb()
    {
      LocalSettings.Settings.LoginUserId = string.Empty;
      LocalSettings.Settings.LoginUserAuth = string.Empty;
      LocalSettings.Settings.InServerBoxId = string.Empty;
      LocalSettings.Settings.InBoxId = -1;
      LocalSettings.Settings.LoginAvatarUrl = string.Empty;
      await LocalSettings.Settings.Save();
    }

    public static async Task<int> CreatInbox()
    {
      if (!(LocalSettings.Settings.InBoxId.ToString() == "-1"))
        return LocalSettings.Settings.InBoxId;
      ProjectModel projectModel = (await ProjectDao.GetAllProjects()).FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (project => project.Isinbox));
      return await ProjectDao.TryUpdateProject(new ProjectModel()
      {
        name = Utils.GetString("Inbox"),
        Isinbox = true,
        userid = LocalSettings.Settings.LoginUserId,
        id = LocalSettings.Settings.InServerBoxId,
        inAll = true,
        color = projectModel?.color,
        sortOrder = long.MinValue
      });
    }

    public static async Task<TaskReminderModel[]> CopyReminderItem(
      TaskModel repeatTask,
      TaskReminderModel[] items)
    {
      if (items == null)
        return new List<TaskReminderModel>().ToArray();
      List<TaskReminderModel> newItemsList = new List<TaskReminderModel>();
      TaskReminderModel[] taskReminderModelArray = items;
      for (int index = 0; index < taskReminderModelArray.Length; ++index)
      {
        TaskReminderModel taskReminderModel1 = taskReminderModelArray[index];
        TaskReminderModel taskReminderModel2 = new TaskReminderModel()
        {
          id = Utils.GetGuid(),
          Taskid = repeatTask._Id,
          taskserverid = repeatTask.id,
          trigger = taskReminderModel1.trigger
        };
        newItemsList.Add(taskReminderModel2);
        int num = await TaskReminderDao.SaveReminders(taskReminderModel2);
      }
      taskReminderModelArray = (TaskReminderModel[]) null;
      return newItemsList.ToArray();
    }

    public static async Task<AttachmentModel[]> CopyAttachmentItem(
      TaskModel repeatTask,
      string oldTaskid)
    {
      List<AttachmentModel> taskAttachments = await AttachmentDao.GetTaskAttachments(oldTaskid);
      List<AttachmentModel> newItemsList = new List<AttachmentModel>();
      foreach (AttachmentModel attachmentModel in taskAttachments)
      {
        AttachmentModel item = attachmentModel;
        if (item.status == 0)
        {
          AttachmentModel temp = new AttachmentModel()
          {
            id = Utils.GetGuid(),
            refId = item.refId,
            createdTime = item.createdTime,
            taskId = repeatTask.id,
            path = item.path,
            deleted = item.deleted,
            fileName = item.fileName,
            fileType = item.fileType,
            localPath = item.localPath,
            size = item.size
          };
          newItemsList.Add(temp);
          await AttachmentDao.InsertOrUpdateAttachment(temp);
          if (repeatTask.kind != "CHECKLIST")
            repeatTask.content = repeatTask.content?.Replace(item.id, temp.id);
          temp = (AttachmentModel) null;
          item = (AttachmentModel) null;
        }
      }
      AttachmentModel[] array = newItemsList.ToArray();
      newItemsList = (List<AttachmentModel>) null;
      return array;
    }

    public static long GetNowTimeStamp()
    {
      return (long) (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))).TotalSeconds;
    }

    public static long GetNowTimeStampInMills()
    {
      return (long) (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1))).TotalMilliseconds;
    }

    public static DateTime TimeStampToDateTime(string timeStamp)
    {
      return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1).Add(new TimeSpan(long.Parse(timeStamp + "0000000"))));
    }

    public static DateTime TimeStampToDateTime(long timeStamp)
    {
      return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1).AddMilliseconds((double) timeStamp));
    }

    public static bool IsValidEmail(string email)
    {
      try
      {
        if (email.IndexOf(' ') >= 0)
          return false;
        int startIndex = email.IndexOf('@');
        if (email[0] == '_' || startIndex == -1)
          return false;
        int num = email.IndexOf('.', startIndex);
        return startIndex == email.LastIndexOf('@') && num > 0 && num != email.Length - 1 && num == email.LastIndexOf('.') && new MailAddress(email).Address == email;
      }
      catch
      {
        return false;
      }
    }

    public static string GetGuid0(int maxLength = -1)
    {
      string str = Guid.NewGuid().ToString("N").Substring(8);
      return maxLength <= 0 || str.Length <= maxLength ? str : str.Substring(0, maxLength);
    }

    public static string GetGuid()
    {
      try
      {
        return ObjectIdGenerator.GenerateString();
      }
      catch (Exception ex)
      {
        UserActCollectUtils.SendCustomException(ExceptionUtils.BuildExceptionMessage(ex));
        return Utils.GetGuid0();
      }
    }

    public static string GetTriggerValue(Dictionary<string, int> dict)
    {
      string str = "TRIGGER:-PT";
      int num1 = dict.ContainsKey("M") ? dict["M"] : 0;
      int num2 = dict.ContainsKey("H") ? dict["H"] : 0;
      int num3 = (dict.ContainsKey("H") ? dict["D"] : 0) * 24 * 60 + num2 * 60 + num1;
      return num3 != 0 ? str + num3.ToString() + "M" : str + "0S";
    }

    public static string GetShortWeekName(int weekday)
    {
      return App.Ci.DateTimeFormat.GetAbbreviatedDayName((DayOfWeek) weekday);
    }

    public static string GetWeekLabel(DayOfWeek day)
    {
      return App.Ci.DateTimeFormat.GetShortestDayName(day);
    }

    public static void LoadDefaultLanguage()
    {
      string str = "zh-CN";
      System.Windows.Application current = System.Windows.Application.Current;
      if (current == null)
        return;
      current.Resources.MergedDictionaries.Add(new ResourceDictionary()
      {
        Source = new Uri("Resource\\StringResource\\StringResource." + str + ".xaml", UriKind.Relative)
      });
    }

    public static void SwitchLanguage()
    {
      System.Windows.Application current1 = System.Windows.Application.Current;
      List<ResourceDictionary> list = current1 != null ? current1.Resources.MergedDictionaries.ToList<ResourceDictionary>() : (List<ResourceDictionary>) null;
      string languageName = App.Ci.ToString();
      if (Utils.IsDida() && languageName == "zh-CN" || !Utils.IsDida() && languageName == "en-US")
      {
        AddExtra();
      }
      else
      {
        string str = App.Ci.ToString();
        if (str != null && str.Length == 5)
        {
          switch (str[3])
          {
            case 'B':
              if (str == "pt-BR")
              {
                languageName = "pt";
                break;
              }
              break;
            case 'C':
              if (str == "cs-CZ")
              {
                languageName = "cs";
                break;
              }
              break;
            case 'D':
              if (str == "de-DE")
              {
                languageName = "de";
                break;
              }
              break;
            case 'E':
              if (str == "es-ES")
              {
                languageName = "es";
                break;
              }
              break;
            case 'F':
              if (str == "fr-FR")
              {
                languageName = "fr";
                break;
              }
              break;
            case 'H':
              if (str == "hu-HU")
              {
                languageName = "hu";
                break;
              }
              break;
            case 'I':
              if (str == "it-IT")
              {
                languageName = "it";
                break;
              }
              break;
            case 'J':
              if (str == "ja-JP")
              {
                languageName = "ja";
                break;
              }
              break;
            case 'K':
              if (str == "ko-KR")
              {
                languageName = "ko";
                break;
              }
              break;
            case 'L':
              switch (str)
              {
                case "lv-LV":
                  languageName = "lv";
                  break;
                case "lt-LT":
                  languageName = "lt";
                  break;
              }
              break;
            case 'N':
              if (str == "nl-NL")
              {
                languageName = "nl";
                break;
              }
              break;
            case 'P':
              if (str == "pl-PL")
              {
                languageName = "pl";
                break;
              }
              break;
            case 'R':
              if (str == "ru-RU")
              {
                languageName = "ru";
                break;
              }
              break;
            case 'S':
              switch (str)
              {
                case "sk-SK":
                  languageName = "sk";
                  break;
                case "sv-SE":
                  languageName = "sv";
                  break;
              }
              break;
            case 'U':
              if (str == "uk-UA")
              {
                languageName = "uk";
                break;
              }
              break;
          }
        }
        string uriString = "Resource\\StringResource\\StringResource." + languageName + ".xaml";
        if (languageName != "en-US")
        {
          string defaultDict = "Resource\\StringResource\\StringResource.en-US.xaml";
          if (languageName == "zh-TW")
            defaultDict = "Resource\\StringResource\\StringResource.zh-CN.xaml";
          ResourceDictionary resourceDictionary = list != null ? list.FirstOrDefault<ResourceDictionary>((Func<ResourceDictionary, bool>) (d => d.Source.OriginalString.Equals(defaultDict))) : (ResourceDictionary) null;
          if (resourceDictionary != null)
          {
            System.Windows.Application.Current?.Resources.MergedDictionaries.Remove(resourceDictionary);
            System.Windows.Application.Current?.Resources.MergedDictionaries.Add(resourceDictionary);
          }
          else
          {
            System.Windows.Application current2 = System.Windows.Application.Current;
            if (current2 != null)
              current2.Resources.MergedDictionaries.Add(new ResourceDictionary()
              {
                Source = new Uri(defaultDict, UriKind.Relative)
              });
          }
        }
        try
        {
          System.Windows.Application current3 = System.Windows.Application.Current;
          if (current3 != null)
            current3.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
              Source = new Uri(uriString, UriKind.Relative)
            });
        }
        catch (Exception ex)
        {
        }
        AddExtra();
      }

      void AddExtra()
      {
        if (!(languageName == "zh-CN") && !(languageName == "zh-TW") && !(languageName == "en-US"))
          return;
        string uriString = string.Format("Resource\\StringResource\\String.D.{0}.xaml", (object) App.Ci);
        System.Windows.Application current = System.Windows.Application.Current;
        if (current == null)
          return;
        current.Resources.MergedDictionaries.Add(new ResourceDictionary()
        {
          Source = new Uri(uriString, UriKind.Relative)
        });
      }
    }

    public static long GetUserLimit(Constants.LimitKind kind) => LimitCache.GetLimitByKey(kind);

    [Obsolete("文不对题，用ThreadUtil.DetachedRunOnUiThread")]
    public static void RunOnUiThread(Dispatcher dispatcher, Action action)
    {
      if (action == null)
        return;
      new Thread((ThreadStart) (() => dispatcher?.Invoke(DispatcherPriority.Normal, (Delegate) action))).Start();
    }

    public static void RunOnBackgroundThread(Dispatcher dispatcher, Action action)
    {
      if (action == null)
        return;
      new Thread((ThreadStart) (() => dispatcher?.Invoke(DispatcherPriority.Background, (Delegate) action))).Start();
    }

    public static Geometry GetIconData(string index, ResourceDictionary context = null)
    {
      if (string.IsNullOrEmpty(index))
        return (Geometry) null;
      return context != null ? (!(context[(object) index] is System.Windows.Shapes.Path path) ? (Geometry) null : path.Data) : (!(System.Windows.Application.Current?.FindResource((object) index) is System.Windows.Shapes.Path resource) ? (Geometry) null : resource.Data);
    }

    public static DrawingImage GetImageSource(string index, ResourceDictionary context = null)
    {
      if (string.IsNullOrEmpty(index))
        return (DrawingImage) null;
      return context != null ? context[(object) index] as DrawingImage : System.Windows.Application.Current?.FindResource((object) index) as DrawingImage;
    }

    public static DrawingImage GetImageSource(string index, FrameworkElement element)
    {
      if (string.IsNullOrEmpty(index))
        return (DrawingImage) null;
      return element != null && element.FindResource((object) index) is DrawingImage resource ? resource : System.Windows.Application.Current?.FindResource((object) index) as DrawingImage;
    }

    public static async Task<BitmapImage> GetNetWorkImageAsync(string url, int? width = null)
    {
      if (string.IsNullOrEmpty(url?.Trim()))
        return (BitmapImage) null;
      BitmapImage image = new BitmapImage();
      await Task.Run((Action) (() =>
      {
        try
        {
          Stream responseStream = WebRequest.Create(url).GetResponse().GetResponseStream();
          if (responseStream == null)
            return;
          Utils.BitmapToBitmapImage(new Bitmap(System.Drawing.Image.FromStream(responseStream)), width, image);
        }
        catch (Exception ex)
        {
        }
      }));
      return image;
    }

    public static BitmapImage GetNetWorkImage(string url, int? width = null)
    {
      if (string.IsNullOrEmpty(url?.Trim()))
        return (BitmapImage) null;
      try
      {
        Stream responseStream = WebRequest.Create(url).GetResponse().GetResponseStream();
        if (responseStream != null)
          return Utils.BitmapToBitmapImage(new Bitmap(System.Drawing.Image.FromStream(responseStream)), width);
      }
      catch (Exception ex)
      {
        return (BitmapImage) null;
      }
      return (BitmapImage) null;
    }

    private static BitmapImage BitmapToBitmapImage(Bitmap bitmap, int? width = null, BitmapImage image = null)
    {
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      Bitmap bitmap1 = new Bitmap(bitmap.Width, bitmap.Height);
      for (int x = 0; x < bitmap.Width; ++x)
      {
        for (int y = 0; y < bitmap.Height; ++y)
        {
          System.Drawing.Color pixel = bitmap.GetPixel(x, y);
          System.Drawing.Color color = System.Drawing.Color.FromArgb((int) pixel.R, (int) pixel.G, (int) pixel.B);
          bitmap1.SetPixel(x, y, color);
        }
      }
      MemoryStream ms = new MemoryStream();
      bitmap1.Save((Stream) ms, ImageFormat.Bmp);
      stopwatch.Stop();
      bool init = false;
      System.Windows.Application.Current.Dispatcher.BeginInvoke((Delegate) (() =>
      {
        try
        {
          image = image ?? new BitmapImage();
          image.BeginInit();
          image.StreamSource = (Stream) new MemoryStream(ms.ToArray());
          if (width.HasValue && bitmap.Width > width.Value)
            image.DecodePixelWidth = width.Value;
          image.EndInit();
          image.Freeze();
        }
        finally
        {
          init = true;
        }
      }));
      while (!init)
        Thread.Sleep(100);
      ms.Close();
      ms.Dispose();
      return image;
    }

    private async void TryDownLoadUrlImage(string url, string filePath)
    {
      new Thread((ThreadStart) (async () =>
      {
        try
        {
          byte[] buffer = new WebClient().DownloadData(url);
          BitmapImage source = new BitmapImage();
          using (MemoryStream memoryStream = new MemoryStream(buffer))
          {
            source.BeginInit();
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.StreamSource = (Stream) memoryStream;
            source.EndInit();
            source.Freeze();
          }
          if (source.IsDownloading || source.Height <= 1.0 || source.Width <= 1.0)
            return;
          BitmapEncoder bitmapEncoder = (BitmapEncoder) new PngBitmapEncoder();
          bitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource) source));
          using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
          {
            bitmapEncoder.Save((Stream) fileStream);
            fileStream.Close();
          }
        }
        catch (NotSupportedException ex)
        {
        }
        catch (Exception ex)
        {
        }
      })).Start();
    }

    public static void GetPicFromControl(FrameworkElement element, string type, string outputPath)
    {
      RenderTargetBitmap source = (RenderTargetBitmap) null;
      try
      {
        int num = element.ActualHeight > 15000.0 ? 1 : 2;
        source = new RenderTargetBitmap((int) element.ActualWidth * num, (int) (element.ActualHeight + 100.0) * num, (double) (96 * num), (double) (96 * num), PixelFormats.Pbgra32);
        source.Render((Visual) element);
        BitmapEncoder bitmapEncoder;
        switch (type.ToUpper())
        {
          case "BMP":
            bitmapEncoder = (BitmapEncoder) new BmpBitmapEncoder();
            break;
          case "GIF":
            bitmapEncoder = (BitmapEncoder) new GifBitmapEncoder();
            break;
          case "JPEG":
            bitmapEncoder = (BitmapEncoder) new JpegBitmapEncoder();
            break;
          case "PNG":
            bitmapEncoder = (BitmapEncoder) new PngBitmapEncoder();
            break;
          case "TIFF":
            bitmapEncoder = (BitmapEncoder) new TiffBitmapEncoder();
            break;
          default:
            bitmapEncoder = (BitmapEncoder) new PngBitmapEncoder();
            break;
        }
        if (string.IsNullOrEmpty(outputPath))
          return;
        bitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource) source));
        string directoryName = System.IO.Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
          Directory.CreateDirectory(directoryName);
        FileStream fileStream = System.IO.File.Create(outputPath);
        bitmapEncoder.Save((Stream) fileStream);
        fileStream.Close();
        source.Clear();
      }
      catch (Exception ex)
      {
        source?.Clear();
      }
    }

    public static Geometry GetIcon(string index)
    {
      try
      {
        return System.Windows.Application.Current?.FindResource((object) index) is System.Windows.Shapes.Path resource ? resource.Data : (Geometry) null;
      }
      catch (Exception ex)
      {
        return (Geometry) null;
      }
    }

    public static string GetString(string key, string defaultText = null)
    {
      object resource = System.Windows.Application.Current?.TryFindResource((object) key);
      return resource != null ? resource.ToString() : defaultText ?? key;
    }

    public static string GetStringFormat(string key, params object[] args)
    {
      string format = Utils.GetString(key);
      return format != key ? string.Format(format, args) : format;
    }

    public static bool IsNetworkAvailable()
    {
      try
      {
        return Utils.CheckNet();
      }
      catch (Exception ex)
      {
        return true;
      }
    }

    [DllImport("wininet.dll")]
    private static extern bool InternetGetConnectedState(out int description, int reservedValue);

    private static bool CheckNet() => Utils.InternetGetConnectedState(out int _, 0);

    public static string BuildReminderString(int num, string unit, string time = "")
    {
      return string.Format(Utils.GetString("ReminderTemplate"), (object) num, (object) unit, (object) time);
    }

    public static bool IsEmptyDate(DateTime date) => date.Year == 1;

    public static long GetTimeStamp(DateTime? time)
    {
      if (!time.HasValue)
        return 0;
      DateTime localTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
      return (long) (time.Value - localTime).TotalMilliseconds;
    }

    public static long GetTimeStampInSecond(DateTime? time)
    {
      if (!time.HasValue)
        return 0;
      DateTime localTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
      return (long) (time.Value - localTime).TotalSeconds;
    }

    public static bool IsEmptyDate(DateTime? date) => !date.HasValue || date.Value.Year == 1;

    public static async Task<List<ProjectUsersMode>> GetRecentProjectUsersList()
    {
      string projectUsersList = await Communicator.GetRecentProjectUsersList();
      if (!string.IsNullOrEmpty(projectUsersList))
      {
        try
        {
          return JsonConvert.DeserializeObject<List<ProjectUsersMode>>(projectUsersList);
        }
        catch (Exception ex)
        {
        }
      }
      return new List<ProjectUsersMode>();
    }

    public static string ToVersion(double version)
    {
      string str = version.ToString((IFormatProvider) CultureInfo.InvariantCulture);
      string version1 = string.Empty;
      foreach (char ch in str)
        version1 = version1 + ch.ToString() + ".";
      if (version1.EndsWith("."))
        version1 = version1.Substring(0, version1.Length - 1);
      return version1;
    }

    public static double VersionNumToDouble()
    {
      int num = System.Windows.Application.ResourceAssembly.GetName().Version.Major;
      string str1 = num.ToString();
      num = System.Windows.Application.ResourceAssembly.GetName().Version.Minor;
      string str2 = num.ToString();
      num = System.Windows.Application.ResourceAssembly.GetName().Version.Build;
      string str3 = num.ToString();
      num = System.Windows.Application.ResourceAssembly.GetName().Version.Revision;
      string str4 = num.ToString();
      return double.Parse(str1 + str2 + str3 + str4);
    }

    public static BitmapImage GetImage(string uri)
    {
      BitmapImage image = new BitmapImage();
      try
      {
        string uriString = Utils.RemoveControlCharacters(uri);
        image.BeginInit();
        image.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
        image.UriSource = new Uri(uriString);
        image.EndInit();
        return image;
      }
      catch (NotSupportedException ex)
      {
      }
      catch (Exception ex)
      {
      }
      return (BitmapImage) null;
    }

    private static string RemoveControlCharacters(string inString)
    {
      if (inString == null)
        return (string) null;
      StringBuilder stringBuilder = new StringBuilder();
      foreach (uint num in inString)
      {
        char ch = (char) ((uint) byte.MaxValue & num);
        switch (ch)
        {
          case '\t':
          case '\n':
          case '\r':
          case ' ':
          case '\u007F':
            continue;
          default:
            stringBuilder.Append(ch);
            continue;
        }
      }
      return stringBuilder.ToString();
    }

    public static long GetResidueAttachmentCount(int nextUploadCount = 0)
    {
      return Utils.GetUserLimit(Constants.LimitKind.DailyUploadNumber) - (long) AttachmentCache.TodayAttachmentCount();
    }

    public static string GetDeviceInfo()
    {
      string str1 = "Windows_" + Utils.GetWindowsVersion();
      string str2 = HttpUtility.UrlEncode(Environment.MachineName);
      return JsonConvert.SerializeObject((object) new DeviceInfoModel()
      {
        platform = "windows",
        os = str1,
        name = str2,
        version = Utils.GetVersion(),
        id = (App.UId ?? Utils.GetGuid0()),
        channel = "website",
        device = (str1 + "x64")
      });
    }

    public static void OutPutJson(object obj)
    {
      JsonTextWriter jsonTextWriter = new JsonTextWriter((TextWriter) new StringWriter());
      jsonTextWriter.Formatting = Formatting.Indented;
      jsonTextWriter.Indentation = 4;
      jsonTextWriter.IndentChar = ' ';
      new JsonSerializer().Serialize((JsonWriter) jsonTextWriter, obj);
    }

    public static string LocalTimeZone { get; private set; }

    public static string GetLocalTimeZone()
    {
      if (!string.IsNullOrEmpty(Utils.LocalTimeZone))
        return Utils.LocalTimeZone;
      Utils.LocalTimeZone = TimeZoneUtils.GetTimeZoneName(TimeZoneInfo.Local);
      return Utils.LocalTimeZone;
    }

    public static Visual GetDescendantByType(Visual element, Type type, string name)
    {
      if (element == null)
        return (Visual) null;
      if (element.GetType() == type && element is FrameworkElement descendantByType1 && descendantByType1.Name == name)
        return (Visual) descendantByType1;
      Visual descendantByType2 = (Visual) null;
      if (element is FrameworkElement frameworkElement)
        frameworkElement.ApplyTemplate();
      for (int childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount((DependencyObject) element); ++childIndex)
      {
        descendantByType2 = Utils.GetDescendantByType(VisualTreeHelper.GetChild((DependencyObject) element, childIndex) as Visual, type, name);
        if (descendantByType2 != null)
          break;
      }
      return descendantByType2;
    }

    public static bool IsTickPackage() => false;

    public static bool IsDida() => true;

    public static bool IsCn(string ci = null)
    {
      ci = string.IsNullOrEmpty(ci) ? App.Ci.ToString() : ci;
      return ci == "zh-CN" || ci == "zh-TW";
    }

    public static bool IsEn() => App.Ci.ToString() == "en-US";

    public static bool IsZhCn() => App.Ci.ToString() == "zh-CN";

    public static bool IsJp() => "ja-JP".Equals(App.Ci.ToString());

    public static bool IsShowHoliday() => Utils.IsJp() || Utils.IsDida();

    public static IToastShowWindow GetToastWindow()
    {
      return Utils.ToastWindow ?? (IToastShowWindow) App.Window;
    }

    public static void Toast(string message)
    {
      if (string.IsNullOrEmpty(message))
        return;
      Utils.GetToastWindow()?.TryToastString((object) null, message);
    }

    public static void ToastInMainWindow(string message)
    {
      if (string.IsNullOrEmpty(message))
        return;
      App.Window?.TryToastString((object) null, message);
    }

    public static Constants.SortType LoadSmartProjectSortType(string index)
    {
      Constants.SortType result;
      System.Enum.TryParse<Constants.SortType>(LocalSettings.Settings[index].ToString(), out result);
      return result;
    }

    public static Constants.SortType LoadSortType(string index)
    {
      Constants.SortType result;
      System.Enum.TryParse<Constants.SortType>(index, out result);
      return result;
    }

    public static async Task StartUpgrade(string type = null)
    {
      Utils.TryProcessStartUrlWithToken("/about/upgrade?" + (string.IsNullOrEmpty(type) ? string.Empty : "from=" + type + "&") + "platform=windows&did=" + App.UId);
    }

    public static async Task<string> GetAutoSignUrl(string destApi)
    {
      string signOnToken = await Communicator.GetSignOnToken();
      return BaseUrl.GetDomainUrl() + "/sign/autoSignOn?token=" + signOnToken + "&dest=" + Utils.UrlEncode0(destApi);
    }

    public static async Task TryProcessStartUrlWithToken(string destApi)
    {
      Utils.TryProcessStartUrl(await Utils.GetAutoSignUrl(destApi));
    }

    public static async void TryProcessStartUrl(string url)
    {
      try
      {
        Process.Start(url);
      }
      catch (Exception ex1)
      {
        try
        {
          Process.Start("IExplore.exe", url);
        }
        catch (Exception ex2)
        {
        }
      }
    }

    public static string GetInboxId() => "inbox" + LocalSettings.Settings.LoginUserId;

    public static bool IsEmptyRepeatFlag(string flag)
    {
      return string.IsNullOrEmpty(flag) || flag == "FREQ=NONE" || flag == "RRULE:FREQ=NONE";
    }

    public static SolidColorBrush GetPriorityColor(int value)
    {
      switch (value)
      {
        case 0:
          return ThemeUtil.GetColor("BaseColorOpacity40");
        case 1:
          return ThemeUtil.GetColor("PriorityLowColor");
        case 3:
          return ThemeUtil.GetColor("PriorityMiddleColor");
        case 5:
          return ThemeUtil.GetColor("PriorityHighColor");
        default:
          return ThemeUtil.GetColor("BaseColorOpacity40");
      }
    }

    public static string GetPriorityName(int value)
    {
      switch (value)
      {
        case 0:
          return Utils.GetString("PriorityNull");
        case 1:
          return Utils.GetString("PriorityLow");
        case 3:
          return Utils.GetString("PriorityMedium");
        case 5:
          return Utils.GetString("PriorityHigh");
        default:
          return Utils.GetString("PriorityNull");
      }
    }

    public static bool IfShiftPressed()
    {
      return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
    }

    public static bool IfCtrlPressed()
    {
      return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
    }

    public static bool IfWinPressed()
    {
      return Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);
    }

    public static bool IfModifierKeyDown() => Keyboard.Modifiers != 0;

    public static System.Windows.Controls.ListViewItem GetListViewItem(System.Windows.Controls.ListView listview, int index)
    {
      try
      {
        if (index < 0 || index >= listview.Items.Count || listview.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
          return (System.Windows.Controls.ListViewItem) null;
        DependencyObject listViewItem = listview.ItemContainerGenerator.ContainerFromIndex(index);
        if (listViewItem == null)
        {
          listview.UpdateLayout();
          listViewItem = listview.ItemContainerGenerator.ContainerFromIndex(index);
        }
        return listViewItem as System.Windows.Controls.ListViewItem;
      }
      catch (Exception ex)
      {
        return (System.Windows.Controls.ListViewItem) null;
      }
    }

    public static string RemoveLastFlag(string data, string flag)
    {
      return !string.IsNullOrEmpty(data) && data.EndsWith(flag) ? data.Substring(0, data.Length - flag.Length) : data;
    }

    public static object GetBoolText(
      object value,
      string positiveKey,
      string negtiveKey,
      string defaultKey)
    {
      return value is bool flag ? (object) Utils.GetString(flag ? positiveKey : negtiveKey) : (object) Utils.GetString(defaultKey);
    }

    public static bool IsPointInTriangle(System.Windows.Point p, System.Windows.Point p1, System.Windows.Point p2, System.Windows.Point p3)
    {
      Vector vector1 = p1 - p;
      Vector vector2 = p2 - p;
      Vector vector3 = p3 - p;
      double num1 = vector1.X * vector2.Y - vector1.Y * vector2.X;
      double num2 = vector2.X * vector3.Y - vector2.Y * vector3.X;
      double num3 = vector3.X * vector1.Y - vector3.Y * vector1.X;
      double num4 = num2;
      return num1 * num4 >= 0.0 && num2 * num3 >= 0.0;
    }

    private static bool IsPointInRect(double x, double y, double width, double height)
    {
      return x >= 0.0 && x <= width && y >= 0.0 && y <= height;
    }

    public static bool IsMouseOver(System.Windows.Input.MouseEventArgs arg, FrameworkElement element)
    {
      System.Windows.Point position = arg.GetPosition((IInputElement) element);
      double x = position.X;
      position = arg.GetPosition((IInputElement) element);
      double y1 = position.Y;
      double actualWidth = element.ActualWidth;
      double actualHeight = element.ActualHeight;
      double y2 = y1;
      double width = actualWidth;
      double height = actualHeight;
      return Utils.IsPointInRect(x, y2, width, height);
    }

    public static bool IsRectInRect(
      double pivotX,
      double pivotY,
      double left,
      double top,
      double right,
      double bottom,
      double width,
      double height)
    {
      double x1 = pivotX - left;
      double num = pivotY - top;
      double x2 = pivotX + right;
      double y1 = pivotY + bottom;
      double y2 = num;
      double width1 = width;
      double height1 = height;
      return Utils.IsPointInRect(x1, y2, width1, height1) && Utils.IsPointInRect(x2, y1, width, height);
    }

    private static bool IsEmptyPoint(System.Windows.Point point)
    {
      return Math.Abs(point.X) <= 0.0 && Math.Abs(point.Y) <= 0.0;
    }

    public static bool CheckMoveDelta(
      FrameworkElement element,
      System.Windows.Input.MouseEventArgs e,
      System.Windows.Point start,
      int delta)
    {
      if (Utils.IsEmptyPoint(start))
        return false;
      System.Windows.Point position = e.GetPosition((IInputElement) element);
      double num1 = position.X - start.X;
      position = e.GetPosition((IInputElement) element);
      double num2 = position.Y - start.Y;
      return Math.Abs(num1) >= (double) delta || Math.Abs(num2) >= (double) delta;
    }

    public static Thickness GetPopupMargin(
      System.Windows.Input.MouseEventArgs arg,
      FrameworkElement element,
      bool modelDayMode = false,
      double verticalOffset = 0.0)
    {
      if (modelDayMode)
        return new Thickness(20.0, 0.0, 20.0, 0.0);
      System.Windows.Point position = arg.GetPosition((IInputElement) element);
      return new Thickness(position.X, position.Y, element.ActualWidth - position.X + 4.0, 0.0);
    }

    public static string GetVersion(string split = "")
    {
      return System.Windows.Application.ResourceAssembly.GetName().Version.Major.ToString() + split + System.Windows.Application.ResourceAssembly.GetName().Version.Minor.ToString() + split + System.Windows.Application.ResourceAssembly.GetName().Version.Build.ToString() + split + System.Windows.Application.ResourceAssembly.GetName().Version.Revision.ToString();
    }

    public static DateTime GetCalendarMonthViewStartDate(DateTime time)
    {
      if (Utils.IsEmptyDate(time))
        time = DateTime.Today;
      DateTime dateTime1 = time.AddDays((double) (1 - time.Day));
      DateTime dateTime2 = dateTime1.AddDays((double) ((int) dateTime1.DayOfWeek * -1)).AddDays((double) Utils.GetWeekFromDiff(new DateTime?(dateTime1)));
      return dateTime2.Month == time.Month && dateTime2.Day != 1 ? dateTime2.AddDays(-7.0) : dateTime2;
    }

    public static int GetWeekFromDiff(DateTime? dateTime = null)
    {
      DateTime dateTime1 = dateTime ?? DateTime.Today;
      switch (LocalSettings.Settings.WeekStartFrom)
      {
        case "Saturday":
          return dateTime1.DayOfWeek != DayOfWeek.Saturday ? -1 : 6;
        case "Monday":
          return dateTime1.DayOfWeek != DayOfWeek.Sunday ? 1 : -6;
        default:
          return 0;
      }
    }

    public static int GetWeekStartDiff(DateTime date)
    {
      return (int) date.DayOfWeek * -1 + Utils.GetWeekFromDiff(new DateTime?(date));
    }

    public static DateTime GetWeekStart(DateTime date)
    {
      return date.AddDays((double) Utils.GetWeekStartDiff(date));
    }

    public static DayOfWeek GetNextDayOfWeek()
    {
      switch (LocalSettings.Settings.WeekStartFrom)
      {
        case "Saturday":
          return DayOfWeek.Saturday;
        case "Monday":
          return DayOfWeek.Monday;
        default:
          return DayOfWeek.Sunday;
      }
    }

    public static async Task<PomodoroSummaryModel[]> CopyPomoSummaries(
      TaskModel repeatTask,
      string oldTaskId,
      bool clear = false)
    {
      List<PomodoroSummaryModel> copies = ((IEnumerable<PomodoroSummaryModel>) (await PomoSummaryDao.GetPomosByTaskId(oldTaskId)).ToArray()).Select<PomodoroSummaryModel, PomodoroSummaryModel>((Func<PomodoroSummaryModel, PomodoroSummaryModel>) (item => item.Copy(repeatTask.id, clear))).ToList<PomodoroSummaryModel>();
      await PomoSummaryDao.SavePomoSummaries(copies);
      PomodoroSummaryModel[] array = copies.ToArray();
      copies = (List<PomodoroSummaryModel>) null;
      return array;
    }

    public static int SecondToMinute(long second)
    {
      return (int) (second / 60L + (long) (second % 60L >= 30L));
    }

    public static string GetDurationString(
      long duration,
      bool isShort = false,
      bool showZero = false,
      bool showSpan = true)
    {
      string str = isShort ? "" : (showSpan ? " " : (Utils.IsCn() ? "" : " "));
      duration = (long) Utils.SecondToMinute(duration);
      long num1 = duration / 60L;
      long num2 = duration % 60L;
      return (num1 > 0L ? num1.ToString() + str + (isShort ? "h" : Utils.GetString(num1 > 1L ? "PublicHours" : "PublicHour")) : "") + (num2 > 0L || num1 == 0L & showZero ? num2.ToString() + str + (isShort ? "m" : Utils.GetString(num2 > 1L ? "PublicMinutes" : "PublicMinute")) : "");
    }

    public static string GetShortDurationString(long duration, bool showZeroMinute = true)
    {
      long num1 = duration / 3600L;
      long num2 = duration / 60L % 60L;
      string shortDurationString = num1 > 0L ? num1.ToString() + "h" : "";
      if (((num2 != 0L ? 1 : (num1 == 0L ? 1 : 0)) | (showZeroMinute ? 1 : 0)) != 0)
        shortDurationString = shortDurationString + num2.ToString() + "m";
      return shortDurationString;
    }

    public static Window GetParentWindow(DependencyObject child)
    {
      if (child == null)
        return (Window) null;
      DependencyObject parent = VisualTreeHelper.GetParent(child);
      if (parent == null)
        return (Window) null;
      return parent is Window window ? window : Utils.GetParentWindow(parent);
    }

    public static bool CheckIfAddText(TextChangedEventArgs textChange)
    {
      return textChange.Changes != null && textChange.Changes.Any<TextChange>((Func<TextChange, bool>) (change => change.AddedLength > 0 && change.RemovedLength == 0));
    }

    public static int GetCurrentUserIdInt()
    {
      int result;
      int.TryParse(LocalSettings.Settings.LoginUserId, out result);
      return result;
    }

    public static string GetCurrentUserStr() => LocalSettings.Settings.LoginUserId;

    public static T FindParent<T>(DependencyObject child) where T : class
    {
      if (child == null)
        return default (T);
      DependencyObject parent = VisualTreeHelper.GetParent(child);
      if (parent == null)
        return default (T);
      return parent is T ? parent as T : Utils.FindParent<T>(parent);
    }

    public static T FindSingleVisualChildren<T>(DependencyObject parentObj) where T : class
    {
      T singleVisualChildren = default (T);
      if (parentObj != null)
      {
        for (int childIndex = 0; childIndex < VisualTreeHelper.GetChildrenCount(parentObj); ++childIndex)
        {
          DependencyObject child = VisualTreeHelper.GetChild(parentObj, childIndex);
          if (child is T obj)
          {
            singleVisualChildren = obj;
            break;
          }
          singleVisualChildren = Utils.FindSingleVisualChildren<T>(child);
          if ((object) singleVisualChildren != null)
            break;
        }
      }
      return singleVisualChildren;
    }

    public static void CleanAfterLogout()
    {
      SearchHelper.ClearTaskSearchModels();
      WindowManager.AppLockOrExit = true;
      App.ClearReminders();
      ClosedListSyncService.ClearLoadDict();
      ProjectWidgetsHelper.CloseAll();
      TaskStickyWindow.CloseAllStickies();
      IndependentWindow.CloseAll();
      CalendarWidgetHelper.CloseWidget();
      MatrixWidgetHelper.CloseWidget();
      CacheManager.Clear();
      TaskViewModelHelper.ClearModels();
      ProjectAndTaskIdsCache.Clear();
      SearchProjectHelper.Clear();
      UserManager.Clear();
      TaskDetailWindows.CloseAll();
      AvatarHelper.Clear();
      SyncManager.Clear();
      ScheduleService.Clear();
      SystemToastUtils.ClearToastHistory();
      TickFocusManager.Clear();
      WebSocketService.DisposeWs();
      InviteHelper.Clear();
      TrashSyncService.Reset();
    }

    public static System.Windows.Point GetLocationSafely(
      System.Windows.Point location,
      double width,
      double height,
      System.Windows.Point defaultLocation,
      double rate)
    {
      Screen[] allScreens = Screen.AllScreens;
      int x1 = (int) (location.X / rate);
      int y = (int) (location.Y / rate);
      int x2 = (int) ((double) x1 + width / rate);
      foreach (Screen screen in allScreens)
      {
        if (screen.WorkingArea.Contains(new System.Drawing.Point(x1, y)) || screen.WorkingArea.Contains(new System.Drawing.Point(x2, y)))
          return location;
      }
      return defaultLocation;
    }

    public static System.Windows.Point GetPopupOffset(
      Window context,
      FrameworkElement target,
      double hoffset,
      double voffset)
    {
      if (context == null)
        context = (Window) App.Window;
      System.Windows.Point point = new System.Windows.Point(0.0, 0.0);
      try
      {
        point = target.TransformToAncestor((Visual) context).Transform(new System.Windows.Point(0.0, 0.0));
      }
      catch (Exception ex1)
      {
        try
        {
          context = Utils.GetParentWindow((DependencyObject) target);
          point = target.TransformToAncestor((Visual) context).Transform(new System.Windows.Point(0.0, 0.0));
        }
        catch (Exception ex2)
        {
          int num = (int) System.Windows.Forms.MessageBox.Show(ExceptionUtils.BuildExceptionMessage(ex1));
        }
      }
      double num1 = context.Left;
      double num2 = context.Top;
      if (context.WindowState == WindowState.Maximized)
      {
        num1 = -6.0;
        num2 = -6.0;
      }
      return new System.Windows.Point(point.X + num1 + hoffset - 22.0, point.Y + num2 + voffset);
    }

    public static bool IsWindows7() => Environment.OSVersion.Version.Major == 6;

    public static bool IsWindows11()
    {
      int major = Environment.OSVersion.Version.Major;
      int build = Environment.OSVersion.Version.Build;
      return major == 10 && build >= 22000;
    }

    public static string GetWindowsVersion()
    {
      int major = Environment.OSVersion.Version.Major;
      int minor = Environment.OSVersion.Version.Minor;
      int build = Environment.OSVersion.Version.Build;
      if (major == 6)
      {
        switch (minor)
        {
          case 1:
            return "7";
          case 2:
            return "8";
          case 3:
            return "8.1";
        }
      }
      if (major == 10 && build >= 22000)
        return "11";
      return major != 10 ? "unknown" : "10";
    }

    public static string Base64Encode(string plainText)
    {
      return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
    }

    public static string Base64Decode(string base64EncodedData)
    {
      try
      {
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
      }
      catch (Exception ex)
      {
        return string.Empty;
      }
    }

    public static void InitBaseEvents(
      Window window,
      Func<string, DependencyObject> getTemplateChild)
    {
      if (getTemplateChild("CloseButton") is UIElement uiElement)
        uiElement.MouseLeftButtonUp += (MouseButtonEventHandler) ((s, e) => window.Close());
      if (!(getTemplateChild("DragGrid") is Grid grid))
        return;
      grid.PreviewMouseDown += (MouseButtonEventHandler) ((s, e) =>
      {
        if (Mouse.LeftButton != MouseButtonState.Pressed)
          return;
        window.DragMove();
      });
    }

    public static void ResetPassword(string username)
    {
      Utils.TryProcessStartUrl(BaseUrl.GetDomainUrl() + "/sign/requestRestPassword?username=" + username);
    }

    public static List<TaskReminderModel> BuildReminders(IReadOnlyCollection<string> reminders)
    {
      List<TaskReminderModel> taskReminderModelList = new List<TaskReminderModel>();
      if (reminders != null && reminders.Count > 0)
        taskReminderModelList.AddRange(reminders.Where<string>((Func<string, bool>) (reminder => !string.IsNullOrEmpty(reminder))).Select<string, TaskReminderModel>((Func<string, TaskReminderModel>) (reminder => new TaskReminderModel()
        {
          trigger = reminder,
          id = Utils.GetGuid()
        })));
      return taskReminderModelList;
    }

    public static async Task<ReleaseNoteModel> GetReleaseNote()
    {
      try
      {
        using (WebClient webClient = new WebClient())
        {
          webClient.Encoding = Encoding.UTF8;
          string str = webClient.DownloadString(new Uri(BaseUrl.GetReleaseNoteUrl()));
          if (!string.IsNullOrEmpty(str))
            return JsonConvert.DeserializeObject<ReleaseNoteModel>(str);
        }
        return (ReleaseNoteModel) null;
      }
      catch (Exception ex)
      {
        return (ReleaseNoteModel) null;
      }
    }

    public static string EscapeReg(string original)
    {
      return original.Replace("?", ".").Replace("//", "..").Replace(":", ".").Replace("(", ".").Replace(")", ".").Replace("+", ".").Replace("$", ".").Replace("\\", ".").Replace("[", ".").Replace("]", ".").Replace("^", ".").Replace("*", ".").Replace("|", ".");
    }

    public static string HandleReg(string original)
    {
      return original.Replace("\\", "\\\\").Replace("?", "\\?").Replace("//", "\\/\\/").Replace(":", "\\:").Replace("(", "\\(").Replace(")", "\\)").Replace("+", "\\+").Replace("\\$", "\\$").Replace("[", "\\[").Replace("]", "\\]").Replace("^", "\\^").Replace("*", "\\*").Replace(".", "\\.");
    }

    public static bool ShowBatchAdd(string text)
    {
      return ((IEnumerable<string>) text.Split('\n')).Count<string>((Func<string, bool>) (line => !string.IsNullOrEmpty(line.Replace("\r", string.Empty).Replace("\n", string.Empty)))) > 1;
    }

    public static bool IsMobilePhone(string input) => new Regex("^1\\d{10}$").IsMatch(input);

    public static bool IsFakeEmail(string email)
    {
      string hostName = BaseUrl.GetHostName();
      return email != null && email.Contains(hostName);
    }

    public static string GetMaskPhone(string phone)
    {
      if (!string.IsNullOrEmpty(phone) && phone.StartsWith("86 "))
      {
        phone = phone.Substring(3);
        if (phone.Length == 11)
          return phone.Substring(0, 3) + "****" + phone.Substring(7);
      }
      return string.Empty;
    }

    public static void ReplaceFirst(ref string name, string old, string newStr)
    {
      int startIndex = name.IndexOf(old, StringComparison.Ordinal);
      name = name.Remove(startIndex, old.Length).Insert(startIndex, newStr);
    }

    public static void DebugToast(string message)
    {
    }

    public static void PlayCompletionSound()
    {
      if (PomoSoundHelper.IsPomoSoundPlaying())
        return;
      string completionSound = LocalSettings.Settings.CompletionSound;
      if (completionSound == "None")
        return;
      Utils.PlaySound(AppDomain.CurrentDomain.BaseDirectory + "completion_sound_" + completionSound.ToLower() + ".wav");
    }

    public static async void PlaySound(string path)
    {
      try
      {
        if (!System.IO.File.Exists(path))
          return;
        SoundPlayer player = new SoundPlayer()
        {
          SoundLocation = path
        };
        player.Play();
        await Task.Delay(3000);
        player.Stop();
        player.Dispose();
        player = (SoundPlayer) null;
      }
      catch (Exception ex)
      {
      }
    }

    public static long GetTotalSecond(DateTime start, DateTime end)
    {
      double totalSeconds = (end - start).TotalSeconds;
      double num1 = Math.Ceiling(totalSeconds);
      double num2 = Math.Floor(totalSeconds);
      return totalSeconds < (num1 + num2) / 2.0 ? (long) num2 : (long) num1;
    }

    public static string GetAppName() => Utils.GetString("PublicDida");

    public static async Task<bool> TryDownloadFile(string url, string localPath, bool ignoreExist = false)
    {
      if (!ignoreExist)
      {
        if (System.IO.File.Exists(localPath))
        {
          try
          {
            if (new FileInfo(localPath).Length > 0L)
              return true;
          }
          catch (Exception ex)
          {
            UtilLog.Info("TryDownloadFile FileExist:" + url + "," + ex.Message);
            return true;
          }
        }
      }
      if (!Utils.IsNetworkAvailable())
        return false;
      WebClient downloader = new WebClient();
      try
      {
        UtilLog.Info("TryDownloadFile:" + url + "," + localPath);
        await Task.Run((Func<Task>) (async () => await downloader.DownloadFileTaskAsync(url, localPath)));
        return true;
      }
      catch (Exception ex)
      {
        UtilLog.Error("TryDownloadFile::" + url + "," + localPath + ExceptionUtils.BuildExceptionMessage(ex));
        if (System.IO.File.Exists(localPath))
          System.IO.File.Delete(localPath);
        return false;
      }
      finally
      {
        downloader.Dispose();
      }
    }

    public static bool IsEqualString(string textA, string textB)
    {
      if (textA == textB)
        return true;
      return string.IsNullOrEmpty(textA) && string.IsNullOrEmpty(textB);
    }

    public static string IntToStringWithDivide(int length)
    {
      string str = length.ToString();
      List<string> values = new List<string>();
      for (; str.Length > 3; str = str.Substring(0, str.Length - 3))
        values.Insert(0, str.Substring(str.Length - 3, 3));
      if (str.Length > 0)
        values.Insert(0, str);
      return string.Join(",", (IEnumerable<string>) values);
    }

    public static T GetMousePointVisibleItem<T>(System.Windows.Input.MouseEventArgs e, FrameworkElement element) where T : class
    {
      System.Windows.Point point = e != null ? e.GetPosition((IInputElement) element) : Mouse.GetPosition((IInputElement) element);
      IInputElement inputElement = element.InputHitTest(new System.Windows.Point(point.X, point.Y));
      if (!(inputElement is DependencyObject child))
        return default (T);
      if (inputElement is Run run)
        child = run.Parent;
      return child is T ? child as T : Utils.FindParent<T>(child);
    }

    public static T GetMousePointElement<T>(System.Windows.Input.MouseEventArgs e, FrameworkElement element) where T : class
    {
      System.Windows.Point point = e != null ? e.GetPosition((IInputElement) element) : Mouse.GetPosition((IInputElement) element);
      HitTestResult hitTestResult = VisualTreeHelper.HitTest((Visual) element, new System.Windows.Point(point.X, point.Y));
      if (hitTestResult?.VisualHit is T)
        return hitTestResult.VisualHit as T;
      return hitTestResult != null ? Utils.FindParent<T>(hitTestResult.VisualHit) : default (T);
    }

    public static T GetMousePointItem<T>(System.Windows.Point point, FrameworkElement element) where T : class
    {
      HitTestResult hitTestResult = VisualTreeHelper.HitTest((Visual) element, point);
      if (hitTestResult?.VisualHit is T)
        return hitTestResult.VisualHit as T;
      return hitTestResult != null ? Utils.FindParent<T>(hitTestResult.VisualHit) : default (T);
    }

    public static string UrlEncode(string text)
    {
      Dictionary<string, string> encodeDict = new Dictionary<string, string>()
      {
        {
          "(",
          "%28"
        },
        {
          ")",
          "%29"
        },
        {
          "!",
          "%21"
        },
        {
          "*",
          "%2a"
        }
      };
      text = HttpUtility.UrlEncode(text);
      text = new Regex("[!()\\*]").Replace(text, (MatchEvaluator) (a => !encodeDict.ContainsKey(a.Value) ? a.Value : encodeDict[a.Value]));
      return text;
    }

    public static string UrlEncode0(string text)
    {
      return !string.IsNullOrEmpty(text) ? HttpUtility.UrlEncode(text) : text;
    }

    public static string RgbaToArgb(string colorStr)
    {
      return colorStr != null && colorStr.Length == 9 ? "#" + colorStr.Substring(7) + colorStr.Substring(1, 6) : colorStr;
    }

    public static double MeasureStringWidth(string text, double fontSize)
    {
      return Utils.MeasureString(text, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), fontSize).Width;
    }

    public static System.Windows.Size MeasureString(
      string text,
      Typeface typeface,
      double fontSize)
    {
      if (string.IsNullOrEmpty(text))
        return new System.Windows.Size(0.0, 0.0);
      FormattedText formattedText = new FormattedText(text, CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, typeface, fontSize, (System.Windows.Media.Brush) System.Windows.Media.Brushes.Black, new NumberSubstitution(), TextFormattingMode.Display);
      return new System.Windows.Size(formattedText.Width, formattedText.Height);
    }

    public static BitmapImage LoadBitmap(
      string imagePath,
      int pixelWidth = -1,
      int pixelHeight = -1,
      BitmapCacheOption cache = BitmapCacheOption.OnLoad)
    {
      try
      {
        if (System.IO.File.Exists(imagePath))
        {
          BitmapImage bitmapImage = new BitmapImage();
          bitmapImage.BeginInit();
          bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
          bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
          bitmapImage.UriSource = new Uri(imagePath);
          if (pixelWidth > 0 && pixelHeight > 0)
          {
            bitmapImage.DecodePixelWidth = pixelWidth;
            bitmapImage.DecodePixelHeight = pixelHeight;
          }
          bitmapImage.EndInit();
          bitmapImage.Freeze();
          return bitmapImage;
        }
      }
      catch (ArgumentException ex)
      {
      }
      catch (Exception ex)
      {
      }
      return (BitmapImage) null;
    }

    public static Rect GetRectByString(string location)
    {
      try
      {
        return Rect.Parse(location);
      }
      catch (Exception ex)
      {
      }
      return new Rect(0.0, 0.0, 0.0, 0.0);
    }

    public static string GetNthString(int num)
    {
      num = num < 0 ? -num : num;
      string format = Utils.GetString("nth");
      switch (num % 10)
      {
        case 1:
          format = Utils.GetString("1st");
          break;
        case 2:
          format = Utils.GetString("2nd");
          break;
        case 3:
          format = Utils.GetString("3rd");
          break;
      }
      return string.Format(format, (object) num);
    }

    public static int GetDotNetReleaseKey()
    {
      try
      {
        using (RegistryKey registryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
        {
          if (registryKey != null)
          {
            if (registryKey.GetValue("Release") is int dotNetReleaseKey)
              return dotNetReleaseKey;
          }
        }
      }
      catch
      {
      }
      return 0;
    }

    public static string GetDotNetVersion()
    {
      int dotNetReleaseKey = Utils.GetDotNetReleaseKey();
      if (dotNetReleaseKey >= 528040)
        return "4.8 or later";
      if (dotNetReleaseKey >= 461808)
        return "4.7.2";
      if (dotNetReleaseKey >= 461308)
        return "4.7.1";
      if (dotNetReleaseKey >= 460798)
        return "4.7";
      if (dotNetReleaseKey >= 394802)
        return "4.6.2";
      if (dotNetReleaseKey >= 394254)
        return "4.6.1";
      if (dotNetReleaseKey >= 393295)
        return "4.6";
      if (dotNetReleaseKey >= 379893)
        return "4.5.2";
      if (dotNetReleaseKey >= 378675)
        return "4.5.1";
      return dotNetReleaseKey >= 378389 ? "4.5" : "No 4.5 or later version detected";
    }

    public static bool IsBetween(double value, double a, double b)
    {
      return a >= b ? b <= value && value <= a : a <= value && value <= b;
    }

    public static bool IsContain(double a1, double b1, double a2, double b2)
    {
      if (a2 <= a1 && a1 <= b2 || a2 <= b1 && b1 <= b2)
        return true;
      return a2 >= a1 && b1 >= b2;
    }

    public static System.Windows.Media.FontFamily GetDefaultFontFamily()
    {
      return LocalSettings.Settings.FontFamily ?? new System.Windows.Media.FontFamily("Microsoft YaHei UI");
    }

    public static string GetLanguage(bool useUnderLine = true)
    {
      string language = App.Ci.ToString();
      if (string.IsNullOrEmpty(language))
        language = Utils.IsDida() ? "zh-CN" : "en-US";
      if (useUnderLine)
        language = language.Replace("-", "_");
      return language;
    }

    public static void LogActionTimes(
      Action action,
      int maxMilliseconds,
      string startMessage,
      string warnMessage)
    {
      if (!string.IsNullOrEmpty(startMessage))
        UtilLog.Info(startMessage);
      Stopwatch stopwatch = new Stopwatch();
      stopwatch.Start();
      action();
      stopwatch.Stop();
      if (maxMilliseconds >= 0 && stopwatch.ElapsedMilliseconds <= (long) maxMilliseconds)
        return;
      UtilLog.Info(warnMessage + " cost : " + stopwatch.ElapsedMilliseconds.ToString());
    }

    public static async Task LogTaskTimes(
      Task task,
      int maxMilliseconds = 1000,
      string startMessage = null,
      string warnMessage = null)
    {
      if (!string.IsNullOrEmpty(startMessage))
        UtilLog.Info(startMessage);
      Stopwatch sw = new Stopwatch();
      sw.Start();
      await task;
      sw.Stop();
      if (maxMilliseconds >= 0 && sw.ElapsedMilliseconds <= (long) maxMilliseconds)
      {
        sw = (Stopwatch) null;
      }
      else
      {
        UtilLog.Info((warnMessage ?? task.ToString()) + " cost : " + sw.ElapsedMilliseconds.ToString());
        sw = (Stopwatch) null;
      }
    }

    public static void Focus(UIElement element)
    {
      element.Dispatcher.BeginInvoke(DispatcherPriority.Input, (Delegate) (() => element.Focus()));
    }

    public static string GetTimeTextFromDuration(int duration, bool showHour)
    {
      int num1 = duration % 60;
      int num2 = showHour ? duration / 60 % 60 : duration / 60;
      int num3 = showHour ? duration / 3600 : 0;
      return (num3 > 0 ? (num3 >= 10 ? "" : "0") + num3.ToString() + ":" : "") + (num2 >= 10 ? "" : "0") + num2.ToString() + ":" + (num1 >= 10 ? "" : "0") + num1.ToString();
    }

    public static string MinutesToHourMinuteText(int minutes, bool showZero = false)
    {
      if (showZero && minutes == 0)
        return " 0 " + Utils.GetString("PublicMinute");
      int num1 = minutes % 60;
      int num2 = minutes / 60;
      return (num2 > 0 ? string.Format("{0} ", (object) num2) + Utils.GetString(num2 > 1 ? "PublicHours" : "PublicHour") : "") + (minutes > 0 ? string.Format(" {0} ", (object) num1) + Utils.GetString(num1 > 1 ? "PublicMinutes" : "PublicMinute") : "");
    }

    public static void SetWindowChrome(Window window, Thickness thickness)
    {
      if (ABTestManager.IsSystemWC())
        WindowUtils.SetWindowChrome(window, thickness);
      else
        WindowUtils.SetCustomWindowChrome(window, thickness);
    }

    public static string GetSystemLanguage()
    {
      List<string> stringList = new List<string>();
      stringList.Add("zh-CN");
      stringList.Add("en-US");
      stringList.Add("ja-JP");
      stringList.Add("ko-KR");
      stringList.Add("fr-FR");
      stringList.Add("ru-RU");
      stringList.Add("pt-BR");
      string systemLanguage = CultureInfo.CurrentCulture.ToString();
      if (stringList.Contains(systemLanguage))
        return systemLanguage;
      return !Utils.IsDida() ? "en-US" : "zh-CN";
    }

    public class TupleList<T1, T2> : List<Tuple<T1, T2>>
    {
      public void Add(T1 item, T2 item2) => this.Add(new Tuple<T1, T2>(item, item2));
    }

    public class TupleList<T1, T2, T3> : List<Tuple<T1, T2, T3>>
    {
      public void Add(T1 item, T2 item2, T3 item3)
      {
        this.Add(new Tuple<T1, T2, T3>(item, item2, item3));
      }
    }

    public class TupleList<T1, T2, T3, T4> : List<Tuple<T1, T2, T3, T4>>
    {
      public void Add(T1 item, T2 item2, T3 item3, T4 item4)
      {
        this.Add(new Tuple<T1, T2, T3, T4>(item, item2, item3, item4));
      }
    }
  }
}
