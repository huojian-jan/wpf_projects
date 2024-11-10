// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusViewModel : BaseViewModel
  {
    private string _title;
    private string _focusId;
    private ProjectIdentity _identity;
    public string ObjId;
    public bool IsHabit;

    public FocusViewModel()
    {
    }

    public FocusViewModel(string focusId) => this._focusId = focusId;

    public bool NoTask => string.IsNullOrEmpty(this._focusId);

    public ProjectIdentity Identity
    {
      get => this._identity ?? ProjectIdentity.CreateSmartIdentity("_special_id_today");
      set
      {
        this._identity = value;
        this.OnPropertyChanged("ProjectName");
        this.OnPropertyChanged("Icon");
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

    public string FocusId => this._focusId;

    public string ProjectName => this.Identity.GetDisplayTitle();

    public Geometry Icon => this.Identity.GetProjectIcon();

    public string TimerId { get; set; }

    public int Type
    {
      get
      {
        if (!string.IsNullOrEmpty(this.TimerId))
          return 2;
        return this.IsHabit ? 1 : 0;
      }
    }

    public async Task SetupTask(string id, int type, string defaultTitle = null)
    {
      FocusViewModel focusViewModel = this;
      if (focusViewModel._focusId == id)
        return;
      await focusViewModel.SetFocusIdAndTitle(id, type, defaultTitle ?? "");
      focusViewModel.OnPropertyChanged("NoTask");
    }

    private async Task SetFocusIdAndTitle(string id, int type, string defaultTitle)
    {
      if (string.IsNullOrEmpty(id))
      {
        this.ObjId = "";
        this.TimerId = "";
        this.Title = "";
        this._focusId = "";
        this.IsHabit = false;
      }
      else
      {
        TimerModel timer = (TimerModel) null;
        HabitModel habit = (HabitModel) null;
        this.ObjId = id;
        this._focusId = id;
        switch (type)
        {
          case 0:
            timer = await TimerDao.GetTimerByObjId(id);
            TaskModel thinTaskById1 = await TaskDao.GetThinTaskById(id);
            this.Title = thinTaskById1 != null ? thinTaskById1.title : timer?.Name ?? defaultTitle;
            break;
          case 1:
            timer = await TimerDao.GetTimerByObjId(id);
            habit = await HabitDao.GetHabitById(id);
            this.Title = habit != null ? habit.Name : timer?.Name ?? defaultTitle;
            break;
          case 2:
            timer = await TimerDao.GetTimerById(id);
            if (timer != null)
            {
              this.TimerId = id;
              this._focusId = id;
              switch (timer.ObjType)
              {
                case "task":
                  TaskModel thinTaskById2 = await TaskDao.GetThinTaskById(timer.ObjId);
                  this.Title = thinTaskById2 != null ? thinTaskById2.title : timer.Name;
                  break;
                case "habit":
                  habit = await HabitDao.GetHabitById(timer.ObjId);
                  this.Title = habit != null ? habit.Name : timer.Name;
                  break;
                default:
                  this.Title = timer.Name;
                  break;
              }
            }
            else
            {
              this.Title = defaultTitle;
              break;
            }
            break;
        }
        this.TimerId = timer?.Id;
        this.IsHabit = habit != null;
        if (timer != null)
        {
          this._focusId = timer.Id;
          this.ObjId = timer.ObjId;
        }
        else
        {
          this._focusId = id;
          this.ObjId = id;
        }
        timer = (TimerModel) null;
        habit = (HabitModel) null;
      }
    }

    public void SetIdentity(ProjectIdentity identity)
    {
      if (identity == null)
        return;
      this.Identity = identity;
    }

    public void SetFocusId(string id) => this._focusId = id;
  }
}
