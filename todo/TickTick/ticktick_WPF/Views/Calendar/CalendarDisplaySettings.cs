// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarDisplaySettings
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarDisplaySettings
  {
    public CalendarSettingModel MainCal = new CalendarSettingModel();
    public CalendarSettingModel CalWidget = new CalendarSettingModel();

    public void SetHeadSwitch(bool inWidget, int val, int num = 0)
    {
      if (inWidget)
      {
        this.CalWidget.HeadSwitch = val;
        this.CalWidget.MultiNum = num;
      }
      else
      {
        this.MainCal.HeadSwitch = val;
        this.MainCal.MultiNum = num;
      }
    }

    public void SetSideBar(bool inWidget, int val)
    {
      if (inWidget)
        this.CalWidget.SideBar = val;
      else
        this.MainCal.SideBar = val;
    }

    public int GetHeadSwitch(bool inWidget)
    {
      return !inWidget ? this.MainCal.HeadSwitch : this.CalWidget.HeadSwitch;
    }

    public int GetMultiNum(bool inWidget)
    {
      return !inWidget ? this.MainCal.MultiNum : this.CalWidget.MultiNum;
    }

    public string GetHeadMode(bool inWidget)
    {
      return !inWidget ? this.MainCal.GetSwitchMode() : this.CalWidget.GetSwitchMode();
    }

    public int GetSideBar(bool inWidget)
    {
      return !inWidget ? this.MainCal.SideBar : this.CalWidget.SideBar;
    }

    public void CheckNull()
    {
      if (this.MainCal == null)
        this.MainCal = new CalendarSettingModel();
      if (this.CalWidget != null)
        return;
      this.CalWidget = new CalendarSettingModel();
    }
  }
}
