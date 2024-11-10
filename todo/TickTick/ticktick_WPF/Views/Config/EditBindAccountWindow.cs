// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.EditBindAccountWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class EditBindAccountWindow : Window, IComponentConnector
  {
    private SubscribeCalendar _parent;
    private bool _contentLoaded;

    public static async Task<EditBindAccountWindow> Build(
      string calendarId,
      SubscribeCalendar parent = null)
    {
      if (!string.IsNullOrEmpty(calendarId))
      {
        BindCalendarAccountModel bindCalendarAccount = await BindCalendarAccountDao.GetBindCalendarAccount(calendarId);
        if (bindCalendarAccount != null)
          return new EditBindAccountWindow(BindAccountViewModel.Build(bindCalendarAccount))
          {
            _parent = parent
          };
      }
      return (EditBindAccountWindow) null;
    }

    public EditBindAccountWindow(BindAccountViewModel model)
    {
      this.InitializeComponent();
      this.DataContext = (object) model;
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      this.Owner?.Activate();
      base.OnClosing(e);
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      EditBindAccountWindow bindAccountWindow = this;
      if (bindAccountWindow.DataContext != null && bindAccountWindow.DataContext is BindAccountViewModel dataContext)
      {
        foreach (BindCalendarViewModel calendar in dataContext.Calendars)
        {
          if (!(calendar.StatusItems.FirstOrDefault<ComboBoxViewModel>((Func<ComboBoxViewModel, bool>) (i => i.Selected))?.Value is string status))
            status = "show";
          await BindCalendarAccountDao.SaveCalendarShowStatus(calendar.Id, status);
        }
      }
      bindAccountWindow.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private async void OnUnsubscribeClicked(object sender, MouseButtonEventArgs e)
    {
      EditBindAccountWindow bindAccountWindow = this;
      if (!(bindAccountWindow.DataContext is BindAccountViewModel dataContext))
        return;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("Unsubscribe"), Utils.GetString("CancelSubscribeMessage"), MessageBoxButton.OKCancel);
      customerDialog.Owner = Window.GetWindow((DependencyObject) bindAccountWindow);
      bool? nullable = customerDialog.ShowDialog();
      if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
        return;
      if (dataContext.IsBindAccount())
        await SubscribeCalendarHelper.UnbindCalendar(dataContext.Id);
      else
        await SubscribeCalendarHelper.UnsubscribeCalendar(dataContext.Id);
      bindAccountWindow._parent?.LoadData();
      App.Window.TryReloadCalendar();
      bindAccountWindow.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/editbindaccountwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnUnsubscribeClicked);
          break;
        case 2:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
