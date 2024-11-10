// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PomoTaskDetailModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PomoTaskDetailModel : BaseViewModel
  {
    public string TaskId;
    public int TaskStatus;
    public string ProjectId;

    public string Title { get; set; }

    public string Content { get; set; }

    public string Desc { get; set; }

    public bool IsText { get; set; } = true;

    public bool ShowDesc { get; set; }

    public ObservableCollection<PomoSubtaskDetailModel> Items { get; set; }

    public void SortItems()
    {
      if (this.IsText)
        return;
      ObservableCollection<PomoSubtaskDetailModel> items = this.Items;
      // ISSUE: explicit non-virtual call
      if ((items != null ? (__nonvirtual (items.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      List<PomoSubtaskDetailModel> list = this.Items.ToList<PomoSubtaskDetailModel>();
      PomoTaskDetailModel.SortItems(list);
      this.Items.Clear();
      list.ForEach(new Action<PomoSubtaskDetailModel>(((Collection<PomoSubtaskDetailModel>) this.Items).Add));
    }

    private static void SortItems(List<PomoSubtaskDetailModel> items)
    {
      items.Sort((Comparison<PomoSubtaskDetailModel>) ((a, b) =>
      {
        if (a.Status == b.Status)
        {
          if (a.Status == 0)
            return a.SortOrder.CompareTo(b.SortOrder);
          if (a.CompleteDate.HasValue && b.CompleteDate.HasValue)
            return a.CompleteDate.Value.CompareTo(a.CompleteDate.Value);
        }
        return a.Status.CompareTo(b.Status);
      }));
    }

    public static async Task<PomoTaskDetailModel> GetDetailModel(string taskId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null || thinTaskById.status != 0)
        return (PomoTaskDetailModel) null;
      PomoTaskDetailModel model = new PomoTaskDetailModel()
      {
        TaskId = taskId,
        Title = thinTaskById.title,
        Content = TaskUtils.ReplaceAttachmentTextInString(thinTaskById.content),
        Desc = thinTaskById.desc,
        IsText = thinTaskById.kind == "TEXT",
        ShowDesc = thinTaskById.kind != "TEXT" && !string.IsNullOrEmpty(thinTaskById.desc),
        TaskStatus = thinTaskById.status,
        ProjectId = thinTaskById.projectId
      };
      if (!model.IsText)
      {
        List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(taskId);
        List<PomoSubtaskDetailModel> list = checkItemsByTaskId != null ? checkItemsByTaskId.Select<TaskDetailItemModel, PomoSubtaskDetailModel>((Func<TaskDetailItemModel, PomoSubtaskDetailModel>) (sub => new PomoSubtaskDetailModel(sub))).ToList<PomoSubtaskDetailModel>() : (List<PomoSubtaskDetailModel>) null;
        PomoTaskDetailModel.SortItems(list);
        model.Items = new ObservableCollection<PomoSubtaskDetailModel>();
        list?.ForEach(new Action<PomoSubtaskDetailModel>(((Collection<PomoSubtaskDetailModel>) model.Items).Add));
      }
      return model;
    }
  }
}
