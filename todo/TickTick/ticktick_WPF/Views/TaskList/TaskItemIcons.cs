// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.TaskItemIcons
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class TaskItemIcons : StackPanel
  {
    private Style _imageStyle;
    protected DisplayItemController _controller;

    public TaskItemIcons()
    {
      this.Height = 20.0;
      this.Orientation = Orientation.Horizontal;
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      this.SetIcon(true);
    }

    public void ResetIcons() => this.SetIcon();

    protected virtual void SetIcon(bool isDataContextChanged = false)
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      int index = 0;
      if (dataContext.HasChildren || dataContext.IsParentTask)
      {
        this.ShowSubTaskImage("AddSubTaskDrawingImage", "Subtask", ref index, isDataContextChanged);
        this.ShowSubTasksCompletionRate(true, dataContext, ref index, isDataContextChanged);
      }
      this.ShowImage(dataContext.GetShowAttachment(), "attachmentsDrawingImage", "Attachment", ref index);
      this.ShowImage(dataContext.ShowLocation, "locationDrawingImage", "Location", ref index);
      this.ShowImage(dataContext.ShowComment, "CommentDrawingImage", "Comment", ref index);
      this.ShowImage(dataContext.ShowDescription, "HasDescDrawingImage", "Description", ref index);
      this.ShowRepeatImage(dataContext, ref index);
      this.ShowImage(dataContext.ShowReminder.GetValueOrDefault(), "reminderDrawingImage", "reminder", ref index);
      this.ShowProgress(dataContext, ref index);
      if (dataContext.Habit?.Type?.ToLower() != "boolean")
      {
        HabitCheckInModel habitCheckIn = dataContext.HabitCheckIn;
        if ((habitCheckIn != null ? (habitCheckIn.Value > 0.0 ? 1 : 0) : 0) != 0)
          this.ShowHabitProgress(dataContext.HabitCheckIn, dataContext.Status != 0, ref index);
        this.ShowHabitProgressText(dataContext, ref index);
      }
      this.Children.RemoveRange(index, this.Children.Count - index);
      this.AddTimeText(dataContext, ref index);
    }

    protected void AddTimeText(DisplayItemModel model, ref int index)
    {
      bool flag = model.Enable && !model.IsHabit;
      if (model.StartDate.HasValue && !model.IsNote || !DateUtils.IsOutDated(model.StartDate, model.DueDate, model.IsAllDay))
      {
        if (this.Children.Count > index && this.Children[index] is TextBlock child && child.Name == "TimeText")
        {
          child.Cursor = flag ? Cursors.Hand : Cursors.Arrow;
          if (flag)
          {
            child.MouseLeftButtonUp -= new MouseButtonEventHandler(this.ShowSetDateDialog);
            child.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowSetDateDialog);
          }
          else
            child.MouseLeftButtonUp -= new MouseButtonEventHandler(this.ShowSetDateDialog);
        }
        else
        {
          TextBlock textBlock = new TextBlock();
          textBlock.Cursor = flag ? Cursors.Hand : Cursors.Arrow;
          textBlock.Margin = new Thickness(4.0, -1.0, 4.0, 0.0);
          textBlock.Name = "TimeText";
          TextBlock element = textBlock;
          element.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeTextStyle");
          if (flag)
            element.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowSetDateDialog);
          this.Children.Insert(index, (UIElement) element);
        }
        ++index;
      }
      else
      {
        if (this.Children.Count <= index || !(this.Children[index] is TextBlock child) || !(child.Name == "TimeText"))
          return;
        this.Children.Remove((UIElement) child);
      }
    }

    private void ShowSetDateDialog(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (!(this.DataContext is DisplayItemModel dataContext) || !dataContext.Enable)
        return;
      this._controller?.SelectDate((FrameworkElement) this, -100.0);
    }

    private void ShowSubTasksCompletionRate(
      bool modelShowChildrenIcon,
      DisplayItemModel model,
      ref int index,
      bool isDataContextChanged)
    {
      if (((!LocalSettings.Settings.ShowDetails || LocalSettings.Settings.InSearch ? 0 : (!model.InMatrix ? 1 : 0)) & (modelShowChildrenIcon ? 1 : 0)) == 0)
        return;
      if (this.Children.Count > index && this.Children[index] is TextBlock child && child.Name != "TimeText")
      {
        child.Text = model.CompletionRate;
      }
      else
      {
        TextBlock textBlock = new TextBlock();
        textBlock.VerticalAlignment = VerticalAlignment.Center;
        textBlock.Margin = new Thickness(2.0, 0.0, 1.0, 0.0);
        textBlock.MaxWidth = 150.0;
        textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        TextBlock element = textBlock;
        element.SetResourceReference(FrameworkElement.StyleProperty, (object) "TaskListViewTitleStyle");
        element.SetResourceReference(TextBlock.ForegroundProperty, model.Status == 0 ? (object) "BaseColorOpacity60" : (object) "BaseColorOpacity20");
        element.Text = model.CompletionRate;
        this.Children.Insert(index, (UIElement) element);
      }
      ++index;
    }

    protected void ShowHabitProgressText(DisplayItemModel model, ref int index)
    {
      if (model.Habit == null)
        return;
      string str;
      if (model.HabitCheckIn != null)
      {
        string[] strArray = new string[5];
        double goal = model.HabitCheckIn.Value;
        strArray[0] = goal.ToString();
        strArray[1] = "/";
        goal = model.HabitCheckIn.Goal;
        strArray[2] = goal.ToString();
        strArray[3] = " ";
        strArray[4] = model.Habit.Unit;
        str = string.Concat(strArray);
      }
      else
        str = 0.ToString() + "/" + model.Habit.Goal.ToString() + " " + model.Habit.Unit;
      if (this.Children.Count > index && this.Children[index] is TextBlock child && child.Name != "TimeText")
      {
        child.Text = str;
      }
      else
      {
        TextBlock textBlock = new TextBlock();
        textBlock.VerticalAlignment = VerticalAlignment.Center;
        textBlock.Margin = new Thickness(2.0, 0.0, 0.0, 0.0);
        textBlock.Text = str;
        textBlock.MaxWidth = 150.0;
        textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        TextBlock element = textBlock;
        element.SetResourceReference(FrameworkElement.StyleProperty, (object) "TaskListViewTitleStyle");
        element.SetResourceReference(TextBlock.ForegroundProperty, model.Status == 0 ? (object) "BaseColorOpacity60" : (object) "BaseColorOpacity20");
        this.Children.Insert(index, (UIElement) element);
      }
      ++index;
    }

    protected void ShowHabitProgress(HabitCheckInModel checkIn, bool completed, ref int index)
    {
      if (checkIn == null)
        return;
      int num = Math.Min((int) (checkIn.Value / checkIn.Goal * 100.0), 100);
      if (this.Children.Count > index && this.Children[index] is CycleProgressBar element)
      {
        element.TargetPercent = (double) num;
      }
      else
      {
        CycleProgressBar cycleProgressBar = new CycleProgressBar();
        cycleProgressBar.Duration = 10;
        cycleProgressBar.Height = 12.0;
        cycleProgressBar.Width = 12.0;
        cycleProgressBar.Thickness = 2.0;
        cycleProgressBar.HorizontalAlignment = HorizontalAlignment.Center;
        cycleProgressBar.TargetPercent = (double) num;
        cycleProgressBar.Margin = new Thickness(4.0, 0.0, 0.0, 0.0);
        cycleProgressBar.Opacity = completed ? 0.4 : 0.8;
        element = cycleProgressBar;
        this.Children.Insert(index, (UIElement) element);
        element.SetResourceReference(CycleProgressBar.UnderColorProperty, (object) "BaseColorOpacity20");
        element.SetResourceReference(CycleProgressBar.TopColorProperty, (object) "PrimaryColor");
      }
      element.ToolTip = (object) (num.ToString() + "%");
      ++index;
    }

    protected void ShowProgress(DisplayItemModel model, ref int index)
    {
      if (!model.ShowProgress)
        return;
      if (this.Children.Count > index && this.Children[index] is TaskCircleProgress child)
      {
        child.Percentage = (double) model.Progress;
        child.ToolTip = (object) model.ProgressContent;
      }
      else
      {
        TaskCircleProgress taskCircleProgress = new TaskCircleProgress();
        taskCircleProgress.Percentage = (double) model.Progress;
        taskCircleProgress.Radius = 4.5;
        taskCircleProgress.StrokeThickness = 1.0;
        taskCircleProgress.HorizontalAlignment = HorizontalAlignment.Center;
        taskCircleProgress.VerticalAlignment = VerticalAlignment.Center;
        taskCircleProgress.Margin = new Thickness(2.0, 0.0, 2.0, 0.0);
        taskCircleProgress.IsHitTestVisible = false;
        TaskCircleProgress element = taskCircleProgress;
        element.SetResourceReference(TaskCircleProgress.SegmentColorProperty, (object) "BaseColorOpacity20");
        element.ToolTip = (object) model.ProgressContent;
        this.Children.Insert(index, (UIElement) element);
      }
      ++index;
    }

    protected void ShowRepeatImage(DisplayItemModel model, ref int index)
    {
      if (!model.ShowRepeat)
        return;
      string str;
      if (RepeatUtils.GetRepeatType(model.RepeatFrom, model.RepeatFlag) == RepeatFromType.Custom)
        str = Utils.GetString("RepeatByCustom");
      else if (string.IsNullOrEmpty(model.RepeatFlag) || model.RepeatFlag == "RRULE:FREQ=NONE")
      {
        str = string.Empty;
      }
      else
      {
        string repeatFlag = model.RepeatFlag;
        str = !repeatFlag.Contains("FREQ=DAILY") || !repeatFlag.Contains("TT_SKIP=HOLIDAY,WEEKEND") || repeatFlag.Contains("INTERVAL") && !repeatFlag.Contains("INTERVAL=1") ? RRuleUtils.RRule2String(model.RepeatFrom, model.RepeatFlag, model.StartDate) : Utils.GetString("OfficialWorkingDays");
      }
      if (this.Children.Count > index && this.Children[index] is Image child)
      {
        child.SetResourceReference(Image.SourceProperty, (object) "RepeatDrawingImage");
        child.ToolTip = (object) str;
      }
      else
      {
        Image element = new Image();
        element.SetResourceReference(Image.SourceProperty, (object) "RepeatDrawingImage");
        element.SetResourceReference(FrameworkElement.StyleProperty, (object) "ListIcon");
        element.ToolTip = (object) str;
        element.SetValue(ToolTipService.InitialShowDelayProperty, (object) 200);
        this.Children.Insert(index, (UIElement) element);
      }
      ++index;
    }

    private void ShowSubTaskImage(
      string source,
      string tooltip,
      ref int index,
      bool isDataContextChanged)
    {
      if (this.Children.Count > index && this.Children[index] is Image child)
      {
        child.SetResourceReference(Image.SourceProperty, (object) source);
        child.SetResourceReference(FrameworkElement.ToolTipProperty, (object) tooltip);
      }
      else
      {
        Image element = new Image();
        element.SetResourceReference(Image.SourceProperty, (object) source);
        element.SetResourceReference(FrameworkElement.StyleProperty, (object) "ListIcon");
        element.SetResourceReference(FrameworkElement.ToolTipProperty, (object) tooltip);
        this.Children.Insert(index, (UIElement) element);
      }
      ++index;
    }

    protected void ShowImage(bool show, string source, string tooltip, ref int index)
    {
      if (!show)
        return;
      if (this.Children.Count > index && this.Children[index] is Image child)
      {
        child.SetResourceReference(Image.SourceProperty, (object) source);
        child.SetResourceReference(FrameworkElement.ToolTipProperty, (object) tooltip);
      }
      else
      {
        Image element = new Image();
        element.SetResourceReference(Image.SourceProperty, (object) source);
        element.SetResourceReference(FrameworkElement.StyleProperty, (object) "ListIcon");
        element.SetResourceReference(FrameworkElement.ToolTipProperty, (object) tooltip);
        this.Children.Insert(index, (UIElement) element);
      }
      ++index;
    }

    public void SetController(DisplayItemController controller) => this._controller = controller;
  }
}
