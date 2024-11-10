// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.SetAssigneeDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.QuickAdd;

#nullable disable
namespace ticktick_WPF.Views
{
  public class SetAssigneeDialog : UserControl, ITabControl, IComponentConnector
  {
    private readonly Popup _popup;
    private string _projectId;
    private string _assignee;
    internal ContentControl Container;
    private bool _contentLoaded;

    public Popup GetPopup() => this._popup;

    public SetAssigneeDialog(string projectId, Popup popup = null, string assignee = null)
    {
      this._projectId = projectId;
      this._assignee = assignee;
      if (popup != null)
      {
        this._popup = popup;
      }
      else
      {
        EscPopup escPopup = new EscPopup();
        escPopup.StaysOpen = false;
        escPopup.Placement = PlacementMode.MousePoint;
        escPopup.VerticalOffset = 0.0;
        escPopup.HorizontalOffset = -120.0;
        this._popup = (Popup) escPopup;
      }
      this._popup.Closed -= new EventHandler(this.OnClosed);
      this._popup.Closed += new EventHandler(this.OnClosed);
      this.InitializeComponent();
    }

    private void OnClosed(object sender, EventArgs e)
    {
      EventHandler closed = this.Closed;
      if (closed == null)
        return;
      closed((object) this, (EventArgs) null);
    }

    public event EventHandler<AvatarInfo> AssigneeSelect;

    public event EventHandler Closed;

    private void OnInitialized(object sender, EventArgs e)
    {
      this.SetItems(this._projectId, this._assignee);
    }

    public async void SetItems(string projectId, string assignee = null)
    {
      SetAssigneeDialog setAssigneeDialog = this;
      setAssigneeDialog._projectId = projectId;
      List<AvatarViewModel> projectAvatars = await AvatarHelper.GetProjectAvatars(setAssigneeDialog._projectId);
      projectAvatars.Insert(0, new AvatarViewModel()
      {
        UserId = "-1",
        Name = Utils.GetString("NoAvatar"),
        IsNoAvatar = true
      });
      AssignInputItems assignInputItems = new AssignInputItems(projectAvatars, false, assignee);
      assignInputItems.OnItemSelected -= new EventHandler<InputItemViewModel<AvatarViewModel>>(setAssigneeDialog.OnAssignSelect);
      assignInputItems.OnItemSelected += new EventHandler<InputItemViewModel<AvatarViewModel>>(setAssigneeDialog.OnAssignSelect);
      setAssigneeDialog.Container.Content = (object) assignInputItems;
    }

    private void OnAssignSelect(object sender, InputItemViewModel<AvatarViewModel> selected)
    {
      EventHandler<AvatarInfo> assigneeSelect = this.AssigneeSelect;
      if (assigneeSelect != null)
        assigneeSelect((object) this, new AvatarInfo(selected.Value, selected.ImageUrl));
      this._popup.IsOpen = false;
    }

    public void Show()
    {
      this._popup.Child = (UIElement) this;
      this._popup.IsOpen = true;
    }

    public void Move(bool forward)
    {
      if (!(this.Container.Content is AssignInputItems content))
        return;
      content.Move(forward);
    }

    public void EnterSelect()
    {
      if (!(this.Container.Content is AssignInputItems content))
        return;
      content.TrySelectItem(false);
    }

    public bool HandleTab(bool shift)
    {
      this.Move(shift);
      return true;
    }

    public bool HandleEnter()
    {
      this.EnterSelect();
      return true;
    }

    public bool HandleEsc() => false;

    public bool UpDownSelect(bool isUp)
    {
      this.Move(isUp);
      return true;
    }

    public bool LeftRightSelect(bool isLeft) => false;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setassigneedialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.Container = (ContentControl) target;
        else
          this._contentLoaded = true;
      }
      else
        ((FrameworkElement) target).Initialized += new EventHandler(this.OnInitialized);
    }
  }
}
