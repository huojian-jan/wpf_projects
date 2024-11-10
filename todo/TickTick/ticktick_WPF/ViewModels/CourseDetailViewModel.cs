// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.CourseDetailViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class CourseDetailViewModel : BaseViewModel
  {
    public string Title { get; set; }

    public string Color { get; set; }

    public List<CourseItemViewModel> Items { get; set; }

    public string ScheduleName { get; set; }

    public static async Task<CourseDetailViewModel> Build(CourseDisplayModel model)
    {
      CourseDetailViewModel result = new CourseDetailViewModel();
      result.Title = model.Title;
      result.Color = string.IsNullOrEmpty(model.Color) ? "#FF4772FA" : model.Color;
      CourseModel coursedByIdAsync = await ScheduleDao.GetCoursedByIdAsync(model.Id);
      if (!string.IsNullOrEmpty(coursedByIdAsync?.ItemsStr))
      {
        List<CourseDetailModel> items = ScheduleService.GetCourseDetailModelsFromJson(coursedByIdAsync.ItemsStr);
        if (items == null || items.Count == 0)
          return result;
        result.Items = items.Select<CourseDetailModel, CourseItemViewModel>((Func<CourseDetailModel, CourseItemViewModel>) (item => new CourseItemViewModel(item, items.Count > 1 ? items.IndexOf(item) + 1 : 0))).ToList<CourseItemViewModel>();
      }
      return result;
    }

    public static async Task<CourseDetailViewModel> GetDetailViewModelById(string id)
    {
      if (string.IsNullOrEmpty(id))
        return (CourseDetailViewModel) null;
      int length = id.IndexOf("_", StringComparison.Ordinal);
      if (length > 0)
        id = id.Substring(0, length);
      CourseModel coursedByIdAsync = await ScheduleDao.GetCoursedByIdAsync(id);
      if (coursedByIdAsync == null)
        return (CourseDetailViewModel) null;
      CourseDetailViewModel result = new CourseDetailViewModel();
      result.Title = coursedByIdAsync.Name;
      result.Color = string.IsNullOrEmpty(coursedByIdAsync.Color) ? "#FF4772FA" : coursedByIdAsync.Color;
      if (!string.IsNullOrEmpty(coursedByIdAsync?.ItemsStr))
      {
        List<CourseDetailModel> items = ScheduleService.GetCourseDetailModelsFromJson(coursedByIdAsync.ItemsStr);
        if (items == null || items.Count == 0)
          return result;
        result.Items = items.Select<CourseDetailModel, CourseItemViewModel>((Func<CourseDetailModel, CourseItemViewModel>) (item => new CourseItemViewModel(item, items.Count > 1 ? items.IndexOf(item) + 1 : 0))).ToList<CourseItemViewModel>();
      }
      result.ScheduleName = (await ScheduleDao.GetScheduleByIdAsync(coursedByIdAsync.ScheduleId))?.Name;
      return result;
    }
  }
}
