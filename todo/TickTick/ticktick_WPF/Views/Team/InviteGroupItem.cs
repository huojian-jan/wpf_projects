// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Team.InviteGroupItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Team
{
  public class InviteGroupItem : UserControl, IComponentConnector
  {
    private InviteUsersDialog _parent;
    internal Polygon OpenIcon;
    internal StackPanel Container;
    internal Grid AllGrid;
    internal ItemsControl ItemsControl;
    private bool _contentLoaded;

    public InviteGroupItem() => this.InitializeComponent();

    private void InviteGroupItem_OnLoaded(object sender, RoutedEventArgs e)
    {
      this._parent = Utils.FindParent<InviteUsersDialog>((DependencyObject) this);
      this._parent.OnSwitch -= new EventHandler(this.Refresh);
      this._parent.OnSwitch += new EventHandler(this.Refresh);
    }

    private void OnProjectClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is InviteGroupModel dataContext))
        return;
      if (dataContext.Opened)
      {
        DoubleAnimation resource1 = (DoubleAnimation) this.FindResource((object) "HideUsers");
        DoubleAnimation resource2 = (DoubleAnimation) this.FindResource((object) "Close");
        resource1.From = new double?(this.ItemsControl.ActualHeight + (this.AllGrid.IsVisible ? 32.0 : 0.0));
        this.OpenIcon.BeginAnimation(RotateTransform.AngleProperty, (AnimationTimeline) resource2);
        this.Container.BeginAnimation(FrameworkElement.HeightProperty, (AnimationTimeline) resource1);
      }
      else
      {
        DoubleAnimation resource3 = (DoubleAnimation) this.FindResource((object) "ShowUsers");
        DoubleAnimation resource4 = (DoubleAnimation) this.FindResource((object) "Open");
        resource3.To = new double?(this.ItemsControl.ActualHeight + (this.AllGrid.IsVisible ? 32.0 : 0.0));
        this.OpenIcon.BeginAnimation(RotateTransform.AngleProperty, (AnimationTimeline) resource4);
        this.Container.BeginAnimation(FrameworkElement.HeightProperty, (AnimationTimeline) resource3);
      }
      dataContext.Opened = !dataContext.Opened;
    }

    private void Refresh(object sender, EventArgs e)
    {
      if (!(this.DataContext is InviteGroupModel dataContext))
        return;
      dataContext.SelectNum = 0;
      dataContext.Users.ForEach((Action<InviteUserModel>) (user => user.Selected = false));
    }

    private async void OnAllSelected(object sender, MouseButtonEventArgs e)
    {
      InviteGroupItem inviteGroupItem = this;
      if (!(inviteGroupItem.DataContext is InviteGroupModel dataContext))
        return;
      if (dataContext.SelectNum < dataContext.Users.Count)
      {
        dataContext.Users.ForEach((Action<InviteUserModel>) (model => model.Selected = true));
        inviteGroupItem._parent.OnProjectItemSelected(dataContext.Users.Select<InviteUserModel, string>((Func<InviteUserModel, string>) (user => user.UserCode)).ToList<string>(), true);
      }
      else
      {
        dataContext.Users.ForEach((Action<InviteUserModel>) (model => model.Selected = false));
        inviteGroupItem._parent.OnProjectItemSelected(dataContext.Users.Select<InviteUserModel, string>((Func<InviteUserModel, string>) (user => user.UserCode)).ToList<string>(), false);
      }
    }

    public void ItemSelected(string userCode, bool selected)
    {
      InviteUsersDialog parent = this._parent;
      if (parent == null)
        return;
      parent.OnProjectItemSelected(new List<string>()
      {
        userCode
      }, selected, false);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/team/invitegroupitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.InviteGroupItem_OnLoaded);
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnProjectClick);
          break;
        case 3:
          this.OpenIcon = (Polygon) target;
          break;
        case 4:
          this.Container = (StackPanel) target;
          break;
        case 5:
          this.AllGrid = (Grid) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAllSelected);
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAllSelected);
          break;
        case 8:
          this.ItemsControl = (ItemsControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
