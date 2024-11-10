// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.OperationDialog
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class OperationDialog : UserControl, ITabControl, IComponentConnector
  {
    protected readonly string _id;
    private readonly EscPopup _popup;
    private OperationItemViewModel _hoverButton;
    private bool _isChild;
    private PopupLocationInfo _tracker = new PopupLocationInfo();
    internal UpDownSelectListView OptionItems;
    internal EscPopup SubPopup;
    private bool _contentLoaded;

    public OperationDialog(string habitKey, OperationItemViewModel type)
      : this(habitKey, new List<OperationItemViewModel>()
      {
        type
      })
    {
    }

    public OperationDialog(string id, List<OperationItemViewModel> types, EscPopup popup = null)
    {
      this._id = id;
      EscPopup escPopup1 = popup;
      if (escPopup1 == null)
      {
        EscPopup escPopup2 = new EscPopup();
        escPopup2.StaysOpen = false;
        escPopup2.Placement = PlacementMode.MousePoint;
        escPopup2.VerticalOffset = -5.0;
        escPopup2.HorizontalOffset = -5.0;
        escPopup1 = escPopup2;
      }
      this._popup = escPopup1;
      this.InitializeComponent();
      this._popup.Closed += new EventHandler(this.OnPopupClosed);
      this.OptionItems.ItemsSource = (IEnumerable) types;
    }

    public event EventHandler<KeyValuePair<string, ActionType>> Operated;

    public event EventHandler Closed;

    private void OnPopupClosed(object sender, EventArgs e)
    {
      PopupStateManager.OnViewPopupClosed();
      EventHandler closed = this.Closed;
      if (closed != null)
        closed((object) this, e);
      this.Operated = (EventHandler<KeyValuePair<string, ActionType>>) null;
      this.Closed = (EventHandler) null;
    }

    protected virtual async void OnActionClick(OperationItemViewModel model)
    {
      OperationDialog sender = this;
      sender._popup.IsOpen = false;
      EventHandler<KeyValuePair<string, ActionType>> operated = sender.Operated;
      if (operated == null)
        return;
      operated((object) sender, new KeyValuePair<string, ActionType>(sender._id, model.Type));
    }

    public void Show()
    {
      PopupStateManager.OnViewPopupOpened();
      this._popup.Child = (UIElement) this;
      this._popup.IsOpen = true;
    }

    private void OnActionMouseMove(object sender, MouseEventArgs e)
    {
      if (!this.SubPopup.IsOpen)
        return;
      if (!OperationDialog.IsInTargetShowPopupArea(sender))
      {
        if (this._tracker.IsInSafeArea())
          return;
        this.SubPopup.IsOpen = false;
      }
      else
        this._tracker.Mark();
    }

    private static bool IsInTargetShowPopupArea(object sender)
    {
      return sender is OptionItemWithImageIcon itemWithImageIcon && itemWithImageIcon.DataContext is OperationItemViewModel dataContext && dataContext.SubActions != null && dataContext.SubActions.Any<OperationItemViewModel>();
    }

    private void OnActionMouseEnter(object sender, MouseEventArgs e)
    {
      if (!OperationDialog.IsInTargetShowPopupArea(sender))
        return;
      this.ShowSubPopup((sender is OptionItemWithImageIcon itemWithImageIcon ? itemWithImageIcon.DataContext : (object) null) as OperationItemViewModel);
    }

    private void ShowSubPopup(OperationItemViewModel model, bool onEnter = false)
    {
      if (model.SubActions == null || !model.SubActions.Any<OperationItemViewModel>())
        return;
      // ISSUE: explicit non-virtual call
      int? nullable1 = this.OptionItems.ItemsSource is List<OperationItemViewModel> itemsSource ? new int?(__nonvirtual (itemsSource.IndexOf(model))) : new int?();
      if (nullable1.HasValue)
      {
        int? nullable2 = nullable1;
        int num = 0;
        if (nullable2.GetValueOrDefault() >= num & nullable2.HasValue)
          this.SubPopup.PlacementTarget = (UIElement) Utils.GetListViewItem((ListView) this.OptionItems, nullable1.Value);
      }
      model.HoverSelected = true;
      this._hoverButton = model;
      this._hoverButton.SubOpened = true;
      this.SubPopup.StaysOpen = true;
      this.SubPopup.Placement = PlacementMode.Right;
      OperationDialog operationDialog = new OperationDialog(this._id, model.SubActions, this.SubPopup)
      {
        _isChild = true
      };
      operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) ((s, t) =>
      {
        this._popup.IsOpen = false;
        EventHandler closed = this.Closed;
        if (closed != null)
          closed(s, (EventArgs) null);
        EventHandler<KeyValuePair<string, ActionType>> operated = this.Operated;
        if (operated == null)
          return;
        operated(s, t);
      });
      operationDialog.Show();
      if (onEnter)
        operationDialog.OptionItems.HoverFirst();
      this._tracker.Bind((Popup) this.SubPopup);
    }

    private void SubpopupClosed(object sender, EventArgs e)
    {
      if (this._hoverButton == null)
        return;
      this._hoverButton.SubOpened = false;
      this._hoverButton = (OperationItemViewModel) null;
    }

    private void OnItemSelect(bool onenter, UpDownSelectViewModel e)
    {
      if (!(e is OperationItemViewModel model) || !model.Enable)
        return;
      if (model.SubActions == null || model.SubActions.Count == 0)
      {
        this.OnActionClick(model);
      }
      else
      {
        if (this.SubPopup.IsOpen)
          return;
        this.ShowSubPopup(model, true);
      }
    }

    private void OnLeftRightKeyDown(object sender, bool isLeft)
    {
      if (this.OptionItems.ItemsSource is List<OperationItemViewModel> itemsSource)
      {
        OperationItemViewModel model = itemsSource.FirstOrDefault<OperationItemViewModel>((Func<OperationItemViewModel, bool>) (m => m.HoverSelected));
        if (model != null)
        {
          if (this._isChild & isLeft)
            this._popup.IsOpen = false;
          if (!this._isChild && !isLeft && !this.SubPopup.IsOpen)
            this.ShowSubPopup(model, true);
        }
      }
      if (!isLeft || !this.SubPopup.IsOpen)
        return;
      this.SubPopup.IsOpen = false;
    }

    public bool HandleTab(bool shift)
    {
      this.UpDownSelect(shift);
      return true;
    }

    public bool HandleEnter()
    {
      if (!this.SubPopup.IsOpen)
        return this.OptionItems.HandleEnter();
      this.SubPopup.HandleEnter();
      return true;
    }

    public bool HandleEsc() => false;

    public bool UpDownSelect(bool isUp)
    {
      if (!this.SubPopup.IsOpen)
        return this.OptionItems.UpDownSelect(isUp);
      this.SubPopup.HandleUpDown(isUp);
      return true;
    }

    public bool LeftRightSelect(bool isLeft)
    {
      if (!(this.SubPopup.IsOpen & isLeft))
        return this.OptionItems.LeftRightSelect(isLeft);
      this.SubPopup.IsOpen = false;
      return true;
    }

    public void SetPlaceTarget(UIElement ele)
    {
      if (this._popup == null)
        return;
      this._popup.PlacementTarget = ele;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/operationdialog.xaml", UriKind.Relative));
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
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.SubPopup = (EscPopup) target;
        else
          this._contentLoaded = true;
      }
      else
        this.OptionItems = (UpDownSelectListView) target;
    }
  }
}
