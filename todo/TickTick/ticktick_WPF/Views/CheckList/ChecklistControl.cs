// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CheckList.ChecklistControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
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
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Event;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.CheckList
{
  public class ChecklistControl : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty InQuickAddWindowProperty = DependencyProperty.Register(nameof (InQuickAddWindow), typeof (bool), typeof (ChecklistControl), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    private bool _dragging;
    private System.Windows.Point _lastHoverIndex = new System.Windows.Point(-1.0, -1.0);
    private Popup _popup;
    public bool IsNewAddTask;
    private Window _window;
    private TaskDetailView _parentControl;
    private bool _inSticky;
    private string _uid;
    private bool _isStickyDark;
    internal ItemsControl CheckListView;
    internal Popup TaskDragPopup;
    private bool _contentLoaded;

    public ChecklistControl()
    {
      this.InitializeComponent();
      this._uid = Utils.GetGuid();
    }

    public bool InQuickAddWindow
    {
      get => (bool) this.GetValue(ChecklistControl.InQuickAddWindowProperty);
      set => this.SetValue(ChecklistControl.InQuickAddWindowProperty, (object) value);
    }

    public string TaskId { get; set; }

    private ItemCollection Items => this.CheckListView.Items;

    public List<CheckItemViewModel> ChecklistItems { get; set; } = new List<CheckItemViewModel>();

    public void SetData(List<CheckItemViewModel> data)
    {
      List<CheckItemViewModel> list = data.Where<CheckItemViewModel>((Func<CheckItemViewModel, bool>) (item => item.IsValid)).OrderBy<CheckItemViewModel, CheckItemViewModel>((Func<CheckItemViewModel, CheckItemViewModel>) (x => x), (IComparer<CheckItemViewModel>) new ChecklistItemComparaer()).ToList<CheckItemViewModel>();
      this.ChecklistItems = list;
      int count1 = this.Items.Count;
      int count2 = list.Count;
      if (count1 > count2)
      {
        List<object> objectList = new List<object>();
        for (int index = count2; index < count1; ++index)
          objectList.Add(this.Items[index]);
        foreach (object removeItem in objectList)
          this.Items.Remove(removeItem);
        for (int index = 0; index < count2; ++index)
        {
          if (this.CheckListView.Items[index] is ChecklistItemControl checklistItemControl)
            checklistItemControl.DataContext = (object) list[index];
        }
      }
      else
      {
        for (int index = 0; index < count1; ++index)
        {
          if (this.Items[index] is ChecklistItemControl checklistItemControl)
            checklistItemControl.DataContext = (object) list[index];
        }
        for (int index = 0; index < count2 - count1; ++index)
        {
          ChecklistItemControl newItem = new ChecklistItemControl(this, this._inSticky, this._isStickyDark);
          newItem.DataContext = (object) list[count1 + index];
          this.Items.Add((object) newItem);
        }
      }
    }

    public event EventHandler CaretMoveUp;

    public event ChecklistControl.ChecklistChangedDelegate Changed;

    public event EventHandler ItemsClear;

    public event EventHandler<string> ItemDrop;

    public event EventHandler<string> TagSelected;

    public event EventHandler<bool> PopOpened;

    public event EventHandler<bool> PopClosed;

    public event EventHandler<TaskDetailItemModel> CheckItemsDeleted;

    public event EventHandler<string> ToastUnableText;

    public event EventHandler<string> ItemSplit;

    public event EventHandler<ProjectTask> Navigate;

    public event EventHandler DatePopOpened;

    public event EventHandler DatePopClosed;

    public event EventHandler QuickAddTask;

    public event EventHandler<QuickSetModel> QuickItemSelected;

    public event EventHandler<double> CaretVerticalOffsetChanged;

    public string ToText(string description)
    {
      string description1 = description;
      List<CheckItemViewModel> checklistItems = this.ChecklistItems;
      List<string> list = checklistItems != null ? checklistItems.Select<CheckItemViewModel, string>((Func<CheckItemViewModel, string>) (item => item.Title)).ToList<string>() : (List<string>) null;
      return ChecklistUtils.Items2Text(description1, list);
    }

    public async void ScrollToItem(string id)
    {
      ChecklistControl checklistControl = this;
      int indexById = checklistControl.GetIndexById(id);
      if (indexById < 0 || indexById >= checklistControl.Items.Count || !(checklistControl.Items[indexById] is ChecklistItemControl checklistItemControl))
        return;
      checklistItemControl.Blink();
      System.Windows.Point point = checklistItemControl.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) checklistControl);
      EventHandler<double> verticalOffsetChanged = checklistControl.CaretVerticalOffsetChanged;
      if (verticalOffsetChanged == null)
        return;
      verticalOffsetChanged((object) checklistControl, point.Y + 8.0);
    }

    public ChecklistItemControl GetItemControlByIndex(int index)
    {
      try
      {
        if (index < this.Items.Count)
        {
          if (this.Items[index] is ChecklistItemControl itemControlByIndex)
            return itemControlByIndex;
        }
      }
      catch (ArgumentOutOfRangeException ex)
      {
      }
      return (ChecklistItemControl) null;
    }

    public void Unregister(ChecklistItemControl item)
    {
      item.Split -= new EventHandler<ItemTextChange>(this.SplitNewItem);
      item.Merge -= new EventHandler<ItemTextChange>(this.MergeItem);
      item.MoveUp -= new EventHandler<string>(this.MoveUp);
      item.MoveDown -= new EventHandler<string>(this.MoveDown);
      item.Check -= new EventHandler<string>(this.CompleteItem);
      item.UnCheck -= new EventHandler<string>(this.UnCompleteItem);
      item.TitleChanged -= new EventHandler<ItemTextChange>(this.OnItemTitleChanged);
      item.ItemDrag -= new ChecklistItemControl.DragDelegate(this.OnItemStartDrag);
      item.TagSelected -= new EventHandler<string>(this.OnTagSelected);
      item.PopOpened -= new EventHandler<bool>(this.OnPopOpened);
      item.PopClosed -= new EventHandler<bool>(this.OnPopClosed);
      item.ToastString -= new EventHandler<string>(this.OnItemToastString);
      item.QuickItemSelected -= new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      item.TitleTextBox.Navigate -= new EventHandler<ProjectTask>(this.OnNavigateTask);
      item.QuickAddTask -= new EventHandler(this.OnQuickAddTask);
      item.CaretVerticalOffsetChanged -= new EventHandler<double>(this.OnItemCaretVOffsetChanged);
    }

    public void Register(ChecklistItemControl item)
    {
      this.Unregister(item);
      item.Split += new EventHandler<ItemTextChange>(this.SplitNewItem);
      item.Merge += new EventHandler<ItemTextChange>(this.MergeItem);
      item.MoveUp += new EventHandler<string>(this.MoveUp);
      item.MoveDown += new EventHandler<string>(this.MoveDown);
      item.Check += new EventHandler<string>(this.CompleteItem);
      item.UnCheck += new EventHandler<string>(this.UnCompleteItem);
      item.TitleChanged += new EventHandler<ItemTextChange>(this.OnItemTitleChanged);
      item.ItemDrag += new ChecklistItemControl.DragDelegate(this.OnItemStartDrag);
      item.TagSelected += new EventHandler<string>(this.OnTagSelected);
      item.PopOpened += new EventHandler<bool>(this.OnPopOpened);
      item.PopClosed += new EventHandler<bool>(this.OnPopClosed);
      item.ToastString += new EventHandler<string>(this.OnItemToastString);
      item.QuickItemSelected += new EventHandler<QuickSetModel>(this.OnQuickItemSelected);
      item.TitleTextBox.Navigate += new EventHandler<ProjectTask>(this.OnNavigateTask);
      item.QuickAddTask += new EventHandler(this.OnQuickAddTask);
      item.CaretVerticalOffsetChanged += new EventHandler<double>(this.OnItemCaretVOffsetChanged);
    }

    public bool InMainDetail { get; set; }

    public TaskDetailView GetParent()
    {
      if (this._parentControl == null)
        this._parentControl = Utils.FindParent<TaskDetailView>((DependencyObject) this);
      return this._parentControl;
    }

    private void OnQuickAddTask(object sender, EventArgs e)
    {
      EventHandler quickAddTask = this.QuickAddTask;
      if (quickAddTask == null)
        return;
      quickAddTask(sender, e);
    }

    private void OnItemCaretVOffsetChanged(object sender, double e)
    {
      if (!(sender is ChecklistItemControl checklistItemControl))
        return;
      System.Windows.Point point = checklistItemControl.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) this);
      EventHandler<double> verticalOffsetChanged = this.CaretVerticalOffsetChanged;
      if (verticalOffsetChanged == null)
        return;
      verticalOffsetChanged((object) this, e + point.Y + 8.0);
    }

    private void OnQuickItemSelected(object sender, QuickSetModel e)
    {
      EventHandler<QuickSetModel> quickItemSelected = this.QuickItemSelected;
      if (quickItemSelected == null)
        return;
      quickItemSelected(sender, e);
    }

    private void OnNavigateTask(object sender, ProjectTask task)
    {
      EventHandler<ProjectTask> navigate = this.Navigate;
      if (navigate == null)
        return;
      navigate(sender, task);
    }

    private void OnPopClosed(object sender, bool e)
    {
      EventHandler<bool> popClosed = this.PopClosed;
      if (popClosed == null)
        return;
      popClosed((object) this, e);
    }

    private void OnPopOpened(object sender, bool e)
    {
      EventHandler<bool> popOpened = this.PopOpened;
      if (popOpened == null)
        return;
      popOpened((object) this, e);
    }

    private void OnTagSelected(object sender, string tag)
    {
      EventHandler<string> tagSelected = this.TagSelected;
      if (tagSelected == null)
        return;
      tagSelected(sender, tag);
    }

    private void OnItemStartDrag(CheckItemViewModel model, MouseEventArgs arg)
    {
      this._dragging = true;
      if (model == null)
        return;
      CheckItemViewModel checkItemViewModel = model.Clone();
      Window window = Window.GetWindow((DependencyObject) this);
      if (window == null)
        return;
      this.GetParent()?.OnShowDialog();
      window.CaptureMouse();
      window.MouseMove += new MouseEventHandler(this.OnItemDragMove);
      window.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnListMouseLeftUp);
      this.TaskDragPopup.DataContext = (object) checkItemViewModel;
      this.TaskDragPopup.IsOpen = true;
      this.MoveDragPopup(arg);
    }

    private async void OnItemDrop()
    {
      ChecklistControl sender = this;
      sender._dragging = false;
      if (!(sender.TaskDragPopup.DataContext is CheckItemViewModel))
        return;
      CheckItemViewModel model = (CheckItemViewModel) sender.TaskDragPopup.DataContext;
      sender.TaskDragPopup.IsOpen = false;
      bool flag = await sender.SetSortOrderOnDrop(model);
      sender.ClearDropLine();
      if (sender.IsNewAddTask)
      {
        sender.SortChecklistItems();
      }
      else
      {
        if (!flag)
        {
          EventHandler<string> itemDrop = sender.ItemDrop;
          if (itemDrop != null)
            itemDrop((object) sender, model.Id);
        }
        model = (CheckItemViewModel) null;
      }
    }

    private async Task<bool> SetSortOrderOnDrop(CheckItemViewModel model)
    {
      CheckItemViewModel checkItemViewModel1 = this.ChecklistItems.FirstOrDefault<CheckItemViewModel>((Func<CheckItemViewModel, bool>) (item => item.ShowBottomDropLine || item.ShowTopDropLine));
      if (checkItemViewModel1 != null)
      {
        List<CheckItemViewModel> list1 = this.ChecklistItems.OrderBy<CheckItemViewModel, long>((Func<CheckItemViewModel, long>) (item => item.SortOrder)).ToList<CheckItemViewModel>();
        List<long> list2 = list1.Select<CheckItemViewModel, long>((Func<CheckItemViewModel, long>) (i => i.SortOrder)).ToList<long>();
        this.CheckSortRepeated(list1, list2);
        int index1 = -1;
        for (int index2 = 0; index2 < list2.Count; ++index2)
        {
          if (list2[index2] == checkItemViewModel1.SortOrder)
          {
            index1 = index2;
            break;
          }
        }
        long num = checkItemViewModel1.SortOrder;
        if (index1 >= 0)
        {
          if (checkItemViewModel1.ShowTopDropLine)
          {
            if (index1 == 0)
              num -= 268435456L;
            else
              num = (list2[index1 - 1] + list2[index1]) / 2L;
          }
          else if (checkItemViewModel1.ShowBottomDropLine)
          {
            if (index1 == list2.Count - 1)
              num += 268435456L;
            else
              num = (list2[index1] + list2[index1 + 1]) / 2L;
          }
          model.SourceViewModel.SortOrder = num;
          CheckItemViewModel checkItemViewModel2 = this.ChecklistItems.FirstOrDefault<CheckItemViewModel>((Func<CheckItemViewModel, bool>) (item => item.Id == model.Id));
          if (checkItemViewModel2 != null)
            checkItemViewModel2.SourceViewModel.SortOrder = num;
          await model.Save(false);
          return true;
        }
      }
      return false;
    }

    private void CheckSortRepeated(List<CheckItemViewModel> items, List<long> sortOrders)
    {
      bool flag = false;
      for (int index = 0; index < sortOrders.Count - 1; ++index)
      {
        if (Math.Abs(sortOrders[index + 1] - sortOrders[index]) < 2L)
        {
          flag = true;
          break;
        }
      }
      if (!flag)
        return;
      long num = 0;
      foreach (CheckItemViewModel checkItemViewModel in items)
      {
        checkItemViewModel.SourceViewModel.SortOrder = num;
        num += 268435456L;
        checkItemViewModel.Save(false);
      }
      sortOrders.Clear();
      sortOrders.AddRange((IEnumerable<long>) items.Select<CheckItemViewModel, long>((Func<CheckItemViewModel, long>) (i => i.SortOrder)).ToList<long>());
    }

    private void OnListMouseLeftUp(object sender, MouseButtonEventArgs e)
    {
      Window window = Window.GetWindow((DependencyObject) this);
      if (window != null)
      {
        window.ReleaseMouseCapture();
        window.MouseMove -= new MouseEventHandler(this.OnItemDragMove);
        window.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnListMouseLeftUp);
        this.GetParent()?.OnCloseDialog();
      }
      if (!this._dragging)
        return;
      this.OnItemDrop();
      TaskDragHelpModel.DragHelp.IsDragging = false;
    }

    private void OnItemDragMove(object sender, MouseEventArgs arg)
    {
      if (!this._dragging || arg.LeftButton != MouseButtonState.Pressed)
        return;
      this.MoveDragPopup(arg);
      if (!(this.GetParentWindow() is MainWindow))
        return;
      System.Windows.Point position = arg.GetPosition((IInputElement) Application.Current?.MainWindow);
      long x = (long) position.X;
      position = arg.GetPosition((IInputElement) Application.Current?.MainWindow);
      long y = (long) position.Y;
      DragEventManager.NotifyDragEvent(new DragMouseEvent((double) x, (double) y)
      {
        MouseArg = arg
      });
    }

    private Window GetParentWindow()
    {
      this._window = this._window ?? Utils.GetParentWindow((DependencyObject) this);
      return this._window;
    }

    private void MoveDragPopup(MouseEventArgs arg)
    {
      this.TaskDragPopup.VerticalOffset = arg.GetPosition((IInputElement) this).Y - 24.0;
      this.TaskDragPopup.HorizontalOffset = arg.GetPosition((IInputElement) this).X;
      if (arg.GetPosition((IInputElement) this).X >= 0.0)
        this.SetDropLine(arg);
      else
        this.ClearDropLine();
    }

    private void SetDropLine(MouseEventArgs arg)
    {
      this.ClearDropLine();
      if (this.ChecklistItems == null || this.ChecklistItems.Count == 0 || this.HandleOuterDrag(arg))
        return;
      this.HandleOverItem(arg);
    }

    private void HandleOverItem(MouseEventArgs arg)
    {
      int delta = 0;
      if (!(this.GetHoverItem(arg.GetPosition((IInputElement) this), ref delta)?.DataContext is CheckItemViewModel dataContext) || dataContext.Status != 0)
        return;
      if (delta >= 0 && delta <= 20)
        dataContext.ShowBottomDropLine = true;
      else
        dataContext.ShowTopDropLine = true;
    }

    private bool HandleOuterDrag(MouseEventArgs arg)
    {
      double y = arg.GetPosition((IInputElement) this).Y;
      if (y <= 0.0)
      {
        CheckItemViewModel checklistItem = this.ChecklistItems[0];
        checklistItem.ShowTopDropLine = true;
        checklistItem.ShowBottomDropLine = false;
        return true;
      }
      int num = this.ChecklistItems.Count<CheckItemViewModel>((Func<CheckItemViewModel, bool>) (item => item.Status == 0));
      long uncheckAreaHeight = this.GetUncheckAreaHeight();
      if (y < (double) uncheckAreaHeight || num < 1)
        return false;
      CheckItemViewModel checklistItem1 = this.ChecklistItems[num - 1];
      checklistItem1.ShowTopDropLine = false;
      checklistItem1.ShowBottomDropLine = true;
      return true;
    }

    private long GetUncheckAreaHeight()
    {
      long uncheckAreaHeight = 0;
      for (int index = 0; index < this.ChecklistItems.Count; ++index)
      {
        if (this.ChecklistItems[index].Status == 0)
        {
          ChecklistItemControl itemControlByIndex = this.GetItemControlByIndex(index);
          if (itemControlByIndex != null)
            uncheckAreaHeight += (long) itemControlByIndex.ActualHeight;
        }
      }
      return uncheckAreaHeight;
    }

    private void OnItemTitleChanged(object sender, ItemTextChange change)
    {
      if (sender.Equals((object) this))
        return;
      if (this.ChecklistItems.Count == 1)
        this.ChecklistItems[0].ShowAddHint = true;
      if (this.InQuickAddWindow || this.IsNewAddTask)
        return;
      TaskChangeNotifier.NotifyTaskTextChanged(this.TaskId);
    }

    public async void NotifyTaskChanged(CheckItemModifyType type)
    {
      if (string.IsNullOrEmpty(this.TaskId) || this.IsNewAddTask)
        return;
      await SyncStatusDao.AddSyncStatus(this.TaskId, 0);
      ChecklistControl.ChecklistChangedDelegate changed = this.Changed;
      if (changed == null)
        return;
      changed(this.TaskId, string.Empty, type);
    }

    private async void UnCompleteItem(object sender, string id)
    {
      CheckItemViewModel item = this.GetItemModelById(id);
      if (item == null)
      {
        item = (CheckItemViewModel) null;
      }
      else
      {
        item.SourceViewModel.Status = 0;
        item.SourceViewModel.CompletedTime = new DateTime?();
        await Task.Delay(50);
        await this.SaveAndNotify(item, CheckItemModifyType.Uncheck);
        if (this.InQuickAddWindow)
        {
          item = (CheckItemViewModel) null;
        }
        else
        {
          ItemChangeNotifier.NotifyItemStatusChanged(item.Id);
          item = (CheckItemViewModel) null;
        }
      }
    }

    private async void CompleteItem(object sender, string id)
    {
      ChecklistControl child = this;
      CheckItemViewModel item = child.GetItemModelById(id);
      if (item == null)
      {
        item = (CheckItemViewModel) null;
      }
      else
      {
        DateTime? displayDate = (DateTime?) child.GetParent()?.GetDisplayDate();
        if (displayDate.HasValue)
        {
          IToastShowWindow dependentWindow = Utils.FindParent<TaskDetailPopup>((DependencyObject) child)?.DependentWindow;
          if (!await ModifyRepeatHandler.CompleteOrSkipRecurrence(item.TaskServerId, displayDate, dependentWindow))
          {
            item = (CheckItemViewModel) null;
            return;
          }
        }
        item.SourceViewModel.Status = 1;
        item.SourceViewModel.CompletedTime = new DateTime?(DateTime.Now);
        await Task.Delay(50);
        await child.SaveAndNotify(item, CheckItemModifyType.Check);
        if (child.InQuickAddWindow)
          item = (CheckItemViewModel) null;
        else if (child.IsNewAddTask)
        {
          item = (CheckItemViewModel) null;
        }
        else
        {
          ItemChangeNotifier.NotifyItemStatusChanged(item.Id);
          Utils.PlayCompletionSound();
          item = (CheckItemViewModel) null;
        }
      }
    }

    private void NotifyItemCompleted(string id)
    {
      int indexById = this.GetIndexById(id);
      List<CheckItemViewModel> list = this.ChecklistItems.ToList<CheckItemViewModel>();
      if (indexById < 0 || indexById >= list.Count)
        return;
      this.SetData(list);
    }

    private async Task SaveAndNotify(CheckItemViewModel item, CheckItemModifyType type)
    {
      switch (type)
      {
        case CheckItemModifyType.Check:
        case CheckItemModifyType.Uncheck:
          this.NotifyItemCompleted(item.Id);
          break;
        default:
          this.SortChecklistItems();
          break;
      }
      if (this.InQuickAddWindow || this.IsNewAddTask)
        return;
      await item.Save();
      await SyncStatusDao.AddSyncStatus(this.TaskId, 0);
      ChecklistControl.ChecklistChangedDelegate changed = this.Changed;
      if (changed == null)
        return;
      changed(this.TaskId, item.Id, type);
    }

    public void SortChecklistItems()
    {
      DelayActionHandlerCenter.TryDoAction(this._uid, (EventHandler) ((sender, args) => this.Dispatcher.Invoke((Action) (() => this.SetData(this.ChecklistItems)))), 100);
    }

    private void MoveUp(object sender, string id)
    {
      int indexById = this.GetIndexById(id);
      if (indexById > 0)
      {
        this.GetItemControlByIndex(indexById - 1)?.FocusEnd();
      }
      else
      {
        if (indexById != 0)
          return;
        EventHandler caretMoveUp = this.CaretMoveUp;
        if (caretMoveUp == null)
          return;
        caretMoveUp((object) this, (EventArgs) null);
      }
    }

    private async void FocusTitle(string id)
    {
      ChecklistItemControl item = this.GetItemControlByIndex(this.GetIndexById(id));
      if (item == null)
      {
        item = (ChecklistItemControl) null;
      }
      else
      {
        await Task.Delay(10);
        ChecklistItemControl checklistItemControl = item;
        if (checklistItemControl == null)
        {
          item = (ChecklistItemControl) null;
        }
        else
        {
          checklistItemControl.FocusTitle(scroll: true);
          item = (ChecklistItemControl) null;
        }
      }
    }

    private void MoveDown(object sender, string id)
    {
      this.GetItemControlByIndex(this.GetIndexById(id) + 1)?.FocusEnd();
    }

    private async void MergeItem(object sender, ItemTextChange change)
    {
      int indexById = this.GetIndexById(change.Id);
      if (indexById > 0)
      {
        this.RemoveItemById(change.Id, false);
        CheckItemViewModel lastItem = this.ChecklistItems[indexById - 1];
        lastItem.SourceViewModel.Title += change.Text;
        this.FocusAndUpdateTitle(lastItem, indexById - 1, 1, focusEnd: true);
        if (!this.IsNewAddTask)
        {
          await TaskDetailItemDao.DeleteById(change.Id);
          await lastItem.Save();
        }
        lastItem = (CheckItemViewModel) null;
      }
      else
      {
        if (indexById != 0 || !string.IsNullOrEmpty(change.Text))
          return;
        this.RemoveItemById(change.Id, false);
        if (this.ChecklistItems.Count < 1)
          return;
        this.GetItemControlByIndex(0)?.FocusTitle(0);
        if (this.IsNewAddTask)
          return;
        await TaskDetailItemDao.DeleteById(change.Id);
      }
    }

    private void SplitNewItem(object sender, ItemTextChange change)
    {
      if (this.CheckItemOverLimit(this.ChecklistItems.Count<CheckItemViewModel>((Func<CheckItemViewModel, bool>) (item => item.IsValid)) + 1))
        return;
      List<CheckItemViewModel> list1 = this.ChecklistItems.OrderBy<CheckItemViewModel, long>((Func<CheckItemViewModel, long>) (item => item.SortOrder)).ToList<CheckItemViewModel>();
      List<long> list2 = list1.Select<CheckItemViewModel, long>((Func<CheckItemViewModel, long>) (i => i.SortOrder)).ToList<long>();
      this.CheckSortRepeated(list1, list2);
      int indexById = this.GetIndexById(change.Id);
      if (indexById < 0 || indexById >= this.ChecklistItems.Count)
        return;
      CheckItemViewModel checklistItem = this.ChecklistItems[indexById];
      if (checklistItem.Status == 0)
        this.SplitNextItem(change, indexById);
      else
        this.InsertEmptyItemAbove(checklistItem);
    }

    private async void SplitNextItem(ItemTextChange change, int currentIndex)
    {
      string newId;
      CheckItemViewModel newItem;
      if (this.CheckItemOverLimit(this.ChecklistItems.Count<CheckItemViewModel>((Func<CheckItemViewModel, bool>) (item => item.IsValid)) + 1))
      {
        newId = (string) null;
        newItem = (CheckItemViewModel) null;
      }
      else if (currentIndex < 0)
      {
        newId = (string) null;
        newItem = (CheckItemViewModel) null;
      }
      else if (currentIndex >= this.ChecklistItems.Count)
      {
        newId = (string) null;
        newItem = (CheckItemViewModel) null;
      }
      else
      {
        CheckItemViewModel checklistItem = this.ChecklistItems[currentIndex];
        ChecklistItemControl itemControlByIndex1 = this.GetItemControlByIndex(currentIndex);
        if (itemControlByIndex1 == null)
        {
          newId = (string) null;
          newItem = (CheckItemViewModel) null;
        }
        else
        {
          if (Utils.GetDescendantByType((Visual) itemControlByIndex1, typeof (DetailTextBox), "TitleTextBox") is DetailTextBox descendantByType)
            descendantByType.Text = change.Text;
          DateTime? startDate = checklistItem.StartDate;
          bool flag = false;
          if (string.IsNullOrEmpty(change.Text))
          {
            checklistItem.SourceViewModel.StartDate = new DateTime?();
            flag = true;
          }
          newId = Utils.GetGuid();
          newItem = new CheckItemViewModel(new TaskBaseViewModel()
          {
            Id = newId,
            ParentId = checklistItem.TaskServerId,
            Title = change.Extra,
            SortOrder = this.CalcSortOrder(checklistItem.SortOrder, false),
            Status = 0,
            StartDate = flag ? startDate : new DateTime?(),
            IsAllDay = flag ? checklistItem.IsAllDay : new bool?(true),
            TimeZoneName = checklistItem.TimeZoneName,
            IsFloating = checklistItem.IsFloating
          })
          {
            IsValid = true,
            RepeatDiff = checklistItem.RepeatDiff
          };
          int index = currentIndex + 1;
          List<CheckItemViewModel> list = this.ChecklistItems.ToList<CheckItemViewModel>();
          if (list.Count > index && !list[index].IsValid)
            list[index] = newItem;
          else
            list.Insert(index, newItem);
          this.SetData(list);
          if (this.GetParent() != null && !this.IsNewAddTask)
          {
            ChecklistItemControl itemControlByIndex2 = this.GetItemControlByIndex(currentIndex);
            if (itemControlByIndex2 != null)
              checklistItem.SourceViewModel.Title = itemControlByIndex2.TitleTextBox.Text;
            await checklistItem.Save();
            await newItem.Insert();
            TaskBaseViewModel checkItemById = TaskDetailItemCache.GetCheckItemById(newId);
            if (checkItemById != null)
              newItem.SetSourceModel(checkItemById);
            this.DelayNotifyAddCheckItem();
          }
          this.FocusTitle(newId);
          newId = (string) null;
          newItem = (CheckItemViewModel) null;
        }
      }
    }

    private bool CheckItemOverLimit(int count)
    {
      long limitByKey = LimitCache.GetLimitByKey(Constants.LimitKind.SubtaskNumber);
      if ((long) count <= limitByKey)
        return false;
      if (UserDao.IsPro())
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("ChecklistLimit"), MessageBoxButton.OK);
        customerDialog.Owner = Window.GetWindow((DependencyObject) this);
        customerDialog.ShowDialog();
      }
      else
        ProChecker.ShowUpgradeDialog(ProType.MoreSubTasks);
      return true;
    }

    private async void DelayNotifyAddCheckItem()
    {
      if (this.IsNewAddTask)
        return;
      await Task.Delay(100);
      this.NotifyTaskChanged(CheckItemModifyType.Add);
    }

    private long CalcSortOrder(long currentOrder, bool isUp)
    {
      List<long> list = this.ChecklistItems.Select<CheckItemViewModel, long>((Func<CheckItemViewModel, long>) (item => item.SortOrder)).OrderBy<long, long>((Func<long, long>) (item => item)).ToList<long>();
      int num = -1;
      if (list.Count <= 0)
        return 0;
      for (int index = 0; index < list.Count; ++index)
      {
        if (list[index] == currentOrder)
        {
          num = index;
          break;
        }
      }
      return isUp ? (num == 0 ? currentOrder - 268435456L : currentOrder - (currentOrder - list[num - 1]) / 2L) : (num == list.Count - 1 ? currentOrder + 268435456L : currentOrder + (list[num + 1] - currentOrder) / 2L);
    }

    private async void InsertEmptyItemAbove(CheckItemViewModel currentItem)
    {
      CheckItemViewModel item = new CheckItemViewModel(new TaskBaseViewModel()
      {
        Id = Utils.GetGuid(),
        ParentId = currentItem.TaskServerId,
        Title = string.Empty,
        SortOrder = this.CalcSortOrder(currentItem.SortOrder, true),
        Status = 0
      })
      {
        IsValid = true,
        RepeatDiff = currentItem.RepeatDiff
      };
      int firstCompleteIndex = this.GetFirstCompleteIndex();
      List<CheckItemViewModel> list = this.ChecklistItems.ToList<CheckItemViewModel>();
      if (firstCompleteIndex < 0)
      {
        item = (CheckItemViewModel) null;
      }
      else
      {
        if (list.Count > firstCompleteIndex)
          list[firstCompleteIndex] = item;
        else
          list.Insert(firstCompleteIndex, item);
        this.SetData(list);
        this.FocusAndUpdateTitle(item, firstCompleteIndex, 1);
        if (this.IsNewAddTask)
        {
          item = (CheckItemViewModel) null;
        }
        else
        {
          await item.Insert();
          TaskBaseViewModel checkItemById = TaskDetailItemCache.GetCheckItemById(item.Id);
          if (checkItemById != null)
            item.SetSourceModel(checkItemById);
          this.NotifyTaskChanged(CheckItemModifyType.Add);
          item = (CheckItemViewModel) null;
        }
      }
    }

    private int GetFirstCompleteIndex()
    {
      for (int index = 0; index < this.ChecklistItems.Count; ++index)
      {
        if (this.ChecklistItems[index].Status != 0)
          return index;
      }
      return -1;
    }

    public async void RemoveItemById(string id, bool allowRestore = true)
    {
      ChecklistControl sender = this;
      List<CheckItemViewModel> checklistItems = sender.ChecklistItems;
      CheckItemViewModel item = checklistItems != null ? checklistItems.FirstOrDefault<CheckItemViewModel>((Func<CheckItemViewModel, bool>) (t => t.Id == id)) : (CheckItemViewModel) null;
      if (item == null)
      {
        item = (CheckItemViewModel) null;
      }
      else
      {
        sender.RemoveItem(item);
        sender.ChecklistItems.Remove(item);
        if (sender.IsNewAddTask)
        {
          if (sender.ChecklistItems.Count != 0)
          {
            item = (CheckItemViewModel) null;
          }
          else
          {
            EventHandler itemsClear = sender.ItemsClear;
            if (itemsClear == null)
            {
              item = (CheckItemViewModel) null;
            }
            else
            {
              itemsClear((object) sender, (EventArgs) null);
              item = (CheckItemViewModel) null;
            }
          }
        }
        else
        {
          TaskModel taskModel = await TaskService.DeleteCheckItem(id);
          if (taskModel != null && taskModel.kind == "TEXT")
          {
            EventHandler itemsClear = sender.ItemsClear;
            if (itemsClear != null)
              itemsClear((object) sender, (EventArgs) null);
          }
          else
            sender.NotifyTaskChanged(CheckItemModifyType.Delete);
          if (!allowRestore)
          {
            item = (CheckItemViewModel) null;
          }
          else
          {
            TaskDetailItemModel e = new TaskDetailItemModel()
            {
              id = Utils.GetGuid(),
              TaskServerId = item.TaskServerId,
              title = item.Title,
              isAllDay = item.IsAllDay,
              startDate = item.StartDate,
              sortOrder = item.SortOrder,
              snoozeReminderTime = item.SnoozeReminderTime,
              completedTime = item.CompletedTime,
              status = item.Status
            };
            EventHandler<TaskDetailItemModel> checkItemsDeleted = sender.CheckItemsDeleted;
            if (checkItemsDeleted == null)
            {
              item = (CheckItemViewModel) null;
            }
            else
            {
              checkItemsDeleted((object) sender, e);
              item = (CheckItemViewModel) null;
            }
          }
        }
      }
    }

    private void RemoveItem(CheckItemViewModel model)
    {
      int indexById = this.GetIndexById(model.Id);
      if (indexById < 0)
        return;
      ChecklistItemControl itemControlByIndex = this.GetItemControlByIndex(indexById);
      if (!(itemControlByIndex.DataContext is CheckItemViewModel dataContext) || !(dataContext.Id == model.Id))
        return;
      this.Items.Remove((object) itemControlByIndex);
    }

    private async void FocusAndUpdateTitle(
      CheckItemViewModel model,
      int itemIndex,
      int delay = 0,
      int caretIndex = 0,
      bool focusEnd = false)
    {
      ChecklistItemControl itemControlByIndex = this.GetItemControlByIndex(itemIndex);
      if (itemControlByIndex == null || !(Utils.GetDescendantByType((Visual) itemControlByIndex, typeof (DetailTextBox), "TitleTextBox") is DetailTextBox descendantByType))
        return;
      descendantByType.Text = model.Title;
      Utils.Focus((UIElement) descendantByType);
      if (focusEnd)
      {
        descendantByType.CaretOffset = descendantByType.Text.Length;
      }
      else
      {
        if (caretIndex <= 0)
          return;
        descendantByType.CaretOffset = caretIndex;
      }
    }

    private CheckItemViewModel GetItemModelById(string id)
    {
      for (int index = 0; index < this.ChecklistItems.Count; ++index)
      {
        CheckItemViewModel checklistItem = this.ChecklistItems[index];
        if (checklistItem.Id == id)
          return checklistItem;
      }
      return (CheckItemViewModel) null;
    }

    private int GetIndexById(string id)
    {
      for (int index = 0; index < this.ChecklistItems.Count; ++index)
      {
        if (this.ChecklistItems[index].Id == id)
          return index;
      }
      return -1;
    }

    private ChecklistItemControl GetHoverItem(System.Windows.Point point, ref int delta)
    {
      for (int index = 0; index < this.ChecklistItems.Count; ++index)
      {
        ChecklistItemControl hoverItem = (ChecklistItemControl) null;
        if (this.CheckListView.Items.Count > index && this.CheckListView.Items[index] is ChecklistItemControl checklistItemControl)
          hoverItem = checklistItemControl;
        if (hoverItem != null)
        {
          System.Windows.Point point1 = hoverItem.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) this);
          System.Windows.Point point2 = hoverItem.TranslatePoint(new System.Windows.Point(hoverItem.ActualWidth, hoverItem.ActualHeight), (UIElement) this);
          if (new Rect(point1, point2).Contains(point))
          {
            delta = (int) (point2.Y - point.Y);
            return hoverItem;
          }
        }
      }
      return (ChecklistItemControl) null;
    }

    public void FocusFirstItem()
    {
      if (this.ChecklistItems == null || this.ChecklistItems.Count <= 0)
        return;
      this.FocusAndUpdateTitle(this.ChecklistItems[0], 0, 50, focusEnd: true);
    }

    public void HandleTaskMove(DragMouseEvent position)
    {
      if (this.CheckListView.Visibility != Visibility.Visible || this.ChecklistItems == null || !this.ChecklistItems.Any<CheckItemViewModel>())
        return;
      System.Windows.Point point1 = this.CheckListView.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) Application.Current?.MainWindow);
      System.Windows.Point point2 = this.CheckListView.TranslatePoint(new System.Windows.Point(this.ActualWidth, this.ActualHeight), (UIElement) Application.Current?.MainWindow);
      Rect rect = new Rect(point1, point2);
      System.Windows.Point point = new System.Windows.Point(position.X, position.Y);
      if (point.X < point1.X || point.X > point2.X || point.Y < point1.Y - 20.0 || point.Y > point2.Y + 20.0)
        this.ClearDropLine();
      else if (point.Y < point1.Y)
      {
        this.ClearDropLine();
        this.ChecklistItems[0].ShowTopDropLine = true;
      }
      else if (point.Y > point2.Y)
      {
        this.ClearDropLine();
        this.ChecklistItems[this.ChecklistItems.Count - 1].ShowBottomDropLine = true;
      }
      else
      {
        if (this._lastHoverIndex.X >= 0.0 && this._lastHoverIndex.X <= (double) (this.ChecklistItems.Count - 1))
        {
          this.ChecklistItems[(int) this._lastHoverIndex.X].ShowTopDropLine = false;
          this.ChecklistItems[(int) this._lastHoverIndex.X].ShowBottomDropLine = false;
        }
        if (!rect.Contains(point))
          return;
        System.Windows.Point hoverIndex = this.GetHoverIndex(new System.Windows.Point(position.X, position.Y));
        if (hoverIndex.X < 0.0 || hoverIndex.X > (double) (this.ChecklistItems.Count - 1))
          return;
        if (Math.Abs(hoverIndex.Y) <= 0.0)
          this.ChecklistItems[(int) hoverIndex.X].ShowTopDropLine = true;
        else
          this.ChecklistItems[(int) hoverIndex.X].ShowBottomDropLine = true;
        this._lastHoverIndex = hoverIndex;
      }
    }

    private void ClearDropLine()
    {
      this.ChecklistItems.ToList<CheckItemViewModel>().ForEach((Action<CheckItemViewModel>) (item =>
      {
        item.ShowTopDropLine = false;
        item.ShowBottomDropLine = false;
      }));
    }

    private System.Windows.Point GetHoverIndex(System.Windows.Point point)
    {
      for (int index = 0; index < this.ChecklistItems.Count; ++index)
      {
        ChecklistItemControl itemControlByIndex = this.GetItemControlByIndex(index);
        if (itemControlByIndex != null)
        {
          System.Windows.Point point1_1 = itemControlByIndex.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) Application.Current?.MainWindow);
          System.Windows.Point point2_1 = itemControlByIndex.TranslatePoint(new System.Windows.Point(itemControlByIndex.ActualWidth, itemControlByIndex.ActualHeight * 0.5), (UIElement) Application.Current?.MainWindow);
          System.Windows.Point point1_2 = itemControlByIndex.TranslatePoint(new System.Windows.Point(0.0, itemControlByIndex.ActualHeight * 0.5), (UIElement) Application.Current?.MainWindow);
          System.Windows.Point point2_2 = itemControlByIndex.TranslatePoint(new System.Windows.Point(itemControlByIndex.ActualWidth, itemControlByIndex.ActualHeight), (UIElement) Application.Current?.MainWindow);
          Rect rect1 = new Rect(point1_1, point2_1);
          Rect rect2 = new Rect(point1_2, point2_2);
          if (rect1.Contains(point))
            return new System.Windows.Point((double) index, 0.0);
          if (rect2.Contains(point))
            return new System.Windows.Point((double) index, 1.0);
        }
      }
      return new System.Windows.Point(0.0, 0.0);
    }

    public async Task HandleTaskDrop(string taskId)
    {
      string fromTaskId = taskId;
      string toTaskId = this.TaskId;
      CheckItemViewModel targetItem;
      if (fromTaskId == toTaskId)
      {
        fromTaskId = (string) null;
        targetItem = (CheckItemViewModel) null;
      }
      else
      {
        List<CheckItemViewModel> checklistItems = this.ChecklistItems;
        targetItem = checklistItems != null ? checklistItems.FirstOrDefault<CheckItemViewModel>((Func<CheckItemViewModel, bool>) (item => item.ShowTopDropLine || item.ShowBottomDropLine)) : (CheckItemViewModel) null;
        if (targetItem == null)
        {
          fromTaskId = (string) null;
          targetItem = (CheckItemViewModel) null;
        }
        else
        {
          bool topOrBottom = targetItem.ShowTopDropLine;
          this.ClearDropLine();
          TaskModel fromTask = await TaskDao.GetThinTaskById(fromTaskId);
          if (fromTask == null)
          {
            fromTaskId = (string) null;
            targetItem = (CheckItemViewModel) null;
          }
          else
          {
            List<TaskModel> tasks = await TaskDao.GetAllSubTasksById(fromTaskId, fromTask.projectId) ?? new List<TaskModel>();
            tasks.Insert(0, fromTask);
            if (tasks.Any<TaskModel>((Func<TaskModel, bool>) (t => t.id == toTaskId)))
            {
              fromTaskId = (string) null;
              targetItem = (CheckItemViewModel) null;
            }
            else if (!string.IsNullOrEmpty(fromTask.attendId))
            {
              EventHandler<string> toastUnableText = this.ToastUnableText;
              if (toastUnableText == null)
              {
                fromTaskId = (string) null;
                targetItem = (CheckItemViewModel) null;
              }
              else
              {
                toastUnableText((object) null, Utils.GetString("DropAgendaHint"));
                fromTaskId = (string) null;
                targetItem = (CheckItemViewModel) null;
              }
            }
            else
            {
              TaskModel dropTask = await TaskDao.GetThinTaskById(toTaskId);
              if (dropTask != null && !string.IsNullOrEmpty(dropTask.attendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) dropTask))
              {
                EventHandler<string> toastUnableText = this.ToastUnableText;
                if (toastUnableText == null)
                {
                  fromTaskId = (string) null;
                  targetItem = (CheckItemViewModel) null;
                }
                else
                {
                  toastUnableText((object) null, Utils.GetString("AttendeeModifyContent"));
                  fromTaskId = (string) null;
                  targetItem = (CheckItemViewModel) null;
                }
              }
              else
              {
                if (this.CanCreateSubtask())
                {
                  List<TaskDetailItemModel> subtask = await TaskService.TaskToSubtask(tasks, toTaskId, targetItem.Id, topOrBottom);
                  if (subtask != null)
                  {
                    TaskDetailViewModel toTaskViewModel = dropTask == null ? (TaskDetailViewModel) null : new TaskDetailViewModel(dropTask);
                    List<CheckItemViewModel> list = this.ChecklistItems.ToList<CheckItemViewModel>();
                    list.AddRange(subtask.Select<TaskDetailItemModel, CheckItemViewModel>((Func<TaskDetailItemModel, CheckItemViewModel>) (item => new CheckItemViewModel(TaskDetailItemCache.SafeGetViewModel(item), toTaskViewModel))));
                    this.SetData(list);
                    if (await TagDao.HandleDropTaskTags(fromTask, dropTask))
                      this.GetParent()?.Navigate(dropTask?.id);
                  }
                }
                fromTask = (TaskModel) null;
                tasks = (List<TaskModel>) null;
                dropTask = (TaskModel) null;
                fromTaskId = (string) null;
                targetItem = (CheckItemViewModel) null;
              }
            }
          }
        }
      }
    }

    private bool CanCreateSubtask()
    {
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("DragTaskToChecklist"), Utils.GetString("DragTaskToChecklistWarning"), Utils.GetString("OK"), Utils.GetString("Cancel"));
      customerDialog.Owner = Window.GetWindow((DependencyObject) this);
      bool? nullable = customerDialog.ShowDialog();
      bool flag = true;
      return nullable.GetValueOrDefault() == flag & nullable.HasValue;
    }

    public void TryToastUnableText()
    {
      EventHandler<string> toastUnableText = this.ToastUnableText;
      if (toastUnableText == null)
        return;
      toastUnableText((object) null, (string) null);
    }

    private void OnItemToastString(object sender, string e)
    {
      EventHandler<string> toastUnableText = this.ToastUnableText;
      if (toastUnableText == null)
        return;
      toastUnableText((object) null, e);
    }

    public void ScrollToSearchOffset()
    {
      if (this.ChecklistItems == null || this.ChecklistItems.Count == 0)
        return;
      IEnumerator enumerator = ((IEnumerable) this.Items).GetEnumerator();
      try
      {
        do
          ;
        while (enumerator.MoveNext() && (!(enumerator.Current is ChecklistItemControl current) || !current.IsVisible || current.GetSearchOffset() < 0.0));
      }
      finally
      {
        if (enumerator is IDisposable disposable)
          disposable.Dispose();
      }
    }

    public void SetStickyMode() => this._inSticky = true;

    public void OnDatePopupOpenChange(bool isOpen)
    {
      if (isOpen)
      {
        EventHandler datePopOpened = this.DatePopOpened;
        if (datePopOpened == null)
          return;
        datePopOpened((object) this, (EventArgs) null);
      }
      else
      {
        EventHandler datePopClosed = this.DatePopClosed;
        if (datePopClosed == null)
          return;
        datePopClosed((object) this, (EventArgs) null);
      }
    }

    public void SetStickyTheme(bool isDark)
    {
      this._isStickyDark = isDark;
      foreach (object obj in (IEnumerable) this.Items)
      {
        if (obj is ChecklistItemControl checklistItemControl)
          checklistItemControl.SetStickyTheme(isDark);
      }
    }

    public void SetTaskId(string taskId) => this.TaskId = taskId;

    public bool ItemFocus() => FocusManager.GetFocusedElement((DependencyObject) this) != null;

    public void RemoveEvents()
    {
      foreach (object obj in (IEnumerable) this.Items)
      {
        if (obj is ChecklistItemControl checklistItemControl)
        {
          this.Unregister(checklistItemControl);
          checklistItemControl.UnbindEvent();
        }
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/checklist/checklistcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.TaskDragPopup = (Popup) target;
        else
          this._contentLoaded = true;
      }
      else
        this.CheckListView = (ItemsControl) target;
    }

    public delegate void ChecklistChangedDelegate(
      string taskId,
      string itemId,
      CheckItemModifyType type);
  }
}
