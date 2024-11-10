// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MiniFocus.PomoStatisticsSetView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MiniFocus
{
  public class PomoStatisticsSetView : UserControl, IComponentConnector, IStyleConnector
  {
    private EscPopup _popup;
    private ObservableCollection<PomoStatisticsSetItemViewModel> _items;
    private int _selectIndex;
    internal ItemsControl Items;
    private bool _contentLoaded;

    public event EventHandler PopupClosed;

    public event EventHandler TypeChanged;

    public PomoStatisticsSetView(int selectedIndex, int anotherSelectedIndex)
    {
      this.InitializeComponent();
      ObservableCollection<PomoStatisticsSetItemViewModel> observableCollection = new ObservableCollection<PomoStatisticsSetItemViewModel>();
      observableCollection.Add(new PomoStatisticsSetItemViewModel()
      {
        Name = MiniFocusStatisticsView.MiniStatisticsTodayDuration
      });
      observableCollection.Add(new PomoStatisticsSetItemViewModel()
      {
        Name = MiniFocusStatisticsView.MiniStatisticsTodayPomo
      });
      observableCollection.Add(new PomoStatisticsSetItemViewModel()
      {
        Name = MiniFocusStatisticsView.MiniStatisticsWeekDuration
      });
      observableCollection.Add(new PomoStatisticsSetItemViewModel()
      {
        Name = MiniFocusStatisticsView.MiniStatisticsWeekPomo
      });
      this._items = observableCollection;
      this._items[selectedIndex].Selected = true;
      this._items[anotherSelectedIndex].IsEnabled = false;
      this._selectIndex = selectedIndex;
      this.Items.ItemsSource = (IEnumerable) this._items;
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is PomoStatisticsSetItemViewModel dataContext) || dataContext.Selected)
        return;
      int num = this._items.IndexOf(dataContext);
      if (!LocalSettings.Settings.PomoLocalSetting.MiniStatisticsTypes.Contains(this._selectIndex.ToString()) || LocalSettings.Settings.PomoLocalSetting.MiniStatisticsTypes.Contains(num.ToString()))
        return;
      LocalSettings.Settings.PomoLocalSetting.MiniStatisticsTypes = LocalSettings.Settings.PomoLocalSetting.MiniStatisticsTypes.Replace(this._selectIndex.ToString(), num.ToString());
      EventHandler typeChanged = this.TypeChanged;
      if (typeChanged != null)
        typeChanged((object) this, (EventArgs) null);
      this._popup.IsOpen = false;
    }

    public void Show(UIElement element)
    {
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.PlacementTarget = element;
      escPopup.Placement = PlacementMode.Center;
      escPopup.VerticalOffset = 74.0;
      this._popup = escPopup;
      this._popup.Closed += (EventHandler) ((o, e) =>
      {
        EventHandler popupClosed = this.PopupClosed;
        if (popupClosed != null)
          popupClosed((object) this, (EventArgs) null);
        this.PopupClosed = (EventHandler) null;
        this.TypeChanged = (EventHandler) null;
      });
      this._popup.Child = (UIElement) this;
      this._popup.IsOpen = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/minifocus/pomostatisticssetview.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.Items = (ItemsControl) target;
      else
        this._contentLoaded = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
    }
  }
}
