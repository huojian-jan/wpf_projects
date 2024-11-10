// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CalendarDisplayModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class CalendarDisplayModel
  {
    public double repeatDiff;
    public bool IsVirtual;
    public CourseDisplayModel CourseDisplayModel;
    private static List<CalendarDisplayModel> visualModels;

    public TaskBaseViewModel SourceViewModel { get; set; }

    public DateTime? StartDate => this.SourceViewModel.StartDate;

    public DateTime? DueDate => this.SourceViewModel.DueDate;

    public DateTime? DisplayStartDate
    {
      get
      {
        DateTime? startDate = this.SourceViewModel.StartDate;
        ref DateTime? local = ref startDate;
        return !local.HasValue ? new DateTime?() : new DateTime?(local.GetValueOrDefault().AddDays(this.repeatDiff));
      }
    }

    public DateTime? DisplayDueDate
    {
      get
      {
        DateTime? dueDate = this.SourceViewModel.DueDate;
        ref DateTime? local = ref dueDate;
        return !local.HasValue ? new DateTime?() : new DateTime?(local.GetValueOrDefault().AddDays(this.repeatDiff));
      }
    }

    public bool? IsAllDay => this.SourceViewModel.IsAllDay;

    public string Title => this.SourceViewModel.Title;

    public DateTime? CompletedTime => this.SourceViewModel.CompletedTime;

    public int Status => this.SourceViewModel.Status;

    public int Priority => this.SourceViewModel.Priority;

    public DisplayType Type => this.SourceViewModel.Type;

    public long SortOrder => this.SourceViewModel.SortOrder;

    public string ProjectId => this.SourceViewModel.ProjectId;

    public string Id => this.SourceViewModel.Id;

    private CalendarDisplayModel()
    {
    }

    public CalendarDisplayModel(
      string title,
      string id,
      string color,
      DateTime startDate,
      DateTime? endTime,
      bool isAllDay,
      int status)
    {
      this.IsVirtual = true;
      this.SourceViewModel = new TaskBaseViewModel()
      {
        Title = title,
        Id = id,
        StartDate = new DateTime?(startDate),
        DueDate = endTime,
        Color = color,
        IsAllDay = new bool?(isAllDay),
        Status = status,
        CompletedTime = status == 0 ? new DateTime?() : new DateTime?(startDate)
      };
    }

    public CalendarDisplayModel(PomodoroModel pomo)
    {
      if (pomo == null)
        return;
      TaskBaseViewModel displayTask = (TaskBaseViewModel) null;
      string id = pomo.Id;
      string str1;
      if (pomo.Tasks == null || pomo.Tasks.Length == 0)
        str1 = Utils.GetString("FocusTime");
      else if (pomo.Tasks.Length == 1)
      {
        string title = pomo.Tasks[0].GetTitle();
        str1 = string.IsNullOrEmpty(title) ? Utils.GetString("FocusTime") : title;
        displayTask = TaskCache.GetTaskById(pomo.Tasks[0].TaskId);
      }
      else
      {
        Dictionary<string, TimeSpan> dictionary = new Dictionary<string, TimeSpan>();
        TimeSpan minValue = TimeSpan.MinValue;
        string str2 = Utils.GetString("FocusTime") + "@";
        string taskId = string.Empty;
        foreach (PomoTask task in pomo.Tasks)
        {
          string empty = string.Empty;
          string key;
          if (!string.IsNullOrEmpty(task.TaskId))
            key = task.GetTitle() + "@" + task.TaskId;
          else if (!string.IsNullOrEmpty(task.HabitId))
          {
            key = task.GetTitle() + "@" + task.HabitId;
          }
          else
          {
            string title = task.GetTitle();
            key = string.IsNullOrEmpty(title) ? Utils.GetString("FocusTime") : title + "@";
          }
          TimeSpan timeSpan = task.EndTime - task.StartTime;
          if (dictionary.ContainsKey(key))
            dictionary[key] += timeSpan;
          else
            dictionary[key] = timeSpan;
          if (dictionary[key] > minValue)
          {
            minValue = dictionary[key];
            str2 = key;
            taskId = task.TaskId;
          }
        }
        str1 = ((IEnumerable<string>) str2.Split('@')).FirstOrDefault<string>() + "...";
        displayTask = TaskCache.GetTaskById(taskId);
      }
      this.SourceViewModel = new TaskBaseViewModel()
      {
        Type = DisplayType.Pomo,
        Id = id,
        Title = str1,
        IsAllDay = new bool?(false),
        StartDate = new DateTime?(pomo.StartTime),
        DueDate = new DateTime?(pomo.EndTime),
        CompletedTime = new DateTime?(pomo.EndTime),
        Color = displayTask?.Color,
        Tag = displayTask?.Tag,
        Priority = displayTask != null ? displayTask.Priority : 0
      };
      this.SourceViewModel.SetDependenceModel(displayTask, "Color", "Tag", nameof (Priority));
    }

    public CalendarDisplayModel(CourseDisplayModel courseDisplayModel)
    {
      this.CourseDisplayModel = courseDisplayModel;
      TaskBaseViewModel taskBaseViewModel = new TaskBaseViewModel();
      taskBaseViewModel.Title = courseDisplayModel.Title;
      taskBaseViewModel.Id = courseDisplayModel.Id;
      taskBaseViewModel.StartDate = new DateTime?(courseDisplayModel.CourseStart);
      taskBaseViewModel.DueDate = courseDisplayModel.CourseStart < courseDisplayModel.CourseEnd ? new DateTime?(courseDisplayModel.CourseEnd) : new DateTime?();
      taskBaseViewModel.Color = courseDisplayModel.Color;
      taskBaseViewModel.IsAllDay = new bool?(false);
      DateTime courseStart;
      int num;
      if (!courseDisplayModel.Archived)
      {
        courseStart = this.CourseDisplayModel.CourseStart;
        if (!(courseStart.Date < DateTime.Today))
        {
          num = 0;
          goto label_4;
        }
      }
      num = 2;
label_4:
      taskBaseViewModel.Status = num;
      courseStart = this.CourseDisplayModel.CourseStart;
      taskBaseViewModel.CompletedTime = courseStart.Date < DateTime.Today ? new DateTime?() : new DateTime?(courseDisplayModel.CourseStart);
      taskBaseViewModel.Type = DisplayType.Course;
      this.SourceViewModel = taskBaseViewModel;
    }

    public static CalendarDisplayModel Build(
      HabitModel habit,
      int status,
      DateTime startDate,
      DateTime? completeTime)
    {
      bool flag = true;
      if (startDate == DateTime.Today)
      {
        string[] reminders = habit.Reminders;
        if ((reminders != null ? (reminders.Length != 0 ? 1 : 0) : 0) != 0)
        {
          List<string> list = ((IEnumerable<string>) habit.Reminders).ToList<string>();
          list.Sort();
          string str = list.FirstOrDefault<string>((Func<string, bool>) (r => string.Compare(r, DateTime.Now.ToString("HH:mm"), StringComparison.Ordinal) >= 0)) ?? list.LastOrDefault<string>();
          int result1;
          int result2;
          if (str != null && int.TryParse(str.Substring(0, 2), out result1) && int.TryParse(str.Substring(3), out result2))
          {
            startDate = startDate.AddHours((double) result1).AddMinutes((double) result2);
            flag = false;
          }
        }
      }
      CalendarDisplayModel calendarDisplayModel = new CalendarDisplayModel();
      HabitBaseViewModel habitBaseViewModel = new HabitBaseViewModel(habit);
      habitBaseViewModel.Status = status;
      habitBaseViewModel.StartDate = new DateTime?(startDate);
      habitBaseViewModel.CompletedTime = completeTime;
      habitBaseViewModel.IsAllDay = new bool?(flag);
      habitBaseViewModel.SortOrder = habit.SortOrder;
      calendarDisplayModel.SourceViewModel = (TaskBaseViewModel) habitBaseViewModel;
      return calendarDisplayModel;
    }

    public CalendarDisplayModel(CalendarEventModel calendarEvent, int? status = null, string color = null)
    {
      TaskBaseViewModel taskBaseViewModel = new TaskBaseViewModel(calendarEvent);
      taskBaseViewModel.StartDate = CalendarDisplayModel.GetDateValue(calendarEvent.DueStart, calendarEvent.IsAllDay);
      taskBaseViewModel.DueDate = CalendarDisplayModel.GetEndDateValue(calendarEvent.DueStart, calendarEvent.DueEnd, calendarEvent.IsAllDay);
      int? nullable1 = status;
      int num;
      if (!nullable1.HasValue)
      {
        DateTime? nullable2 = calendarEvent.DueStart;
        if (nullable2.HasValue)
        {
          nullable2 = calendarEvent.DueStart;
          if (nullable2.Value < DateTime.Today)
          {
            nullable2 = calendarEvent.DueEnd;
            if (nullable2.HasValue)
            {
              nullable2 = calendarEvent.DueEnd;
              if (nullable2.Value <= DateTime.Today)
              {
                num = 2;
                goto label_8;
              }
            }
          }
        }
        num = calendarEvent.Status;
      }
      else
        num = nullable1.GetValueOrDefault();
label_8:
      taskBaseViewModel.Status = num;
      taskBaseViewModel.Color = color;
      this.SourceViewModel = taskBaseViewModel;
    }

    private static DateTime? GetEndDateValue(DateTime? startDate, DateTime? endDate, bool isAllDay)
    {
      return isAllDay && startDate.HasValue && endDate.HasValue && (endDate.Value - startDate.Value).Days == 1 ? new DateTime?() : CalendarDisplayModel.GetDateValue(endDate, isAllDay);
    }

    private static DateTime? GetDateValue(DateTime? date, bool isAllDay)
    {
      if (!date.HasValue)
        return new DateTime?();
      return isAllDay ? new DateTime?(date.Value.Date) : date;
    }

    public CalendarDisplayModel(TaskBaseViewModel model) => this.SourceViewModel = model;

    private static int GetTypeByTaskType(int taskType, string taskKind)
    {
      switch (taskType)
      {
        case 0:
          switch (taskKind)
          {
            case "TEXT":
            case "CHECKLIST":
              return 0;
            case "NOTE":
              return 5;
          }
          break;
        case 1:
          return 1;
        case 2:
          return 3;
        case 3:
          return 4;
      }
      return 0;
    }

    internal static List<CalendarDisplayModel> GetVirtualModels(
      DateTime startDate,
      DateTime dueDate)
    {
      if (CalendarDisplayModel.visualModels == null)
        CalendarDisplayModel.visualModels = new List<CalendarDisplayModel>()
        {
          new CalendarDisplayModel("制作ppt", "0", "#52B8D2", startDate.AddDays(1.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("月度计划", "1", "#86C0EF", startDate.AddDays(1.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("选题会", "2", "#52B8D2", startDate.AddDays(1.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("ifs探店", "3", "#FFE599", startDate.AddDays(2.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("打电话给Sandy", "4", "#F18181", startDate.AddDays(2.0).AddHours(15.0).AddMinutes(30.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("审美的乌托邦：俄国文学100讲", "5", "#5DD1A8", startDate.AddDays(3.0), new DateTime?(startDate.AddDays(6.0)), true, 2),
          new CalendarDisplayModel("瑜伽课", "6", "#5DD1A8", startDate.AddDays(3.0).AddHours(18.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("旅行计划", "7", "#93C47D", startDate.AddDays(3.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("整理衣柜", "8", "#D1D7A8", startDate.AddDays(4.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("买狗粮", "9", "#D1D7A8", startDate.AddDays(4.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("实习生面试", "10", "#86C0EF", startDate.AddDays(5.0).AddHours(14.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("苏州旅行", "11", "#52B8D2", startDate.AddDays(6.0), new DateTime?(startDate.AddDays(8.0)), true, 2),
          new CalendarDisplayModel("写影评", "12", "#F9CB9C", startDate.AddDays(7.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("vlog剪辑", "13", "#F9CB9C", startDate.AddDays(7.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("功能评审会议", "14", "#86C0EF", startDate.AddDays(8.0).AddHours(16.0).AddMinutes(30.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("浇花", "15", "#52B8D2", startDate.AddDays(8.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("晨间日记", "16", "#52B8D2", startDate.AddDays(8.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("买礼物", "17", "#D1D7A8", startDate.AddDays(9.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("超市采购", "18", "#D1D7A8", startDate.AddDays(9.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("瑜伽课", "19", "#5DD1A8", startDate.AddDays(9.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("需求会议", "20", "#A4AFC7", startDate.AddDays(9.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("学习Python", "21", "#F9CB9C", startDate.AddDays(10.0), new DateTime?(startDate.AddDays(14.0)), true, 2),
          new CalendarDisplayModel("戏剧新生活", "22", "#52B8D2", startDate.AddDays(10.0).AddHours(12.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("今日工作复盘", "23", "#D1D7A8", startDate.AddDays(10.0).AddHours(18.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("新闻通稿", "24", "#F9CB9C", startDate.AddDays(11.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("问候父母", "25", "#F18181", startDate.AddDays(11.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("月度需求会议", "26", "#A4AFC7", startDate.AddDays(11.0).AddHours(17.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("理发", "27", "#D1D7A8", startDate.AddDays(12.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("旅游规划", "28", "#93C47D", startDate.AddDays(12.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("买礼物", "29", "#D1D7A8", startDate.AddDays(12.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("汗蒸", "30", "#B6D7A8", startDate.AddDays(13.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("周复盘", "31", "#D1D7A8", startDate.AddDays(13.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("练琴", "32", "#F9CB9C", startDate.AddDays(13.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("预定餐厅", "33", "#F2B04B", startDate.AddDays(13.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("公园野餐", "34", "#FFE599", startDate.AddDays(14.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("看话剧", "35", "#F9CB9C", startDate.AddDays(14.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("耳边风：马世芳的音乐时光机", "36", "#5DD1A8", startDate.AddDays(15.0).AddHours(11.0), new DateTime?(startDate.AddDays(20.0)), false, 2),
          new CalendarDisplayModel("交电费", "37", "#D1D7A8", startDate.AddDays(15.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("交房租", "38", "#D1D7A8", startDate.AddDays(16.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("新功能宣传", "39", "#A4AFC7", startDate.AddDays(17.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("lily生日", "40", "#F18181", startDate.AddDays(17.0).AddHours(19.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("H5游戏策划", "41", "#F9CB9C", startDate.AddDays(18.0).AddHours(9.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("佳佳生日", "42", "#F18181", startDate.AddDays(18.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("拿快递", "43", "#D1D7A8", startDate.AddDays(19.0).AddHours(20.0).AddMinutes(0.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("视频脚本", "44", "#B6D7A8", startDate.AddDays(19.0).AddHours(16.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("部门培训", "45", "#86C0EF", startDate.AddDays(19.0).AddHours(13.0).AddMinutes(30.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("吃饭", "46", "#86C0EF", startDate.AddDays(20.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("买洗发水", "47", "#D1D7A8", startDate.AddDays(20.0).AddHours(19.0).AddMinutes(45.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("周复盘", "48", "#D1D7A8", startDate.AddDays(20.0).AddHours(20.0).AddMinutes(15.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("演唱会", "49", "#FFE599", startDate.AddDays(20.0).AddHours(14.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("艺术集市", "50", "#FFE599", startDate.AddDays(21.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("吃饭", "51", "#86C0EF", startDate.AddDays(21.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("吃饭", "52", "#86C0EF", startDate.AddDays(22.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("生日会", "53", "#F2B04B", startDate.AddDays(22.0).AddHours(15.0).AddMinutes(0.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("读《夜晚的潜水艇》", "54", "#52B8D2", startDate.AddDays(23.0).AddHours(15.0).AddMinutes(0.0), new DateTime?(startDate.AddDays(27.0)), false, 0),
          new CalendarDisplayModel("瑜伽课", "55", "#5DD1A8", startDate.AddDays(23.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("部门例会", "56", "#52B8D2", startDate.AddDays(23.0).AddHours(18.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("宣传物料", "57", "#F9CB9C", startDate.AddDays(24.0).AddHours(12.0).AddMinutes(45.0), new DateTime?(), false, 2),
          new CalendarDisplayModel("大使培训", "58", "#F9CB9C", startDate.AddDays(24.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("朋友圈海报", "59", "#F9CB9C", startDate.AddDays(24.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("背单词", "60", "#F9CB9C", startDate.AddDays(25.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("敷面膜", "61", "#B6D7A8", startDate.AddDays(25.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("线下沙龙嘉宾邀请", "62", "#F9CB9C", startDate.AddDays(25.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("嘉宾邀约", "63", "#F9CB9C", startDate.AddDays(25.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("回复社媒用户留言", "64", "#86C0EF", startDate.AddDays(25.0), new DateTime?(), true, 2),
          new CalendarDisplayModel("增长方案", "65", "#F9CB9C", startDate.AddDays(26.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("看画展", "66", "#F9CB9C", startDate.AddDays(27.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("周复盘", "67", "#D1D7A8", startDate.AddDays(27.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("练琴", "68", "#F9CB9C", startDate.AddDays(27.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("看《恋恋风尘》", "69", "#52B8D2", startDate.AddDays(28.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("公园晨跑", "70", "#5DD1A8", startDate.AddDays(28.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("瑜伽课", "71", "#5DD1A8", startDate.AddDays(29.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("买牛奶", "72", "#D1D7A8", startDate.AddDays(29.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("看《脸庞，村庄》", "73", "#52B8D2", startDate.AddDays(29.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("采购奖品", "74", "#F9CB9C", startDate.AddDays(30.0).AddHours(12.0).AddMinutes(15.0), new DateTime?(), false, 0),
          new CalendarDisplayModel("反馈统计", "75", "#86C0EF", startDate.AddDays(30.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("用户支持", "76", "#86C0EF", startDate.AddDays(31.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("月度复盘", "77", "#86C0EF", startDate.AddDays(31.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("制作宣传视频", "78", "#F9CB9C", startDate.AddDays(32.0).AddHours(9.0).AddMinutes(30.0), new DateTime?(), false, 0),
          new CalendarDisplayModel("敷面膜", "79", "#B6D7A8", startDate.AddDays(32.0), new DateTime?(), true, 0),
          new CalendarDisplayModel("经费核算", "80", "#F9CB9C", startDate.AddDays(33.0), new DateTime?(), true, 0)
        };
      return CalendarDisplayModel.visualModels.ToList<CalendarDisplayModel>();
    }
  }
}
