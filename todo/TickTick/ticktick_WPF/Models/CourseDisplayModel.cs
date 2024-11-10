// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CourseDisplayModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class CourseDisplayModel
  {
    public string Title { get; set; }

    public string Id { get; set; }

    public string ScheduleId { get; set; }

    public string Color { get; set; }

    public DateTime CourseStart { get; set; }

    public DateTime CourseEnd { get; set; }

    public string Room { get; set; }

    public string Teacher { get; set; }

    public int StartLesson { get; set; }

    public int EndLesson { get; set; }

    public List<string> Reminders { get; set; }

    public string ScheduleName { get; set; }

    public bool Archived { get; set; }

    public int Index { get; set; }

    public CourseDisplayModel(CourseModel course, DateTime start, DateTime end)
    {
      this.Title = course.Name;
      this.Id = course.Id;
      this.ScheduleId = course.ScheduleId;
      this.Color = string.IsNullOrEmpty(course.Color) ? "#FF4772FA" : course.Color;
      this.CourseStart = start;
      this.CourseEnd = end;
    }

    public static CourseDisplayModel Build(CourseDisplayModel model) => model;

    public string UniqueId
    {
      get
      {
        return this.Id + "_" + DateUtils.GetDateNum(this.CourseStart).ToString() + "_" + this.StartLesson.ToString() + "_" + this.EndLesson.ToString();
      }
    }
  }
}
