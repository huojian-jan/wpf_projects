// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WidgetSettings
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.Views.CustomControl;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class WidgetSettings : Window, IComponentConnector
  {
    private static readonly Dictionary<string, int> DisplayDict = new Dictionary<string, int>()
    {
      {
        "embed",
        0
      },
      {
        "top",
        1
      }
    };
    private bool _isSingle;
    private Dictionary<string, string> _sortNameTypeDict;
    internal CustomSimpleComboBox ThemeComboBox;
    internal Slider OpacitySlider;
    internal CheckBox TopmostCheckBox;
    internal CheckBox ShowAddCheckBox;
    internal TextBlock TaskOptionText;
    internal Border TaskOptionBorder;
    internal Grid GroupOptionPanel;
    internal TextBlock GroupOptionText;
    internal CustomSimpleComboBox GroupTypeComboBox;
    internal Grid SortOptionPanel;
    internal TextBlock SortOptionText;
    internal CustomSimpleComboBox SortTypeComboBox;
    internal Grid HideCompletedPanel;
    internal TextBlock HideCompletedText;
    internal CheckBox HideCompletedCheckBox;
    internal Grid ToastGrid;
    private bool _contentLoaded;

    public WidgetSettings(WidgetViewModel model, bool isSingle = false)
    {
      this.InitializeComponent();
      this.DataContext = (object) model;
      this.InitDisplayOption();
      this._isSingle = isSingle;
      if (!isSingle)
        return;
      this.Title = Utils.GetString(model.IsMatrix ? "ConfigMatrixWidget" : "ConfigCalendarWidget");
      this.TaskOptionText.Visibility = Visibility.Collapsed;
    }

    private WidgetViewModel Model => (WidgetViewModel) this.DataContext;

    public string WidgetId => this.Model.Id;

    public event EventHandler HideCompleteChanged;

    public event EventHandler<string> DisplayOptionChanged;

    public event EventHandler<string> SortTypeChanged;

    public event EventHandler<string> GroupTypeChanged;

    public event EventHandler<float> OpacityChanged;

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext is WidgetViewModel dataContext) || dataContext.IsSingleMode)
        return;
      this.InitSortType();
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    public void Rebind(WidgetViewModel model)
    {
      if (model == null)
        return;
      this.DataContext = (object) model;
      UtilLog.Info("WidgetRebind " + model.GroupType + "  " + model.SortType);
      if (model.IsSingleMode)
        return;
      this.InitSortType();
      this.InitDisplayOption();
    }

    public async void InitSortType()
    {
      UtilLog.Info("InitSortType " + this.Model.GroupType + "  " + this.Model.SortType);
      this._sortNameTypeDict = new Dictionary<string, string>();
      Utils.TupleList<string, string, int> tupleList;
      switch (this.Model.Type)
      {
        case "normal":
          ProjectModel projectById = await ProjectDao.GetProjectById(this.Model.Identity);
          tupleList = projectById != null ? SortOptionHelper.GetSortOptionData(true, projectById.IsShareList(), kind: projectById.IsNote ? TaskType.Note : TaskType.Task) : SortOptionHelper.GetSortOptionData(false, false);
          break;
        case "tag":
          tupleList = SortOptionHelper.GetSortOptionData(false, false, true, TagDao.IsParentTag(this.Model.Identity));
          break;
        case "group":
          ObservableCollection<ProjectModel> projectsInGroup = await ProjectDao.GetProjectsInGroup(this.Model.Identity);
          TaskType kind = TaskType.Task;
          // ISSUE: explicit non-virtual call
          if (projectsInGroup != null && __nonvirtual (projectsInGroup.Count) > 0)
          {
            bool flag1 = false;
            bool flag2 = false;
            foreach (ProjectModel projectModel in (Collection<ProjectModel>) projectsInGroup)
            {
              if (projectModel.IsNote)
                flag2 = true;
              else
                flag1 = true;
            }
            kind = flag2 ? (flag1 ? TaskType.TaskAndNote : TaskType.Note) : TaskType.Task;
          }
          tupleList = SortOptionHelper.GetSortOptionDataInGroup(kind);
          break;
        default:
          tupleList = SortOptionHelper.GetSortOptionData(false, false);
          break;
      }
      if (tupleList == null)
        return;
      List<string> stringList1 = new List<string>();
      List<string> stringList2 = new List<string>();
      List<string> items1 = new List<string>();
      List<string> items2 = new List<string>();
      foreach (Tuple<string, string, int> tuple in (List<Tuple<string, string, int>>) tupleList)
      {
        if (tuple.Item1 != "project")
        {
          stringList1.Add(tuple.Item1);
          items1.Add(tuple.Item2);
        }
        if (tuple.Item1 != "title" && tuple.Item1 != "createdTime" && tuple.Item1 != "modifiedTime")
        {
          stringList2.Add(tuple.Item1);
          items2.Add(tuple.Item2);
        }
        this._sortNameTypeDict[tuple.Item2] = tuple.Item1;
      }
      stringList2.Add("none");
      items2.Add(Utils.GetString("none"));
      this._sortNameTypeDict[Utils.GetString("none")] = "none";
      this.SortTypeComboBox.Init(items1);
      this.GroupTypeComboBox.Init(items2);
      int num1 = stringList1.IndexOf(this.Model.SortType);
      if (num1 >= 0)
      {
        this.SortTypeComboBox.SelectedIndex = num1;
      }
      else
      {
        this.SortTypeComboBox.SelectedIndex = 0;
        this.Model.SortType = stringList1[0];
      }
      int num2 = stringList2.IndexOf(this.Model.GroupType);
      if (num2 >= 0)
      {
        this.GroupTypeComboBox.SelectedIndex = num2;
      }
      else
      {
        this.GroupTypeComboBox.SelectedIndex = 0;
        this.Model.GroupType = stringList2[0];
      }
    }

    private void OnGroupTypeChanged(object sender, SimpleComboBoxViewModel model)
    {
      string title = model.Title;
      string e;
      if (this._sortNameTypeDict == null || !this._sortNameTypeDict.TryGetValue(title, out e) || string.IsNullOrEmpty(e))
        return;
      this.Model.GroupType = e;
      if (this._isSingle)
        this.Model.SaveSingleModel();
      else
        this.Model.Save();
      EventHandler<string> groupTypeChanged = this.GroupTypeChanged;
      if (groupTypeChanged == null)
        return;
      groupTypeChanged((object) this, e);
    }

    private void OnSortTypeChanged(object sender, SimpleComboBoxViewModel model)
    {
      string title = model.Title;
      string e;
      if (this._sortNameTypeDict == null || !this._sortNameTypeDict.TryGetValue(title, out e) || string.IsNullOrEmpty(e))
        return;
      this.Model.SortType = e;
      if (this._isSingle)
        this.Model.SaveSingleModel();
      else
        this.Model.Save();
      EventHandler<string> sortTypeChanged = this.SortTypeChanged;
      if (sortTypeChanged == null)
        return;
      sortTypeChanged((object) this, e);
    }

    private void InitDisplayOption()
    {
      this.InitThemeComboBox();
      this.TopmostCheckBox.Checked -= new RoutedEventHandler(this.OnTopmostCheckedChanged);
      this.TopmostCheckBox.Unchecked -= new RoutedEventHandler(this.OnTopmostCheckedChanged);
      this.TopmostCheckBox.IsChecked = new bool?(this.Model.DisplayOption == "top");
      this.TopmostCheckBox.Checked += new RoutedEventHandler(this.OnTopmostCheckedChanged);
      this.TopmostCheckBox.Unchecked += new RoutedEventHandler(this.OnTopmostCheckedChanged);
      this.ShowAddCheckBox.IsChecked = new bool?(LocalSettings.Settings.ExtraSettings.PwShowAdd);
      if ((double) this.Model.Opacity < 0.0 || (double) this.Model.Opacity > 1.0)
        return;
      this.OpacitySlider.ValueChanged -= new RoutedPropertyChangedEventHandler<double>(this.OnSlideValueChanged);
      this.OpacitySlider.Value = (double) this.Model.Opacity * 10.0;
      this.OpacitySlider.ToolTip = (object) (((int) (this.OpacitySlider.Value * 10.0)).ToString() + "%");
      this.OpacitySlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(this.OnSlideValueChanged);
    }

    private async void OnTopmostCheckedChanged(object sender, RoutedEventArgs e)
    {
      WidgetSettings sender1 = this;
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      string option = sender1.TopmostCheckBox.IsChecked.GetValueOrDefault() ? "top" : "embed";
      if (string.IsNullOrEmpty(option))
      {
        option = (string) null;
      }
      else
      {
        sender1.Model.DisplayOption = option;
        if (sender1._isSingle)
          sender1.Model.SaveSingleModel();
        else
          sender1.Model.Save();
        await Task.Delay(100);
        EventHandler<string> displayOptionChanged = sender1.DisplayOptionChanged;
        if (displayOptionChanged != null)
          displayOptionChanged((object) sender1, option);
        sender1.Topmost = false;
        sender1.Topmost = true;
        option = (string) null;
      }
    }

    private void OnSlideValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
      this.Model.Opacity = (float) e.NewValue / 10f;
      this.OpacitySlider.ToolTip = (object) (((int) (e.NewValue * 10.0)).ToString() + "%");
      if (this._isSingle)
        this.Model.SaveSingleModel();
      else
        this.Model.Save();
      EventHandler<float> opacityChanged = this.OpacityChanged;
      if (opacityChanged == null)
        return;
      opacityChanged((object) this, this.Model.Opacity);
    }

    private void OnCompletedCheckedChanged(object sender, RoutedEventArgs e)
    {
      if (this._isSingle)
        this.Model.SaveSingleModel();
      else
        this.Model.Save();
      EventHandler hideCompleteChanged = this.HideCompleteChanged;
      if (hideCompleteChanged == null)
        return;
      hideCompleteChanged((object) this, (EventArgs) null);
    }

    private void OnThemeChecked(object sender, RoutedEventArgs e)
    {
      if (!(sender is RadioButton radioButton) || !(radioButton.Tag is string tag) || !(tag != this.Model.ThemeId))
        return;
      this.Model.ThemeId = this.Model.ThemeId == "light" ? "dark" : "light";
      if (this._isSingle)
        this.Model.SaveSingleModel();
      else
        this.Model.Save();
      EventHandler<string> displayOptionChanged = this.DisplayOptionChanged;
      if (displayOptionChanged == null)
        return;
      displayOptionChanged((object) this, this.Model.ThemeId);
    }

    private void InitThemeComboBox()
    {
      this.ThemeComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("DarkColor"),
        Utils.GetString("LightColor")
      };
      this.ThemeComboBox.SelectedIndex = !(this.Model.ThemeId == "dark") ? 1 : 0;
    }

    private void OnThemeChanged(object sender, SimpleComboBoxViewModel e)
    {
      this.Model.ThemeId = this.ThemeComboBox.SelectedIndex == 0 ? "dark" : "light";
      if (this._isSingle)
        this.Model.SaveSingleModel();
      else
        this.Model.Save();
      EventHandler<string> displayOptionChanged = this.DisplayOptionChanged;
      if (displayOptionChanged == null)
        return;
      displayOptionChanged((object) this, this.Model.ThemeId);
    }

    private void OnAddCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      LocalSettings.Settings.ExtraSettings.PwShowAdd = !LocalSettings.Settings.ExtraSettings.PwShowAdd;
      this.ShowAddCheckBox.IsChecked = new bool?(LocalSettings.Settings.ExtraSettings.PwShowAdd);
      ProjectWidgetsHelper.OnShowAddChanged();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/widget/widgetsettings.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnWindowLoaded);
          break;
        case 2:
          this.ThemeComboBox = (CustomSimpleComboBox) target;
          break;
        case 3:
          this.OpacitySlider = (Slider) target;
          break;
        case 4:
          this.TopmostCheckBox = (CheckBox) target;
          break;
        case 5:
          this.ShowAddCheckBox = (CheckBox) target;
          this.ShowAddCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnAddCheckBoxClick);
          break;
        case 6:
          this.TaskOptionText = (TextBlock) target;
          break;
        case 7:
          this.TaskOptionBorder = (Border) target;
          break;
        case 8:
          this.GroupOptionPanel = (Grid) target;
          break;
        case 9:
          this.GroupOptionText = (TextBlock) target;
          break;
        case 10:
          this.GroupTypeComboBox = (CustomSimpleComboBox) target;
          break;
        case 11:
          this.SortOptionPanel = (Grid) target;
          break;
        case 12:
          this.SortOptionText = (TextBlock) target;
          break;
        case 13:
          this.SortTypeComboBox = (CustomSimpleComboBox) target;
          break;
        case 14:
          this.HideCompletedPanel = (Grid) target;
          break;
        case 15:
          this.HideCompletedText = (TextBlock) target;
          break;
        case 16:
          this.HideCompletedCheckBox = (CheckBox) target;
          this.HideCompletedCheckBox.Checked += new RoutedEventHandler(this.OnCompletedCheckedChanged);
          this.HideCompletedCheckBox.Unchecked += new RoutedEventHandler(this.OnCompletedCheckedChanged);
          break;
        case 17:
          this.ToastGrid = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
