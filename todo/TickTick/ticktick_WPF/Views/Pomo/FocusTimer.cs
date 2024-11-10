// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusTimer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusTimer
  {
    private static readonly SemaphoreLocker TimerLock = new SemaphoreLocker();
    private static readonly Timer FTimer = new Timer(1000.0);
    private static string _savedId;
    private static string _lastCompletePomoId;

    public static FocusConfig Config => TickFocusManager.Config;

    public static DateTime TimerStartTime { get; set; }

    public static double TimerPauseDuration { get; set; }

    static FocusTimer()
    {
      FocusTimer.FTimer.Elapsed += new ElapsedEventHandler(FocusTimer.OnFTimerElapsed);
    }

    public static bool IsTiming => FocusTimer.FTimer.Enabled;

    public static bool IsPomo => FocusTimer.Config.Type == 0;

    public static bool IsWorking
    {
      get => FocusTimer.Status == PomoStatus.Working || FocusTimer.Status == PomoStatus.Pause;
    }

    private static PomoStatus Status
    {
      get => FocusTimer.Config.Status;
      set => FocusTimer.Config.Status = value;
    }

    public static void BeginTimer(bool auto = false)
    {
      DateTime now = DateTime.Now;
      if (FocusTimer.Status == PomoStatus.WaitingWork)
      {
        PomoSoundPlayer.StartPlaySound(false);
        FocusTimer.Config.StartFocus(now, auto);
        FocusTimer.Status = PomoStatus.Working;
        if (LocalSettings.Settings.PomoLocalSetting.AutoShowWidget)
          TickFocusManager.HideOrShowFocusWidget(true, 600);
      }
      else
        FocusTimer.Status = PomoStatus.Relaxing;
      FocusTimer.TimerStartTime = now;
      FocusTimer.TimerPauseDuration = 0.0;
      FocusTimer.FTimer.Start();
      FocusOptionUploader.AddOption(FocusTimer.Status == PomoStatus.Working ? FocusOption.start : FocusOption.startBreak, now, !auto);
    }

    private static async void OnFTimerElapsed(object sender, ElapsedEventArgs e)
    {
      await FocusTimer.TimerLock.LockAsync((Func<Task>) (async () =>
      {
        FocusTimer.TimerStartTime = Utils.IsEmptyDate(FocusTimer.TimerStartTime) ? FocusTimer.Config.StartTime : FocusTimer.TimerStartTime;
        double num1 = (DateTime.Now - FocusTimer.TimerStartTime).TotalSeconds - FocusTimer.TimerPauseDuration;
        if (FocusTimer.IsPomo)
        {
          if (FocusTimer.Config.CurrentSeconds <= 0.0 && FocusTimer.Config.Id == FocusTimer._lastCompletePomoId)
            return;
          FocusTimer.Config.CurrentSeconds = (double) FocusTimer.Config.Second - num1;
          if (FocusTimer.Config.CurrentSeconds > 0.0)
            return;
          FocusTimer._lastCompletePomoId = FocusTimer.Config.Id;
          if (Application.Current.Dispatcher == null)
            return;
          Task task = await Application.Current.Dispatcher.InvokeAsync<Task>((Func<Task>) (async () =>
          {
            FocusTimer.FTimer.Stop();
            FocusTimer.Config.CurrentSeconds = 0.0;
            DateTime time = DateTime.Now;
            int num2 = FocusTimer.Status == PomoStatus.Working ? 1 : 0;
            if (num2 != 0)
              time = FocusTimer.TimerStartTime.AddSeconds(FocusTimer.TimerPauseDuration + (double) FocusTimer.Config.Second);
            FocusOptionUploader.AddOption(num2 != 0 ? FocusOption.finish : FocusOption.endBreak, time, false);
            if (FocusTimer.IsWorking)
            {
              FocusTimer.Config.LastPomoCompleteTime = time;
              await FocusTimer.CompleteFocus(FocusTimer.Config.Second, time, FocusTimer.Config.Type);
            }
            TickFocusManager.PlayTerminationSound(FocusTimer.Status != 0);
            await FocusTimer.SwitchPomoBreak();
          }));
        }
        else
        {
          FocusTimer.Config.CurrentSeconds = num1;
          if (FocusTimer.Config.CurrentSeconds < 43200.0)
            return;
          FocusTimer.StopTiming();
        }
      }));
    }

    private static async Task StopTiming()
    {
      Task task = await Application.Current.Dispatcher.InvokeAsync<Task>((Func<Task>) (async () =>
      {
        FocusTimer.FTimer.Stop();
        PomoSoundPlayer.StopPlaySound();
        if (FocusTimer.Config.CurrentSeconds > 3600.0)
          await FocusTimer.EnsureTimingNum();
        else if (FocusTimer.Config.CurrentSeconds > 300.0)
          await FocusTimer.SaveTimingNum((long) FocusTimer.Config.CurrentSeconds);
        else
          FocusOptionUploader.AddOption(FocusOption.drop, DateTime.Now, true);
        FocusTimer.Status = PomoStatus.WaitingWork;
        FocusTimer.Config.Reset();
      }));
    }

    private static async Task SaveTimingNum(long num)
    {
      DateTime time = FocusTimer.Config.StartTime.AddSeconds(FocusTimer.Config.PauseDuration + (double) num);
      if (num >= 300L)
        FocusOptionUploader.AddOption(FocusOption.finish, time, true);
      FocusTimer.CompleteFocus(num, DateTime.Now, 1);
    }

    private static async Task EnsureTimingNum()
    {
      long duration = (long) FocusTimer.Config.CurrentSeconds;
      bool manual = duration < 43200L;
      Window dialogParentWindow;
      for (dialogParentWindow = TickFocusManager.GetDialogParentWindow(manual); !manual && dialogParentWindow == null; dialogParentWindow = TickFocusManager.GetDialogParentWindow(false))
        await Task.Delay(500);
      EnsureTimingDialog ensureTimingDialog = new EnsureTimingDialog(duration);
      try
      {
        ensureTimingDialog.Owner = dialogParentWindow;
      }
      catch (Exception ex)
      {
        ensureTimingDialog.Owner = (Window) null;
      }
      ensureTimingDialog.OnSaveClick += (EventHandler<long>) ((o, e) => FocusTimer.SaveTimingNum(e));
      ensureTimingDialog.ShowDialog();
    }

    public static async Task SwitchPomoBreak(bool isSkip = false)
    {
      FocusTimer.FTimer.Stop();
      PomoSoundPlayer.StopPlaySound();
      TickFocusManager.HidePomoReminder();
      switch (FocusTimer.Status)
      {
        case PomoStatus.Working:
        case PomoStatus.Pause:
          if (LocalSettings.Settings.AutoNextPomo && LocalSettings.Settings.AutoBreak && FocusTimer.Config.AutoTimes <= 0)
          {
            FocusTimer.Status = PomoStatus.WaitingWork;
            FocusOptionUploader.AddOption(FocusOption.exit, DateTime.Now, true);
            return;
          }
          FocusTimer.Status = PomoStatus.WaitingRelax;
          break;
        case PomoStatus.Relaxing:
          FocusTimer.Status = PomoStatus.WaitingWork;
          break;
      }
      TickFocusManager.ShowPomoReminder(FocusTimer.Status == PomoStatus.WaitingRelax, isSkip);
      await Task.Delay(600);
      if ((FocusTimer.Status != PomoStatus.WaitingWork || !LocalSettings.Settings.AutoNextPomo ? (FocusTimer.Status != PomoStatus.WaitingRelax ? 0 : (LocalSettings.Settings.AutoBreak ? 1 : 0)) : 1) == 0 || FocusTimer.Config.AutoTimes <= 0)
        return;
      FocusTimer.BeginTimer(true);
    }

    public static void Pause(DateTime time, bool isManual = true)
    {
      if (FocusTimer.Status == PomoStatus.Pause)
        return;
      FocusTimer.Status = PomoStatus.Pause;
      FocusTimer.Config.OnPause(time);
      FocusTimer.FTimer.Stop();
      PomoSoundPlayer.StopPlaySound();
      if (!isManual)
        return;
      FocusOptionUploader.AddOption(FocusOption.pause, time, true);
    }

    public static void Continue(DateTime? time, bool isManual = true)
    {
      FocusTimer.Status = PomoStatus.Working;
      PomoSoundPlayer.StartPlaySound(false);
      if (time.HasValue)
        FocusTimer.Config.OnContinue(time.Value);
      FocusTimer.TimerPauseDuration = FocusTimer.Config.PauseDuration;
      FocusTimer.FTimer.Start();
      if (!isManual || !time.HasValue)
        return;
      FocusTimer.Config.PieceHandler.AddPiece(FocusTimer.Config.FocusVModel, new DateTime?(time.Value));
      FocusOptionUploader.AddOption(FocusOption.@continue, time.Value, true);
    }

    public static void Drop()
    {
      switch (FocusTimer.Status)
      {
        case PomoStatus.Working:
        case PomoStatus.Pause:
          FocusTimer.StopOrAbandon();
          break;
        case PomoStatus.Relaxing:
        case PomoStatus.WaitingRelax:
          FocusOptionUploader.AddOption(FocusOption.endBreak, DateTime.Now, true);
          FocusTimer.SkipRelax();
          break;
      }
    }

    public static async Task<bool> StopOrAbandon(bool waitSync = false)
    {
      bool flag;
      if (FocusTimer.IsPomo)
        flag = await FocusTimer.TryAbandonPomo();
      else
        flag = await FocusTimer.TryStopTiming(waitSync);
      int num = flag ? 1 : 0;
      if (num != 0)
        TickFocusManager.OnDrop();
      return num != 0;
    }

    public static async Task<bool> TryAbandonPomo()
    {
      long second = (long) Math.Ceiling((double) FocusTimer.Config.Second - FocusTimer.Config.CurrentSeconds);
      bool? result = new bool?(false);
      if (FocusTimer.IsWorking && second >= 30L)
        await App.Instance.Dispatcher.InvokeAsync((Action) (() =>
        {
          Window dialogParentWindow = TickFocusManager.GetDialogParentWindow(true);
          PomoAbandonDialog pomoAbandonDialog = new PomoAbandonDialog(!FocusTimer.Config.FocusVModel.NoTask && !FocusTimer.Config.FocusVModel.IsHabit, second >= 300L);
          try
          {
            pomoAbandonDialog.Owner = dialogParentWindow;
          }
          catch (Exception ex)
          {
            pomoAbandonDialog.Owner = (Window) null;
          }
          pomoAbandonDialog.ShowDialog();
          result = pomoAbandonDialog.Saved;
        }));
      if (result.HasValue)
      {
        DateTime now = DateTime.Now;
        if (result.Value && second >= 300L)
        {
          await FocusTimer.CompleteFocus(second, now, 0);
          FocusOptionUploader.AddOption(FocusOption.finish, now, true);
          FocusTimer.Reset();
        }
        else
        {
          FocusOptionUploader.AddOption(FocusOption.drop, now, true);
          FocusTimer.ForceDrop();
        }
      }
      return result.HasValue;
    }

    public static void ForceDrop()
    {
      FocusTimer.FTimer.Stop();
      PomoSoundPlayer.StopPlaySound();
      FocusTimer.Status = PomoStatus.WaitingWork;
      FocusTimer.Config.Reset();
      TickFocusManager.HidePomoReminder();
    }

    public static void Reset(bool isExit = true, bool resetId = true)
    {
      FocusTimer.FTimer.Stop();
      TickFocusManager.ClearLocalFocus();
      if (isExit)
        FocusOptionUploader.AddOption(FocusOption.exit, DateTime.Now, true);
      PomoSoundPlayer.StopPlaySound();
      FocusTimer.Config.Reset(resetId);
      TickFocusManager.HidePomoReminder();
    }

    public static async Task<bool> TryStopTiming(bool waitSync)
    {
      bool? result = new bool?(FocusTimer.Config.CurrentSeconds >= 300.0);
      if (FocusTimer.IsWorking && FocusTimer.Config.CurrentSeconds >= 30.0 && FocusTimer.Config.CurrentSeconds < 300.0)
      {
        PomoAbandonDialog pomoAbandonDialog = new PomoAbandonDialog(false, false);
        pomoAbandonDialog.ShowDialog();
        result = pomoAbandonDialog.Saved;
      }
      if (result.HasValue)
      {
        if (!result.Value)
          FocusTimer.Config.CurrentSeconds = 0.0;
        await FocusTimer.StopTiming();
        if (result.Value & waitSync)
          await Task.Delay(1000);
      }
      return result.HasValue;
    }

    public static void SkipRelax()
    {
      FocusOptionUploader.AddOption(FocusOption.endBreak, DateTime.Now, true);
      FocusTimer.Config.Status = PomoStatus.Relaxing;
      FocusTimer.SwitchPomoBreak(true);
    }

    public static void StoreConfigAndStop() => FocusTimer.FTimer.Stop();

    public static void StartTimer()
    {
      if (FocusTimer.Status != PomoStatus.Working && FocusTimer.Status != PomoStatus.Relaxing)
        return;
      FocusTimer.FTimer.Start();
    }

    public static void Stop() => FocusTimer.FTimer.Stop();

    private static async Task CompleteFocus(long duration, DateTime time, int type)
    {
      if (duration < 300L)
        return;
      FocusTimer.Config.PieceHandler.AddPiece(new FocusPieceModel()
      {
        Id = "StartPause",
        BeginTime = time
      }, true);
      await FocusTimer.SaveFocus(type, duration);
    }

    private static async Task SaveFocus(int type, long duration)
    {
      if (FocusTimer._savedId == FocusTimer.Config.Id)
        return;
      FocusTimer._savedId = FocusTimer.Config.Id;
      if (duration < 300L)
        return;
      List<PomoTask> pomoTaskModels = await FocusTimer.Config.PieceHandler.GetPomoTaskModels(duration);
      PomodoroModel pomo = new PomodoroModel()
      {
        Id = FocusTimer.Config.Id,
        StartTime = FocusTimer.Config.PieceHandler.GetBeginTime(),
        SyncStatus = 0,
        Type = type,
        Status = 1,
        UserId = Utils.GetCurrentUserStr(),
        Note = FocusTimer.Config.Note
      };
      long num = 0;
      List<PomoTask> pomoTaskList = new List<PomoTask>();
      // ISSUE: explicit non-virtual call
      if (pomoTaskModels != null && __nonvirtual (pomoTaskModels.Count) > 0)
      {
        foreach (PomoTask pomoTask in pomoTaskModels)
        {
          if (pomoTask.TaskId == "StartPause")
          {
            num += Utils.GetTotalSecond(pomoTask.StartTime, pomoTask.EndTime);
          }
          else
          {
            pomoTask.PomoId = pomo.Id;
            DateTime dateTime = pomo.StartTime.AddSeconds((double) (num + duration));
            if (pomoTask.EndTime > dateTime)
            {
              if (pomoTask.StartTime < dateTime)
              {
                pomoTask.EndTime = dateTime;
                pomoTaskList.Add(pomoTask);
                break;
              }
              break;
            }
            pomoTaskList.Add(pomoTask);
          }
        }
        pomo.PauseDuration = num;
        pomo.Tasks = pomoTaskList.ToArray();
      }
      if (pomo.Tasks == null || pomo.Tasks.Length == 0)
        FocusTimer.Config.PieceHandler.PrintLog();
      pomo.EndTime = pomo.StartTime.AddSeconds((double) (pomo.PauseDuration + duration));
      await PomoService.SaveFocusModel(pomo);
    }

    public static async Task TryStartWithId(string id, int type = 0, bool force = false)
    {
      await FocusTimer.Config.SetFocusId(id, type);
      if (!force)
        return;
      FocusTimer.TryBeginFocus();
    }

    private static async void TryBeginFocus()
    {
      switch (FocusTimer.Status)
      {
        case PomoStatus.Relaxing:
        case PomoStatus.WaitingRelax:
          FocusTimer.SkipRelax();
          FocusTimer.BeginTimer();
          break;
        case PomoStatus.WaitingWork:
          FocusTimer.BeginTimer();
          break;
        case PomoStatus.Pause:
          FocusTimer.Continue(new DateTime?(DateTime.Now));
          break;
      }
    }
  }
}
