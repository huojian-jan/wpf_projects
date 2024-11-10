// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.NormalFilterControl
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class NormalFilterControl : UserControl, IComponentConnector
  {
    private NormalFilterViewModel _viewModel;
    private ProjectOrGroupPopup _projectEditDialog;
    private DateEditDialog _dateEditDialog;
    private TagEditDialog _tagEditDialog;
    private AssigneeEditDialog _assignEditDialog;
    private bool _useInSearchFilter;
    internal EmjTextBlock listTextBox;
    internal EscPopup listPopup;
    internal Grid tagGrid;
    internal EmjTextBlock tagTextBox;
    internal EscPopup tagPopup;
    internal TextBlock dateTextBox;
    internal EscPopup datePopup;
    internal RadioButton rbPriority;
    internal Grid assigneeGrid;
    internal Grid AssignVersion3Grid;
    internal EmjTextBlock AssignTextBox;
    internal EscPopup AssignPopup;
    internal Grid KeywordsGrid;
    internal TextBox KeywordsText;
    internal Grid TaskTypeGrid;
    internal RadioButton AllType;
    private bool _contentLoaded;

    public event EventHandler NotifyInvalid;

    public event EventHandler RuleChanged;

    public NormalFilterViewModel ViewModel
    {
      set
      {
        this._viewModel = value;
        this.DataContext = (object) this._viewModel;
      }
    }

    public bool CheckValid { get; set; }

    public bool PopupOpened
    {
      get
      {
        return this.AssignPopup.IsOpen || this.listPopup.IsOpen || this.tagPopup.IsOpen || this.datePopup.IsOpen;
      }
    }

    public NormalFilterControl() => this.InitializeComponent();

    public async void Init(bool inMatrix = false)
    {
      this._viewModel.DisplayProjectText = ProjectOrGroupEditDialog.GetDisplayProjectText(this._viewModel.Projects, this._viewModel.Groups);
      List<TagModel> tags = TagDataHelper.GetTags();
      if ((tags == null || tags.Count == 0) && (this._viewModel.Tags == null || this._viewModel.Tags.Count == 0))
        this.tagGrid.Visibility = Visibility.Collapsed;
      if (inMatrix)
      {
        this.assigneeGrid.Visibility = Visibility.Collapsed;
        this.KeywordsGrid.Visibility = Visibility.Collapsed;
      }
      else
      {
        if (!CacheManager.GetProjects().Any<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsShareList())))
          this.assigneeGrid.Visibility = Visibility.Collapsed;
        TextBox keywordsText = this.KeywordsText;
        List<string> keywords = this._viewModel.Keywords;
        // ISSUE: explicit non-virtual call
        string str = (keywords != null ? (__nonvirtual (keywords.Count) > 0 ? 1 : 0) : 0) != 0 ? this._viewModel.Keywords[0] : string.Empty;
        keywordsText.Text = str;
      }
    }

    private void OnDateClick(object sender, MouseButtonEventArgs e)
    {
      if (!this.Editable())
        return;
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (this._useInSearchFilter)
        return;
      this._dateEditDialog = new DateEditDialog(true, this._viewModel.DueDates, LogicType.Or, this._viewModel.Version);
      this._dateEditDialog.OnSelectedDateChanged += (EventHandler<List<string>>) ((s, duedates) =>
      {
        this._viewModel.DueDates = duedates;
        EventHandler ruleChanged = this.RuleChanged;
        if (ruleChanged == null)
          return;
        ruleChanged((object) this, (EventArgs) null);
      });
      this.datePopup.Closed -= new EventHandler(this.DatePopup_Closed);
      this.datePopup.Closed += new EventHandler(this.DatePopup_Closed);
      this.OnConditionClick((ConditionEditDialog) this._dateEditDialog, (Popup) this.datePopup);
    }

    private void DatePopup_Closed(object sender, EventArgs e)
    {
      if (!this._dateEditDialog.IsSave)
        this._dateEditDialog.Restore();
      this._dateEditDialog.IsSave = false;
    }

    private void OnAssigneeClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      this._assignEditDialog = new AssigneeEditDialog(this._viewModel.Assignees, true);
      this._assignEditDialog.OnSelectedAssigneeChanged += (EventHandler<List<string>>) ((s, assignees) => this._viewModel.Assignees = assignees);
      this.AssignPopup.Closed -= new EventHandler(this.AssigneePopup_Closed);
      this.AssignPopup.Closed += new EventHandler(this.AssigneePopup_Closed);
      this.OnConditionClick((ConditionEditDialog) this._assignEditDialog, (Popup) this.AssignPopup);
    }

    private void AssigneePopup_Closed(object sender, EventArgs e)
    {
      if (!this._assignEditDialog.IsSave)
        this._assignEditDialog.Restore();
      this._assignEditDialog.IsSave = false;
    }

    private void OnTagClick(object sender, MouseButtonEventArgs e)
    {
      if (!this.Editable())
        return;
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      this._tagEditDialog = new TagEditDialog(true, this._viewModel.Tags);
      this._tagEditDialog.OnSelectedTagChanged += (EventHandler<List<string>>) ((s, tags) =>
      {
        this._viewModel.Tags = tags;
        EventHandler ruleChanged = this.RuleChanged;
        if (ruleChanged == null)
          return;
        ruleChanged((object) this, (EventArgs) null);
      });
      this.tagPopup.Closed -= new EventHandler(this.TagPopup_Closed);
      this.tagPopup.Closed += new EventHandler(this.TagPopup_Closed);
      this.OnConditionClick((ConditionEditDialog) this._tagEditDialog, (Popup) this.tagPopup);
      this.datePopup.IsOpen = false;
      this.listPopup.IsOpen = false;
    }

    private bool Editable()
    {
      if (!this.CheckValid || UserDao.IsUserValid())
        return true;
      EventHandler notifyInvalid = this.NotifyInvalid;
      if (notifyInvalid != null)
        notifyInvalid((object) this, (EventArgs) null);
      return false;
    }

    private void TagPopup_Closed(object sender, EventArgs e)
    {
      if (!this._tagEditDialog.IsSave)
        this._tagEditDialog.Restore();
      this._tagEditDialog.IsSave = false;
    }

    private void OnListClick(object sender, MouseButtonEventArgs e)
    {
      if (!this.Editable())
        return;
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) this.listPopup, new ProjectExtra()
      {
        ProjectIds = this._viewModel.Projects,
        GroupIds = this._viewModel.Groups
      }, new ProjectSelectorExtra()
      {
        popupWidth = 348.0,
        showCalendarCategory = true
      });
      projectOrGroupPopup.Save += new EventHandler<ProjectExtra>(this.OnProjectSelect);
      projectOrGroupPopup.Show();
      this.tagPopup.IsOpen = false;
      this.datePopup.IsOpen = false;
    }

    private async void OnProjectSelect(object sender, ProjectExtra data)
    {
      NormalFilterControl sender1 = this;
      sender1._viewModel.Projects = data.ProjectIds;
      sender1._viewModel.Groups = data.GroupIds;
      EventHandler ruleChanged = sender1.RuleChanged;
      if (ruleChanged != null)
        ruleChanged((object) sender1, (EventArgs) null);
      sender1._viewModel.DisplayProjectText = ProjectExtra.FormatDisplayText(ProjectExtra.Serialize(data));
    }

    private void OnConditionClick(ConditionEditDialog dialog, Popup popup)
    {
      if (dialog == null)
        return;
      dialog.OnCancel += (EventHandler) ((s, arg) => popup.IsOpen = false);
      dialog.OnSave += (EventHandler<FilterConditionViewModel>) ((s, model) => popup.IsOpen = false);
      popup.Child = (UIElement) dialog;
      popup.IsOpen = true;
    }

    private void PriorityAllClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!this.Editable() || this._viewModel.Priorities.Count <= 0)
        return;
      this._viewModel.Priorities.Clear();
      this._viewModel.NotifyPriorityChanged();
      EventHandler ruleChanged = this.RuleChanged;
      if (ruleChanged == null)
        return;
      ruleChanged((object) this, (EventArgs) null);
    }

    private void TaskTypeAllClick(object sender, RoutedEventArgs e)
    {
      if (this._viewModel.TaskTypes.Count <= 0)
        return;
      this._viewModel.TaskTypes.Clear();
      this._viewModel.NotifyTaskTypeChanged();
      EventHandler ruleChanged = this.RuleChanged;
      if (ruleChanged == null)
        return;
      ruleChanged((object) this, (EventArgs) null);
    }

    private void AssigneeAllClick(object sender, RoutedEventArgs e)
    {
      if (this._viewModel.Assignees.Count <= 0)
        return;
      this._viewModel.Assignees.Clear();
      this._viewModel.NotifyAssigneeChanged();
    }

    private void TaskTypeCheckboxClick(object sender, RoutedEventArgs e)
    {
      if (sender is CheckBox checkBox)
      {
        string str = checkBox.Tag.ToString();
        if (this._viewModel.TaskTypes.Contains(str))
          this._viewModel.TaskTypes.Remove(str);
        else
          this._viewModel.TaskTypes.Add(str);
        if (this._viewModel.TaskTypes.Count == 2)
          this._viewModel.TaskTypes.Clear();
      }
      EventHandler ruleChanged = this.RuleChanged;
      if (ruleChanged != null)
        ruleChanged((object) this, (EventArgs) null);
      this._viewModel.NotifyTaskTypeChanged();
    }

    private void PriorityCheckboxClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!this.Editable())
        return;
      if (sender is CheckBox checkBox)
      {
        int num = int.Parse(checkBox.Tag.ToString());
        if (this._viewModel.Priorities.Contains(num))
          this._viewModel.Priorities.Remove(num);
        else
          this._viewModel.Priorities.Add(num);
        if (this._viewModel.Priorities.Count == 4)
          this._viewModel.Priorities.Clear();
      }
      EventHandler ruleChanged = this.RuleChanged;
      if (ruleChanged != null)
        ruleChanged((object) this, (EventArgs) null);
      this._viewModel.NotifyPriorityChanged();
    }

    private void AssigneeCheckboxClick(object sender, RoutedEventArgs e)
    {
      if (sender is CheckBox checkBox)
      {
        string str = checkBox.Tag.ToString();
        if (this._viewModel.Assignees.Contains(str))
          this._viewModel.Assignees.Remove(str);
        else
          this._viewModel.Assignees.Add(str);
      }
      this._viewModel.NotifyAssigneeChanged();
    }

    private void OnKeywordsChagned(object sender, TextChangedEventArgs e)
    {
      this._viewModel.Keywords.Clear();
      string str = this.KeywordsText.Text.Trim();
      if (string.IsNullOrEmpty(str))
        return;
      this._viewModel.Keywords.Add(str);
    }

    public void SetUseInSearchFilter()
    {
      this._useInSearchFilter = true;
      this.KeywordsGrid.Visibility = Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/normalfiltercontrol.xaml", UriKind.Relative));
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
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnListClick);
          break;
        case 2:
          this.listTextBox = (EmjTextBlock) target;
          break;
        case 3:
          this.listPopup = (EscPopup) target;
          break;
        case 4:
          this.tagGrid = (Grid) target;
          break;
        case 5:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnTagClick);
          break;
        case 6:
          this.tagTextBox = (EmjTextBlock) target;
          break;
        case 7:
          this.tagPopup = (EscPopup) target;
          break;
        case 8:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnDateClick);
          break;
        case 9:
          this.dateTextBox = (TextBlock) target;
          break;
        case 10:
          this.datePopup = (EscPopup) target;
          break;
        case 11:
          this.rbPriority = (RadioButton) target;
          this.rbPriority.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityAllClick);
          break;
        case 12:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityCheckboxClick);
          break;
        case 13:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityCheckboxClick);
          break;
        case 14:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityCheckboxClick);
          break;
        case 15:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.PriorityCheckboxClick);
          break;
        case 16:
          this.assigneeGrid = (Grid) target;
          break;
        case 17:
          this.AssignVersion3Grid = (Grid) target;
          break;
        case 18:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnAssigneeClick);
          break;
        case 19:
          this.AssignTextBox = (EmjTextBlock) target;
          break;
        case 20:
          this.AssignPopup = (EscPopup) target;
          break;
        case 21:
          this.KeywordsGrid = (Grid) target;
          break;
        case 22:
          this.KeywordsText = (TextBox) target;
          this.KeywordsText.TextChanged += new TextChangedEventHandler(this.OnKeywordsChagned);
          break;
        case 23:
          this.TaskTypeGrid = (Grid) target;
          break;
        case 24:
          this.AllType = (RadioButton) target;
          this.AllType.Click += new RoutedEventHandler(this.TaskTypeAllClick);
          break;
        case 25:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.TaskTypeCheckboxClick);
          break;
        case 26:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.TaskTypeCheckboxClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
