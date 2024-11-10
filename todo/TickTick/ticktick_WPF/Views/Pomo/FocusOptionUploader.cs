// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusOptionUploader
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views.Tag;
using TickTickDao;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public static class FocusOptionUploader
  {
    private static readonly SemaphoreLocker UploadLocker = new SemaphoreLocker();
    private static CancelModel _optionCanCel;

    public static async Task AddOption(
      FocusOption option,
      DateTime time,
      bool manual,
      string completedTaskId = null,
      string note = null)
    {
      FocusConfig config = TickFocusManager.Config;
      UtilLog.Info("Focus option " + option.ToString() + " oid " + config.Id + " tid " + config.FocusVModel?.FocusId);
      if (string.IsNullOrEmpty(config.Id))
        return;
      if (!config.NeedUpload)
      {
        FocusOptionUploader.SaveLocalFocus();
      }
      else
      {
        FocusOptionModel focusOptionModel1 = new FocusOptionModel();
        focusOptionModel1.id = Utils.GetGuid();
        focusOptionModel1.oId = config.Id;
        focusOptionModel1.oType = config.Type;
        focusOptionModel1.op = option.ToString();
        focusOptionModel1.time = time;
        focusOptionModel1.manual = manual;
        focusOptionModel1.focusOnId = config.FocusVModel?.FocusId;
        FocusViewModel focusVmodel = config.FocusVModel;
        focusOptionModel1.focusOnType = focusVmodel != null ? focusVmodel.Type : 0;
        focusOptionModel1.focusOnTitle = config.FocusVModel?.Title ?? string.Empty;
        focusOptionModel1.firstFocusId = config.FirstFocusId;
        focusOptionModel1.autoPomoLeft = config.AutoTimes;
        focusOptionModel1.pomoCount = config.PomoCount;
        focusOptionModel1.duration = config.Type == 0 ? config.Second / 60L : 0L;
        FocusOptionModel focusOptionModel2 = focusOptionModel1;
        if (!string.IsNullOrEmpty(completedTaskId))
        {
          focusOptionModel2.focusOnId = completedTaskId;
          focusOptionModel2.focusOnType = 0;
          focusOptionModel2.focusOnTitle = TaskCache.GetTaskById(completedTaskId)?.Title;
        }
        if (focusOptionModel2.focusOnTitle != null && focusOptionModel2.focusOnTitle.Length > 64)
          focusOptionModel2.focusOnTitle = focusOptionModel2.focusOnTitle.Substring(0, 64);
        switch (option)
        {
          case FocusOption.drop:
            focusOptionModel2.focusOnId = string.Empty;
            focusOptionModel2.focusOnType = 0;
            break;
          case FocusOption.note:
            if (string.IsNullOrEmpty(note))
              return;
            focusOptionModel2.note = note;
            break;
        }
        if (string.IsNullOrEmpty(focusOptionModel2.firstFocusId))
          focusOptionModel2.firstFocusId = focusOptionModel2.oId;
        List<FocusOptionModel> options = new List<FocusOptionModel>()
        {
          focusOptionModel2
        };
        if (option == FocusOption.finish && !string.IsNullOrEmpty(config.Note))
        {
          FocusOptionModel focusOptionModel3 = focusOptionModel2.Copy();
          focusOptionModel3.op = FocusOption.note.ToString();
          focusOptionModel3.id = Utils.GetGuid();
          focusOptionModel3.note = config.Note;
          options.Add(focusOptionModel3);
        }
        FocusOptionUploader.SaveOption(options, delaySync: true);
      }
    }

    public static async Task SaveOption(
      List<FocusOptionModel> options,
      bool force = false,
      bool resetLocalPieces = false,
      bool delaySync = false,
      bool needLog = false)
    {
      if (!UserDao.IsPro())
        return;
      if (options != null)
      {
        foreach (FocusOptionModel option in options)
        {
          int num = await BaseDao<FocusOptionModel>.InsertAsync(option);
        }
      }
      force = force && LocalSettings.Settings.EnableFocus && LocalSettings.Settings.FocusKeepInSync;
      await FocusOptionUploader.UploadOptions(force, resetLocalPieces, delaySync, needLog);
    }

    private static async Task UploadOptions(
      bool force,
      bool resetLocalPieces,
      bool delaySync = false,
      bool needLog = false)
    {
      FocusOptionUploader._optionCanCel?.Cancel();
      CancelModel cancelModel = new CancelModel();
      FocusOptionUploader._optionCanCel = cancelModel;
      await FocusOptionUploader.UploadLocker.LockAsync((Func<Task>) (async () =>
      {
        List<FocusOptionModel> options = await BaseDao<FocusOptionModel>.GetAllAsync();
        if (!force)
        {
          List<FocusOptionModel> focusOptionModelList = options;
          // ISSUE: explicit non-virtual call
          if ((focusOptionModelList != null ? (__nonvirtual (focusOptionModelList.Count) > 0 ? 1 : 0) : 0) == 0)
          {
            options = (List<FocusOptionModel>) null;
            return;
          }
        }
        long lastPoint = LocalSettings.Settings.FocusPushPoint;
        FocusSyncResultBean focusSyncResultBean = await Communicator.UploadFocusOptions(options);
        if (!string.IsNullOrEmpty(focusSyncResultBean?.errorCode))
        {
          if (focusSyncResultBean.errorCode == "too_busy")
          {
            await Task.Delay(new Random().Next(1000, 2000));
            FocusOptionUploader.UploadOptions(force, resetLocalPieces);
            options = (List<FocusOptionModel>) null;
          }
          else
          {
            UtilLog.Info("HandleRemoteFocusError " + focusSyncResultBean.errorCode);
            foreach (FocusOptionModel model in options)
            {
              int num = await BaseDao<FocusOptionModel>.DeleteAsync(model);
            }
            options = (List<FocusOptionModel>) null;
          }
        }
        else if (!LocalSettings.Settings.FocusKeepInSync)
        {
          options = (List<FocusOptionModel>) null;
        }
        else
        {
          if (needLog && focusSyncResultBean?.current != null)
            UtilLog.Info(JsonConvert.SerializeObject((object) focusSyncResultBean.current));
          FocusSyncResultBean model = focusSyncResultBean;
          List<FocusOptionModel> options1 = options;
          int num1 = resetLocalPieces ? 1 : 0;
          CancelModel optionRandom = cancelModel;
          long num2 = lastPoint;
          long? point = focusSyncResultBean?.point;
          long valueOrDefault = point.GetValueOrDefault();
          int num3 = !(num2 == valueOrDefault & point.HasValue) ? 1 : 0;
          int num4 = delaySync ? 1 : 0;
          await FocusOptionUploader.HandleRemoteFocus(model, options1, num1 != 0, optionRandom, num3 != 0, num4 != 0);
          options = (List<FocusOptionModel>) null;
        }
      }));
    }

    private static async Task HandleRemoteFocus(
      FocusSyncResultBean model,
      List<FocusOptionModel> options,
      bool resetLocalPieces,
      CancelModel optionRandom,
      bool needHandleResult = false,
      bool delaySync = false)
    {
      if (model != null && model.point > 0L)
      {
        foreach (FocusOptionModel option in options)
        {
          int num = await BaseDao<FocusOptionModel>.DeleteAsync(option);
        }
        if (model.current == null || !needHandleResult && (model.current.status == 0 ? 1 : (model.current.status == 1 ? 1 : 0)) == (TickFocusManager.Working ? 1 : 0))
          return;
        string id = TickFocusManager.Config.IsWorking ? TickFocusManager.Config.Id : (string) null;
        string note = TickFocusManager.Config.Note;
        CancelModel cancelModel = optionRandom;
        if ((cancelModel != null ? (cancelModel.IsCanceled() ? 1 : 0) : 0) == 0)
          TickFocusManager.HandleFocusResult(model.current, resetLocalPieces);
        FocusOptionUploader.SaveLocalFocus();
        LocalSettings.Settings.FocusPushPoint = model.point;
        await FocusOptionUploader.UpdateFocuses(((IEnumerable<FocusSyncStatusModel>) model.updates).ToList<FocusSyncStatusModel>(), model.current.id != id ? id : (string) null, note);
      }
      else
        FocusOptionUploader.SaveLocalFocus();
    }

    private static async void SaveLocalFocus()
    {
      await Task.Delay(1000);
      LocalSettings.Settings.ExtraSettings.CurrentFocus = TickFocusManager.Config.ToSaveSyncModel();
      LocalSettings.Settings.Save(true);
    }

    private static async Task UpdateFocuses(
      List<FocusSyncStatusModel> focusModels,
      string localCurrentId,
      string localNote)
    {
      foreach (FocusSyncStatusModel focusModel in focusModels)
      {
        FocusSyncStatusModel focus = focusModel;
        PomodoroModel local = await PomoDao.GetPomoById(focus.id);
        List<PomoTask> newPomoTasks;
        PomoTask task1;
        if (local != null)
        {
          List<PomoTask> pomoTasks = await PomoDao.GetPomoTasksByPomoId(focus.id);
          if (!focus.valid || focus.status != 2 && (DateTime.Now - local.EndTime).TotalSeconds > 20.0)
          {
            if (local.SyncStatus == 0)
            {
              await DeletePomoTasks();
              await PomoDao.DeleteAsync(local);
              continue;
            }
            await PomoService.DeleteById(local.Id);
            continue;
          }
          if (!FocusOptionUploader.IsSamePieces(pomoTasks, focus.focusTasks))
          {
            if (focus.status == 2)
            {
              await DeletePomoTasks();
              newPomoTasks = await FocusOptionUploader.GetPomoTasksInFocus(focus);
              foreach (PomoTask pomoTask in newPomoTasks.Where<PomoTask>((Func<PomoTask, bool>) (task => task.TaskId != null)))
              {
                task1 = pomoTask;
                PomodoroSummaryModel pomo = await PomoSummaryDao.GetPomoByTaskId(task1.TaskId);
                object[] objArray1;
                if (pomo == null)
                {
                  objArray1 = (object[]) null;
                }
                else
                {
                  List<object[]> focuses = pomo.focuses;
                  objArray1 = focuses != null ? focuses.FirstOrDefault<object[]>((Func<object[], bool>) (f => f.Length == 3 && f[0] as string == focus.id)) : (object[]) null;
                }
                object[] objArray2 = objArray1;
                long totalSecond = Utils.GetTotalSecond(task1.StartTime, task1.EndTime);
                if (objArray2 != null)
                {
                  objArray2[2] = (object) ((objArray2[2] as long?).GetValueOrDefault() + totalSecond);
                }
                else
                {
                  if (pomo == null)
                    pomo = new PomodoroSummaryModel()
                    {
                      id = Utils.GetGuid(),
                      userId = LocalSettings.Settings.LoginUserId,
                      taskId = task1.TaskId
                    };
                  pomo.AddFocuses(new object[3]
                  {
                    (object) focus.id,
                    (object) focus.type,
                    (object) totalSecond
                  });
                }
                await PomoSummaryDao.SavePomoSummary(pomo);
                task1 = (PomoTask) null;
              }
              await PomoTaskDao.InsertAllAsync((IEnumerable<PomoTask>) newPomoTasks);
              DateTime? endTime = focus.endTime;
              if (endTime.HasValue)
              {
                local.EndTime = endTime.Value;
                local.PauseDuration = (long) (int) focus.GetPauseTime(true);
                if (Utils.IsEmptyDate(local.StartTime))
                  UtilLog.Info("FocusOptionUploader.UpdateFocuses : " + local.Id);
                else
                  await PomoDao.UpdatePomo(local);
              }
              newPomoTasks = (List<PomoTask>) null;
            }
          }
          else
            continue;
          // ISSUE: variable of a compiler-generated type
          FocusOptionUploader.\u003C\u003Ec__DisplayClass7_0 cDisplayClass70;

          async Task DeletePomoTasks()
          {
            foreach (PomoTask task in pomoTasks)
            {
              if (task.TaskId != null)
              {
                PomodoroSummaryModel pomoByTaskId = await PomoSummaryDao.GetPomoByTaskId(task.TaskId);
                object[] objArray1;
                if (pomoByTaskId == null)
                {
                  objArray1 = (object[]) null;
                }
                else
                {
                  List<object[]> focuses = pomoByTaskId.focuses;
                  // ISSUE: reference to a compiler-generated method
                  objArray1 = focuses != null ? focuses.FirstOrDefault<object[]>(closure_8 ?? (closure_8 = new Func<object[], bool>(cDisplayClass70.\u003CUpdateFocuses\u003Eb__1))) : (object[]) null;
                }
                object[] objArray2 = objArray1;
                if (objArray2 != null)
                {
                  pomoByTaskId.focuses.Remove(objArray2);
                  await PomoSummaryDao.SavePomoSummary(pomoByTaskId);
                }
              }
              await PomoTaskDao.DeleteAsync(task);
            }
          }
        }
        else if (focus.id == localCurrentId && focus.valid && focus.status == 2 && focus.endTime.HasValue)
        {
          newPomoTasks = await FocusOptionUploader.GetPomoTasksInFocus(focus);
          foreach (PomoTask pomoTask in newPomoTasks.Where<PomoTask>((Func<PomoTask, bool>) (task => task.TaskId != null)))
          {
            task1 = pomoTask;
            PomodoroSummaryModel pomo = await PomoSummaryDao.GetPomoByTaskId(task1.TaskId);
            object[] objArray3;
            if (pomo == null)
            {
              objArray3 = (object[]) null;
            }
            else
            {
              List<object[]> focuses = pomo.focuses;
              objArray3 = focuses != null ? focuses.FirstOrDefault<object[]>((Func<object[], bool>) (f => f.Length == 3 && f[0] as string == focus.id)) : (object[]) null;
            }
            object[] objArray4 = objArray3;
            long totalSecond = Utils.GetTotalSecond(task1.StartTime, task1.EndTime);
            if (objArray4 != null)
            {
              objArray4[2] = (object) ((objArray4[2] as long?).GetValueOrDefault() + totalSecond);
            }
            else
            {
              if (pomo == null)
                pomo = new PomodoroSummaryModel()
                {
                  id = Utils.GetGuid(),
                  userId = LocalSettings.Settings.LoginUserId,
                  taskId = task1.TaskId
                };
              pomo.AddFocuses(new object[3]
              {
                (object) focus.id,
                (object) focus.type,
                (object) totalSecond
              });
            }
            await PomoSummaryDao.SavePomoSummary(pomo);
            task1 = (PomoTask) null;
          }
          await PomoTaskDao.InsertAllAsync((IEnumerable<PomoTask>) newPomoTasks);
          await PomoDao.InsertAsync(new PomodoroModel()
          {
            Id = focus.id,
            UserId = LocalSettings.Settings.LoginUserId,
            SyncStatus = 0,
            StartTime = focus.startTime,
            EndTime = focus.endTime.Value,
            PauseDuration = (long) focus.GetPauseTime(true),
            Type = focus.type,
            Note = localNote,
            Status = 1
          });
          newPomoTasks = (List<PomoTask>) null;
        }
        local = (PomodoroModel) null;
      }
    }

    private static bool IsSamePieces(List<PomoTask> pomoTasks, FocusTaskModel[] focusTasks)
    {
      int? count = pomoTasks?.Count;
      int? length = focusTasks?.Length;
      if (!(count.GetValueOrDefault() == length.GetValueOrDefault() & count.HasValue == length.HasValue))
        return false;
      if (pomoTasks == null)
        return true;
      foreach (PomoTask pomoTask1 in pomoTasks)
      {
        PomoTask pomoTask = pomoTask1;
        FocusTaskModel focusTaskModel = ((IEnumerable<FocusTaskModel>) focusTasks).FirstOrDefault<FocusTaskModel>((Func<FocusTaskModel, bool>) (f => EqualTime(new DateTime?(f.startTime), new DateTime?(pomoTask.StartTime))));
        if (focusTaskModel == null || !EqualTime(focusTaskModel.endTime, new DateTime?(pomoTask.EndTime)))
          return false;
      }
      return true;

      static bool EqualTime(DateTime? a, DateTime? b)
      {
        return a.HasValue && b.HasValue && Math.Abs((a.Value - b.Value).TotalSeconds) < 5.0;
      }
    }

    public static async Task<List<PomoTask>> GetPomoTasksInFocus(FocusSyncStatusModel model)
    {
      List<PomoTask> result = new List<PomoTask>();
      if (model.focusTasks == null || model.focusTasks.Length == 0)
      {
        List<PomoTask> pomoTaskModels = await FocusPieceHandler.GetPieceHandleFromSyncModel(model).GetPomoTaskModels(model.GetFocusDuration());
        foreach (PomoTask pomoTask in pomoTaskModels)
          pomoTask.PomoId = model.id;
        return pomoTaskModels;
      }
      FocusTaskModel[] focusTaskModelArray = model.focusTasks;
      for (int index = 0; index < focusTaskModelArray.Length; ++index)
      {
        FocusTaskModel focusTask = focusTaskModelArray[index];
        DateTime? endTime = focusTask.endTime;
        if (endTime.HasValue)
        {
          PomoTask pomoTask1 = new PomoTask();
          pomoTask1.StartTime = focusTask.startTime;
          endTime = focusTask.endTime;
          pomoTask1.EndTime = endTime.Value;
          pomoTask1.PomoId = model.id;
          PomoTask pomoTask = pomoTask1;
          int? type = focusTask.type;
          if (type.HasValue)
          {
            type = focusTask.type;
            int num1 = 0;
            if (!(type.GetValueOrDefault() == num1 & type.HasValue))
            {
              type = focusTask.type;
              int num2 = 2;
              if (type.GetValueOrDefault() == num2 & type.HasValue)
              {
                TimerModel timerById = await TimerDao.GetTimerById(focusTask.id);
                pomoTask.TimerSid = focusTask.id;
                if (timerById != null)
                {
                  pomoTask.TimerName = timerById.Name;
                  if (!string.IsNullOrEmpty(timerById.ObjId))
                  {
                    if (timerById.ObjType == "task")
                    {
                      pomoTask.TaskId = timerById.ObjId;
                      TaskModel taskById = await TaskDao.GetTaskById(timerById.ObjId);
                      if (taskById != null)
                      {
                        pomoTask.ProjectName = CacheManager.GetProjectById(taskById?.projectId)?.name;
                        pomoTask.Title = taskById?.title;
                        PomoTask pomoTask2 = pomoTask;
                        string[] tags = pomoTask.Tags;
                        string jsonContent = TagSerializer.ToJsonContent(tags != null ? ((IEnumerable<string>) tags).ToList<string>() : (List<string>) null);
                        pomoTask2.TagString = jsonContent;
                        goto label_25;
                      }
                      else
                        goto label_25;
                    }
                    else if (timerById.ObjType == "habit")
                    {
                      pomoTask.HabitId = timerById.ObjId;
                      pomoTask.Title = timerById.Name;
                      goto label_25;
                    }
                    else
                      goto label_25;
                  }
                  else
                    goto label_25;
                }
                else
                {
                  pomoTask.TimerName = focusTask.title;
                  goto label_25;
                }
              }
              else
              {
                pomoTask.HabitId = focusTask.id;
                pomoTask.Title = (await HabitDao.GetHabitById(focusTask.id))?.Name ?? focusTask.title;
                goto label_25;
              }
            }
          }
          pomoTask.TaskId = focusTask.id;
          TaskModel taskById1 = await TaskDao.GetTaskById(focusTask.id);
          pomoTask.ProjectName = CacheManager.GetProjectById(taskById1?.projectId)?.name;
          pomoTask.Title = taskById1?.title ?? focusTask.title;
          pomoTask.TagString = taskById1?.tag;
label_25:
          result.Add(pomoTask);
          pomoTask = (PomoTask) null;
          focusTask = (FocusTaskModel) null;
        }
      }
      focusTaskModelArray = (FocusTaskModel[]) null;
      return result;
    }

    private static void SaveFocusStatus()
    {
    }
  }
}
