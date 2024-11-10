// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.Item.KanbanItemController
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Dal;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Kanban.Item
{
  public class KanbanItemController : DisplayItemController
  {
    public KanbanItemController(UIElement element, DisplayItemModel model)
      : base(element, model)
    {
    }

    public void OnBatchSelectMouseUp()
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      IKanban kanban = this.GetKanban();
      if (kanban == null || kanban.IsInDragging() || !model.Enable)
        return;
      if (Utils.IfShiftPressed())
      {
        this.GetTaskList().BatchShiftSelected(model.TaskId);
      }
      else
      {
        if (!Utils.IfCtrlPressed())
          return;
        this.GetTaskList().BatchCtrlSelected(model.TaskId);
      }
    }

    public async Task<TaskDetailPopup> ShowDetailWindow()
    {
      KanbanItemController kanbanItemController = this;
      DisplayItemModel model = kanbanItemController.Model;
      if (model == null)
        return (TaskDetailPopup) null;
      IKanban kanban = kanbanItemController.GetKanban();
      return kanban == null || kanban.IsInDragging() ? (TaskDetailPopup) null : await kanbanItemController.SelectKanbanItem(model);
    }

    public async Task<TaskDetailPopup> SelectKanbanItem(DisplayItemModel model, bool focusTitle = false)
    {
      KanbanItemController kanbanItemController = this;
      model.Selected = true;
      TaskListView taskListView = kanbanItemController.GetTaskListView();
      if (taskListView != null)
        taskListView.SetSelected(new List<string>()
        {
          model.TaskId
        });
      return await kanbanItemController.ShowTaskDetail(focusTitle);
    }

    public override void ReloadOnDetailClosed()
    {
      IKanban kanban = this.GetKanban();
      if (kanban == null)
        return;
      kanban.CancelOperation();
      if (Utils.IfShiftPressed() || Utils.IfCtrlPressed())
        return;
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      model.Selected = false;
      IKanbanColumn column = this.GetColumn();
      if (column == null || !column.TaskChanged)
        return;
      column.TaskChanged = false;
      column.ReloadColumn();
      SyncManager.TryDelaySync();
    }

    public void OnKanbanItemDrag()
    {
      try
      {
        this.DoColumnItemDrag();
      }
      catch (Exception ex)
      {
      }
    }

    protected override void OnEventArchived() => this.GetKanban()?.Reload(true);

    private void DoColumnItemDrag()
    {
      if (this.Model == null || this.Element == null)
        return;
      DisplayItemModel model1 = this.Model;
      IKanban kanban = this.GetKanban();
      if (kanban == null || kanban.IsTaskPopupOpened())
        return;
      this.GetTaskList()?.ResetDrag();
      DisplayItemModel model2 = DisplayItemModel.Copy(model1);
      model2.ShowBottomMargin = true;
      string columnId = this.GetColumn()?.GetColumnId();
      List<string> selectedTaskIds = Utils.FindParent<IBatchEditable>((DependencyObject) this.Element)?.GetSelectedTaskIds();
      if (selectedTaskIds != null && selectedTaskIds.Count > 1)
      {
        List<DisplayItemModel> source = this.GetColumn()?.RemoveTasks(selectedTaskIds, model1.Id) ?? new List<DisplayItemModel>();
        if (!source.Contains(model1))
          source.Add(model1);
        List<string> treeTopIds = TaskDao.GetTreeTopIds(selectedTaskIds, (string) null);
        List<DisplayItemModel> list = source.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => treeTopIds.Contains(m.Id))).ToList<DisplayItemModel>();
        list.ForEach((Action<DisplayItemModel>) (m => m.OriginLevel = m.Level));
        DisplayItemModel model3 = new DisplayItemModel(new TaskBaseViewModel()
        {
          Id = model1.Id,
          Title = string.Format(Utils.GetString("MultiTasks"), (object) source.Count),
          Type = model1.Type,
          ColumnId = columnId
        })
        {
          DragTaskIds = source.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (m => m.Id)).ToList<string>(),
          BatchModels = list
        };
        kanban.StartDragTask(columnId, model3);
        model1.Dragging = true;
      }
      else
      {
        this.GetColumn()?.RemoveTaskChildren(model1);
        model2.SourceViewModel.ColumnId = columnId;
        kanban.StartDragTask(columnId, model2);
        model1.OriginLevel = model1.Level;
        model1.Dragging = true;
      }
    }

    private IKanban GetKanban() => Utils.FindParent<IKanban>((DependencyObject) this.Element);

    private IKanbanColumn GetColumn()
    {
      return Utils.FindParent<IKanbanColumn>((DependencyObject) this.Element);
    }
  }
}
