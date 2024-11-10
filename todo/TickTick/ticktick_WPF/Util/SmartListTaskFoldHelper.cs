// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.SmartListTaskFoldHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using ticktick_WPF.Resource;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public class SmartListTaskFoldHelper
  {
    private static Dictionary<string, HashSet<string>> _statusDict = new Dictionary<string, HashSet<string>>();

    public static HashSet<string> GetOpenedTaskIds(string catId)
    {
      return string.IsNullOrEmpty(catId) || !SmartListTaskFoldHelper._statusDict.ContainsKey(catId) ? new HashSet<string>() : SmartListTaskFoldHelper._statusDict[catId];
    }

    public static void UpdateStatus(string taskId, string catId, bool isFolded)
    {
      if (isFolded)
        SmartListTaskFoldHelper.RemoveStatus(taskId, catId);
      else
        SmartListTaskFoldHelper.AddStatus(taskId, catId);
    }

    private static void RemoveStatus(string taskId, string catId)
    {
      if (SmartListTaskFoldHelper._statusDict.ContainsKey(catId))
        SmartListTaskFoldHelper._statusDict[catId]?.Remove(taskId);
      SmartListTaskFoldHelper.Save();
    }

    private static void AddStatus(string taskId, string catId)
    {
      if (SmartListTaskFoldHelper._statusDict.ContainsKey(catId))
      {
        HashSet<string> stringSet = SmartListTaskFoldHelper._statusDict[catId];
        if (stringSet == null)
          SmartListTaskFoldHelper._statusDict[catId] = new HashSet<string>()
          {
            taskId
          };
        else
          stringSet.Add(taskId);
      }
      else
        SmartListTaskFoldHelper._statusDict[catId] = new HashSet<string>()
        {
          taskId
        };
      SmartListTaskFoldHelper.Save();
    }

    public static void ResetStatus(string catId, List<string> ids)
    {
      // ISSUE: explicit non-virtual call
      SmartListTaskFoldHelper._statusDict[catId] = ids == null || __nonvirtual (ids.Count) <= 0 ? new HashSet<string>() : new HashSet<string>((IEnumerable<string>) ids);
      SmartListTaskFoldHelper.Save();
    }

    public static void Save()
    {
      try
      {
        string str = JsonConvert.SerializeObject((object) SmartListTaskFoldHelper._statusDict);
        LocalSettings.Settings.ExtraSettings.TaskFoldStatus = str;
        LocalSettings.Settings.SetChanged();
        LocalSettings.Settings.Save();
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
    }

    public static void Load()
    {
      string taskFoldStatus = LocalSettings.Settings.ExtraSettings.TaskFoldStatus;
      try
      {
        if (taskFoldStatus != null && !string.IsNullOrEmpty(taskFoldStatus) && !string.Equals(taskFoldStatus.ToLower(), "null"))
          SmartListTaskFoldHelper._statusDict = JsonConvert.DeserializeObject<Dictionary<string, HashSet<string>>>(taskFoldStatus);
        else
          SmartListTaskFoldHelper._statusDict = new Dictionary<string, HashSet<string>>();
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
        SmartListTaskFoldHelper._statusDict = new Dictionary<string, HashSet<string>>();
      }
    }
  }
}
