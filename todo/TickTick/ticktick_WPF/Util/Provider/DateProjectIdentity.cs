// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.DateProjectIdentity
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class DateProjectIdentity : ProjectIdentity
  {
    public string DateStamp { get; }

    public override string Id => "_special_id_date";

    public override string CatId => this.Id + this.DateStamp;

    public DateTime TargetDate { get; }

    public DateProjectIdentity(string dateStamp)
    {
      this.CanDrag = false;
      this.DateStamp = dateStamp;
      this.TargetDate = DateTime.ParseExact(this.DateStamp, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture);
      this.SortOption = new SortOption()
      {
        groupBy = "none",
        orderBy = "dueDate"
      };
    }

    public override TimeData GetTimeData()
    {
      TimeData timeData = TimeData.InitDefaultTime();
      timeData.StartDate = new DateTime?(this.TargetDate);
      return timeData;
    }

    public override string GetDisplayTitle()
    {
      switch ((this.TargetDate - DateTime.Today).Days)
      {
        case 0:
          return Utils.GetString("Today");
        case 1:
          return Utils.GetString("Tomorrow");
        default:
          return this.TargetDate.ToString("yyyy-MM-dd");
      }
    }
  }
}
