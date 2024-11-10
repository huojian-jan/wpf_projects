// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.AddOptionDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Template;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class AddOptionDialog : UserControl, ITabControl, IComponentConnector
  {
    public static readonly DependencyProperty PriorityProperty = DependencyProperty.Register(nameof (Priority), typeof (int), typeof (AddOptionDialog), new PropertyMetadata((object) -1, (PropertyChangedCallback) null));
    public static readonly DependencyProperty TabSelectedIndexProperty = DependencyProperty.Register(nameof (TabSelectedIndex), typeof (int), typeof (AddOptionDialog), new PropertyMetadata((object) -1, (PropertyChangedCallback) null));
    private bool _isNote;
    private BatchSetTagControl _batchSetTagCtrl;
    private AddTaskViewModel _model;
    private ProjectOrGroupPopup _projectPopup;
    private SetAssigneeDialog _assignDialog;
    private bool _assignPopupShow;
    private bool _setProjectPopupShow;
    private bool _tagPopupShow;
    private readonly PopupLocationInfo _projectPopupTracker = new PopupLocationInfo();
    private readonly PopupLocationInfo _tagPopupTracker = new PopupLocationInfo();
    private readonly PopupLocationInfo _assignPopupTracker = new PopupLocationInfo();
    private readonly List<PopupLocationInfo> _popupTrackers = new List<PopupLocationInfo>();
    private List<EscPopup> _popups = new List<EscPopup>();
    internal AddOptionDialog Root;
    internal Grid PriorityGrid;
    internal Grid MoveButton;
    internal OptionCheckBox ProjectButton;
    internal EscPopup SetProjectPopup;
    internal Grid AssignBtn;
    internal EscPopup SetAssigneePopup;
    internal Grid TagGrid;
    internal EscPopup SetTagPopup;
    internal OptionCheckBox AddTemplateButton;
    private bool _contentLoaded;

    public int Priority
    {
      get => (int) this.GetValue(AddOptionDialog.PriorityProperty);
      set => this.SetValue(AddOptionDialog.PriorityProperty, (object) value);
    }

    public int TabSelectedIndex
    {
      get => (int) this.GetValue(AddOptionDialog.TabSelectedIndexProperty);
      set => this.SetValue(AddOptionDialog.TabSelectedIndexProperty, (object) value);
    }

    public AddOptionDialog() => this.InitializeComponent();

    public QuickAddView QuickAddView { private get; set; }

    private void PriorityGridClick(object sender, MouseButtonEventArgs e)
    {
      this.QuickAddView.HideOptionDialog(true);
      if (sender is FrameworkElement frameworkElement)
        this.QuickAddView.SetPriority(int.Parse(frameworkElement.Tag.ToString()));
      e.Handled = true;
    }

    public void Init(AddTaskViewModel model, bool showTab)
    {
      this._model = model;
      this.InitProject(model.ProjectId, model.IsNote);
      this.InitPriority(model.Priority);
      this.TabSelectedIndex = showTab ? (this.PriorityGrid.Visibility == Visibility.Visible ? 0 : 4) : -1;
      this._popups.Add(this.SetProjectPopup);
      this._popups.Add(this.SetTagPopup);
      this._popups.Add(this.SetAssigneePopup);
      this._popupTrackers.Add(this._projectPopupTracker);
      this._popupTrackers.Add(this._tagPopupTracker);
      this._popupTrackers.Add(this._assignPopupTracker);
    }

    private void TryCloseOtherPopups(EscPopup popup)
    {
      foreach (EscPopup escPopup in this._popups.Where<EscPopup>((Func<EscPopup, bool>) (escPopup => escPopup != popup)))
        escPopup.Close();
    }

    private void InitProject(string projectId, bool isNote)
    {
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
      if (projectModel == null)
        return;
      this.ProjectButton.Text = projectModel.name;
      this.ProjectButton.TitleText.MaxWidth = 160.0;
      this.AssignBtn.Visibility = projectModel.IsShareList() ? Visibility.Visible : Visibility.Collapsed;
      this._isNote = projectModel.IsNote | isNote;
      this.PriorityGrid.Visibility = this._isNote ? Visibility.Collapsed : Visibility.Visible;
    }

    private void InitPriority(int priority) => this.Priority = priority;

    private async void OnAddFromTemplateClick(object sender, EventArgs e)
    {
      this.ShowAddFromTemplateDialog(this._isNote ? TemplateKind.Note : TemplateKind.Task);
    }

    private void ShowAddFromTemplateDialog(TemplateKind kind = TemplateKind.Task)
    {
      AddTemplateDialog addTemplateDialog = new AddTemplateDialog(kind, this._model, Utils.FindParent<ListViewContainer>((DependencyObject) this.QuickAddView));
      addTemplateDialog.Owner = Window.GetWindow((DependencyObject) this);
      addTemplateDialog.TemplateSelected -= new EventHandler<string>(this.OnTemplateSelected);
      addTemplateDialog.TemplateSelected += new EventHandler<string>(this.OnTemplateSelected);
      addTemplateDialog.ShowDialog();
      addTemplateDialog.Activate();
      addTemplateDialog.Topmost = true;
      this.QuickAddView?.TryFocus();
    }

    private async void OnTemplateSelected(object sender, string templateId)
    {
      if (await ProChecker.CheckTaskLimit(this.QuickAddView.GetTaskAddModel().ProjectId) || string.IsNullOrEmpty(templateId) || this.QuickAddView == null)
        return;
      TaskModel task = await TaskService.AddTaskFromTemplate(templateId, this.QuickAddView.GetTaskAddModel(), this.QuickAddView.GetExtra());
      if (task != null)
      {
        this.QuickAddView.NotifyAddTask(task);
        await Task.Delay(200);
        Utils.FindParent<TaskView>((DependencyObject) this.QuickAddView)?.GetTaskDetail()?.TryFocusDetail(taskId: task.id);
      }
      task = (TaskModel) null;
    }

    private void ShowPopup(object sender, MouseEventArgs e)
    {
      if (this._popupTrackers.Any<PopupLocationInfo>((Func<PopupLocationInfo, bool>) (tracker => tracker.IsSafeShowing())))
        return;
      this.TryShowAssignPopup();
      this.TryShowSetProjectPopup();
      this.TryShowTagPopup();
    }

    private void TryShowTagPopup()
    {
      if (this.SetTagPopup.IsOpen)
        this._tagPopupTracker.Mark();
      if (!this.TagGrid.IsMouseOver && !this.SetTagPopup.IsMouseOver)
      {
        this._tagPopupShow = false;
        this.SetTagPopup.IsOpen = false;
      }
      else
      {
        if (this._tagPopupShow)
          return;
        this._tagPopupShow = true;
        this.DelayShowTagPopup();
      }
    }

    private async Task DelayShowTagPopup()
    {
      AddOptionDialog addOptionDialog = this;
      bool isFirst = addOptionDialog._batchSetTagCtrl == null;
      if (isFirst)
      {
        addOptionDialog._batchSetTagCtrl = new BatchSetTagControl();
        addOptionDialog._batchSetTagCtrl.Close += new EventHandler(addOptionDialog.OnBatchSetTagClosed);
        // ISSUE: reference to a compiler-generated method
        addOptionDialog._batchSetTagCtrl.TagsSelect += new EventHandler<TagSelectData>(addOptionDialog.\u003CDelayShowTagPopup\u003Eb__36_0);
        addOptionDialog.SetTagPopup.Child = (UIElement) addOptionDialog._batchSetTagCtrl;
      }
      await Task.Delay(150);
      if (!addOptionDialog._tagPopupShow)
        return;
      BatchSetTagControl batchSetTagCtrl = addOptionDialog._batchSetTagCtrl;
      TagSelectData tags = new TagSelectData();
      tags.OmniSelectTags = new List<string>((IEnumerable<string>) addOptionDialog.QuickAddView.GetSelectedTags());
      int num = isFirst ? 1 : 0;
      batchSetTagCtrl.Init(tags, num != 0);
      addOptionDialog.SetTagPopup.IsOpen = true;
      addOptionDialog._tagPopupTracker.Bind((Popup) addOptionDialog.SetTagPopup);
      addOptionDialog.TryCloseOtherPopups(addOptionDialog.SetTagPopup);
    }

    private void OnBatchSetTagClosed(object sender, EventArgs e) => this.SetTagPopup.IsOpen = false;

    private void TryShowAssignPopup()
    {
      if (this.SetAssigneePopup.IsOpen)
        this._assignPopupTracker.Mark();
      if (!this.AssignBtn.IsMouseOver && !this.SetAssigneePopup.IsMouseOver)
      {
        this._assignPopupShow = false;
        this.SetAssigneePopup.IsOpen = false;
      }
      else
      {
        if (this._assignPopupShow)
          return;
        this._assignPopupShow = true;
        this.DelayShowAssignPopup();
      }
    }

    private async void DelayShowAssignPopup()
    {
      await Task.Delay(150);
      if (!this._assignPopupShow)
        return;
      this.ShowAssignPopup();
    }

    private void TryShowSetProjectPopup()
    {
      if (this.SetProjectPopup.IsOpen)
        this._projectPopupTracker.Mark();
      bool flag = this._projectPopup != null && this._projectPopup.ChildMouseOver();
      if (!this.MoveButton.IsMouseOver && !this.SetProjectPopup.IsMouseOver && !flag)
      {
        this._setProjectPopupShow = false;
        this._projectPopup?.ClosePopup();
        this.SetProjectPopup.IsOpen = false;
      }
      else
      {
        if (this._setProjectPopupShow)
          return;
        this._setProjectPopupShow = true;
        this.DelayShowSetProjectPopup();
      }
    }

    private async void DelayShowSetProjectPopup()
    {
      await Task.Delay(150);
      if (!this._setProjectPopupShow)
        return;
      this.ShowSelectProjectPopup();
    }

    private void ShowAssignPopup()
    {
      if (this._assignDialog == null)
      {
        this._assignDialog = new SetAssigneeDialog(this._model.ProjectId, (Popup) this.SetAssigneePopup, this.QuickAddView?.Avatar?.UserId);
        this._assignDialog.AssigneeSelect += new EventHandler<AvatarInfo>(this.OnAssigneeSelect);
      }
      else
        this._assignDialog.SetItems(this._model.ProjectId, this.QuickAddView?.Avatar?.UserId);
      this._assignDialog.Move(false);
      this._assignDialog.Show();
      this._assignPopupTracker.Bind((Popup) this.SetAssigneePopup);
      this.TryCloseOtherPopups(this.SetAssigneePopup);
    }

    private void OnAssigneeSelect(object sender, AvatarInfo model)
    {
      this.SetAssigneePopup.IsOpen = false;
      this.QuickAddView.HideOptionDialog(true);
      this.QuickAddView.OnAssigneeSelect(model);
    }

    private void ShowSelectProjectPopup(bool enter = false)
    {
      if (this._projectPopup == null)
      {
        this._projectPopup = new ProjectOrGroupPopup((Popup) this.SetProjectPopup, new ProjectExtra()
        {
          ProjectIds = new List<string>()
          {
            this._model.ProjectId
          }
        }, new ProjectSelectorExtra()
        {
          showAll = false,
          batchMode = false,
          canSelectGroup = false,
          onlyShowPermission = true,
          ShowColumn = true,
          ColumnId = this._model.GetProjectColumnId()
        });
        this._projectPopup.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnProjectSelect);
      }
      else
      {
        ProjectOrGroupPopup projectPopup = this._projectPopup;
        ProjectExtra data = new ProjectExtra();
        data.ProjectIds = new List<string>()
        {
          this._model.ProjectId
        };
        string projectColumnId = this._model.GetProjectColumnId();
        projectPopup.SetData(data, projectColumnId);
      }
      if (enter)
        this._projectPopup.HoverSelectFirst();
      this._projectPopup.Show();
      this._projectPopupTracker.Bind((Popup) this.SetProjectPopup);
      this.TryCloseOtherPopups(this.SetProjectPopup);
    }

    private void OnProjectSelect(object sender, SelectableItemViewModel e)
    {
      this.SetProjectPopup.IsOpen = false;
      this.QuickAddView.HideOptionDialog(true);
      this.QuickAddView.OnProjectSelect(sender, e);
    }

    public bool HandleTab(bool shift)
    {
      if (this.SetTagPopup.IsOpen)
      {
        this._batchSetTagCtrl?.HandleTab(shift);
        return false;
      }
      if (this.SetProjectPopup.IsOpen)
      {
        this._projectPopup?.UpDownSelect(shift);
        return false;
      }
      if (this.SetAssigneePopup.IsOpen)
      {
        this.SetAssigneePopup.HandleTab(shift);
        return false;
      }
      int num = (this.TabSelectedIndex + (8 + (shift ? -1 : 1))) % 8;
      if (num < 4 && this.PriorityGrid.Visibility != Visibility.Visible)
      {
        this.TabSelectedIndex = num;
        return this.HandleTab(shift);
      }
      if (num == 5 && !this.AssignBtn.IsVisible)
      {
        this.TabSelectedIndex = num;
        return this.HandleTab(shift);
      }
      this.TabSelectedIndex = num;
      return true;
    }

    public bool HandleEnter()
    {
      if (this.TabSelectedIndex >= 0 && this.TabSelectedIndex <= 3)
      {
        this.QuickAddView.HideOptionDialog(true);
        this.QuickAddView.SetPriority(new int[4]
        {
          5,
          3,
          1,
          0
        }[this.TabSelectedIndex]);
        return true;
      }
      if (this.TabSelectedIndex == 4 && !this.SetProjectPopup.IsOpen)
        this.ShowSelectProjectPopup(true);
      if (this.TabSelectedIndex == 5)
      {
        if (this.SetAssigneePopup.IsOpen)
          this._assignDialog?.EnterSelect();
        else
          this.ShowAssignPopup();
        return true;
      }
      if (this.TabSelectedIndex == 6 && !this.SetTagPopup.IsOpen)
      {
        this._tagPopupShow = true;
        this.DelayShowTagPopup();
      }
      if (this.TabSelectedIndex == 7)
        this.ShowAddFromTemplateDialog(this._isNote ? TemplateKind.Note : TemplateKind.Task);
      return true;
    }

    public bool HandleEsc() => false;

    public bool UpDownSelect(bool isUp)
    {
      if (this.SetProjectPopup.IsOpen)
      {
        this._projectPopup?.UpDownSelect(isUp);
        return true;
      }
      if (this.SetTagPopup.IsOpen)
        return false;
      if (!this.SetAssigneePopup.IsOpen)
        return this.HandleTab(isUp);
      this._assignDialog?.Move(isUp);
      return true;
    }

    public bool LeftRightSelect(bool isLeft)
    {
      if (this.TabSelectedIndex >= 0 && this.TabSelectedIndex <= 3)
        return this.HandleTab(isLeft);
      if (this.TabSelectedIndex >= 4 && this.TabSelectedIndex <= 6)
      {
        if (this.SetProjectPopup.IsOpen)
        {
          ProjectOrGroupPopup projectPopup = this._projectPopup;
          // ISSUE: explicit non-virtual call
          if ((projectPopup != null ? (__nonvirtual (projectPopup.LeftRightSelect(isLeft)) ? 1 : 0) : 0) != 0)
            return true;
          if (isLeft)
          {
            this.SetProjectPopup.IsOpen = false;
            return true;
          }
        }
        if (this.SetTagPopup.IsOpen)
          return false;
        if (!isLeft)
        {
          if (!this.SetAssigneePopup.IsOpen)
            this.HandleEnter();
        }
        else if (this.SetAssigneePopup.IsOpen)
          this.SetAssigneePopup.IsOpen = false;
      }
      return true;
    }

    private void OnAssignMouseEnter(object sender, MouseEventArgs e)
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/quickadd/addoptiondialog.xaml", UriKind.Relative));
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
          this.Root = (AddOptionDialog) target;
          break;
        case 2:
          ((UIElement) target).MouseMove += new MouseEventHandler(this.ShowPopup);
          break;
        case 3:
          this.PriorityGrid = (Grid) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityGridClick);
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityGridClick);
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityGridClick);
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityGridClick);
          break;
        case 8:
          this.MoveButton = (Grid) target;
          break;
        case 9:
          this.ProjectButton = (OptionCheckBox) target;
          break;
        case 10:
          this.SetProjectPopup = (EscPopup) target;
          break;
        case 11:
          this.AssignBtn = (Grid) target;
          this.AssignBtn.MouseEnter += new MouseEventHandler(this.OnAssignMouseEnter);
          break;
        case 12:
          this.SetAssigneePopup = (EscPopup) target;
          break;
        case 13:
          this.TagGrid = (Grid) target;
          break;
        case 14:
          this.SetTagPopup = (EscPopup) target;
          break;
        case 15:
          this.AddTemplateButton = (OptionCheckBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
