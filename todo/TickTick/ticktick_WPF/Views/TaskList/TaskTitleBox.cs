// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.TaskTitleBox
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.TaskList.Item;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public sealed class TaskTitleBox : LinkTextEditBox
  {
    private ListItemContent _parent;

    public TaskTitleBox()
    {
      this.ShowUrlNameOnly = true;
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (!Utils.IfCtrlPressed() && !Utils.IfShiftPressed() || this.CurrentFocused)
        return;
      e.Handled = true;
    }

    public event EventHandler<string> MultipleTextPaste;

    protected override void SetupGenerators()
    {
      this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new CustomLinkElementGenerator((ILinkTextEditor) this));
      this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new LinkNameGenerator((ILinkTextEditor) this, true));
      this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new EmojiGenerator((IEmojiRender) this));
      this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new HideLinkBracketGenerator((FrameworkElement) this, this.LinkNameDict, true));
      this.TextArea.TextView.ElementGenerators.Add((VisualLineElementGenerator) new HideLinkBracketGenerator((FrameworkElement) this, this.LinkNameDict, false));
    }

    protected override async void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
      TaskTitleBox taskTitleBox = this;
      if (taskTitleBox.MultipleTextPaste != null)
      {
        string text = string.Empty;
        try
        {
          text = e.SourceDataObject.GetData(DataFormats.UnicodeText) as string;
        }
        catch (Exception ex)
        {
          Utils.Toast(Utils.GetString("OperateFailed"));
        }
        if (!string.IsNullOrEmpty(text) && text.StartsWith("\r\n"))
          text = " " + text.Substring(2);
        if (!string.IsNullOrEmpty(text) && Utils.ShowBatchAdd(text))
        {
          e.CancelCommand();
          if (taskTitleBox.CanBatchAdd())
          {
            Utils.RunOnUiThread(taskTitleBox.Dispatcher, (Action) (() =>
            {
              if (this.ShowBatchAddWindow(text))
                return;
              base.OnPaste(sender, e);
            }));
            return;
          }
        }
      }
      // ISSUE: reference to a compiler-generated method
      taskTitleBox.\u003C\u003En__0(sender, e);
    }

    private bool CanBatchAdd()
    {
      IBatchAddable parent = Utils.FindParent<IBatchAddable>((DependencyObject) this);
      return parent != null && parent.CanBatchAdd();
    }

    private bool ShowBatchAddWindow(string text)
    {
      TaskDetailPopup.SetCanClose(false);
      Utils.FindParent<TaskDetailView>((DependencyObject) this)?.SetPopupShowing(true);
      bool closeClick = false;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("BatchAddTasks"), Utils.GetString("BatchAddTasksContent"), Utils.GetString("Add"), Utils.GetString("Cancel"));
      customerDialog.Owner = Window.GetWindow((DependencyObject) this);
      customerDialog.CloseClick += (EventHandler) ((sender, e) => closeClick = true);
      bool? nullable = customerDialog.ShowDialog();
      if (closeClick || (!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
        return false;
      EventHandler<string> multipleTextPaste = this.MultipleTextPaste;
      if (multipleTextPaste != null)
        multipleTextPaste((object) this, text);
      Utils.FindParent<TaskDetailView>((DependencyObject) this)?.SetPopupShowing(false);
      TaskDetailPopup.SetCanClose(true);
      return true;
    }

    public async void FocusOnMove()
    {
      TaskTitleBox element = this;
      Utils.Focus((UIElement) element);
      element.CaretOffset = 0;
    }

    public void SetFocus(int offset)
    {
      Utils.Focus((UIElement) this);
      this.CaretOffset = offset;
    }

    public void SetParent(ListItemContent content) => this._parent = content;

    protected override string GetAccountId() => string.Empty;

    private TaskBaseViewModel GetTaskBaseModel()
    {
      return this._parent?.DataContext is DisplayItemModel dataContext ? dataContext.SourceViewModel : (TaskBaseViewModel) null;
    }

    protected override string GetAvatarProjectId() => this.GetTaskBaseModel()?.ProjectId;

    protected override string GetAssignee() => this.GetTaskBaseModel()?.Assignee;

    protected override int GetPriority()
    {
      TaskBaseViewModel taskBaseModel = this.GetTaskBaseModel();
      return taskBaseModel == null ? -1 : taskBaseModel.Priority;
    }

    protected override List<string> GetTags()
    {
      TaskBaseViewModel taskBaseModel = this.GetTaskBaseModel();
      if (taskBaseModel == null)
        return (List<string>) null;
      string[] tags = taskBaseModel.Tags;
      return tags == null ? (List<string>) null : ((IEnumerable<string>) tags).ToList<string>();
    }

    protected override bool IsUsedInCalendar() => false;
  }
}
