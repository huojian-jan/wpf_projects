// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.EscPopup
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class EscPopup : Popup
  {
    public bool NeedFocus = true;

    public EscPopup()
    {
      this.AllowsTransparency = true;
      this.Opened += new EventHandler(this.OnOpened);
      this.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
      this.KeyUp += new KeyEventHandler(this.OnKeyUp);
      this.Closed += new EventHandler(this.OnClosed);
    }

    private void OnClosed(object sender, EventArgs e)
    {
      if (this.PlacementTarget == null)
        return;
      HwndHelper.SetFocus(this.PlacementTarget);
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          ITabControl tabControl1 = this.GetTabControl();
          if (tabControl1 == null)
            break;
          e.Handled = tabControl1.HandleEnter();
          break;
        case Key.Escape:
          ITabControl tabControl2 = this.GetTabControl();
          if (tabControl2 != null && tabControl2.HandleEsc())
          {
            e.Handled = true;
            break;
          }
          this.Close();
          e.Handled = true;
          break;
      }
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Tab:
          this.GetTabControl()?.HandleTab(Utils.IfShiftPressed());
          e.Handled = true;
          break;
        case Key.Escape:
          e.Handled = true;
          break;
        case Key.Left:
        case Key.Right:
          ITabControl tabControl1 = this.GetTabControl();
          if (tabControl1 == null)
            break;
          e.Handled = tabControl1.LeftRightSelect(e.Key == Key.Left);
          break;
        case Key.Up:
        case Key.Down:
          ITabControl tabControl2 = this.GetTabControl();
          if (tabControl2 == null)
            break;
          e.Handled = tabControl2.UpDownSelect(e.Key == Key.Up);
          break;
      }
    }

    private ITabControl GetTabControl()
    {
      if (this.Child == null)
        return (ITabControl) null;
      return this.Child is ITabControl child ? child : Utils.FindSingleVisualChildren<ITabControl>((DependencyObject) this.Child);
    }

    private async void OnOpened(object sender, EventArgs e)
    {
      EscPopup escPopup = this;
      await Task.Delay(50);
      if (!escPopup.NeedFocus)
        return;
      HwndHelper.SetFocus((Popup) escPopup, !escPopup.StaysOpen);
    }

    public bool HandleTab(bool shift)
    {
      ITabControl tabControl = this.GetTabControl();
      return tabControl != null && tabControl.HandleTab(shift);
    }

    public bool HandleUpDown(bool isUp)
    {
      ITabControl tabControl = this.GetTabControl();
      return tabControl != null && tabControl.UpDownSelect(isUp);
    }

    public bool HandleLeftRight(bool isLeft)
    {
      ITabControl tabControl = this.GetTabControl();
      return tabControl != null && tabControl.LeftRightSelect(isLeft);
    }

    public bool HandleEnter()
    {
      ITabControl tabControl = this.GetTabControl();
      return tabControl != null && tabControl.HandleEnter();
    }

    public bool HandleEsc()
    {
      ITabControl tabControl = this.GetTabControl();
      if (tabControl != null && tabControl.HandleEsc())
        return true;
      this.Close();
      return false;
    }

    public void Close() => this.IsOpen = false;

    public async void TryFocus()
    {
      EscPopup element = this;
      Keyboard.ClearFocus();
      await Task.Delay(10);
      FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
      Keyboard.Focus((IInputElement) element);
    }
  }
}
