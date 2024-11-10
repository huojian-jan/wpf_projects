// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TemplateDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.MainListView.DetailView;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TemplateDao
  {
    public static async Task<bool> MergeTemplates(
      List<TaskTemplateModel> taskTemplates,
      List<TaskTemplateModel> noteTemplates,
      List<TaskTemplateModel> localTemplate)
    {
      if (taskTemplates == null && noteTemplates == null)
        return false;
      List<TaskTemplateModel> remoteTemplate = new List<TaskTemplateModel>();
      List<TaskTemplateModel> taskTemplateModelList1 = taskTemplates;
      // ISSUE: explicit non-virtual call
      if ((taskTemplateModelList1 != null ? (__nonvirtual (taskTemplateModelList1.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        foreach (TaskTemplateModel taskTemplate in taskTemplates)
        {
          taskTemplate.Kind = TemplateKind.Task.ToString();
          remoteTemplate.Add(taskTemplate);
        }
      }
      List<TaskTemplateModel> taskTemplateModelList2 = noteTemplates;
      // ISSUE: explicit non-virtual call
      if ((taskTemplateModelList2 != null ? (__nonvirtual (taskTemplateModelList2.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        foreach (TaskTemplateModel noteTemplate in noteTemplates)
        {
          noteTemplate.Kind = TemplateKind.Note.ToString();
          remoteTemplate.Add(noteTemplate);
        }
      }
      List<SyncStatusModel> localStatus = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (m => m.Type >= 13 && m.Type <= 15 && m.UserId == LocalSettings.Settings.LoginUserId)).ToListAsync();
      if (localTemplate != null)
      {
        List<TaskTemplateModel> updates = new List<TaskTemplateModel>();
        List<TaskTemplateModel> add = new List<TaskTemplateModel>();
        List<TaskTemplateModel> deleted = new List<TaskTemplateModel>((IEnumerable<TaskTemplateModel>) localTemplate);
        deleted.RemoveAll((Predicate<TaskTemplateModel>) (m => localStatus.Where<SyncStatusModel>((Func<SyncStatusModel, bool>) (s => s.Type == 13)).Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (s => s.EntityId)).Contains<string>(m.Id)));
        foreach (TaskTemplateModel taskTemplateModel1 in remoteTemplate)
        {
          bool flag = true;
          taskTemplateModel1.UserId = LocalSettings.Settings.LoginUserId;
          foreach (TaskTemplateModel taskTemplateModel2 in localTemplate)
          {
            if (taskTemplateModel2.Id == taskTemplateModel1.Id)
            {
              deleted.Remove(taskTemplateModel2);
              flag = false;
              if (taskTemplateModel2.Etag != taskTemplateModel1.Etag)
              {
                taskTemplateModel1._Id = taskTemplateModel2._Id;
                updates.Add(taskTemplateModel1);
                break;
              }
              break;
            }
          }
          if (flag && !localStatus.Where<SyncStatusModel>((Func<SyncStatusModel, bool>) (s => s.Type == 15)).Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (s => s.EntityId)).Contains<string>(taskTemplateModel1.Id))
            add.Add(taskTemplateModel1);
        }
        int num1 = await App.Connection.UpdateAllAsync((IEnumerable) updates);
        int num2 = await App.Connection.InsertAllAsync((IEnumerable) add);
        foreach (object obj in deleted)
        {
          int num3 = await App.Connection.DeleteAsync(obj);
        }
        return updates.Count > 0 || add.Count > 0 || deleted.Count > 0;
      }
      int num = await App.Connection.InsertAllAsync((IEnumerable) taskTemplates);
      return remoteTemplate.Count > 0;
    }

    public static async void PullTemplates()
    {
      List<TaskTemplateModel> localTemplate = await TemplateDao.GetLocalTemplate();
      TemplatesModel templatesModel = await Communicator.PullTemplates();
      if (templatesModel == null)
      {
        localTemplate = (List<TaskTemplateModel>) null;
      }
      else
      {
        TemplateDao.MergeTemplates(templatesModel.taskTemplates, templatesModel.noteTemplates, localTemplate);
        localTemplate = (List<TaskTemplateModel>) null;
      }
    }

    public static async Task AddTemplates(List<TaskTemplateModel> templates, bool isNote = false)
    {
      int num = await App.Connection.InsertAllAsync((IEnumerable) templates);
      TemplateDao.TryPushTemplate(templates, isNote);
    }

    private static async void TryPushTemplate(List<TaskTemplateModel> templates, bool isNote)
    {
      TaskTemplateSyncBean syncBean = new TaskTemplateSyncBean();
      syncBean.Add.AddRange((IEnumerable<TaskTemplateModel>) templates);
      BatchUpdateResult result = await Communicator.PushTemplates(syncBean, isNote);
      BatchUpdateResult batchUpdateResult = result;
      int num1;
      if (batchUpdateResult == null)
      {
        num1 = 0;
      }
      else
      {
        int? count = batchUpdateResult.Id2error?.Keys.Count;
        int num2 = 0;
        num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
      }
      if (num1 == 0)
        ;
      else
      {
        List<SyncStatusModel> addStatus = new List<SyncStatusModel>();
        templates.ForEach((Action<TaskTemplateModel>) (t =>
        {
          if (!result.Id2error.Keys.Contains<string>(t.Id))
            return;
          addStatus.Add(new SyncStatusModel()
          {
            EntityId = t.Id,
            Type = 13,
            UserId = LocalSettings.Settings.LoginUserId,
            ModifyPoint = DateTime.Now.Ticks
          });
        }));
        App.Connection.InsertAllAsync((IEnumerable) addStatus);
      }
    }

    public static async Task<List<TaskTemplateModel>> GetLocalTemplate()
    {
      List<TaskTemplateModel> localTemplate = await App.Connection.QueryAsync<TaskTemplateModel>(" select * from TaskTemplateModel where UserId = '" + LocalSettings.Settings.LoginUserId + "' order by CreatedTime desc ");
      localTemplate?.ForEach((Action<TaskTemplateModel>) (m =>
      {
        if (m.ItemsToString != null)
          m.Items = JsonConvert.DeserializeObject<List<string>>(m.ItemsToString);
        if (m.TagsToString != null)
          m.Tags = JsonConvert.DeserializeObject<List<string>>(m.TagsToString);
        if (m.ChidlrenToString == null)
          return;
        m.Chidlren = JsonConvert.DeserializeObject<List<TaskTemplateModel>>(m.ChidlrenToString);
      }));
      if (localTemplate == null)
        localTemplate = new List<TaskTemplateModel>();
      return localTemplate;
    }

    public static async void UpdateTemplate(TaskTemplateModel model)
    {
      App.Connection.UpdateAsync((object) model);
      BatchUpdateResult batchUpdateResult = await Communicator.PushTemplates(new TaskTemplateSyncBean()
      {
        Update = {
          model
        }
      }, model.Kind == TemplateKind.Note.ToString());
      int num1;
      if (batchUpdateResult == null)
      {
        num1 = 0;
      }
      else
      {
        int? count = batchUpdateResult.Id2error?.Keys.Count;
        int num2 = 0;
        num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
      }
      if (num1 == 0)
        return;
      SyncStatusModel status = new SyncStatusModel()
      {
        EntityId = model.Id,
        Type = 14,
        UserId = LocalSettings.Settings.LoginUserId,
        ModifyPoint = DateTime.Now.Ticks
      };
      SyncStatusModel syncStatusModel = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (m => m.EntityId == status.EntityId && m.UserId == status.UserId)).FirstOrDefaultAsync();
      if (syncStatusModel != null)
      {
        syncStatusModel.ModifyPoint = status.ModifyPoint;
        int num3 = await App.Connection.UpdateAsync((object) syncStatusModel);
      }
      else
      {
        int num4 = await App.Connection.InsertAsync((object) status);
      }
      SyncManager.Sync();
    }

    public static async void DeleteTemplate(TaskTemplateModel model)
    {
      if (model == null)
        return;
      App.Connection.DeleteAsync((object) model);
      BatchUpdateResult batchUpdateResult = await Communicator.PushTemplates(new TaskTemplateSyncBean()
      {
        Delete = {
          model.Id
        }
      }, model.Kind == TemplateKind.Note.ToString());
      int num1;
      if (batchUpdateResult == null)
      {
        num1 = 0;
      }
      else
      {
        int? count = batchUpdateResult.Id2error?.Keys.Count;
        int num2 = 0;
        num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
      }
      if (num1 == 0)
        return;
      SyncStatusModel status = new SyncStatusModel()
      {
        EntityId = model.Id,
        Type = 15,
        UserId = LocalSettings.Settings.LoginUserId,
        ModifyPoint = DateTime.Now.Ticks
      };
      SyncStatusModel syncStatusModel = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (m => m.EntityId == status.EntityId && m.UserId == status.UserId)).FirstOrDefaultAsync();
      if (syncStatusModel != null)
      {
        if (syncStatusModel.Type == 14)
        {
          syncStatusModel.ModifyPoint = status.ModifyPoint;
          syncStatusModel.Type = status.Type;
          int num3 = await App.Connection.UpdateAsync((object) syncStatusModel);
        }
        else
        {
          int num4 = await App.Connection.DeleteAsync((object) syncStatusModel);
        }
      }
      else
      {
        int num5 = await App.Connection.InsertAsync((object) status);
      }
      SyncManager.Sync();
    }

    public static async Task<TaskTemplateModel> GetTemplateById(string templateId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      TaskTemplateModel templateById = await App.Connection.Table<TaskTemplateModel>().Where((Expression<Func<TaskTemplateModel, bool>>) (model => model.Id == templateId && model.UserId == userId)).FirstOrDefaultAsync();
      if (templateById != null)
      {
        if (templateById.ItemsToString != null)
          templateById.Items = JsonConvert.DeserializeObject<List<string>>(templateById.ItemsToString);
        if (templateById.TagsToString != null)
          templateById.Tags = JsonConvert.DeserializeObject<List<string>>(templateById.TagsToString);
        if (templateById.ChidlrenToString != null)
          templateById.Chidlren = JsonConvert.DeserializeObject<List<TaskTemplateModel>>(templateById.ChidlrenToString);
      }
      return templateById;
    }

    public static async Task SaveTemplate(string taskId, TaskDetailView control)
    {
      TaskTemplateModel newTemp = await TaskTemplateModel.Build(taskId);
      if (newTemp == null)
      {
        control.TryToast(Utils.GetString("IsEmptyTask"));
      }
      else
      {
        List<TaskTemplateModel> localTemplates = await TemplateDao.GetLocalTemplate();
        List<TaskTemplateModel> source = localTemplates;
        List<string> list = source != null ? source.Where<TaskTemplateModel>((Func<TaskTemplateModel, bool>) (t => t.IsNote == newTemp.IsNote)).Select<TaskTemplateModel, string>((Func<TaskTemplateModel, string>) (t => t.Title)).ToList<string>() : (List<string>) null;
        EditTemplateWindow editTemplateWindow = new EditTemplateWindow(newTemp.Title, list, true, Utils.GetString(newTemp.IsNote ? "AddNoteTemplateText" : "AddTaskTemplateText"));
        editTemplateWindow.Title = Utils.GetString("SaveAsTemplate");
        editTemplateWindow.Owner = Window.GetWindow((DependencyObject) control);
        EditTemplateWindow saveWindow = editTemplateWindow;
        saveWindow.Replace += (EventHandler<string>) ((obj, title) =>
        {
          newTemp.Title = title;
          TemplateDao.ReplaceTemplates(newTemp, localTemplates);
          control.TryToast(Utils.GetString("SaveTemplateSuccess"));
        });
        saveWindow.Save += (EventHandler<string>) ((obj, title) =>
        {
          newTemp.Title = title;
          TemplateDao.AddTemplates(new List<TaskTemplateModel>()
          {
            newTemp
          }, newTemp.IsNote);
          control.TryToast(Utils.GetString("SaveTemplateSuccess"));
        });
        saveWindow.Closed += (EventHandler) ((obj, title) => saveWindow.Owner?.Activate());
        saveWindow.ShowDialog();
      }
    }

    private static void ReplaceTemplates(
      TaskTemplateModel newTemp,
      List<TaskTemplateModel> localTemplates)
    {
      TaskTemplateModel taskTemplateModel = localTemplates.FirstOrDefault<TaskTemplateModel>((Func<TaskTemplateModel, bool>) (temp => temp.Kind == newTemp.Kind && temp.Title == newTemp.Title));
      if (taskTemplateModel != null)
      {
        newTemp._Id = taskTemplateModel._Id;
        newTemp.Id = taskTemplateModel.Id;
        newTemp.CreatedTime = taskTemplateModel.CreatedTime;
        newTemp.Etag = taskTemplateModel.Etag;
        TemplateDao.UpdateTemplate(newTemp);
      }
      else
        TemplateDao.AddTemplates(new List<TaskTemplateModel>()
        {
          newTemp
        }, newTemp.IsNote);
    }
  }
}
