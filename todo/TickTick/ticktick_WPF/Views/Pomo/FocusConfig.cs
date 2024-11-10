// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusConfig
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using TickTickDao;
using TickTickModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusConfig
  {
    private const int MinToSecFactor = 60;
    public DateTime LastPomoCompleteTime;
    public int PomoCount;
    public long Second = 60;
    private PomoStatus _previewStatus;
    private PomoStatus _status = PomoStatus.WaitingWork;
    private readonly Action _statusChange;
    private double _currentSeconds;
    private readonly Action _secondChange;
    public DateTime PauseTime;
    public DateTime? AutoStopTime;
    private int? _pomoDuration;

    private static bool AutoBreak => LocalSettings.Settings.AutoBreak;

    private static bool AutoNextPomo => LocalSettings.Settings.AutoNextPomo;

    public FocusConfig(Action secondChange, Action statusChange)
    {
      this._secondChange = secondChange;
      this._statusChange = statusChange;
      this.Type = !(LocalSettings.Settings.PomoType == FocusConstance.Focus) ? 1 : 0;
      this.SetPomoSeconds();
    }

    public bool IsWorking => this.Status == PomoStatus.Working || this.Status == PomoStatus.Pause;

    public bool InRelax
    {
      get => this.Status == PomoStatus.WaitingRelax || this.Status == PomoStatus.Relaxing;
    }

    public string Id { get; set; }

    public int Type { get; set; }

    public FocusViewModel FocusVModel { get; set; } = new FocusViewModel();

    public FocusPieceHandler PieceHandler { get; set; } = new FocusPieceHandler();

    public string FirstFocusId { get; set; }

    public DateTime StartTime { get; set; }

    public double PauseDuration { get; set; }

    public string Note { get; set; }

    public int AutoTimes { get; set; }

    public PomoStatus Status
    {
      get => this._status;
      set
      {
        this._previewStatus = this._status;
        if (this._status == value)
          return;
        this._status = value;
        this._statusChange();
        this.OnStatusChanged();
      }
    }

    private void OnStatusChanged()
    {
      if (this.Status == PomoStatus.WaitingRelax)
        DelayActionHandlerCenter.TryDoAction("StartWaitingRelax", (EventHandler) ((sender, args) =>
        {
          this.PomoCount = 0;
          this.Status = PomoStatus.WaitingWork;
        }), 10800000);
      else
        DelayActionHandlerCenter.RemoveAction("StartWaitingRelax");
      switch (this.Status)
      {
        case PomoStatus.WaitingWork:
          this.SetStartSeconds();
          App.RefreshIconMenu();
          JumpHelper.InitJumpList();
          break;
        case PomoStatus.WaitingRelax:
          this.AutoStopTime = new DateTime?(DateTime.Now);
          this.SetRelaxSeconds();
          App.RefreshIconMenu();
          JumpHelper.InitJumpList();
          break;
      }
      if ((this.Status != PomoStatus.Working || LocalSettings.Settings.AutoNextPomo) && (this.Status != PomoStatus.Relaxing || LocalSettings.Settings.AutoBreak))
        return;
      TickFocusManager.HidePomoReminder();
    }

    public double CurrentSeconds
    {
      get => this._currentSeconds;
      set
      {
        this._currentSeconds = value;
        this._secondChange();
      }
    }

    public bool FromRelax
    {
      get
      {
        return this._previewStatus == PomoStatus.Relaxing || this._previewStatus == PomoStatus.WaitingRelax;
      }
    }

    public bool NeedUpload { get; set; }

    public void StartFocus(DateTime now, bool auto = false)
    {
      this.Id = Utils.GetGuid();
      this.NeedUpload = LocalSettings.Settings.FocusKeepInSync;
      this.AutoStopTime = new DateTime?();
      this.Note = string.Empty;
      this.PieceHandler.Reset(now);
      this.PieceHandler.AddPiece(this.FocusVModel, new DateTime?(now));
      this.PauseDuration = 0.0;
      this.StartTime = now;
      if (auto)
        auto = LocalSettings.Settings.AutoBreak && LocalSettings.Settings.AutoNextPomo;
      if (!auto && this.Type == 0)
        this.AutoTimes = LocalSettings.Settings.AutoPomoTimes;
      if (this.Type == 1 || !auto || string.IsNullOrEmpty(this.FirstFocusId) || !LocalSettings.Settings.FocusKeepInSync)
        this.FirstFocusId = this.Id;
      if (this.Type == 0)
      {
        if ((now - this.LastPomoCompleteTime).TotalMinutes > 60.0)
          this.PomoCount = 0;
        if (LocalSettings.Settings.LongBreakEvery >= 1 && this.PomoCount >= 1 && this.PomoCount % LocalSettings.Settings.LongBreakEvery == 0)
          this.PomoCount = 0;
        --this.AutoTimes;
      }
      else
        this.PomoCount = 0;
      ++this.PomoCount;
    }

    public void SetRelaxSeconds()
    {
      this.Second = this.GetRelaxMinutes() * 60L;
      this.CurrentSeconds = (double) this.Second;
    }

    public long GetRelaxMinutes()
    {
      bool flag = LocalSettings.Settings.LongBreakEvery >= 1 && this.PomoCount >= 1 && this.PomoCount % LocalSettings.Settings.LongBreakEvery != 0;
      if ((DateTime.Now - this.LastPomoCompleteTime).TotalMinutes > 60.0)
        flag = true;
      return flag ? (long) LocalSettings.Settings.ShortBreakDuration : (long) LocalSettings.Settings.LongBreakDuration;
    }

    public void SetPomoSeconds(int? dur = null)
    {
      this._pomoDuration = dur;
      this.SetStartSeconds();
    }

    public void SetStartSeconds()
    {
      if (this.Type == 0)
      {
        int val2 = this._pomoDuration ?? LocalSettings.Settings.PomoDuration;
        this.Second = (long) ((val2 == 0 ? 25 : Math.Max(5, val2)) * 60);
        this.CurrentSeconds = (double) this.Second;
      }
      else
      {
        this.Second = 0L;
        this.CurrentSeconds = 0.0;
      }
    }

    public void OnPause(DateTime time)
    {
      this.PieceHandler.AddPiece(new FocusPieceModel()
      {
        Id = "StartPause",
        BeginTime = time
      });
      double num = (time - this.StartTime).TotalSeconds - this.PauseDuration;
      this.PauseTime = time;
      this.CurrentSeconds = this.Type == 0 ? (double) this.Second - num : num;
    }

    public void OnContinue(DateTime time)
    {
      this.PauseDuration += (time - this.PauseTime).TotalSeconds;
    }

    public void Reset(bool resetId = true)
    {
      if (resetId)
        this.FocusVModel.SetupTask(string.Empty, 0);
      this.AutoTimes = 0;
      this.PomoCount = 0;
      this.AutoStopTime = new DateTime?();
      this._previewStatus = PomoStatus.WaitingWork;
      this._status = PomoStatus.WaitingWork;
      this._statusChange();
      this.SetPomoSeconds();
      this.OnStatusChanged();
    }

    public async void OnTaskSelected(FocusPieceModel pieceModel)
    {
      DateTime now = DateTime.Now;
      if (pieceModel == null || string.IsNullOrEmpty(pieceModel.Id))
      {
        if (this.Status == PomoStatus.Working)
          this.PieceHandler.AddPiece(new FocusPieceModel()
          {
            BeginTime = DateTime.Now
          });
        if (!this.IsWorking)
          return;
        FocusOptionUploader.AddOption(FocusOption.focus, now, true);
      }
      else
      {
        if (!this.IsWorking)
          return;
        (FocusPieceModel, bool) lastRealPiece = this.PieceHandler.GetLastRealPiece();
        FocusPieceModel last = lastRealPiece.Item1;
        int num = lastRealPiece.Item2 ? 1 : 0;
        DateTime time = now;
        pieceModel.BeginTime = time;
        bool flag = last != null;
        if (num != 0 && last != null && string.IsNullOrEmpty(last.Id))
        {
          List<Inline> inlines = new List<Inline>();
          List<Inline> inlineList1 = inlines;
          Run run1 = new Run(Utils.GetString("SwitchPomoTaskTip"));
          run1.Foreground = (Brush) ThemeUtil.GetColor("BaseColorOpacity100_80");
          inlineList1.Add((Inline) run1);
          string name = string.Empty;
          TimerModel timer = await TimerDao.GetTimerByIdOrObjId(pieceModel.Id);
          if (pieceModel.Type == 1 || timer?.ObjType == "habit")
          {
            HabitModel habitById = await HabitDao.GetHabitById(timer?.ObjId ?? pieceModel.Id);
            if (habitById != null)
              name = habitById.Name;
          }
          else
          {
            TaskModel thinTaskById = await TaskDao.GetThinTaskById(timer?.ObjId ?? pieceModel.Id);
            if (thinTaskById != null)
              name = thinTaskById.title;
          }
          if (string.IsNullOrEmpty(name) && timer != null && !timer.Deleted)
            name = timer.Name;
          List<Inline> inlineList2 = inlines;
          Run run2 = new Run(" " + name + " ");
          run2.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
          inlineList2.Add((Inline) run2);
          List<Inline> inlineList3 = inlines;
          Run run3 = new Run(Utils.IsCn() ? "？" : "?");
          run3.Foreground = (Brush) ThemeUtil.GetColor("BaseColorOpacity100_80");
          inlineList3.Add((Inline) run3);
          bool? nullable = new CustomerDialog(string.Empty, inlines, Utils.GetString("BelongTo"), Utils.GetString("NotBelongTo")).ShowDialog();
          flag = nullable.GetValueOrDefault();
          if (nullable.GetValueOrDefault())
            time = last.BeginTime;
          inlines = (List<Inline>) null;
          name = (string) null;
          timer = (TimerModel) null;
        }
        if (flag)
        {
          last.Id = pieceModel.Id;
          last.Type = pieceModel.Type;
        }
        else if (this.Status == PomoStatus.Working)
          this.PieceHandler.AddPiece(pieceModel);
        FocusOptionUploader.AddOption(FocusOption.focus, time, true);
        last = (FocusPieceModel) null;
      }
    }

    public async Task SetFocusId(string focusId, int type = 0)
    {
      FocusConfig focusConfig = this;
      await focusConfig.FocusVModel.SetupTask(focusId, type);
      focusConfig.OnTaskSelected(new FocusPieceModel()
      {
        Id = focusId,
        Type = type
      });
    }

    public void TrySetSeconds()
    {
      switch (this.Status)
      {
        case PomoStatus.WaitingWork:
          this.SetPomoSeconds();
          break;
        case PomoStatus.WaitingRelax:
          this.SetRelaxSeconds();
          break;
      }
    }

    public double GetDisplayPercent()
    {
      return this.Type == 0 ? ((double) this.Second - this.CurrentSeconds) / (double) this.Second : this.CurrentSeconds % 60.0 / 60.0;
    }

    public double GetDisplayAngle(bool next)
    {
      return this.Type == 0 ? ((double) this.Second - this.CurrentSeconds + (double) (next ? 1 : 0)) / (double) this.Second * 360.0 : (this.CurrentSeconds + (double) (next ? 1 : 0)) % 60.0 * 6.0;
    }

    public void AdjustDuration(bool isAdd)
    {
      if (this.Type != 0 || !this.IsWorking)
        return;
      long num1 = this.Second + (isAdd ? 300L : -300L);
      double num2 = (DateTime.Now - this.StartTime).TotalSeconds - this.PauseDuration;
      double num3 = this.Status == PomoStatus.Pause ? this.CurrentSeconds : (double) num1 - num2;
      if (num3 <= 0.0 || num1 < 300L)
      {
        FocusTimer.TryAbandonPomo();
      }
      else
      {
        if (num1 > 10800L)
          return;
        this.Second = num1;
        this.CurrentSeconds = num3;
        FocusOptionUploader.AddOption(FocusOption.changeDuration, DateTime.Now, true);
      }
    }

    public List<(DateTime, DateTime)> GetPauseSpans() => this.PieceHandler.GetPauseSpans();

    public void SetNote(object sender, string note)
    {
      if (this.IsWorking)
      {
        this.Note = note;
        PomoNotifier.NotifyFocusNoteChanged(sender);
      }
      else
      {
        if (!this.AutoStopTime.HasValue)
          return;
        this.Note = note;
        PomoService.SaveNote(this.Id, note);
      }
    }

    public string ToSaveSyncModel()
    {
      if (this.Status != PomoStatus.Working && this.Status != PomoStatus.Pause)
        return (string) null;
      FocusSaveStatusModel focusSaveStatusModel1 = new FocusSaveStatusModel();
      focusSaveStatusModel1.id = this.Id;
      focusSaveStatusModel1.startTime = this.StartTime;
      focusSaveStatusModel1.endTime = new DateTime?();
      focusSaveStatusModel1.duration = this.Type != 0 || !this.IsWorking ? 0L : this.Second / 60L;
      focusSaveStatusModel1.status = this.Status == PomoStatus.Working ? 0 : (this.Status == PomoStatus.Pause ? 1 : 2);
      focusSaveStatusModel1.type = this.Type;
      focusSaveStatusModel1.valid = true;
      focusSaveStatusModel1.exited = false;
      focusSaveStatusModel1.firstId = this.FirstFocusId;
      focusSaveStatusModel1.autoPomoLeft = this.AutoTimes;
      focusSaveStatusModel1.pomoCount = this.PomoCount;
      focusSaveStatusModel1.needSync = this.NeedUpload;
      FocusSaveStatusModel focusSaveStatusModel2 = focusSaveStatusModel1;
      FocusBreakModel focusBreakModel = new FocusBreakModel();
      if (this.Status == PomoStatus.WaitingWork)
        focusBreakModel.endTime = new DateTime?(DateTime.Now);
      if (this.Status == PomoStatus.Relaxing)
      {
        focusBreakModel.startTime = new DateTime?(FocusTimer.TimerStartTime);
        focusBreakModel.duration = new long?(this.Second / 60L);
      }
      focusSaveStatusModel2.focusBreak = focusBreakModel;
      List<FocusLogModel> source = new List<FocusLogModel>();
      List<FocusLogModel> focusLogModelList1 = new List<FocusLogModel>();
      foreach (FocusPieceModel piece in this.PieceHandler.GetPieces())
      {
        if (piece.Id != "StartPause")
        {
          FocusLogModel focusLogModel = source.LastOrDefault<FocusLogModel>();
          if (focusLogModel == null || focusLogModel.id != piece.Id)
            source.Add(new FocusLogModel()
            {
              type = new int?(piece.Type),
              id = piece.Id,
              time = piece.BeginTime
            });
        }
        else
        {
          focusLogModelList1.Add(new FocusLogModel()
          {
            type = new int?(0),
            time = piece.BeginTime
          });
          DateTime? endTime = piece.EndTime;
          if (endTime.HasValue)
          {
            List<FocusLogModel> focusLogModelList2 = focusLogModelList1;
            FocusLogModel focusLogModel = new FocusLogModel();
            focusLogModel.type = new int?(1);
            endTime = piece.EndTime;
            focusLogModel.time = endTime.Value;
            focusLogModelList2.Add(focusLogModel);
          }
        }
      }
      focusSaveStatusModel2.focusOnLogs = source.ToArray();
      focusSaveStatusModel2.pauseLogs = focusLogModelList1.ToArray();
      return JsonConvert.SerializeObject((object) focusSaveStatusModel2);
    }
  }
}
