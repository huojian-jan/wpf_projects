// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CountSpinner
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

#nullable disable
namespace ticktick_WPF.Views
{
  public class CountSpinner : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty CountDependencyProperty = DependencyProperty.Register(nameof (Count), typeof (int), typeof (CountSpinner), new PropertyMetadata((object) 0, (PropertyChangedCallback) null));
    internal CountSpinner Root;
    internal TextBox CountTextBox;
    private bool _contentLoaded;

    public int Count
    {
      get => (int) this.GetValue(CountSpinner.CountDependencyProperty);
      set => this.SetValue(CountSpinner.CountDependencyProperty, (object) value);
    }

    private int Min { get; } = 1;

    private int Max { get; } = 60;

    public CountSpinner()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.CountTextBox.TextChanged += new TextChangedEventHandler(this.OnCountTextChanged);
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
    }

    private void OnUpClick(object sender, MouseButtonEventArgs e) => this.CountUp();

    private void CountUp() => this.Count = Math.Min(this.Count + 1, this.Max);

    private void OnDownClick(object sender, MouseButtonEventArgs e) => this.CountDown();

    private void CountDown() => this.Count = Math.Max(this.Count - 1, this.Min);

    private void OnCountTextChanged(object sender, TextChangedEventArgs e)
    {
      int result;
      if ((!int.TryParse(this.CountTextBox.Text, out result) || result < this.Min ? 0 : (result <= this.Max ? 1 : 0)) == 0)
      {
        this.CountTextBox.Text = "5";
        this.Count = 5;
      }
      else
        this.Count = result;
    }

    private void OnTextKeyUp(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Up:
          this.CountUp();
          break;
        case Key.Down:
          this.CountDown();
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/countspinner.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (CountSpinner) target;
          break;
        case 2:
          this.CountTextBox = (TextBox) target;
          this.CountTextBox.KeyUp += new KeyEventHandler(this.OnTextKeyUp);
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnUpClick);
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDownClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
