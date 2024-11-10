// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.EditUrlWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Framework.Collections;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class EditUrlWindow : Window, IComponentConnector
  {
    private SubscribeCalendar _parent;
    internal CustomComboBox StatusComboBox;
    internal ticktick_WPF.Views.Misc.ColorSelector.ColorSelector ColorItems;
    private bool _contentLoaded;

    public EditUrlWindow(SubscribeCalendarViewModel model, SubscribeCalendar parent = null)
    {
      this._parent = parent;
      this.InitializeComponent();
      this.DataContext = (object) model;
      if (model.SubType == SubscribeCalendarType.Url)
      {
        this.ColorItems.Visibility = Visibility.Visible;
        this.ColorItems.SetSelectedColor(model.Color);
      }
      CustomComboBox statusComboBox = this.StatusComboBox;
      ExtObservableCollection<ComboBoxViewModel> items = new ExtObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel1 = new ComboBoxViewModel((object) "show", Utils.GetString("Show"), 32.0);
      comboBoxViewModel1.Selected = model.ShowStatus == 0;
      items.Add(comboBoxViewModel1);
      ComboBoxViewModel comboBoxViewModel2 = new ComboBoxViewModel((object) "calendar", Utils.GetString("ShowOnlyInCalendar"), 32.0);
      comboBoxViewModel2.Selected = model.ShowStatus == 1;
      items.Add(comboBoxViewModel2);
      ComboBoxViewModel comboBoxViewModel3 = new ComboBoxViewModel((object) "hidden", Utils.GetString("Hide"), 32.0);
      comboBoxViewModel3.Selected = model.ShowStatus == 2;
      items.Add(comboBoxViewModel3);
      statusComboBox.Init<ComboBoxViewModel>((ObservableCollection<ComboBoxViewModel>) items, (ComboBoxViewModel) null);
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      EditUrlWindow editUrlWindow = this;
      if (editUrlWindow.DataContext != null && editUrlWindow.DataContext is SubscribeCalendarViewModel dataContext)
      {
        string showStatus = EditUrlWindow.GetShowStatus(editUrlWindow.StatusComboBox.SelectedIndex);
        string selectedColor = editUrlWindow.ColorItems.GetSelectedColor();
        if (selectedColor != dataContext.Color)
          ticktick_WPF.Views.Misc.ColorSelector.ColorSelector.TryAddClickEvent(selectedColor);
        dataContext.Color = selectedColor;
        await CalendarSubscribeProfileDao.UpdateShowStatusAndColor(dataContext.CalendarId, showStatus, selectedColor);
      }
      editUrlWindow.Close();
    }

    private static string GetShowStatus(int index)
    {
      switch (index)
      {
        case 0:
          return "show";
        case 1:
          return "calendar";
        case 2:
          return "hidden";
        default:
          return "show";
      }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private async void OnUnsubscribeClicked(object sender, MouseButtonEventArgs e)
    {
      EditUrlWindow editUrlWindow = this;
      if (!(editUrlWindow.DataContext is SubscribeCalendarViewModel dataContext))
        return;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("Unsubscribe"), Utils.GetString("CancelSubscribeMessage"), MessageBoxButton.OKCancel);
      customerDialog.Owner = Window.GetWindow((DependencyObject) editUrlWindow);
      bool? nullable = customerDialog.ShowDialog();
      if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
        return;
      await SubscribeCalendarHelper.UnsubscribeCalendar(dataContext.CalendarId);
      editUrlWindow._parent?.LoadData();
      App.Window.TryReloadCalendar();
      editUrlWindow.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/editurlwindow.xaml", UriKind.Relative));
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
          this.StatusComboBox = (CustomComboBox) target;
          break;
        case 2:
          this.ColorItems = (ticktick_WPF.Views.Misc.ColorSelector.ColorSelector) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnUnsubscribeClicked);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
