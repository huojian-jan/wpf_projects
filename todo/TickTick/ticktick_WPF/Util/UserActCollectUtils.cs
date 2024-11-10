// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.UserActCollectUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class UserActCollectUtils
  {
    private static readonly DelayActionHandler _repostDeviceHandler = new DelayActionHandler(180000);
    private static readonly Timer _postEventTimer = new Timer(10000.0);
    private static string _fileUrl = AppPaths.DataDir + "ticktick_events_tmp.txt";
    private static readonly BlockingList<string> StoredJson = new BlockingList<string>();
    private static object _o = new object();
    private static bool _init;
    private static DateTime _nextPostTime;
    private static int CustomExceptionCount = 100;

    public static void StartPost()
    {
      if (UserActCollectUtils._init)
        return;
      UserActCollectUtils._repostDeviceHandler.SetAction(new EventHandler(UserActCollectUtils.TryPostDevice));
      UserActCollectUtils._postEventTimer.Elapsed += new ElapsedEventHandler(UserActCollectUtils.TryPostEvent);
      UserActCollectUtils._postEventTimer.Start();
      UserActCollectUtils.PostEvents().ContinueWith(new Action<Task>(UtilRun.LogTask));
      UserActCollectUtils._init = true;
    }

    private static void TryPostDevice(object sender, EventArgs e)
    {
      UserActCollectUtils.PostDevice().ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    public static async Task OnDeviceDataChanged()
    {
      try
      {
        await UserActCollectUtils.PostDevice();
      }
      catch (HttpRequestException ex)
      {
      }
    }

    private static async Task PostDevice()
    {
      await UserActCollectUtils.UpdateDevice();
      bool flag = true;
      if (!string.IsNullOrEmpty(LocalSettings.Settings.UserDeviceString))
        flag = await Communicator.PostUserDevice(LocalSettings.Settings.UserDeviceString);
      if (flag)
        UserActCollectUtils._repostDeviceHandler.CancelAction();
      else
        UserActCollectUtils._repostDeviceHandler.TryDoAction();
    }

    private static async Task UpdateDevice()
    {
      string windowsVersion = "Windows_" + Utils.GetWindowsVersion();
      string machineName = Environment.MachineName;
      UserDeviceInfo userDeviceInfo1 = new UserDeviceInfo();
      userDeviceInfo1.did = App.UId;
      userDeviceInfo1.ver = Utils.GetVersion();
      userDeviceInfo1.channel = "website";
      userDeviceInfo1.os = windowsVersion;
      userDeviceInfo1.name = machineName;
      userDeviceInfo1.ctime = DateTime.Now;
      UserDeviceInfo userDeviceInfo2 = userDeviceInfo1;
      userDeviceInfo2.userCode = await UserManager.GetUserCode();
      userDeviceInfo1.hl = LocalSettings.Settings.UserChooseLanguage;
      userDeviceInfo1.tz = TimeZoneData.LocalTimeZoneModel.TimeZoneName;
      userDeviceInfo1.enable_tz = TimeZoneData.EnableTz;
      userDeviceInfo1.locale = CultureInfo.InstalledUICulture.Name.Replace("-", "_");
      UserDeviceInfo deviceModel = userDeviceInfo1;
      userDeviceInfo2 = (UserDeviceInfo) null;
      userDeviceInfo1 = (UserDeviceInfo) null;
      deviceModel.device = windowsVersion + "x64";
      UserActionModel userActionModel1 = new UserActionModel();
      userActionModel1.kind = "app";
      userActionModel1.time = DateTime.Now;
      UserActionModel userActionModel2 = userActionModel1;
      userActionModel2.userCode = await UserManager.GetUserCode();
      userActionModel1.type = "device";
      userActionModel1.ctype = "update";
      userActionModel1.data = (object) deviceModel;
      userActionModel1.did = App.UId;
      userActionModel1.id = Utils.GetGuid();
      UserActionModel userActionModel = userActionModel1;
      userActionModel2 = (UserActionModel) null;
      userActionModel1 = (UserActionModel) null;
      string str = JsonConvert.SerializeObject((object) userActionModel);
      LocalSettings.Settings.UserDeviceString = str;
      windowsVersion = (string) null;
      deviceModel = (UserDeviceInfo) null;
    }

    public static async void AddSortOptionEvent(
      string type,
      string ctype,
      string groupBy,
      string orderBy)
    {
      try
      {
        await UserActCollectUtils.AddEvent("biz", type, ctype, (object) new JObject()
        {
          ["label"] = (JToken) (groupBy + "_" + orderBy),
          ["pm_group_by"] = (JToken) groupBy,
          ["pm_order_by"] = (JToken) orderBy
        });
      }
      catch (Exception ex)
      {
      }
    }

    public static async void AddTaskLabelEvent(string type, string ctype, string label, long time)
    {
      try
      {
        await UserActCollectUtils.AddEvent("biz", type, ctype, (object) new JObject()
        {
          [nameof (label)] = (JToken) label,
          ["loading_time"] = (JToken) time
        });
      }
      catch (Exception ex)
      {
      }
    }

    public static async void AddClickEvent(string type, string ctype, string data)
    {
      try
      {
        await UserActCollectUtils.AddEvent("biz", type, ctype, (object) new ActionCollectModel()
        {
          label = data
        });
        UtilLog.Info("UserActCollect.AddClickEvent: " + type + " " + ctype + " " + data);
      }
      catch (Exception ex)
      {
      }
    }

    private static async Task AddEvent(string kind, string type, string ctype, object data)
    {
      UserActionModel userActionModel1 = new UserActionModel();
      userActionModel1.kind = kind;
      userActionModel1.time = DateTime.Now;
      userActionModel1.did = App.UId;
      UserActionModel userActionModel2 = userActionModel1;
      userActionModel2.userCode = await UserManager.GetUserCode();
      userActionModel1.type = type;
      userActionModel1.ctype = ctype;
      userActionModel1.data = data;
      userActionModel1.id = Utils.GetGuid();
      UserActionModel userActionModel = userActionModel1;
      userActionModel2 = (UserActionModel) null;
      userActionModel1 = (UserActionModel) null;
      UserActCollectUtils.AddJson(JsonConvert.SerializeObject((object) userActionModel));
    }

    private static void AddJson(string json)
    {
      if (string.IsNullOrEmpty(json))
        return;
      if (UserActCollectUtils.StoredJson.Count < 100)
      {
        UserActCollectUtils.StoredJson.Add(json);
      }
      else
      {
        if (UserActCollectUtils.StoredJson.Count != 100)
          return;
        UserActCollectUtils.StoredJson.Add(json);
        UtilLog.Warn("UserActCollect.AddJson Warn: over limit");
      }
    }

    private static void TryPostEvent(object sender, ElapsedEventArgs e)
    {
      UserActCollectUtils.WriteJsonsToFile();
      UserActCollectUtils.PostEvents().ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    private static void WriteJsonsToFile()
    {
      lock (UserActCollectUtils._o)
      {
        List<string> list = UserActCollectUtils.StoredJson.ToList();
        if (list.Count == 0)
          return;
        UserActCollectUtils.StoredJson.Clear();
        try
        {
          using (FileStream fileStream = new FileStream(UserActCollectUtils._fileUrl, FileMode.Append))
          {
            foreach (string str in list)
            {
              byte[] bytes = Encoding.UTF8.GetBytes(str + Environment.NewLine);
              fileStream.Write(bytes, 0, bytes.Length);
            }
          }
        }
        catch (Exception ex)
        {
          UtilLog.Warn(ex.Message);
        }
      }
    }

    private static async Task PostEvents()
    {
      if (DateTime.Now < UserActCollectUtils._nextPostTime)
        return;
      DateTime now = DateTime.Now;
      UserActCollectUtils._nextPostTime = now.AddHours(1.0);
      UtilLog.Info("UserActCollect.StartPostEvents");
      UserActCollectUtils.DoCompress();
      if (Utils.IsNetworkAvailable())
      {
        List<string> fileList = ((IEnumerable<FileInfo>) new DirectoryInfo(AppPaths.DataDir).GetFiles("ticktick_events_*.zip")).Select<FileInfo, string>((Func<FileInfo, string>) (f => f.FullName)).OrderBy<string, string>((Func<string, string>) (f => f)).ToList<string>();
        UtilLog.Info("UserActCollect.StartPostEvents FileCount: " + fileList.Count.ToString());
        if (fileList.Any<string>())
        {
          List<string> postFiles = new List<string>();
          long num = 0;
          bool postFailed = false;
          foreach (string fileName in fileList)
          {
            num += new FileInfo(fileName).Length;
            postFiles.Add(fileName);
            if (num > 200000L)
            {
              if (await UserActCollectUtils.DoPost(postFiles))
              {
                postFiles.Clear();
                num = 0L;
              }
              else
              {
                UserActCollectUtils.SendException(new Exception("PostEvent Failed " + fileList.Count.ToString()), ExceptionType.OtherThread);
                postFailed = true;
                break;
              }
            }
          }
          if (!postFailed && postFiles.Any<string>())
            postFailed = !await UserActCollectUtils.DoPost(postFiles);
          if (postFailed)
          {
            UserActCollectUtils._nextPostTime = DateTime.Now.AddMinutes(5.0);
            UtilLog.Info("UserActCollect.StartPostEvents Failed");
          }
          postFiles = (List<string>) null;
        }
        fileList = (List<string>) null;
      }
      else
      {
        now = DateTime.Now;
        UserActCollectUtils._nextPostTime = now.AddMinutes(5.0);
      }
    }

    private static async Task<bool> DoPost(List<string> files)
    {
      if (files == null || files.Count == 0)
        return true;
      if (!await Communicator.PostUserEvents(files))
        return false;
      foreach (string file in files)
      {
        if (File.Exists(file))
          File.Delete(file);
      }
      return true;
    }

    private static void DoCompress()
    {
      if (!File.Exists(UserActCollectUtils._fileUrl) || new FileInfo(UserActCollectUtils._fileUrl).Length <= 10L)
        return;
      UtilLog.Info("UserActCollect.DoCompress fileLength: " + new FileInfo(UserActCollectUtils._fileUrl).Length.ToString());
      string zipUrl = AppPaths.DataDir + "ticktick_events_" + DateTime.Now.ToString("yyyyMMddHHmm") + ".zip";
      if (!UserActCollectUtils.CompressFile(UserActCollectUtils._fileUrl, zipUrl))
        return;
      new FileStream(UserActCollectUtils._fileUrl, FileMode.Truncate, FileAccess.ReadWrite).Close();
    }

    private static bool CompressFile(string fileUrl, string zipUrl)
    {
      try
      {
        using (ZipArchive destination = ZipFile.Open(zipUrl, ZipArchiveMode.Create))
        {
          string fileName = Path.GetFileName(fileUrl);
          destination.CreateEntryFromFile(fileUrl, fileName);
          return true;
        }
      }
      catch (Exception ex)
      {
        UtilLog.Warn(ex.Message);
        return false;
      }
    }

    public static async Task SendCustomException(string custom)
    {
      if (UserActCollectUtils.CustomExceptionCount-- > 0)
        await Communicator.PostException(new Exception(custom), ExceptionType.Custom);
      UtilLog.Info(custom);
    }

    public static async Task SendException(Exception e, ExceptionType type)
    {
      if (e == null)
        return;
      await Communicator.PostException(e, type);
    }

    public static void OnAppExit()
    {
      UserActCollectUtils._postEventTimer.Stop();
      if (UserActCollectUtils.StoredJson.Count <= 0)
        return;
      UserActCollectUtils.WriteJsonsToFile();
    }

    public static void AddShortCutEvent(string ctype, string label)
    {
      UserActCollectUtils.AddClickEvent("shortcut", ctype, label);
    }
  }
}
