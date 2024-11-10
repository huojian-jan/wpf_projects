// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.ABTestManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.ReminderTime;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service
{
  public static class ABTestManager
  {
    private const string CleanTask = "202402_clean_task";
    private const string MatrixTab = "202401_matrix_tab";
    public const string NewUser = "newuser3_202403";
    private const string WindowChrome = "202403_window_chrome";
    private const string RemindCalculate = "2024_pc_remind";
    private const string OGroup = "_o";
    private const string AGroup = "_a";
    private const string BGroup = "_b";
    private const string NGroup = "_n";
    private static readonly List<string> AbTestList = new List<string>()
    {
      "202402_clean_task",
      "202403_window_chrome",
      "2024_pc_remind"
    };

    public static async Task AssignGroupOnLaunch()
    {
      await ABTestManager.GetAssignedTestGroup();
      await ABTestManager.ApplyABTestGroup();
    }

    private static async Task GetAssignedTestGroup()
    {
      if (!Utils.IsNetworkAvailable())
      {
        UtilLog.Info("GetAssignedTestGroup no network");
      }
      else
      {
        TestCode2PlanCodeInfo abTestGroupResult = await Communicator.GetABTestGroupResult(new BatchTestInfo()
        {
          appCode = "608903cbe4b00f4b6d989f98",
          clientCode = "60890f038f0839d464599a56",
          testCodes = ABTestManager.AbTestList,
          deviceId = App.UId,
          userId = LocalSettings.Settings.Common.LoginUserId
        });
        if (abTestGroupResult == null || abTestGroupResult.testCode2PlanCode.Count <= 0)
          return;
        Dictionary<string, string> abTestData = LocalSettings.Settings.ExtraSettings.ABTestData;
        foreach (string key in abTestGroupResult.testCode2PlanCode.Keys)
        {
          UtilLog.Info("ABTestManager.GetAssignedTestGroup() code: " + key + ", result: " + abTestGroupResult.testCode2PlanCode[key]);
          abTestData[key] = abTestGroupResult.testCode2PlanCode[key];
        }
        LocalSettings.Settings.ExtraSettings.ABTestData = abTestData;
        LocalSettings.Settings.Save();
      }
    }

    private static async Task ApplyABTestGroup()
    {
      if (!Utils.IsNetworkAvailable())
      {
        UtilLog.Info("AssignGroupOnLaunch no network");
      }
      else
      {
        Dictionary<string, string> code2Result = LocalSettings.Settings.ExtraSettings.ABTestData;
        foreach (string code in ABTestManager.AbTestList.Where<string>((Func<string, bool>) (code => !code2Result.ContainsKey(code))))
        {
          TabPlanData tabPlanData = await Communicator.ApplyABTestGroupResult(new TestInfo()
          {
            appCode = "608903cbe4b00f4b6d989f98",
            clientCode = "60890f038f0839d464599a56",
            deviceId = App.UId,
            testCode = code,
            userId = LocalSettings.Settings.Common.LoginUserId
          });
          UtilLog.Info("ABTestManager.AssignGroupOnLaunch() code: " + code + ", result: " + tabPlanData?.ToString());
          code2Result[code] = tabPlanData.data.planCode;
        }
        LocalSettings.Settings.ExtraSettings.ABTestData = code2Result;
        LocalSettings.Settings.Save();
      }
    }

    public static async Task<string> GetABTestGroupResult(string code)
    {
      if (!Utils.IsNetworkAvailable())
      {
        UtilLog.Info("AssignGroupOnLaunch no network");
        return "_n";
      }
      TabPlanData tabPlanData = await Communicator.ApplyABTestGroupResult(new TestInfo()
      {
        appCode = "608903cbe4b00f4b6d989f98",
        clientCode = "60890f038f0839d464599a56",
        deviceId = App.UId,
        testCode = code,
        userId = LocalSettings.Settings.Common.LoginUserId
      });
      UtilLog.Info("ABTestManager.AssignGroupOnLaunch() code: " + code + ", result: " + tabPlanData?.ToString());
      string lower = tabPlanData?.data?.planCode?.ToLower();
      return !string.Equals(lower, code + "_a", StringComparison.CurrentCultureIgnoreCase) ? (!string.Equals(lower, code + "_b", StringComparison.CurrentCultureIgnoreCase) ? (!string.Equals(lower, code + "_o", StringComparison.CurrentCultureIgnoreCase) ? "_n" : "_o") : "_b") : "_a";
    }

    private static string TryGetABTestGroup(string code)
    {
      string str;
      string abTestGroup = LocalSettings.Settings.ExtraSettings.ABTestData.TryGetValue(code, out str) ? str : code + "_o";
      UtilLog.Info("ABTestManager.TryGetABTestGroup() code: " + code + ", result: " + abTestGroup);
      return abTestGroup;
    }

    public static bool IsTabWithMatrix()
    {
      return string.Equals(ABTestManager.TryGetABTestGroup("202401_matrix_tab"), "202401_matrix_tab_a", StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool IsCleanTask()
    {
      return string.Equals(ABTestManager.TryGetABTestGroup("202402_clean_task"), "202402_clean_task_a", StringComparison.CurrentCultureIgnoreCase);
    }

    public static bool IsSystemWC()
    {
      return string.Equals(ABTestManager.TryGetABTestGroup("202403_window_chrome"), "202403_window_chrome_a", StringComparison.CurrentCultureIgnoreCase);
    }

    public static void HandleBestTest(List<Best> abBest)
    {
      Dictionary<string, string> abTestData = LocalSettings.Settings.ExtraSettings.ABTestData;
      foreach (Best best in abBest)
        abTestData[best.testCode] = best.planCode;
      LocalSettings.Settings.ExtraSettings.ABTestData = abTestData;
      LocalSettings.Settings.Save();
    }

    public static bool IsAGroup(string abTest) => abTest == "_a";

    public static bool IsBGroup(string abTest) => abTest == "_b";

    public static bool IsNewRemindCalculate()
    {
      return ReminderBase.UseNewReminderCalculate.HasValue ? ReminderBase.UseNewReminderCalculate.Value : ABTestManager.TryGetABTestGroup("2024_pc_remind") == "2024_pc_remind_a";
    }
  }
}
