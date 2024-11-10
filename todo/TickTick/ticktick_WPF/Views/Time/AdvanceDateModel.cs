// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.AdvanceDateModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class AdvanceDateModel : UpDownSelectViewModel
  {
    private int _advanceDays;
    private string _displayText;
    private int _hour;
    private int _minute;
    private string _trigger;

    public bool IsSplit { get; set; }

    public AdvanceDateModel()
    {
      this.IsSplit = true;
      this.IsEnable = false;
    }

    public AdvanceDateModel(string rule, bool isAllDay)
    {
      this.Trigger = rule;
      this.IsAllDay = isAllDay;
      if (rule == null)
        return;
      Match match = new Regex("(-)?P((\\d{0,})Y)?((\\d{0,})M)?((\\d{0,})W)?((\\d{0,})D)?T?((\\d{0,})H)?((\\d{0,})M)?((\\d{0,})S)?").Match(rule);
      if (!match.Success)
        return;
      int num = match.Groups[1].ToString() == "-" ? 1 : 0;
      int result1;
      int.TryParse(match.Groups[7].ToString(), out result1);
      int result2;
      int.TryParse(match.Groups[9].ToString(), out result2);
      int result3;
      int.TryParse(match.Groups[11].ToString(), out result3);
      int result4;
      int.TryParse(match.Groups[13].ToString(), out result4);
      if (result1 > 0)
        result2 += result1 * 7;
      this.Order = result3 * 60 + result4;
      if ((num & (isAllDay ? 1 : 0)) != 0)
        ++result2;
      if (num != 0)
      {
        this.Order = (result2 + 2) * 24 * 60 - this.Order;
        result3 *= -1;
        result4 *= -1;
      }
      if (!isAllDay)
        this.Order = result2 * 1440 + (result3 * 60 + result4) * -1;
      DateTime dateTime = DateTime.Today.AddHours((double) result3).AddMinutes((double) result4);
      this.AdvanceDays = result2;
      this.Hour = dateTime.Hour;
      this.Minute = dateTime.Minute;
      this.DisplayText = ReminderUtils.GetReminderText(rule, isAllDay);
    }

    public AdvanceDateModel(bool isEmpty, bool isCustom)
    {
      this.IsCustom = isCustom;
      this.IsEmpty = isEmpty;
      this.DisplayText = this.ToString();
      if (isEmpty)
      {
        this.Order = int.MinValue;
      }
      else
      {
        if (!isCustom)
          return;
        this.Order = int.MaxValue;
      }
    }

    public DateTime Date { get; set; }

    public bool IsAllDay { get; set; }

    public bool IsEmpty { get; set; }

    public bool IsCustom { get; set; }

    public int Order { get; set; }

    public string Trigger
    {
      get => this._trigger;
      set
      {
        this._trigger = value;
        this.OnPropertyChanged(nameof (Trigger));
      }
    }

    public int AdvanceDays
    {
      get => this._advanceDays;
      set
      {
        this._advanceDays = value;
        this.OnPropertyChanged(nameof (AdvanceDays));
      }
    }

    public int Hour
    {
      get => this._hour;
      set
      {
        this._hour = value;
        this.OnPropertyChanged(nameof (Hour));
      }
    }

    public int Minute
    {
      get => this._minute;
      set
      {
        this._minute = value;
        this.OnPropertyChanged(nameof (Minute));
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

    public override string ToString()
    {
      if (this.IsSplit)
        return (string) null;
      if (this.IsEmpty)
        return Utils.GetString("none");
      return this.IsCustom ? Utils.GetString("Custom") : ReminderUtils.GetReminderText(this.Trigger, this.IsAllDay);
    }

    public static List<AdvanceDateModel> BuildAllDayModels(bool withToday)
    {
      List<AdvanceDateModel> advanceDateModelList = new List<AdvanceDateModel>()
      {
        new AdvanceDateModel(true, false) { IsAllDay = true }
      };
      if (withToday)
        advanceDateModelList.Add(new AdvanceDateModel("TRIGGER:P0DT9H0M0S", true));
      List<AdvanceDateModel> collection = new List<AdvanceDateModel>()
      {
        new AdvanceDateModel("TRIGGER:-P0DT15H0M0S", true),
        new AdvanceDateModel("TRIGGER:-P1DT15H0M0S", true),
        new AdvanceDateModel("TRIGGER:-P2DT15H0M0S", true),
        new AdvanceDateModel("TRIGGER:-P6DT15H0M0S", true),
        new AdvanceDateModel() { Order = 2147483637 },
        new AdvanceDateModel(false, true) { IsAllDay = true }
      };
      advanceDateModelList.AddRange((IEnumerable<AdvanceDateModel>) collection);
      return advanceDateModelList;
    }

    public static List<AdvanceDateModel> BuildTimeDayModels()
    {
      return new List<AdvanceDateModel>()
      {
        new AdvanceDateModel(true, false) { IsAllDay = false },
        new AdvanceDateModel("TRIGGER:-PT0S", false),
        new AdvanceDateModel("TRIGGER:-PT5M", false),
        new AdvanceDateModel("TRIGGER:-PT30M", false),
        new AdvanceDateModel("TRIGGER:-PT60M", false),
        new AdvanceDateModel("TRIGGER:-PT1440M", false),
        new AdvanceDateModel() { Order = 2147483637 },
        new AdvanceDateModel(false, true) { IsAllDay = false }
      };
    }

    public string ToRule() => AdvanceDateModel.ToRule(this.AdvanceDays, this.Hour, this.Minute);

    private static string ToRule(int day, int hour, int minute)
    {
      int num1 = day - 1;
      int num2 = 1440 - hour * 60 - minute;
      if (num1 < 0)
        return "TRIGGER:P0DT" + hour.ToString() + "H" + minute.ToString() + "M0S";
      int num3 = num2 / 60;
      int num4 = num2 - num3 * 60;
      if (num1 <= 0 && num3 <= 0 && minute <= 0)
        return string.Empty;
      string rule = "TRIGGER:-P" + num1.ToString() + "D";
      if (num3 > 0 || minute > 0)
        rule = rule + "T" + num3.ToString() + "H" + num4.ToString() + "M0S";
      return rule;
    }
  }
}
