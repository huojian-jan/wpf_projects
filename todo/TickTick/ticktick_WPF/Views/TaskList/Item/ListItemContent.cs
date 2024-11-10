// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.Item.ListItemContent
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.DateParser;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.MarkDown.Colorizer;
using ticktick_WPF.Views.QuickAdd;

#nullable disable
namespace ticktick_WPF.Views.TaskList.Item
{
  public class ListItemContent : Canvas
  {
    private const int TitleLeft = 38;
    private DisplayItemController _controller;
    private string _id;
    private List<AvatarViewModel> _avatars;
    private string _projectId;
    private bool _checkBoxDown;
    private bool _inProjectList;
    public bool InSticky;
    public TaskCheckIcon CheckIcon;
    private Border _openBorder;
    public TaskTitleBox TitleTextBox;
    private TextBlock _hintText;

    public ListItemContent()
    {
      TaskTitleBox taskTitleBox = new TaskTitleBox();
      taskTitleBox.MaxLength = 2048;
      this.TitleTextBox = taskTitleBox;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this.AddTextControl();
      this.SetResourceReference(FrameworkElement.HeightProperty, (object) "Height40");
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      this.CheckIcon.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckBoxClick);
      this.CheckIcon.PreviewMouseRightButtonUp += new MouseButtonEventHandler(this.CheckBoxRightMouseUp);
      this.CheckIcon.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnCheckBoxMouseDown);
      this.Children.Add((UIElement) this.CheckIcon);
      Canvas.SetLeft((UIElement) this.CheckIcon, 18.0);
      this.CheckIcon.SetResourceReference(Canvas.TopProperty, (object) "Font13");
      this.TitleTextBox.PreviewTextInput += new TextCompositionEventHandler(this.OnInput);
      this.Unloaded += (RoutedEventHandler) ((o, e) =>
      {
        this._controller?.Clear();
        this._controller = (DisplayItemController) null;
      });
    }

    private void OnInput(object sender, TextCompositionEventArgs e) => this.SetHintText(false);

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.TitleTextBox.Width = Math.Max(0.0, this.Width - 38.0 - 8.0);
    }

    private async void AddTextControl()
    {
      ListItemContent content = this;
      content.Children.Add((UIElement) content.TitleTextBox);
      content.TitleTextBox.QuickItemSelected += new EventHandler<QuickSetModel>(content.OnQuickItemSelected);
      content.TitleTextBox.SetParent(content);
      content.TitleTextBox.PopOpened += new EventHandler(content.OnQuickSetPopupOpened);
      content.TitleTextBox.PopClosed += new EventHandler(content.OnQuickSetPopupClosed);
      Canvas.SetLeft((UIElement) content.TitleTextBox, 38.0);
      content.TitleTextBox.SetResourceReference(Canvas.TopProperty, (object) "Double11Add1");
      content.TitleTextBox.SetResourceReference(Control.FontSizeProperty, (object) "Font14");
      content.TitleTextBox.VerticalAlignment = VerticalAlignment.Top;
      content.TitleTextBox.TextLostFocus += new EventHandler(content.TitleLostFocus);
      content.TitleTextBox.TextGotFocus += new EventHandler<RoutedEventArgs>(content.TitleGotFocus);
      content.TitleTextBox.LinkTextChange += new EventHandler(content.TitleTextChanged);
      content.TitleTextBox.MoveUp += new EventHandler(content.OnMoveUp);
      content.TitleTextBox.DateParsed += new EventHandler<IPaserDueDate>(content.OnDateParsed);
      content.TitleTextBox.MoveDown += new EventHandler(content.OnMoveDown);
      content.TitleTextBox.SplitText += new EventHandler<int>(content.OnSplit);
      content.TitleTextBox.MergeText += new EventHandler<string>(content.OnMergeText);
      content.TitleTextBox.IgnoreTokenChanged += new EventHandler<List<string>>(content.OnIgnoreTokenChanged);
      content.TitleTextBox.MultipleTextPaste += new EventHandler<string>(content.OnMultipleTextPaste);
      content.TitleTextBox.CaretChanged += new EventHandler<int>(content.TitleCaretChanged);
      content.TitleTextBox.Navigate += new EventHandler<ProjectTask>(content.OnNavigate);
      content.TitleTextBox.SelectDate += new EventHandler(content.OnSelectDateKeyDown);
      content.TitleTextBox.RequestBringIntoView += new RequestBringIntoViewEventHandler(content.EditorOnRequestBringIntoView);
      content.TitleTextBox.SetWordWrap(false);
      content.TitleTextBox.EnableSpellCheck = false;
    }

    public void SetHintText(bool show)
    {
      if (show)
      {
        if (this._hintText == null)
        {
          this._hintText = new TextBlock();
          Canvas.SetLeft((UIElement) this._hintText, 38.0);
          if (this.InSticky)
          {
            this._hintText.FontSize = 13.0;
            Canvas.SetTop((UIElement) this._hintText, 11.0);
          }
          else
          {
            this._hintText.SetResourceReference(Canvas.TopProperty, (object) "Double11Add1");
            this._hintText.SetResourceReference(TextBlock.FontSizeProperty, (object) "Font14");
          }
          this._hintText.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity20");
          this._hintText.SetResourceReference(TextBlock.TextProperty, (object) "NoTitle");
          this._hintText.IsHitTestVisible = false;
          this.Children.Add((UIElement) this._hintText);
        }
        this._hintText.TextDecorations = LocalSettings.Settings.ExtraSettings.ShowCompleteLine != 1 || (this.DataContext is DisplayItemModel dataContext ? (dataContext.Status != 0 ? 1 : 0) : 1) == 0 ? (TextDecorationCollection) null : DeleteLineColorizer.HintStrikethrough;
      }
      else
      {
        if (this._hintText == null)
          return;
        this.Children.Remove((UIElement) this._hintText);
        this._hintText = (TextBlock) null;
      }
    }

    private void ShowOpenIcon(bool show, bool open)
    {
      if (show)
      {
        if (this._openBorder == null)
        {
          this._openBorder = new Border();
          this.Children.Add((UIElement) this._openBorder);
          this._openBorder.SetResourceReference(Canvas.TopProperty, this.InSticky ? (object) "StickyFont14" : (object) "Font14");
          this._openBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpenPathClick);
          this._openBorder.Cursor = Cursors.Hand;
          this._openBorder.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_100");
          Path path1 = new Path();
          path1.Width = 12.0;
          path1.Height = 12.0;
          path1.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
          path1.Stretch = Stretch.Uniform;
          path1.VerticalAlignment = VerticalAlignment.Center;
          Path path2 = path1;
          path2.SetResourceReference(Shape.FillProperty, (object) "BaseColorOpacity100_80");
          path2.Data = Utils.GetIcon("ArrowLine");
          path2.RenderTransform = (Transform) new RotateTransform(open ? 0.0 : -90.0);
          this._openBorder.Child = (UIElement) path2;
          Canvas.SetLeft((UIElement) this._openBorder, 3.0);
        }
        else
        {
          if (!(this._openBorder.Child is Path child))
            return;
          RotateTransform rotateTransform = new RotateTransform(open ? 0.0 : -90.0);
          child.RenderTransform = (Transform) rotateTransform;
        }
      }
      else
      {
        if (this._openBorder == null)
          return;
        this.Children.Remove((UIElement) this._openBorder);
        this._openBorder = (Border) null;
      }
    }

    public bool TitleFocused => this.TitleTextBox.TextArea.IsKeyboardFocused;

    public bool IsIconMouseOver
    {
      get
      {
        Border openBorder = this._openBorder;
        return (openBorder != null ? (__nonvirtual (openBorder.IsMouseOver) ? 1 : 0) : 0) != 0 || this.CheckIcon.IsMouseOver;
      }
    }

    public bool BatchSelected => this.TitleTextBox.SelectionLength > 0;

    private void OnQuickSetPopupOpened(object sender, EventArgs e)
    {
      if (sender is Popup popup && popup.Equals((object) this.TitleTextBox.SelectionPopup))
        this._controller?.SetQuickSetPopup(popup);
      this._controller?.SetParentInOperation(true);
    }

    private async void OnQuickSetPopupClosed(object sender, EventArgs e)
    {
      this._controller?.SetParentInOperation(false);
      if (!(sender is Popup objA) || !object.Equals((object) objA, (object) this.TitleTextBox.SelectionPopup))
        return;
      this._controller?.SetQuickSetPopup((Popup) null);
      await Task.Delay(100);
      if (this.TitleFocused)
        return;
      this.TitleLostFocus((object) null, (EventArgs) null);
    }

    private async void OnQuickItemSelected(object sender, QuickSetModel e)
    {
      ListItemContent listItemContent = this;
      QuickSetModel quickSetModel1 = e;
      if ((quickSetModel1 != null ? (quickSetModel1.Type == QuickSetType.Date ? 1 : 0) : 0) != 0)
        listItemContent.TryClearParseDate();
      if (listItemContent._controller != null)
        await listItemContent._controller.OnInputSelected(e, listItemContent.TitleTextBox.Text, !listItemContent.TitleTextBox.ParsingDate);
      if (!listItemContent.TitleTextBox.ParsingDate)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        QuickSetModel quickSetModel2 = e;
        if ((quickSetModel2 != null ? (quickSetModel2.Type == QuickSetType.Project ? 1 : 0) : 0) == 0)
          model = (DisplayItemModel) null;
        else if (!(listItemContent.DataContext is DisplayItemModel model))
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.Id);
          if (thinTaskById == null)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            thinTaskById.startDate = model.StartDate;
            thinTaskById.dueDate = model.DueDate;
            if (MoveToastHelper.CheckTaskMatched(listItemContent._controller?.GetProjectIdentify(), thinTaskById))
            {
              model = (DisplayItemModel) null;
            }
            else
            {
              string taskName = listItemContent.SaveParsedDate(model);
              IToastShowWindow toastWindow = Utils.GetToastWindow();
              if (toastWindow == null)
              {
                model = (DisplayItemModel) null;
              }
              else
              {
                toastWindow.ToastMoveProjectControl(thinTaskById.projectId, taskName);
                model = (DisplayItemModel) null;
              }
            }
          }
        }
      }
    }

    public void TryClearParseDate()
    {
      this.TitleTextBox.ClearParseDate();
      if (this._inProjectList)
        Utils.FindParent<TaskView>((DependencyObject) this)?.ClearDetailParseDate();
      TaskListItemFocusHelper.ParseDateId = string.Empty;
      TaskListItemFocusHelper.ParseDateItem = (ListItemContent) null;
    }

    public async void TrySaveParseDate()
    {
      ListItemContent listItemContent = this;
      await Task.Delay(100);
      if (!listItemContent.TitleTextBox.CanParseDate || !(listItemContent.DataContext is DisplayItemModel dataContext))
        return;
      listItemContent.SaveParsedDate(dataContext);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      DisplayItemModel newValue = e.NewValue as DisplayItemModel;
      if (e.OldValue is DisplayItemModel oldValue)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnPropertyChange), "");
      if (newValue == null)
        return;
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnPropertyChange), "");
      if (newValue.IsNewAdd)
        this.TrySetCanParse();
      newValue.SetIcon();
      this.CheckIcon.Cursor = !newValue.Enable ? Cursors.No : (newValue.IsEvent || newValue.IsNote || newValue.IsCourse ? Cursors.Arrow : Cursors.Hand);
      this.CheckIcon.SetIconColor(newValue.Type, newValue.Priority, newValue.Status);
      if (this._id != newValue.Id)
      {
        this._id = newValue.Id;
        this.TitleTextBox.AutoGetUrlTitle = !newValue.IsEvent;
        this.TitleTextBox.LinkTextChange -= new EventHandler(this.TitleTextChanged);
        this.TitleTextBox.CaretChanged -= new EventHandler<int>(this.TitleCaretChanged);
        this.TitleTextBox.SetTextOffset(newValue.Title, true, true);
        this.TitleTextBox.LinkTextChange += new EventHandler(this.TitleTextChanged);
        this.TitleTextBox.CaretChanged += new EventHandler<int>(this.TitleCaretChanged);
        this.TitleTextBox.SetMarkRegexText(newValue != null && newValue.Type == DisplayType.Event, false);
      }
      else if (this.TitleTextBox.Text != newValue.Title)
        this.TitleTextBox.SetTextOffset(newValue.Title, true);
      this.TitleTextBox.TextStatus = newValue.Status;
      this.TitleTextBox.ReadOnly = newValue.ReadOnly;
      if (newValue.CaretIndex.HasValue)
      {
        this.TitleTextBox.SetFocus(newValue.CaretIndex.Value);
        newValue.CaretIndex = new int?();
      }
      if (newValue.IsTaskOrNote && this._projectId != newValue.ProjectId)
      {
        this._projectId = newValue.ProjectId;
        this._avatars = (List<AvatarViewModel>) null;
      }
      if (this._controller == null)
        this._controller = new DisplayItemController((UIElement) this, newValue);
      else
        this._controller.Reset((UIElement) this, newValue);
      this.ShowOpenIcon(newValue.HasChildren && newValue.Level < 4, newValue.IsOpen);
      this.SetHintText(string.IsNullOrEmpty(newValue.Title));
    }

    private void OnPropertyChange(object sender, PropertyChangedEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      switch (e.PropertyName)
      {
        case "Type":
        case "Priority":
          this.CheckIcon.SetIconColor(dataContext.Type, dataContext.Priority, dataContext.Status);
          break;
        case "Status":
          this.CheckIcon.SetIconColor(dataContext.Type, dataContext.Priority, dataContext.Status);
          this.TitleTextBox.TextStatus = dataContext.Status;
          this.SetHintText(string.IsNullOrEmpty(dataContext.Title));
          break;
        case "ReadOnly":
          this.TitleTextBox.ReadOnly = dataContext.ReadOnly;
          break;
        case "IsOpen":
        case "HasChildren":
          this.ShowOpenIcon(dataContext.HasChildren && dataContext.Level < 4, dataContext.IsOpen);
          break;
      }
    }

    private void TitleLostFocus(object sender, EventArgs e)
    {
      DisplayItemModel dataContext = this.DataContext as DisplayItemModel;
      try
      {
        if (this.TitleTextBox.ReadOnly || this.TitleTextBox.SelectionPopupOpened && this.TitleTextBox.ParsingDate)
          return;
        if (this.TitleTextBox.ParsedData == null)
        {
          this.TitleTextBox.SetCanParseDate(false);
        }
        else
        {
          if (dataContext == null || !dataContext.Enable)
            return;
          if (this.TitleTextBox.CanParseDate || dataContext.IsNewAdd)
          {
            if (this._inProjectList)
            {
              TaskView parent = Utils.FindParent<TaskView>((DependencyObject) this);
              if ((parent != null ? (parent.DetailTitleFocus() ? 1 : 0) : 0) != 0)
                goto label_9;
            }
            this.SaveParsedDate(dataContext);
            return;
          }
label_9:
          if (this.TitleTextBox.ParsingDate || dataContext.IsNewAdd)
            return;
          this._controller?.TitleLostFocus(false);
        }
      }
      finally
      {
        if (dataContext != null && dataContext.IsNewAdd)
        {
          dataContext.IsNewAdd = false;
          this._controller?.TitleLostFocus(true);
        }
        this.TitleTextBox.ScrollToHome();
      }
    }

    private void TitleGotFocus(object sender, RoutedEventArgs e)
    {
      this.TrySetCanParse();
      if (Mouse.RightButton == MouseButtonState.Pressed)
        return;
      this._controller?.TitleGotFocus();
    }

    private string SaveParsedDate(DisplayItemModel model)
    {
      model.IsNewAdd = false;
      IPaserDueDate parsedData = this.TitleTextBox.ParsedData;
      this.TitleTextBox.SetCanParseDate(false);
      string parsedText = this.TitleTextBox.GetParsedText();
      TaskListItemFocusHelper.ParseDateId = string.Empty;
      TaskListItemFocusHelper.ParseDateItem = (ListItemContent) null;
      this._controller?.SetDateAndTitle(parsedText, parsedData);
      this.TitleTextBox.ScrollToHome();
      this._controller?.TitleLostFocus(true);
      model.ParseData = (TimeData) null;
      return parsedText;
    }

    private void TrySetCanParse()
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || this.TitleTextBox.CanParseDate)
        return;
      bool canParseDate = dataContext.IsNewAdd;
      if (!dataContext.IsTask)
        canParseDate = false;
      else if (!canParseDate)
      {
        TimeData timeData = this._controller?.GetDefaultDate() ?? TimeData.BuildDefaultStartAndEnd();
        if (dataContext.StartDate.HasValue)
        {
          DateTime? nullable = dataContext.StartDate;
          DateTime? startDate = timeData.StartDate;
          if ((nullable.HasValue == startDate.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == startDate.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
          {
            DateTime? dueDate = dataContext.DueDate;
            nullable = timeData.DueDate;
            if ((dueDate.HasValue == nullable.HasValue ? (dueDate.HasValue ? (dueDate.GetValueOrDefault() == nullable.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
            {
              bool? isAllDay1 = dataContext.IsAllDay;
              bool? isAllDay2 = timeData.IsAllDay;
              if (!(isAllDay1.GetValueOrDefault() == isAllDay2.GetValueOrDefault() & isAllDay1.HasValue == isAllDay2.HasValue))
                goto label_9;
            }
            else
              goto label_9;
          }
          else
            goto label_9;
        }
        canParseDate = true;
      }
label_9:
      TaskListItemFocusHelper.FocusingItem = this;
      this.TitleTextBox.SetCanParseDate(canParseDate);
      if (canParseDate)
      {
        DisplayItemController controller = this._controller;
        this._inProjectList = controller != null && controller.InProjectList();
        if (this._inProjectList)
        {
          TaskListItemFocusHelper.ParseDateId = dataContext.Id;
          TaskListItemFocusHelper.ParseDateItem = this;
        }
        else
        {
          TaskListItemFocusHelper.ParseDateId = string.Empty;
          TaskListItemFocusHelper.ParseDateItem = (ListItemContent) null;
        }
      }
      else
      {
        this._inProjectList = false;
        TaskListItemFocusHelper.ParseDateId = string.Empty;
        TaskListItemFocusHelper.ParseDateItem = (ListItemContent) null;
      }
    }

    protected virtual void TitleTextChanged(object sender, EventArgs e)
    {
      string text = this.TitleTextBox.Text;
      this.SetHintText(string.IsNullOrEmpty(text));
      this._controller?.TitleTextChanged(text);
    }

    private void OnMoveUp(object sender, EventArgs e)
    {
      this.TitleTextBox.ScrollToHome();
      this._controller?.MoveUp();
    }

    private void OnMoveDown(object sender, EventArgs e)
    {
      this.TitleTextBox.ScrollToHome();
      this._controller?.MoveDown();
    }

    private void OnMergeText(object sender, string text) => this._controller?.OnMergeText(text);

    private void OnMultipleTextPaste(object sender, string text)
    {
      this._controller?.OnMultipleTextPaste(text);
    }

    private void TitleCaretChanged(object sender, int caretIndex)
    {
      if (!this.TitleTextBox.TextArea.IsKeyboardFocused)
        return;
      this._controller?.TitleSelectionChanged(caretIndex);
    }

    private async void OnCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      ListItemContent child = this;
      if (!child._checkBoxDown)
        model = (DisplayItemModel) null;
      else if (!(child.DataContext is DisplayItemModel model))
        model = (DisplayItemModel) null;
      else if (model.IsToggling)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        model.IsToggling = true;
        child._checkBoxDown = false;
        bool soundPlayed = false;
        TaskListItem parent = Utils.FindParent<TaskListItem>((DependencyObject) child);
        if (parent != null)
          soundPlayed = await parent.TryPlayCompleteStory(model);
        child._controller?.OnCheckBoxClick(model, soundPlayed);
        child.RemoveFocus();
        model = (DisplayItemModel) null;
      }
    }

    public void RemoveFocus()
    {
      this.TitleTextBox.TabIndex = -1;
      Keyboard.ClearFocus();
    }

    public void FocusTitle() => this.TitleTextBox.Focus();

    public bool IsNote() => this.DataContext is DisplayItemModel dataContext && dataContext.IsNote;

    private void OnNavigate(object sender, ProjectTask e)
    {
      Utils.FindParent<IListViewParent>((DependencyObject) this)?.NavigateTask(e);
    }

    private void OnSplit(object sender, int e) => this._controller?.SplitNewTask((SplitData) null);

    public string GetProjectId()
    {
      return this.DataContext is DisplayItemModel dataContext && dataContext.IsTaskOrNote ? dataContext.ProjectId : (string) null;
    }

    private void EditorOnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }

    private void OnSelectDateKeyDown(object sender, EventArgs e)
    {
      if (this.InSticky)
        return;
      this._controller?.SelectDate();
    }

    private void OnCheckBoxMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._checkBoxDown = true;
    }

    private void OnDateParsed(object sender, IPaserDueDate e)
    {
      this._controller?.SetParsedDate(e?.ToTimeData(true));
    }

    private void OnIgnoreTokenChanged(object sender, List<string> e)
    {
      if (!this._inProjectList)
        return;
      Utils.FindParent<TaskView>((DependencyObject) this)?.GetTaskDetail()?.GetTitleText().SetIgnoreToken(e);
    }

    public void SetIgnoreTokens(List<string> tokens) => this.TitleTextBox.SetIgnoreToken(tokens);

    private void OnOpenPathClick(object sender, MouseButtonEventArgs e)
    {
      this._controller?.OnOpenPathClick(sender, e);
      e.Handled = true;
    }

    private void CheckBoxRightMouseUp(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this._controller?.OnCheckBoxRightMouseUp((UIElement) this.CheckIcon);
    }

    public void SetStickyMode()
    {
      this.InSticky = true;
      this.SetResourceReference(FrameworkElement.HeightProperty, (object) "StickyHeight30");
      this.CheckIcon.SetResourceReference(Canvas.TopProperty, (object) "StickyFont13");
      this._openBorder?.SetResourceReference(Canvas.TopProperty, (object) "StickyFont14");
      this.TitleTextBox.SetResourceReference(Canvas.TopProperty, (object) "StickyFont11");
      this.TitleTextBox.SetBaseColor("StickyContentTextColor", "StickyContentTextColor", "StickyCompletedTextColor", "StickyTextColor20");
      this.TitleTextBox.SetLightTheme();
      this.TitleTextBox.SetResourceReference(Control.FontSizeProperty, (object) "StickyFont13");
      if (this._hintText == null)
        return;
      this._hintText.SetResourceReference(Control.FontSizeProperty, (object) "StickyFont13");
      this._hintText.SetResourceReference(Canvas.TopProperty, (object) "StickyFont11");
    }
  }
}
