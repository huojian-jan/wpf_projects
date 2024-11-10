// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ProChecker
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Views;
using TickTickDao;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ProChecker
  {
    private static bool _dialogShowing;
    private static UpgradeProDialog _calendarProDialog;

    public static async Task ShowUpgradeDialog(ProType type, Window owner = null, string teamId = null)
    {
      if (ProChecker._dialogShowing)
        return;
      ProChecker._dialogShowing = true;
      string imageName = ProChecker.SetImage(type);
      UpgradeProDialog upgradeProDialog1 = new UpgradeProDialog(type, await ProChecker.TryDownloadBackground(imageName), imageName, teamId);
      upgradeProDialog1.Topmost = true;
      UpgradeProDialog upgradeProDialog2 = upgradeProDialog1;
      try
      {
        upgradeProDialog2.Owner = owner;
      }
      catch (Exception ex)
      {
        upgradeProDialog2.Owner = (Window) null;
      }
      if (upgradeProDialog2.Owner == null)
        upgradeProDialog2.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      if (type == ProType.TimeLine)
      {
        LocalSettings.Settings.ExtraSettings.TLUsed = true;
        LocalSettings.Settings.Save();
      }
      if (type == ProType.CalendarView)
      {
        upgradeProDialog2.Show();
        ProChecker._calendarProDialog = upgradeProDialog2;
        owner?.Activate();
      }
      else
        upgradeProDialog2.ShowDialog();
      ProChecker._dialogShowing = false;
      imageName = (string) null;
    }

    public static void TryCloseCalendarProDialog()
    {
      try
      {
        ProChecker._calendarProDialog?.Close();
        ProChecker._calendarProDialog = (UpgradeProDialog) null;
      }
      catch (Exception ex)
      {
      }
    }

    private static string SetImage(ProType type)
    {
      string str = type.ToString();
      bool flag = false;
      switch (type)
      {
        case ProType.MoreListsUnlimited:
          str = ProType.MoreLists.ToString();
          break;
        case ProType.TimeLine:
          flag = true;
          break;
        case ProType.StickyColor:
          str = ProType.PremiumThemes.ToString();
          break;
        case ProType.EmailReminder:
          str += "_dida";
          break;
        case ProType.CalendarWidget:
          str = ProType.CalendarView.ToString();
          break;
        case ProType.SummaryStyle:
          str = "SummaryContent";
          break;
        case ProType.SummaryTemplate:
          str = ProType.SummaryStyle.ToString();
          break;
      }
      return str + (Utils.IsCn() ? "_cn" : "_en") + (flag ? ".gif" : ".png");
    }

    private static async Task<bool> TryDownloadBackground(string name)
    {
      return await Task.Run<bool>((Func<Task<bool>>) (async () => await ProChecker.TryDownloadBackgroundAsync(name)));
    }

    private static async Task<bool> TryDownloadBackgroundAsync(string name)
    {
      if (!Directory.Exists(AppPaths.UpgradeProDir))
        Directory.CreateDirectory(AppPaths.UpgradeProDir);
      string localPath = AppPaths.UpgradeProDir + name;
      if (System.IO.File.Exists(localPath))
      {
        FileStream fileStream = System.IO.File.OpenRead(localPath);
        if (fileStream.Length == 0L)
        {
          fileStream.Close();
        }
        else
        {
          fileStream.Close();
          return true;
        }
      }
      string address = "https://" + BaseUrl.GetPullDomain() + "/windows/static/" + name;
      if (!Utils.IsNetworkAvailable())
        return false;
      using (WebClient webClient = new WebClient())
      {
        try
        {
          await webClient.DownloadFileTaskAsync(address, localPath);
          return true;
        }
        catch (Exception ex)
        {
          if (System.IO.File.Exists(localPath))
            System.IO.File.Delete(localPath);
          return false;
        }
      }
    }

    public static bool CheckPro(ProType type, Window owner = null)
    {
      if (UserDao.IsUserValid())
        return true;
      ProChecker.ShowUpgradeDialog(type, owner);
      return false;
    }

    private static void ShowOverLimitDialog() => ProChecker.ShowUpgradeDialog(ProType.MoreTasks);

    private static void ShowProLimit(long limit)
    {
      new CustomerDialog(Utils.GetString("LimitTips"), string.Format(Utils.GetString("TasksLimit"), (object) limit), MessageBoxButton.OK, Utils.GetToastWindow() as Window).ShowDialog();
    }

    public static async Task<bool> CheckTaskLimit(string projectId, int addCount = 1)
    {
      long limit = Utils.GetUserLimit(Constants.LimitKind.ProjectTaskNumber);
      int num = (await TaskDao.GetUncompletedTasksInProject(projectId)).Count + addCount;
      if (projectId == LocalSettings.Settings.InServerBoxId)
      {
        if (!UserDao.IsPro() && num > 999)
        {
          ProChecker.ShowOverLimitDialog();
          return true;
        }
      }
      else if ((long) num > limit)
      {
        if (UserDao.IsPro())
        {
          ProChecker.ShowProLimit(limit);
          return true;
        }
        ProChecker.ShowOverLimitDialog();
        return true;
      }
      return false;
    }

    public static async Task<bool> CheckTimerLimit(Window owner)
    {
      if ((long) await TimerDao.GetActiveTimerCountAsync() < Utils.GetUserLimit(Constants.LimitKind.TimerNumber))
        return true;
      if (UserDao.IsPro())
      {
        CustomerDialog customerDialog = new CustomerDialog("", Utils.GetString("TimerLimitMessage"), MessageBoxButton.OK);
        customerDialog.Owner = owner;
        customerDialog.ShowDialog();
      }
      else
        ProChecker.CheckPro(ProType.MoreTimers, owner);
      return false;
    }
  }
}
