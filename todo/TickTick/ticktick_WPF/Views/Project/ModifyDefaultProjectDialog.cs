// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.ModifyDefaultProjectDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class ModifyDefaultProjectDialog : Window, IComponentConnector
  {
    private string _currentId;
    private TaskDefaultModel _taskDefault;
    internal EmjTextBlock DefaultAddProjectNameText;
    internal EscPopup DefaultAddProjectPopup;
    internal Button OkButton;
    private bool _contentLoaded;

    public ModifyDefaultProjectDialog(bool isDelete = false, bool isArchive = false)
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      if (isDelete)
        this.Title = Utils.GetString("CannotDeleteDefault");
      else if (isArchive)
        this.Title = Utils.GetString("CannotArchiveDefault");
      else
        this.Title = Utils.GetString("CannotHideDefault");
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      this._taskDefault = TaskDefaultDao.GetDefaultSafely();
      this._currentId = this._taskDefault.ProjectId;
      ProjectModel projectById = CacheManager.GetProjectById(this._currentId);
      if (this._currentId != null)
        this.DefaultAddProjectNameText.Text = projectById.name;
      this.OkButton.IsEnabled = false;
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      ModifyDefaultProjectDialog defaultProjectDialog = this;
      if (defaultProjectDialog._taskDefault != null && defaultProjectDialog._taskDefault.ProjectId != defaultProjectDialog._currentId)
      {
        defaultProjectDialog._taskDefault.ProjectId = defaultProjectDialog._currentId;
        await TaskDefaultDao.SaveTaskDefault(defaultProjectDialog._taskDefault);
        SettingsHelper.PushLocalSettings();
        DataChangedNotifier.NotifyTaskDefaultChanged();
      }
      defaultProjectDialog.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private async void OnDefaultAddProjectClick(object sender, MouseButtonEventArgs e)
    {
      ModifyDefaultProjectDialog defaultProjectDialog = this;
      ProjectOrGroupPopup projectOrGroupPopup = new ProjectOrGroupPopup((Popup) defaultProjectDialog.DefaultAddProjectPopup, new ProjectExtra()
      {
        ProjectIds = new List<string>()
        {
          defaultProjectDialog._currentId
        }
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
      projectOrGroupPopup.ItemSelect += new EventHandler<SelectableItemViewModel>(defaultProjectDialog.OnDefaultProjectSelect);
      projectOrGroupPopup.Show();
    }

    private void OnDefaultProjectSelect(object sender, SelectableItemViewModel e)
    {
      if (e is ProjectGroupViewModel)
        return;
      this.DefaultAddProjectPopup.IsOpen = false;
      ProjectModel projectById = CacheManager.GetProjectById(e.Id);
      if (projectById != null)
      {
        if (projectById.IsShareList() && !new CustomerDialog(Utils.GetString("SetShareProjectAsDefault"), Utils.GetString("SetShareProjectAsDefaultDesc"), Utils.GetString("OK"), Utils.GetString("Cancel"), Window.GetWindow((DependencyObject) this)).ShowDialog().GetValueOrDefault())
          return;
        this.DefaultAddProjectNameText.Text = projectById.name;
        this._currentId = projectById.id;
      }
      this.OkButton.IsEnabled = this._currentId != this._taskDefault?.ProjectId;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/project/modifydefaultprojectdialog.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDefaultAddProjectClick);
          break;
        case 2:
          this.DefaultAddProjectNameText = (EmjTextBlock) target;
          break;
        case 3:
          this.DefaultAddProjectPopup = (EscPopup) target;
          break;
        case 4:
          this.OkButton = (Button) target;
          this.OkButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
