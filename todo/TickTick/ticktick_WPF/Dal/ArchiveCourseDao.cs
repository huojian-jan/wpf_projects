// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ArchiveCourseDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class ArchiveCourseDao : ArchivedDao
  {
    public static async Task CheckOldArchiveCourse()
    {
      foreach (ArchivedEventModel arc in await ArchivedDao.GetArchivedModels(ArchiveKind.Course))
      {
        string key = arc.Key;
        string[] strArray1;
        if (key == null)
          strArray1 = (string[]) null;
        else
          strArray1 = key.Split('_');
        string[] strArray2 = strArray1;
        if (strArray2 != null && strArray2.Length == 3)
        {
          string id = strArray2[0];
          string date = strArray2[1];
          int result;
          int index = int.TryParse(strArray2[2], out result) ? result : 0;
          CourseModel coursedByIdAsync = await ScheduleDao.GetCoursedByIdAsync(id);
          if (coursedByIdAsync != null)
          {
            List<CourseDetailModel> detailModelsFromJson = ScheduleService.GetCourseDetailModelsFromJson(coursedByIdAsync.ItemsStr);
            // ISSUE: explicit non-virtual call
            if (detailModelsFromJson != null && __nonvirtual (detailModelsFromJson.Count) > index)
            {
              CourseDetailModel courseDetailModel = detailModelsFromJson[index];
              arc.Key = id + "_" + date + "_" + courseDetailModel.StartLesson.ToString() + "_" + courseDetailModel.EndLesson.ToString();
              int num = await BaseDao<ArchivedEventModel>.UpdateAsync(arc);
            }
            else
            {
              int num1 = await BaseDao<ArchivedEventModel>.DeleteAsync(arc);
            }
          }
          else
          {
            int num2 = await BaseDao<ArchivedEventModel>.DeleteAsync(arc);
          }
          id = (string) null;
          date = (string) null;
        }
      }
    }
  }
}
