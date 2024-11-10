// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.SmartListConfig
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Project;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class SmartListConfig : UserControl, IComponentConnector
  {
    internal ItemsControl SmartListView;
    internal ItemsControl ExtraView;
    internal ItemsControl BottomView;
    private bool _contentLoaded;

    public SmartListConfig()
    {
      this.InitializeComponent();
      this.InitData();
    }

    private void InitData()
    {
      ObservableCollection<SmartListViewModel> observableCollection1 = new ObservableCollection<SmartListViewModel>();
      observableCollection1.Add(new SmartListViewModel(SmartListType.All, false));
      observableCollection1.Add(new SmartListViewModel(SmartListType.Today));
      observableCollection1.Add(new SmartListViewModel(SmartListType.Tomorrow));
      observableCollection1.Add(new SmartListViewModel(SmartListType.Week));
      observableCollection1.Add(new SmartListViewModel(SmartListType.Assign));
      observableCollection1.Add(new SmartListViewModel(SmartListType.Inbox));
      observableCollection1.Add(new SmartListViewModel(SmartListType.Summary, false));
      this.SmartListView.ItemsSource = (IEnumerable) observableCollection1;
      ObservableCollection<SmartListViewModel> observableCollection2 = new ObservableCollection<SmartListViewModel>();
      observableCollection2.Add(new SmartListViewModel(SmartListType.Tag));
      observableCollection2.Add(new SmartListViewModel(SmartListType.Filter, false));
      this.ExtraView.ItemsSource = (IEnumerable) observableCollection2;
      ObservableCollection<SmartListViewModel> observableCollection3 = new ObservableCollection<SmartListViewModel>();
      observableCollection3.Add(new SmartListViewModel(SmartListType.Completed, false));
      observableCollection3.Add(new SmartListViewModel(SmartListType.Abandoned));
      observableCollection3.Add(new SmartListViewModel(SmartListType.Trash, false));
      this.BottomView.ItemsSource = (IEnumerable) observableCollection3;
    }

    private void OnItemSelectionChanged(object sender, SimpleComboBoxViewModel e)
    {
      if (!(sender is CustomSimpleComboBox customSimpleComboBox))
        return;
      bool flag = customSimpleComboBox.ItemsSource.Count == 3;
      ShowStatus showStatus = customSimpleComboBox.SelectedIndex == 0 ? ShowStatus.Show : (customSimpleComboBox.SelectedIndex == 1 ? (flag ? ShowStatus.Auto : ShowStatus.Hide) : ShowStatus.Hide);
      if (!(customSimpleComboBox.Tag is SmartListType tag))
        return;
      if (tag == SmartListType.Inbox && showStatus == ShowStatus.Hide && LocalSettings.Settings.SmartListInbox != 1 && TaskDefaultDao.GetDefaultSafely().ProjectId == LocalSettings.Settings.InServerBoxId)
      {
        ModifyDefaultProjectDialog defaultProjectDialog = new ModifyDefaultProjectDialog();
        defaultProjectDialog.Owner = Window.GetWindow((DependencyObject) this);
        defaultProjectDialog.ShowDialog();
        if (TaskDefaultDao.GetDefaultSafely().ProjectId == LocalSettings.Settings.InServerBoxId)
        {
          customSimpleComboBox.SelectedIndex = LocalSettings.Settings.SmartListInbox;
          return;
        }
        Utils.FindParent<MoreSettingsConfig>((DependencyObject) this)?.ResetTaskDefault();
      }
      switch (tag)
      {
        case SmartListType.All:
          LocalSettings.Settings.SmartListAll = (int) showStatus;
          break;
        case SmartListType.Today:
          LocalSettings.Settings.SmartListToday = (int) showStatus;
          break;
        case SmartListType.Tomorrow:
          LocalSettings.Settings.SmartListTomorrow = (int) showStatus;
          break;
        case SmartListType.Week:
          LocalSettings.Settings.SmartList7Day = (int) showStatus;
          break;
        case SmartListType.Assign:
          LocalSettings.Settings.SmartListForMe = (int) showStatus;
          break;
        case SmartListType.Completed:
          LocalSettings.Settings.SmartListComplete = (int) showStatus;
          break;
        case SmartListType.Trash:
          LocalSettings.Settings.SmartListTrash = (int) showStatus;
          break;
        case SmartListType.Summary:
          LocalSettings.Settings.SmartListSummary = (int) showStatus;
          break;
        case SmartListType.Inbox:
          LocalSettings.Settings.SmartListInbox = (int) showStatus;
          break;
        case SmartListType.Abandoned:
          LocalSettings.Settings.SmartListAbandoned = (int) showStatus;
          break;
        case SmartListType.Tag:
          LocalSettings.Settings.SmartListTag = (int) showStatus;
          ListViewContainer.ReloadProjectData();
          if (LocalSettings.Settings.SmartListTag != 1)
            return;
          ListViewContainer.OnProjectHide(PtfType.Tag);
          return;
        case SmartListType.Filter:
          LocalSettings.Settings.ShowCustomSmartList = (int) showStatus;
          ListViewContainer.ReloadProjectData();
          if (LocalSettings.Settings.ShowCustomSmartList != 1)
            return;
          ListViewContainer.OnProjectHide(PtfType.Filter);
          return;
      }
      if (showStatus == ShowStatus.Hide)
        ListViewContainer.OnProjectHide(tag);
      ListViewContainer.ReloadProjectData();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setting/smartlistconfig.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.SmartListView = (ItemsControl) target;
          break;
        case 2:
          this.ExtraView = (ItemsControl) target;
          break;
        case 3:
          this.BottomView = (ItemsControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
