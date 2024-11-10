// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.CourseArchiveSyncService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Service
{
  public class CourseArchiveSyncService
  {
    public static void PullArchivedModels()
    {
      Task.Run((Action) (() => CourseArchiveSyncService.PullArchivedModelsAsync()));
    }

    public static async Task PullArchivedModelsAsync()
    {
      List<CourseArchiveSyncModel> archiveModels = await Communicator.GetArchivedCourses();
      List<ArchivedEventModel> archivedModels = await ArchivedDao.GetArchivedModels(ArchiveKind.Course);
      Dictionary<string, ArchivedEventModel> localDict = new Dictionary<string, ArchivedEventModel>();
      List<ArchivedEventModel> added = new List<ArchivedEventModel>();
      foreach (ArchivedEventModel model in archivedModels)
      {
        if (string.IsNullOrEmpty(model.Key) || localDict.ContainsKey(model.Key))
          BaseDao<ArchivedEventModel>.DeleteAsync(model);
        else
          localDict[model.Key] = model;
      }
      if (archiveModels == null)
      {
        archiveModels = (List<CourseArchiveSyncModel>) null;
        localDict = (Dictionary<string, ArchivedEventModel>) null;
        added = (List<ArchivedEventModel>) null;
      }
      else
      {
        foreach (CourseArchiveSyncModel archiveSyncModel in archiveModels)
        {
          if (!string.IsNullOrEmpty(archiveSyncModel.courseId))
          {
            if (localDict.ContainsKey(archiveSyncModel.courseId))
              localDict.Remove(archiveSyncModel.courseId);
            else
              added.Add(new ArchivedEventModel()
              {
                UserId = LocalSettings.Settings.LoginUserId,
                Key = archiveSyncModel.courseId,
                Kind = 1,
                SyncStatus = 2
              });
          }
        }
        foreach (ArchivedEventModel model in localDict.Values.ToList<ArchivedEventModel>())
          BaseDao<ArchivedEventModel>.DeleteAsync(model);
        int num = await BaseDao<ArchivedEventModel>.InsertAllAsync(added);
        if (localDict.Count <= 0 && added.Count <= 0)
        {
          archiveModels = (List<CourseArchiveSyncModel>) null;
          localDict = (Dictionary<string, ArchivedEventModel>) null;
          added = (List<ArchivedEventModel>) null;
        }
        else
        {
          DataChangedNotifier.NotifyCalendarChanged();
          archiveModels = (List<CourseArchiveSyncModel>) null;
          localDict = (Dictionary<string, ArchivedEventModel>) null;
          added = (List<ArchivedEventModel>) null;
        }
      }
    }

    public static async Task PushLocalArchivedModels()
    {
      List<ArchivedEventModel> locals = await ArchivedDao.GetArchivedModels(ArchiveKind.Course, true);
      CourseArchivePostModel model1 = new CourseArchivePostModel();
      foreach (ArchivedEventModel archivedEventModel in locals.Where<ArchivedEventModel>((Func<ArchivedEventModel, bool>) (local => !string.IsNullOrEmpty(local.Key))))
      {
        switch (archivedEventModel.SyncStatus)
        {
          case -1:
            model1.delete.Add(archivedEventModel.Key);
            continue;
          case 0:
            string key = archivedEventModel.Key;
            string[] strArray1;
            if (key == null)
              strArray1 = (string[]) null;
            else
              strArray1 = key.Split('_');
            string[] strArray2 = strArray1;
            string str = strArray2 == null || strArray2.Length <= 1 ? "" : strArray2[1];
            model1.add.Add(new CourseArchiveSyncModel()
            {
              courseId = archivedEventModel.Key,
              dateStamp = str
            });
            continue;
          default:
            continue;
        }
      }
      if (model1.Empty)
        locals = (List<ArchivedEventModel>) null;
      else if (!await Communicator.PostArchivedCourses(model1))
      {
        locals = (List<ArchivedEventModel>) null;
      }
      else
      {
        foreach (ArchivedEventModel model2 in locals.Where<ArchivedEventModel>((Func<ArchivedEventModel, bool>) (local => !string.IsNullOrEmpty(local.Key))))
        {
          switch (model2.SyncStatus)
          {
            case -1:
              int num1 = await BaseDao<ArchivedEventModel>.DeleteAsync(model2);
              continue;
            case 0:
              model2.SyncStatus = 2;
              int num2 = await BaseDao<ArchivedEventModel>.UpdateAsync(model2);
              continue;
            default:
              continue;
          }
        }
        locals = (List<ArchivedEventModel>) null;
      }
    }
  }
}
