// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomControl.CustomMenuList
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
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.CustomControl
{
  public class CustomMenuList : UserControl, ITabControl, IComponentConnector, IStyleConnector
  {
    private readonly Popup _popup;
    private CustomMenuItemViewModel _hoverButton;
    private bool _isChild;
    private ITabControl _topTabControl;
    private ITabControl _currentFocus;
    private readonly PopupLocationInfo _popupTracker = new PopupLocationInfo();
    internal ContentControl Container;
    internal ContentControl TopItem;
    internal UpDownSelectListView Items;
    internal EscPopup SubPopup;
    private bool _contentLoaded;

    public CustomMenuList(
      IEnumerable<CustomMenuItemViewModel> types,
      Popup popup = null,
      ITabControl topTabControl = null)
    {
      this.InitializeComponent();
      Popup popup1 = popup;
      if (popup1 == null)
      {
        EscPopup escPopup = new EscPopup();
        escPopup.StaysOpen = false;
        escPopup.Placement = PlacementMode.MousePoint;
        escPopup.VerticalOffset = -5.0;
        escPopup.HorizontalOffset = -5.0;
        popup1 = (Popup) escPopup;
      }
      this._popup = popup1;
      this._popup.Closed += new EventHandler(this.OnPopupClosed);
      this.Items.ItemsSource = (IEnumerable) types;
      this.Items.CanTabSelect = true;
      if (topTabControl is UIElement uiElement)
      {
        this.TopItem.Content = (object) uiElement;
        this._topTabControl = topTabControl;
        this._currentFocus = this._topTabControl;
        this.Items.CanUpDownOverLimit = true;
      }
      else
        this._currentFocus = (ITabControl) this.Items;
    }

    public event EventHandler<object> Operated;

    public event EventHandler Closed;

    public bool AutoClose { get; set; } = true;

    private async void OnPopupClosed(object sender, EventArgs e)
    {
      CustomMenuList sender1 = this;
      sender1._popup.Closed -= new EventHandler(sender1.OnPopupClosed);
      EventHandler closed = sender1.Closed;
      if (closed != null)
        closed((object) sender1, e);
      sender1._popup.Child = (UIElement) null;
      await Task.Delay(40);
      sender1.Closed = (EventHandler) null;
      sender1.Operated = (EventHandler<object>) null;
    }

    private void OnActionClick(CustomMenuItemViewModel model)
    {
      EventHandler<object> operated = this.Operated;
      if (operated != null)
        operated((object) this, model.Value);
      if (!this.AutoClose)
        return;
      this._popup.IsOpen = false;
    }

    public void Show()
    {
      this._popup.Child = (UIElement) this;
      this._popup.IsOpen = true;
    }

    private void OnActionMouseEnter(object sender, MouseEventArgs e)
    {
      if (sender is FrameworkElement frameworkElement && frameworkElement.DataContext is CustomMenuItemViewModel)
        return;
      this.SubPopup.IsOpen = false;
    }

    private async Task ShowSubPopup(CustomMenuItemViewModel model, bool onEnter = false)
    {
      CustomMenuList customMenuList1 = this;
      if ((model.SubActions == null || !model.SubActions.Any<CustomMenuItemViewModel>()) && model.SubControl == null)
        return;
      // ISSUE: explicit non-virtual call
      int? nullable1 = customMenuList1.Items.ItemsSource is List<CustomMenuItemViewModel> itemsSource ? new int?(__nonvirtual (itemsSource.IndexOf(model))) : new int?();
      if (nullable1.HasValue)
      {
        int? nullable2 = nullable1;
        int num = 0;
        if (nullable2.GetValueOrDefault() >= num & nullable2.HasValue)
        {
          ListViewItem listViewItem = Utils.GetListViewItem((ListView) customMenuList1.Items, nullable1.Value);
          customMenuList1.SubPopup.PlacementTarget = (UIElement) listViewItem;
        }
      }
      model.HoverSelected = true;
      customMenuList1._hoverButton = model;
      customMenuList1._hoverButton.SubOpened = true;
      customMenuList1.SubPopup.StaysOpen = true;
      customMenuList1.SubPopup.Placement = PlacementMode.Right;
      CustomMenuList customMenuList2 = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) model.SubActions, (Popup) customMenuList1.SubPopup, model.SubControl)
      {
        _isChild = true
      };
      if (!model.NeedSubContentStyle)
        customMenuList2.Container.Style = (Style) null;
      // ISSUE: reference to a compiler-generated method
      customMenuList2.Operated += new EventHandler<object>(customMenuList1.\u003CShowSubPopup\u003Eb__21_0);
      customMenuList2.Show();
      if (onEnter)
        customMenuList2.Items.HoverFirst();
      await Task.Delay(120);
      customMenuList1._popupTracker.Bind((Popup) customMenuList1.SubPopup);
      customMenuList1._popupTracker.Mark();
    }

    private void SubpopupClosed(object sender, EventArgs e)
    {
      if (this._hoverButton == null)
        return;
      this._hoverButton.SubOpened = false;
      this._hoverButton = (CustomMenuItemViewModel) null;
    }

    private void OnItemSelect(bool onenter, UpDownSelectViewModel e)
    {
      if (!(e is CustomMenuItemViewModel model))
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
      if (!(this.Items.ItemsSource is List<CustomMenuItemViewModel> itemsSource))
        return;
      CustomMenuItemViewModel model = itemsSource.FirstOrDefault<CustomMenuItemViewModel>((Func<CustomMenuItemViewModel, bool>) (m => m.HoverSelected));
      if (model == null)
        return;
      if (this._isChild & isLeft)
        this._popup.IsOpen = false;
      if (this._isChild || isLeft || this.SubPopup.IsOpen)
        return;
      this.ShowSubPopup(model, true);
    }

    public void Close() => this._popup.IsOpen = false;

    public bool HandleTab(bool shift)
    {
      if (this.SubPopup.IsOpen)
      {
        this.SubPopup.HandleUpDown(shift);
        return true;
      }
      if (this._currentFocus.HandleTab(shift))
        return true;
      this._currentFocus = !object.Equals((object) this._currentFocus, (object) this.Items) || this._topTabControl == null ? (ITabControl) this.Items : this._topTabControl;
      return this._currentFocus.HandleTab(shift);
    }

    public bool HandleEnter()
    {
      if (!this._currentFocus.HandleEnter())
        return false;
      if (this.AutoClose)
        this._popup.IsOpen = false;
      return true;
    }

    public bool HandleEsc() => this._currentFocus.HandleEsc();

    public bool UpDownSelect(bool isUp)
    {
      if (this.SubPopup.IsOpen)
      {
        this.SubPopup.HandleUpDown(isUp);
        return true;
      }
      if (this._currentFocus.UpDownSelect(isUp))
        return true;
      this._currentFocus = !object.Equals((object) this._currentFocus, (object) this.Items) || this._topTabControl == null ? (ITabControl) this.Items : this._topTabControl;
      return this._currentFocus.UpDownSelect(isUp);
    }

    public bool LeftRightSelect(bool isLeft)
    {
      if (!isLeft || !this.SubPopup.IsOpen)
        return this._currentFocus.LeftRightSelect(isLeft);
      this.SubPopup.IsOpen = false;
      return true;
    }

    private void OnItemLoaded(object sender, RoutedEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is CustomMenuItemViewModel dataContext))
        return;
      if (dataContext.FontSize == 12)
        frameworkElement.MaxHeight = 30.0;
      if (dataContext.FontSize != 11)
        return;
      frameworkElement.MaxHeight = 28.0;
    }

    public void CloseSubPopup(object sender, bool e)
    {
      this.SubPopup.IsOpen = false;
      this._popup.IsOpen = false;
    }

    private void OnActionMouseMove(object sender, MouseEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is CustomMenuItemViewModel dataContext) || dataContext.SubOpened && object.Equals((object) this._hoverButton, (object) dataContext) || this.SubPopup.IsOpen && this._popupTracker.IsInSafeArea())
        return;
      this.SubPopup.IsOpen = false;
      this.ShowSubPopup(dataContext);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/customcontrol/custommenulist.xaml", UriKind.Relative));
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
        case 3:
          this.Container = (ContentControl) target;
          break;
        case 4:
          this.TopItem = (ContentControl) target;
          break;
        case 5:
          this.Items = (UpDownSelectListView) target;
          break;
        case 6:
          this.SubPopup = (EscPopup) target;
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
      if (connectionId != 1)
      {
        if (connectionId != 2)
          return;
        ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnActionMouseEnter);
        ((UIElement) target).MouseMove += new MouseEventHandler(this.OnActionMouseMove);
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnItemLoaded);
      }
      else
      {
        ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnActionMouseEnter);
        ((UIElement) target).MouseMove += new MouseEventHandler(this.OnActionMouseMove);
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnItemLoaded);
      }
    }
  }
}
