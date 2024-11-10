// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.CheckInLogControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Service;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class CheckInLogControl : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty ShowEditProperty = DependencyProperty.Register(nameof (ShowEdit), typeof (bool), typeof (CheckInLogControl), new PropertyMetadata((object) true));
    internal CheckInLogControl Root;
    internal TextBox ContentText;
    private bool _contentLoaded;

    public CheckInLogControl() => this.InitializeComponent();

    public bool ShowEdit
    {
      get => (bool) this.GetValue(CheckInLogControl.ShowEditProperty);
      set => this.SetValue(CheckInLogControl.ShowEditProperty, (object) value);
    }

    private void OnExpandClick(object sender, MouseButtonEventArgs e)
    {
      this.ContentText.MaxLines = this.ContentText.MaxLines <= 3 ? 100 : 3;
    }

    private void OnEditClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext == null || !(this.DataContext is CheckInLogViewModel dataContext))
        return;
      new EditHabitLogWindow(dataContext).ShowDialog();
    }

    private async void OnDeleteClick(object sender, MouseButtonEventArgs e)
    {
      CheckInLogControl child = this;
      if (child.DataContext == null || !(child.DataContext is CheckInLogViewModel dataContext))
        return;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("DeleteHabitRecord"), string.Format(Utils.GetString("DeleteHabitRecordHint"), (object) dataContext.DisplayDate), Utils.GetString("Delete"), Utils.GetString("Cancel"));
      customerDialog.Owner = Window.GetWindow((DependencyObject) child);
      bool? nullable = customerDialog.ShowDialog();
      if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
        return;
      dataContext.IsHide = true;
      Utils.FindParent<HabitLogControl>((DependencyObject) child)?.OnItemDeleted();
      await HabitService.DeleteCheckInRecord(dataContext.Id);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/checkinlogcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (CheckInLogControl) target;
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnEditClick);
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteClick);
          break;
        case 4:
          this.ContentText = (TextBox) target;
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnExpandClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
