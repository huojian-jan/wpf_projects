// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TimeLine
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar.Week;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TimeLine : UserControl, IComponentConnector
  {
    public bool TopHandleDragging;
    public bool BottomHandleDragging;
    private TimeLineBlock[] _blocks;
    private List<TimeOffset> _items;
    public static readonly DependencyProperty TopFoldHoverProperty = DependencyProperty.Register(nameof (TopFoldHover), typeof (bool), typeof (TimeLine), new PropertyMetadata((object) false));
    public static readonly DependencyProperty BottomFoldHoverProperty = DependencyProperty.Register(nameof (BottomFoldHover), typeof (bool), typeof (TimeLine), new PropertyMetadata((object) false));
    internal Grid Containner;
    internal StackPanel ItemsPanel;
    internal Border TopExpandBorder;
    internal Border BottomExpandBorder;
    private bool _contentLoaded;

    public bool TopFoldHover
    {
      get => (bool) this.GetValue(TimeLine.TopFoldHoverProperty);
      set => this.SetValue(TimeLine.TopFoldHoverProperty, (object) value);
    }

    public bool BottomFoldHover
    {
      get => (bool) this.GetValue(TimeLine.BottomFoldHoverProperty);
      set => this.SetValue(TimeLine.BottomFoldHoverProperty, (object) value);
    }

    public TimeLine()
    {
      this.InitializeComponent();
      this.InitItems();
      this.InitBoard();
      this.DataContext = (object) new TimeLineViewModel()
      {
        StartDate = DateTime.Today
      };
      this.Loaded += (RoutedEventHandler) ((sender, e) => CalendarGeoHelper.TopFoldedChanged += new EventHandler(this.OnTopFoldedChanged));
      this.Unloaded += (RoutedEventHandler) ((s, e) => CalendarGeoHelper.TopFoldedChanged -= new EventHandler(this.OnTopFoldedChanged));
      this.TopExpandBorder.Visibility = CalendarGeoHelper.TopFolded ? Visibility.Collapsed : Visibility.Visible;
      this.BottomExpandBorder.Visibility = CalendarGeoHelper.TopFolded ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnTopFoldedChanged(object sender, EventArgs e)
    {
      this.TopExpandBorder.Visibility = CalendarGeoHelper.TopFolded ? Visibility.Collapsed : Visibility.Visible;
      this.BottomExpandBorder.Visibility = CalendarGeoHelper.TopFolded ? Visibility.Collapsed : Visibility.Visible;
    }

    private void InitItems()
    {
      this._blocks = new TimeLineBlock[26];
      for (int index = 0; index < 26; ++index)
      {
        TimeLineBlock element = new TimeLineBlock();
        this._blocks[index] = element;
        this.ItemsPanel.Children.Add((UIElement) element);
      }
    }

    public void InitBoard(bool checkNow = false)
    {
      this._items = !CalendarGeoHelper.TopFolded ? this.InitFullBoard() : this.InitCollapsedBoard();
      for (int index = 0; index < this._blocks.Length; ++index)
      {
        this._blocks[index].CheckNow = checkNow;
        if (index < this._items.Count)
        {
          this._blocks[index].Visibility = Visibility.Visible;
          this._blocks[index].DataContext = (object) this._items[index];
        }
        else
          this._blocks[index].Visibility = Visibility.Collapsed;
      }
    }

    private List<TimeOffset> InitCollapsedBoard()
    {
      List<TimeOffset> timeOffsetList = new List<TimeOffset>();
      int startHour = CalendarGeoHelper.GetStartHour(false);
      int endHour = CalendarGeoHelper.GetEndHour(false);
      timeOffsetList.Add(new TimeOffset()
      {
        Offset = 0,
        Folded = true
      });
      timeOffsetList.Add(new TimeOffset()
      {
        Offset = startHour,
        Folded = true,
        ShowDivider = true
      });
      timeOffsetList.Add(new TimeOffset()
      {
        Offset = startHour + 1,
        IsFirst = true
      });
      for (int index = startHour + 1; index < endHour; ++index)
        timeOffsetList.Add(new TimeOffset()
        {
          Offset = index
        });
      timeOffsetList.Add(new TimeOffset()
      {
        Offset = endHour,
        Folded = true
      });
      if (endHour < 24)
        timeOffsetList.Add(new TimeOffset()
        {
          Offset = 24,
          Folded = true,
          ShowDivider = true
        });
      return timeOffsetList;
    }

    private List<TimeOffset> InitFullBoard()
    {
      int startHour = CalendarGeoHelper.GetStartHour(false);
      int endHour = CalendarGeoHelper.GetEndHour(false);
      List<TimeOffset> timeOffsetList = new List<TimeOffset>();
      for (int index = 0; index < 25; ++index)
      {
        TimeOffset timeOffset = new TimeOffset()
        {
          Offset = index
        };
        bool flag1 = index >= 0 && index < startHour || index >= endHour;
        bool flag2 = index == startHour;
        bool flag3 = index == endHour - 1;
        timeOffset.FoldExpanded = flag1;
        timeOffset.ShowTopHandle = flag2;
        timeOffset.ShowBottomHandle = flag3;
        timeOffsetList.Add(timeOffset);
      }
      return timeOffsetList;
    }

    public void ExpandTimeline(TimeOffset model)
    {
      CalendarGeoHelper.TopFolded = false;
      CalendarGeoHelper.NotifyTopUnFolded(model.Offset < LocalSettings.Settings.CollapsedEnd);
    }

    public void NotifyHandleDragged(int offset, bool isTop)
    {
      if (this._items == null)
        return;
      this._items.ForEach((Action<TimeOffset>) (item =>
      {
        if (isTop)
        {
          item.ShowTopHandle = item.Offset == offset + 1;
          CalendarGeoHelper.SetStartHour(offset + 1);
        }
        else
        {
          item.ShowBottomHandle = item.Offset == offset;
          CalendarGeoHelper.SetEndHour(offset + 1);
        }
        if (isTop)
        {
          int? offset1 = this._items.FirstOrDefault<TimeOffset>((Func<TimeOffset, bool>) (model => model.ShowBottomHandle))?.Offset;
          TimeOffset timeOffset = item;
          int num;
          if (item.Offset > offset)
          {
            int offset2 = item.Offset;
            int? nullable = offset1;
            int valueOrDefault = nullable.GetValueOrDefault();
            num = offset2 > valueOrDefault & nullable.HasValue ? 1 : 0;
          }
          else
            num = 1;
          timeOffset.FoldExpanded = num != 0;
        }
        else
        {
          int? offset3 = this._items.FirstOrDefault<TimeOffset>((Func<TimeOffset, bool>) (model => model.ShowTopHandle))?.Offset;
          TimeOffset timeOffset = item;
          int num;
          if (item.Offset <= offset)
          {
            int offset4 = item.Offset;
            int? nullable = offset3;
            int valueOrDefault = nullable.GetValueOrDefault();
            num = offset4 < valueOrDefault & nullable.HasValue ? 1 : 0;
          }
          else
            num = 1;
          timeOffset.FoldExpanded = num != 0;
        }
      }));
    }

    public int GetBottomOffset()
    {
      if (this._items != null)
      {
        int? offset = this._items.FirstOrDefault<TimeOffset>((Func<TimeOffset, bool>) (model => model.ShowBottomHandle))?.Offset;
        if (offset.HasValue)
          return offset.Value;
      }
      return 0;
    }

    public int GetTopOffset()
    {
      if (this._items != null)
      {
        int? offset = this._items.FirstOrDefault<TimeOffset>((Func<TimeOffset, bool>) (model => model.ShowTopHandle))?.Offset;
        if (offset.HasValue)
          return offset.Value;
      }
      return 0;
    }

    public void OnHandleDrop(TimeOffset offset, bool mouseDown)
    {
      if (this.BottomHandleDragging || this.TopHandleDragging)
        mouseDown = false;
      this.BottomHandleDragging = false;
      this.TopHandleDragging = false;
    }

    private void OnFoldClick(object sender, MouseButtonEventArgs e)
    {
      CalendarGeoHelper.TopFolded = true;
    }

    public void SetTryDrag(bool tryDrag)
    {
      if (tryDrag)
      {
        this.TopExpandBorder.IsHitTestVisible = false;
        this.BottomExpandBorder.IsHitTestVisible = false;
      }
      else
      {
        this.TopExpandBorder.IsHitTestVisible = true;
        this.BottomExpandBorder.IsHitTestVisible = true;
      }
    }

    private void OnBorderEnter(object sender, MouseEventArgs e)
    {
      if (!(sender is Border border))
        return;
      if (border.Equals((object) this.TopExpandBorder))
      {
        this.TopFoldHover = true;
        this.BottomFoldHover = false;
      }
      else if (border.Equals((object) this.BottomExpandBorder))
      {
        this.BottomFoldHover = true;
        this.TopFoldHover = false;
      }
      Utils.FindParent<CalendarTimelineView>((DependencyObject) this)?.ShowOrHideFoldTooltip(true);
    }

    private void OnBorderLeave(object sender, MouseEventArgs e)
    {
      if (!(sender is Border border))
        return;
      if (border.Equals((object) this.TopExpandBorder))
        this.TopFoldHover = false;
      else if (border.Equals((object) this.BottomExpandBorder))
        this.BottomFoldHover = false;
      Utils.FindParent<CalendarTimelineView>((DependencyObject) this)?.ShowOrHideFoldTooltip(false);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/timeline.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Containner = (Grid) target;
          break;
        case 2:
          this.ItemsPanel = (StackPanel) target;
          break;
        case 3:
          this.TopExpandBorder = (Border) target;
          this.TopExpandBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFoldClick);
          this.TopExpandBorder.MouseEnter += new MouseEventHandler(this.OnBorderEnter);
          this.TopExpandBorder.MouseLeave += new MouseEventHandler(this.OnBorderLeave);
          break;
        case 4:
          this.BottomExpandBorder = (Border) target;
          this.BottomExpandBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFoldClick);
          this.BottomExpandBorder.MouseEnter += new MouseEventHandler(this.OnBorderEnter);
          this.BottomExpandBorder.MouseLeave += new MouseEventHandler(this.OnBorderLeave);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
