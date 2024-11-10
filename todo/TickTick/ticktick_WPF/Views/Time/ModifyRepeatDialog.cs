// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.ModifyRepeatDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.Util.KotlinUtils;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class ModifyRepeatDialog : MyWindow, IOkCancelWindow, IComponentConnector
  {
    public EditorType Selected = EditorType.CANCEL;
    public static bool Showing;
    internal TextBlock TitleTextBlock;
    internal TextBlock ContentTextBlock;
    internal StackPanel ModifyRadios;
    internal RadioButton CurrentRadio;
    internal RadioButton FutureRadio;
    internal RadioButton AllRadio;
    internal StackPanel CompleteRadios;
    internal RadioButton CompleteAllRadio;
    internal RadioButton SkipAllRadio;
    internal Button OkButton;
    internal Button CancelButton;
    internal Button CloseButton;
    private bool _contentLoaded;

    public ModifyRepeatDialog(List<string> strings, bool isDelete = false)
    {
      this.InitializeComponent();
      if (!strings.Contains(EditorType.CURRENT.ToString()))
        this.CurrentRadio.Visibility = Visibility.Collapsed;
      else
        this.CurrentRadio.IsChecked = new bool?(true);
      if (!strings.Contains(EditorType.FROM_CURRENT.ToString()))
        this.FutureRadio.Visibility = Visibility.Collapsed;
      if (!strings.Contains(EditorType.ALL.ToString()))
        this.AllRadio.Visibility = Visibility.Collapsed;
      this.TitleTextBlock.Text = Utils.GetString("EditRecurringTask");
      this.ContentTextBlock.Text = Utils.GetString(isDelete ? "DeletingRepeat" : "EditingRepeat");
      ModifyRepeatDialog.Showing = true;
    }

    public ModifyRepeatDialog(List<DateTime> repeatDates)
    {
      this.InitializeComponent();
      this.ModifyRadios.Visibility = Visibility.Collapsed;
      this.CompleteRadios.Visibility = Visibility.Visible;
      this.CompleteAllRadio.IsChecked = new bool?(true);
      this.TitleTextBlock.Text = Utils.GetString("EditRecurringTask");
      this.ContentTextBlock.Text = string.Format(Utils.GetString(repeatDates.Count > 1 ? "HandleTaskCycles" : "HandleTaskCycle"), (object) repeatDates.Count);
      ModifyRepeatDialog.Showing = true;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      this.Selected = EditorType.CANCEL;
      this.Close();
    }

    private void OnOkClick(object sender, RoutedEventArgs e) => this.Ok();

    private async void OnCloseClick(object sender, RoutedEventArgs e)
    {
      ModifyRepeatDialog modifyRepeatDialog = this;
      modifyRepeatDialog.Selected = EditorType.CANCEL;
      modifyRepeatDialog.Close();
    }

    public async void OnCancel()
    {
      ModifyRepeatDialog modifyRepeatDialog = this;
      modifyRepeatDialog.Selected = EditorType.CANCEL;
      modifyRepeatDialog.Close();
    }

    public void Ok()
    {
      this.Selected = !this.CurrentRadio.IsChecked.GetValueOrDefault() ? (!this.FutureRadio.IsChecked.GetValueOrDefault() ? (!this.AllRadio.IsChecked.GetValueOrDefault() ? (!this.CompleteAllRadio.IsChecked.GetValueOrDefault() ? (!this.SkipAllRadio.IsChecked.GetValueOrDefault() ? EditorType.CANCEL : EditorType.SKIP) : EditorType.COMPLETEALL) : EditorType.ALL) : EditorType.FROM_CURRENT) : EditorType.CURRENT;
      this.Close();
    }

    protected override void OnClosed(EventArgs e)
    {
      ModifyRepeatDialog.Showing = false;
      ticktick_WPF.Notifier.GlobalEventManager.NotifyModifyRecurrenceCompleted();
      base.OnClosed(e);
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      InputBindingCollection inputBindings1 = this.InputBindings;
      KeyBinding keyBinding1 = new KeyBinding(OkCancelWindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding1.CommandParameter = (object) this;
      inputBindings1.Add((InputBinding) keyBinding1);
      InputBindingCollection inputBindings2 = this.InputBindings;
      KeyBinding keyBinding2 = new KeyBinding(OkCancelWindowCommands.OkCommand, new KeyGesture(Key.Return));
      keyBinding2.CommandParameter = (object) this;
      inputBindings2.Add((InputBinding) keyBinding2);
      this.Activate();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/modifyrepeatdialog.xaml", UriKind.Relative));
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
          this.TitleTextBlock = (TextBlock) target;
          break;
        case 2:
          this.ContentTextBlock = (TextBlock) target;
          break;
        case 3:
          this.ModifyRadios = (StackPanel) target;
          break;
        case 4:
          this.CurrentRadio = (RadioButton) target;
          break;
        case 5:
          this.FutureRadio = (RadioButton) target;
          break;
        case 6:
          this.AllRadio = (RadioButton) target;
          break;
        case 7:
          this.CompleteRadios = (StackPanel) target;
          break;
        case 8:
          this.CompleteAllRadio = (RadioButton) target;
          break;
        case 9:
          this.SkipAllRadio = (RadioButton) target;
          break;
        case 10:
          this.OkButton = (Button) target;
          this.OkButton.Click += new RoutedEventHandler(this.OnOkClick);
          break;
        case 11:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 12:
          this.CloseButton = (Button) target;
          this.CloseButton.Click += new RoutedEventHandler(this.OnCloseClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
