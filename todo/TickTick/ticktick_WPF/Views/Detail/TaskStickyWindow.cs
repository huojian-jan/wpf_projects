// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskStickyWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Undo;
using ticktick_WPF.Views.Widget;
using TickTickDao;
using TickTickModels;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskStickyWindow : MyWindow, IToastShowWindow, IComponentConnector
  {
    private const double AdsorbWidth = 10.0;
    private const double UnAdsorbWidth = 10.0;
    private const double WMin = 224.0;
    private const double HMin = 212.0;
    private const double DefaultW = 254.0;
    private const double DefaultH = 228.0;
    private static ConcurrentDictionary<string, TaskStickyWindow.WindowRect> _windowRects = new ConcurrentDictionary<string, TaskStickyWindow.WindowRect>();
    private static BlockingList<TaskStickyWindow.WindowRect> _screenRects = new BlockingList<TaskStickyWindow.WindowRect>();
    private double _adsorbLeft = double.NegativeInfinity;
    private System.Windows.Point _sizeChangeStartPoint;
    private TaskStickyWindow.DragSizeMode _sizeMode;
    private static bool _aligning;
    private static ConcurrentDictionary<string, TaskStickyWindow> _openedStickies = new ConcurrentDictionary<string, TaskStickyWindow>();
    public readonly TaskStickyViewModel ViewModel;
    private TaskType _taskType;
    private bool _editing;
    private DateTime _headerClickOldTime;
    private double _originHeight;
    private bool _collapsed;
    private bool _dragMouseDown;
    private bool _storyPlaying;
    private string _color;
    private static bool _locking;
    private bool _checkEmpty;
    private static SettingDialog _settingDialog;
    private System.Windows.Point _mouseDownPoint;
    private List<(string, KeyBinding)> _keyBindings = new List<(string, KeyBinding)>();
    private static readonly Dictionary<StickyColorKey, string> BorderColorDict = new Dictionary<StickyColorKey, string>()
    {
      {
        StickyColorKey.Dark,
        "#323232"
      },
      {
        StickyColorKey.DarkBlue,
        "#203972"
      }
    };
    private static readonly Dictionary<StickyColorKey, string> PopupBackColorDict = new Dictionary<StickyColorKey, string>()
    {
      {
        StickyColorKey.Dark,
        "#2F323B"
      },
      {
        StickyColorKey.DarkBlue,
        "#1E2E52"
      }
    };
    internal TaskStickyWindow Root;
    internal Border BackBorder;
    internal Grid Container;
    internal RowDefinition FirstRow;
    internal RowDefinition SecondRow;
    internal Border BottomBackground;
    internal Border TopBackground;
    internal Border TitleGrid;
    internal DetailTextBox TopTitleText;
    internal StackPanel TopGrid;
    internal Border ThemeBorder;
    internal System.Windows.Controls.ToolTip ColorTooltip;
    internal Border PinGrid;
    internal System.Windows.Controls.ToolTip PinTooltip;
    internal Path PinPath;
    internal Border CloseGrid;
    internal TaskDetailStickyView DetailControl;
    internal Grid BottomGrid;
    internal Border DateBtn;
    internal StackPanel DatePanel;
    internal Path MoreIcon;
    internal EscPopup MorePopup;
    internal Grid ToastGrid;
    internal Border SizeChangeBorder;
    private bool _contentLoaded;

    private static double StickySpacing
    {
      get => (double) LocalSettings.Settings.ExtraSettings.StickySpacing;
    }

    private static bool ResetOnAlign => LocalSettings.Settings.ExtraSettings.ResetStickyWhenAlign;

    private static void RemoveGeoById(string id)
    {
      TaskStickyWindow._windowRects.TryRemove(id, out TaskStickyWindow.WindowRect _);
    }

    private static void AddOrUpdateWindowLocation(TaskStickyModel stickyModel)
    {
      TaskStickyWindow._windowRects[stickyModel.TaskId] = new TaskStickyWindow.WindowRect(stickyModel.Left + 8.0, stickyModel.Top + 8.0, stickyModel.Width - 16.0, stickyModel.Height - 16.0);
    }

    private void MovePosition(System.Windows.Point origin, System.Windows.Point target)
    {
      this.RemoveAnimation();
      TaskStickyWindow.WindowRect windowRect1 = this.GetWindowRect();
      bool adsorb1 = this.IsAdsorb(windowRect1, true);
      bool adsorb2 = this.IsAdsorb(windowRect1, false);
      double diff1 = target.X - origin.X;
      double diff2 = target.Y - origin.Y;
      double? targetValue1 = GetTargetValue(adsorb1, diff1, windowRect1, true);
      if (targetValue1.HasValue)
        this.Left = targetValue1.Value;
      TaskStickyWindow.WindowRect windowRect2 = this.GetWindowRect();
      double? targetValue2 = GetTargetValue(adsorb2, diff2, windowRect2, false);
      if (!targetValue2.HasValue)
        return;
      this.Top = targetValue2.Value;

      double? GetTargetValue(
        bool adsorb,
        double diff,
        TaskStickyWindow.WindowRect windowRect,
        bool isHorizon)
      {
        if (adsorb && Math.Abs(diff) <= 10.0)
          return new double?();
        windowRect.SetSide1(isHorizon, windowRect.GetSide1(isHorizon) + diff);
        foreach (KeyValuePair<string, TaskStickyWindow.WindowRect> windowRect1 in TaskStickyWindow._windowRects)
        {
          TaskStickyWindow.WindowRect rect = windowRect1.Value;
          if (!(windowRect1.Key == this.ViewModel.Id))
          {
            if (Math.Abs(windowRect.GetSide1(isHorizon) - rect.GetSide1(isHorizon)) < 10.0)
            {
              windowRect.SetSide1(isHorizon, rect.GetSide1(isHorizon));
              break;
            }
            if (Math.Abs(windowRect.GetSide1(isHorizon) - rect.GetSide2(isHorizon)) < 10.0)
            {
              windowRect.SetSide1(isHorizon, rect.GetSide2(isHorizon));
              break;
            }
            if (Math.Abs(windowRect.GetSide2(isHorizon) - rect.GetSide1(isHorizon)) < 10.0)
            {
              windowRect.SetSide2(isHorizon, rect.GetSide1(isHorizon));
              break;
            }
            if (Math.Abs(windowRect.GetSide2(isHorizon) - rect.GetSide2(isHorizon)) < 10.0)
            {
              windowRect.SetSide2(isHorizon, rect.GetSide2(isHorizon));
              break;
            }
            if (windowRect.IsIntersect(rect, !isHorizon))
            {
              if (Math.Abs(windowRect.GetSide1(isHorizon) - rect.GetSide2(isHorizon) - TaskStickyWindow.StickySpacing) < 10.0)
              {
                windowRect.SetSide1(isHorizon, rect.GetSide2(isHorizon) + TaskStickyWindow.StickySpacing);
                break;
              }
              if (Math.Abs(rect.GetSide1(isHorizon) - windowRect.GetSide2(isHorizon) - TaskStickyWindow.StickySpacing) < 10.0)
              {
                windowRect.SetSide2(isHorizon, rect.GetSide1(isHorizon) - TaskStickyWindow.StickySpacing);
                break;
              }
            }
          }
        }
        return new double?(windowRect.GetSide1(isHorizon) - 8.0);
      }
    }

    private TaskStickyWindow.WindowRect GetWindowRect()
    {
      return new TaskStickyWindow.WindowRect(this.Left + 8.0, this.Top + 8.0, this.ActualWidth - 16.0, this.ActualHeight - 16.0);
    }

    private bool IsAdsorb(TaskStickyWindow.WindowRect windowRect, bool isHorizon)
    {
      foreach (KeyValuePair<string, TaskStickyWindow.WindowRect> windowRect1 in TaskStickyWindow._windowRects)
      {
        if (!(windowRect1.Key == this.ViewModel.Id))
        {
          TaskStickyWindow.WindowRect rect = windowRect1.Value;
          if (Math.Abs(windowRect.GetSide1(isHorizon) - rect.GetSide1(isHorizon)) < 1.0 || Math.Abs(windowRect.GetSide1(isHorizon) - rect.GetSide2(isHorizon)) < 1.0 || Math.Abs(windowRect.GetSide2(isHorizon) - rect.GetSide1(isHorizon)) < 1.0 || Math.Abs(windowRect.GetSide2(isHorizon) - rect.GetSide2(isHorizon)) < 1.0 || (Math.Abs(windowRect.GetSide1(isHorizon) - rect.GetSide2(isHorizon) - TaskStickyWindow.StickySpacing) < 1.0 || Math.Abs(rect.GetSide1(isHorizon) - windowRect.GetSide2(isHorizon) - TaskStickyWindow.StickySpacing) < 1.0) && windowRect.IsIntersect(rect, !isHorizon))
            return true;
        }
      }
      return false;
    }

    private void OnBorderMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Released)
        return;
      this._sizeChangeStartPoint = new System.Windows.Point();
      int pointMode = (int) this.GetPointMode(e.GetPosition((IInputElement) this.SizeChangeBorder));
    }

    private void OnSizeChangeMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._sizeChangeStartPoint = e.GetPosition((IInputElement) this.SizeChangeBorder);
      this._sizeMode = this.GetPointMode(this._sizeChangeStartPoint);
      if ((this._sizeMode & TaskStickyWindow.DragSizeMode.Right) == TaskStickyWindow.DragSizeMode.Right)
        this._sizeChangeStartPoint.X -= this.ActualWidth;
      if ((this._sizeMode & TaskStickyWindow.DragSizeMode.Bottom) == TaskStickyWindow.DragSizeMode.Bottom)
        this._sizeChangeStartPoint.Y -= this.ActualHeight;
      this.SizeChangeBorder.CaptureMouse();
      this.SizeChangeBorder.MouseMove += new System.Windows.Input.MouseEventHandler(this.ChangeSize);
      this.SizeChangeBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.ReleaseSizeChange);
    }

    private void ReleaseSizeChange(object sender, MouseButtonEventArgs e)
    {
      this.SizeChangeBorder.MouseMove -= new System.Windows.Input.MouseEventHandler(this.ChangeSize);
      this.SizeChangeBorder.MouseLeftButtonUp -= new MouseButtonEventHandler(this.ReleaseSizeChange);
      this.SizeChangeBorder.ReleaseMouseCapture();
    }

    private void ChangeSize(object sender, System.Windows.Input.MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this.SizeChangeBorder);
      bool sideDrag1 = (this._sizeMode & TaskStickyWindow.DragSizeMode.Left) == TaskStickyWindow.DragSizeMode.Left;
      bool flag1 = (this._sizeMode & TaskStickyWindow.DragSizeMode.Right) == TaskStickyWindow.DragSizeMode.Right;
      bool sideDrag2 = (this._sizeMode & TaskStickyWindow.DragSizeMode.Top) == TaskStickyWindow.DragSizeMode.Top;
      bool flag2 = (this._sizeMode & TaskStickyWindow.DragSizeMode.Bottom) == TaskStickyWindow.DragSizeMode.Bottom;
      if (flag1)
        position.X -= this.ActualWidth;
      if (flag2)
        position.Y -= this.ActualHeight;
      this.RemoveAnimation();
      if (sideDrag1 | flag1)
      {
        double diff = position.X - this._sizeChangeStartPoint.X;
        double num = this.Left + this.ActualWidth;
        if (this.ActualWidth + (sideDrag1 ? -1.0 : 1.0) * diff <= this.MinWidth)
        {
          if (this.Width > this.MinWidth)
          {
            this.Left = sideDrag1 ? num - this.MinWidth : this.Left;
            this.Width = this.MinWidth;
          }
        }
        else
        {
          TaskStickyWindow.WindowRect targetValue = GetTargetValue(this.GetWindowRect(), true, sideDrag1, diff);
          this.Left = targetValue.Left - 8.0;
          this.Width = targetValue.Width + 16.0;
        }
      }
      if (!(sideDrag2 | flag2))
        return;
      double diff1 = position.Y - this._sizeChangeStartPoint.Y;
      double num1 = this.Top + this.ActualHeight;
      if (this.ActualHeight + (sideDrag2 ? -1.0 : 1.0) * diff1 <= this.MinHeight)
      {
        if (this.Height <= this.MinHeight)
          return;
        this.Top = sideDrag2 ? num1 - this.MinHeight : this.Top;
        this.Height = this.MinHeight;
      }
      else
      {
        TaskStickyWindow.WindowRect targetValue = GetTargetValue(this.GetWindowRect(), false, sideDrag2, diff1);
        this.Top = targetValue.Top - 8.0;
        this.Height = targetValue.Height + 16.0;
      }

      TaskStickyWindow.WindowRect GetTargetValue(
        TaskStickyWindow.WindowRect windowRect,
        bool isHorizon,
        bool sideDrag,
        double diff)
      {
        if (!this.IsAdsorb(windowRect, isHorizon) || Math.Abs(diff) > 10.0)
        {
          if (sideDrag)
          {
            windowRect.SetSide1(isHorizon, windowRect.GetSide1(isHorizon) + diff);
            windowRect.SetLength(isHorizon, windowRect.GetLength(isHorizon) - diff);
          }
          else
            windowRect.SetLength(isHorizon, windowRect.GetLength(isHorizon) + diff);
          double val = windowRect.GetSide1(isHorizon);
          double num = windowRect.GetSide2(isHorizon);
          foreach (KeyValuePair<string, TaskStickyWindow.WindowRect> windowRect1 in TaskStickyWindow._windowRects)
          {
            TaskStickyWindow.WindowRect rect = windowRect1.Value;
            if (!(windowRect1.Key == this.ViewModel.Id))
            {
              if (sideDrag && Math.Abs(val - rect.GetSide1(isHorizon)) < 10.0)
              {
                val = rect.GetSide1(isHorizon);
                break;
              }
              if (sideDrag && Math.Abs(val - rect.GetSide2(isHorizon)) < 10.0)
                val = rect.GetSide2(isHorizon);
              if (!sideDrag && Math.Abs(num - rect.GetSide1(isHorizon)) < 10.0)
              {
                num = rect.GetSide1(isHorizon);
                break;
              }
              if (!sideDrag && Math.Abs(num - rect.GetSide2(isHorizon)) < 10.0)
              {
                num = rect.GetSide2(isHorizon);
                break;
              }
              if (windowRect.IsIntersect(rect, isHorizon))
              {
                if (sideDrag && Math.Abs(val - rect.GetSide2(isHorizon) - TaskStickyWindow.StickySpacing) < 10.0)
                {
                  val = rect.GetSide2(isHorizon) + TaskStickyWindow.StickySpacing;
                  break;
                }
                if (!sideDrag && Math.Abs(rect.GetSide1(isHorizon) - num - TaskStickyWindow.StickySpacing) < 10.0)
                {
                  num = rect.GetSide1(isHorizon) - TaskStickyWindow.StickySpacing;
                  break;
                }
              }
            }
          }
          windowRect.SetSide1(isHorizon, val);
          windowRect.SetLength(isHorizon, num - val);
        }
        return windowRect;
      }
    }

    private TaskStickyWindow.DragSizeMode GetPointMode(System.Windows.Point point)
    {
      bool flag1 = point.Y <= 8.0;
      bool flag2 = point.X <= 8.0;
      bool flag3 = point.Y >= this.SizeChangeBorder.ActualHeight - 8.0;
      bool flag4 = point.X >= this.SizeChangeBorder.ActualWidth - 8.0;
      if (flag1 & flag2 || flag3 & flag4)
      {
        this.SizeChangeBorder.Cursor = System.Windows.Input.Cursors.SizeNWSE;
        return !flag1 ? TaskStickyWindow.DragSizeMode.Bottom | TaskStickyWindow.DragSizeMode.Right : TaskStickyWindow.DragSizeMode.Top | TaskStickyWindow.DragSizeMode.Left;
      }
      if (flag1 & flag4 || flag3 & flag2)
      {
        this.SizeChangeBorder.Cursor = System.Windows.Input.Cursors.SizeNESW;
        return !flag1 ? TaskStickyWindow.DragSizeMode.Bottom | TaskStickyWindow.DragSizeMode.Left : TaskStickyWindow.DragSizeMode.Top | TaskStickyWindow.DragSizeMode.Right;
      }
      if (flag1 | flag3)
      {
        this.SizeChangeBorder.Cursor = System.Windows.Input.Cursors.SizeNS;
        return !flag1 ? TaskStickyWindow.DragSizeMode.Bottom : TaskStickyWindow.DragSizeMode.Top;
      }
      if (!(flag4 | flag2))
        return TaskStickyWindow.DragSizeMode.None;
      this.SizeChangeBorder.Cursor = System.Windows.Input.Cursors.SizeWE;
      return !flag2 ? TaskStickyWindow.DragSizeMode.Right : TaskStickyWindow.DragSizeMode.Left;
    }

    public void Align(bool isVertical, bool isRight)
    {
      UserActCollectUtils.AddClickEvent("sticky_note", "arrange_by", isVertical ? (isRight ? "right" : "left") : "top");
      System.Drawing.Rectangle monitorFromWindow = WindowSizing.GetMonitorFromWindow((Window) this);
      if (monitorFromWindow == new System.Drawing.Rectangle())
        return;
      double ratio = PresentationSource.FromVisual((Visual) this)?.CompositionTarget?.TransformFromDevice.M22 ?? 1.0 / ScreenPositionUtils.GetScalingRatio();
      TaskStickyWindow.AlignWindows(isVertical, isRight, monitorFromWindow, ratio);
      this.Activate();
    }

    private static void AlignWindows(
      bool isVertical,
      bool isRight,
      System.Drawing.Rectangle screen,
      double ratio)
    {
      if (TaskStickyWindow._aligning)
        return;
      TaskStickyWindow._aligning = true;
      List<TaskStickyWindow.StickySortModel> list = TaskStickyWindow._windowRects.Select<KeyValuePair<string, TaskStickyWindow.WindowRect>, TaskStickyWindow.StickySortModel>((Func<KeyValuePair<string, TaskStickyWindow.WindowRect>, TaskStickyWindow.StickySortModel>) (kv => new TaskStickyWindow.StickySortModel()
      {
        Id = kv.Key,
        Rect = new TaskStickyWindow.WindowRect(kv.Value.Left, kv.Value.Top, TaskStickyWindow.ResetOnAlign ? 238.0 : kv.Value.Width, !TaskStickyWindow.ResetOnAlign || kv.Value.Height <= 100.0 ? kv.Value.Height : 212.0)
      })).ToList<TaskStickyWindow.StickySortModel>();
      List<TaskStickyWindow.StickySortModel> stickySortModels = new List<TaskStickyWindow.StickySortModel>();
      foreach (TaskStickyWindow.StickySortModel stickySortModel in list)
      {
        if (TaskStickyWindow._openedStickies.ContainsKey(stickySortModel.Id) && WindowSizing.GetMonitorFromWindow((Window) TaskStickyWindow._openedStickies[stickySortModel.Id]) == screen)
          stickySortModels.Add(stickySortModel);
      }
      screen = new System.Drawing.Rectangle((int) ((double) screen.Left * ratio), (int) ((double) screen.Top * ratio), (int) ((double) screen.Width * ratio), (int) ((double) screen.Height * ratio));
      TaskStickyWindow.SortInScreen(isRight | isVertical, isRight, screen, stickySortModels);
      TaskStickyWindow._aligning = false;
    }

    private static void SortInScreen(
      bool isVertical,
      bool isRight,
      System.Drawing.Rectangle screenRect,
      List<TaskStickyWindow.StickySortModel> stickySortModels)
    {
      double num1 = (isVertical ? 224.0 : 212.0) + TaskStickyWindow.StickySpacing;
      int num2 = isVertical ? screenRect.Height : screenRect.Width;
      int screenSide = isVertical ? screenRect.Top : screenRect.Left;
      int screenOtherSide = isVertical ? screenRect.Bottom : screenRect.Right;
      double num3 = isVertical ? 228.0 : 254.0;
      double num4 = Math.Max(20.0, TaskStickyWindow.StickySpacing);
      double sideMargin = isVertical ? num4 : num4 + ((double) num2 - 2.0 * num4) % (num3 - 16.0 + TaskStickyWindow.StickySpacing) / 2.0;
      int num5 = isRight ? -1 : 1;
      System.Windows.Point point1 = new System.Windows.Point((isRight ? (double) screenRect.Right : (double) screenRect.Left) + (double) num5 * sideMargin, (double) screenRect.Top + num4);
      int count = stickySortModels.Count;
      List<TaskStickyWindow.StickySortModel> sorted = new List<TaskStickyWindow.StickySortModel>();
      bool flag1 = false;
      while (sorted.Count < count)
      {
        if (!flag1)
        {
          double maxLength = (double) screenOtherSide - sideMargin - (isVertical ? point1.Y : point1.X) + 20.0;
          if (maxLength < 60.0)
          {
            flag1 = true;
            continue;
          }
          foreach (TaskStickyWindow.StickySortModel stickySortModel in stickySortModels)
            stickySortModel.Direction = Math.Sqrt(Math.Pow(stickySortModel.Rect.GetLOrR(isRight) - point1.X, 2.0) + Math.Pow(stickySortModel.Rect.Top - point1.Y, 2.0));
          stickySortModels.Sort((Comparison<TaskStickyWindow.StickySortModel>) ((a, b) => a.Direction.CompareTo(b.Direction)));
          TaskStickyWindow.StickySortModel stickySortModel1 = stickySortModels.FirstOrDefault<TaskStickyWindow.StickySortModel>((Func<TaskStickyWindow.StickySortModel, bool>) (m => (isVertical ? m.Rect.Height : m.Rect.Width) <= maxLength));
          if (stickySortModel1 == null)
          {
            flag1 = true;
          }
          else
          {
            stickySortModels.Remove(stickySortModel1);
            if (isRight)
              stickySortModel1.Rect.SetRight(point1.X);
            else
              stickySortModel1.Rect.Left = point1.X;
            stickySortModel1.Rect.Top = point1.Y;
            if (isVertical)
              point1.Y = point1.Y + stickySortModel1.Rect.Height + TaskStickyWindow.StickySpacing;
            else
              point1.X = point1.X + stickySortModel1.Rect.Width + TaskStickyWindow.StickySpacing;
            sorted.Add(stickySortModel1);
            continue;
          }
        }
        double num6 = isVertical ? (isRight ? (double) screenRect.Right : (double) screenRect.Left) : (double) screenRect.Top;
        sorted.Sort((Comparison<TaskStickyWindow.StickySortModel>) ((a, b) => a.Rect.GetSide1(!isVertical).CompareTo(b.Rect.GetSide1(!isVertical))));
        bool flag2 = false;
        int max = isVertical ? (isRight ? screenRect.Left : screenRect.Right) : screenRect.Bottom;
        do
        {
          TaskStickyWindow.StickySortModel minBottomModel = GetMinBottomModel(num6, (double) max);
          if (minBottomModel != null)
          {
            num6 = isRight ? minBottomModel.Rect.Left : minBottomModel.Rect.GetSide2(isVertical);
            if (Math.Abs((double) max - num6) >= num1)
            {
              foreach ((double, double) intersectLine in GetIntersectLines(num6))
              {
                foreach (TaskStickyWindow.StickySortModel stickySortModel in stickySortModels)
                {
                  double num7 = isVertical ? num6 : intersectLine.Item1;
                  double num8 = isVertical ? intersectLine.Item1 : num6;
                  stickySortModel.Direction = Math.Sqrt(Math.Pow(stickySortModel.Rect.GetLOrR(isRight) - num7, 2.0) + Math.Pow(stickySortModel.Rect.Top - num8, 2.0));
                }
                stickySortModels.Sort((Comparison<TaskStickyWindow.StickySortModel>) ((a, b) => a.Direction.CompareTo(b.Direction)));
                foreach (TaskStickyWindow.StickySortModel stickySortModel2 in stickySortModels)
                {
                  double num9 = intersectLine.Item1;
                  for (double num10 = intersectLine.Item2 - num9; stickySortModel2.Rect.GetLength(!isVertical) <= num10 - TaskStickyWindow.StickySpacing + 2.0; num10 = intersectLine.Item2 - num9)
                  {
                    TaskStickyWindow.WindowRect rect = new TaskStickyWindow.WindowRect(isVertical ? minBottomModel.Rect.Right + TaskStickyWindow.StickySpacing : num9, isVertical ? num9 : minBottomModel.Rect.Bottom + TaskStickyWindow.StickySpacing, stickySortModel2.Rect.Width, stickySortModel2.Rect.Height);
                    if (isRight)
                      rect.SetRight(minBottomModel.Rect.Left - TaskStickyWindow.StickySpacing);
                    TaskStickyWindow.StickySortModel stickySortModel3 = sorted.FirstOrDefault<TaskStickyWindow.StickySortModel>((Func<TaskStickyWindow.StickySortModel, bool>) (s => rect.IntersectWith(s.Rect)));
                    if (stickySortModel3 != null)
                    {
                      num9 = stickySortModel3.Rect.GetSide2(!isVertical) + TaskStickyWindow.StickySpacing;
                    }
                    else
                    {
                      stickySortModels.Remove(stickySortModel2);
                      stickySortModel2.Rect.Left = rect.Left;
                      stickySortModel2.Rect.Top = rect.Top;
                      sorted.Add(stickySortModel2);
                      flag2 = true;
                      break;
                    }
                  }
                  if (flag2)
                    break;
                }
                if (flag2)
                  break;
              }
            }
            else
              break;
          }
          else
            break;
        }
        while (!flag2);
        if (!flag2)
        {
          double num11 = 10000.0;
          TaskStickyWindow.StickySortModel stickySortModel4 = sorted.FirstOrDefault<TaskStickyWindow.StickySortModel>();
          System.Windows.Point point2 = isRight ? new System.Windows.Point((double) screenRect.Left, (double) screenRect.Bottom) : new System.Windows.Point((double) screenRect.Right, (double) screenRect.Bottom);
          foreach (TaskStickyWindow.StickySortModel stickySortModel5 in sorted)
          {
            double num12 = Math.Sqrt(Math.Pow(stickySortModel5.Rect.GetLOrR(!isRight) - point2.X, 2.0) + Math.Pow(stickySortModel5.Rect.Top - point2.Y, 2.0));
            if (num12 < num11)
            {
              stickySortModel4 = stickySortModel5;
              num11 = num12;
            }
          }
          double num13 = stickySortModel4 != null ? stickySortModel4.Rect.Top : (double) screenRect.Top + num4;
          double num14 = stickySortModel4 != null ? stickySortModel4.Rect.Left : (double) screenRect.Left + num4;
          double right = stickySortModel4 != null ? stickySortModel4.Rect.Right : (double) screenRect.Right + num4;
          foreach (TaskStickyWindow.StickySortModel stickySortModel6 in stickySortModels)
          {
            num13 += 12.0;
            if (num13 > (double) screenRect.Bottom - 212.0)
              num13 = (double) screenRect.Bottom - 212.0;
            stickySortModel6.Rect.Top = num13;
            if (isRight)
              stickySortModel6.Rect.SetRight(right);
            else
              stickySortModel6.Rect.Left = num14;
            sorted.Add(stickySortModel6);
          }
        }
      }
      foreach (TaskStickyWindow.StickySortModel stickySortModel in sorted)
      {
        TaskStickyWindow taskStickyWindow;
        if (TaskStickyWindow._openedStickies.TryGetValue(stickySortModel.Id, out taskStickyWindow))
          taskStickyWindow.Activate();
      }
      foreach (TaskStickyWindow.StickySortModel stickySortModel in sorted)
      {
        TaskStickyWindow taskStickyWindow;
        if (TaskStickyWindow._openedStickies.TryGetValue(stickySortModel.Id, out taskStickyWindow))
          taskStickyWindow.AnimSetPosition(stickySortModel.Rect.Left - 8.0, stickySortModel.Rect.Top - 8.0);
      }

      TaskStickyWindow.StickySortModel GetMinBottomModel(double min, double max)
      {
        TaskStickyWindow.StickySortModel minBottomModel = (TaskStickyWindow.StickySortModel) null;
        if (isRight)
        {
          min *= -1.0;
          max *= -1.0;
        }
        for (int index = sorted.Count - 1; index >= 0; --index)
        {
          TaskStickyWindow.StickySortModel stickySortModel = sorted[index];
          double num = isRight ? -1.0 * stickySortModel.Rect.Left : stickySortModel.Rect.GetSide2(isVertical);
          if (num > min && num < max)
          {
            minBottomModel = stickySortModel;
            max = num;
          }
        }
        return minBottomModel;
      }

      List<(double, double)> GetIntersectLines(double offset)
      {
        List<TaskStickyWindow.WindowRect> windowRectList = new List<TaskStickyWindow.WindowRect>();
        foreach (TaskStickyWindow.StickySortModel stickySortModel in sorted)
        {
          if (isRight)
          {
            if (MathUtil.Between(offset, stickySortModel.Rect.Right + TaskStickyWindow.StickySpacing, stickySortModel.Rect.Left + 4.0))
              windowRectList.Add(stickySortModel.Rect);
          }
          else if (MathUtil.Between(offset, stickySortModel.Rect.GetSide1(isVertical) - TaskStickyWindow.StickySpacing, stickySortModel.Rect.GetSide2(isVertical) - 4.0))
            windowRectList.Add(stickySortModel.Rect);
        }
        windowRectList.Sort((Comparison<TaskStickyWindow.WindowRect>) ((a, b) => a.GetSide1(!isVertical).CompareTo(b.GetSide1(!isVertical))));
        List<(double, double)> intersectLines = new List<(double, double)>();
        double num = (double) screenSide + sideMargin;
        foreach (TaskStickyWindow.WindowRect windowRect in windowRectList)
        {
          if (windowRect.GetSide1(!isVertical) - num >= 60.0)
            intersectLines.Add((num, windowRect.GetSide1(!isVertical)));
          num = windowRect.GetSide2(!isVertical) + TaskStickyWindow.StickySpacing;
        }
        if ((double) screenOtherSide - num >= 60.0)
          intersectLines.Add((num, (double) screenOtherSide));
        return intersectLines;
      }
    }

    public TaskStickyWindow(TaskStickyModel stickyModel)
    {
      this.InitializeComponent();
      this.SourceInitialized += new EventHandler(this.OnWindowSourceInitialized);
      this.ViewModel = new TaskStickyViewModel(stickyModel.TaskId);
      this.InitWindow();
      this.SetStickyColor(stickyModel.Color, false);
      this.DataContext = (object) this.ViewModel;
      this.ViewModel.Window = this;
      this.InitWindowSetting(stickyModel);
      this.Loaded += (RoutedEventHandler) (async (o, e) =>
      {
        TaskStickyWindow taskStickyWindow = this;
        taskStickyWindow.SetOpacity();
        taskStickyWindow.LocationChanged += new EventHandler(taskStickyWindow.OnLocationChanged);
        taskStickyWindow.SizeChanged += new SizeChangedEventHandler(taskStickyWindow.OnSizeChanged);
        taskStickyWindow.ResetPosition();
        if (taskStickyWindow.ViewModel.IsEmptyTask())
          taskStickyWindow.DetailControl.TryFocusTitle();
        taskStickyWindow.UpdateLocationCache();
        await Task.Delay(100);
        taskStickyWindow.Activate();
      });
      this.BindEvent();
      this.InitShortCut();
      this.SetDatePanelOpacity();
      this.ShowInTaskbar = !LocalSettings.Settings.ExtraSettings.StickyHideInTaskBar;
      this.StateChanged += new EventHandler(this.OnStateChanged);
    }

    private void InitWindow()
    {
      this.DetailControl.SetTitleAndContent(this.ViewModel.SourceModel);
      this.DetailControl.SetStickyMode();
      this.DetailControl.Navigate(this.ViewModel.Id);
      this.TopTitleText.SetText(this.ViewModel.Title);
      this.TopTitleText.SetBaseColor("StickyTitleTextColor");
      this.ColorTooltip.Content = (object) (Utils.GetString("Color") + (string.IsNullOrEmpty(LocalSettings.Settings.ShortCutModel.StickyColor) ? "" : "  " + LocalSettings.Settings.ShortCutModel.StickyColor));
    }

    private void SetStickyColor(string color, bool collect = true)
    {
      StickyColorKey result;
      if (!Enum.TryParse<StickyColorKey>(color, true, out result))
        color = StickyColorKey.Default.ToString();
      this._color = color;
      bool isDark = color.ToLower().StartsWith("dark");
      ThemeUtil.SetTheme(isDark ? "dark" : "light", (FrameworkElement) this);
      this.SetStickyColor(result);
      this.DetailControl.SetTheme(isDark);
      this.TopTitleText.SetLightTheme();
      if (!collect)
        return;
      this.CollectColorSelect(result);
    }

    private void CollectColorSelect(StickyColorKey colorKey)
    {
      string key;
      switch (colorKey)
      {
        case StickyColorKey.Apricot:
          key = "pink";
          break;
        case StickyColorKey.Watermelon:
          key = "watermelon_red";
          break;
        case StickyColorKey.LakeBlue:
          key = "light_blue";
          break;
        case StickyColorKey.Blue:
          key = "blue";
          break;
        case StickyColorKey.Purple:
          key = "purple";
          break;
        case StickyColorKey.BlueGreen:
          key = "green";
          break;
        case StickyColorKey.Gray:
          key = "grey";
          break;
        default:
          key = "yellow";
          break;
      }
      DelayActionHandlerCenter.TryDoAction("StickyColor" + this.ViewModel.Id, (EventHandler) ((o, e) => UserActCollectUtils.AddClickEvent("sticky_note", "color", key)), 60000);
    }

    private void OnWindowSourceInitialized(object sender, EventArgs e)
    {
      if (!this.Topmost)
        return;
      IntPtr handle = new WindowInteropHelper((Window) this).Handle;
      NativeUtils.SetWindowLong(handle, WinParameter.GWL_STYLE, NativeUtils.GetWindowLong(handle, -16) & -65537 & -131073);
    }

    private async void ResetPosition()
    {
      TaskStickyWindow taskStickyWindow = this;
      double left = taskStickyWindow.Left;
      double top = taskStickyWindow.Top;
      WindowHelper.MoveTo((Window) taskStickyWindow, (int) taskStickyWindow.Left, (int) taskStickyWindow.Top);
      taskStickyWindow.Left = left;
      taskStickyWindow.Top = top;
      Matrix? transform = PresentationSource.FromVisual((Visual) taskStickyWindow)?.CompositionTarget?.TransformFromDevice;
      await Task.Delay(1000);
      System.Windows.Point defaultPoint = new System.Windows.Point(SystemParameters.PrimaryScreenWidth / 2.0, SystemParameters.PrimaryScreenHeight / 2.0);
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      System.Windows.Point pomoLocationSafely = WidgetLocationHelper.GetPomoLocationSafely(taskStickyWindow.Left, taskStickyWindow.Top, __nonvirtual (taskStickyWindow.Width), __nonvirtual (taskStickyWindow.Height), transform, defaultPoint);
      taskStickyWindow.Left = pomoLocationSafely.X;
      taskStickyWindow.Top = pomoLocationSafely.Y;
    }

    private void InitWindowSetting(TaskStickyModel stickyModel)
    {
      this.Width = stickyModel.Width;
      this.Left = stickyModel.Left;
      this.Top = stickyModel.Top;
      this._originHeight = stickyModel.Height;
      if (stickyModel.Expand)
      {
        this.Height = stickyModel.Height;
        this.TopBackground.SetResourceReference(System.Windows.Controls.Control.BackgroundProperty, (object) "StickyTopColor");
      }
      else
      {
        this._collapsed = true;
        this.SetResourceReference(FrameworkElement.MinHeightProperty, (object) "StickyHeight42");
        this.SetResourceReference(FrameworkElement.HeightProperty, (object) "StickyHeight42");
        this.SetResourceReference(FrameworkElement.MaxHeightProperty, (object) "StickyHeight42");
        this.FirstRow.Height = new GridLength(22.0);
        this.SecondRow.Height = new GridLength(0.0);
        this.BottomGrid.Visibility = Visibility.Collapsed;
        this.SetTopTooltip();
        this.Container.VerticalAlignment = VerticalAlignment.Center;
        this.TitleGrid.Visibility = Visibility.Visible;
      }
      this.SetTopMost(stickyModel.TopMost);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => this.SaveLocation();

    private async void SaveLocation()
    {
      TaskStickyWindow taskStickyWindow = this;
      // ISSUE: reference to a compiler-generated method
      DelayActionHandlerCenter.TryDoAction("StickySizeChange" + taskStickyWindow.ViewModel.Id, new EventHandler(taskStickyWindow.\u003CSaveLocation\u003Eb__55_0));
    }

    private void OnLocationChanged(object sender, EventArgs e) => this.SaveLocation();

    private void BindEvent()
    {
      this.BindPropertyChangeEvent();
      this.DetailControl.NavigateTask += new EventHandler<ProjectTask>(this.OnNavigateTask);
      KeyBindingManager.ShortCutChanged += new EventHandler<string>(this.OnShortCutChanged);
      ticktick_WPF.Notifier.GlobalEventManager.StickyOpacityChanged += new EventHandler(this.OnOpacityChanged);
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnThemeChanged);
    }

    private void OnThemeChanged(object sender, EventArgs e)
    {
      this.SetStickyColor(this._color, false);
    }

    private async void OnNavigateTask(object sender, ProjectTask e)
    {
      if (await TaskDetailWindows.ShowTaskWindows(e.TaskId))
        return;
      this.TryToast(Utils.GetString("NoTaskFound"));
    }

    private void OnShortCutChanged(object sender, string e)
    {
      if (e == "StickyColor")
        this.ColorTooltip.Content = (object) (Utils.GetString("Color") + (string.IsNullOrEmpty(LocalSettings.Settings.ShortCutModel.StickyColor) ? "" : "  " + LocalSettings.Settings.ShortCutModel.StickyColor));
      if (!(e == "PinSticky"))
        return;
      this.PinTooltip.Content = (object) (Utils.GetString(this.Topmost ? "Unpin" : "Pin") + (string.IsNullOrEmpty(LocalSettings.Settings.ShortCutModel.PinSticky) ? "" : "  " + LocalSettings.Settings.ShortCutModel.PinSticky));
    }

    private void UnbindEvent()
    {
      this.DetailControl.NavigateTask -= new EventHandler<ProjectTask>(this.OnNavigateTask);
      ticktick_WPF.Notifier.GlobalEventManager.StickyOpacityChanged -= new EventHandler(this.OnOpacityChanged);
      KeyBindingManager.ShortCutChanged -= new EventHandler<string>(this.OnShortCutChanged);
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnThemeChanged);
    }

    private void OnToastString(object sender, string e) => this.TryToastString((object) this, e);

    private void BindPropertyChangeEvent()
    {
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.ViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) =>
      {
        if (this.ViewModel.Enable)
          return;
        TaskStickyDao.CloseAsync(this.ViewModel.Id);
        this.Close();
      }), "Enable");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.ViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) =>
      {
        if (this.ViewModel.SourceModel.Deleted == 0)
          return;
        TaskStickyDao.CloseAsync(this.ViewModel.Id);
        this.Close();
      }), "Deleted");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.ViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) =>
      {
        if (string.IsNullOrEmpty(this.ViewModel.ToastString))
          return;
        this.TryToast(this.ViewModel.ToastString);
      }), "ToastString");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.ViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) =>
      {
        this.SetDatePanelOpacity();
        this.SetTopTooltip();
      }), "DateText");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) this.ViewModel, (EventHandler<PropertyChangedEventArgs>) ((o, e) => this.TopTitleText.SetText(this.ViewModel.Title)), "Title");
    }

    private void SetDatePanelOpacity()
    {
      if (Utils.IsEmptyDate(this.ViewModel.SourceModel.StartDate))
        this.DatePanel.SetBinding(UIElement.OpacityProperty, (BindingBase) new System.Windows.Data.Binding()
        {
          ElementName = "MoreIcon",
          Path = new PropertyPath((object) UIElement.OpacityProperty)
        });
      else
        this.DatePanel.Opacity = 1.0;
    }

    private void SetTopTooltip()
    {
      if (Utils.IsEmptyDate(this.ViewModel.SourceModel.StartDate) || !this._collapsed)
        this.TopBackground.ToolTip = (object) null;
      else
        this.TopBackground.SetBinding(FrameworkElement.ToolTipProperty, "DateText");
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (this._checkEmpty && this.ViewModel.IsEmptyTask())
        TaskService.DeleteTask(this.ViewModel.Id, 2);
      TaskStickyWindow.RemoveGeoById(this.ViewModel.SourceModel.Id);
      TaskStickyWindow._openedStickies.TryRemove(this.ViewModel.SourceModel.Id, out TaskStickyWindow _);
      this.RemoveKeyBinding();
      this.UnbindEvent();
      if (!WindowManager.AppLockOrExit)
        TaskStickyDao.CloseAsync(this.ViewModel.Id);
      base.OnClosing(e);
    }

    public static async Task<TaskStickyModel> GetTaskStickyModel(string taskId)
    {
      if (string.IsNullOrEmpty(taskId))
        return (TaskStickyModel) null;
      TaskStickyWindow taskStickyWindow;
      if (!TaskStickyWindow._openedStickies.TryGetValue(taskId, out taskStickyWindow))
      {
        TaskStickyModel sticky = await TaskStickyDao.GetModelByTaskIdAsync(taskId, LocalSettings.Settings.LoginUserId);
        if (sticky != null)
        {
          if (sticky.Closed)
          {
            sticky.Closed = false;
            if (sticky.LastCloseDate != 0L && (long) ticktick_WPF.Util.DateUtils.GetDateNum(DateTime.Now.AddDays(-7.0)) >= sticky.LastCloseDate)
            {
              sticky.ResetValue();
              sticky.TopMost = LocalSettings.Settings.ExtraSettings.StickyDefaultPin;
            }
            int num = await BaseDao<TaskStickyModel>.UpdateAsync(sticky);
          }
        }
        else
        {
          if (!TaskCache.ExistTask(taskId))
            return (TaskStickyModel) null;
          sticky = new TaskStickyModel(taskId, LocalSettings.Settings.LoginUserId)
          {
            Color = TaskStickyWindow.GetDefaultColor(),
            TopMost = LocalSettings.Settings.ExtraSettings.StickyDefaultPin
          };
          await TaskStickyDao.AddStickyAsync(sticky);
        }
        return sticky;
      }
      try
      {
        taskStickyWindow.WindowState = WindowState.Normal;
        taskStickyWindow.Activate();
      }
      catch
      {
      }
      return (TaskStickyModel) null;
    }

    private static string GetDefaultColor()
    {
      string stickyDefaultColor = LocalSettings.Settings.ExtraSettings.StickyDefaultColor;
      if (stickyDefaultColor == StickyColorKey.Random.ToString())
        stickyDefaultColor = ((StickyColorKey) new Random().Next(0, Enum.GetValues(typeof (StickyColorKey)).Cast<StickyColorKey>().Count<StickyColorKey>() - 2)).ToString();
      return stickyDefaultColor;
    }

    private void OnPinClick(object sender, MouseButtonEventArgs e)
    {
      this.Pin();
      UserActCollectUtils.AddClickEvent("sticky_note", "sticky_note_ui", this.Topmost ? "pin" : "unpin");
    }

    private void SetTopMost(bool topMost)
    {
      this.Topmost = topMost;
      this.PinPath.RenderTransform = (Transform) new RotateTransform(topMost ? 0.0 : 45.0);
      this.PinTooltip.Content = (object) (Utils.GetString(topMost ? "Unpin" : "Pin") + (string.IsNullOrEmpty(LocalSettings.Settings.ShortCutModel.PinSticky) ? "" : "  " + LocalSettings.Settings.ShortCutModel.PinSticky));
      this.SetStick();
    }

    private void SetStick()
    {
      if (!this.Topmost)
        return;
      this.Topmost = true;
      IntPtr handle = new WindowInteropHelper((Window) this).Handle;
      NativeUtils.SetWindowLong(handle, WinParameter.GWL_STYLE, NativeUtils.GetWindowLong(handle, -16) & -65537 & -131073);
    }

    private void OnCloseClick(object sender, MouseButtonEventArgs e)
    {
      UserActCollectUtils.AddClickEvent("sticky_note", "sticky_note_ui", "close");
      this.Close();
    }

    public void TryClose() => this.Close();

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      this.TopBackground.ReleaseMouseCapture();
      this._mouseDownPoint = new System.Windows.Point();
      foreach (string key in TaskStickyWindow._windowRects.Keys.Where<string>((Func<string, bool>) (k => k.StartsWith("screen"))))
        TaskStickyWindow._windowRects.TryRemove(key, out TaskStickyWindow.WindowRect _);
      TaskStickyWindow._screenRects.Clear();
      if ((DateTime.Now - this._headerClickOldTime).TotalMilliseconds <= 300.0 && !this._storyPlaying)
      {
        this.CollapseWindowWithStory();
        this._headerClickOldTime = DateTime.Now.AddSeconds(-1.0);
      }
      else
        this._headerClickOldTime = DateTime.Now;
    }

    private void TryDragWindow(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (this._mouseDownPoint != new System.Windows.Point() && Mouse.LeftButton == MouseButtonState.Pressed)
      {
        if (TaskStickyWindow._screenRects.Count == 0 && !Utils.IsWindows7())
        {
          foreach (Screen allScreen in Screen.AllScreens)
          {
            uint dpiX;
            uint dpiY;
            allScreen.GetDpi(DpiType.Effective, out dpiX, out dpiY);
            System.Drawing.Rectangle rectangle = allScreen.WorkingArea;
            double left1 = (double) rectangle.Left / ((double) dpiX / 96.0);
            rectangle = allScreen.WorkingArea;
            double top1 = (double) rectangle.Top / ((double) dpiY / 96.0);
            rectangle = allScreen.WorkingArea;
            double width1 = (double) rectangle.Width / ((double) dpiX / 96.0);
            rectangle = allScreen.WorkingArea;
            double height1 = (double) rectangle.Height / ((double) dpiY / 96.0);
            rectangle = allScreen.WorkingArea;
            int left2 = rectangle.Left;
            rectangle = allScreen.WorkingArea;
            int width2 = rectangle.Width;
            double left3 = (double) (left2 + width2) / ((double) dpiX / 96.0);
            rectangle = allScreen.WorkingArea;
            int height2 = rectangle.Height;
            rectangle = allScreen.Bounds;
            int height3 = rectangle.Height;
            if (height2 == height3)
            {
              rectangle = allScreen.WorkingArea;
              double top2 = (double) rectangle.Bottom / ((double) dpiY / 96.0);
              TaskStickyWindow._screenRects.Add(new TaskStickyWindow.WindowRect(left1, top2, width1, 0.1));
            }
            TaskStickyWindow._screenRects.Add(new TaskStickyWindow.WindowRect(left1, top1, 0.1, height1));
            TaskStickyWindow._screenRects.Add(new TaskStickyWindow.WindowRect(left1, top1, width1, 0.1));
            TaskStickyWindow._screenRects.Add(new TaskStickyWindow.WindowRect(left3, top1, 0.1, height1));
          }
          List<TaskStickyWindow.WindowRect> windowRectList = TaskStickyWindow._screenRects.Value;
          for (int index = 0; index < windowRectList.Count; ++index)
          {
            TaskStickyWindow.WindowRect windowRect = windowRectList[index];
            TaskStickyWindow._windowRects.TryAdd("screen" + index.ToString(), windowRect);
          }
        }
        this.MovePosition(this._mouseDownPoint, e.GetPosition((IInputElement) this));
      }
      else
        this._mouseDownPoint = new System.Windows.Point();
    }

    private void TopGridMouseDown(object sender, MouseButtonEventArgs e)
    {
      this.TopBackground.CaptureMouse();
      this._mouseDownPoint = e.GetPosition((IInputElement) this);
    }

    private void OnMoreClick(object sender, MouseButtonEventArgs e)
    {
      this.SetEditing(true);
      UserActCollectUtils.AddClickEvent("sticky_note", "sticky_note_ui", "om");
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "AddTask", Utils.GetString("NewAdd"), (Geometry) null)
        {
          RightText = LocalSettings.Settings.ShortCutModel.AddTask,
          FontSize = 12
        },
        new CustomMenuItemViewModel((object) "Alignment", Utils.GetString("Alignment"), (Geometry) null)
        {
          FontSize = 12,
          SubActions = new List<CustomMenuItemViewModel>()
          {
            new CustomMenuItemViewModel((object) "TopAlignment", Utils.GetString("ToTop"), (Geometry) null)
            {
              RightText = HotkeyModel.HandleUpDownText(LocalSettings.Settings.ShortCutModel.StickyAlignTop),
              FontSize = 12
            },
            new CustomMenuItemViewModel((object) "LeftAlignment", Utils.GetString("ToLeft"), (Geometry) null)
            {
              RightText = HotkeyModel.HandleUpDownText(LocalSettings.Settings.ShortCutModel.StickyAlignLeft),
              FontSize = 12
            },
            new CustomMenuItemViewModel((object) "RightAlignment", Utils.GetString("ToRight"), (Geometry) null)
            {
              RightText = HotkeyModel.HandleUpDownText(LocalSettings.Settings.ShortCutModel.StickyAlignRight),
              FontSize = 12
            }
          }
        },
        new CustomMenuItemViewModel((object) "OpenMainWindow", Utils.GetString("OpenMainWindow"), (Geometry) null)
        {
          FontSize = 12
        },
        new CustomMenuItemViewModel((object) "Settings", Utils.GetString("Settings"), (Geometry) null)
        {
          FontSize = 12
        },
        new CustomMenuItemViewModel((object) null),
        new CustomMenuItemViewModel((object) "Delete", Utils.GetString("Delete"), (Geometry) null)
        {
          FontSize = 12
        }
      };
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this.MorePopup);
      if (this.ViewModel.Kind != "NOTE")
      {
        types.Insert(1, new CustomMenuItemViewModel((object) "Complete", Utils.GetString(this.ViewModel.Status == 0 ? "Done" : "TaskUndone"), (Geometry) null)
        {
          RightText = LocalSettings.Settings.ShortCutModel.CompleteTask,
          FontSize = 12
        });
        TaskPomoSetDialog taskPomoSetDialog = new TaskPomoSetDialog(true);
        taskPomoSetDialog.InitData(this.ViewModel.Id, true, "task_detail", "task_detail_pomo");
        taskPomoSetDialog.Closed += new EventHandler<bool>(customMenuList.CloseSubPopup);
        types.Insert(3, new CustomMenuItemViewModel((object) "Focus", Utils.GetString("BeginFocus"), (Geometry) null)
        {
          FontSize = 12,
          SubControl = (ITabControl) taskPomoSetDialog
        });
      }
      this.MorePopup.VerticalOffset = this.ViewModel.Kind != "NOTE" ? (double) sbyte.MinValue : -88.0;
      customMenuList.Operated += new EventHandler<object>(this.OnSelected);
      customMenuList.Show();
    }

    private void OnSelected(object sender, object e)
    {
      this.MorePopup.IsOpen = false;
      if (!(e is string str) || str == null)
        return;
      switch (str.Length)
      {
        case 6:
          if (!(str == "Delete"))
            break;
          this.DeleteTask();
          break;
        case 7:
          if (!(str == "AddTask"))
            break;
          this.CreateNewSticky();
          break;
        case 8:
          switch (str[0])
          {
            case 'C':
              if (!(str == "Complete"))
                return;
              this.ToggleTask();
              return;
            case 'S':
              if (!(str == "Settings"))
                return;
              this.OpenSettings();
              return;
            default:
              return;
          }
        case 12:
          if (!(str == "TopAlignment"))
            break;
          this.Align(false, false);
          break;
        case 13:
          if (!(str == "LeftAlignment"))
            break;
          this.Align(true, false);
          break;
        case 14:
          switch (str[0])
          {
            case 'O':
              if (!(str == "OpenMainWindow"))
                return;
              this.OpenInApp();
              return;
            case 'R':
              if (!(str == "RightAlignment"))
                return;
              this.Align(true, true);
              return;
            default:
              return;
          }
      }
    }

    protected override async void OnActivated(EventArgs e)
    {
      base.OnActivated(e);
      await Task.Delay(50);
    }

    protected override void OnDeactivated(EventArgs e) => base.OnDeactivated(e);

    private async void CollapseWindow()
    {
      TaskStickyWindow element = this;
      element._collapsed = !element._collapsed;
      if (!element._collapsed)
      {
        element.DetailControl.ScrollToTop();
        element.TitleGrid.Visibility = Visibility.Collapsed;
        element.SecondRow.Height = new GridLength(1.0, GridUnitType.Star);
        element.MaxHeight = double.PositiveInfinity;
        element.MinHeight = 80.0;
        // ISSUE: explicit non-virtual call
        __nonvirtual (element.BeginAnimation(FrameworkElement.HeightProperty, (AnimationTimeline) null));
        // ISSUE: explicit non-virtual call
        __nonvirtual (element.Height) = element._originHeight;
        element.StartExpand();
        element.DetailControl.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        element.TitleGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        element.DetailControl.Opacity = 1.0;
        element.TitleGrid.Opacity = 0.0;
      }
      else
      {
        Keyboard.ClearFocus();
        FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
        Keyboard.Focus((IInputElement) element);
        element._originHeight = element.ActualHeight;
        element.TopBackground.Background = (System.Windows.Media.Brush) System.Windows.Media.Brushes.Transparent;
        element.FirstRow.Height = new GridLength(22.0);
        element.Container.VerticalAlignment = VerticalAlignment.Center;
        element.TitleGrid.Visibility = Visibility.Visible;
        element.TitleGrid.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        element.DetailControl.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
        element.TitleGrid.Opacity = 1.0;
        element.DetailControl.Opacity = 0.0;
        element.SecondRow.Height = new GridLength(0.0);
        element.BottomGrid.Visibility = Visibility.Collapsed;
        element.SetResourceReference(FrameworkElement.MinHeightProperty, (object) "StickyHeight42");
        element.SetResourceReference(FrameworkElement.MaxHeightProperty, (object) "StickyHeight42");
        // ISSUE: explicit non-virtual call
        __nonvirtual (element.BeginAnimation(FrameworkElement.HeightProperty, (AnimationTimeline) null));
        element.SetResourceReference(FrameworkElement.HeightProperty, (object) "StickyHeight42");
      }
      element.UpdateLocationCache();
      element.SetTopTooltip();
    }

    private async void CollapseWindowWithStory()
    {
      TaskStickyWindow element = this;
      element._storyPlaying = true;
      element._collapsed = !element._collapsed;
      if (!element._collapsed)
      {
        element.DetailControl.ScrollToTop();
        element.SecondRow.Height = new GridLength(1.0, GridUnitType.Star);
        element.MaxHeight = double.PositiveInfinity;
        Storyboard resource = (Storyboard) element.FindResource((object) "ExpandStory");
        if (resource.Children[0] is DoubleAnimation child)
          child.To = new double?(element._originHeight);
        resource.Begin();
        await Task.Delay(100);
        element.StartExpand();
      }
      else
      {
        Keyboard.ClearFocus();
        FocusManager.SetFocusedElement((DependencyObject) element, (IInputElement) element);
        Keyboard.Focus((IInputElement) element);
        element._originHeight = element.ActualHeight;
        element.TopBackground.Background = (System.Windows.Media.Brush) System.Windows.Media.Brushes.Transparent;
        element.SetResourceReference(FrameworkElement.MinHeightProperty, (object) "StickyHeight42");
        double resource1 = (double) element.FindResource((object) "StickyHeight42");
        Storyboard resource2 = (Storyboard) element.FindResource((object) "CollapseStory");
        if (resource2.Children[0] is DoubleAnimation child)
        {
          child.From = new double?(element.ActualHeight);
          child.To = new double?(resource1);
        }
        resource2.Begin();
        element.FirstRow.Height = new GridLength(22.0);
        element.Container.VerticalAlignment = VerticalAlignment.Center;
        element.TitleGrid.Visibility = Visibility.Visible;
        await Task.Delay(180);
        element.StartCollapse();
      }
      element.SetTopTooltip();
    }

    private void StartExpand()
    {
      this.BottomGrid.Visibility = Visibility.Visible;
      this.FirstRow.Height = new GridLength(14.0);
      this.TopBackground.SetResourceReference(System.Windows.Controls.Control.BackgroundProperty, (object) "StickyTopColor");
      this.Container.VerticalAlignment = VerticalAlignment.Stretch;
      this.UpdateLocationCache();
    }

    private void StartCollapse()
    {
      ((Storyboard) this.FindResource((object) "ShowTitleStory")).Begin();
    }

    private void OnExpand(object sender, EventArgs e)
    {
      this.MinHeight = 80.0;
      this.BeginAnimation(FrameworkElement.HeightProperty, (AnimationTimeline) null);
      this.TitleGrid.Visibility = Visibility.Collapsed;
      this._storyPlaying = false;
    }

    private void OnCollapsed(object sender, EventArgs e)
    {
      this.SecondRow.Height = new GridLength(0.0);
      this.BottomGrid.Visibility = Visibility.Collapsed;
      this.SetResourceReference(FrameworkElement.MaxHeightProperty, (object) "StickyHeight42");
      this.BeginAnimation(FrameworkElement.HeightProperty, (AnimationTimeline) null);
      this.SetResourceReference(FrameworkElement.HeightProperty, (object) "StickyHeight42");
      this._storyPlaying = false;
      this.UpdateLocationCache();
    }

    private void UpdateLocationCache()
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        TaskStickyWindow.AddOrUpdateWindowLocation(new TaskStickyModel()
        {
          TaskId = this.ViewModel.Id,
          Top = this.Top,
          Left = this.Left,
          Width = this.Width,
          Height = this.Height
        });
        this.SaveSticky();
      }));
    }

    private async Task SaveSticky()
    {
      TaskStickyWindow taskStickyWindow = this;
      // ISSUE: explicit non-virtual call
      // ISSUE: explicit non-virtual call
      TaskStickyModel taskStickyModel = await TaskStickyDao.SaveStickyLocation(taskStickyWindow.ViewModel.Id, taskStickyWindow.Left, taskStickyWindow.Top, __nonvirtual (taskStickyWindow.Width), taskStickyWindow._collapsed ? taskStickyWindow._originHeight : __nonvirtual (taskStickyWindow.Height), !taskStickyWindow._collapsed);
    }

    private void OnWindowMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
      ((Storyboard) this.FindResource((object) "ShowOptionStory")).Begin();
    }

    private void OnWindowMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (this._editing)
        return;
      ((Storyboard) this.FindResource((object) "HideOptionStory")).Begin();
    }

    private void OnDateClick(object sender, MouseButtonEventArgs e)
    {
      UserActCollectUtils.AddClickEvent("sticky_note", "sticky_note_ui", "set_time");
      this.ViewModel.ShowDateSelector((FrameworkElement) this);
    }

    public async void SetEditing(bool editing)
    {
      TaskStickyWindow taskStickyWindow = this;
      taskStickyWindow._editing = editing;
      if (editing)
      {
        taskStickyWindow.OnWindowMouseEnter((object) null, (System.Windows.Input.MouseEventArgs) null);
      }
      else
      {
        await Task.Delay(100);
        // ISSUE: explicit non-virtual call
        if (__nonvirtual (taskStickyWindow.IsMouseOver))
          return;
        taskStickyWindow.OnWindowMouseLeave((object) null, (System.Windows.Input.MouseEventArgs) null);
      }
    }

    private void TryToast(string toast) => this.TryToastString((object) null, toast);

    public static async Task TryLoadSavedStickies()
    {
      List<TaskStickyModel> allAsync = await TaskStickyDao.GetAllAsync();
      UtilLog.Info(nameof (TryLoadSavedStickies));
      foreach (TaskStickyModel taskStickyModel in allAsync)
      {
        TaskStickyModel sticky = taskStickyModel;
        if (!sticky.Closed)
        {
          if (!TaskCache.ExistTask(sticky.TaskId))
            BaseDao<TaskStickyModel>.DeleteAsync(sticky);
          else
            System.Windows.Application.Current?.Dispatcher.Invoke((Action) (() =>
            {
              if (!string.IsNullOrEmpty(TaskUtils.ToastOnOpenSticky(sticky.TaskId)))
              {
                sticky.Closed = true;
                BaseDao<TaskStickyModel>.UpdateAsync(sticky);
              }
              else
              {
                TaskStickyWindow taskStickyWindow = new TaskStickyWindow(sticky);
                TaskStickyWindow._openedStickies.TryAdd(sticky.TaskId, taskStickyWindow);
                taskStickyWindow.Show();
              }
            }));
        }
      }
    }

    public static async Task CloseAllStickies()
    {
      try
      {
        UtilLog.Info(nameof (CloseAllStickies));
        foreach (KeyValuePair<string, TaskStickyWindow> openedSticky in TaskStickyWindow._openedStickies)
          openedSticky.Value.Close();
      }
      catch (Exception ex)
      {
      }
      TaskStickyWindow._openedStickies.Clear();
    }

    private async void DeleteTask()
    {
      TaskModel task = await TaskDao.GetThinTaskById(this.ViewModel.Id);
      if (task == null)
        task = (TaskModel) null;
      else if (this.ViewModel.IsEmptyTask())
      {
        TaskService.DeleteTask(this.ViewModel.Id, 2);
        task = (TaskModel) null;
      }
      else
      {
        List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(task.id, task.projectId);
        // ISSUE: explicit non-virtual call
        if (subTasksByIdAsync != null && __nonvirtual (subTasksByIdAsync.Count) > 0)
        {
          subTasksByIdAsync.Add(task);
          int num = await TaskService.BatchDeleteTasks(subTasksByIdAsync, false) ? 1 : 0;
          task = (TaskModel) null;
        }
        else
        {
          await TaskService.DeleteTask(task.id);
          task = (TaskModel) null;
        }
      }
    }

    public void TaskDeleted(string taskId)
    {
    }

    public void TryToastString(object sender, string e)
    {
      this.ToastGrid.Margin = new Thickness(8.0, 0.0, 8.0, this.ActualHeight > 150.0 ? 30.0 : 10.0);
      WindowToastHelper.ToastString(this.ToastGrid, e, height: 30.0, fontSize: 10.0);
    }

    public async Task<bool> BatchDeleteTask(List<TaskModel> tasks) => false;

    public void TaskComplete(CloseUndoToast undo)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, (FrameworkElement) undo);
    }

    public void TryHideToast()
    {
    }

    public void ToastDeleteRecUndo(List<TaskDeleteRecurrenceUndoEntity> undoModels)
    {
    }

    public void ToastMoveProjectControl(
      string projectProjectId,
      string taskName = null,
      MoveToastType moveType = MoveToastType.Move)
    {
    }

    public void Toast(FrameworkElement uiElement)
    {
      WindowToastHelper.ShowAndHideToast(this.ToastGrid, uiElement);
    }

    public static bool TryCloseInProject(string projectId)
    {
      bool flag = false;
      foreach (string str in TaskStickyWindow._openedStickies.Keys.ToList<string>())
      {
        TaskBaseViewModel taskById = TaskCache.GetTaskById(str);
        if (taskById != null && taskById.ProjectId == projectId)
        {
          TaskStickyWindow._openedStickies[str].Close();
          TaskStickyDao.CloseAsync(str);
          flag = true;
        }
      }
      return flag;
    }

    public static bool ExistSticky(string taskId)
    {
      return !string.IsNullOrEmpty(taskId) && TaskStickyWindow._openedStickies.ContainsKey(taskId);
    }

    private void OnColorClick(object sender, MouseButtonEventArgs e)
    {
      this.ShowColorSelector();
      UserActCollectUtils.AddClickEvent("sticky_note", "sticky_note_ui", "color");
    }

    public void ShowColorSelector()
    {
      this.SetEditing(true);
      StickyColorSelector stickyColorSelector = new StickyColorSelector(this._color);
      stickyColorSelector.SetPlacementTarget((UIElement) this.ThemeBorder);
      stickyColorSelector.ColorSelect += new EventHandler<string>(this.OnColorSelect);
      stickyColorSelector.Closed += new EventHandler(this.OnPopupClosed);
      stickyColorSelector.IsOpen = true;
    }

    private void OnPopupClosed(object sender, EventArgs e) => this.SetEditing(false);

    public void QuickChangeColor()
    {
      if (!ProChecker.CheckPro(ProType.StickyColor))
        return;
      StickyColorKey result;
      if (!Enum.TryParse<StickyColorKey>(this._color, true, out result))
        result = StickyColorKey.Default;
      ++result;
      if (result >= (StickyColorKey) (Enum.GetValues(typeof (StickyColorKey)).Cast<StickyColorKey>().Count<StickyColorKey>() - 1))
        result = StickyColorKey.Default;
      this.SetStickyColor(result.ToString());
      TaskStickyDao.SaveStickyColor(this.ViewModel.Id, result.ToString());
    }

    private void OnColorSelect(object sender, string e)
    {
      if (this._color == e)
        return;
      this.SetStickyColor(e);
      TaskStickyDao.SaveStickyColor(this.ViewModel.Id, e);
    }

    private async void CreateNewSticky()
    {
      await Task.Delay(50);
      UserActCollectUtils.AddClickEvent("sticky_note", "om", "add");
      this.CreateNew();
    }

    public async Task CreateNew()
    {
      TaskStickyWindow taskStickyWindow = this;
      TaskModel task = await TaskService.CreateNewTask();
      TaskStickyWindow window;
      if (task == null)
      {
        task = (TaskModel) null;
        window = (TaskStickyWindow) null;
      }
      else
      {
        TaskStickyModel stickyModel = new TaskStickyModel(task.id, LocalSettings.Settings.LoginUserId);
        stickyModel.Color = TaskStickyWindow.GetDefaultColor();
        stickyModel.TopMost = LocalSettings.Settings.ExtraSettings.StickyDefaultPin;
        stickyModel.Left = taskStickyWindow.Left;
        // ISSUE: explicit non-virtual call
        double top = taskStickyWindow.Top + __nonvirtual (taskStickyWindow.Height) - 15.0;
        System.Drawing.Rectangle rect = ScreenPositionUtils.GetWindowPositionRect((Window) taskStickyWindow);
        if (top >= (double) (rect.Top + rect.Height - 100))
        {
          top = (double) rect.Top;
          int topExtra = 20;
          List<TaskStickyModel> source = await TaskStickyDao.GetOpenedAsync() ?? new List<TaskStickyModel>();
          while (source.Any<TaskStickyModel>((Func<TaskStickyModel, bool>) (s => Math.Abs(s.Left - stickyModel.Left) < 4.0 && Math.Abs(s.Top - top) < 4.0)))
          {
            top = (double) (rect.Top + topExtra);
            topExtra += 20;
            if (topExtra == 500)
              break;
          }
        }
        else if (top >= (double) (rect.Top + rect.Height) - stickyModel.Height)
          top = (double) (rect.Top + rect.Height) - stickyModel.Height;
        stickyModel.Top = top;
        await TaskStickyDao.AddStickyAsync(stickyModel);
        window = new TaskStickyWindow(await TaskStickyWindow.GetTaskStickyModel(task.id));
        window._checkEmpty = true;
        window.Show();
        TaskStickyWindow._openedStickies[task.id] = window;
        await Task.Delay(50);
        window.Activate();
        task = (TaskModel) null;
        window = (TaskStickyWindow) null;
      }
    }

    private async void ToggleTask()
    {
      UserActCollectUtils.AddClickEvent("sticky_note", "om", "done");
      this.ViewModel?.CompleteTaskCommand();
    }

    private async void OpenInApp()
    {
      UserActCollectUtils.AddClickEvent("sticky_note", "om", "open_main_window");
      App.ShowMainWindow(this.ViewModel.SourceModel.Id, string.Empty);
    }

    public static async Task<List<TaskStickyWindow>> ShowTaskSticky(List<string> taskIds)
    {
      if (taskIds == null)
        return (List<TaskStickyWindow>) null;
      await Task.Delay(50);
      System.Drawing.Rectangle rect = ScreenPositionUtils.GetMousePositionRect();
      List<TaskStickyModel> stickies = await TaskStickyDao.GetAllAsync() ?? new List<TaskStickyModel>();
      stickies = stickies.Where<TaskStickyModel>((Func<TaskStickyModel, bool>) (s => !s.Closed || taskIds.Contains(s.TaskId))).ToList<TaskStickyModel>();
      List<TaskStickyModel> models = new List<TaskStickyModel>();
      int topExtra = 0;
      foreach (string taskId in taskIds)
      {
        string id = taskId;
        TaskStickyModel model = await TaskStickyWindow.GetTaskStickyModel(id);
        if (model == null)
        {
          if (TaskStickyWindow._openedStickies.ContainsKey(id))
          {
            TaskStickyWindow._openedStickies[id].Show();
            TaskStickyWindow._openedStickies[id].Activate();
          }
        }
        else
        {
          if (Math.Abs(model.Top + 1.0) < 0.01 && Math.Abs(model.Left + 1.0) < 0.01)
          {
            model.Left = (double) rect.Left + ((double) rect.Width - model.Width) / 2.0;
            model.Top = (double) (rect.Top + rect.Height / 4);
            if (topExtra >= 500)
            {
              model.Top = (double) (rect.Top + topExtra);
            }
            else
            {
              int num = topExtra;
              while (true)
              {
                do
                {
                  TaskStickyModel taskStickyModel = stickies.FirstOrDefault<TaskStickyModel>((Func<TaskStickyModel, bool>) (sticky => Math.Abs(sticky.Left - model.Left) < 4.0 && Math.Abs(sticky.Top - model.Top) < 4.0));
                  if (taskStickyModel != null)
                  {
                    model.Top += taskStickyModel.Height - 15.0;
                    if (model.Top >= (double) (rect.Top + rect.Height - 100))
                    {
                      model.Top = (double) (rect.Top + num);
                      num += 20;
                      if (num >= 520)
                        goto label_18;
                    }
                  }
                  else
                    goto label_18;
                }
                while (model.Top < (double) (rect.Top + rect.Height) - model.Height);
                model.Top = (double) (rect.Top + rect.Height) - model.Height;
              }
label_18:
              if (num != topExtra)
                topExtra = num - 20;
            }
            int num1 = await BaseDao<TaskStickyModel>.UpdateAsync(model);
            stickies.Add(model);
          }
          models.Add(model);
          id = (string) null;
        }
      }
      List<TaskStickyWindow> taskStickyWindowList = new List<TaskStickyWindow>();
      foreach (TaskStickyModel stickyModel in models)
      {
        if (stickyModel != null)
        {
          TaskStickyWindow taskStickyWindow = new TaskStickyWindow(stickyModel);
          taskStickyWindow.ShowAndActive();
          taskStickyWindowList.Add(taskStickyWindow);
          TaskStickyWindow._openedStickies[stickyModel.TaskId] = taskStickyWindow;
        }
      }
      return taskStickyWindowList;
    }

    private async Task ShowAndActive()
    {
      TaskStickyWindow taskStickyWindow = this;
      taskStickyWindow.Show();
      await Task.Delay(50);
      taskStickyWindow.Activate();
    }

    public static async Task CreateNewStickyStatic()
    {
      TaskModel newTask = await TaskService.CreateNewTask();
      if (newTask == null)
        return;
      List<TaskStickyWindow> taskStickyWindowList = await TaskStickyWindow.ShowTaskSticky(new List<string>()
      {
        newTask.id
      });
      // ISSUE: explicit non-virtual call
      if (taskStickyWindowList == null || __nonvirtual (taskStickyWindowList.Count) <= 0)
        return;
      taskStickyWindowList[0]._checkEmpty = true;
    }

    private void InitShortCut()
    {
      this._keyBindings.Add(("StickyColor", GetKeyBinding(TaskStickyCommands.SetColorCommand)));
      this._keyBindings.Add(("AddTask", GetKeyBinding(TaskStickyCommands.CreatNewCommand)));
      this._keyBindings.Add(("PinSticky", GetKeyBinding(TaskStickyCommands.PinCommand)));
      this._keyBindings.Add(("CompleteTask", GetKeyBinding(TaskStickyCommands.CompleteCommand)));
      this._keyBindings.Add(("StickyCollapse", GetKeyBinding(TaskStickyCommands.StickyCollapseCommand)));
      this._keyBindings.Add(("StickyAlignTop", GetKeyBinding(TaskStickyCommands.StickyAlignTopCommand)));
      this._keyBindings.Add(("StickyAlignLeft", GetKeyBinding(TaskStickyCommands.StickyAlignLeftCommand)));
      this._keyBindings.Add(("StickyAlignRight", GetKeyBinding(TaskStickyCommands.StickyAlignRightCommand)));
      foreach ((string, KeyBinding) keyBinding in this._keyBindings)
        KeyBindingManager.TryAddKeyBinding(keyBinding.Item1, keyBinding.Item2);

      KeyBinding GetKeyBinding(ICommand command)
      {
        if (command == null)
          return (KeyBinding) null;
        KeyBinding keyBinding1 = new KeyBinding(command, new KeyGesture(Key.None));
        keyBinding1.CommandParameter = (object) this;
        KeyBinding keyBinding2 = keyBinding1;
        this.InputBindings.Add((InputBinding) keyBinding2);
        return keyBinding2;
      }
    }

    public void RemoveKeyBinding()
    {
      foreach ((string, KeyBinding) keyBinding in this._keyBindings)
        KeyBindingManager.RemoveKeyBinding(keyBinding.Item1, keyBinding.Item2);
    }

    public static void OnAppExit() => WindowManager.AppLockOrExit = true;

    public void Pin()
    {
      bool topMost = !this.Topmost;
      TaskStickyDao.SaveStickyTopMost(this.ViewModel.Id, topMost);
      this.SetTopMost(topMost);
      this.TryToast(Utils.GetString(topMost ? "Pinned" : "Unpinned"));
    }

    public void CompleteTaskCommand() => this.ViewModel?.CompleteTaskCommand();

    public async void Reload()
    {
      this.DetailControl.Reload();
      await Task.Delay(100);
      this.ViewModel.ResetDateText();
    }

    public static void ReloadStickies()
    {
      foreach (TaskStickyWindow taskStickyWindow in TaskStickyWindow._openedStickies.Values.ToList<TaskStickyWindow>())
        taskStickyWindow?.Reload();
    }

    public static void SetShowInTaskBar()
    {
      foreach (TaskStickyWindow taskStickyWindow in TaskStickyWindow._openedStickies.Values.ToList<TaskStickyWindow>())
      {
        if (taskStickyWindow != null)
        {
          double left = taskStickyWindow.Left;
          taskStickyWindow.ShowInTaskbar = !LocalSettings.Settings.ExtraSettings.StickyHideInTaskBar;
          taskStickyWindow.Left -= 2.0;
          taskStickyWindow.Left = left;
        }
      }
    }

    private void OpenSettings()
    {
      SettingsHelper.PullRemoteSettings();
      UserActCollectUtils.AddClickEvent("sticky_note", "om", "settings");
      SettingsHelper.PullRemoteSettings();
      SettingDialog.ShowSettingDialog(SettingsType.StickyNote, (Window) this);
    }

    private void AnimSetPosition(double left, double top)
    {
      this.RemoveAnimation();
      Storyboard resource = (Storyboard) this.FindResource((object) "MoveStory");
      DoubleAnimation child1 = (DoubleAnimation) resource.Children[0];
      DoubleAnimation child2 = (DoubleAnimation) resource.Children[1];
      DoubleAnimation child3 = (DoubleAnimation) resource.Children[2];
      DoubleAnimation child4 = (DoubleAnimation) resource.Children[3];
      child1.To = new double?(left);
      child2.To = new double?(top);
      child3.To = new double?(TaskStickyWindow.ResetOnAlign ? 254.0 : this.Width);
      double? nullable = new double?(this._collapsed || !TaskStickyWindow.ResetOnAlign ? this.Height : 228.0);
      child4.To = nullable;
      if (this._collapsed && TaskStickyWindow.ResetOnAlign)
      {
        this.Width = 254.0;
        this._originHeight = 228.0;
      }
      resource.Begin();
    }

    private void RemoveAnimation()
    {
      this.BeginAnimation(Window.LeftProperty, (AnimationTimeline) null);
      this.BeginAnimation(Window.TopProperty, (AnimationTimeline) null);
      this.BeginAnimation(FrameworkElement.WidthProperty, (AnimationTimeline) null);
      this.BeginAnimation(FrameworkElement.HeightProperty, (AnimationTimeline) null);
    }

    public void CollapseOrExpandAll()
    {
      System.Drawing.Rectangle rectangle1 = WindowSizing.GetMonitorFromWindow((Window) this);
      if (rectangle1 == new System.Drawing.Rectangle())
        rectangle1 = ScreenPositionUtils.GetWindowPositionRect((Window) this);
      List<TaskStickyWindow> source = new List<TaskStickyWindow>();
      foreach (TaskStickyWindow taskStickyWindow in TaskStickyWindow._openedStickies.Values.ToList<TaskStickyWindow>())
      {
        System.Drawing.Rectangle rectangle2 = WindowSizing.GetMonitorFromWindow((Window) taskStickyWindow);
        if (rectangle2 == new System.Drawing.Rectangle())
          rectangle2 = ScreenPositionUtils.GetWindowPositionRect((Window) taskStickyWindow);
        if (rectangle1 == rectangle2)
          source.Add(taskStickyWindow);
      }
      bool flag = source.Any<TaskStickyWindow>((Func<TaskStickyWindow, bool>) (w => !w._collapsed));
      if (source.Any<TaskStickyWindow>((Func<TaskStickyWindow, bool>) (window => window._storyPlaying)))
        return;
      foreach (TaskStickyWindow taskStickyWindow in source)
      {
        if (taskStickyWindow._collapsed != flag)
          taskStickyWindow.CollapseWindow();
      }
    }

    public void OnOpacityChanged(object sender, EventArgs e) => this.SetOpacity();

    private void SetOpacity()
    {
      this.BottomBackground.Opacity = Math.Max(0.01, LocalSettings.Settings.ExtraSettings.StickyOpacity / 100.0);
      ((DropShadowEffect) this.BackBorder.Effect).Opacity = LocalSettings.Settings.ExtraSettings.StickyOpacity < 80.0 ? 0.0 : 0.24;
    }

    public static void ShowHideAllStickyStatic()
    {
      List<TaskStickyWindow> list = TaskStickyWindow._openedStickies.Values.ToList<TaskStickyWindow>();
      if (list.Count == 0)
        return;
      bool flag = list.Any<TaskStickyWindow>((Func<TaskStickyWindow, bool>) (w => w.WindowState != WindowState.Minimized));
      foreach (TaskStickyWindow taskStickyWindow in list)
      {
        if (flag && taskStickyWindow.WindowState != WindowState.Minimized)
        {
          if (LocalSettings.Settings.ExtraSettings.StickyHideInTaskBar)
            taskStickyWindow.ShowInTaskbar = true;
          taskStickyWindow.WindowState = WindowState.Minimized;
        }
        if (!flag && taskStickyWindow.WindowState == WindowState.Minimized)
        {
          if (LocalSettings.Settings.ExtraSettings.StickyHideInTaskBar)
            taskStickyWindow.ShowInTaskbar = false;
          taskStickyWindow.WindowState = WindowState.Normal;
        }
      }
    }

    private void OnStateChanged(object sender, EventArgs e)
    {
      if (this.WindowState != WindowState.Normal || !this.ShowInTaskbar || !LocalSettings.Settings.ExtraSettings.StickyHideInTaskBar)
        return;
      this.ShowInTaskbar = false;
    }

    private void SetStickyColor(StickyColorKey colorKey)
    {
      this.Resources[(object) "StickyBackColor"] = (object) StickyColorViewModel.GetBackColor(colorKey);
      this.Resources[(object) "StickyTopColor"] = (object) StickyColorViewModel.GetTopColor(colorKey);
      if (TaskStickyWindow.BorderColorDict.ContainsKey(colorKey))
        this.Resources[(object) "StickyBorderColor"] = (object) ThemeUtil.GetColorInString(TaskStickyWindow.BorderColorDict[colorKey]);
      else
        this.Resources[(object) "StickyBorderColor"] = (object) System.Windows.Media.Brushes.Transparent;
      if (TaskStickyWindow.PopupBackColorDict.ContainsKey(colorKey))
        this.Resources[(object) "PopupBackground"] = (object) ThemeUtil.GetColorInString(TaskStickyWindow.PopupBackColorDict[colorKey]);
      else
        this.Resources[(object) "PopupBackground"] = (object) System.Windows.Media.Brushes.White;
      this.Resources[(object) "StickyTextColor20"] = (object) ThemeUtil.GetColorInString(TaskStickyWindow.IsDarkTheme(colorKey) ? "#33FFFFFF" : "#33191919");
      this.Resources[(object) "StickyTextColor40"] = (object) ThemeUtil.GetColorInString(TaskStickyWindow.IsDarkTheme(colorKey) ? "#66FFFFFF" : "#66191919");
      this.Resources[(object) "StickyTextColor60"] = (object) ThemeUtil.GetColorInString(TaskStickyWindow.IsDarkTheme(colorKey) ? "#99FFFFFF" : "#99191919");
      this.Resources[(object) "StickyContentTextColor"] = (object) ThemeUtil.GetColorInString(TaskStickyWindow.IsDarkTheme(colorKey) ? "#82FFFFFF" : "#CC191919");
      this.Resources[(object) "StickyTitleTextColor"] = (object) ThemeUtil.GetColorInString(TaskStickyWindow.IsDarkTheme(colorKey) ? "#B3FFFFFF" : "#E8191919");
      this.Resources[(object) "StickyCompletedTextColor"] = (object) ThemeUtil.GetColorInString(TaskStickyWindow.IsDarkTheme(colorKey) ? "#33FFFFFF" : "#66191919");
      this.Resources[(object) "StickyTextColor100"] = (object) ThemeUtil.GetColorInString(TaskStickyWindow.IsDarkTheme(colorKey) ? "#FFFFFF" : "#191919");
      this.Resources[(object) "StickyBorderThickness"] = (object) new Thickness((double) (TaskStickyWindow.IsDarkTheme(colorKey) ? 1 : 0));
    }

    private static bool IsDarkTheme(StickyColorKey colorKey)
    {
      return colorKey.ToString().StartsWith("Dark");
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/taskstickywindow.xaml", UriKind.Relative));
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
        case 1:
          this.Root = (TaskStickyWindow) target;
          break;
        case 2:
          ((Timeline) target).Completed += new EventHandler(this.OnCollapsed);
          break;
        case 3:
          ((Timeline) target).Completed += new EventHandler(this.OnExpand);
          break;
        case 4:
          this.BackBorder = (Border) target;
          break;
        case 5:
          this.Container = (Grid) target;
          break;
        case 6:
          this.FirstRow = (RowDefinition) target;
          break;
        case 7:
          this.SecondRow = (RowDefinition) target;
          break;
        case 8:
          this.BottomBackground = (Border) target;
          break;
        case 9:
          this.TopBackground = (Border) target;
          this.TopBackground.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.TopGridMouseDown);
          this.TopBackground.MouseMove += new System.Windows.Input.MouseEventHandler(this.TryDragWindow);
          this.TopBackground.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseUp);
          break;
        case 10:
          this.TitleGrid = (Border) target;
          break;
        case 11:
          this.TopTitleText = (DetailTextBox) target;
          break;
        case 12:
          this.TopGrid = (StackPanel) target;
          break;
        case 13:
          this.ThemeBorder = (Border) target;
          this.ThemeBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnColorClick);
          break;
        case 14:
          this.ColorTooltip = (System.Windows.Controls.ToolTip) target;
          break;
        case 15:
          this.PinGrid = (Border) target;
          this.PinGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnPinClick);
          break;
        case 16:
          this.PinTooltip = (System.Windows.Controls.ToolTip) target;
          break;
        case 17:
          this.PinPath = (Path) target;
          break;
        case 18:
          this.CloseGrid = (Border) target;
          this.CloseGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
          break;
        case 19:
          this.DetailControl = (TaskDetailStickyView) target;
          break;
        case 20:
          this.BottomGrid = (Grid) target;
          break;
        case 21:
          this.DateBtn = (Border) target;
          this.DateBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDateClick);
          break;
        case 22:
          this.DatePanel = (StackPanel) target;
          break;
        case 23:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
          break;
        case 24:
          this.MoreIcon = (Path) target;
          break;
        case 25:
          this.MorePopup = (EscPopup) target;
          break;
        case 26:
          this.ToastGrid = (Grid) target;
          break;
        case 27:
          this.SizeChangeBorder = (Border) target;
          this.SizeChangeBorder.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnSizeChangeMouseDown);
          this.SizeChangeBorder.MouseMove += new System.Windows.Input.MouseEventHandler(this.OnBorderMouseMove);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [Flags]
    private enum DragSizeMode
    {
      None = 0,
      Top = 1,
      Bottom = 2,
      Left = 4,
      Right = 8,
    }

    private struct WindowRect
    {
      public double Left;
      public double Top;
      public double Width;
      public double Height;

      public double Right => this.Left + this.Width;

      public double Bottom => this.Top + this.Height;

      public WindowRect(double left, double top, double width, double height)
      {
        this.Left = left;
        this.Top = top;
        this.Width = width;
        this.Height = height;
      }

      public double GetLOrR(bool right) => !right ? this.Left : this.Right;

      public void SetRight(double right) => this.Left = right - this.Width;

      public void SetBottom(double bottom) => this.Top = bottom - this.Height;

      public double GetSide1(bool isHorizon) => !isHorizon ? this.Top : this.Left;

      public double GetSide2(bool isHorizon) => !isHorizon ? this.Bottom : this.Right;

      public bool IsIntersect(TaskStickyWindow.WindowRect rect, bool isHorizon, double extra = 0.0)
      {
        return MathUtil.Between(this.GetSide1(isHorizon), rect.GetSide1(isHorizon) - extra, rect.GetSide2(isHorizon) + extra) || MathUtil.Between(this.GetSide2(isHorizon), rect.GetSide1(isHorizon) - extra, rect.GetSide2(isHorizon) + extra);
      }

      public void SetSide1(bool isHorizon, double val)
      {
        if (isHorizon)
          this.Left = val;
        else
          this.Top = val;
      }

      public void SetSide2(bool isHorizon, double val)
      {
        if (isHorizon)
          this.SetRight(val);
        else
          this.SetBottom(val);
      }

      public double GetLength(bool isHorizon) => !isHorizon ? this.Height : this.Width;

      public void SetLength(bool isHorizon, double val)
      {
        if (isHorizon)
          this.Width = val;
        else
          this.Height = val;
      }

      public bool IntersectWith(TaskStickyWindow.WindowRect rect)
      {
        if (rect.Left < this.Right && rect.Right > this.Left && rect.Top < this.Bottom && rect.Bottom > this.Top)
          return true;
        return MathUtil.Between(rect.Right, this.Left, this.Right, false) && MathUtil.Between(rect.Top, this.Top, this.Bottom, false);
      }
    }

    private class StickySortModel
    {
      public string Id;
      public TaskStickyWindow.WindowRect Rect;
      public double Direction;
    }
  }
}
