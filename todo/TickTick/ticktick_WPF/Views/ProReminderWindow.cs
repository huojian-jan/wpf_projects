// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.ProReminderWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class ProReminderWindow : MyWindow, IComponentConnector
  {
    internal StackPanel LimitPanel;
    internal StackPanel ProjectLimitPanel;
    internal TextBlock ProjectLimitText;
    internal StackPanel TaskLimitPanel;
    internal TextBlock TaskLimitText;
    internal StackPanel ShareLimitPanel;
    internal TextBlock ShareLimitText;
    internal TextBlock ReminderText;
    internal CheckBox DontShowCheckBox;
    private bool _contentLoaded;

    public ProReminderWindow(ListOverLimitsModel model)
    {
      this.InitializeComponent();
      this.InitHintText(model);
      this.TrySetOwner();
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding(WindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
    }

    private void InitHintText(ListOverLimitsModel model)
    {
      if (model.ProjectNum > 9)
      {
        this.ProjectLimitPanel.Visibility = Visibility.Visible;
        this.ProjectLimitText.Text = Utils.GetString("ProjectOverLimitText");
      }
      if (model.TaskNum > 0)
      {
        this.TaskLimitPanel.Visibility = Visibility.Visible;
        this.TaskLimitText.Text = string.Format(Utils.GetString("TaskOverLimitText"), (object) model.TaskNum);
      }
      if (model.ShareNum > 0)
      {
        this.ShareLimitPanel.Visibility = Visibility.Visible;
        this.ShareLimitText.Text = string.Format(Utils.GetString("ShareOverLimitText"), (object) model.ShareNum);
      }
      if (model.ProjectNum > 9 || model.TaskNum > 0 || model.ShareNum > 0)
        this.LimitPanel.Visibility = Visibility.Visible;
      this.ReminderText.Text = string.Format(Utils.GetString("UpdateReminder"), (object) Utils.GetAppName());
    }

    private void CloseWindow(object sender, RoutedEventArgs e) => this.Close();

    private async void StartUpdate(object sender, RoutedEventArgs e)
    {
      ProReminderWindow proReminderWindow = this;
      await Utils.StartUpgrade("pro_expired_downgrade");
      proReminderWindow.Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (this.DontShowCheckBox.IsChecked.GetValueOrDefault())
        LocalSettings.Settings.DontShowProWindow = true;
      base.OnClosing(e);
    }

    private void TrySetOwner()
    {
      try
      {
        this.Owner = (Window) App.Window;
      }
      catch (Exception ex)
      {
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/proreminderwindow.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.CloseWindow);
          break;
        case 2:
          this.LimitPanel = (StackPanel) target;
          break;
        case 3:
          this.ProjectLimitPanel = (StackPanel) target;
          break;
        case 4:
          this.ProjectLimitText = (TextBlock) target;
          break;
        case 5:
          this.TaskLimitPanel = (StackPanel) target;
          break;
        case 6:
          this.TaskLimitText = (TextBlock) target;
          break;
        case 7:
          this.ShareLimitPanel = (StackPanel) target;
          break;
        case 8:
          this.ShareLimitText = (TextBlock) target;
          break;
        case 9:
          this.ReminderText = (TextBlock) target;
          break;
        case 10:
          this.DontShowCheckBox = (CheckBox) target;
          break;
        case 11:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.StartUpdate);
          break;
        case 12:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CloseWindow);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
