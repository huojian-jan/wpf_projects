// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusDetail.FocusDetailTitle
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusDetail
{
  public class FocusDetailTitle : DockPanel
  {
    private EmjTextBlock _title;
    private HoverIconButton _linkIcon;
    private FocusObjIcon _iconBorder;
    private PomoFilterControl _focusFilter;

    public FocusDetailTitle() => this.Loaded += new RoutedEventHandler(this.OnLoaded);

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      this.InitIcon();
      this.InitLinkIcon();
      this.InitTitle();
    }

    private void InitLinkIcon()
    {
      HoverIconButton hoverIconButton = new HoverIconButton();
      hoverIconButton.IconData = Utils.GetIcon("PomoLinkTask");
      hoverIconButton.IsImage = false;
      hoverIconButton.VerticalAlignment = VerticalAlignment.Center;
      this._linkIcon = hoverIconButton;
      this._linkIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnLinkClick);
      this._linkIcon.SetValue(DockPanel.DockProperty, (object) Dock.Right);
      this.Children.Add((UIElement) this._linkIcon);
    }

    private void OnLinkClick(object sender, MouseButtonEventArgs e)
    {
      bool flag = !object.Equals(sender, (object) this._linkIcon);
      if (this._focusFilter == null)
      {
        EscPopup escPopup = new EscPopup();
        escPopup.StaysOpen = false;
        PomoFilterControl pomoFilterControl = new PomoFilterControl((Popup) escPopup);
        pomoFilterControl.DataContext = (object) TickFocusManager.Config.FocusVModel;
        this._focusFilter = pomoFilterControl;
      }
      Popup parentPopup = this._focusFilter.GetParentPopup();
      parentPopup.PlacementTarget = flag ? (UIElement) this._title : sender as UIElement;
      parentPopup.Placement = flag ? PlacementMode.Bottom : PlacementMode.Left;
      parentPopup.HorizontalOffset = flag ? -5.0 : 30.0;
      parentPopup.VerticalOffset = flag ? -5.0 : 30.0;
      parentPopup.IsOpen = true;
    }

    private void InitIcon()
    {
      FocusObjIcon focusObjIcon = new FocusObjIcon(30.0);
      focusObjIcon.Cursor = Cursors.Hand;
      this._iconBorder = focusObjIcon;
      this._iconBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTitleClick);
      this.Children.Add((UIElement) this._iconBorder);
      this._iconBorder.SetValue(DockPanel.DockProperty, (object) Dock.Left);
      this.SetIcon();
      this.SetFocusIcon();
    }

    private void InitTitle()
    {
      EmjTextBlock emjTextBlock = new EmjTextBlock();
      emjTextBlock.FontSize = 18.0;
      emjTextBlock.VerticalAlignment = VerticalAlignment.Center;
      emjTextBlock.Margin = new Thickness(12.0, -1.0, 0.0, 0.0);
      emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
      emjTextBlock.TextWrapping = TextWrapping.Wrap;
      emjTextBlock.MaxHeight = 24.0;
      emjTextBlock.ClipToBounds = true;
      emjTextBlock.Cursor = Cursors.Hand;
      emjTextBlock.Background = (Brush) Brushes.Transparent;
      emjTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
      this._title = emjTextBlock;
      this._title.MouseEnter += (MouseEventHandler) ((o, e) => this._title.SetResourceReference(TextBlock.ForegroundProperty, (object) "PrimaryColor"));
      this._title.MouseLeave += (MouseEventHandler) ((o, e) => this._title.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100"));
      this._title.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTitleClick);
      this._title.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this.Children.Add((UIElement) this._title);
      this.SetTitle();
    }

    private async void OnTitleClick(object sender, MouseButtonEventArgs e)
    {
      FocusDetailTitle target = this;
      string objId = TickFocusManager.Config.FocusVModel.ObjId;
      if (!string.IsNullOrEmpty(objId))
      {
        if (TickFocusManager.Config.FocusVModel.IsHabit)
        {
          HabitModel habit = await HabitDao.GetHabitById(objId);
          if (habit != null)
          {
            HabitCheckInModel byHabitIdAndStamp = await HabitCheckInDao.GetHabitCheckInsByHabitIdAndStamp(habit.Id, DateUtils.GetDateNum(DateTime.Today).ToString());
            new HabitCheckInWindow(habit, byHabitIdAndStamp != null ? byHabitIdAndStamp.CheckStatus : 0, new DateTime?(DateTime.Today), (IToastShowWindow) App.Window).Show((UIElement) target, 0.0, 0.0, true);
          }
          habit = (HabitModel) null;
        }
        else
        {
          if (!TaskCache.ExistTask(objId))
            return;
          new TaskDetailPopup()
          {
            DependentWindow = ((IToastShowWindow) null)
          }.Show(objId, (string) null, new TaskWindowDisplayArgs((UIElement) target._title, 30.0));
        }
        UserActCollectUtils.AddClickEvent("focus", TickFocusManager.GetActCType(), "select_task_task_detail");
      }
      else
        target.OnLinkClick(sender, e);
    }

    public void SetTitle()
    {
      if (this._title == null)
        return;
      this._title.Text = string.IsNullOrWhiteSpace(TickFocusManager.Config.FocusVModel.FocusId) ? Utils.GetString(TickFocusManager.IsPomo ? "PomoTimer2" : "Timing") : TickFocusManager.Config.FocusVModel.Title;
    }

    public void SetIcon() => this._iconBorder?.SetIcon();

    public void SetFocusIcon()
    {
      this._iconBorder?.SetFocusIcon(TickFocusManager.Config.FocusVModel.FocusId, TickFocusManager.Config.FocusVModel.Type);
    }
  }
}
