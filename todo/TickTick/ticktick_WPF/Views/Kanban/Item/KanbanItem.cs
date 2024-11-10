// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.Item.KanbanItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Kanban.Item
{
  public class KanbanItem : Grid, ITaskOperation, IShowTaskDetailWindow, IComponentConnector
  {
    private KanbanItemController _controller;
    private bool _previewLeftClick;
    private bool _previewRightClick;
    private System.Windows.Point _startPoint;
    private KanbanItemContent _itemContent;
    private ListItemTagsControl _tagPanel;
    private bool _isWaitingDoubleClick;
    private DateTime _lastClickTime;
    internal Border Container;
    internal DockPanel ContentGrid;
    internal TextBlock ContentText;
    internal KanbanItemIcons ItemIcons;
    internal Border TagBd;
    internal Border AvatarBorder;
    private bool _contentLoaded;

    public KanbanItem()
    {
      this.InitializeComponent();
      this.Container.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDragMouseUp);
      this.Container.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnLeftMouseDown);
      this.Container.MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
      this.Container.MouseRightButtonDown += new MouseButtonEventHandler(this.OnRightMouseDown);
      this.Container.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      this.Container.MouseMove += new MouseEventHandler(this.OnItemDrag);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.ClearBinding());
    }

    public void SetPriority(int priority) => this._controller?.SetPriority(priority);

    public void SetDate(string key) => this._controller?.SetDate(key);

    public void ClearDate() => this._controller?.ClearDate();

    public void SelectDate(bool relative) => this._controller?.SelectDate();

    public void ToggleTaskCompleted()
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      this._controller?.ToggleItemCompleted(dataContext);
    }

    public void Delete() => this._controller?.DeleteTask();

    public bool ParsingDate() => false;

    public bool IsNewAdd() => false;

    public void PinOrUnpinTask()
    {
    }

    public void OpenSticky()
    {
    }

    private async void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      KanbanItem element = this;
      element.SetModel(e.OldValue, e.NewValue);
      if (!(element.DataContext is DisplayItemModel model))
        model = (DisplayItemModel) null;
      else if (string.IsNullOrEmpty(model.Id))
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        model.SetAvatar();
        if (element._controller == null)
        {
          element._controller = new KanbanItemController((UIElement) element, model);
          element.ItemIcons.SetController((DisplayItemController) element._controller);
        }
        else
          element._controller.Reset((UIElement) element, model);
        if (element._itemContent == null)
        {
          element._itemContent = new KanbanItemContent();
          element._itemContent.SetResourceReference(FrameworkElement.MinHeightProperty, (object) "Height36");
          element.ContentGrid.Children.Add((UIElement) element._itemContent);
        }
        element._itemContent.SetController(element._controller);
        double num = model.Status == 0 ? 1.0 : 0.4;
        element.AvatarBorder.Opacity = num;
        element.ContentText.Opacity = num;
        element.TagBd.Opacity = num;
        int count = 0;
        do
        {
          await Task.Delay(5);
          ++count;
        }
        while (!element.IsLoaded && count < 3);
        element.SetTagPanel();
        if (model.ShowReminder.HasValue)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          TaskItemLoadHelper.LoadShowReminder(model);
          model = (DisplayItemModel) null;
        }
      }
    }

    private void SetModel(object oldVal, object newVal)
    {
      DisplayItemModel oldModel = oldVal as DisplayItemModel;
      DisplayItemModel newModel = newVal as DisplayItemModel;
      newModel?.TrySetAvatar();
      Task.Run((Action) (() =>
      {
        newModel?.SetPropertyChangedEvent();
        if (oldModel != null)
          PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldModel, new EventHandler<PropertyChangedEventArgs>(this.OnTaskPropertyChanged), string.Empty);
        if (newModel == null)
          return;
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newModel, new EventHandler<PropertyChangedEventArgs>(this.OnTaskPropertyChanged), string.Empty);
      }));
    }

    private void OnTaskPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!object.Equals(sender, this.DataContext))
      {
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) (sender as DisplayItemModel), new EventHandler<PropertyChangedEventArgs>(this.OnTaskPropertyChanged), string.Empty);
      }
      else
      {
        switch (e.PropertyName)
        {
          case "Title":
            if (!(this.DataContext is DisplayItemModel dataContext) || this._itemContent == null || !(this._itemContent.TitleBox.Text != dataContext.Title))
              break;
            this._itemContent.TitleBox.SetText(dataContext.Title);
            break;
          case "ShowIcons":
            this.ItemIcons.ResetIcons();
            break;
          case "DisplayTags":
            this.SetTagPanel();
            break;
        }
      }
    }

    private void ClearBinding()
    {
      if (this.DataContext is DisplayItemModel dataContext)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) dataContext, new EventHandler<PropertyChangedEventArgs>(this.OnTaskPropertyChanged), string.Empty);
      if (this._tagPanel != null)
      {
        this._tagPanel.TagDelete -= new EventHandler<string>(this.OnTagDelete);
        this._tagPanel = (ListItemTagsControl) null;
        this.TagBd.Child = (UIElement) null;
      }
      if (this._itemContent != null)
      {
        this._itemContent.SetController((KanbanItemController) null);
        this.ContentGrid.Children.Remove((UIElement) this._itemContent);
        this._itemContent = (KanbanItemContent) null;
      }
      this.ItemIcons.Children.Clear();
      this._controller?.Reset((UIElement) null, (DisplayItemModel) null);
      this._controller = (KanbanItemController) null;
    }

    private async Task SetTagPanel()
    {
      KanbanItem kanbanItem = this;
      if (kanbanItem.DataContext is DisplayItemModel dataContext && dataContext.ShowTag)
      {
        if (kanbanItem._tagPanel != null)
          return;
        kanbanItem._tagPanel = new ListItemTagsControl();
        kanbanItem._tagPanel.TagDelete += new EventHandler<string>(kanbanItem.OnTagDelete);
        kanbanItem.TagBd.Child = (UIElement) kanbanItem._tagPanel;
      }
      else
      {
        if (kanbanItem._tagPanel == null)
          return;
        kanbanItem._tagPanel.TagDelete -= new EventHandler<string>(kanbanItem.OnTagDelete);
        kanbanItem._tagPanel = (ListItemTagsControl) null;
        kanbanItem.TagBd.Child = (UIElement) null;
      }
    }

    private async void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._previewRightClick)
        return;
      this._previewRightClick = false;
      bool flag = this._controller != null;
      if (flag)
        flag = !await this._controller?.ShowOperationDialogSafely();
      if (!flag)
        return;
      TaskDetailPopup window = await this._controller.ShowTaskDetail();
      await Task.Delay(100);
      window?.TryShow();
      window = (TaskDetailPopup) null;
    }

    private void OnTagDelete(object sender, string tag)
    {
      this._controller?.OnTagDelete(tag.ToLower());
    }

    private void OnItemDrag(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || !this._previewLeftClick || !(this.DataContext is DisplayItemModel dataContext) || !dataContext.Enable)
        return;
      System.Windows.Point position = e.GetPosition((IInputElement) this.Container);
      if (Math.Abs(position.X - this._startPoint.X) <= 4.0 && Math.Abs(position.Y - this._startPoint.Y) <= 4.0)
        return;
      this._previewLeftClick = false;
      this._controller?.OnKanbanItemDrag();
    }

    private async void OnDragMouseUp(object sender, MouseButtonEventArgs e)
    {
      KanbanItem kanbanItem = this;
      if (!(kanbanItem.DataContext is DisplayItemModel model))
        model = (DisplayItemModel) null;
      else if (kanbanItem._controller == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        bool flag = Utils.IfCtrlPressed() || Utils.IfShiftPressed();
        if (((kanbanItem._previewLeftClick ? 1 : (model.Selected ? 1 : 0)) | (flag ? 1 : 0)) == 0)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          kanbanItem._isWaitingDoubleClick = false;
          if (flag)
          {
            kanbanItem._controller.OnBatchSelectMouseUp();
            kanbanItem._previewLeftClick = false;
            model = (DisplayItemModel) null;
          }
          else
          {
            bool firstClick = (DateTime.Now - kanbanItem._lastClickTime).TotalMilliseconds > 300.0;
            kanbanItem._lastClickTime = DateTime.Now;
            TaskDetailPopup window = (TaskDetailPopup) null;
            if (firstClick)
            {
              kanbanItem._isWaitingDoubleClick = true;
              model.Selected = true;
              window = await kanbanItem._controller.ShowDetailWindow();
              await Task.Delay(160);
              if (!kanbanItem._isWaitingDoubleClick)
              {
                TaskDetailPopup taskDetailPopup = window;
                if (taskDetailPopup == null)
                {
                  model = (DisplayItemModel) null;
                  return;
                }
                taskDetailPopup.Clear();
                model = (DisplayItemModel) null;
                return;
              }
            }
            if (firstClick)
            {
              window?.TryShow();
            }
            else
            {
              model.Selected = false;
              TaskDetailWindows.ShowTaskWindows(model.Id);
            }
            kanbanItem._previewLeftClick = false;
            window = (TaskDetailPopup) null;
            model = (DisplayItemModel) null;
          }
        }
      }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      this._previewLeftClick = false;
      this._previewRightClick = false;
    }

    public TaskListView GetParentList() => this._controller?.GetTaskListView();

    private void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
    {
      KanbanItemContent itemContent = this._itemContent;
      if ((itemContent != null ? (itemContent.CheckBoxMouseOver() ? 1 : 0) : 0) != 0 || this.TagBd.IsMouseOver)
        return;
      this._startPoint = e.GetPosition((IInputElement) this.Container);
      this._previewLeftClick = true;
    }

    private void OnRightMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._previewRightClick = true;
    }

    public async Task SelectItem(bool focusTitle)
    {
      KanbanItem kanbanItem = this;
      if (!(kanbanItem.DataContext is DisplayItemModel dataContext) || kanbanItem._controller == null)
        return;
      TaskDetailPopup window = await kanbanItem._controller.SelectKanbanItem(dataContext, focusTitle);
      await Task.Delay(100);
      window?.TryShow();
      window = (TaskDetailPopup) null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/kanban/item/kanbanitem.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
          break;
        case 2:
          this.Container = (Border) target;
          break;
        case 3:
          this.ContentGrid = (DockPanel) target;
          break;
        case 4:
          this.ContentText = (TextBlock) target;
          break;
        case 5:
          this.ItemIcons = (KanbanItemIcons) target;
          break;
        case 6:
          this.TagBd = (Border) target;
          break;
        case 7:
          this.AvatarBorder = (Border) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
