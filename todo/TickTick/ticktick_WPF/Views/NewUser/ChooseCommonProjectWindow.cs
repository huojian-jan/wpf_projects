// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.NewUser.ChooseCommonProjectWindow
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.NewUser
{
  public class ChooseCommonProjectWindow : Window, IComponentConnector, IStyleConnector
  {
    private readonly List<string> _selectedProjects = new List<string>();
    internal ItemsControl ChooseListItems;
    internal Button SaveButton;
    private bool _contentLoaded;

    public ChooseCommonProjectWindow()
    {
      this.InitializeComponent();
      this.InitItems();
    }

    private void InitItems()
    {
      List<string> list = NewUserGuide.DefaultProjects.Keys.ToList<string>();
      ObservableCollection<GuideChooseViewModel> observableCollection = new ObservableCollection<GuideChooseViewModel>();
      foreach (string str in list)
        observableCollection.Add(new GuideChooseViewModel(str, NewUserGuide.DefaultProjects[str]));
      this.ChooseListItems.ItemsSource = (IEnumerable) observableCollection;
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is GuideChooseViewModel dataContext))
        return;
      if (this._selectedProjects.Contains(dataContext.Name))
      {
        this._selectedProjects.Remove(dataContext.Name);
        dataContext.Selected = false;
      }
      else
      {
        this._selectedProjects.Add(dataContext.Name);
        dataContext.Selected = true;
      }
      this.SaveButton.IsEnabled = this._selectedProjects.Count > 0;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      ChooseCommonProjectWindow commonProjectWindow = this;
      if (!UserDao.IsPro() && CacheManager.GetProjectsWithoutInbox().Count<ProjectModel>((Func<ProjectModel, bool>) (p => !p.delete_status)) + commonProjectWindow._selectedProjects.Count > 9)
      {
        ProChecker.CheckPro(ProType.MoreListsUnlimited);
      }
      else
      {
        await ProjectDao.InitProjects(commonProjectWindow._selectedProjects);
        commonProjectWindow.Close();
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/newuser/choosecommonprojectwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.ChooseListItems = (ItemsControl) target;
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 4:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
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
      if (connectionId != 2)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
    }
  }
}
