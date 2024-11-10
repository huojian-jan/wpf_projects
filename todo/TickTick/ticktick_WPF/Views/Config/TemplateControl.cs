// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.TemplateControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media.Animation;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.QuickAdd;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class TemplateControl : UserControl, IComponentConnector, IStyleConnector
  {
    private TemplateKind _kind;
    public static readonly DependencyProperty UsedInSettingsProperty = DependencyProperty.Register(nameof (UsedInSettings), typeof (bool), typeof (TemplateControl), new PropertyMetadata((object) true, (PropertyChangedCallback) null));
    private List<TaskTemplateModel> _localTaskTemplate = new List<TaskTemplateModel>();
    private bool _pulled;
    private ObservableCollection<TemplateViewModel> _templateViewModels = new ObservableCollection<TemplateViewModel>();
    private AddTaskViewModel _addModel;
    private ListViewContainer _listView;
    private bool _inSettings;
    internal TemplateControl Root;
    internal Grid Container;
    internal Border SwitchBorder;
    internal TextBlock TitleText;
    internal EscPopup TitlePopup;
    internal ScrollViewer Scroller;
    internal ItemsControl MyTemplates;
    internal ItemsControl MyTemplatesInSetting;
    internal Grid EmptyGrid;
    internal TextBlock EmptyText;
    internal Grid ToastGrid;
    internal TextBlock ToastTextBlock;
    private bool _contentLoaded;

    public TemplateControl()
    {
      this.InitializeComponent();
      this.Loaded += (RoutedEventHandler) ((sender, args) => DataChangedNotifier.TemplateChanged += new EventHandler(this.OnTemplateChanged));
      this.Unloaded += (RoutedEventHandler) ((sender, args) => DataChangedNotifier.TemplateChanged -= new EventHandler(this.OnTemplateChanged));
    }

    private async void OnTemplateChanged(object sender, EventArgs e)
    {
      this._localTaskTemplate = await TemplateDao.GetLocalTemplate();
      this.ShowLocalTemplate(this._kind);
    }

    public TemplateControl(TemplateKind kind, AddTaskViewModel addModel = null)
    {
      this.InitializeComponent();
      this.Init(kind, addModel);
    }

    public async void Init(
      TemplateKind kind,
      AddTaskViewModel addModel = null,
      ListViewContainer listView = null)
    {
      this._kind = kind;
      this._localTaskTemplate = await TemplateDao.GetLocalTemplate();
      this.ShowLocalTemplate(kind);
      this._addModel = addModel;
      this._listView = listView;
      await Task.Delay(10);
      this.TryPullRemoteTemplate(kind);
    }

    public bool UsedInSettings
    {
      get => (bool) this.GetValue(TemplateControl.UsedInSettingsProperty);
      set => this.SetValue(TemplateControl.UsedInSettingsProperty, (object) value);
    }

    public event EventHandler<TemplateViewModel> TemplateSelected;

    private void ShowLocalTemplate(TemplateKind kind)
    {
      this.TitleText.Text = Utils.GetString(kind == TemplateKind.Note ? "NoteTemplate" : "TaskTemplate");
      List<TemplateViewModel> list = TemplateViewModel.Build(this._localTaskTemplate.Where<TaskTemplateModel>((Func<TaskTemplateModel, bool>) (t =>
      {
        if (t.Kind == kind.ToString())
          return true;
        return kind == TemplateKind.Task && string.IsNullOrEmpty(t.Kind);
      })).ToList<TaskTemplateModel>());
      list.Sort((Comparison<TemplateViewModel>) ((a, b) => b.CreatedTime.CompareTo(a.CreatedTime)));
      this._templateViewModels = new ObservableCollection<TemplateViewModel>(list);
      ItemsControl itemsControl = this._inSettings ? this.MyTemplatesInSetting : this.MyTemplates;
      itemsControl.ItemsSource = (IEnumerable) this._templateViewModels;
      ObservableCollection<TemplateViewModel> templateViewModels = this._templateViewModels;
      // ISSUE: explicit non-virtual call
      if ((templateViewModels != null ? (__nonvirtual (templateViewModels.Count) <= 0 ? 1 : 0) : 0) != 0)
      {
        itemsControl.Visibility = Visibility.Collapsed;
        this.EmptyGrid.Visibility = Visibility.Visible;
        this.EmptyText.Text = Utils.GetString(kind == TemplateKind.Note ? "TemplateEmptyNoteText" : "TemplateEmptyTaskText");
      }
      else
      {
        itemsControl.Visibility = Visibility.Visible;
        this.EmptyGrid.Visibility = Visibility.Collapsed;
      }
    }

    private async void TryPullRemoteTemplate(TemplateKind kind)
    {
      TemplatesModel templates;
      if (!Utils.IsNetworkAvailable())
      {
        this.Toast(Utils.GetString("TemplateNoNetWork"));
        templates = (TemplatesModel) null;
      }
      else
      {
        templates = await Communicator.PullTemplates();
        if (templates == null)
        {
          templates = (TemplatesModel) null;
        }
        else
        {
          List<TaskTemplateModel> defaultTemplate = new List<TaskTemplateModel>();
          List<TaskTemplateModel> defaultTemplates;
          if (templates.noteTemplates == null && this._localTaskTemplate.Count<TaskTemplateModel>((Func<TaskTemplateModel, bool>) (t => t.Kind == TemplateKind.Note.ToString())) == 0)
          {
            defaultTemplates = TaskTemplateModel.GetDefaultNoteTemp();
            await TemplateDao.AddTemplates(defaultTemplates, true);
            defaultTemplate.AddRange((IEnumerable<TaskTemplateModel>) defaultTemplates);
            defaultTemplates = (List<TaskTemplateModel>) null;
          }
          if (templates.taskTemplates == null && this._localTaskTemplate.Count<TaskTemplateModel>((Func<TaskTemplateModel, bool>) (t => t.Kind == TemplateKind.Task.ToString() || string.IsNullOrEmpty(t.Kind))) == 0)
          {
            defaultTemplates = TaskTemplateModel.GetDefaultTaskTemp();
            await TemplateDao.AddTemplates(defaultTemplates);
            defaultTemplate.AddRange((IEnumerable<TaskTemplateModel>) defaultTemplates);
            defaultTemplates = (List<TaskTemplateModel>) null;
          }
          if (await TemplateDao.MergeTemplates(templates.taskTemplates, templates.noteTemplates, this._localTaskTemplate))
          {
            this._localTaskTemplate = await TemplateDao.GetLocalTemplate();
            this.ShowLocalTemplate(kind);
          }
          else if (defaultTemplate.Count > 0)
          {
            this._localTaskTemplate.AddRange((IEnumerable<TaskTemplateModel>) defaultTemplate);
            this.ShowLocalTemplate(kind);
          }
          defaultTemplate = (List<TaskTemplateModel>) null;
          templates = (TemplatesModel) null;
        }
      }
    }

    private void Toast(string toastText)
    {
      this.ToastTextBlock.Text = toastText;
      this.ToastGrid.Visibility = Visibility.Visible;
      ((Storyboard) this.FindResource((object) "ToastShowAndHide")).Begin();
    }

    private void HideToastGrid(object sender, EventArgs e)
    {
      this.ToastGrid.Visibility = Visibility.Collapsed;
    }

    private void TemplateOptionsClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (!(((FrameworkElement) sender).DataContext is TemplateViewModel dataContext))
        return;
      dataContext.OpenOption = true;
    }

    private void OnApplyTemplateClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (!(sender is Button button) || !(button.DataContext is TemplateViewModel dataContext))
        return;
      this.AddTask(dataContext);
      dataContext.OpenOption = false;
    }

    private async void AddTask(TemplateViewModel model)
    {
      TemplateControl child = this;
      string projectId;
      if (model == null)
      {
        projectId = (string) null;
      }
      else
      {
        // ISSUE: reference to a compiler-generated method
        projectId = CacheManager.GetProjects().FirstOrDefault<ProjectModel>(new Func<ProjectModel, bool>(child.\u003CAddTask\u003Eb__24_0))?.id ?? Utils.GetInboxId();
        if (await ProChecker.CheckTaskLimit(projectId))
        {
          projectId = (string) null;
        }
        else
        {
          TaskModel taskModel = await TaskService.AddTaskFromTemplate(model.Id, child._addModel);
          if (taskModel == null)
            projectId = (string) null;
          else if (child._addModel != null)
          {
            child._listView?.ReloadTaskListAndSelect(true, taskModel.id, false);
            await Task.Delay(100);
            SettingDialog parent = Utils.FindParent<SettingDialog>((DependencyObject) child);
            if (parent == null)
            {
              projectId = (string) null;
            }
            else
            {
              parent.Close();
              projectId = (string) null;
            }
          }
          else if (!(child._listView?.GetSelectedProject() is NormalProjectIdentity selectedProject))
            projectId = (string) null;
          else if (!(selectedProject.Id == projectId))
          {
            projectId = (string) null;
          }
          else
          {
            ListViewContainer listView = child._listView;
            if (listView == null)
            {
              projectId = (string) null;
            }
            else
            {
              listView.ReloadTaskListAndSelect(true, taskModel.id, false);
              projectId = (string) null;
            }
          }
        }
      }
    }

    private void OnRenameTemplateClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (!(sender is Button button))
        return;
      TemplateViewModel model = button.DataContext as TemplateViewModel;
      if (model == null)
        return;
      string title1 = model.Title;
      List<TaskTemplateModel> localTaskTemplate = this._localTaskTemplate;
      List<string> list = localTaskTemplate != null ? localTaskTemplate.Select<TaskTemplateModel, string>((Func<TaskTemplateModel, string>) (m => m.Title)).ToList<string>() : (List<string>) null;
      EditTemplateWindow editTemplateWindow = new EditTemplateWindow(title1, list);
      editTemplateWindow.Owner = (Window) Utils.FindParent<SettingDialog>((DependencyObject) this);
      editTemplateWindow.Save += (EventHandler<string>) ((obj, title) =>
      {
        if (!(model.Title != title))
          return;
        model.Title = title;
        TaskTemplateModel model1 = this._localTaskTemplate.FirstOrDefault<TaskTemplateModel>((Func<TaskTemplateModel, bool>) (m => m.Id == model.Id));
        if (model1 == null)
          return;
        model1.Title = title;
        TemplateDao.UpdateTemplate(model1);
      });
      editTemplateWindow.ShowDialog();
    }

    private async void OnDeleteTemplateClick(object sender, RoutedEventArgs routedEventArgs)
    {
      if (sender is Button button)
      {
        TemplateViewModel model = button.DataContext as TemplateViewModel;
        if (model != null && new CustomerDialog(Utils.GetString("DeleteTemplate"), string.Format(Utils.GetString("SureDeleteTemplate"), (object) model.Title), MessageBoxButton.OKCancel).ShowDialog().GetValueOrDefault())
        {
          this._localTaskTemplate = await TemplateDao.GetLocalTemplate();
          TaskTemplateModel model1 = this._localTaskTemplate.FirstOrDefault<TaskTemplateModel>((Func<TaskTemplateModel, bool>) (m => m.Id == model.Id));
          this._templateViewModels.Remove(this._templateViewModels.FirstOrDefault<TemplateViewModel>((Func<TemplateViewModel, bool>) (m => m.Id == model.Id)));
          TemplateDao.DeleteTemplate(model1);
          this._localTaskTemplate.Remove(model1);
        }
      }
      ObservableCollection<TemplateViewModel> templateViewModels = this._templateViewModels;
      // ISSUE: explicit non-virtual call
      if ((templateViewModels != null ? (__nonvirtual (templateViewModels.Count) <= 0 ? 1 : 0) : 0) == 0)
        ;
      else
      {
        this.MyTemplates.Visibility = Visibility.Collapsed;
        this.MyTemplatesInSetting.Visibility = Visibility.Collapsed;
        this.EmptyGrid.Visibility = Visibility.Visible;
        this.EmptyText.Text = Utils.GetString(this._kind == TemplateKind.Note ? "TemplateEmptyNoteText" : "TemplateEmptyTaskText");
      }
    }

    private void OnTemplateClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Grid grid) || !(grid.DataContext is TemplateViewModel dataContext))
        return;
      if (this.UsedInSettings)
      {
        PreviewTemplateWindow previewTemplateWindow = new PreviewTemplateWindow(dataContext);
        previewTemplateWindow.Owner = (Window) Utils.FindParent<SettingDialog>((DependencyObject) this);
        previewTemplateWindow.AddTask += (EventHandler<TemplateViewModel>) ((obj, template) => this.AddTask(template));
        previewTemplateWindow.ShowDialog();
      }
      else
      {
        EventHandler<TemplateViewModel> templateSelected = this.TemplateSelected;
        if (templateSelected == null)
          return;
        templateSelected((object) this, dataContext);
      }
    }

    private async void ReloadData()
    {
      this._localTaskTemplate = await TemplateDao.GetLocalTemplate();
      this.ShowLocalTemplate(this._kind);
    }

    private void SwitchTemplateType(TemplateKind kind)
    {
      if (this._kind == kind)
        return;
      this.ShowLocalTemplate(kind);
      this._kind = kind;
    }

    private void TemplateGuideClick(object sender, MouseButtonEventArgs e)
    {
      new CustomerDialog(Utils.GetString("HowToCreateTemplate"), Utils.GetString("HowToCreateTemplateContent"), Utils.GetString("GotIt"), "", Window.GetWindow((DependencyObject) this)).ShowDialog();
    }

    private async void ManageTemplateClick(object sender, MouseButtonEventArgs e)
    {
      TemplateControl templateControl = this;
      Window window = Window.GetWindow((DependencyObject) templateControl);
      Window.GetWindow((DependencyObject) templateControl)?.Close();
      await Task.Delay(50);
      SettingDialog.ShowTemplateSettingDialog(window?.Owner, templateControl._kind, templateControl._addModel);
      window = (Window) null;
    }

    public void SetSettingMode()
    {
      this._inSettings = true;
      this.Scroller.Visibility = Visibility.Collapsed;
      this.MyTemplatesInSetting.Visibility = Visibility.Visible;
    }

    private void OnSwitchBorderClick(object sender, MouseButtonEventArgs e)
    {
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "Task", Utils.GetString("TaskTemplate"), (Geometry) null);
      menuItemViewModel1.Selected = this._kind == TemplateKind.Task;
      types.Add(menuItemViewModel1);
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "Note", Utils.GetString("NoteTemplate"), (Geometry) null);
      menuItemViewModel2.Selected = this._kind == TemplateKind.Note;
      types.Add(menuItemViewModel2);
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this.TitlePopup);
      customMenuList.Operated += new EventHandler<object>(this.OnActionSelected);
      customMenuList.Show();
    }

    private void OnActionSelected(object sender, object e)
    {
      if (!(e is string str))
        return;
      this.SwitchTemplateType(str != "Note" ? TemplateKind.Task : TemplateKind.Note);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/templatecontrol.xaml", UriKind.Relative));
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
          this.Root = (TemplateControl) target;
          break;
        case 7:
          ((Timeline) target).Completed += new EventHandler(this.HideToastGrid);
          break;
        case 8:
          this.Container = (Grid) target;
          break;
        case 9:
          this.SwitchBorder = (Border) target;
          this.SwitchBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSwitchBorderClick);
          break;
        case 10:
          this.TitleText = (TextBlock) target;
          break;
        case 11:
          this.TitlePopup = (EscPopup) target;
          break;
        case 12:
          this.Scroller = (ScrollViewer) target;
          break;
        case 13:
          this.MyTemplates = (ItemsControl) target;
          break;
        case 14:
          this.MyTemplatesInSetting = (ItemsControl) target;
          break;
        case 15:
          this.EmptyGrid = (Grid) target;
          break;
        case 16:
          this.EmptyText = (TextBlock) target;
          break;
        case 17:
          this.ToastGrid = (Grid) target;
          break;
        case 18:
          this.ToastTextBlock = (TextBlock) target;
          break;
        case 19:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.TemplateGuideClick);
          break;
        case 20:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ManageTemplateClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTemplateClick);
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.TemplateOptionsClick);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnApplyTemplateClick);
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnRenameTemplateClick);
          break;
        case 6:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnDeleteTemplateClick);
          break;
      }
    }
  }
}
