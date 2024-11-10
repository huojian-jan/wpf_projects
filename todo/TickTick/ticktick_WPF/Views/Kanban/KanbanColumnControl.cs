// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.KanbanColumnControl
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
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Kanban.Item;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Kanban
{
  public class KanbanColumnControl : Grid, IKanbanColumn, IComponentConnector
  {
    private IKanban _kanban;
    private bool _nameHandled;
    private string _originalColumnName;
    private QuickAddView _quickAddView;
    private bool _setting;
    internal Border DragArea;
    internal StackPanel NameTextBlock;
    internal Border EditNameText;
    internal TextBox ColumnNameText;
    internal Popup ErrorPopup;
    internal Border MoreGrid;
    internal EscPopup MorePopup;
    internal Border QuickAddGrid;
    internal TaskListView TaskList;
    internal Button SaveButton;
    private bool _contentLoaded;

    public KanbanColumnControl()
    {
      this.InitializeComponent();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataChanged);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    private void OnDataChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is ColumnViewModel oldValue)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnNotifyReload), "Reload");
      if (!(e.NewValue is ColumnViewModel newValue))
        return;
      if (newValue.Identity != null)
      {
        this.TaskList.SetIdentity((ProjectIdentity) newValue.Identity, TaskListDisplayType.Kanban);
        this.TaskList.SortItemsAndLoad(newValue.SourceItems);
      }
      newValue.SetTaskListViewModel(this.TaskList.ViewModel);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnNotifyReload), "Reload");
    }

    private void OnNotifyReload(object sender, PropertyChangedEventArgs e)
    {
      this.TaskList.LoadAsync();
    }

    public string GetColumnId() => this.GetModel()?.ColumnId;

    public void ReloadColumn() => this.TaskList.ReLoad((string) null);

    public List<DisplayItemModel> RemoveTasks(List<string> selected, string except)
    {
      return this.TaskList.ViewModel.RemoveItemByIds(selected, except);
    }

    public void RemoveTaskChildren(DisplayItemModel model)
    {
      TaskListViewModel viewModel = this.TaskList.ViewModel;
      List<string> taskIds = new List<string>();
      taskIds.Add(model.Id);
      string id = model.Id;
      viewModel.RemoveItemByIds(taskIds, id);
    }

    public bool TaskChanged { get; set; }

    public void RegisterEvents(IKanban kanban, bool resetQuickAdd = false)
    {
      this.UnbindEvents();
      this._kanban = kanban;
      this.BindEvents();
      this.InitView(resetQuickAdd: resetQuickAdd);
      this.RegisterItem();
      this.TaskList.ViewModel.SetKanbanParent(kanban.GetViewModel());
    }

    private void RegisterItem()
    {
    }

    private void UnbindEvents()
    {
      this.TaskList.ItemsCountChanged -= new EventHandler(this.OnTaskCountChanged);
      ItemDragNotifier.MouseMove -= new EventHandler<MouseEventArgs>(this.OnDragMouseMove);
      IKanban kanban = this.GetKanban();
      if (kanban != null)
        kanban.SetSelected -= new EventHandler<List<string>>(this.OnSetSelected);
      this._kanban = (IKanban) null;
    }

    private void BindEvents()
    {
      this.TaskList.ItemsCountChanged += new EventHandler(this.OnTaskCountChanged);
      ItemDragNotifier.MouseMove += new EventHandler<MouseEventArgs>(this.OnDragMouseMove);
      IKanban kanban = this.GetKanban();
      if (kanban == null)
        return;
      kanban.SetSelected += new EventHandler<List<string>>(this.OnSetSelected);
    }

    private void OnSetSelected(object sender, List<string> e) => this.TaskList.SetSelected(e);

    private async void OnTaskCountChanged(object sender, EventArgs e)
    {
      await Task.Delay(100);
      ColumnViewModel model = this.GetModel();
      if (model == null)
        return;
      model.TaskCount = this.TaskList.ViewModel.SourceModels.Count<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (i => i.IsTaskOrNote && i.Status == 0));
    }

    public async Task OnBatchTaskDrop(DisplayItemModel dragModel)
    {
      await this.TaskList.OnBatchTaskDrop(dragModel);
    }

    private void OnDragMouseMove(object sender, MouseEventArgs e)
    {
      ColumnViewModel model = this.GetModel();
      if (model == null)
        return;
      DisplayItemModel draggingTask = this.GetKanban()?.GetDraggingTask();
      if (draggingTask == null || draggingTask.IsNote && model.ColumnId.StartsWith("priority:"))
        return;
      double x = e.GetPosition((IInputElement) this).X;
      if ((x < 0.0 ? 0 : (x <= this.ActualWidth ? 1 : 0)) != 0)
      {
        if (draggingTask.ColumnId != model.ColumnId)
        {
          model.CanDrop = true;
        }
        else
        {
          model.CanDrop = false;
          this.TaskList.OnKanbanItemDragMove(draggingTask, e);
        }
        model.MouseOver = true;
      }
      else
      {
        model.MouseOver = false;
        model.CanDrop = false;
      }
    }

    public void TryFocus() => this.ColumnNameText.Focus();

    private void InitView(bool focus = false, bool resetQuickAdd = false)
    {
      ColumnViewModel model = this.GetModel();
      if (model == null)
        return;
      this._nameHandled = false;
      this._originalColumnName = model.Name;
      if (this._quickAddView == null)
      {
        if (model.CanAdd && !this.InitQuickAddView((ProjectIdentity) model.Identity, model.ColumnId, focus, 0))
          return;
      }
      else if (resetQuickAdd || this._quickAddView.Model.ColumnId != model.ColumnId || string.IsNullOrEmpty(this._quickAddView.TitleText.EditBox.Text))
        this._quickAddView.Reset(AddTaskViewModel.Build((IProjectTaskDefault) new ColumnProjectIdentity((ProjectIdentity) model.Identity, model.ColumnId)), QuickAddView.Scenario.Kanban, focus);
      this.SetNewName(model.NewAdd);
    }

    public void ResetQuickAddView(bool keepText = true)
    {
      if (!this.IsVisible)
        return;
      ColumnViewModel model = this.GetModel();
      if (model?.Identity == null)
        return;
      this._quickAddView?.ResetView((IProjectTaskDefault) model.Identity, keepText: keepText);
    }

    private bool InitQuickAddView(
      ProjectIdentity identity,
      string columnId,
      bool focus,
      int tryTimes)
    {
      if (tryTimes >= 5)
        return false;
      try
      {
        this._quickAddView = new QuickAddView((IProjectTaskDefault) new ColumnProjectIdentity(identity, columnId), QuickAddView.Scenario.Kanban, focus: focus);
        this._quickAddView.TaskAdded -= new EventHandler<TaskModel>(this.OnTaskAdded);
        this._quickAddView.TaskAdded += new EventHandler<TaskModel>(this.OnTaskAdded);
        this._quickAddView.BatchTaskAdded -= new EventHandler<List<TaskModel>>(this.OnBatchTaskAdded);
        this._quickAddView.BatchTaskAdded += new EventHandler<List<TaskModel>>(this.OnBatchTaskAdded);
        this.QuickAddGrid.Child = (UIElement) this._quickAddView;
        return true;
      }
      catch (Exception ex)
      {
        return this.InitQuickAddView(identity, columnId, focus, ++tryTimes);
      }
    }

    private void OnBatchTaskAdded(object sender, List<TaskModel> tasks)
    {
      SyncManager.TryDelaySync();
    }

    public async void OnTaskDrop(DisplayItemModel model)
    {
      ColumnViewModel model1 = this.GetModel();
      if (model1 == null || !(model.ColumnId == model1.ColumnId))
        return;
      await this.TaskList.OnTaskDrop(model);
      this.ReloadColumn();
    }

    private void OnTaskAdded(object sender, TaskModel model)
    {
      this.GetKanban()?.CancelOperation();
      if (model.projectId != this.GetModel().ProjectId)
      {
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.projectId));
        if (projectModel != null)
          Utils.Toast(string.Format(Utils.GetString("ToastTextWithoutDate"), (object) projectModel.name));
      }
      ColumnViewModel model1 = this.GetModel();
      if (model1 == null)
        return;
      model1.CanDrop = false;
      if (!(model.projectId == model1.ProjectId) || !(model.columnId == model1.ColumnId) && (!model1.IsPinned || model.pinnedTimeStamp <= 0L))
        return;
      ++model1.TaskCount;
    }

    private void MoreGridClick(object sender, MouseButtonEventArgs e)
    {
      ColumnViewModel model = this.GetModel();
      if ((model != null ? (model.Enable ? 1 : 0) : 1) == 0)
        return;
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "rename", Utils.GetString("Rename"), Utils.GetImageSource("EditDrawingImage")),
        new CustomMenuItemViewModel((object) "addToLeft", Utils.GetString("AddColumnToTheLeft"), Utils.GetImageSource("AddToLeftDrawingImage")),
        new CustomMenuItemViewModel((object) "addToRight", Utils.GetString("AddColumnToTheRight"), Utils.GetImageSource("AddToRightDrawingImage"))
      };
      IKanban kanban = this.GetKanban();
      if ((kanban != null ? (kanban.GetColumnCount() > 1 ? 1 : 0) : 0) != 0)
        types.Add(new CustomMenuItemViewModel((object) "delete", Utils.GetString("Delete"), Utils.GetImageSource("DeleteDrawingLine")));
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this.MorePopup);
      customMenuList.Operated += new EventHandler<object>(this.OnMoreItemSelected);
      customMenuList.Show();
    }

    private void OnMoreItemSelected(object sender, object e)
    {
      if (!(e is string str))
        return;
      switch (str)
      {
        case "rename":
          this.SetNewName(true);
          break;
        case "addToLeft":
          this.GetKanban().AddColumn(this.GetModel()?.ColumnId, HorizontalDirection.Left);
          break;
        case "addToRight":
          this.GetKanban().AddColumn(this.GetModel()?.ColumnId, HorizontalDirection.Right);
          break;
        case "delete":
          this.GetKanban().DeleteColumn(this.GetModel()?.ColumnId, true);
          break;
      }
    }

    private async void SetNewName(bool show)
    {
      if (show)
      {
        this.EditNameText.Visibility = Visibility.Visible;
        this.NameTextBlock.Visibility = Visibility.Collapsed;
        await Task.Delay(100);
        this.ColumnNameText.Visibility = Visibility.Visible;
        this.ColumnNameText.SelectAll();
        this.ColumnNameText.Focus();
      }
      else
      {
        this.EditNameText.Visibility = Visibility.Collapsed;
        this.ColumnNameText.Visibility = Visibility.Collapsed;
        this.NameTextBlock.Visibility = Visibility.Visible;
      }
    }

    public ColumnViewModel GetModel()
    {
      return this.DataContext != null && this.DataContext is ColumnViewModel dataContext ? dataContext : (ColumnViewModel) null;
    }

    private IKanban GetKanban() => this._kanban;

    private void OnNameGotFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
      ColumnViewModel model = this.GetModel();
      if (model == null)
        return;
      model.Editing = true;
    }

    private async void OnNameLostFocus(object sender, RoutedEventArgs e)
    {
      KanbanColumnControl kanbanColumnControl = this;
      ColumnViewModel model = kanbanColumnControl.GetModel();
      if (model == null)
        return;
      // ISSUE: explicit non-virtual call
      if (__nonvirtual (kanbanColumnControl.IsMouseOver))
        kanbanColumnControl.ColumnNameText.Focus();
      model.Editing = false;
      // ISSUE: explicit non-virtual call
      if (kanbanColumnControl._nameHandled || model.NewAdd && __nonvirtual (kanbanColumnControl.IsMouseOver))
        return;
      if (!model.NewAdd || !string.IsNullOrEmpty(kanbanColumnControl.ColumnNameText.Text))
      {
        await kanbanColumnControl.SaveColumnName();
      }
      else
      {
        // ISSUE: explicit non-virtual call
        if (!string.IsNullOrEmpty(kanbanColumnControl.ColumnNameText.Text) || __nonvirtual (kanbanColumnControl.IsMouseOver))
          return;
        kanbanColumnControl.GetKanban();
      }
    }

    private async Task SaveColumnName()
    {
      if (this.EditNameText.Visibility != Visibility.Visible)
        return;
      string saveName = this.ColumnNameText.Text;
      if (!NameUtils.IsValidColumnName(saveName))
      {
        this.ColumnNameText.SelectAll();
        this.ErrorPopup.IsOpen = true;
      }
      else if (string.IsNullOrEmpty(saveName))
      {
        this.ColumnNameText.Text = this._originalColumnName;
        this.GetModel().Name = this._originalColumnName;
        this.EditNameText.Visibility = Visibility.Collapsed;
        this.NameTextBlock.Visibility = Visibility.Visible;
        Utils.Toast(Utils.GetString("SectionNameCannotBeEmpty"));
        int num = this.GetModel().NewAdd ? 1 : 0;
      }
      else if (await this.CheckIfColumnNameExisted(saveName))
      {
        this.ColumnNameText.Text = this._originalColumnName;
        this.GetModel().Name = this._originalColumnName;
        this.EditNameText.Visibility = Visibility.Collapsed;
        this.NameTextBlock.Visibility = Visibility.Visible;
        Utils.Toast(Utils.GetString("SectionNameExisted"));
        int num = this.GetModel().NewAdd ? 1 : 0;
      }
      else
      {
        ColumnViewModel model = this.GetModel();
        if (model.NewAdd)
        {
          ColumnModel columnModel = await ColumnDao.AddColumn(saveName, model.ProjectId, model.SortOrder);
        }
        else
          await ColumnDao.SaveColumnName(saveName, model.ColumnId, model.NewAdd);
        this._originalColumnName = saveName;
        this.GetModel().Name = saveName;
        this.EditNameText.Visibility = Visibility.Collapsed;
        this.NameTextBlock.Visibility = Visibility.Visible;
        if (model.NewAdd)
        {
          model.Editing = false;
          this.GetKanban().Reload();
        }
        SyncManager.TryDelaySync();
        saveName = (string) null;
        model = (ColumnViewModel) null;
      }
    }

    private async Task<bool> CheckIfColumnNameExisted(string name)
    {
      return !(name == this._originalColumnName) && this.GetKanban() != null && (await this.GetKanban().GetColumnNames()).Contains(name);
    }

    private void OnColumnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return && e.Key != Key.Escape)
        return;
      this.SaveColumnName();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      this._nameHandled = true;
      this.SaveColumnName();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      this._nameHandled = true;
      this.GetKanban();
    }

    private void OnNameTextChanged(object sender, TextChangedEventArgs e)
    {
      string str = this.ColumnNameText.Text.TrimEnd();
      this.SaveButton.IsEnabled = NameUtils.IsValidColumnName(this.ColumnNameText.Text) && !string.IsNullOrEmpty(str.Trim());
      if (string.IsNullOrEmpty(str))
        return;
      this.CheckIfColumnNameValid();
    }

    private void CheckIfColumnNameValid()
    {
      if (!NameUtils.IsValidColumnName(this.ColumnNameText.Text))
      {
        this.ErrorPopup.IsOpen = true;
        this.ColumnNameText.SelectAll();
      }
      else
        this.ErrorPopup.IsOpen = false;
    }

    public bool QuickAddPopupOpened()
    {
      QuickAddView quickAddView = this._quickAddView;
      return quickAddView != null && quickAddView.IsInOperation;
    }

    public async void TryShowTaskDetail(string taskModelId, bool focusTitle = false)
    {
      this.GetModel();
      if (this._kanban == null)
        ;
      else
      {
        await Task.Delay(100);
        DisplayItemModel item = this.TaskList.ViewModel.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.Id == taskModelId));
        if (item != null)
        {
          KanbanItemView singleVisualChildren = Utils.FindSingleVisualChildren<KanbanItemView>((DependencyObject) this.TaskList.GetItemByModel(item));
          if (singleVisualChildren != null)
          {
            singleVisualChildren.SelectItem(focusTitle);
            return;
          }
          this.TaskList.ScrollToItemById(item.Id);
          await Task.Delay(100);
          Utils.FindSingleVisualChildren<KanbanItemView>((DependencyObject) this.TaskList.GetItemByModel(item))?.SelectItem(focusTitle);
        }
        item = (DisplayItemModel) null;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/kanban/kanbancolumncontrol.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseRightButtonUp += new MouseButtonEventHandler(this.MoreGridClick);
          break;
        case 2:
          this.DragArea = (Border) target;
          break;
        case 3:
          this.NameTextBlock = (StackPanel) target;
          break;
        case 4:
          this.EditNameText = (Border) target;
          break;
        case 5:
          this.ColumnNameText = (TextBox) target;
          this.ColumnNameText.TextChanged += new TextChangedEventHandler(this.OnNameTextChanged);
          this.ColumnNameText.GotKeyboardFocus += new KeyboardFocusChangedEventHandler(this.OnNameGotFocus);
          this.ColumnNameText.LostKeyboardFocus += new KeyboardFocusChangedEventHandler(this.OnNameLostFocus);
          this.ColumnNameText.KeyUp += new KeyEventHandler(this.OnColumnKeyUp);
          break;
        case 6:
          this.ErrorPopup = (Popup) target;
          break;
        case 7:
          this.MoreGrid = (Border) target;
          this.MoreGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.MoreGridClick);
          break;
        case 8:
          this.MorePopup = (EscPopup) target;
          break;
        case 9:
          this.QuickAddGrid = (Border) target;
          break;
        case 10:
          this.TaskList = (TaskListView) target;
          break;
        case 11:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 12:
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
