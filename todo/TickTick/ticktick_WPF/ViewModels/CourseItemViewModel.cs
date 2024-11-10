// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.CourseItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class CourseItemViewModel : BaseViewModel
  {
    public string WeekText { get; set; }

    public string LessonText { get; set; }

    public string Room { get; set; }

    public string Teacher { get; set; }

    public string TimeTitle { get; set; }

    public Visibility ShowTimeTitle
    {
      get => string.IsNullOrEmpty(this.TimeTitle) ? Visibility.Collapsed : Visibility.Visible;
    }

    public Visibility ShowRoom
    {
      get => string.IsNullOrEmpty(this.Room) ? Visibility.Collapsed : Visibility.Visible;
    }

    public Visibility ShowTeacher
    {
      get => string.IsNullOrEmpty(this.Teacher) ? Visibility.Collapsed : Visibility.Visible;
    }

    public CourseItemViewModel(CourseDetailModel item, int index)
    {
      this.Room = item.Room;
      this.Teacher = item.Teacher;
      this.TimeTitle = index > 0 ? string.Format(Utils.GetString("CourseTime"), (object) index) : (string) null;
      this.LessonText = DateUtils.GetWeekTextByWeekDay(item.Weekday) + ", " + string.Format(Utils.GetString("LessonNum"), item.StartLesson == item.EndLesson ? (object) (item.StartLesson.ToString() ?? "") : (object) (item.StartLesson.ToString() + " - " + item.EndLesson.ToString()));
      this.WeekText = CourseItemViewModel.GetWeekText(item.Weeks);
    }

    private static string GetWeekText(int[] weeks)
    {
      if (weeks == null || weeks.Length == 0)
        return (string) null;
      if (weeks.Length == 1)
        return string.Format(Utils.GetString("InWeekNum"), (object) weeks[0]);
      List<int> list = ((IEnumerable<int>) weeks).OrderBy<int, int>((Func<int, int>) (week => week)).ToList<int>();
      int num1 = list.Last<int>();
      int num2 = list.First<int>();
      if (num1 - num2 + 1 == weeks.Length)
        return string.Format(Utils.GetString("InWeekNum"), (object) (num2.ToString() + " - " + num1.ToString()));
      bool flag = true;
      for (int index = 0; index < list.Count - 1; ++index)
      {
        if (list[index + 1] - list[index] != 2)
        {
          flag = false;
          break;
        }
      }
      if (flag)
        return string.Format(Utils.GetString("InWeekNum"), (object) (num2.ToString() + " - " + num1.ToString())) + " (" + Utils.GetString(list[0] % 2 == 1 ? "Odd" : "Even") + ")";
      int num3 = -1;
      int num4 = -1;
      string weekText = string.Empty;
      foreach (int num5 in list)
      {
        if (num3 == -1)
        {
          num3 = num5;
          num4 = num5;
        }
        else if (num4 + 1 == num5)
        {
          num4 = num5;
        }
        else
        {
          weekText = weekText + (string.IsNullOrEmpty(weekText) ? "" : ", ") + string.Format(Utils.GetString("InWeekNum"), num3 == num4 ? (object) (num3.ToString() + string.Empty) : (object) (num3.ToString() + " - " + num4.ToString()));
          num3 = num5;
          num4 = num5;
        }
      }
      if (num3 != -1)
        weekText = weekText + (string.IsNullOrEmpty(weekText) ? "" : ", ") + string.Format(Utils.GetString("InWeekNum"), num3 == num4 ? (object) (num3.ToString() + string.Empty) : (object) (num3.ToString() + " - " + num4.ToString()));
      return weekText;
    }
  }
}
