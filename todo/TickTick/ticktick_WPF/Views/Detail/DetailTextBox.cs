// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.DetailTextBox
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.MarkDown;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class DetailTextBox : LinkTextEditBox
  {
    public DetailTextBox()
    {
      this.Margin = new Thickness(2.0, 2.0, 0.0, 0.0);
      this.SetMarkRegexText(false);
      this.ContextMenuOpening += new ContextMenuEventHandler(((LinkTextEditBox) this).EditorMenuOnContextMenuOpening);
      this.ContextMenuClosing += (ContextMenuEventHandler) ((o, e) => this.OnPopupOpenChanged(false));
    }

    private TaskBaseViewModel GetTaskBaseModel()
    {
      return Utils.FindParent<TaskDetailView>((DependencyObject) this)?.DataContext is TaskDetailViewModel dataContext ? dataContext.SourceViewModel : (TaskBaseViewModel) null;
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
  }
}
