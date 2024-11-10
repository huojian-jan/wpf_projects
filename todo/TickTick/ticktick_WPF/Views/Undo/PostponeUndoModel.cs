// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Undo.PostponeUndoModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Undo
{
  public class PostponeUndoModel : UndoController
  {
    private List<string> _originalTasks;
    private List<string> _originalCheckItems;
    private List<TimeData> _originalTimeDates;
    private List<TaskModel> _originalTaskModels;
    private List<TaskDetailItemModel> _originalItemModels;
    private readonly HashSet<string> _fullTasks = new HashSet<string>();

    public async Task Do()
    {
      PostponeUndoModel sender = this;
      if (sender._fullTasks.Count <= 0)
        return;
      if (sender._originalCheckItems != null && sender._originalCheckItems.Any<string>())
      {
        List<TaskDetailItemModel> items = await TaskDetailItemDao.BatchGetItems(sender._originalCheckItems);
        sender._originalItemModels = items;
        int today = await TaskService.BatchSetCheckItemToToday(sender._originalCheckItems);
      }
      if (sender._originalTasks != null && sender._originalTasks.Any<string>())
      {
        List<TaskModel> thinTasksInBatch = await TaskDao.GetThinTasksInBatch(sender._originalTasks);
        sender._originalTaskModels = thinTasksInBatch;
        TimeData model = new TimeData()
        {
          StartDate = new DateTime?(DateTime.Today),
          BatchData = new BatchData()
          {
            IsTimeUnified = false,
            IsRepeatUnified = false,
            IsDateUnified = true
          }
        };
        await TaskService.BatchSetDate(sender._originalTasks, model);
      }
      TaskChangeNotifier.NotifyBatchDateChanged(sender._fullTasks.ToList<string>(), (object) sender, false);
    }

    public override string GetTitle() => "";

    public override string GetContent() => Utils.GetString("Postponed");

    public override async void Undo()
    {
      PostponeUndoModel sender = this;
      if (!sender._fullTasks.Any<string>())
        return;
      if (sender._originalTaskModels != null && sender._originalTaskModels.Count > 0)
        await TaskDao.BatchUpdateTasks(sender._originalTaskModels, false, CheckMatchedType.CheckSmart);
      if (sender._originalItemModels != null && sender._originalItemModels.Count > 0)
        await TaskDetailItemDao.BatchUpdateChecklists(sender._originalItemModels);
      TaskChangeNotifier.NotifyBatchDateChanged(sender._fullTasks.ToList<string>(), (object) sender, false);
    }

    public override void Finished() => SyncManager.TryDelaySync();

    public bool InitData(List<TaskBaseViewModel> models)
    {
      if (models == null || models.Count <= 0)
        return false;
      this._fullTasks.Clear();
      models.ForEach((Action<TaskBaseViewModel>) (o =>
      {
        if (!o.IsTask || string.IsNullOrEmpty(o.GetTaskId()))
          return;
        this._fullTasks.Add(o.Id);
      }));
      List<string> list1 = models.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (item =>
      {
        if (!item.IsTask)
          return false;
        if (!item.IsAgenda)
          return true;
        return item.IsAgenda && item.AttendId == item.Id;
      })).Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (item => item.Id)).ToList<string>();
      List<string> list2 = models.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (item => item.IsCheckItem)).Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (item => item.Id)).ToList<string>();
      if (list1.Count > 0)
        this._originalTasks = list1;
      if (list2.Count > 0)
        this._originalCheckItems = list2;
      return list1.Count > 0 || list2.Count > 0;
    }
  }
}
