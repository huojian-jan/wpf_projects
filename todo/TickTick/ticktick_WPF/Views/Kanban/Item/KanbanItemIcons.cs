// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.Item.KanbanItemIcons
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Kanban.Item
{
  public class KanbanItemIcons : TaskItemIcons
  {
    private TextBlock _text;

    protected override void SetIcon(bool isDataContextChanged = false)
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      int index = 0;
      this.AddTimeText(dataContext, ref index);
      if (dataContext.Habit?.Type?.ToLower() != "boolean")
      {
        HabitCheckInModel habitCheckIn = dataContext.HabitCheckIn;
        if ((habitCheckIn != null ? (habitCheckIn.Value > 0.0 ? 1 : 0) : 0) != 0)
          this.ShowHabitProgress(dataContext.HabitCheckIn, dataContext.Status != 0, ref index);
        this.ShowHabitProgressText(dataContext, ref index);
        this.Children.RemoveRange(index, this.Children.Count - index);
      }
      this.ShowImage(dataContext.GetShowAttachment(), "attachmentsDrawingImage", "Attachment", ref index);
      this.ShowImage(dataContext.ShowLocation, "locationDrawingImage", "Location", ref index);
      this.ShowImage(dataContext.ShowComment, "CommentDrawingImage", "Comment", ref index);
      this.ShowImage(dataContext.ShowDescription, "HasDescDrawingImage", "Description", ref index);
      this.ShowRepeatImage(dataContext, ref index);
      this.ShowImage(dataContext.ShowReminder.GetValueOrDefault(), "reminderDrawingImage", "reminder", ref index);
      this.ShowProgress(dataContext, ref index);
      this.Children.RemoveRange(index, this.Children.Count - index);
      if (dataContext.ShowPomo)
        this.AddPomoSummary(dataContext.PomoSummary);
      this.Visibility = this.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
      Utils.FindParent<KanbanItemView>((DependencyObject) this)?.SetContainerMargin(dataContext);
    }

    private void AddPomoSummary(PomodoroSummaryModel pomoSummary)
    {
      TaskItemPomoSummaryControl pomoSummaryControl = new TaskItemPomoSummaryControl();
      pomoSummaryControl.Margin = new Thickness(2.0, 2.0, 0.0, 2.0);
      pomoSummaryControl.MaxWidth = 150.0;
      TaskItemPomoSummaryControl element = pomoSummaryControl;
      this.Children.Add((UIElement) element);
      element.SetData(pomoSummary);
    }
  }
}
