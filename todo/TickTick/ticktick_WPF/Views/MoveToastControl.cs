// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MoveToastControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class MoveToastControl : UserControl, IComponentConnector
  {
    private readonly string _projectId;
    private readonly INavigateProject _navigate;
    private readonly bool _isToday;
    private readonly bool _isTomorrow;
    internal EmjTextBlock TitleText;
    internal EmjTextBlock ProjectTitle;
    private bool _contentLoaded;

    public MoveToastControl(
      string projectId,
      INavigateProject navigate,
      string taskName,
      MoveToastType moveType)
    {
      this.InitializeComponent();
      ProjectModel projectById = CacheManager.GetProjectById(projectId);
      this._projectId = projectById?.id;
      this._navigate = navigate;
      this.TitleText.Text = taskName;
      this.TitleText.Visibility = string.IsNullOrEmpty(taskName) ? Visibility.Collapsed : Visibility.Visible;
      switch (moveType)
      {
        case MoveToastType.Add:
          this.ProjectTitle.Text = Utils.GetString("AddTo") + " " + (projectById?.name ?? Utils.GetString("Inbox"));
          break;
        case MoveToastType.Restore:
          this.ProjectTitle.Text = string.Format(Utils.GetString("TaskHasBeenRestoreToProject"), (object) (projectById?.name ?? Utils.GetString("Inbox")));
          break;
        default:
          this.ProjectTitle.Text = string.Format(Utils.GetString("MovedTo"), (object) (Utils.GetString("List") + " " + projectById?.name));
          break;
      }
    }

    public MoveToastControl(bool isToday, INavigateProject navigate, string taskName)
    {
      this.InitializeComponent();
      this._navigate = navigate;
      this.TitleText.Text = taskName;
      this.TitleText.Visibility = string.IsNullOrEmpty(taskName) ? Visibility.Collapsed : Visibility.Visible;
      this._isToday = isToday;
      this._isTomorrow = !isToday;
      this.ProjectTitle.Text = string.Format(Utils.GetString("MovedTo"), (object) Utils.GetString(isToday ? "Today" : "Tomorrow"));
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!string.IsNullOrEmpty(this._projectId))
        this._navigate.NavigateProjectById(this._projectId);
      else if (this._isToday)
        this._navigate.NavigateTodayProject();
      else if (this._isTomorrow)
        this._navigate.NavigateTomorrowProject();
      this.Visibility = Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/movetoastcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
          break;
        case 2:
          this.TitleText = (EmjTextBlock) target;
          break;
        case 3:
          this.ProjectTitle = (EmjTextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
