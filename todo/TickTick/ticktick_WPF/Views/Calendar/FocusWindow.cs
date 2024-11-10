// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.FocusWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Pomo;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class FocusWindow : IndependentWindow
  {
    public static FocusWindow Window;
    public static string Name = DisplayModule.Pomo.ToString();
    private FocusView _focus;

    public static bool IsShowing => FocusWindow.Window != null;

    private FocusWindow(WindowModel windowModel)
      : base(windowModel)
    {
      this.Id = FocusWindow.Name;
      this.MinWidth = 400.0;
      this.MinHeight = 600.0;
      FocusView focusView = new FocusView();
      focusView.MinWidth = 400.0;
      this._focus = focusView;
      this.Container.Children.Add((UIElement) this._focus);
      this.Title = Utils.GetString("PomoFocus");
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnWindowMouseUp);
      this.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnWindowMouseDown);
    }

    private void OnWindowMouseDown(object sender, MouseButtonEventArgs e) => this._mouseDown = true;

    private async void OnWindowMouseUp(object sender, MouseButtonEventArgs e)
    {
      FocusWindow element = this;
      if (!element._mouseDown)
        return;
      element._mouseDown = false;
      if (Utils.GetMousePointVisibleItem<TextBox>((MouseEventArgs) e, (FrameworkElement) element) != null)
        return;
      await Task.Delay(100);
      if (!element.IsActive || e.Handled)
        return;
      element.TryFocus();
    }

    public async Task TryFocus()
    {
      FocusWindow element = this;
      await Task.Delay(10);
      Keyboard.ClearFocus();
      if (!element.IsActive)
        return;
      FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
      Keyboard.Focus((IInputElement) element);
    }

    public static async Task ShowWindow(bool force = true)
    {
      WindowModel windowModel = await WindowModelDao.GetModelByIdAsync(FocusWindow.Name);
      if (!force)
      {
        if (windowModel == null)
        {
          windowModel = (WindowModel) null;
          return;
        }
        if (windowModel.Closed)
        {
          windowModel = (WindowModel) null;
          return;
        }
      }
      if (FocusWindow.Window != null)
      {
        FocusWindow.Window.Show();
        FocusWindow.Window.Activate();
        FocusWindow.Window.Reload();
        windowModel = (WindowModel) null;
      }
      else
      {
        if (windowModel == null || windowModel.Closed)
          IndependentWindow.AddShowEvent("focus");
        if (windowModel == null)
          windowModel = await IndependentWindow.GetNewWindowModel(FocusWindow.Name);
        else
          await WindowModelDao.OpenWindow(windowModel);
        FocusWindow.Window = new FocusWindow(windowModel);
        FocusWindow.Window.Show();
        FocusWindow.Window.SetBackBrush();
        windowModel = (WindowModel) null;
      }
    }

    private void Reload() => this._focus?.Reload();

    protected override void OnClosing()
    {
      this.Container.Children.Clear();
      FocusWindow.Window = (FocusWindow) null;
    }

    public override void Print()
    {
    }

    public static void OpenOrCloseWindow(System.Windows.Window window)
    {
      if (FocusWindow.Window != null)
      {
        FocusWindow.Window.Close();
      }
      else
      {
        if (!IndependentWindow.CheckCount(window))
          return;
        FocusWindow.ShowWindow();
      }
    }
  }
}
