// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.PreviewTemplateWindow
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
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class PreviewTemplateWindow : Window, IComponentConnector, IStyleConnector
  {
    private bool _contentLoaded;

    public event EventHandler<TemplateViewModel> AddTask;

    public PreviewTemplateWindow(TemplateViewModel model)
    {
      this.DataContext = (object) model;
      this.InitializeComponent();
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding(WindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
    }

    private void ApplyTemplateClick(object sender, RoutedEventArgs e)
    {
      EventHandler<TemplateViewModel> addTask = this.AddTask;
      if (addTask != null)
        addTask((object) null, this.DataContext as TemplateViewModel);
      this.Close();
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnTagLoaded(object sender, RoutedEventArgs e)
    {
      switch (sender)
      {
        case Border border:
          SolidColorBrush solidColorBrush = new SolidColorBrush()
          {
            Color = ThemeUtil.GetColor("PrimaryColor").Color
          };
          solidColorBrush.Opacity = 0.56;
          border.Background = (Brush) solidColorBrush;
          break;
        case TextBlock textBlock:
          textBlock.Text = TagDataHelper.GetTagDisplayName(textBlock.Text);
          SolidColorBrush colorInString = ThemeUtil.GetColorInString(LocalSettings.Settings.ThemeId == "Dark" ? "#B2FFFFFF" : "#CC191919");
          textBlock.Foreground = (Brush) colorInString;
          break;
      }
    }

    private void HowToModifyClick(object sender, MouseButtonEventArgs e)
    {
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("HowToModifyTemplate"), Utils.GetString("HowToModifyTemplateContent"), Utils.GetString("GotIt"), "");
      customerDialog.Owner = (Window) this;
      customerDialog.ShowDialog();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/previewtemplatewindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 3)
      {
        if (connectionId == 4)
          ((ButtonBase) target).Click += new RoutedEventHandler(this.ApplyTemplateClick);
        else
          this._contentLoaded = true;
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.HowToModifyClick);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId != 2)
          return;
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnTagLoaded);
      }
      else
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnTagLoaded);
    }
  }
}
