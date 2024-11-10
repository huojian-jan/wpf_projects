// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TickFocusManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.SyncServices;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Pomo.MiniFocus;
using ticktick_WPF.Views.Remind;
using TickTickDao;
using TickTickModels;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public static class TickFocusManager
  {
    public static FocusConfig Config = new FocusConfig(new Action(TickFocusManager.OnCurrentSecondChange), new Action(TickFocusManager.OnStatusChange));
    public static FocusView MainFocus;
    private static bool _showClockPanel;
    public static MiniFocusWindow MiniFocusWindow;
    public static FocusImmerseWindow ImmerseWindow;
    public static bool SaveAfterClose = true;

    public static event FocusChange StatusChanged;

    public static event FocusChange TypeChanged;

    public static event FocusChange CurrentSecondChanged;

    public static event FocusChange DurationChanged;

    public static PomoStatus Status => TickFocusManager.Config.Status;

    public static bool Working => TickFocusManager.Config.IsWorking;

    public static bool IsPomo => TickFocusManager.Config.Type == 0;

    public static bool NoFocus => string.IsNullOrEmpty(TickFocusManager.Config.FocusVModel.FocusId);

    public static bool InRelax
    {
      get
      {
        return TickFocusManager.Status == PomoStatus.WaitingRelax || TickFocusManager.Status == PomoStatus.Relaxing;
      }
    }

    public static int DisplaySecond
    {
      get
      {
        return Math.Min(43200, (int) Math.Round(TickFocusManager.Config.CurrentSeconds, 0, MidpointRounding.AwayFromZero));
      }
    }

    static TickFocusManager()
    {
      PomoSoundHelper.InitSetFocusSound();
      TickFocusManager.LoadTodayData();
    }

    private static void OnStatusChange()
    {
      FocusChange statusChanged = TickFocusManager.StatusChanged;
      if (statusChanged == null)
        return;
      statusChanged();
    }

    private static void OnCurrentSecondChange()
    {
      FocusChange currentSecondChanged = TickFocusManager.CurrentSecondChanged;
      if (currentSecondChanged == null)
        return;
      currentSecondChanged();
    }

    public static void InitFocusControl()
    {
      if (TickFocusManager.MainFocus != null)
        return;
      TickFocusManager.MainFocus = new FocusView(TickFocusManager._showClockPanel);
    }

    public static bool ConfirmSwitch(string id, string title, Window owner = null)
    {
      if (TickFocusManager.IsFocusing(id))
        return false;
      if (!TickFocusManager.Working)
        return true;
      List<Inline> content = new List<Inline>();
      string[] strArray = Utils.GetString(TickFocusManager.IsPomo ? "SwitchTimingTips" : "SwitchPomoTips").Split('0');
      if (strArray.Length == 2)
      {
        content.Add((Inline) new Run(strArray[0].Substring(0, strArray[0].Length - 1)));
        List<Inline> inlineList = content;
        Run run = new Run(title);
        run.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
        inlineList.Add((Inline) run);
        content.Add((Inline) new Run(strArray[1].Substring(1, strArray[1].Length - 1)));
      }
      bool? nullable1 = new CustomerDialog("", content, Utils.GetString("Ok"), Utils.GetString("Cancel"), owner).ShowDialog();
      if (!nullable1.HasValue)
        return false;
      bool? nullable2 = nullable1;
      bool flag = false;
      return !(nullable2.GetValueOrDefault() == flag & nullable2.HasValue);
    }

    public static async Task<bool> TryStartFocusTimer(string id, Window owner)
    {
      TimerModel timerById = await TimerDao.GetTimerById(id);
      if (timerById == null || !TickFocusManager.ConfirmSwitch(timerById.Id, timerById.Name, owner))
        return false;
      if (!FocusTimer.IsWorking)
      {
        TickFocusManager.SetFocusType(!(timerById.Type == "pomodoro") ? 1 : 0);
        if (timerById.Type == "pomodoro")
          TickFocusManager.Config.SetPomoSeconds(new int?(timerById.PomodoroTime));
      }
      FocusTimer.TryStartWithId(timerById.Id, 2, true);
      return true;
    }

    public static async void TryStartFocusHabit(string id, bool isPomo)
    {
      if (TickFocusManager.IsFocusing(id))
      {
        Utils.Toast(Utils.GetString("TaskIsFocused"));
      }
      else
      {
        HabitModel habitById = await HabitDao.GetHabitById(id);
        if (habitById == null || !TickFocusManager.ConfirmSwitch(habitById.Id, habitById.Name))
          return;
        if (!FocusTimer.IsWorking)
        {
          TickFocusManager.SetFocusType(!isPomo ? 1 : 0);
          if (isPomo)
          {
            TimerModel timerByObjId = await TimerDao.GetTimerByObjId(id);
            if (timerByObjId != null && timerByObjId.Type == "pomodoro")
              TickFocusManager.Config.SetPomoSeconds(new int?(timerByObjId.PomodoroTime));
          }
        }
        TickFocusManager.StartFocus(id, true);
      }
    }

    public static async Task StartFocus(string objId = "", bool isHabit = false, bool checkTimer = false)
    {
      if (checkTimer)
      {
        TimerModel timerByObjId = await TimerDao.GetTimerByObjId(objId);
        if (timerByObjId != null)
        {
          TickFocusManager.TryStartFocusTimer(timerByObjId.Id, (Window) null);
          return;
        }
      }
      if (!isHabit)
      {
        TaskBaseViewModel taskById = TaskCache.GetTaskById(objId);
        if (taskById != null)
        {
          ProjectModel projectById = CacheManager.GetProjectById(taskById.ProjectId);
          if (projectById != null)
            TickFocusManager.Config.FocusVModel.SetIdentity((ProjectIdentity) new NormalProjectIdentity(projectById));
        }
      }
      FocusTimer.TryStartWithId(objId, isHabit ? 1 : 0, true);
    }

    public static void OnSessionChanged()
    {
      if (FocusTimer.IsTiming)
        FocusTimer.StoreConfigAndStop();
      else
        FocusTimer.StartTimer();
    }

    public static void ResetPomoSetting() => TickFocusManager.Config.TrySetSeconds();

    public static bool IsFocusing(string id)
    {
      return (id == TickFocusManager.Config.FocusVModel?.FocusId && !string.IsNullOrEmpty(TickFocusManager.Config.FocusVModel?.FocusId) || id == TickFocusManager.Config.FocusVModel?.ObjId && !string.IsNullOrEmpty(TickFocusManager.Config.FocusVModel?.ObjId)) && FocusTimer.IsTiming;
    }

    public static void HidePomoReminder()
    {
      Application.Current?.Dispatcher?.Invoke((Action) (() => ReminderWindowManager.CloseAllPomoReminders()));
      SystemToastUtils.RemovePomoToast();
    }

    public static async Task NotifyPomoTaskChanged(string oldTaskId, string newTaskId)
    {
      if (!string.IsNullOrEmpty(newTaskId))
        await TickFocusManager.Config.PieceHandler.ChangePieceTaskId(oldTaskId, newTaskId);
      if (!(TickFocusManager.Config.FocusVModel.ObjId == oldTaskId))
        return;
      await TickFocusManager.Config.FocusVModel.SetupTask("", 0);
      DateTime now = DateTime.Now;
      if (TickFocusManager.Status == PomoStatus.Working)
        TickFocusManager.Config.PieceHandler.AddPiece(new FocusPieceModel()
        {
          BeginTime = now
        });
      if (!TickFocusManager.Working)
        return;
      FocusOptionUploader.AddOption(FocusOption.focus, now, true);
    }

    public static void ShowPomoReminder(bool relax, bool isSkip)
    {
      if (!TickFocusManager.IsPomo || !relax & isSkip)
        return;
      bool automatic = relax ? LocalSettings.Settings.AutoBreak : LocalSettings.Settings.AutoNextPomo;
      if (!LocalSettings.Settings.ShowReminderInClient && !Utils.IsWindows7() && SystemToastUtils.CheckSystemToastEnable())
        SystemToastUtils.ToastPomo(relax, automatic);
      else
        Application.Current?.Dispatcher?.Invoke((Action) (() => ReminderWindowManager.AddPomoReminder(relax, automatic)));
    }

    public static void BeginFocus(bool isPomo, bool forceShowWidget = false)
    {
      if (!TickFocusManager.Working)
      {
        if (isPomo != (LocalSettings.Settings.PomoType == FocusConstance.Focus))
          TickFocusManager.SetFocusType(!isPomo ? 1 : 0);
        TickFocusManager.StartFocus();
      }
      if (!forceShowWidget)
        return;
      TickFocusManager.HideOrShowFocusWidget(true);
    }

    public static void GetRemotePomos()
    {
      TickFocusManager.LoadTodayData(loadRemote: true, force: false);
    }

    public static void OnPomoSettingsChanged() => TickFocusManager.Config.TrySetSeconds();

    public static void HandleRemoteOption(FocusOptionModel model)
    {
      if (TickFocusManager.Config.Id == null || TickFocusManager.Config.Id == model.oId)
      {
        if (TickFocusManager.Config.FocusVModel.FocusId != model.focusOnId)
          TickFocusManager.Config.FocusVModel.SetupTask(model.focusOnId, model.focusOnType, model.focusOnTitle);
        FocusOption result;
        if (LocalSettings.Settings.FocusKeepInSync && Enum.TryParse<FocusOption>(model.op, true, out result))
        {
          switch (result)
          {
            case FocusOption.pause:
              FocusTimer.Pause(model.time, false);
              break;
            case FocusOption.@continue:
              FocusTimer.Continue(new DateTime?(model.time), false);
              break;
          }
        }
      }
      FocusOptionUploader.SaveOption((List<FocusOptionModel>) null, true, true, needLog: true);
    }

    public static void HandleFocusResult(
      FocusSyncStatusModel model,
      bool resetLocalPieces,
      bool? localNeedSync = null)
    {
      Application.Current?.Dispatcher?.Invoke((Action) (() => TickFocusManager.HandleSyncStatus(model, resetLocalPieces, localNeedSync)));
    }

    public static async Task HandleSyncStatus(
      FocusSyncStatusModel model,
      bool resetLocalPieces,
      bool? localNeedSync = null)
    {
      if (!TickFocusManager.Config.NeedUpload && TickFocusManager.Config.IsWorking || model.type != TickFocusManager.Config.Type && (model.IsDropOrExit() || model.type == 1 && model.status >= 2))
        return;
      if (model.type != TickFocusManager.Config.Type)
      {
        TickFocusManager.SetFocusType(model.type);
        FocusTimer.Stop();
        TickFocusManager.Config.Status = PomoStatus.WaitingWork;
      }
      resetLocalPieces = resetLocalPieces || TickFocusManager.Config.Id != model.id || model.status == 2;
      if (TickFocusManager.Config.Id == model.id && TickFocusManager.Working && model.status == 2 || TickFocusManager.Config.Id != model.id)
        FocusOptionUploader.AddOption(FocusOption.note, DateTime.Now, true, note: TickFocusManager.Config.Note);
      if (TickFocusManager.Config.Id != model.id)
      {
        TickFocusManager.Config.Id = model.id;
        TickFocusManager.Config.Note = string.Empty;
      }
      if (model.IsDropOrExit())
      {
        FocusTimer.Reset(false);
      }
      else
      {
        if (model.status != 3)
        {
          TickFocusManager.Config.NeedUpload = ((int) localNeedSync ?? 1) != 0;
          TickFocusManager.Config.FirstFocusId = model.firstId;
          if (resetLocalPieces)
            TickFocusManager.Config.PieceHandler.SetRemoteModel(model);
          if (model.status < 2)
          {
            FocusLogModel currentFocusOn = model.GetCurrentFocusOn();
            TickFocusManager.Config.FocusVModel.SetupTask(currentFocusOn?.id ?? string.Empty, ((int?) currentFocusOn?.type).GetValueOrDefault(), currentFocusOn?.title);
          }
        }
        else if (TickFocusManager.Config.Status == PomoStatus.WaitingWork)
          return;
        switch (model.status)
        {
          case 0:
            TickFocusManager.Config.Second = model.type == 0 ? model.duration * 60L : 0L;
            TickFocusManager.Config.PauseDuration = model.GetPauseTime();
            TickFocusManager.Config.StartTime = model.startTime;
            FocusTimer.TimerStartTime = model.startTime;
            if (TickFocusManager.Config.Status != PomoStatus.Working)
              FocusTimer.Continue(new DateTime?(), false);
            FocusTimer.StartTimer();
            TickFocusManager.HidePomoReminder();
            break;
          case 1:
            TickFocusManager.Config.Second = model.type == 0 ? model.duration * 60L : 0L;
            TickFocusManager.Config.PauseDuration = model.GetPauseTime();
            TickFocusManager.Config.StartTime = model.startTime;
            FocusTimer.TimerStartTime = model.startTime;
            if (TickFocusManager.Config.Status != PomoStatus.Pause)
            {
              FocusTimer.Pause(model.GetLastPauseTime(), false);
            }
            else
            {
              DateTime lastPauseTime = model.GetLastPauseTime();
              TickFocusManager.Config.PauseTime = model.GetLastPauseTime();
              DateTime startTime = TickFocusManager.Config.StartTime;
              double num = (lastPauseTime - startTime).TotalSeconds - TickFocusManager.Config.PauseDuration;
              TickFocusManager.Config.CurrentSeconds = TickFocusManager.Config.Type == 0 ? (double) TickFocusManager.Config.Second - num : num;
            }
            TickFocusManager.HidePomoReminder();
            break;
          case 2:
            if (model.type == 1 || !string.IsNullOrEmpty(model.firstId) && model.firstId != model.id && model.autoPomoLeft <= 0)
            {
              FocusTimer.Reset(false);
              break;
            }
            DateTime? nullable;
            if (model.type == 0)
            {
              FocusBreakModel focusBreak = model.focusBreak;
              int num;
              if (focusBreak == null)
              {
                num = 0;
              }
              else
              {
                nullable = focusBreak.endTime;
                num = nullable.HasValue ? 1 : 0;
              }
              if (num != 0)
              {
                int status = (int) TickFocusManager.Config.Status;
                TickFocusManager.Config.Status = PomoStatus.WaitingRelax;
                TickFocusManager.Config.Status = PomoStatus.WaitingWork;
                FocusTimer.Stop();
                if (status != 2)
                {
                  TickFocusManager.ShowPomoReminder(false, false);
                  break;
                }
                break;
              }
            }
            FocusBreakModel focusBreak1 = model.focusBreak;
            long? duration;
            int num1;
            if (focusBreak1 == null)
            {
              num1 = 0;
            }
            else
            {
              duration = focusBreak1.duration;
              long num2 = 0;
              num1 = duration.GetValueOrDefault() > num2 & duration.HasValue ? 1 : 0;
            }
            if (num1 != 0)
            {
              FocusBreakModel focusBreak2 = model.focusBreak;
              int num3;
              if (focusBreak2 == null)
              {
                num3 = 0;
              }
              else
              {
                nullable = focusBreak2.startTime;
                num3 = nullable.HasValue ? 1 : 0;
              }
              if (num3 != 0)
              {
                nullable = model.focusBreak.startTime;
                DateTime startTime = model.startTime;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() > startTime ? 1 : 0) : 0) != 0)
                {
                  nullable = model.focusBreak.startTime;
                  DateTime dateTime = nullable.Value;
                  ref DateTime local = ref dateTime;
                  duration = model.focusBreak.duration;
                  double num4 = (double) duration.Value;
                  if (local.AddMinutes(num4) > DateTime.Now)
                  {
                    if (TickFocusManager.Config.Status != PomoStatus.Relaxing)
                      TickFocusManager.Config.Status = PomoStatus.Relaxing;
                    FocusConfig config = TickFocusManager.Config;
                    duration = model.focusBreak.duration;
                    long num5 = duration.Value * 60L;
                    config.Second = num5;
                    nullable = model.focusBreak.startTime;
                    FocusTimer.TimerStartTime = nullable.Value;
                    FocusTimer.TimerPauseDuration = 0.0;
                    FocusTimer.StartTimer();
                    TickFocusManager.HidePomoReminder();
                    break;
                  }
                }
              }
            }
            if (model.type == 0 && TickFocusManager.Config.Status != PomoStatus.Relaxing && TickFocusManager.Config.Status != PomoStatus.WaitingRelax)
            {
              TickFocusManager.Config.Status = PomoStatus.Working;
              FocusTimer.SwitchPomoBreak();
              break;
            }
            break;
          case 3:
            FocusTimer.Reset(false);
            break;
        }
        TickFocusManager.Config.AutoTimes = model.autoPomoLeft;
        TickFocusManager.Config.PomoCount = model.pomoCount;
      }
    }

    public static void PlayTerminationSound(bool isRest)
    {
      RemindSoundPlayer.PlayFocusRemindSound(isRest);
    }

    public static void OnTasksChanged(TasksChangeEventArgs e)
    {
      if (e.DeletedChangedIds.Contains(TickFocusManager.Config.FocusVModel.FocusId))
        FocusTimer.TryStartWithId(string.Empty);
      if (!e.TaskTextChangedIds.Contains(TickFocusManager.Config.FocusVModel.ObjId))
        return;
      TaskBaseViewModel taskById = TaskCache.GetTaskById(TickFocusManager.Config.FocusVModel.ObjId);
      TickFocusManager.Config.FocusVModel.Title = taskById?.Title;
    }

    public static Geometry GetSoundIcon()
    {
      return Utils.GetIconData("Ic" + FocusConstance.GetSoundIndex(LocalSettings.Settings.PomoSound)) ?? Utils.GetIconData("IcMute");
    }

    public static void SetFocusType(int type)
    {
      if (TickFocusManager.Config.Type == type)
        return;
      int num = !TickFocusManager.Config.FromRelax || string.IsNullOrEmpty(TickFocusManager.Config.Id) || !TickFocusManager.IsPomo ? (TickFocusManager.InRelax ? 1 : 0) : 1;
      LocalSettings.Settings.PomoType = type == 0 ? FocusConstance.Focus : FocusConstance.Timing;
      TickFocusManager.Config.Type = type;
      FocusTimer.Reset(num != 0, false);
      FocusChange typeChanged = TickFocusManager.TypeChanged;
      if (typeChanged != null)
        typeChanged();
      TickFocusManager.LoadTodayData(new bool?(TickFocusManager.IsPomo), true);
    }

    public static Window GetDialogParentWindow(bool manual)
    {
      if (!manual)
      {
        if (TickFocusManager.ImmerseWindow != null)
          return (Window) TickFocusManager.ImmerseWindow;
        if (App.Window.IsActive && App.Window.IsVisible && App.Window.ShowFocus)
          return (Window) App.Window;
        if (TickFocusManager.MiniFocusWindow != null)
          return (Window) TickFocusManager.MiniFocusWindow;
      }
      else
      {
        if (App.Window.IsActive && App.Window.IsVisible)
          return (Window) App.Window;
        if (TickFocusManager.MiniFocusWindow != null && TickFocusManager.MiniFocusWindow.IsActive)
          return (Window) TickFocusManager.MiniFocusWindow;
      }
      return (Window) TickFocusManager.ImmerseWindow ?? (Window) App.Window;
    }

    public static void AdjustDuration(bool add)
    {
      if (!TickFocusManager.IsPomo || !TickFocusManager.Working)
        return;
      UserActCollectUtils.AddClickEvent("focus", "pomo_running", add ? "click_+" : "click_-");
      TickFocusManager.Config.AdjustDuration(add);
      FocusChange durationChanged = TickFocusManager.DurationChanged;
      if (durationChanged == null)
        return;
      durationChanged();
    }

    public static async Task OnFocusIdSelected(string id, int type)
    {
      if (string.IsNullOrEmpty(id))
        return;
      FocusTimer.TryStartWithId(id, type);
      if (TickFocusManager.Status != PomoStatus.WaitingWork || TickFocusManager.Config.FromRelax || !TickFocusManager.IsPomo)
        return;
      TimerModel timerByIdOrObjId = await TimerDao.GetTimerByIdOrObjId(id);
      if (timerByIdOrObjId == null || timerByIdOrObjId.Status != 0 || !(timerByIdOrObjId.Type == "pomodoro") || timerByIdOrObjId.Deleted)
        return;
      TickFocusManager.Config.SetPomoSeconds(new int?(timerByIdOrObjId.PomodoroTime));
    }

    public static void ShowImmerseWindow(Window owner)
    {
      if (TickFocusManager.ImmerseWindow == null)
      {
        FocusImmerseWindow focusImmerseWindow = new FocusImmerseWindow();
        focusImmerseWindow.Owner = owner;
        TickFocusManager.ImmerseWindow = focusImmerseWindow;
        TickFocusManager.ImmerseWindow.Show();
      }
      else
      {
        try
        {
          TickFocusManager.ImmerseWindow.Activate();
        }
        catch (Exception ex)
        {
          TickFocusManager.ImmerseWindow = (FocusImmerseWindow) null;
          FocusImmerseWindow focusImmerseWindow = new FocusImmerseWindow();
          focusImmerseWindow.Owner = owner;
          TickFocusManager.ImmerseWindow = focusImmerseWindow;
          TickFocusManager.ImmerseWindow.Show();
        }
      }
    }

    public static void OpenStatisticsInWeb()
    {
      Utils.TryProcessStartUrlWithToken("/webapp/#statistics/pomo?enablePomo=true");
    }

    public static void ReloadStatistics()
    {
      TickFocusManager.LoadStatistics();
      TickFocusManager.MainFocus?.ReloadStatistics();
    }

    public static string GetActCType()
    {
      string actCtype = "focus_tab";
      switch (TickFocusManager.Status)
      {
        case PomoStatus.Working:
          actCtype = TickFocusManager.IsPomo ? "pomo_running" : "stopwatch_running";
          break;
        case PomoStatus.Relaxing:
          actCtype = "pomo_relaxing";
          break;
        case PomoStatus.WaitingWork:
          if (TickFocusManager.IsPomo && TickFocusManager.Config.FromRelax)
          {
            actCtype = "pomo_again";
            break;
          }
          break;
        case PomoStatus.WaitingRelax:
          actCtype = "pomo_finished";
          break;
        case PomoStatus.Pause:
          actCtype = TickFocusManager.IsPomo ? "pomo_paused" : "stopwatch_pause";
          break;
      }
      return actCtype;
    }

    public static void Clear()
    {
      FocusTimer.Stop();
      TickFocusManager.Config.Reset();
    }

    public static void OnDrop()
    {
      TickFocusManager.MainFocus?.OnDropFocus();
      LocalSettings.Settings.ExtraSettings.CurrentFocus = (string) null;
      LocalSettings.Settings.Save(true);
    }

    public static void OnTimerChanged(TimerModel timer)
    {
      if (!(TickFocusManager.Config.FocusVModel.FocusId == timer.Id))
        return;
      TickFocusManager.Config.FocusVModel.Title = timer.Name;
    }

    public static async void LoadSavedFocus()
    {
      try
      {
        await FocusOptionUploader.SaveOption((List<FocusOptionModel>) null, true, true);
        await Task.Delay(3000);
        if (TickFocusManager.Status != PomoStatus.WaitingWork)
          return;
        FocusSyncStatusModel model = JsonConvert.DeserializeObject<FocusSyncStatusModel>(LocalSettings.Settings.ExtraSettings.CurrentFocus);
        if (model != null && !Utils.IsEmptyDate(model.startTime))
        {
          model.autoPomoLeft = 0;
          TickFocusManager.HandleFocusResult(model, true, JsonConvert.DeserializeObject<FocusSaveStatusModel>(LocalSettings.Settings.ExtraSettings.CurrentFocus)?.needSync);
        }
        UtilLog.Info("LoadSavedFocus:" + LocalSettings.Settings.ExtraSettings.CurrentFocus);
      }
      catch (Exception ex)
      {
      }
    }

    public static void SetFocusInitPanel()
    {
      FocusView mainFocus = TickFocusManager.MainFocus;
      TickFocusManager._showClockPanel = mainFocus != null && mainFocus.GetIsShowClockPanel();
    }

    public static async void ClearLocalFocus()
    {
      await Task.Delay(400);
      LocalSettings.Settings.ExtraSettings.CurrentFocus = (string) null;
      LocalSettings.Settings.Save(true);
    }

    public static async void HideOrShowFocusWidget(bool force, int delay = 400)
    {
      await Task.Delay(delay);
      MiniFocusWindow miniFocusWindow = TickFocusManager.MiniFocusWindow;
      if (miniFocusWindow == null)
        TickFocusManager.ShowMiniFocusWindow();
      else if (force || !miniFocusWindow.IsActive)
      {
        miniFocusWindow.Show();
        miniFocusWindow.Activate();
      }
      else
        miniFocusWindow?.Close();
    }

    private static void ShowMiniFocusWindow()
    {
      int num1 = LocalSettings.Settings.PomoWindowLeft;
      int num2 = LocalSettings.Settings.PomoWindowTop;
      if (num1 == -1)
      {
        num1 = (int) (SystemParameters.PrimaryScreenWidth - 280.0 - 40.0);
        num2 = (int) (SystemParameters.PrimaryScreenHeight - 397.0 - 100.0);
      }
      bool flag = LocalSettings.Settings.PomoLocalSetting.DisplayType == "Circle";
      MiniFocusWindow miniFocusWindow1 = new MiniFocusWindow();
      miniFocusWindow1.Left = (double) (num1 - (flag ? 69 : 0));
      miniFocusWindow1.Top = (double) num2;
      MiniFocusWindow miniFocusWindow2 = miniFocusWindow1;
      miniFocusWindow2.Closed += (EventHandler) ((sender, args) => TickFocusManager.MiniFocusWindow = (MiniFocusWindow) null);
      UtilLog.Info("FocusWidgetLocation : " + string.Format("Setting {0},{1},System {2},{3},{4}", (object) LocalSettings.Settings.PomoWindowLeft, (object) LocalSettings.Settings.PomoWindowTop, (object) SystemParameters.WorkArea.Width, (object) SystemParameters.WorkArea.Height, (object) flag));
      TickFocusManager.MiniFocusWindow = miniFocusWindow2;
      TickFocusManager.MiniFocusWindow.TryShowWindow();
    }

    public static async Task<bool> ExitPomo()
    {
      LocalSettings.Settings.ExtraSettings.CurrentFocus = string.Empty;
      await LocalSettings.Settings.Save(true);
      if (!await FocusTimer.StopOrAbandon(true))
        return false;
      TickFocusManager.SaveAfterClose = false;
      TickFocusManager.MiniFocusWindow?.Close();
      return true;
    }

    public static PomoStat PomoStatistics { get; set; }

    public static async void LoadTodayData(bool? isPomo = null, bool loadRemote = false, bool force = true)
    {
      bool flag1 = true;
      if (loadRemote)
      {
        if (!isPomo.HasValue)
        {
          bool flag2 = await PomoSyncService.Pull7Days(true);
          if (!flag2)
            flag2 = await PomoSyncService.Pull7Days(false);
          flag1 = flag2;
        }
        else if (isPomo.Value)
          flag1 = await PomoSyncService.Pull7Days(true);
        else
          flag1 = await PomoSyncService.Pull7Days(false);
      }
      if (!(flag1 | force))
        return;
      TickFocusManager.LoadStatistics();
    }

    public static async void LoadStatistics()
    {
      TickFocusManager.PomoStatistics = await PomoService.LoadStatistics(TickFocusManager.IsPomo);
      TickFocusManager.MiniFocusWindow?.OnStatisticsChanged();
    }

    public static void OnDisplayTypeChanged() => TickFocusManager.MiniFocusWindow?.SetDisplayType();

    public static void ContinueOrPause()
    {
      switch (TickFocusManager.Status)
      {
        case PomoStatus.Working:
          UserActCollectUtils.AddShortCutEvent("focus", "continue_pause");
          FocusTimer.Pause(DateTime.Now);
          break;
        case PomoStatus.Pause:
          UserActCollectUtils.AddShortCutEvent("focus", "continue_pause");
          FocusTimer.Continue(new DateTime?(DateTime.Now));
          break;
      }
    }

    public static void StartOrDrop()
    {
      switch (TickFocusManager.Status)
      {
        case PomoStatus.Working:
        case PomoStatus.Pause:
          UserActCollectUtils.AddShortCutEvent("focus", "start_finish");
          FocusTimer.Drop();
          break;
        case PomoStatus.WaitingWork:
          UserActCollectUtils.AddShortCutEvent("focus", "start_finish");
          FocusTimer.BeginTimer();
          break;
      }
    }

    public static int[] GetStatisticsTypesSafely()
    {
      if (!TickFocusManager.IsPomo)
        return new int[2]{ 0, 2 };
      string miniStatisticsTypes = LocalSettings.Settings.PomoLocalSetting.MiniStatisticsTypes;
      if (!string.IsNullOrEmpty(miniStatisticsTypes))
      {
        string[] strArray = miniStatisticsTypes.Split(',');
        int result1;
        int result2;
        if (strArray.Length == 2 && int.TryParse(strArray[0], out result1) && int.TryParse(strArray[1], out result2) && result1 != result2 && MathUtil.Between(result1, 0, 3, true) && MathUtil.Between(result2, 0, 3, true))
          return new int[2]{ result1, result2 };
      }
      return new int[2]{ 0, 2 };
    }
  }
}
