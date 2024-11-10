// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.CalendarInfoItem
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
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class CalendarInfoItem : UserControl, IComponentConnector
  {
    internal Border Container;
    internal TextBlock PopupShowText;
    private bool _contentLoaded;

    public CalendarInfoItem() => this.InitializeComponent();

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext == null)
        return;
      SubscribeCalendarViewModel model = this.DataContext as SubscribeCalendarViewModel;
      if (model == null)
        return;
      model.Expand = !model.Expand;
      model.Parent.Where<SubscribeCalendarViewModel>((Func<SubscribeCalendarViewModel, bool>) (item => item.Expand && item.CalendarId != model.CalendarId)).ToList<SubscribeCalendarViewModel>().ForEach((Action<SubscribeCalendarViewModel>) (item => item.Expand = false));
    }

    private async void OnEditClick(object sender, RoutedEventArgs e)
    {
      CalendarInfoItem child = this;
      if (child.DataContext == null)
        return;
      SubscribeCalendarViewModel model = child.DataContext as SubscribeCalendarViewModel;
      if (model == null)
        return;
      switch (model.SubType)
      {
        case SubscribeCalendarType.Account:
          if (string.IsNullOrEmpty(model.CalendarId))
            break;
          if (model.IsExpired)
          {
            child.PopupShowText.Visibility = Visibility.Visible;
            List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
            types.Add(new CustomMenuItemViewModel((object) "Resubscribe", Utils.GetString("Resubscribe"), (Geometry) null));
            types.Add(new CustomMenuItemViewModel((object) "Unsubscribe", Utils.GetString("Unsubscribe"), (Geometry) null));
            EscPopup escPopup1 = new EscPopup();
            escPopup1.StaysOpen = false;
            escPopup1.PlacementTarget = sender as UIElement;
            escPopup1.Placement = PlacementMode.Bottom;
            escPopup1.HorizontalOffset = -82.0;
            EscPopup escPopup2 = escPopup1;
            escPopup2.Closed += (EventHandler) ((o, args) => this.PopupShowText.Visibility = Visibility.Collapsed);
            CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) escPopup2);
            customMenuList.Operated += (EventHandler<object>) ((o, val) =>
            {
              if (!(val is string str2))
                return;
              switch (str2)
              {
                case "Resubscribe":
                  if (model.Account.IsBindAccountPassword())
                  {
                    new BindAccountWindow(model.Account)
                    {
                      Owner = Utils.GetParentWindow((DependencyObject) this)
                    }.ShowDialog();
                    break;
                  }
                  if (model.Account.Site == "outlook")
                  {
                    BindOutlookWindow bindOutlookWindow = new BindOutlookWindow();
                    bindOutlookWindow.SetOriginAccount(model.Account?.Id);
                    bindOutlookWindow.ShowDialog();
                    break;
                  }
                  if (model.Account.Site == "google")
                  {
                    BindGoogleAccount.GetInstance().Start(model.Account?.Id);
                    break;
                  }
                  if (!(model.Account.Kind == "icloud"))
                    break;
                  new BindICloudWindow((SubscribeCalendar) null, model.Account).ShowDialog();
                  break;
                case "Unsubscribe":
                  bool? nullable = new CustomerDialog(Utils.GetString("Unsubscribe"), Utils.GetString("CancelSubscribeMessage"), MessageBoxButton.OKCancel, Window.GetWindow((DependencyObject) this)).ShowDialog();
                  if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
                    break;
                  SubscribeCalendarHelper.UnbindCalendar(model.Account.Id);
                  break;
              }
            });
            customMenuList.Show();
            break;
          }
          EditBindAccountWindow bindAccountWindow = await EditBindAccountWindow.Build(model.CalendarId, Utils.FindParent<SubscribeCalendar>((DependencyObject) child));
          if (bindAccountWindow == null)
            break;
          bindAccountWindow.Owner = Window.GetWindow((DependencyObject) child);
          bindAccountWindow.ShowDialog();
          break;
        case SubscribeCalendarType.Url:
          EditUrlWindow editUrlWindow = new EditUrlWindow(model);
          editUrlWindow.Owner = Window.GetWindow((DependencyObject) child);
          editUrlWindow.ShowDialog();
          break;
      }
    }

    private async void OnUnsubscribeClick(object sender, RoutedEventArgs e)
    {
      CalendarInfoItem child = this;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("Unsubscribe"), Utils.GetString("CancelSubscribeMessage"), MessageBoxButton.OKCancel);
      customerDialog.Owner = Window.GetWindow((DependencyObject) child);
      bool? nullable = customerDialog.ShowDialog();
      if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0 || child.DataContext == null || !(child.DataContext is SubscribeCalendarViewModel dataContext))
        return;
      switch (dataContext.SubType)
      {
        case SubscribeCalendarType.Account:
          await SubscribeCalendarHelper.UnbindCalendar(dataContext.CalendarId);
          break;
        case SubscribeCalendarType.Url:
          await SubscribeCalendarHelper.UnsubscribeCalendar(dataContext.CalendarId);
          break;
      }
      Utils.FindParent<SubscribeCalendar>((DependencyObject) child)?.LoadData();
    }

    private void OnReauthorizeClick(object sender, RoutedEventArgs e)
    {
      if (!(this.DataContext is SubscribeCalendarViewModel dataContext))
        return;
      SubscribeCalendar parent = Utils.FindParent<SubscribeCalendar>((DependencyObject) this);
      if (dataContext.Account.IsBindAccountPassword())
      {
        BindAccountWindow bindAccountWindow = new BindAccountWindow(dataContext.Account, parent: parent);
        bindAccountWindow.Owner = Utils.GetParentWindow((DependencyObject) this);
        bindAccountWindow.ShowDialog();
      }
      else if (dataContext.Account.Site == "outlook")
      {
        BindOutlookWindow bindOutlookWindow = new BindOutlookWindow(parent);
        bindOutlookWindow.SetOriginAccount(dataContext.Account?.Id);
        bindOutlookWindow.ShowDialog();
      }
      else if (dataContext.Account.Site == "google")
      {
        BindGoogleAccount.GetInstance(parent).Start(dataContext.Account?.Id);
      }
      else
      {
        if (!(dataContext.Account.Kind == "icloud"))
          return;
        new BindICloudWindow(parent, dataContext.Account).ShowDialog();
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/calendarinfoitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Container = (Border) target;
          this.Container.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnEditClick);
          break;
        case 3:
          this.PopupShowText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
