// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomPopup.SetClosedStatusPopup
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.CustomPopup
{
  public class SetClosedStatusPopup : EscPopup, IComponentConnector
  {
    private CustomMenuList _moreMenu;
    private bool _isCompleted;
    private bool _isAbandoned;
    private bool _contentLoaded;

    public event EventHandler Abandoned;

    public event EventHandler Completed;

    private SetClosedStatusPopup()
    {
      this.InitializeComponent();
      this.Closed += (EventHandler) ((s, e) =>
      {
        this.Abandoned = (EventHandler) null;
        this.Completed = (EventHandler) null;
      });
    }

    public static SetClosedStatusPopup GetInstance(
      UIElement targetElement,
      bool isCompleted,
      bool isAbandoned,
      double verticalOffset = 0.0,
      double horizontalOffset = 0.0)
    {
      SetClosedStatusPopup instance = new SetClosedStatusPopup()
      {
        _isCompleted = isCompleted,
        _isAbandoned = isAbandoned
      };
      if (targetElement != null)
        instance.PlacementTarget = targetElement;
      else
        instance.Placement = PlacementMode.Mouse;
      instance.VerticalOffset = verticalOffset;
      instance.HorizontalOffset = horizontalOffset;
      return instance;
    }

    public void Show()
    {
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "complete", Utils.GetString("Completed"), (Geometry) null);
      menuItemViewModel1.Selected = this._isCompleted;
      types.Add(menuItemViewModel1);
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "abandoned", Utils.GetString("Abandoned"), (Geometry) null);
      menuItemViewModel2.Selected = this._isAbandoned;
      types.Add(menuItemViewModel2);
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this);
      customMenuList.Operated += new EventHandler<object>(this.OnSelected);
      customMenuList.Show();
    }

    private void OnSelected(object sender, object e)
    {
      if (!(e is string str))
        return;
      if (str == "complete" && !this._isCompleted)
      {
        EventHandler completed = this.Completed;
        if (completed != null)
          completed((object) null, (EventArgs) null);
      }
      if (!(str == "abandoned") || this._isAbandoned)
        return;
      EventHandler abandoned = this.Abandoned;
      if (abandoned == null)
        return;
      abandoned((object) null, (EventArgs) null);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/custompopup/setclosedstatuspopup.xaml", UriKind.Relative));
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
    void IComponentConnector.Connect(int connectionId, object target) => this._contentLoaded = true;
  }
}
