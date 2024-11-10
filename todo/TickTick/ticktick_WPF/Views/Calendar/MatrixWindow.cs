// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.MatrixWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Eisenhower;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class MatrixWindow : IndependentWindow
  {
    public static MatrixWindow Window;
    public static string Name = DisplayModule.Matrix.ToString();
    private MatrixContainer _matrix;

    public static bool IsShowing => MatrixWindow.Window != null;

    private MatrixWindow(WindowModel windowModel)
      : base(windowModel)
    {
      this.MinWidth = 580.0;
      this.MinHeight = 462.0;
      this.Id = MatrixWindow.Name;
      this._matrix = new MatrixContainer();
      this.Container.SetResourceReference(Control.BackgroundProperty, (object) "ShowAreaBackground");
      this.Container.Children.Add((UIElement) this._matrix);
      this.Title = Utils.GetString("Matrix");
    }

    public static async Task ShowWindow(bool force = true)
    {
      WindowModel windowModel = await WindowModelDao.GetModelByIdAsync(MatrixWindow.Name);
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
      if (MatrixWindow.Window != null)
      {
        MatrixWindow.Window.Show();
        MatrixWindow.Window.Activate();
      }
      else
      {
        if (windowModel == null || windowModel.Closed)
          IndependentWindow.AddShowEvent("matrix");
        if (windowModel == null)
          windowModel = await IndependentWindow.GetNewWindowModel(MatrixWindow.Name);
        else
          await WindowModelDao.OpenWindow(windowModel);
        MatrixWindow.Window = new MatrixWindow(windowModel);
        MatrixWindow.Window.Show();
      }
      MatrixWindow.Window.LoadTask();
      windowModel = (WindowModel) null;
    }

    private void LoadTask() => this._matrix.LoadTask(false);

    protected override void OnClosing()
    {
      this.Container.Children.Clear();
      MatrixWindow.Window = (MatrixWindow) null;
    }

    public override void BatchSetPriorityCommand(int priority)
    {
      this._matrix?.TryBatchSetPriority(priority);
    }

    public override void BatchSetDateCommand(DateTime? date) => this._matrix?.TryBatchSetDate(date);

    public override void BatchPinTaskCommand() => this._matrix?.BatchPinTask();

    public override void BatchOpenStickyCommand() => this._matrix?.BatchOpenSticky();

    public override void BatchDeleteCommand() => this._matrix?.BatchDeleteTask();

    public static void OpenOrCloseWindow(System.Windows.Window window)
    {
      if (MatrixWindow.Window != null)
      {
        MatrixWindow.Window.Close();
      }
      else
      {
        if (!IndependentWindow.CheckCount(window))
          return;
        MatrixWindow.ShowWindow();
      }
    }
  }
}
