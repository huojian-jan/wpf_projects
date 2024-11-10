// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.NewTaskDefault
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
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class NewTaskDefault : UserControl, IComponentConnector
  {
    private List<int> _durationValues;
    private bool _changed;
    internal CustomSimpleComboBox DefaultDateCombobox;
    internal Border TimeReminderBorder;
    internal TextBlock SelectTimeText;
    internal Path TimeReminderIcon;
    internal EscPopup SelectReminderPopup;
    internal Border AlldayReminderBorder;
    internal TextBlock SelectAlldayText;
    internal Path AllDayReminderIcon;
    internal EscPopup SelectAllDayReminderPopup;
    internal CustomSimpleComboBox DefaultPriorityComboBox;
    internal EmjTextBlock DefaultAddTagNameText;
    internal EscPopup DefaultAddTagPopup;
    internal Path ProjectArrow;
    internal EmjTextBlock DefaultAddProjectNameText;
    internal EscPopup DefaultAddProjectPopup;
    internal Grid DateModePanel;
    internal CustomSimpleComboBox DateModeComboBox;
    internal CustomSimpleComboBox DurationUnitComboBox;
    internal CustomSimpleComboBox AddToComboBox;
    internal CustomSimpleComboBox PosOfOverdueComboBox;
    internal TextBlock ResetDefault;
    private bool _contentLoaded;

    public NewTaskDefault()
    {
      this.InitializeComponent();
      this.InitData();
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    public async void InitData()
    {
      NewTaskDefault newTaskDefault = this;
      TaskDefaultModel model = await TaskDefaultDao.GetTaskDefaultWithDefault();
      TaskDefaultViewModel vm = new TaskDefaultViewModel(model);
      string emptyText = Utils.GetString("none");
      if (!string.IsNullOrEmpty(model.TimeReminders))
      {
        List<string> list = ((IEnumerable<string>) model.TimeReminders.Split(',')).ToList<string>();
        newTaskDefault.SelectTimeText.Text = NewTaskDefault.GetDisplayText((IReadOnlyCollection<string>) list, false, emptyText);
        newTaskDefault.SelectTimeText.Tag = (object) model.TimeReminders;
      }
      else
      {
        newTaskDefault.SelectTimeText.Text = Utils.GetString("none");
        newTaskDefault.SelectTimeText.Tag = (object) string.Empty;
      }
      if (!string.IsNullOrEmpty(model.AllDayReminders))
      {
        List<string> list = ((IEnumerable<string>) model.AllDayReminders.Split(',')).ToList<string>();
        newTaskDefault.SelectAlldayText.Text = NewTaskDefault.GetDisplayText((IReadOnlyCollection<string>) list, true, emptyText);
        newTaskDefault.SelectAlldayText.Tag = (object) model.AllDayReminders;
      }
      else
      {
        newTaskDefault.SelectAlldayText.Text = Utils.GetString("none");
        newTaskDefault.SelectAlldayText.Tag = (object) string.Empty;
      }
      newTaskDefault.SetTagText(vm.Tags);
      if (LocalSettings.Settings.InServerBoxId.Equals(model.ProjectId))
      {
        SetDefaultProjectToInbox();
      }
      else
      {
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.ProjectId));
        if (projectModel != null)
        {
          newTaskDefault.DefaultAddProjectNameText.Text = projectModel.name;
          newTaskDefault.DefaultAddProjectNameText.Tag = (object) projectModel.id;
        }
        else
          SetDefaultProjectToInbox();
      }
      newTaskDefault.DurationUnitComboBox.Visibility = model.DateMode == 1 ? Visibility.Visible : Visibility.Collapsed;
      newTaskDefault.DateModePanel.Visibility = UserDao.IsPro() ? Visibility.Visible : Visibility.Collapsed;
      newTaskDefault.DataContext = (object) vm;
      newTaskDefault.InitDurationUnitComboBox(vm.Duration);
      newTaskDefault.InitDefaultDateComboBox(vm);

      void SetDefaultProjectToInbox()
      {
        this.DefaultAddProjectNameText.Text = Utils.GetString("Inbox");
        this.DefaultAddProjectNameText.Tag = (object) LocalSettings.Settings.InServerBoxId;
      }
    }

    private void InitDefaultDateComboBox(TaskDefaultViewModel vm)
    {
      this.DefaultDateCombobox.ItemsSource = new List<string>()
      {
        Utils.GetString("none"),
        Utils.GetString("Today"),
        Utils.GetString("Tomorrow"),
        Utils.GetString("DayAfterTomorrow"),
        Utils.GetString("NextWeek")
      };
      this.DefaultDateCombobox.SelectedIndex = vm.DateIndex;
      this.DateModeComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("date"),
        Utils.GetString("Duration")
      };
      this.DateModeComboBox.SelectedIndex = vm.DateMode;
      this.DefaultPriorityComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("PriorityHigh"),
        Utils.GetString("PriorityMedium"),
        Utils.GetString("PriorityLow"),
        Utils.GetString("PriorityNull")
      };
      this.DefaultPriorityComboBox.SelectedIndex = vm.PriorityIndex;
      this.AddToComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("TopOfList"),
        Utils.GetString("BottomOfList")
      };
      this.AddToComboBox.SelectedIndex = vm.AddTo;
      this.PosOfOverdueComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("Top"),
        Utils.GetString("Bottom")
      };
      this.PosOfOverdueComboBox.SelectedIndex = LocalSettings.Settings.PosOfOverdue;
    }

    private void InitDurationUnitComboBox(int current)
    {
      List<string> stringList = new List<string>()
      {
        10.ToString() + " " + Utils.GetString("PublicMinutes"),
        15.ToString() + " " + Utils.GetString("PublicMinutes"),
        30.ToString() + " " + Utils.GetString("PublicMinutes"),
        45.ToString() + " " + Utils.GetString("PublicMinutes"),
        Utils.GetString("OneHour"),
        Utils.GetString("OneHourAndHalf"),
        Utils.GetString("TwoHours"),
        Utils.GetString("TwoHoursAndHalf"),
        Utils.GetString("ThreeHours"),
        Utils.GetString("OneDay"),
        Utils.GetString("TwoDays"),
        Utils.GetString("ThreeDays")
      };
      this._durationValues = new List<int>()
      {
        10,
        15,
        30,
        45,
        60,
        90,
        120,
        150,
        180,
        1440,
        2880,
        4320
      };
      int num = this._durationValues.IndexOf(current);
      this.DurationUnitComboBox.SelectedIndex = num >= 0 ? num : 0;
      this.DurationUnitComboBox.ItemsSource = stringList;
    }

    private void OnDefaultPrioritySelect(object sender, SimpleComboBoxViewModel e)
    {
      if (!(this.DataContext is TaskDefaultViewModel dataContext))
        return;
      dataContext.PriorityIndex = this.DefaultPriorityComboBox.SelectedIndex;
    }

    private void OnDefaultDateSelect(object sender, SimpleComboBoxViewModel e)
    {
      if (!(this.DataContext is TaskDefaultViewModel dataContext))
        return;
      dataContext.DateIndex = this.DefaultDateCombobox.SelectedIndex;
    }

    private void OnDurationUnitChanged(object sender, SimpleComboBoxViewModel e)
    {
      if (!(this.DataContext is TaskDefaultViewModel dataContext) || this.DurationUnitComboBox.SelectedIndex >= this._durationValues.Count)
        return;
      dataContext.Duration = this._durationValues[this.DurationUnitComboBox.SelectedIndex];
    }

    private void OnAddToSelected(object sender, SimpleComboBoxViewModel e)
    {
      if (!(this.DataContext is TaskDefaultViewModel dataContext))
        return;
      dataContext.AddTo = this.AddToComboBox.SelectedIndex;
    }

    private void OnPosOfOverdueChanged(object sender, SimpleComboBoxViewModel e)
    {
      this._changed = true;
      LocalSettings.Settings.PosOfOverdue = this.PosOfOverdueComboBox.SelectedIndex;
      LocalSettings.Settings.Save();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e) => this.SaveDefault();

    public async Task SaveDefault()
    {
      NewTaskDefault newTaskDefault = this;
      if (newTaskDefault.DataContext == null || !(newTaskDefault.DataContext is TaskDefaultViewModel dataContext))
        return;
      TaskDefaultModel model = dataContext.ToModel();
      model.TimeReminders = newTaskDefault.SelectTimeText.Tag?.ToString() ?? string.Empty;
      model.AllDayReminders = newTaskDefault.SelectAlldayText.Tag?.ToString() ?? string.Empty;
      model.ProjectId = newTaskDefault.DefaultAddProjectNameText.Tag?.ToString() ?? LocalSettings.Settings.InServerBoxId;
      model.DateMode = newTaskDefault.DateModeComboBox.SelectedIndex;
      model.AddTo = newTaskDefault.AddToComboBox.SelectedIndex;
      if (!TaskDefaultDao.GetDefaultSafely().Equal(model))
      {
        await TaskDefaultDao.SaveTaskDefault(model);
        newTaskDefault._changed = true;
      }
      if (!newTaskDefault._changed)
        return;
      newTaskDefault._changed = false;
      DataChangedNotifier.NotifyTaskDefaultChanged();
      SettingsHelper.PushLocalSettings();
    }

    private void OnTimeReminderClick(object sender, MouseButtonEventArgs e)
    {
      SetReminderControl setReminderControl = new SetReminderControl(false, (IEnumerable<TaskReminderModel>) Utils.BuildReminders((IReadOnlyCollection<string>) this.SelectTimeText.Tag.ToString().Split(',')), new DateTime());
      setReminderControl.OnCancel += (EventHandler) ((s, arg) => this.SelectReminderPopup.IsOpen = false);
      setReminderControl.OnSelected += new EventHandler<List<string>>(this.OnTimeReminderSelected);
      Mouse.Capture((IInputElement) null);
      this.SelectReminderPopup.Child = (UIElement) setReminderControl;
      this.SelectReminderPopup.IsOpen = true;
    }

    private void OnTimeReminderSelected(object sender, List<string> reminders)
    {
      this.SelectReminderPopup.IsOpen = false;
      this.SelectTimeText.Tag = (object) string.Join(",", (IEnumerable<string>) reminders);
      this.SelectTimeText.Text = NewTaskDefault.GetDisplayText((IReadOnlyCollection<string>) reminders, false, Utils.GetString("none"));
    }

    private void OnAllDayReminderClick(object sender, MouseButtonEventArgs e)
    {
      SetReminderControl setReminderControl = new SetReminderControl(true, (IEnumerable<TaskReminderModel>) Utils.BuildReminders((IReadOnlyCollection<string>) this.SelectAlldayText.Tag.ToString().Split(',')), new DateTime());
      setReminderControl.OnCancel += (EventHandler) ((s, arg) => this.SelectAllDayReminderPopup.IsOpen = false);
      setReminderControl.OnSelected += new EventHandler<List<string>>(this.OnAllDayReminderSelected);
      Mouse.Capture((IInputElement) null);
      this.SelectAllDayReminderPopup.Child = (UIElement) setReminderControl;
      this.SelectAllDayReminderPopup.IsOpen = true;
    }

    private void OnAllDayReminderSelected(object sender, List<string> reminders)
    {
      this.SelectAllDayReminderPopup.IsOpen = false;
      this.SelectAlldayText.Tag = (object) string.Join(",", (IEnumerable<string>) reminders);
      this.SelectAlldayText.Text = NewTaskDefault.GetDisplayText((IReadOnlyCollection<string>) reminders, true, Utils.GetString("none"));
    }

    private static string GetDisplayText(
      IReadOnlyCollection<string> reminders,
      bool isAllDay,
      string emptyText = "")
    {
      return ReminderUtils.GetReminderListDisplayText((ICollection<TaskReminderModel>) Utils.BuildReminders(reminders), isAllDay, emptyText);
    }

    private void OnDateModeChanged(object sender, SimpleComboBoxViewModel e)
    {
      this.DurationUnitComboBox.Visibility = this.DateModeComboBox.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnDefaultAddProjectClick(object sender, MouseButtonEventArgs e)
    {
      string str = LocalSettings.Settings.InServerBoxId;
      if (this.DefaultAddProjectNameText.Tag is string tag)
        str = tag;
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) this.DefaultAddProjectPopup, new ProjectExtra()
      {
        ProjectIds = new List<string>() { str }
      }, new ProjectSelectorExtra()
      {
        showAll = false,
        batchMode = false,
        canSelectGroup = false,
        onlyShowPermission = true,
        showNoteProject = false,
        showSharedProject = true,
        CanSearch = true
      });
      projectOrGroupPopup.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnDefaultProjectSelect);
      projectOrGroupPopup.Show();
    }

    private void OnDefaultTagClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext == null)
        return;
      TaskDefaultViewModel vm = this.DataContext as TaskDefaultViewModel;
      if (vm == null)
        return;
      BatchSetTagControl batchSetTagControl1 = new BatchSetTagControl();
      BatchSetTagControl batchSetTagControl2 = batchSetTagControl1;
      TagSelectData tags1 = new TagSelectData();
      List<string> tags2 = vm.Tags;
      tags1.OmniSelectTags = (tags2 != null ? tags2.ToList<string>() : (List<string>) null) ?? new List<string>();
      batchSetTagControl2.Init(tags1, true);
      batchSetTagControl1.TagsSelect += (EventHandler<TagSelectData>) ((s, tags) =>
      {
        vm.Tags = tags.OmniSelectTags;
        this.DefaultAddTagPopup.IsOpen = false;
        this.SetTagText(vm.Tags);
      });
      batchSetTagControl1.Close += (EventHandler) ((s, args) => this.DefaultAddTagPopup.IsOpen = false);
      this.DefaultAddTagPopup.Child = (UIElement) batchSetTagControl1;
      this.DefaultAddTagPopup.IsOpen = true;
    }

    private void SetTagText(List<string> tags)
    {
      if (tags == null || tags.Count == 0)
      {
        this.DefaultAddTagNameText.Text = Utils.GetString("none");
      }
      else
      {
        tags = tags.Select<string, string>((Func<string, string>) (t => CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (ta => ta.name == t.ToLower()))?.label)).ToList<string>();
        tags.Remove((string) null);
        this.DefaultAddTagNameText.Text = tags.Join<string>(",");
      }
    }

    private void OnDefaultProjectSelect(object sender, SelectableItemViewModel e)
    {
      if (e is ProjectGroupViewModel)
        return;
      this.DefaultAddProjectPopup.IsOpen = false;
      string projectId = e.Id;
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
      if (projectModel == null || projectModel.IsShareList() && !new CustomerDialog(Utils.GetString("SetShareProjectAsDefault"), Utils.GetString("SetShareProjectAsDefaultDesc"), Utils.GetString("OK"), Utils.GetString("Cancel"), Window.GetWindow((DependencyObject) this)).ShowDialog().GetValueOrDefault())
        return;
      this.DefaultAddProjectNameText.Text = projectModel.name;
      this.DefaultAddProjectNameText.Tag = (object) projectModel.id;
      this.SaveDefault();
    }

    private async void ResetDefaultClick(object sender, MouseButtonEventArgs e)
    {
      await TaskDefaultDao.SaveTaskDefault(TaskDefaultModel.BuildDefault());
      DataChangedNotifier.NotifyTaskDefaultChanged();
      SettingsHelper.PushLocalSettings();
      this.InitData();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setting/newtaskdefault.xaml", UriKind.Relative));
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
          this.DefaultDateCombobox = (CustomSimpleComboBox) target;
          break;
        case 2:
          this.TimeReminderBorder = (Border) target;
          this.TimeReminderBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTimeReminderClick);
          break;
        case 3:
          this.SelectTimeText = (TextBlock) target;
          break;
        case 4:
          this.TimeReminderIcon = (Path) target;
          break;
        case 5:
          this.SelectReminderPopup = (EscPopup) target;
          break;
        case 6:
          this.AlldayReminderBorder = (Border) target;
          this.AlldayReminderBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAllDayReminderClick);
          break;
        case 7:
          this.SelectAlldayText = (TextBlock) target;
          break;
        case 8:
          this.AllDayReminderIcon = (Path) target;
          break;
        case 9:
          this.SelectAllDayReminderPopup = (EscPopup) target;
          break;
        case 10:
          this.DefaultPriorityComboBox = (CustomSimpleComboBox) target;
          break;
        case 11:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDefaultTagClick);
          break;
        case 12:
          this.DefaultAddTagNameText = (EmjTextBlock) target;
          break;
        case 13:
          this.DefaultAddTagPopup = (EscPopup) target;
          break;
        case 14:
          this.ProjectArrow = (Path) target;
          break;
        case 15:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDefaultAddProjectClick);
          break;
        case 16:
          this.DefaultAddProjectNameText = (EmjTextBlock) target;
          break;
        case 17:
          this.DefaultAddProjectPopup = (EscPopup) target;
          break;
        case 18:
          this.DateModePanel = (Grid) target;
          break;
        case 19:
          this.DateModeComboBox = (CustomSimpleComboBox) target;
          break;
        case 20:
          this.DurationUnitComboBox = (CustomSimpleComboBox) target;
          break;
        case 21:
          this.AddToComboBox = (CustomSimpleComboBox) target;
          break;
        case 22:
          this.PosOfOverdueComboBox = (CustomSimpleComboBox) target;
          break;
        case 23:
          this.ResetDefault = (TextBlock) target;
          this.ResetDefault.MouseLeftButtonUp += new MouseButtonEventHandler(this.ResetDefaultClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
