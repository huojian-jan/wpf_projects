// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusTimerViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Views.Misc;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusTimerViewModel : SortableListItemViewModel
  {
    private bool _selected;
    private string _title;
    private string _durationText;
    private int _timerStatus;
    private string _icon;
    private string _color;
    private bool _focusing;

    public string ObjId { get; set; }

    public string ObjType { get; set; }

    public string Icon
    {
      get => this._icon;
      set
      {
        this._icon = value;
        this.OnPropertyChanged(nameof (Icon));
      }
    }

    public string Color
    {
      get => this._color;
      set
      {
        this._color = value;
        this.OnPropertyChanged(nameof (Color));
      }
    }

    public string DurationText
    {
      get => this._durationText;
      set
      {
        this._durationText = value;
        this.OnPropertyChanged(nameof (DurationText));
      }
    }

    public string Title
    {
      get => this._title;
      set
      {
        this._title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        if (this._selected == value)
          return;
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public bool Focusing
    {
      get => this._focusing;
      set
      {
        if (this._focusing == value)
          return;
        this._focusing = value;
        this.OnPropertyChanged(nameof (Focusing));
      }
    }

    public bool Archive { get; set; }

    public FocusTimerViewModel(TimerModel timer)
    {
      this.Id = timer.Id;
      this.ObjId = timer.ObjId;
      this.ObjType = timer.ObjType;
      this._title = timer.Name;
      this._icon = timer.Icon;
      this._color = timer.Color;
      this.SortOrder = timer.SortOrder;
      this.Archive = timer.Status == 1;
      this.SetFocusing();
      this.CheckTimerName();
    }

    public async Task CheckTimerName()
    {
      FocusTimerViewModel focusTimerViewModel = this;
      if (string.IsNullOrEmpty(focusTimerViewModel.ObjId))
        return;
      string objTitle = await TimerService.GetObjTitle(focusTimerViewModel.ObjId, focusTimerViewModel.ObjType);
      if (objTitle == null || !(objTitle != focusTimerViewModel.Title))
        return;
      focusTimerViewModel.Title = objTitle;
      TimerService.UpdateName(focusTimerViewModel.Id, objTitle);
    }

    public override void SaveSortOrder()
    {
      TimerService.SaveSortOrder(this.Id, this.SortOrder);
      PomoNotifier.NotifyTimerChanged();
    }

    public void SetFocusing()
    {
      this.Focusing = !this.Archive && TickFocusManager.Working && TickFocusManager.Config.FocusVModel.TimerId == this.Id;
    }
  }
}
