// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.ProjectOrGroupPopup
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.IdentityModel.Tokens;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class ProjectOrGroupPopup : UserControl, ITabControl, IComponentConnector
  {
    private readonly bool _batchMode;
    private readonly Popup _popup;
    private ItemSelection _projectItems;
    internal Grid SearchGrid;
    internal TextBox SearchProjectTextBox;
    internal Grid OperationPanel;
    internal Grid ItemsContainer;
    private bool _contentLoaded;

    public event EventHandler<SelectableItemViewModel> ItemSelect;

    public event EventHandler Closed;

    public event EventHandler<ProjectExtra> Save;

    public ProjectOrGroupPopup(Popup popup, ProjectExtra data, ProjectSelectorExtra extra)
    {
      this._popup = popup;
      this._batchMode = extra.batchMode;
      this.InitializeComponent();
      data = data ?? new ProjectExtra();
      if (!extra.ShowInbox.HasValue)
        extra.ShowInbox = new bool?(data.ProjectIds.IsNullOrEmpty<string>() || data.ProjectIds.Contains(LocalSettings.Settings.InServerBoxId) || LocalSettings.Settings.SmartListInbox != 1);
      if (!extra.isCalendarMode)
      {
        this.InitProjectItems(data, extra);
        this._popup.MinHeight = 240.0;
      }
      else
      {
        this.InitCalendarItems(data, extra.accountId);
        this._popup.MinHeight = 0.0;
      }
      if (!this._batchMode)
        this.OperationPanel.Visibility = Visibility.Collapsed;
      if (extra.CanSearch)
        this.SearchGrid.Visibility = Visibility.Visible;
      this._popup.Width = extra.popupWidth;
      this._popup.Closed += new EventHandler(this.OnPopupClosed);
      this.Loaded += (RoutedEventHandler) ((sender, args) => this.DelayFocus());
    }

    public async void DelayFocus()
    {
      if (this.SearchGrid.Visibility != Visibility.Visible)
        return;
      await Task.Delay(250);
      if (this.SearchProjectTextBox.IsFocused)
        return;
      this.SearchProjectTextBox.Focus();
    }

    private void InitCalendarItems(ProjectExtra data, string accountId)
    {
      this._projectItems = new ItemSelection(this._popup)
      {
        AccountId = accountId,
        OriginalData = data
      };
      this._projectItems.Container.MinHeight = 0.0;
      this._projectItems.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnItemSelect);
      this.ItemsContainer.Children.Add((UIElement) this._projectItems);
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      EventHandler closed = this.Closed;
      if (closed == null)
        return;
      closed((object) this, (EventArgs) null);
    }

    private void InitProjectItems(ProjectExtra data, ProjectSelectorExtra extra)
    {
      this._projectItems = new ItemSelection(this._popup, extra.canSelectGroup)
      {
        ShowAll = extra.showAll,
        ShowFilters = extra.showFilters,
        ShowTags = extra.showTags,
        ShowSmartProjects = extra.showSmartProjects,
        ShowCalendars = extra.showCalendars,
        BatchMode = extra.batchMode,
        OriginalData = data,
        CanSelectGroup = extra.canSelectGroup,
        OnlyShowPermission = extra.onlyShowPermission,
        ShowSmartAll = extra.showSmartAll,
        ShowFilterGroup = extra.showFilterGroup,
        ShowNoteProject = extra.showNoteProject,
        ShowSharedProject = extra.showSharedProject,
        ShowListGroup = extra.showListGroup,
        ShowFilterCalendar = extra.showCalendarCategory,
        ShowHabitCategory = extra.showHabitCategory,
        ShowColumns = extra.ShowColumn,
        SelectedColumn = extra.ColumnId,
        ShowAllProjectsCategory = extra.showAllProjectCategory,
        ShowInbox = ((int) extra.ShowInbox ?? 1) != 0
      };
      this._projectItems.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnItemSelect);
      this.ItemsContainer.Children.Add((UIElement) this._projectItems);
    }

    private async void OnItemSelect(object sender, SelectableItemViewModel model)
    {
      ProjectOrGroupPopup sender1 = this;
      if (sender1._batchMode)
        return;
      sender1._popup.IsOpen = false;
      await Task.Delay(80);
      EventHandler<SelectableItemViewModel> itemSelect = sender1.ItemSelect;
      if (itemSelect == null)
        return;
      itemSelect((object) sender1, model);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this._popup.IsOpen = false;

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      EventHandler<ProjectExtra> save = this.Save;
      if (save != null)
        save((object) this, this._projectItems.GetSelectedData());
      this._popup.IsOpen = false;
    }

    public void Show()
    {
      this._popup.Child = (UIElement) this;
      this._popup.IsOpen = true;
    }

    public void SetData(ProjectExtra data, string columnId = null)
    {
      this._projectItems.OriginalData = data;
      this._projectItems.SelectedColumn = columnId;
      this._projectItems.LoadData();
    }

    public bool ChildMouseOver()
    {
      return this._projectItems != null && this._projectItems.ChildMouseOver();
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this._projectItems.LoadData(this.SearchProjectTextBox.Text.Trim());
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
    }

    public void SetPopupPosition(System.Windows.Point point)
    {
      this._popup.HorizontalOffset = point.X;
      this._popup.VerticalOffset = point.Y;
    }

    public bool HandleTab(bool shift)
    {
      ItemSelection projectItems = this._projectItems;
      // ISSUE: explicit non-virtual call
      return projectItems != null && __nonvirtual (projectItems.HandleTab(shift));
    }

    public bool HandleEnter()
    {
      ItemSelection projectItems = this._projectItems;
      // ISSUE: explicit non-virtual call
      return projectItems != null && __nonvirtual (projectItems.HandleEnter());
    }

    public bool HandleEsc()
    {
      ItemSelection projectItems = this._projectItems;
      // ISSUE: explicit non-virtual call
      return projectItems != null && __nonvirtual (projectItems.HandleEsc());
    }

    public bool UpDownSelect(bool isUp)
    {
      ItemSelection projectItems = this._projectItems;
      // ISSUE: explicit non-virtual call
      return projectItems != null && __nonvirtual (projectItems.UpDownSelect(isUp));
    }

    public bool LeftRightSelect(bool isLeft)
    {
      ItemSelection projectItems = this._projectItems;
      // ISSUE: explicit non-virtual call
      return projectItems != null && __nonvirtual (projectItems.LeftRightSelect(isLeft));
    }

    public void EnterSelect() => this._projectItems?.EnterSelect();

    public async void HoverSelectFirst()
    {
      await Task.Delay(200);
      this._projectItems?.HoverSelectFirst();
    }

    public void ClosePopup() => this._projectItems.ClosePopup();

    public Popup GetProjectPopup() => this._popup;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/project/projectorgrouppopup.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.SearchGrid = (Grid) target;
          break;
        case 2:
          this.SearchProjectTextBox = (TextBox) target;
          this.SearchProjectTextBox.KeyDown += new KeyEventHandler(this.OnKeyDown);
          this.SearchProjectTextBox.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          break;
        case 3:
          this.OperationPanel = (Grid) target;
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 6:
          this.ItemsContainer = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
