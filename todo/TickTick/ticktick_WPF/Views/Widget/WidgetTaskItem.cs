// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WidgetTaskItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Tag;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class WidgetTaskItem : UserControl, IShowTaskDetailWindow, IComponentConnector
  {
    public static readonly DependencyProperty ShowCountDownProperty = DependencyProperty.Register(nameof (ShowCountDown), typeof (bool), typeof (WidgetTaskItem), new PropertyMetadata((object) false));
    private bool _customDateSelected;
    private string _edittingTaskId = string.Empty;
    private ProjectWidget _parent;
    internal WidgetTaskItem RootView;
    internal Grid RootGrid;
    internal Grid ItemContainer;
    internal Path OpenIndicator;
    internal Popup ManuallyCheckInPopup;
    internal ManualRecordCheckinControl CheckInControl;
    internal Grid CheckBox;
    private bool _contentLoaded;

    public bool ShowCountDown
    {
      get => (bool) this.GetValue(WidgetTaskItem.ShowCountDownProperty);
      set => this.SetValue(WidgetTaskItem.ShowCountDownProperty, (object) value);
    }

    protected WidgetTaskItem(string themeId)
    {
      ThemeUtil.SetTheme(themeId, (FrameworkElement) this);
      this.InitializeComponent();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      model.SetIcon();
      Task.Run((Action) (() => model.SetPropertyChangedEvent()));
    }

    private DisplayItemModel Model => this.DataContext as DisplayItemModel;

    private async void OnCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        e.Handled = true;
        ProjectWidget parent1 = this.GetParent();
        if ((parent1 != null ? (parent1.IsLocked ? 1 : 0) : 0) != 0)
          model = (DisplayItemModel) null;
        else if (model.IsToggling)
          model = (DisplayItemModel) null;
        else if (!model.Enable)
        {
          if (string.IsNullOrEmpty(model.TeamId))
            model = (DisplayItemModel) null;
          else if (model.Permission == null)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            switch (model.Permission)
            {
              case "read":
                string str1 = Utils.GetString("ReadOnly");
                ProjectWidget parent2 = this.GetParent();
                if (parent2 == null)
                {
                  model = (DisplayItemModel) null;
                  break;
                }
                // ISSUE: explicit non-virtual call
                __nonvirtual (parent2.TryToastString((object) null, string.Format(Utils.GetString("NoPermissionToEdit"), (object) str1)));
                model = (DisplayItemModel) null;
                break;
              case "comment":
                string str2 = Utils.GetString("CanComment");
                ProjectWidget parent3 = this.GetParent();
                if (parent3 == null)
                {
                  model = (DisplayItemModel) null;
                  break;
                }
                // ISSUE: explicit non-virtual call
                __nonvirtual (parent3.TryToastString((object) null, string.Format(Utils.GetString("NoPermissionToEdit"), (object) str2)));
                model = (DisplayItemModel) null;
                break;
              default:
                model = (DisplayItemModel) null;
                break;
            }
          }
        }
        else
        {
          model.IsToggling = true;
          bool soundPlayed = await this.TryPlayCompleteStory(model);
          if (model.IsTask)
          {
            model.SourceViewModel.Status = model.SourceViewModel.Status == 0 ? 2 : 0;
            await this.CompleteTask(model, soundPlayed);
          }
          else if (model.IsHabit)
            this.ToggleHabitCompleted();
          else if (model.IsItem)
          {
            TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.TaskId);
            if (!string.IsNullOrEmpty(thinTaskById.attendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) thinTaskById))
            {
              ProjectWidget parent4 = this.GetParent();
              if (parent4 == null)
              {
                model = (DisplayItemModel) null;
                return;
              }
              // ISSUE: explicit non-virtual call
              __nonvirtual (parent4.TryToastString((object) null, Utils.GetString("OnlyOwnerCanCompleteSubtask")));
              model = (DisplayItemModel) null;
              return;
            }
            await this.CompleteCheckItem(model);
          }
          model.IsToggling = false;
          model = (DisplayItemModel) null;
        }
      }
    }

    public async Task<bool> TryPlayCompleteStory(DisplayItemModel model)
    {
      ProjectWidget parent = this.GetParent();
      if (!model.IsTask && !model.IsItem || parent == null)
        return false;
      bool flag = model.Status == 0;
      string completeStoryText;
      if (flag)
      {
        completeStoryText = await parent.GetCompleteStoryText(model);
        flag = completeStoryText != null;
      }
      if (!flag)
        return false;
      await this.PlayCompleteStory(completeStoryText);
      return true;
    }

    private async Task PlayCompleteStory(string text)
    {
      WidgetTaskItem widgetTaskItem = this;
      bool flag = true;
      if (text == Utils.GetString("FirstTaskDone"))
      {
        flag = LocalSettings.Settings.ExtraSettings.CpltStoryTimes < 2;
        ++LocalSettings.Settings.ExtraSettings.CpltStoryTimes;
      }
      int left = (widgetTaskItem.DataContext is DisplayItemModel dataContext ? dataContext.Level : 0) * 16;
      double num = ((double) widgetTaskItem.FindResource((object) "Height32") - 32.0) / 2.0;
      CompleteStory completeStoryView = new CompleteStory();
      completeStoryView.HorizontalAlignment = HorizontalAlignment.Left;
      widgetTaskItem.RootGrid.Children.Add((UIElement) completeStoryView);
      completeStoryView.Margin = new Thickness((double) (left + 10), num - 4.0, 0.0, 0.0);
      DoubleAnimation doubleAnimation1 = new DoubleAnimation();
      doubleAnimation1.Duration = new Duration(TimeSpan.FromMilliseconds(300.0));
      doubleAnimation1.From = new double?(-40.0);
      doubleAnimation1.To = new double?(0.0);
      DoubleAnimation animation1 = doubleAnimation1;
      TextBlock textBlock = new TextBlock()
      {
        Text = text,
        FontSize = 14.0
      };
      textBlock.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      TranslateTransform translateTransform1 = new TranslateTransform()
      {
        Y = -40.0
      };
      textBlock.RenderTransform = (Transform) translateTransform1;
      textBlock.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock.Margin = new Thickness((double) (left + 48), num + 7.0, 0.0, 0.0);
      widgetTaskItem.RootGrid.Children.Add((UIElement) textBlock);
      DoubleAnimation doubleAnimation2 = new DoubleAnimation();
      doubleAnimation2.Duration = new Duration(TimeSpan.FromMilliseconds(300.0));
      doubleAnimation2.From = new double?(0.0);
      doubleAnimation2.To = new double?(Math.Max(0.0, widgetTaskItem.ActualWidth - (double) left));
      DoubleAnimation animation2 = doubleAnimation2;
      Border border = new Border();
      border.Width = 0.0;
      border.Height = widgetTaskItem.ActualHeight;
      border.Opacity = 0.2;
      Border backBorder = border;
      backBorder.HorizontalAlignment = HorizontalAlignment.Left;
      backBorder.SetResourceReference(Control.BackgroundProperty, (object) "TickYellow");
      backBorder.Margin = new Thickness((double) left, 0.0, 0.0, 0.0);
      widgetTaskItem.RootGrid.Children.Add((UIElement) backBorder);
      TranslateTransform translateTransform2 = new TranslateTransform();
      widgetTaskItem.ItemContainer.RenderTransform = (Transform) translateTransform2;
      DoubleAnimation doubleAnimation3 = new DoubleAnimation();
      doubleAnimation3.Duration = new Duration(TimeSpan.FromMilliseconds(300.0));
      doubleAnimation3.From = new double?(0.0);
      doubleAnimation3.To = new double?(widgetTaskItem.ItemContainer.ActualHeight);
      DoubleAnimation animation3 = doubleAnimation3;
      Utils.PlayCompletionSound();
      translateTransform2.BeginAnimation(TranslateTransform.YProperty, (AnimationTimeline) animation3);
      translateTransform1.BeginAnimation(TranslateTransform.YProperty, (AnimationTimeline) animation1);
      backBorder.BeginAnimation(FrameworkElement.WidthProperty, (AnimationTimeline) animation2);
      completeStoryView.PlayStory();
      await Task.Delay(flag ? 900 : 500);
      widgetTaskItem.RootGrid.Children.Remove((UIElement) completeStoryView);
      widgetTaskItem.RootGrid.Children.Remove((UIElement) textBlock);
      widgetTaskItem.RootGrid.Children.Remove((UIElement) backBorder);
      widgetTaskItem.ItemContainer.RenderTransform = (Transform) null;
      completeStoryView = (CompleteStory) null;
      textBlock = (TextBlock) null;
      backBorder = (Border) null;
    }

    private void ToggleHabitCompleted()
    {
      if (this.Model.Status == 0)
        this.CheckInHabit();
      else
        this.ResetHabit();
    }

    private async void ResetHabit()
    {
      int num = await HabitService.ResetCheckInHabit(this.Model.Id, DateTime.Today, this.Model.Habit.Type) ? 1 : 0;
      this.GetParent()?.Reload();
    }

    private async void CheckInHabit()
    {
      if (this.Model.Habit.Type.ToLower() == "boolean")
        await this.CheckInBooleanHabit();
      else if (Math.Abs(this.Model.Habit.Step + 1.0) <= 0.001)
        this.ManuallyRecordCheckIn();
      else
        await this.CheckInRealHabit(this.Model.Habit.Step);
    }

    private void ManuallyRecordCheckIn()
    {
      this.ManuallyCheckInPopup.IsOpen = true;
      this.CheckInControl.Init(this.Model.Habit.Unit);
      this.CheckInControl.Cancel -= new EventHandler(this.OnCheckInPopupCancel);
      this.CheckInControl.Cancel += new EventHandler(this.OnCheckInPopupCancel);
      this.CheckInControl.Save -= new EventHandler<double>(this.OnCheckInPopupSave);
      this.CheckInControl.Save += new EventHandler<double>(this.OnCheckInPopupSave);
    }

    private void OnCheckInPopupSave(object sender, double amount)
    {
      this.ManuallyCheckInPopup.IsOpen = false;
      this.CheckInRealHabit(amount);
    }

    private void OnCheckInPopupCancel(object sender, EventArgs e)
    {
      this.ManuallyCheckInPopup.IsOpen = false;
    }

    private async Task CheckInBooleanHabit()
    {
      DisplayItemModel model = this.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        Utils.PlayCompletionSound();
        await Task.Delay(20);
        await HabitService.CheckInHabit(model.Id, DateTime.Today, window: (IToastShowWindow) this.GetParent());
        ProjectWidget parent = this.GetParent();
        if (parent == null)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          parent.Reload();
          model = (DisplayItemModel) null;
        }
      }
    }

    private async Task CheckInRealHabit(double step)
    {
      DisplayItemModel model = this.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        model.SourceViewModel.Status = 2;
        Utils.PlayCompletionSound();
        if (model.HabitCheckIn == null)
        {
          model.SetHabitCheckIn(new HabitCheckInModel()
          {
            Goal = model.Habit.Goal,
            Value = step
          });
        }
        else
        {
          model.HabitCheckIn.Value += step;
          model.SetHabitCheckIn(model.HabitCheckIn);
        }
        await HabitService.CheckInHabit(model.Id, DateTime.Today, step, window: (IToastShowWindow) this.GetParent());
        if (model.HabitCheckIn.Value >= model.Habit.Goal)
        {
          ProjectWidget parent = this.GetParent();
          if (parent == null)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            parent.Reload();
            model = (DisplayItemModel) null;
          }
        }
        else if (step > 0.0)
        {
          await Task.Delay(1500);
          ProjectWidget parent = this.GetParent();
          if (parent == null)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            parent.Reload();
            model = (DisplayItemModel) null;
          }
        }
        else
        {
          ProjectWidget parent = this.GetParent();
          if (parent == null)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            parent.Reload();
            model = (DisplayItemModel) null;
          }
        }
      }
    }

    private async Task CompleteCheckItem(DisplayItemModel item)
    {
      await TaskDetailItemService.CompleteCheckItem(item.Id, needUndo: true, window: (IToastShowWindow) this.GetParent());
      await Task.Delay(120);
      this.GetParent()?.Reload();
    }

    private async Task CompleteTask(DisplayItemModel item, bool soundPlayed)
    {
      int status = item.Status;
      await Task.Delay(250);
      TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(item.Id, status, status != 0, (IToastShowWindow) this.GetParent(), playSound: !soundPlayed);
      this.GetParent()?.Reload();
    }

    private ProjectWidget GetParent()
    {
      return this._parent ?? (this._parent = WidgetTaskItem.FindParent((DependencyObject) this));
    }

    private static ProjectWidget FindParent(DependencyObject child)
    {
      if (child == null)
        return (ProjectWidget) null;
      DependencyObject parent = VisualTreeHelper.GetParent(child);
      if (parent == null)
        return (ProjectWidget) null;
      return parent is ProjectWidget projectWidget ? projectWidget : WidgetTaskItem.FindParent(parent);
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      if (this.Model == null || !this.Model.Enable && !this.Model.IsEvent)
        return;
      ProjectWidget parent = this.GetParent();
      if ((parent != null ? (parent.IsLocked ? 1 : 0) : 0) != 0)
        return;
      this.SetEditting(true);
      this.ShowOperationDialog();
    }

    private async void ShowOperationDialog()
    {
      WidgetTaskItem widgetTaskItem = this;
      DisplayItemModel model = widgetTaskItem.DataContext as DisplayItemModel;
      if (model == null)
        ;
      else if (model.IsItem)
        ;
      else
      {
        model.Selected = true;
        widgetTaskItem.SetEditting(true);
        widgetTaskItem._edittingTaskId = model.TaskId;
        if (model.IsCourse || model.IsEvent)
        {
          OperationDialog operationDialog = model.IsCourse ? (OperationDialog) new ArchiveOperationDialog(model.Id, ArchiveKind.Course) : (OperationDialog) new CalendarOperationDialog(new EventArchiveArgs(model.SourceViewModel));
          operationDialog.SetPlaceTarget((UIElement) widgetTaskItem);
          operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) ((obj, kv) =>
          {
            this.GetParent()?.RemoveItem(model);
            if (!model.IsCourse)
              return;
            DataChangedNotifier.OnScheduleChanged();
          });
          operationDialog.Closed += (EventHandler) ((o, e) =>
          {
            model.Selected = false;
            this.SetEditting(false);
          });
          operationDialog.Show();
        }
        else if (model.Type == DisplayType.Habit)
        {
          if (model.Status != 0)
            ;
          else
          {
            List<OperationItemViewModel> types = new List<OperationItemViewModel>()
            {
              new OperationItemViewModel(ActionType.Skip)
            };
            if (LocalSettings.Settings.EnableFocus)
              types.Insert(0, new OperationItemViewModel(ActionType.StartFocus)
              {
                SubActions = new List<OperationItemViewModel>()
                {
                  new OperationItemViewModel(ActionType.StartPomo)
                  {
                    Enable = !TickFocusManager.Working || LocalSettings.Settings.PomoType == FocusConstance.Focus
                  },
                  new OperationItemViewModel(ActionType.StartTiming)
                  {
                    Enable = !TickFocusManager.Working || LocalSettings.Settings.PomoType == FocusConstance.Timing
                  }
                }
              });
            OperationDialog operationDialog = new OperationDialog(model.Id, types);
            operationDialog.SetPlaceTarget((UIElement) widgetTaskItem);
            operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) (async (obj, kv) =>
            {
              switch (kv.Value)
              {
                case ActionType.Skip:
                  model.Selected = false;
                  this.SetEditting(false);
                  await HabitService.SkipHabit(kv.Key);
                  this.GetParent()?.RemoveItem(model);
                  this.GetParent()?.TryToastString((object) null, Utils.GetString("Skipped"));
                  break;
                case ActionType.StartTiming:
                  TickFocusManager.TryStartFocusHabit(kv.Key, false);
                  break;
                case ActionType.StartPomo:
                  TickFocusManager.TryStartFocusHabit(kv.Key, true);
                  break;
              }
            });
            operationDialog.Closed += new EventHandler(widgetTaskItem.OnOperationClosed);
            operationDialog.Show();
          }
        }
        else
        {
          ProjectWidget parent = widgetTaskItem.GetParent();
          OperationExtra taskAccessInfo = await TaskOperationHelper.GetTaskAccessInfo(model.TaskId, (parent != null ? (parent.CanAddSubTask() ? 1 : 0) : 0) != 0 && model.Enable && (model.InDetail || model.Status == 0));
          if (taskAccessInfo == null)
            ;
          else
          {
            if (model.IsTaskOrNote)
              taskAccessInfo.IsPinned = new bool?(model.IsPinned);
            TaskOperationDialog dialog = new TaskOperationDialog(taskAccessInfo, (UIElement) widgetTaskItem);
            WidgetWindow parentWindow = widgetTaskItem.GetParent()?.GetParentWindow();
            if (parentWindow != null)
              dialog.Resources = parentWindow.Resources;
            dialog.CreateSubTask += new EventHandler(widgetTaskItem.CreateSubTask);
            dialog.Copied += new EventHandler(widgetTaskItem.CopyTask);
            dialog.LinkCopied += new EventHandler(widgetTaskItem.CopyTaskLink);
            dialog.Deleted += new EventHandler(widgetTaskItem.DeleteTask);
            dialog.PrioritySelect += new EventHandler<int>(widgetTaskItem.SetPriority);
            dialog.ProjectSelect += new EventHandler<SelectableItemViewModel>(widgetTaskItem.OnProjectSelect);
            dialog.AbandonOrReopen += new EventHandler(widgetTaskItem.OnAbandonOrReopen);
            dialog.AssigneeSelect += new EventHandler<AvatarInfo>(widgetTaskItem.OnAssigneeSelect);
            dialog.Closed += new EventHandler(widgetTaskItem.OnOperationClosed);
            dialog.CustomDateSelect += new EventHandler(widgetTaskItem.OnCustomSelect);
            dialog.TagsSelect += new EventHandler<TagSelectData>(widgetTaskItem.OnTagsSelected);
            dialog.SkipCurrentRecurrence += new EventHandler(widgetTaskItem.OnSkipRecurrence);
            dialog.SwitchTaskOrNote += new EventHandler(widgetTaskItem.OnSwitchTaskOrNote);
            dialog.Toast += new EventHandler<string>(widgetTaskItem.OnTaskOperationToast);
            dialog.Starred += new EventHandler<bool>(widgetTaskItem.OnStarClick);
            dialog.CompleteDateChanged += (EventHandler<DateTime>) (async (o, date) =>
            {
              await TaskService.ChangeCompleteDate(model.TaskId, date);
              SyncManager.TryDelaySync();
            });
            dialog.TimeClear += (EventHandler) (async (arg, obj) =>
            {
              dialog.Dismiss();
              await this.ClearDate();
            });
            dialog.TimeSelect += (EventHandler<TimeData>) (async (arg, data) =>
            {
              this._customDateSelected = false;
              if (this.Model != null)
                model.Selected = false;
              this.SetEditting(false);
              await TaskService.SetDate(this._edittingTaskId, data);
              dialog.Dismiss();
            });
            dialog.QuickDateSelect += (EventHandler<DateTime>) (async (arg, date) =>
            {
              this.SetEditting(false);
              TaskModel taskModel = await TaskService.SetStartDate(this._edittingTaskId, date);
              dialog.Dismiss();
            });
            dialog.Show();
          }
        }
      }
    }

    private void OnTaskOperationToast(object sender, string e)
    {
      this.GetParent()?.TryToastString((object) null, e);
    }

    private async void OnAbandonOrReopen(object sender, EventArgs e)
    {
      if (this.Model == null)
        return;
      int closeStatus = this.Model.IsAbandoned ? 0 : -1;
      TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(this.Model.TaskId, closeStatus, closeStatus != 0, (IToastShowWindow) this.GetParent());
    }

    private async void OnStarClick(object sender, bool isPin)
    {
      if (this.Model == null)
        return;
      await TaskService.TogglesStarred(this.Model.TaskId, this.Model.CurrentProjectIdentity?.CatId ?? this.Model.ProjectId);
    }

    private void CreateSubTask(object sender, EventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      this.GetParent()?.CreateSubtask(dataContext);
    }

    private async void OnSwitchTaskOrNote(object sender, EventArgs e)
    {
      await TaskService.SwitchTaskOrNote(this.Model.Id);
      SyncManager.TryDelaySync();
    }

    private async void CopyTaskLink(object sender, EventArgs e)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(this._edittingTaskId);
      if (thinTaskById == null)
        return;
      TaskUtils.CopyTaskLink(thinTaskById.id, thinTaskById.projectId, thinTaskById.title);
    }

    private async Task ClearDate()
    {
      WidgetTaskItem parent = this;
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(parent._edittingTaskId);
      if (thinTaskById == null)
        return;
      if (string.IsNullOrEmpty(thinTaskById.attendId))
      {
        await TaskService.ClearDate(parent._edittingTaskId);
      }
      else
      {
        if (!await TaskOperationHelper.CheckIfTaskAllowClearDate(parent._edittingTaskId, (DependencyObject) parent))
          return;
        await TaskService.ClearAgendaDate(parent._edittingTaskId);
      }
    }

    private async void OnSkipRecurrence(object sender, EventArgs e)
    {
      TaskModel taskModel = await TaskService.SkipCurrentRecurrence(this._edittingTaskId, toastWindow: (IToastShowWindow) this.GetParent());
      SyncManager.TryDelaySync();
    }

    private async void OnTagsSelected(object sender, TagSelectData tags)
    {
      await TaskService.SetTags(this._edittingTaskId, tags.OmniSelectTags);
    }

    private void OnCustomSelect(object sender, EventArgs e) => this._customDateSelected = true;

    private void NotifyTaskChange()
    {
      DisplayItemModel model = this.Model;
    }

    private void OnOperationClosed(object sender, EventArgs e)
    {
      this.SetEditting(false);
      if (this.DataContext == null || !(this.DataContext is DisplayItemModel dataContext) || this._customDateSelected)
        return;
      dataContext.Selected = false;
    }

    private async void OnAssigneeSelect(object sender, AvatarInfo assignee)
    {
      await TaskService.SetAssignee(this._edittingTaskId, assignee.UserId);
    }

    private async void OnProjectSelect(object sender, SelectableItemViewModel e)
    {
      (string projectId, string str) = e.GetProjectAndColumnId();
      ProjectModel project = CacheManager.GetProjectById(projectId);
      if (project == null)
      {
        project = (ProjectModel) null;
      }
      else
      {
        await TaskService.TryMoveProject(new MoveProjectArgs(this._edittingTaskId, project)
        {
          ColumnId = str
        });
        ProjectWidget parent = this.GetParent();
        if (parent == null)
        {
          project = (ProjectModel) null;
        }
        else
        {
          parent.OnTaskProjectChanged(this._edittingTaskId, project);
          project = (ProjectModel) null;
        }
      }
    }

    private async void SetPriority(object sender, int priority)
    {
      await TaskService.SetPriority(this._edittingTaskId, priority);
    }

    private async void DeleteTask(object sender, EventArgs e)
    {
      WidgetTaskItem widgetTaskItem = this;
      TaskModel task = await TaskDao.GetThinTaskById(widgetTaskItem._edittingTaskId);
      if (string.IsNullOrEmpty(task.attendId))
      {
        List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(widgetTaskItem._edittingTaskId, task.projectId);
        // ISSUE: explicit non-virtual call
        if (subTasksByIdAsync != null && __nonvirtual (subTasksByIdAsync.Count) > 0)
        {
          subTasksByIdAsync.Add(task);
          ProjectWidget parent = WidgetTaskItem.FindParent((DependencyObject) widgetTaskItem);
          if (parent == null)
          {
            task = (TaskModel) null;
          }
          else
          {
            // ISSUE: explicit non-virtual call
            __nonvirtual (parent.BatchDeleteTask(subTasksByIdAsync));
            task = (TaskModel) null;
          }
        }
        else
        {
          ProjectWidget parent = WidgetTaskItem.FindParent((DependencyObject) widgetTaskItem);
          if (parent == null)
          {
            task = (TaskModel) null;
          }
          else
          {
            // ISSUE: explicit non-virtual call
            __nonvirtual (parent.TaskDeleted(widgetTaskItem._edittingTaskId));
            task = (TaskModel) null;
          }
        }
      }
      else if (!await TaskOperationHelper.CheckIfAllowDeleteAgenda(task, (DependencyObject) widgetTaskItem))
      {
        task = (TaskModel) null;
      }
      else
      {
        await TaskService.DeleteAgenda(widgetTaskItem._edittingTaskId, task.projectId, task.attendId);
        task = (TaskModel) null;
      }
    }

    private async void CopyTask(object sender, EventArgs e)
    {
      TaskModel taskModel = await TaskService.CopyTask(this._edittingTaskId);
    }

    private void OnLeftClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      ProjectWidget parent = this.GetParent();
      if ((parent != null ? (parent.IsLocked ? 1 : 0) : 0) != 0)
        return;
      this.TryShowDisplayWindow(this.Model, true);
    }

    public async void TryShowDisplayWindow(DisplayItemModel model, bool byMouse, bool focusTitle = false)
    {
      if (model == null || !PopupStateManager.CanShowDetailPopup())
        return;
      model.Selected = true;
      if (model.IsTaskOrNote || model.IsItem)
        this.ShowTaskOrItemWindow(model, byMouse, focusTitle);
      else if (model.IsEvent)
        await this.ShowCalendarEventWindow(model);
      else if (model.IsHabit)
      {
        await this.ShowHabitWindow(model);
      }
      else
      {
        if (!model.IsCourse)
          return;
        await this.ShowCourseWindow(model);
      }
    }

    private async Task ShowCourseWindow(DisplayItemModel model)
    {
      WidgetTaskItem target = this;
      CourseDetailViewModel detailViewModelById = await CourseDetailViewModel.GetDetailViewModelById(model.Id);
      if (detailViewModelById == null)
        ;
      else
      {
        model.Selected = true;
        CourseDetailWindow courseDetailWindow = new CourseDetailWindow(detailViewModelById);
        Window window = Window.GetWindow((DependencyObject) target);
        if (window != null)
          courseDetailWindow.Resources = window.Resources;
        courseDetailWindow.Closed += (EventHandler) (async (obj, e) =>
        {
          await Task.Delay(200);
          model.Selected = false;
          this.SetEditting(false);
        });
        courseDetailWindow.Show((UIElement) target, target.ActualWidth, target.ActualWidth > 300.0);
        target.SetEditting(true);
      }
    }

    private async Task ShowHabitWindow(DisplayItemModel model)
    {
      WidgetTaskItem target = this;
      HabitModel habitById = await HabitDao.GetHabitById(model.Id);
      if (habitById == null)
        return;
      model.Selected = true;
      target.SetEditting(true);
      HabitCheckInWindow habitCheckInWindow = new HabitCheckInWindow(habitById, model.Status, model.StartDate, (IToastShowWindow) target.GetParent());
      Window window = Window.GetWindow((DependencyObject) target);
      if (window != null)
        habitCheckInWindow.Resources = window.Resources;
      habitCheckInWindow.Show((UIElement) target, target.ActualWidth, 0.0, target.ActualWidth >= 350.0);
      habitCheckInWindow.OnAction -= new EventHandler(target.OnCheckInAction);
      habitCheckInWindow.OnAction += new EventHandler(target.OnCheckInAction);
    }

    private async void OnCheckInAction(object sender, EventArgs extra)
    {
      WidgetTaskItem widgetTaskItem = this;
      widgetTaskItem.SetEditting(false);
      if (!(widgetTaskItem.DataContext is DisplayItemModel dataContext) || !dataContext.Selected)
        return;
      dataContext.Selected = false;
    }

    private async Task ShowCalendarEventWindow(DisplayItemModel model)
    {
      WidgetTaskItem widgetTaskItem = this;
      string eventId = model.Id;
      if (string.IsNullOrEmpty(eventId))
      {
        eventId = (string) null;
      }
      else
      {
        widgetTaskItem.SetEditting(true);
        if (eventId.Contains("_time_"))
          eventId = eventId.Remove(eventId.IndexOf("_time_", StringComparison.Ordinal));
        CalendarEventModel calendarEventModel = await CalendarEventDao.GetEventById(eventId);
        if (calendarEventModel == null)
          calendarEventModel = await CalendarEventDao.GetEventByEventId(eventId);
        CalendarEventModel model1 = calendarEventModel;
        if (model1 == null)
        {
          eventId = (string) null;
        }
        else
        {
          ProjectWidget parent = Utils.FindParent<ProjectWidget>((DependencyObject) widgetTaskItem);
          if (parent != null && parent.TaskList.ItemsSource is ObservableCollection<DisplayItemModel> itemsSource && itemsSource.Any<DisplayItemModel>())
          {
            DisplayItemModel displayItemModel = itemsSource.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.Id == model.Id));
            if (displayItemModel != null)
            {
              model1.DueStart = displayItemModel.StartDate;
              model1.DueEnd = displayItemModel.DueDate;
            }
          }
          CalendarDetailWindow calendarDetailWindow = new CalendarDetailWindow();
          Window window = Window.GetWindow((DependencyObject) widgetTaskItem);
          if (window != null)
            calendarDetailWindow.Resources = window.Resources;
          calendarDetailWindow.Disappear += new EventHandler<string>(widgetTaskItem.OnDetailClosed);
          calendarDetailWindow.EventArchiveChanged += new EventHandler<bool>(widgetTaskItem.OnEventArchiveChanged);
          calendarDetailWindow.Show((UIElement) widgetTaskItem, widgetTaskItem.ActualWidth, 0.0, widgetTaskItem.ActualWidth > 350.0, new CalendarDetailViewModel(model1));
          eventId = (string) null;
        }
      }
    }

    private void OnEventArchiveChanged(object sender, bool e)
    {
      this.GetParent()?.Reload(force: true);
    }

    private void ShowTaskOrItemWindow(DisplayItemModel model, bool byMouse, bool focusTitle)
    {
      this.SetEditting(true);
      TaskDetailPopup taskDetailPopup = new TaskDetailPopup();
      ProjectWidget parent = this.GetParent();
      if (parent != null)
      {
        if (PopupStateManager.LastTarget == this)
          return;
        taskDetailPopup.DependentWindow = (IToastShowWindow) parent;
      }
      taskDetailPopup.Disappear -= new EventHandler<string>(this.OnDetailClosed);
      taskDetailPopup.Disappear += new EventHandler<string>(this.OnDetailClosed);
      taskDetailPopup.Show(model.TaskId, model.IsItem ? model.Id : string.Empty, new TaskWindowDisplayArgs((UIElement) this, byMouse ? 0.0 : 256.0, PopupLocationCalculator.GetMousePoint(!byMouse), focusTitle));
    }

    private void SetEditting(bool editting)
    {
      ProjectWidget parent = this.GetParent();
      if (parent == null)
        return;
      parent.IsEditting = editting;
    }

    private void OnDetailClosed(object sender, string e)
    {
      this.SetEditting(false);
      if (this.DataContext is DisplayItemModel dataContext && dataContext.Selected)
        dataContext.Selected = false;
      if (!(sender is TaskDetailWindow taskDetailWindow))
        return;
      taskDetailWindow.Disappear -= new EventHandler<string>(this.OnDetailClosed);
    }

    private void OnOpenPathClick(object sender, MouseButtonEventArgs e)
    {
      ProjectWidget parent = this.GetParent();
      if ((parent != null ? (parent.IsLocked ? 1 : 0) : 0) != 0 || !(this.DataContext is DisplayItemModel dataContext))
        return;
      this.GetParent()?.OnItemOpenClick(dataContext);
      e.Handled = true;
    }

    private void OnCheckRightMouseUp(object sender, MouseButtonEventArgs e)
    {
      ProjectWidget parent = this.GetParent();
      if ((parent != null ? (parent.IsLocked ? 1 : 0) : 0) != 0 || !(this.DataContext is DisplayItemModel dataContext))
        return;
      this.GetParent()?.PopupCloseTask(dataContext, this);
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/widget/widgettaskitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.RootView = (WidgetTaskItem) target;
          this.RootView.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnLeftClick);
          this.RootView.MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
          break;
        case 2:
          this.RootGrid = (Grid) target;
          break;
        case 3:
          this.ItemContainer = (Grid) target;
          break;
        case 4:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpenPathClick);
          break;
        case 5:
          this.OpenIndicator = (Path) target;
          break;
        case 6:
          this.ManuallyCheckInPopup = (Popup) target;
          break;
        case 7:
          this.CheckInControl = (ManualRecordCheckinControl) target;
          break;
        case 8:
          this.CheckBox = (Grid) target;
          this.CheckBox.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckBoxClick);
          this.CheckBox.MouseRightButtonUp += new MouseButtonEventHandler(this.OnCheckRightMouseUp);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
