// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PomoAbandonDialog
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
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PomoAbandonDialog : MyWindow, IOkCancelWindow, IComponentConnector
  {
    public bool? Saved;
    internal TextBlock TitleTextBlock;
    internal TextBlock ContentTextBlock;
    internal Button SaveButton;
    internal Border CloseButton;
    private bool _contentLoaded;

    public PomoAbandonDialog(bool taskSelected, bool over5Min)
    {
      this.InitializeComponent();
      this.SaveButton.Visibility = over5Min ? Visibility.Visible : Visibility.Collapsed;
      this.TitleTextBlock.Text = Utils.GetString(over5Min ? "EndPomoTitle" : "AbandonPomoTitle");
      this.ContentTextBlock.Text = Utils.GetString(over5Min ? "EndPomoContent" : "AbandonPomoContent");
      this.SaveButton.Content = (object) Utils.GetString("EndAndSave");
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
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnDropClick(object sender, RoutedEventArgs e)
    {
      this.DialogResult = new bool?(false);
      this.Saved = new bool?(false);
      this.Close();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      this.DialogResult = new bool?(true);
      this.Saved = new bool?(true);
      this.Close();
    }

    public async void OnCancel() => this.Close();

    public void Ok()
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/pomoabandondialog.xaml", UriKind.Relative));
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
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnDropClick);
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCloseClick);
          break;
        case 6:
          this.CloseButton = (Border) target;
          this.CloseButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
