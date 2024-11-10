// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.HabitWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Habit;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class HabitWindow : IndependentWindow
  {
    public static HabitWindow Window;
    public static string Name = DisplayModule.Habit.ToString();
    private HabitContainer _habit;

    public static bool IsShowing => HabitWindow.Window != null;

    private HabitWindow(WindowModel windowModel)
      : base(windowModel)
    {
      double result;
      if (double.TryParse(windowModel.Data, out result))
      {
        this.Column1.Width = new GridLength(1.0, GridUnitType.Star);
        this.Column2.Width = new GridLength(result > 0.0 ? result : 1.0, GridUnitType.Star);
      }
      this.MinWidth = 400.0;
      this.MinHeight = 400.0;
      this.Id = HabitWindow.Name;
      this._habit = new HabitContainer();
      this.Container.Children.Add((UIElement) this._habit);
      this.Title = Utils.GetString("Habit");
      this._habit.LoadHabits();
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnWindowMouseUp);
      this.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnWindowMouseDown);
    }

    private void OnWindowMouseDown(object sender, MouseButtonEventArgs e) => this._mouseDown = true;

    private async void OnWindowMouseUp(object sender, MouseButtonEventArgs e)
    {
      HabitWindow element = this;
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
      HabitWindow element = this;
      await Task.Delay(10);
      Keyboard.ClearFocus();
      if (!element.IsActive)
        return;
      FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
      Keyboard.Focus((IInputElement) element);
    }

    public static async Task ShowWindow(bool force = true)
    {
      WindowModel windowModel = await WindowModelDao.GetModelByIdAsync(HabitWindow.Name);
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
      if (HabitWindow.Window != null)
      {
        HabitWindow.Window.Show();
        HabitWindow.Window.Activate();
        HabitWindow.Window.Reload();
        windowModel = (WindowModel) null;
      }
      else
      {
        if (windowModel == null || windowModel.Closed)
          IndependentWindow.AddShowEvent("habit");
        if (windowModel == null)
          windowModel = await IndependentWindow.GetNewWindowModel(HabitWindow.Name);
        else
          await WindowModelDao.OpenWindow(windowModel);
        HabitWindow.Window = new HabitWindow(windowModel);
        HabitWindow.Window.Show();
        windowModel = (WindowModel) null;
      }
    }

    private void Reload() => this._habit.LoadHabits();

    protected override void OnClosing()
    {
      this.Container.Children.Clear();
      HabitWindow.Window = (HabitWindow) null;
    }

    public static void OpenOrCloseWindow(System.Windows.Window window)
    {
      if (HabitWindow.Window != null)
      {
        HabitWindow.Window.Close();
      }
      else
      {
        if (!IndependentWindow.CheckCount(window))
          return;
        HabitWindow.ShowWindow();
      }
    }
  }
}
