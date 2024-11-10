// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.ShareUserItemControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class ShareUserItemControl : UserControl, IComponentConnector
  {
    internal TextBlock DisplayName;
    internal TextBlock UserName;
    internal TextBlock OwnerOrMyPermission;
    internal ProjectPermissionSetControl OptionControl;
    internal CheckBox OpenCheckBox;
    private bool _contentLoaded;

    public ShareUserItemControl()
    {
      this.InitializeComponent();
      this.InitEvent();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext is ShareUserViewMode dataContext))
        return;
      if (dataContext.isTeam)
      {
        this.OptionControl.SetTeamItem();
        this.OpenCheckBox.IsChecked = new bool?(dataContext.openToTeam);
        this.OpenCheckBox.Visibility = Visibility.Visible;
        this.OpenCheckBox.Checked += new RoutedEventHandler(this.OnOpenCheckChanged);
        this.OpenCheckBox.Unchecked += new RoutedEventHandler(this.OnOpenCheckChanged);
        this.IsEnabled = dataContext.editable;
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) dataContext, new EventHandler<PropertyChangedEventArgs>(this.OnEnabledChanged), "editable");
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) dataContext, new EventHandler<PropertyChangedEventArgs>(this.OnPermitChanged), "permission");
      }
      else
      {
        if (string.IsNullOrEmpty(dataContext.displayName))
        {
          this.DisplayName.Text = dataContext.username;
          this.UserName.Visibility = Visibility.Collapsed;
        }
        if (!dataContext.isAccept)
          this.OptionControl.UnableChangePermission();
        if (dataContext.deleted)
          this.UserName.Visibility = Visibility.Collapsed;
        if (dataContext.isOwner)
        {
          if (dataContext.userId.ToString() == LocalSettings.Settings.LoginUserId)
          {
            if (dataContext.userCount <= 1 || dataContext.noMembers)
              this.OptionControl.Visibility = Visibility.Collapsed;
            else
              this.OptionControl.ShowTransfer();
          }
          else
            this.OptionControl.Visibility = Visibility.Collapsed;
          this.OwnerOrMyPermission.Visibility = Visibility.Visible;
        }
        else if (dataContext.userId.ToString() == LocalSettings.Settings.LoginUserId)
        {
          if (!dataContext.openToTeam || dataContext.visitor)
            this.OptionControl.ShowExit();
          else
            this.OptionControl.Visibility = Visibility.Collapsed;
          this.OwnerOrMyPermission.Visibility = Visibility.Visible;
          string str = Utils.GetString("Editable");
          switch (dataContext.permission)
          {
            case "read":
              str = Utils.GetString("ReadOnly");
              break;
            case "comment":
              str = Utils.GetString("CanComment");
              break;
          }
          this.OwnerOrMyPermission.Text = str;
          if (dataContext.editable)
            return;
          this.OptionControl.Width = 24.0;
          this.OptionControl.Height = 28.0;
          this.OptionControl.BorderThickness = new Thickness(0.0);
        }
        else if (!dataContext.editable)
        {
          this.OptionControl.Visibility = Visibility.Collapsed;
        }
        else
        {
          Constants.ProjectPermission result;
          Enum.TryParse<Constants.ProjectPermission>(dataContext.permission, true, out result);
          this.OptionControl.SwitchPermission((int) result);
        }
      }
    }

    private void OnEnabledChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(this.DataContext is ShareUserViewMode dataContext))
        return;
      this.IsEnabled = dataContext.editable;
    }

    private void OnPermitChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(this.DataContext is ShareUserViewMode dataContext) || !dataContext.isTeam)
        return;
      Constants.ProjectPermission result;
      Enum.TryParse<Constants.ProjectPermission>(dataContext.permission, true, out result);
      this.OptionControl.SwitchPermission((int) result);
    }

    private void OnOpenCheckChanged(object sender, RoutedEventArgs e)
    {
      Utils.FindParent<ShareProjectDialog>((DependencyObject) this)?.OpenToTeamChanged(this.OpenCheckBox.IsChecked.GetValueOrDefault());
    }

    private void InitEvent()
    {
      this.OptionControl.Delete += new EventHandler(this.DeleteUser);
      this.OptionControl.Exit += new EventHandler(this.ExitClick);
      this.OptionControl.Transfer += new EventHandler(this.TransferClick);
      this.OptionControl.PermissionSelect += new EventHandler<int>(this.UserPermissionSet);
    }

    private void UserPermissionSet(object sender, int e)
    {
      if (!(this.DataContext is ShareUserViewMode dataContext))
        return;
      Utils.FindParent<ShareProjectDialog>((DependencyObject) this)?.SetUserPermission(dataContext, e);
    }

    private void TransferClick(object sender, EventArgs e)
    {
      Utils.FindParent<ShareProjectDialog>((DependencyObject) this)?.TransferProjectOwner();
    }

    private void DeleteUser(object sender, EventArgs e)
    {
      Utils.FindParent<ShareProjectDialog>((DependencyObject) this)?.DeleteUser(this.DataContext, (EventArgs) null);
    }

    private void ExitClick(object sender, EventArgs e)
    {
      Utils.FindParent<ShareProjectDialog>((DependencyObject) this)?.ExitProject(sender, (EventArgs) null);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/project/shareuseritemcontrol.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnLoaded);
          break;
        case 2:
          this.DisplayName = (TextBlock) target;
          break;
        case 3:
          this.UserName = (TextBlock) target;
          break;
        case 4:
          this.OwnerOrMyPermission = (TextBlock) target;
          break;
        case 5:
          this.OptionControl = (ProjectPermissionSetControl) target;
          break;
        case 6:
          this.OpenCheckBox = (CheckBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
