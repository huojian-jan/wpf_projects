// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.NumInputTextBox
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class NumInputTextBox : Grid, IComponentConnector
  {
    internal NumInputTextBox Root;
    internal TextBox InputText;
    internal Border UpBorder;
    private bool _contentLoaded;

    public int MaxNum { get; set; } = 100000;

    public int MinNum { get; set; }

    public string Text
    {
      get => this.InputText.Text;
      set => this.InputText.Text = value;
    }

    public NumInputTextBox() => this.InitializeComponent();

    private void OnNumPreviewInput(object sender, TextCompositionEventArgs e)
    {
      if (e.Text.Length < 1 || char.IsDigit(e.Text, e.Text.Length - 1) || !(e.Text != "."))
        return;
      e.Handled = true;
    }

    private async void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      int result1;
      int.TryParse(this.InputText.Text, out result1);
      int num;
      if (result1 > this.MaxNum)
      {
        TextBox inputText = this.InputText;
        num = this.MaxNum;
        string str = num.ToString() ?? "";
        inputText.Text = str;
        this.InputText.SelectAll();
      }
      if (result1 >= this.MinNum)
        return;
      await Task.Delay(1200);
      int result2;
      int.TryParse(this.InputText.Text, out result2);
      if (result2 >= this.MinNum)
        return;
      TextBox inputText1 = this.InputText;
      num = this.MinNum;
      string str1 = num.ToString() ?? "";
      inputText1.Text = str1;
      this.InputText.SelectAll();
    }

    private void OnUpClick(object sender, MouseButtonEventArgs e)
    {
      int result;
      int.TryParse(this.InputText.Text, out result);
      int num = result + 1;
      if (num <= this.MaxNum)
        this.InputText.Text = num.ToString() ?? "";
      else
        this.InputText.SelectAll();
    }

    private void OnDownClick(object sender, MouseButtonEventArgs e)
    {
      int result;
      int.TryParse(this.InputText.Text, out result);
      int num = result - 1;
      if (num >= this.MinNum)
        this.InputText.Text = num.ToString() ?? "";
      else
        this.InputText.SelectAll();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/numinputtextbox.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (NumInputTextBox) target;
          break;
        case 2:
          this.InputText = (TextBox) target;
          this.InputText.PreviewTextInput += new TextCompositionEventHandler(this.OnNumPreviewInput);
          this.InputText.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          break;
        case 3:
          this.UpBorder = (Border) target;
          this.UpBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnUpClick);
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
