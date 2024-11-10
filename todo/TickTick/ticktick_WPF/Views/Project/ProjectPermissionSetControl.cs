// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.ProjectPermissionSetControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class ProjectPermissionSetControl : UserControl, IComponentConnector
  {
    private bool _canModify = true;
    public static readonly DependencyProperty InUserItemProperty = DependencyProperty.Register(nameof (InUserItem), typeof (bool), typeof (ProjectPermissionSetControl), new PropertyMetadata((object) false, new PropertyChangedCallback(ProjectPermissionSetControl.OnInUserItemChanged)));
    public static readonly DependencyProperty SelectedPermissionProperty = DependencyProperty.Register(nameof (SelectedPermission), typeof (Constants.ProjectPermission), typeof (ProjectPermissionSetControl), new PropertyMetadata((object) Constants.ProjectPermission.write, (PropertyChangedCallback) null));
    public EventHandler<int> PermissionSelect;
    public EventHandler Transfer;
    public EventHandler Exit;
    public EventHandler Delete;
    public EventHandler<bool> NeedAuditChange;
    internal ProjectPermissionSetControl Root;
    internal Grid Container;
    internal StackPanel OptionPanel;
    internal TextBlock OptionText;
    internal Path Arrow;
    internal Image TransferImage;
    internal Image ExitProject;
    internal EscPopup OptionPopup;
    internal OptionCheckBox WriteButton;
    internal OptionCheckBox CommentButton;
    internal OptionCheckBox ReadOnlyButton;
    internal Line DividerLine;
    internal Border DeletePanel;
    internal DockPanel NeedAuditPanel;
    internal CheckBox NeedAuditCheckBox;
    internal Border ApprovalTooltipBorder;
    private bool _contentLoaded;

    public ProjectPermissionSetControl() => this.InitializeComponent();

    public Constants.ProjectPermission SelectedPermission
    {
      get
      {
        return (Constants.ProjectPermission) this.GetValue(ProjectPermissionSetControl.SelectedPermissionProperty);
      }
      set => this.SetValue(ProjectPermissionSetControl.SelectedPermissionProperty, (object) value);
    }

    private static void OnInUserItemChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is ProjectPermissionSetControl permissionSetControl) || !(e.NewValue is bool newValue) || !newValue)
        return;
      permissionSetControl.DividerLine.Visibility = Visibility.Visible;
      permissionSetControl.DeletePanel.Visibility = Visibility.Visible;
    }

    public bool InUserItem
    {
      get => (bool) this.GetValue(ProjectPermissionSetControl.InUserItemProperty);
      set => this.SetValue(ProjectPermissionSetControl.InUserItemProperty, (object) value);
    }

    private void OnOptionButtonClick(object sender, RoutedEventArgs e)
    {
      if (this.OptionPanel.IsVisible)
        this.OptionPopup.IsOpen = true;
      if (this.TransferImage.IsVisible)
      {
        EventHandler transfer = this.Transfer;
        if (transfer != null)
          transfer((object) this, (EventArgs) null);
      }
      if (!this.ExitProject.IsVisible)
        return;
      EventHandler exit = this.Exit;
      if (exit == null)
        return;
      exit((object) this, (EventArgs) null);
    }

    private void OnOptionSelect(object sender, EventArgs e)
    {
      if (!this._canModify)
      {
        Utils.Toast(Utils.GetString("CantModifyPermission"));
      }
      else
      {
        int result;
        if (sender is FrameworkElement frameworkElement && int.TryParse((string) frameworkElement.Tag, out result))
        {
          this.SwitchPermission(result);
          EventHandler<int> permissionSelect = this.PermissionSelect;
          if (permissionSelect != null)
            permissionSelect((object) this, result);
        }
        this.OptionPopup.IsOpen = false;
      }
    }

    public void SwitchPermission(int tag)
    {
      switch (tag)
      {
        case 1:
          this.OptionText.Text = this.WriteButton.Text;
          break;
        case 2:
          this.OptionText.Text = this.CommentButton.Text;
          break;
        case 3:
          this.OptionText.Text = this.ReadOnlyButton.Text;
          break;
        default:
          tag = 1;
          this.OptionText.Text = this.WriteButton.Text;
          break;
      }
      this.SelectedPermission = (Constants.ProjectPermission) tag;
    }

    private void OnDeleteClick(object sender, MouseButtonEventArgs e)
    {
      EventHandler delete = this.Delete;
      if (delete != null)
        delete((object) this, (EventArgs) null);
      this.OptionPopup.IsOpen = false;
    }

    public void ShowTransfer()
    {
      this.OptionPanel.Visibility = Visibility.Collapsed;
      this.TransferImage.Visibility = Visibility.Visible;
    }

    public void ShowExit()
    {
      this.OptionPanel.Visibility = Visibility.Collapsed;
      this.ExitProject.Visibility = Visibility.Visible;
    }

    public void UnableChangePermission()
    {
      this.WriteButton.IsEnabled = false;
      this.CommentButton.IsEnabled = false;
      this.ReadOnlyButton.IsEnabled = false;
      this._canModify = false;
    }

    public void SetTeamItem()
    {
      this.DividerLine.Visibility = Visibility.Collapsed;
      this.DeletePanel.Visibility = Visibility.Collapsed;
    }

    public void SetShowNeedAuditPanel(bool show, bool? needAudit = null, bool isTeam = false)
    {
      this.WriteButton.Icon = (Geometry) null;
      this.CommentButton.Icon = (Geometry) null;
      this.ReadOnlyButton.Icon = (Geometry) null;
      if (show)
      {
        this.NeedAuditPanel.Visibility = Visibility.Visible;
        this.DividerLine.Visibility = Visibility.Visible;
      }
      else
      {
        this.NeedAuditPanel.Visibility = Visibility.Collapsed;
        this.DividerLine.Visibility = Visibility.Collapsed;
      }
      if (needAudit.HasValue)
        this.NeedAuditCheckBox.IsChecked = needAudit;
      this.ApprovalTooltipBorder.ToolTip = (object) Utils.GetString(isTeam ? "OwnerGuestHint" : "ApprovalByOwnerTooltip");
    }

    private void OnNeedAuditClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      this.NeedAuditCheckBox.IsChecked = new bool?(!this.NeedAuditCheckBox.IsChecked.GetValueOrDefault());
      EventHandler<bool> needAuditChange = this.NeedAuditChange;
      if (needAuditChange == null)
        return;
      needAuditChange((object) this, this.NeedAuditCheckBox.IsChecked.GetValueOrDefault());
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/project/projectpermissionsetcontrol.xaml", UriKind.Relative));
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
          this.Root = (ProjectPermissionSetControl) target;
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnOptionButtonClick);
          break;
        case 4:
          this.OptionPanel = (StackPanel) target;
          break;
        case 5:
          this.OptionText = (TextBlock) target;
          break;
        case 6:
          this.Arrow = (Path) target;
          break;
        case 7:
          this.TransferImage = (Image) target;
          break;
        case 8:
          this.ExitProject = (Image) target;
          break;
        case 9:
          this.OptionPopup = (EscPopup) target;
          break;
        case 10:
          this.WriteButton = (OptionCheckBox) target;
          break;
        case 11:
          this.CommentButton = (OptionCheckBox) target;
          break;
        case 12:
          this.ReadOnlyButton = (OptionCheckBox) target;
          break;
        case 13:
          this.DividerLine = (Line) target;
          break;
        case 14:
          this.DeletePanel = (Border) target;
          this.DeletePanel.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteClick);
          break;
        case 15:
          this.NeedAuditPanel = (DockPanel) target;
          break;
        case 16:
          this.NeedAuditCheckBox = (CheckBox) target;
          this.NeedAuditCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnNeedAuditClick);
          break;
        case 17:
          this.ApprovalTooltipBorder = (Border) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
