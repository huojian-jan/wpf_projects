// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskDetailMoreContent
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskDetailMoreContent : StackPanel, ITabControl, IComponentConnector
  {
    private bool _tagPopupShow;
    private bool _pomoPopupShow;
    private TaskDetailView _parent;
    public bool FirstOpenTag;
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof (SelectedIndex), typeof (int), typeof (TaskDetailMoreContent), new PropertyMetadata((object) -1, (PropertyChangedCallback) null));
    private EscPopup _popup;
    private System.Windows.Point _position;
    internal TaskDetailMoreContent Root;
    internal OptionItemWithImageIcon NewAddSubTaskItem;
    internal OptionItemWithImageIcon PinButton;
    internal Grid AbandonedGrid;
    internal Grid NewTagItem;
    internal OptionCheckBox TagItem;
    internal EscPopup SetTagPopup;
    internal BatchSetTagControl BatchSetTagControl;
    internal OptionItemWithImageIcon NewUploadItem;
    internal OptionItemWithImageIcon NewInsetSummaryItem;
    internal Grid PomoItem;
    internal OptionCheckBox Pomo;
    internal EscPopup TaskPomoPopup;
    internal TaskPomoSetDialog TaskPomoSetDialog;
    internal Line OMTopLine;
    internal OptionItemWithImageIcon ActivityItem;
    internal OptionItemWithImageIcon AddTemplate;
    internal OptionItemWithImageIcon CopyItem;
    internal OptionItemWithImageIcon CopyLinkItem;
    internal OptionItemWithImageIcon StickyItem;
    internal OptionItemWithImageIcon SwitchTaskNoteItem;
    internal OptionItemWithImageIcon Print;
    internal OptionItemWithImageIcon DeletePanel;
    private bool _contentLoaded;

    public TaskDetailMoreContent() => this.InitializeComponent();

    public int SelectedIndex
    {
      get => (int) this.GetValue(TaskDetailMoreContent.SelectedIndexProperty);
      set
      {
        this.SetValue(TaskDetailMoreContent.SelectedIndexProperty, (object) value);
        this.SetItemTabSelected();
      }
    }

    private void TryShowPopup(object sender, MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      if (Math.Abs(this._position.Y - position.Y) <= 4.0 && Math.Abs(this._position.X - position.X) <= 4.0)
        return;
      this._position = position;
      this.TryShowPomoPopup();
      this.TryShowTagPopup();
    }

    private void TryShowTagPopup()
    {
      if (!this.TagItem.IsMouseOver && !this.SetTagPopup.IsMouseOver)
      {
        this.TryHideTagPopup();
      }
      else
      {
        if (this._tagPopupShow || this.SetTagPopup.IsOpen)
          return;
        this._tagPopupShow = true;
        this.DelayShowTagPopup();
      }
    }

    private async Task TryHideTagPopup()
    {
      this._tagPopupShow = false;
      if (!this.SetTagPopup.IsOpen)
        return;
      await Task.Delay(100);
      if (!this.TagItem.IsMouseOver && !this.SetTagPopup.IsMouseOver)
        this.SetTagPopup.IsOpen = false;
      else
        this._tagPopupShow = this.SetTagPopup.IsOpen;
    }

    private async Task DelayShowTagPopup(bool wait = true)
    {
      if (wait)
      {
        await Task.Delay(150);
        if (this.TaskPomoPopup.IsOpen)
        {
          this._tagPopupShow = false;
          return;
        }
      }
      this.ShowTagPopup();
    }

    private void ShowTagPopup()
    {
      if (!(this.DataContext is TaskDetailViewModel dataContext))
        return;
      TagSelectData tags = new TagSelectData()
      {
        OmniSelectTags = TagSerializer.ToTags(dataContext.Tag)
      };
      if (!this._tagPopupShow)
        return;
      this.BatchSetTagControl.Init(tags, this.FirstOpenTag);
      this.FirstOpenTag = false;
      this.SetTagPopup.IsOpen = true;
    }

    private void OnBatchSetTagClosed(object sender, EventArgs e) => this.SetTagPopup.IsOpen = false;

    private async void OnTagsSelected(object sender, TagSelectData tags)
    {
      this._parent.AddActionEvent("om", "tag");
      this._parent.OnTagsSelected(tags, false);
      this.SetTagPopup.IsOpen = false;
      await Task.Delay(200);
      this._popup.IsOpen = false;
    }

    private async void OpenTaskSticky(object sender, MouseButtonEventArgs e)
    {
      TaskDetailMoreContent detailMoreContent = this;
      detailMoreContent._popup.IsOpen = false;
      if (!(detailMoreContent.DataContext is TaskDetailViewModel viewModel))
      {
        viewModel = (TaskDetailViewModel) null;
      }
      else
      {
        string getString = TaskUtils.ToastOnOpenSticky(viewModel.TaskId);
        if (!string.IsNullOrEmpty(getString))
        {
          detailMoreContent._parent.TryToast(getString);
          viewModel = (TaskDetailViewModel) null;
        }
        else
        {
          UserActCollectUtils.AddClickEvent("task_detail", "om", "open_as_sticky_note");
          detailMoreContent._parent.TryForceHideWindow();
          await Task.Delay(200);
          TaskStickyWindow.ShowTaskSticky(new List<string>()
          {
            viewModel.TaskId
          });
          viewModel = (TaskDetailViewModel) null;
        }
      }
    }

    private void TryShowPomoPopup()
    {
      if (!this.Pomo.IsMouseOver && !this.TaskPomoPopup.IsMouseOver)
      {
        this.TryHidePomoPopup();
      }
      else
      {
        if (this._pomoPopupShow || this.TaskPomoPopup.IsOpen)
          return;
        this._pomoPopupShow = true;
        this.DelayShowPomoPopup();
      }
    }

    private async Task TryHidePomoPopup()
    {
      this._pomoPopupShow = false;
      if (!this.TaskPomoPopup.IsOpen)
        return;
      await Task.Delay(100);
      if (!this.Pomo.IsMouseOver && !this.TaskPomoSetDialog.CheckMouseMove())
        this.TaskPomoPopup.IsOpen = false;
      else
        this._pomoPopupShow = this.TaskPomoPopup.IsOpen;
    }

    private async Task DelayShowPomoPopup(bool wait = true)
    {
      if (wait)
      {
        await Task.Delay(150);
        if (this.SetTagPopup.IsOpen)
        {
          this._tagPopupShow = false;
          return;
        }
      }
      this.ShowPomoPopup();
    }

    private void ShowPomoPopup()
    {
      if (!this._pomoPopupShow)
        return;
      this.TaskPomoSetDialog.InitData(this.DataContext is TaskDetailViewModel dataContext ? dataContext.TaskId : (string) null, true, "task_detail", "om");
      this.TaskPomoPopup.IsOpen = true;
    }

    private void AddSubTaskClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      this._parent?.AddSubTaskClick(sender, (RoutedEventArgs) e);
    }

    private void OnStarClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      this._parent?.OnStarClick();
    }

    private void OnAbandonedOrReopenClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      this._parent?.OnAbandonOrReopenClick();
    }

    private async void OnAttachmentClick(object sender, MouseButtonEventArgs e)
    {
      if (e != null)
        e.Handled = true;
      this._parent?.SetNeedFocusDetail(false);
      this._popup.IsOpen = false;
      await Task.Delay(300);
      this._tagPopupShow = false;
      this._pomoPopupShow = false;
      this._parent?.OnAddAttachmentClick();
    }

    private async void OnInsertSummaryClick(object sender, MouseButtonEventArgs e)
    {
      if (e != null)
        e.Handled = true;
      this._parent?.SetNeedFocusDetail(false);
      this._popup.IsOpen = false;
      await Task.Delay(300);
      this._tagPopupShow = false;
      this._pomoPopupShow = false;
      this._parent?.TryInsertSummary();
    }

    private async void ClosePomoPopup(object sender, bool e)
    {
      this._parent?.SetNeedFocusDetail(false);
      this.TaskPomoPopup.IsOpen = false;
      await Task.Delay(200);
      this._popup.IsOpen = false;
    }

    private void OnActivityClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      this._parent?.ShowTaskActivities();
    }

    private async void OnAddTemplateClick(object sender, MouseButtonEventArgs e)
    {
      this._parent?.SetNeedFocusDetail(false);
      this._popup.IsOpen = false;
      await Task.Delay(240);
      this._parent?.OnAddTemplateClick();
    }

    private void OnCopyClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      this._parent?.OnTaskCopy();
    }

    private async void OnCopyLinkClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      this._parent?.OnCopyLinkClick();
    }

    private async void SwitchTaskNoteClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      if (this.SwitchTaskNoteItem.Tag is string tag)
        this._parent?.TryToast(tag);
      else
        this._parent?.SwitchTaskNoteClick();
    }

    private async void OnPrintClick(object sender, MouseButtonEventArgs e)
    {
      this._parent?.SetNeedFocusDetail(false);
      this._popup.IsOpen = false;
      UserActCollectUtils.AddClickEvent("task_detail", "om", "print");
      this._parent?.OnPrint();
    }

    private void OnDeleteClick(object sender, MouseButtonEventArgs e)
    {
      this._popup.IsOpen = false;
      this._parent?.OnDelete();
    }

    public bool HandleTab(bool shift)
    {
      if (this.TaskPomoPopup.IsOpen)
      {
        this.TaskPomoSetDialog.HandleTab(shift);
        return true;
      }
      if (!this.SetTagPopup.IsOpen)
        return this.UpDownSelect(shift);
      this.BatchSetTagControl?.HandleTab(shift);
      return true;
    }

    public bool HandleEnter()
    {
      if (this.SetTagPopup.IsOpen || this.TaskPomoPopup.IsOpen)
        return false;
      switch (this.SelectedIndex)
      {
        case 0:
          this.AddSubTaskClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 1:
          this.OnStarClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 2:
          this.OnAbandonedOrReopenClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 3:
          this._tagPopupShow = true;
          this.DelayShowTagPopup();
          break;
        case 4:
          this.OnAttachmentClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 5:
          this.OnInsertSummaryClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 6:
          this._pomoPopupShow = true;
          this.ShowPomoPopup();
          break;
        case 7:
          this.OnActivityClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 8:
          this.OnAddTemplateClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 9:
          this.OnCopyClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 10:
          this.OnCopyLinkClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 11:
          this.OpenTaskSticky((object) this, (MouseButtonEventArgs) null);
          break;
        case 12:
          this.SwitchTaskNoteClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 13:
          this.OnPrintClick((object) this, (MouseButtonEventArgs) null);
          break;
        case 14:
          this.OnDeleteClick((object) this, (MouseButtonEventArgs) null);
          break;
      }
      return false;
    }

    public bool HandleEsc() => false;

    public bool UpDownSelect(bool isUp)
    {
      if (this.SetTagPopup.IsOpen || this.TaskPomoPopup.IsOpen)
        return false;
      if (this.SelectedIndex < 0)
      {
        this.SelectedIndex = 0;
        return true;
      }
      this.SelectedIndex += isUp ? 14 : 1;
      this.SelectedIndex %= 15;
      switch (this.SelectedIndex)
      {
        case 0:
          if (!this.NewAddSubTaskItem.IsVisible)
            break;
          goto default;
        case 1:
          if (!this.PinButton.IsVisible)
            break;
          goto default;
        case 2:
          if (!this.AbandonedGrid.IsVisible)
            break;
          goto default;
        case 3:
          if (!this.NewTagItem.IsVisible)
            break;
          goto default;
        case 4:
          if (!this.NewUploadItem.IsVisible)
            break;
          goto default;
        case 5:
          if (!this.NewInsetSummaryItem.IsVisible)
            break;
          goto default;
        case 6:
          if (!this.PomoItem.IsVisible)
            break;
          goto default;
        case 7:
          if (!this.ActivityItem.IsVisible)
            break;
          goto default;
        case 8:
          if (!this.AddTemplate.IsVisible)
            break;
          goto default;
        case 9:
          if (!this.CopyItem.IsVisible)
            break;
          goto default;
        case 10:
          if (!this.CopyLinkItem.IsVisible)
            break;
          goto default;
        case 11:
          if (!this.StickyItem.IsVisible)
            break;
          goto default;
        case 12:
          if (!this.SwitchTaskNoteItem.IsVisible)
            break;
          goto default;
        case 13:
          if (!this.Print.IsVisible)
            break;
          goto default;
        case 14:
          if (this.DeletePanel.IsVisible)
            goto default;
          else
            break;
        default:
label_21:
          return true;
      }
      this.UpDownSelect(isUp);
      goto label_21;
    }

    public bool LeftRightSelect(bool isLeft)
    {
      if (this.TaskPomoPopup.IsOpen)
      {
        if (!this.TaskPomoSetDialog.IsInputFocus())
          this.TaskPomoPopup.IsOpen = false;
        return false;
      }
      if (this.SetTagPopup.IsOpen || isLeft)
        return false;
      switch (this.SelectedIndex)
      {
        case 3:
        case 6:
          this.HandleEnter();
          return true;
        default:
          return false;
      }
    }

    private void OnChildPopupOpened(object sender, EventArgs e)
    {
      if (object.Equals(sender, (object) this.SetTagPopup))
      {
        this.TagItem.HoverSelected = true;
      }
      else
      {
        if (!object.Equals(sender, (object) this.TaskPomoPopup))
          return;
        this.Pomo.HoverSelected = true;
      }
    }

    private void OnChildPopupClosed(object sender, EventArgs e)
    {
      if (object.Equals(sender, (object) this.SetTagPopup))
      {
        this.TagItem.HoverSelected = this.SelectedIndex == 3;
      }
      else
      {
        if (!object.Equals(sender, (object) this.TaskPomoPopup))
          return;
        this.Pomo.HoverSelected = this.SelectedIndex == 6;
      }
    }

    private void SetItemTabSelected()
    {
      this.TagItem.HoverSelected = this.SelectedIndex == 3 || this.SetTagPopup.IsOpen;
      this.Pomo.HoverSelected = this.SelectedIndex == 6 || this.TaskPomoPopup.IsOpen;
    }

    public void SetParent(TaskDetailView parent, EscPopup popup)
    {
      this._parent = parent;
      this._popup = popup;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/taskdetailmorecontent.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (TaskDetailMoreContent) target;
          this.Root.MouseMove += new MouseEventHandler(this.TryShowPopup);
          break;
        case 2:
          this.NewAddSubTaskItem = (OptionItemWithImageIcon) target;
          break;
        case 3:
          this.PinButton = (OptionItemWithImageIcon) target;
          break;
        case 4:
          this.AbandonedGrid = (Grid) target;
          break;
        case 5:
          this.NewTagItem = (Grid) target;
          break;
        case 6:
          this.TagItem = (OptionCheckBox) target;
          break;
        case 7:
          this.SetTagPopup = (EscPopup) target;
          break;
        case 8:
          this.BatchSetTagControl = (BatchSetTagControl) target;
          break;
        case 9:
          this.NewUploadItem = (OptionItemWithImageIcon) target;
          break;
        case 10:
          this.NewInsetSummaryItem = (OptionItemWithImageIcon) target;
          break;
        case 11:
          this.PomoItem = (Grid) target;
          break;
        case 12:
          this.Pomo = (OptionCheckBox) target;
          break;
        case 13:
          this.TaskPomoPopup = (EscPopup) target;
          break;
        case 14:
          this.TaskPomoSetDialog = (TaskPomoSetDialog) target;
          break;
        case 15:
          this.OMTopLine = (Line) target;
          break;
        case 16:
          this.ActivityItem = (OptionItemWithImageIcon) target;
          break;
        case 17:
          this.AddTemplate = (OptionItemWithImageIcon) target;
          break;
        case 18:
          this.CopyItem = (OptionItemWithImageIcon) target;
          break;
        case 19:
          this.CopyLinkItem = (OptionItemWithImageIcon) target;
          break;
        case 20:
          this.StickyItem = (OptionItemWithImageIcon) target;
          break;
        case 21:
          this.SwitchTaskNoteItem = (OptionItemWithImageIcon) target;
          break;
        case 22:
          this.Print = (OptionItemWithImageIcon) target;
          break;
        case 23:
          this.DeletePanel = (OptionItemWithImageIcon) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
