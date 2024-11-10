// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.AddTaskWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
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
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class AddTaskWindow : Window, IComponentConnector
  {
    public static readonly DependencyProperty TabSelectedIndexProperty = DependencyProperty.Register(nameof (TabSelectedIndex), typeof (int), typeof (AddTaskWindow), new PropertyMetadata((object) 0, (PropertyChangedCallback) null));
    private static AddTaskWindow _instance = new AddTaskWindow();
    private bool _popOpened;
    private AddTaskViewModel _model;
    private BatchAddTagWindow _tagSelectWindow;
    private bool _imeDown;
    internal AddTaskWindow Root;
    internal QuickAddView TaskTitle;
    internal Grid SetDateGrid;
    internal Path SetDatePath;
    internal Grid SetTagGrid;
    internal Popup SetTagPopup;
    internal Grid SetPriorityGrid;
    internal Popup SetPriorityPopup;
    internal StackPanel ProjectPanel;
    internal EmjTextBlock ProjectName;
    internal EscPopup SetProjectPopup;
    internal Button SaveButton;
    private bool _contentLoaded;

    public int TabSelectedIndex
    {
      get => (int) this.GetValue(AddTaskWindow.TabSelectedIndexProperty);
      set
      {
        if (this.TabSelectedIndex == value)
          return;
        this.SetValue(AddTaskWindow.TabSelectedIndexProperty, (object) value);
        this.OnIndexChanged();
      }
    }

    public AddTaskWindow()
    {
      this.InitializeComponent();
      this.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
      this.PreviewKeyUp += new KeyEventHandler(this.OnPreviewKeyUp);
      this.KeyUp += new KeyEventHandler(this.OnKeyUp);
      this.TaskTitle.OperationPanel.Visibility = Visibility.Collapsed;
      this.TaskTitle.TaskAdded += (EventHandler<TaskModel>) ((o, e) =>
      {
        this.Hide();
        this.Owner?.Activate();
      });
      this.TaskTitle.TitleText.EditBox.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
      this.TaskTitle.TitleText.EditBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
      this.TaskTitle.TitleText.EditBox.WordWrap = true;
      this.TaskTitle.TitleText.Width = 610.0;
    }

    private void OnPreviewKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Escape && !this._imeDown)
        this.TryClose();
      this._imeDown = false;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Tab:
          this.HandleTab();
          e.Handled = true;
          break;
        case Key.Up:
        case Key.Down:
          this.HandleUpDown(e.Key == Key.Up);
          break;
        case Key.ImeProcessed:
          this._imeDown = true;
          break;
      }
    }

    private void HandleTab()
    {
      if (this._popOpened || this.TaskTitle.IsInOperation)
        return;
      this.TabSelectedIndex = (this.TabSelectedIndex + (6 + (Utils.IfShiftPressed() ? -1 : 1))) % 6;
    }

    private void OnIndexChanged()
    {
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, this.TabSelectedIndex == 5);
      if (this.TabSelectedIndex > 0)
      {
        Keyboard.ClearFocus();
        FocusManager.SetFocusedElement((DependencyObject) this, (IInputElement) this);
        Keyboard.Focus((IInputElement) this);
      }
      else
        this.TaskTitle.FocusText();
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          if (Utils.IfCtrlPressed())
            break;
          this.HandleEnter();
          break;
        case Key.Left:
        case Key.Right:
          if (!this.SetProjectPopup.IsOpen || !(this.SetProjectPopup.Child is ProjectOrGroupPopup child))
            break;
          child.LeftRightSelect(e.Key == Key.Left);
          break;
      }
    }

    private void HandleUpDown(bool isUp)
    {
      switch (this.TabSelectedIndex)
      {
        case 3:
          if (!this.SetPriorityPopup.IsOpen || !(this.SetPriorityPopup.Child is SetPriorityDialog child1))
            break;
          child1.MoveHover(isUp);
          break;
        case 4:
          if (!this.SetProjectPopup.IsOpen || !(this.SetProjectPopup.Child is ProjectOrGroupPopup child2))
            break;
          child2.UpDownSelect(isUp);
          break;
      }
    }

    private async void HandleEnter()
    {
      switch (this.TabSelectedIndex)
      {
        case 1:
          this.ShowSetDateDialog(true);
          break;
        case 2:
          this.ShowSetTagDialog();
          break;
        case 3:
          if (this.SetPriorityPopup.IsOpen)
          {
            if (!(this.SetPriorityPopup.Child is SetPriorityDialog child))
              break;
            child.EnterSelect();
            break;
          }
          this.ShowSetPriorityDialog();
          break;
        case 4:
          if (this.SetProjectPopup.IsOpen)
          {
            if (!(this.SetProjectPopup.Child is ProjectOrGroupPopup child))
              break;
            child.EnterSelect();
            break;
          }
          this.ShowSetProjectDialog();
          break;
        case 5:
          await this.TryAddTask();
          this.TryClose();
          break;
      }
    }

    private async Task TryAddTask() => await this.TaskTitle.TryAddTask();

    private void TryClose()
    {
      if (this._popOpened || this.TaskTitle.IsInOperation)
      {
        this.SetProjectPopup.IsOpen = false;
        this.SetPriorityPopup.IsOpen = false;
      }
      else
      {
        this.Hide();
        this.Owner?.Activate();
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      this.Hide();
      this.Owner?.Activate();
      e.Cancel = true;
    }

    protected override async void OnDeactivated(EventArgs e)
    {
      AddTaskWindow addTaskWindow = this;
      // ISSUE: reference to a compiler-generated method
      addTaskWindow.\u003C\u003En__0(e);
      await Task.Delay(10);
      if (addTaskWindow._popOpened || addTaskWindow.TaskTitle.IsInOperation)
        return;
      addTaskWindow.Close();
    }

    protected override async void OnActivated(EventArgs e)
    {
      base.OnActivated(e);
      await Task.Delay(10);
      if (this.TabSelectedIndex != 0)
        return;
      this.TaskTitle.FocusText();
    }

    public static void ShowWindow(Window owner, ProjectIdentity identity)
    {
      AddTaskWindow._instance = new AddTaskWindow();
      AddTaskWindow._instance.Owner = owner;
      if (string.IsNullOrEmpty(identity.GetProjectId()))
        identity = (ProjectIdentity) ProjectIdentity.GetDefaultProject();
      AddTaskViewModel model = AddTaskViewModel.Build((IProjectTaskDefault) identity);
      AddTaskWindow._instance._model = model;
      AddTaskWindow._instance.TaskTitle.Reset(model, QuickAddView.Scenario.AddWindow);
      AddTaskWindow._instance.DataContext = (object) model;
      AddTaskWindow._instance.Show();
      AddTaskWindow._instance.TabSelectedIndex = 0;
    }

    private void SetDateClick(object sender, MouseButtonEventArgs e) => this.ShowSetDateDialog();

    private void ShowSetDateDialog(bool withTab = false)
    {
      if (this._model.TimeData == null)
        this._model.TimeData = new TimeData()
        {
          IsDefault = false
        };
      SetDateDialog dialog = SetDateDialog.GetDialog(withTab);
      dialog.ClearEventHandle();
      dialog.Save += (EventHandler<TimeData>) ((obj, data) =>
      {
        TaskService.TryFixRepeatFlag(ref data);
        this._model.TimeData = TimeData.Clone(data);
        this._model.TimeData.IsDefault = false;
        this.TaskTitle.ManualSelectedDate = true;
        if (!data.DueDate.HasValue)
          DateUtils.CheckIfTomorrowWronglySet(data);
        if (!this._model.IsCalendar || !this._model.TimeData.StartDate.HasValue)
          return;
        TimeData timeData1 = this._model.TimeData;
        bool? isAllDay;
        int num;
        if (timeData1 == null)
        {
          num = 0;
        }
        else
        {
          isAllDay = timeData1.IsAllDay;
          num = isAllDay.HasValue ? 1 : 0;
        }
        if (num == 0)
          return;
        isAllDay = this._model.TimeData.IsAllDay;
        if (!isAllDay.Value)
          return;
        TimeData timeData2 = this._model.TimeData;
        DateTime? dueDate = this._model.TimeData.DueDate;
        ref DateTime? local = ref dueDate;
        DateTime? nullable = new DateTime?(local.HasValue ? local.GetValueOrDefault().AddDays(1.0) : this._model.TimeData.StartDate.Value.AddDays(1.0));
        timeData2.DueDate = nullable;
      });
      dialog.Clear += (EventHandler) ((obj, arg) =>
      {
        if (!this._model.IsCalendar)
          this._model.TimeData = (TimeData) null;
        this.TaskTitle.ManualSelectedDate = true;
      });
      dialog.Hided += new EventHandler(this.PopupClosed);
      dialog.Show(this._model.TimeData, new SetDateDialogArgs(target: (UIElement) this.SetDateGrid, hOffset: -20.0, vOffset: 25.0, canSkip: false));
      this._popOpened = true;
    }

    private void SetTagClick(object sender, MouseButtonEventArgs e) => this.ShowSetTagDialog();

    private void ShowSetTagDialog()
    {
      BatchSetTagControl batchSetTagControl = new BatchSetTagControl();
      batchSetTagControl.Close += (EventHandler) ((s, e) => this.SetTagPopup.IsOpen = false);
      batchSetTagControl.TagsSelect += (EventHandler<TagSelectData>) ((s, tags) =>
      {
        this.TaskTitle.OnTagsAdded(tags);
        this.SetTagPopup.IsOpen = false;
      });
      this.SetTagPopup.Child = (UIElement) batchSetTagControl;
      batchSetTagControl.Init(new TagSelectData()
      {
        OmniSelectTags = new List<string>((IEnumerable<string>) this.TaskTitle.GetSelectedTags())
      }, true);
      this.SetTagPopup.IsOpen = true;
    }

    private void SetPriorityClick(object sender, MouseButtonEventArgs e)
    {
      if (this.SetPriorityPopup.IsOpen)
        this.SetPriorityPopup.IsOpen = false;
      else
        this.ShowSetPriorityDialog();
    }

    private void ShowSetPriorityDialog()
    {
      SetPriorityDialog setPriorityDialog = new SetPriorityDialog(this.SetPriorityPopup, this._model.Priority);
      setPriorityDialog.PrioritySelect += new EventHandler<int>(this.PrioritySelect);
      setPriorityDialog.Show();
    }

    private void PrioritySelect(object sender, int priority)
    {
      this.TaskTitle.SetPriority(priority);
      this.SetPriorityPopup.IsOpen = false;
    }

    private void PopupOpened(object sender, EventArgs e) => this._popOpened = true;

    private void PopupClosed(object sender, EventArgs e) => this._popOpened = false;

    private void SetProjectClick(object sender, MouseButtonEventArgs e)
    {
      this.ShowSetProjectDialog();
    }

    private async void ShowSetProjectDialog()
    {
      AddTaskWindow addTaskWindow = this;
      addTaskWindow.TaskTitle.IsInOperation = true;
      EscPopup setProjectPopup = addTaskWindow.SetProjectPopup;
      ProjectExtra data = new ProjectExtra();
      List<string> stringList;
      if (!addTaskWindow._model.SelectProject)
      {
        stringList = new List<string>();
      }
      else
      {
        stringList = new List<string>();
        stringList.Add(addTaskWindow._model.ProjectId);
      }
      data.ProjectIds = stringList;
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) setProjectPopup, data, new ProjectSelectorExtra()
      {
        showAll = false,
        batchMode = false,
        canSelectGroup = false,
        CanSearch = true,
        onlyShowPermission = true,
        ShowColumn = true,
        ColumnId = addTaskWindow._model.GetProjectColumnId()
      });
      projectOrGroupPopup.ItemSelect += new EventHandler<SelectableItemViewModel>(addTaskWindow.OnProjectSelect);
      // ISSUE: reference to a compiler-generated method
      projectOrGroupPopup.Closed += new EventHandler(addTaskWindow.\u003CShowSetProjectDialog\u003Eb__33_0);
      projectOrGroupPopup.Show();
    }

    private void OnProjectSelect(object sender, SelectableItemViewModel e)
    {
      string projectId = e.GetProjectAndColumnId().Item1;
      if (CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId)) == null)
        return;
      this.TaskTitle.OnProjectSelect((object) this, e);
      this.SetProjectPopup.IsOpen = false;
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      await this.TryAddTask();
      this.TryClose();
    }

    private void OnTitleGotFocus(object sender, RoutedEventArgs e) => this.TabSelectedIndex = 0;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/quickadd/addtaskwindow.xaml", UriKind.Relative));
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
          this.Root = (AddTaskWindow) target;
          break;
        case 2:
          this.TaskTitle = (QuickAddView) target;
          break;
        case 3:
          this.SetDateGrid = (Grid) target;
          break;
        case 4:
          this.SetDatePath = (Path) target;
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SetDateClick);
          break;
        case 6:
          this.SetTagGrid = (Grid) target;
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SetTagClick);
          break;
        case 8:
          this.SetTagPopup = (Popup) target;
          this.SetTagPopup.Opened += new EventHandler(this.PopupOpened);
          this.SetTagPopup.Closed += new EventHandler(this.PopupClosed);
          break;
        case 9:
          this.SetPriorityGrid = (Grid) target;
          break;
        case 10:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SetPriorityClick);
          break;
        case 11:
          this.SetPriorityPopup = (Popup) target;
          this.SetPriorityPopup.Opened += new EventHandler(this.PopupOpened);
          this.SetPriorityPopup.Closed += new EventHandler(this.PopupClosed);
          break;
        case 12:
          this.ProjectPanel = (StackPanel) target;
          this.ProjectPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetProjectClick);
          break;
        case 13:
          this.ProjectName = (EmjTextBlock) target;
          break;
        case 14:
          this.SetProjectPopup = (EscPopup) target;
          break;
        case 15:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
