// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Team.InviteNormalItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Team
{
  public class InviteNormalItem : UserControl, IComponentConnector
  {
    private InviteGroupItem _groupParent;
    private InviteUsersDialog _parent;
    internal InviteNormalItem Root;
    internal ColumnDefinition TheThirdColumn;
    private bool _contentLoaded;

    public InviteNormalItem() => this.InitializeComponent();

    private void InviteNormalItem_OnLoaded(object sender, RoutedEventArgs e)
    {
      this._groupParent = Utils.FindParent<InviteGroupItem>((DependencyObject) this);
      this._parent = Utils.FindParent<InviteUsersDialog>((DependencyObject) this);
    }

    private void OnSelectedItem(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is InviteUserModel dataContext))
        return;
      dataContext.Selected = !dataContext.Selected;
      if (this._groupParent != null)
        this._groupParent.ItemSelected(dataContext.UserCode, dataContext.Selected);
      else
        this._parent?.CheckInviteEnable(dataContext.Selected);
    }

    private void OnDeleteClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (!(this.DataContext is InviteUserModel dataContext))
        return;
      this._parent?.DeleteRecentRecord(dataContext.Email);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/team/invitenormalitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (InviteNormalItem) target;
          this.Root.Loaded += new RoutedEventHandler(this.InviteNormalItem_OnLoaded);
          break;
        case 2:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnSelectedItem);
          break;
        case 3:
          this.TheThirdColumn = (ColumnDefinition) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
