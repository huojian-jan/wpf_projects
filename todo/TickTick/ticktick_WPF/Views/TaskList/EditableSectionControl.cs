// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.EditableSectionControl
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.Misc;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class EditableSectionControl : Grid, IComponentConnector
  {
    private TaskListView _parent;
    private bool _startDrag;
    public static readonly DependencyProperty InOperateProperty = DependencyProperty.Register(nameof (InOperate), typeof (bool), typeof (EditableSectionControl), new PropertyMetadata((object) false));
    internal EditableSectionControl Root;
    internal Grid Container;
    internal StackPanel NormalGrid;
    internal Path OpenIndicator;
    internal StackPanel OptionPanel;
    internal Border AddButton;
    internal Border MoreButton;
    internal Image MoreImage;
    internal EscPopup MorePopup;
    internal TextBlock RightText;
    internal TextBox EditBox;
    internal Popup ErrorPopup;
    private bool _contentLoaded;

    private DisplayItemModel Model => this.DataContext as DisplayItemModel;

    protected bool Editing { get; set; }

    public bool InOperate
    {
      get => (bool) this.GetValue(EditableSectionControl.InOperateProperty);
      set => this.SetValue(EditableSectionControl.InOperateProperty, (object) value);
    }

    public EditableSectionControl()
    {
      this.InitializeComponent();
      this.EditBox.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataContextChanged);
      this.Container.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSectionClick);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    protected EditableSectionControl(bool inherit)
    {
    }

    private void OnSectionClick(object sender, MouseButtonEventArgs e)
    {
      if (this.InOperate || TaskDragHelpModel.DragHelp.IsDragging)
        return;
      this.ChangeOpenStatus();
    }

    private void ChangeOpenStatus()
    {
      DisplayItemModel model = this.Model;
      if (model?.Section?.Children == null)
        return;
      model.Num = model.Section.Children.Count;
      foreach (DisplayItemModel child in model.Section.Children)
        model.Num += child.GetChildrenModels(true).Count;
      model.IsOpen = !model.IsOpen;
      ISectionList parent = Utils.FindParent<ISectionList>((DependencyObject) this);
      if (parent == null)
        return;
      parent.OnSectionStatusChanged(new SectionStatus()
      {
        SectionId = model.Section.SectionId,
        IsOpen = model.IsOpen
      });
    }

    private TaskListView GetParent()
    {
      this._parent = this._parent ?? Utils.FindParent<TaskListView>((DependencyObject) this);
      return this._parent;
    }

    private async void OnAddTaskClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (this.InOperate)
        return;
      this.AddButtonClick();
    }

    protected virtual void AddButtonClick()
    {
      DisplayItemModel model = this.Model;
      if (model == null)
        return;
      if (!model.IsOpen)
      {
        this.ChangeOpenStatus();
        this.GetParent()?.UpdateLayout();
      }
      this.GetParent()?.AddTaskInSection(model);
    }

    private void OnMoreClick(object sender, MouseButtonEventArgs e)
    {
      if (this.InOperate)
        return;
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      CustomSectionOption option = new CustomSectionOption(this.MorePopup);
      this.SetOptionAction(option);
      DisplayItemModel model = this.Model;
      option.Show(model?.Section.SectionId, model?.Section.ProjectId);
      this.InOperate = true;
    }

    protected virtual void SetOptionAction(CustomSectionOption option)
    {
      DisplayItemModel dataContext = this.DataContext as DisplayItemModel;
      bool flag = false;
      if (dataContext != null)
        flag = dataContext.Enable && dataContext.Section is CustomizedSection section && section.CanDelete;
      option.SetAction(new Action(this.AddSectionUpper), new Action(this.AddSectionUnder), new Action(this.Rename), flag ? new Action(this.Delete) : (Action) null, new Action<string, string, SelectableItemViewModel>(this.OnMoveColumnItemSelected));
    }

    private async void OnPopupClosed(object sender, EventArgs e)
    {
      await Task.Delay(200);
      this.InOperate = false;
    }

    private void OnMoveColumnItemSelected(
      string columnId,
      string originProjectId,
      SelectableItemViewModel e)
    {
      string id = e.Id;
      if (!(id != originProjectId))
        return;
      TaskService.MoveColumnAsync(columnId, originProjectId, id, true);
    }

    private void AddSectionUpper()
    {
      this.MorePopup.IsOpen = false;
      this.GetParent()?.AddNewSection(this.Model, false);
    }

    private void AddSectionUnder()
    {
      this.MorePopup.IsOpen = false;
      this.GetParent()?.AddNewSection(this.Model, true);
    }

    private async void Rename()
    {
      EditableSectionControl editableSectionControl = this;
      editableSectionControl.MorePopup.IsOpen = false;
      editableSectionControl.SetEditing(true);
      editableSectionControl.EditBox.Text = editableSectionControl.Model.Title;
      await Task.Delay(50);
      editableSectionControl.EditBox.Focus();
      editableSectionControl.EditBox.SelectAll();
      editableSectionControl.EditBox.LostFocus -= new RoutedEventHandler(editableSectionControl.OnLostFocus);
      editableSectionControl.EditBox.LostFocus += new RoutedEventHandler(editableSectionControl.OnLostFocus);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      if (this.EditBox.Visibility != Visibility.Visible)
        return;
      this.SetEditing(false);
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
      string str = this.EditBox.Text.Trim();
      if (!this.Editing)
      {
        this.SetEditing(false);
      }
      else
      {
        this.SetEditing(false);
        if (!string.IsNullOrEmpty(str) && NameUtils.IsValidColumnName(str))
        {
          this.SaveSectionName(str);
        }
        else
        {
          if (string.IsNullOrEmpty(str))
            Utils.Toast(Utils.GetString("SectionNameCannotBeEmpty"));
          DisplayItemModel model = this.Model;
          if (model != null && model.NewAdd)
          {
            model.NewAdd = false;
            this.GetParent()?.RemoveSection(model.Id);
            return;
          }
        }
        this.ErrorPopup.IsOpen = false;
      }
    }

    private async Task SaveSectionName(string saveName)
    {
      DisplayItemModel model = this.Model;
      if (string.IsNullOrEmpty(saveName))
        model = (DisplayItemModel) null;
      else if (model?.Section == null)
        model = (DisplayItemModel) null;
      else if (saveName == model.Title)
        model = (DisplayItemModel) null;
      else if (await this.CheckIfColumnNameExisted(saveName))
      {
        Utils.Toast(Utils.GetString("SectionNameExisted"));
        if (!model.NewAdd)
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          model.NewAdd = false;
          TaskListView parent = this.GetParent();
          if (parent == null)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            parent.RemoveSection(model.Id);
            model = (DisplayItemModel) null;
          }
        }
      }
      else
      {
        if (model.NewAdd)
        {
          ColumnModel columnModel = await ColumnDao.AddColumn(saveName, model.ProjectId, model.SortOrder, model.Id);
          model.NewAdd = false;
          model.Section.SectionId = columnModel.id;
          model.SourceViewModel.Id = columnModel.id;
          this.GetParent()?.LoadAsync(true);
        }
        await ColumnDao.SaveColumnName(saveName, model.Section.SectionId);
        model.SourceViewModel.Title = saveName;
        model.NotifyPropertyChanged("Title");
        SyncManager.TryDelaySync();
        model = (DisplayItemModel) null;
      }
    }

    private async Task<bool> CheckIfColumnNameExisted(string name)
    {
      if (name == this.Model.Title)
        return false;
      List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(this.Model.ProjectId);
      return columnsByProjectId != null && columnsByProjectId.Exists((Predicate<ColumnModel>) (c => c.name == name));
    }

    private async void Delete()
    {
      EditableSectionControl editableSectionControl = this;
      editableSectionControl.MorePopup.IsOpen = false;
      DisplayItemModel model = editableSectionControl.Model;
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("DeleteColumn"), Utils.GetString("DeleteSectionHint"), Utils.GetString(nameof (Delete)), Utils.GetString("Cancel"));
        customerDialog.Owner = Window.GetWindow((DependencyObject) editableSectionControl);
        bool? nullable = customerDialog.ShowDialog();
        bool flag = true;
        if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        {
          model = (DisplayItemModel) null;
        }
        else
        {
          await ColumnDao.DeleteColumn(model.Id);
          string projectDefaultColumnId = await ColumnDao.GetProjectDefaultColumnId(model.ProjectId);
          UtilLog.Info("SectionControl.Delete : " + model.Id);
          await TaskService.BatchDeleteTaskInCustomSection(model.ProjectId, model.Id, projectDefaultColumnId);
          Utils.Toast(Utils.GetString("Deleted"));
          SyncManager.TryDelaySync();
          TaskListView parent = editableSectionControl.GetParent();
          if (parent == null)
          {
            model = (DisplayItemModel) null;
          }
          else
          {
            parent.LoadAsync(true);
            model = (DisplayItemModel) null;
          }
        }
      }
    }

    private void OnNameTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!NameUtils.IsValidColumnName(this.EditBox.Text.Trim()))
      {
        this.ErrorPopup.IsOpen = true;
        this.EditBox.SelectAll();
      }
      else
        this.ErrorPopup.IsOpen = false;
    }

    public async void ShowEditBox()
    {
      EditableSectionControl editableSectionControl = this;
      editableSectionControl.SetEditing(true);
      editableSectionControl.EditBox.Text = "";
      await Task.Delay(50);
      editableSectionControl.EditBox.Focus();
      editableSectionControl.EditBox.SelectAll();
      editableSectionControl.EditBox.LostFocus -= new RoutedEventHandler(editableSectionControl.OnLostFocus);
      editableSectionControl.EditBox.LostFocus += new RoutedEventHandler(editableSectionControl.OnLostFocus);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      if (this.Model == null)
        return;
      switch (e.Key)
      {
        case Key.Return:
          string str = this.EditBox.Text.Trim();
          if (!string.IsNullOrEmpty(str) && NameUtils.IsValidColumnName(str))
          {
            this.SetEditing(false);
            this.SaveSectionName(str);
          }
          else if (!this.Model.NewAdd)
            this.SetEditing(false);
          if (!string.IsNullOrEmpty(str))
            break;
          Utils.Toast(Utils.GetString("SectionNameCannotBeEmpty"));
          break;
        case Key.Escape:
          this.SetEditing(false);
          if (!this.Model.NewAdd)
            break;
          this.Model.NewAdd = false;
          TaskListView parent = this.GetParent();
          if (parent == null)
            break;
          parent.RemoveSection(this.Model.Id);
          break;
      }
    }

    public void SetEditing(bool edit)
    {
      TaskListView parent = Utils.FindParent<TaskListView>((DependencyObject) this);
      if (parent != null)
        parent.SectionEditing = edit;
      this.Editing = edit;
      if (edit)
      {
        this.EditBox.Visibility = Visibility.Visible;
        this.NormalGrid.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.EditBox.Visibility = Visibility.Collapsed;
        this.EditBox.Text = "";
        this.NormalGrid.Visibility = Visibility.Visible;
      }
    }

    private void OnDragMouseDown(object sender, MouseButtonEventArgs e) => this._startDrag = true;

    private void OnDragMouseMove(object sender, MouseEventArgs e)
    {
      if (this._startDrag && e.LeftButton == MouseButtonState.Pressed)
        this.StartDrag(e);
      this._startDrag = false;
    }

    protected virtual void StartDrag(MouseEventArgs e)
    {
      this.Model.Dragging = true;
      this.GetParent()?.StartDragSection(this.Model);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is DisplayItemModel newValue) || !newValue.IsSection)
        return;
      newValue.Num = newValue.Section.Children.Count;
      foreach (DisplayItemModel child in newValue.Section.Children)
        newValue.Num += child.GetChildrenModels(true).Count;
      this.EditBox.Tag = newValue.NewAdd ? (object) Utils.GetString("AddSectionHint") : (object) "";
      if (!newValue.NewAdd)
        return;
      this.ShowEditBox();
    }

    private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      TaskListView parent = Utils.FindParent<TaskListView>((DependencyObject) this);
      if (parent == null)
        return;
      parent.SectionEditing = this.EditBox.Visibility == Visibility.Visible;
    }

    private void RightTextMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext))
        return;
      bool flag = dataContext.SectionRightActionText == Utils.GetString("DeselectAll");
      e.Handled = true;
      if (!dataContext.IsOpen)
      {
        dataContext.IsOpen = true;
        ISectionList parent = Utils.FindParent<ISectionList>((DependencyObject) this);
        if (parent != null)
          parent.OnSectionStatusChanged(new SectionStatus()
          {
            SectionId = dataContext.Section.SectionId,
            IsOpen = dataContext.IsOpen
          });
      }
      Utils.FindParent<ISectionList>((DependencyObject) this)?.SelectOrDeselectAll(dataContext, !flag);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tasklist/editablesectioncontrol.xaml", UriKind.Relative));
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
          this.Root = (EditableSectionControl) target;
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnDragMouseDown);
          ((UIElement) target).MouseMove += new MouseEventHandler(this.OnDragMouseMove);
          break;
        case 4:
          this.NormalGrid = (StackPanel) target;
          break;
        case 5:
          this.OpenIndicator = (Path) target;
          break;
        case 6:
          this.OptionPanel = (StackPanel) target;
          break;
        case 7:
          this.AddButton = (Border) target;
          this.AddButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAddTaskClick);
          break;
        case 8:
          this.MoreButton = (Border) target;
          this.MoreButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
          break;
        case 9:
          this.MoreImage = (Image) target;
          break;
        case 10:
          this.MorePopup = (EscPopup) target;
          break;
        case 11:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.RightTextMouseUp);
          break;
        case 12:
          this.RightText = (TextBlock) target;
          break;
        case 13:
          this.EditBox = (TextBox) target;
          this.EditBox.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.OnVisibleChanged);
          this.EditBox.TextChanged += new TextChangedEventHandler(this.OnNameTextChanged);
          break;
        case 14:
          this.ErrorPopup = (Popup) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
