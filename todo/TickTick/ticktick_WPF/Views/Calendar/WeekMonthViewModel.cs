// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.WeekMonthViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class WeekMonthViewModel : BaseViewModel
  {
    public const string Month = "0";
    public const string Week = "1";
    public const string Day = "2";
    private string _mode = "0";
    private string _displayText;

    public string Mode
    {
      get => this._mode;
      set
      {
        this._mode = value;
        this.OnPropertyChanged(nameof (Mode));
        this.SetDisplayText();
      }
    }

    public string DisplayText
    {
      get => this._displayText;
      set
      {
        this._displayText = value;
        this.OnPropertyChanged(nameof (DisplayText));
      }
    }

    private void SetDisplayText()
    {
      string mode = this.Mode;
      bool flag = mode.StartsWith("D");
      if (mode.StartsWith("W") | flag)
      {
        int result;
        if (!int.TryParse(mode.Substring(1), out result))
          return;
        if (flag)
          this.DisplayText = result.ToString() + " " + Utils.GetString(result > 1 ? "PublicDays" : "PublicDay");
        else
          this.DisplayText = result.ToString() + " " + Utils.GetString(result > 1 ? "PublicWeeks" : "PublicWeek");
      }
      else
      {
        switch (mode)
        {
          case "2":
            this.DisplayText = Utils.GetString("Day");
            break;
          case "1":
            this.DisplayText = Utils.GetString("Week");
            break;
          case "0":
            this.DisplayText = Utils.GetString("Month");
            break;
        }
      }
    }
  }
}
