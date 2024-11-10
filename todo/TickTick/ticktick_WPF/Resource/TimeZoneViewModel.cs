// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Resource.TimeZoneViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Resource
{
  public class TimeZoneViewModel : BaseViewModel
  {
    private bool _selected;

    public TimeZoneInfo TimeZone { get; set; }

    public string DisplayName { get; set; }

    public string TimeZoneName { get; set; }

    public bool IsFloat { get; set; }

    public bool IsSplit { get; set; }

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

    public TimeZoneViewModel()
    {
    }

    public TimeZoneViewModel(TimeZoneInfo tz)
    {
      this.TimeZone = tz;
      try
      {
        this.DisplayName = TimeZoneUtils.GetTimeZoneDisplayName(tz);
        this.TimeZoneName = TimeZoneUtils.GetTimeZoneName(tz);
      }
      catch (Exception ex)
      {
        this.DisplayName = tz?.DisplayName;
        this.TimeZoneName = string.Empty;
      }
      this.IsFloat = false;
    }

    public TimeZoneViewModel(bool isFloat, string tzName)
    {
      this.IsFloat = isFloat;
      if (string.IsNullOrEmpty(tzName))
        return;
      this.TimeZone = TimeZoneUtils.GetTimeZoneInfo(tzName);
      this.DisplayName = this.IsFloat ? Utils.GetString("TimeRemainsUnchanged") : TimeZoneUtils.GetTimeZoneDisplayName(this.TimeZone);
      this.TimeZoneName = tzName;
    }

    public static TimeZoneViewModel GetFloatModel()
    {
      return new TimeZoneViewModel()
      {
        TimeZone = TimeZoneData.LocalTimeZoneModel.TimeZone,
        TimeZoneName = TimeZoneData.LocalTimeZoneModel.TimeZoneName,
        DisplayName = Utils.GetString("TimeRemainsUnchanged"),
        IsFloat = true
      };
    }

    public static string GetShortName(string displayName)
    {
      if (!string.IsNullOrEmpty(displayName))
      {
        int startIndex = displayName.IndexOf("，", StringComparison.Ordinal);
        if (startIndex == -1)
          startIndex = displayName.IndexOf(",", StringComparison.Ordinal);
        if (startIndex > 0)
          return displayName.Remove(startIndex);
      }
      return displayName;
    }
  }
}
