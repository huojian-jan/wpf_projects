// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.TransferProjectWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class TransferProjectWindow : Window, IComponentConnector
  {
    private readonly string _projectId;
    private string _selectedUserCode;
    private string _selectedUserName;
    private int _userCount;
    internal TextBlock ComboHint;
    internal ComboBox AvatarComboBox;
    internal Button SaveButton;
    private bool _contentLoaded;

    public TransferProjectWindow(string projectId)
    {
      this.InitializeComponent();
      this._projectId = projectId;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      TransferProjectWindow transferProjectWindow = this;
      if (!await transferProjectWindow.TransferProject())
        return;
      transferProjectWindow.DialogResult = new bool?(true);
      transferProjectWindow.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnWindowLoaded(object sender, RoutedEventArgs e) => this.InitData();

    private async Task<bool> TransferProject()
    {
      TransferProjectWindow transferProjectWindow = this;
      if (!string.IsNullOrEmpty(transferProjectWindow._selectedUserCode))
      {
        ErrorModel errorModel = await Communicator.TransferProject(transferProjectWindow._projectId, transferProjectWindow._selectedUserCode);
        if (errorModel != null)
        {
          string content = string.Empty;
          switch (errorModel.errorCode)
          {
            case "no_project_permission":
            case "no_share_permission":
              content = string.Format(Utils.GetString("TransferCannotFound"), (object) transferProjectWindow._selectedUserName);
              break;
            case "not_project_owner":
              content = Utils.GetString("TransferNotOwner");
              break;
            case "user_not_exist":
              content = string.Format(Utils.GetString("TransferCannotFound"), (object) transferProjectWindow._selectedUserName);
              break;
            case "project_transfer":
              content = string.Format(Utils.GetString("TransferNotProAccount"), (object) transferProjectWindow._selectedUserName, (object) transferProjectWindow._userCount);
              break;
            case "exceed_team_to_pro_max_share_limit":
              content = string.Format(Utils.GetString("TransferNotTeamAccount"), (object) transferProjectWindow._selectedUserName);
              break;
          }
          CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("TransferFailed"), content, Utils.GetString("GotIt"), string.Empty);
          customerDialog.Owner = Window.GetWindow((DependencyObject) transferProjectWindow);
          customerDialog.ShowDialog();
          return false;
        }
      }
      return true;
    }

    private async void InitData()
    {
      TransferProjectWindow transferProjectWindow = this;
      List<ShareUserModel> projectUsersAsync = await AvatarHelper.GetProjectUsersAsync(transferProjectWindow._projectId, true);
      List<AvatarComboBoxViewModel> comboBoxViewModelList = new List<AvatarComboBoxViewModel>();
      if (projectUsersAsync != null && projectUsersAsync.Count != 0)
      {
        comboBoxViewModelList.AddRange(projectUsersAsync.Where<ShareUserModel>((Func<ShareUserModel, bool>) (item =>
        {
          if (item.isAccept)
          {
            long? userId = item.userId;
            long currentUserIdInt = (long) Utils.GetCurrentUserIdInt();
            if (!(userId.GetValueOrDefault() == currentUserIdInt & userId.HasValue) && !item.deleted)
              return !item.visitor;
          }
          return false;
        })).Select<ShareUserModel, AvatarComboBoxViewModel>((Func<ShareUserModel, AvatarComboBoxViewModel>) (item => new AvatarComboBoxViewModel()
        {
          AvatarUrl = item.avatarUrl,
          Name = TransferProjectWindow.GetDisplayName(item),
          UserId = item.userId.ToString(),
          UserCode = item.userCode
        })));
        transferProjectWindow._userCount = projectUsersAsync.Count;
      }
      transferProjectWindow.AvatarComboBox.ItemsSource = (IEnumerable) comboBoxViewModelList;
      transferProjectWindow.AvatarComboBox.SelectionChanged += new SelectionChangedEventHandler(transferProjectWindow.OnSelectionChanged);
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      this.ComboHint.Visibility = Visibility.Collapsed;
      this.SaveButton.IsEnabled = true;
      AvatarComboBoxViewModel selectedItem = (AvatarComboBoxViewModel) this.AvatarComboBox.SelectedItem;
      this._selectedUserCode = selectedItem.UserCode;
      this._selectedUserName = selectedItem.Name;
    }

    private static string GetDisplayName(ShareUserModel item)
    {
      return !string.IsNullOrEmpty(item.displayName) ? item.displayName : item.username;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/project/transferprojectwindow.xaml", UriKind.Relative));
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
          this.ComboHint = (TextBlock) target;
          break;
        case 3:
          this.AvatarComboBox = (ComboBox) target;
          break;
        case 4:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
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
