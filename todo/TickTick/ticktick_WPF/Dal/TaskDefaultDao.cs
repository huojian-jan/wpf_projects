// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TaskDefaultDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TaskDefaultDao
  {
    private static TaskDefaultModel _cacheModel;

    public static event EventHandler<TaskDefaultModel> TaskDefaultChanged;

    public static async Task InitCache()
    {
      TaskDefaultDao._cacheModel = await TaskDefaultDao.GetTaskDefaultWithDefault();
    }

    public static TaskDefaultModel GetDefaultSafely()
    {
      return TaskDefaultDao._cacheModel ?? TaskDefaultModel.BuildDefault();
    }

    public static async Task<TaskDefaultModel> GetTaskDefault()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      TaskDefaultModel taskDefault = await App.Connection.Table<TaskDefaultModel>().Where((Expression<Func<TaskDefaultModel, bool>>) (m => m.UserId == userId)).FirstOrDefaultAsync();
      if (taskDefault != null && !string.IsNullOrEmpty(taskDefault.AllDayReminders))
      {
        string[] source = taskDefault.AllDayReminders.Split(',');
        if (((IEnumerable<string>) source).Any<string>())
          taskDefault.AllDayReminders = string.Join(",", (IEnumerable<string>) ((IEnumerable<string>) source).Select<string, string>(new Func<string, string>(TriggerUtils.ConvertLegacyTrigger)).ToList<string>());
      }
      TaskDefaultDao._cacheModel = taskDefault;
      return taskDefault;
    }

    public static async Task<TaskDefaultModel> GetTaskDefaultWithDefault()
    {
      return await TaskDefaultDao.GetTaskDefault() ?? TaskDefaultModel.BuildDefault();
    }

    public static async Task SaveTaskDefault(TaskDefaultModel model)
    {
      if (string.IsNullOrEmpty(model.UserId))
        model.UserId = Utils.GetCurrentUserIdInt().ToString();
      TaskDefaultModel taskDefault = await TaskDefaultDao.GetTaskDefault();
      if (taskDefault == null)
      {
        TaskDefaultDao._cacheModel = model;
        int num = await App.Connection.InsertAsync((object) model);
      }
      else
      {
        model._Id = taskDefault._Id;
        TaskDefaultDao._cacheModel = model;
        int num = await App.Connection.UpdateAsync((object) model);
      }
    }

    public static async Task UpdateDefaultTagsOnTagChanged(string origin, string newTag)
    {
      TaskDefaultModel taskDefault = await TaskDefaultDao.GetTaskDefault();
      List<string> tags = taskDefault?.Tags;
      if (tags == null)
        ;
      else if (!tags.Any<string>((Func<string, bool>) (t => string.Equals(t, origin, StringComparison.CurrentCultureIgnoreCase))))
        ;
      else
      {
        if (string.IsNullOrEmpty(newTag))
        {
          tags.Remove(origin.ToLower());
          taskDefault.Tags = tags;
        }
        else
        {
          int index = tags.IndexOf(origin.ToLower());
          if (index >= 0)
          {
            tags[index] = newTag.ToLower();
            taskDefault.Tags = tags;
          }
        }
        await TaskDefaultDao.SaveTaskDefault(taskDefault);
        DataChangedNotifier.NotifyTaskDefaultChanged();
        await SettingsHelper.PushLocalSettings();
      }
    }

    public static void ClearCache() => TaskDefaultDao._cacheModel = (TaskDefaultModel) null;
  }
}
