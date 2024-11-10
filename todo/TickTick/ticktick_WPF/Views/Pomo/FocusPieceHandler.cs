// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusPieceHandler
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
using ticktick_WPF.Util;
using TickTickDao;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusPieceHandler
  {
    private List<FocusPieceModel> _pieceModels = new List<FocusPieceModel>();
    private FocusPieceModel _finalModel;
    private DateTime _startTime;

    public void Reset(DateTime startTime)
    {
      this._pieceModels.Clear();
      this._startTime = startTime;
      this._finalModel = (FocusPieceModel) null;
    }

    public void AddPiece(FocusPieceModel model, bool force = false)
    {
      if (!force && this._finalModel != null && Utils.IsEqualString(this._finalModel.Id, model.Id))
        return;
      if (this._finalModel != null && !this._finalModel.EndTime.HasValue)
        this._finalModel.EndTime = new DateTime?(model.BeginTime);
      this._finalModel = model;
      this._pieceModels.Add(model);
    }

    public (FocusPieceModel, bool) GetLastRealPiece()
    {
      for (int index = this._pieceModels.Count - 1; index >= 0; --index)
      {
        if (!(this._pieceModels[index].Id == "StartPause"))
        {
          DateTime? endTime = this._pieceModels[index].EndTime;
          TimeSpan timeSpan;
          if (endTime.HasValue && !Utils.IsEmptyDate(this._pieceModels[index].EndTime))
          {
            endTime = this._pieceModels[index].EndTime;
            timeSpan = endTime.Value - this._pieceModels[index].BeginTime;
          }
          else
            timeSpan = DateTime.Now - this._pieceModels[index].BeginTime;
          if (string.IsNullOrEmpty(this._pieceModels[index].Id) || timeSpan < TimeSpan.FromMinutes(2.0))
            return (this._pieceModels[index], timeSpan >= TimeSpan.FromMinutes(2.0));
        }
      }
      return ((FocusPieceModel) null, false);
    }

    public async Task ChangePieceTaskId(string oldTaskId, string newTaskId)
    {
      TimerModel timer;
      if (oldTaskId == newTaskId)
      {
        timer = (TimerModel) null;
      }
      else
      {
        timer = await TimerDao.GetTimerByObjId(oldTaskId);
        bool needAdd = true;
        foreach (FocusPieceModel model in this._pieceModels)
        {
          if (await model.GetTaskId() == oldTaskId || timer != null && model.Id == timer.Id)
          {
            model.Id = newTaskId;
            model.Type = 0;
            if (needAdd)
            {
              await FocusOptionUploader.AddOption(FocusOption.focus, model.BeginTime, false, newTaskId);
              needAdd = false;
            }
          }
          else
            needAdd = model.Id != "StartPause";
        }
        timer = (TimerModel) null;
      }
    }

    public void AddPiece(FocusViewModel focusModel, DateTime? now = null)
    {
      this.AddPiece(new FocusPieceModel()
      {
        BeginTime = now ?? DateTime.Now,
        Id = focusModel.FocusId,
        Type = focusModel.Type
      });
    }

    public async Task<List<PomoTask>> GetPomoTaskModels(long duration)
    {
      List<PomoTask> pomoTasks = new List<PomoTask>();
      long count = 0;
      foreach (FocusPieceModel focusPieceModel in this._pieceModels.ToList<FocusPieceModel>())
      {
        FocusPieceModel piece = focusPieceModel;
        if (piece.EndTime.HasValue)
        {
          DateTime start = piece.BeginTime;
          DateTime end = piece.EndTime.Value;
          if (piece.Id == "StartPause")
            pomoTasks.Add(new PomoTask()
            {
              TaskId = piece.Id,
              StartTime = start,
              EndTime = end
            });
          else if (count < duration)
          {
            long totalSecond = Utils.GetTotalSecond(start, end);
            count += totalSecond;
            bool overLimit = false;
            if (count > duration)
            {
              end = start.AddSeconds((double) (duration + totalSecond - count));
              overLimit = true;
            }
            TimerModel timer = (TimerModel) null;
            if (!string.IsNullOrEmpty(piece.Id))
              timer = await TimerDao.GetTimerByIdOrObjId(piece.Id);
            if (piece.Type == 1 || timer?.ObjType == "habit")
            {
              HabitModel habitById = await HabitDao.GetHabitById(timer?.ObjId ?? piece.Id);
              pomoTasks.Add(new PomoTask()
              {
                HabitId = habitById?.Id ?? timer?.ObjId,
                TagString = (string) null,
                StartTime = start,
                EndTime = end,
                ProjectName = (string) null,
                Title = habitById?.Name,
                TimerSid = timer?.Id,
                TimerName = timer?.Name
              });
            }
            else
            {
              TaskModel task = await TaskDao.GetThinTaskById(timer?.ObjId ?? piece.Id);
              pomoTasks.Add(new PomoTask()
              {
                TaskId = task?.id ?? timer?.ObjId,
                TagString = task?.tag,
                StartTime = start,
                EndTime = end,
                ProjectName = task == null ? (string) null : CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == task.projectId))?.name,
                Title = task?.title,
                TimerSid = timer?.Id,
                TimerName = timer?.Name
              });
            }
            if (!overLimit)
            {
              timer = (TimerModel) null;
              piece = (FocusPieceModel) null;
            }
            else
              break;
          }
          else
            break;
        }
        else
          break;
      }
      List<PomoTask> pomoTaskModels = pomoTasks;
      pomoTasks = (List<PomoTask>) null;
      return pomoTaskModels;
    }

    public DateTime GetBeginTime()
    {
      if (this._startTime == DateTime.MinValue)
      {
        FocusPieceModel focusPieceModel = this._pieceModels.FirstOrDefault<FocusPieceModel>();
        this._startTime = focusPieceModel != null ? focusPieceModel.BeginTime : DateTime.Now;
      }
      return this._startTime;
    }

    public static async Task LoadSavedFocus()
    {
      await Task.Delay(500);
      try
      {
        FocusSaveStatusModel model = JsonConvert.DeserializeObject<FocusSaveStatusModel>(LocalSettings.Settings.ExtraSettings.CurrentFocus);
        if (model == null || model.status >= 2)
          return;
        TickFocusManager.Config.NeedUpload = model.needSync;
        TickFocusManager.Config.PieceHandler.SetRemoteModel((FocusSyncStatusModel) model);
      }
      catch (Exception ex)
      {
      }
    }

    public void SetRemoteModel(FocusSyncStatusModel model)
    {
      FocusPieceHandler handleFromSyncModel = FocusPieceHandler.GetPieceHandleFromSyncModel(model);
      this._startTime = handleFromSyncModel._startTime;
      this._pieceModels = handleFromSyncModel._pieceModels;
      this._finalModel = handleFromSyncModel._finalModel;
    }

    public static FocusPieceHandler GetPieceHandleFromSyncModel(FocusSyncStatusModel model)
    {
      FocusPieceHandler pieceHandle = new FocusPieceHandler();
      pieceHandle._startTime = model.startTime;
      FocusLogModel[] focusOnLogs = model.focusOnLogs;
      FocusLogModel focusLogModel1 = focusOnLogs != null ? ((IEnumerable<FocusLogModel>) focusOnLogs).FirstOrDefault<FocusLogModel>((Func<FocusLogModel, bool>) (f => f.time == model.startTime)) : (FocusLogModel) null;
      FocusPieceHandler focusPieceHandler1 = pieceHandle;
      FocusPieceModel model1 = new FocusPieceModel();
      model1.BeginTime = model.startTime;
      model1.Id = focusLogModel1?.id ?? string.Empty;
      int? type = (int?) focusLogModel1?.type;
      model1.Type = type.GetValueOrDefault();
      focusPieceHandler1.AddPiece(model1);
      List<(bool, FocusLogModel)> valueTupleList = new List<(bool, FocusLogModel)>();
      if (model.focusOnLogs != null)
        valueTupleList.AddRange(((IEnumerable<FocusLogModel>) model.focusOnLogs).Select<FocusLogModel, (bool, FocusLogModel)>((Func<FocusLogModel, (bool, FocusLogModel)>) (log => (false, log))));
      if (model.pauseLogs != null)
        valueTupleList.AddRange(((IEnumerable<FocusLogModel>) model.pauseLogs).Select<FocusLogModel, (bool, FocusLogModel)>((Func<FocusLogModel, (bool, FocusLogModel)>) (log => (true, log))));
      valueTupleList.Sort((Comparison<(bool, FocusLogModel)>) ((a, b) => a.Item2.time.CompareTo(b.Item2.time)));
      bool flag1 = false;
      string str = focusLogModel1?.id ?? string.Empty;
      int num1 = 0;
      foreach ((bool flag2, FocusLogModel focusLogModel2) in valueTupleList)
      {
        if (!(focusLogModel2.time == model.startTime))
        {
          if (!flag2)
          {
            if (flag1)
            {
              str = focusLogModel2.id;
              type = focusLogModel2.type;
              num1 = type.GetValueOrDefault();
              continue;
            }
            if (str != focusLogModel2.id)
            {
              str = focusLogModel2.id;
              type = focusLogModel2.type;
              num1 = type.GetValueOrDefault();
              FocusPieceHandler focusPieceHandler2 = pieceHandle;
              FocusPieceModel model2 = new FocusPieceModel();
              model2.BeginTime = focusLogModel2.time;
              model2.Id = str ?? string.Empty;
              type = focusLogModel2.type;
              model2.Type = type.GetValueOrDefault();
              focusPieceHandler2.AddPiece(model2);
            }
          }
          else
          {
            type = focusLogModel2.type;
            int num2 = 1;
            if (type.GetValueOrDefault() == num2 & type.HasValue)
              pieceHandle.AddPiece(new FocusPieceModel()
              {
                BeginTime = focusLogModel2.time,
                Id = str ?? string.Empty,
                Type = num1
              });
            else
              pieceHandle.AddPiece(new FocusPieceModel()
              {
                BeginTime = focusLogModel2.time,
                Id = "StartPause"
              });
          }
          int num3;
          if (flag2)
          {
            type = focusLogModel2.type;
            int num4 = 0;
            num3 = type.GetValueOrDefault() == num4 & type.HasValue ? 1 : 0;
          }
          else
            num3 = 0;
          flag1 = num3 != 0;
        }
      }
      if (model.status == 2)
        pieceHandle.AddPiece(new FocusPieceModel()
        {
          BeginTime = model.endTime ?? DateTime.Now
        }, true);
      FocusPieceHandler.HandleShortPieces(pieceHandle);
      return pieceHandle;
    }

    private static void HandleShortPieces(FocusPieceHandler pieceHandle)
    {
      List<FocusPieceModel> focusPieceModelList = new List<FocusPieceModel>();
      long num = 0;
      string str = string.Empty;
      foreach (FocusPieceModel focusPieceModel1 in pieceHandle._pieceModels.Where<FocusPieceModel>((Func<FocusPieceModel, bool>) (piece => piece.Id != "StartPause")))
      {
        if (str != (focusPieceModel1.Id ?? string.Empty))
        {
          if (num < 120L && !string.IsNullOrEmpty(focusPieceModel1.Id))
          {
            foreach (FocusPieceModel focusPieceModel2 in focusPieceModelList)
            {
              focusPieceModel2.Id = focusPieceModel1.Id;
              focusPieceModel2.Type = focusPieceModel1.Type;
            }
          }
          else
          {
            focusPieceModelList.Clear();
            num = 0L;
          }
          focusPieceModelList.Add(focusPieceModel1);
          num += focusPieceModel1.GetDuration();
          str = focusPieceModel1.Id;
        }
        else
        {
          focusPieceModelList.Add(focusPieceModel1);
          num += focusPieceModel1.GetDuration();
        }
      }
    }

    public List<(DateTime, DateTime)> GetPauseSpans()
    {
      List<(DateTime, DateTime)> pauseSpans = new List<(DateTime, DateTime)>();
      foreach (FocusPieceModel focusPieceModel in this._pieceModels.ToList<FocusPieceModel>())
      {
        DateTime beginTime = focusPieceModel.BeginTime;
        DateTime dateTime = focusPieceModel.EndTime ?? DateTime.Now;
        if (focusPieceModel.Id == "StartPause")
          pauseSpans.Add((beginTime, dateTime));
      }
      return pauseSpans;
    }

    public List<FocusPieceModel> GetPieces() => this._pieceModels.ToList<FocusPieceModel>();

    public void PrintLog()
    {
      try
      {
        string str = "FocusPieceLog " + JsonConvert.SerializeObject((object) this._pieceModels);
        UtilLog.Info(str);
        UserActCollectUtils.SendException(new Exception(str), ExceptionType.Custom);
      }
      catch (Exception ex)
      {
      }
    }
  }
}
