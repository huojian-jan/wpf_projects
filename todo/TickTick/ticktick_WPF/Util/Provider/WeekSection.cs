// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.WeekSection
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class WeekSection : Section
  {
    public WeekSection(
      string name,
      int oridinal,
      DateTime? sectionDate,
      string sectionId,
      string sectionUserId = "")
      : base(name, oridinal, sectionDate, sectionId, sectionUserId)
    {
    }

    public override bool CanSwitch(DisplayType displayType) => displayType != DisplayType.Habit;

    public override bool CanSort(string sortBy) => sortBy == "sortOrder";
  }
}
