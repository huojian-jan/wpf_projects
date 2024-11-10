// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineExtensions
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public static class TimelineExtensions
  {
    public static string GetName(this TimelineColorType myEnum)
    {
      string str = myEnum.ToString();
      return char.ToLowerInvariant(str[0]).ToString() + str.Substring(1);
    }

    public static bool IsNormalStatus(this TimelineCellOperation myEnum)
    {
      TimelineCellOperation timelineCellOperation = myEnum.Remove(myEnum.GetPos());
      switch (timelineCellOperation)
      {
        case TimelineCellOperation.None:
        case TimelineCellOperation.Hide:
          return true;
        default:
          return timelineCellOperation == TimelineCellOperation.Fold;
      }
    }

    public static bool IsEditingOrMoving(this TimelineCellOperation myEnum)
    {
      if (myEnum.Contain(TimelineCellOperation.Edit))
        return true;
      return myEnum.GetPos() != (TimelineCellOperation) 0 && myEnum.Contain(TimelineCellOperation.Hover);
    }

    public static bool IsMoving(this TimelineCellOperation myEnum)
    {
      return myEnum.GetPos() != (TimelineCellOperation) 0 && myEnum.Contain(TimelineCellOperation.Hover);
    }

    public static bool Contain(this TimelineCellOperation myEnum, TimelineCellOperation otherEnum)
    {
      return (myEnum & otherEnum) == otherEnum;
    }

    public static TimelineCellOperation Add(
      this TimelineCellOperation myEnum,
      TimelineCellOperation other)
    {
      return myEnum | other;
    }

    public static TimelineCellOperation Remove(
      this TimelineCellOperation myEnum,
      TimelineCellOperation other)
    {
      return myEnum & ~other;
    }

    public static TimelineCellOperation RemovePos(this TimelineCellOperation myEnum)
    {
      return myEnum.Remove(TimelineCellOperation.Start).Remove(TimelineCellOperation.Full).Remove(TimelineCellOperation.End);
    }

    public static TimelineCellOperation GetPos(this TimelineCellOperation myEnum)
    {
      return myEnum & (TimelineCellOperation.Start | TimelineCellOperation.Full | TimelineCellOperation.End);
    }

    public static TimelineCellOperation SetPos(
      this TimelineCellOperation myEnum,
      TimelineCellOperation other)
    {
      return myEnum.RemovePos() | other;
    }
  }
}
