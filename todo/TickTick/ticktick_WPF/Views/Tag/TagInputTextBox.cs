// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagInputTextBox
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
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class TagInputTextBox : Grid, IComponentConnector
  {
    internal TextBlock HintText;
    internal TextBox InputText;
    private bool _contentLoaded;

    public event EventHandler<KeyEventArgs> TextKeyDown;

    public event EventHandler<KeyEventArgs> TextKeyUp;

    public event EventHandler<string> TextChanged;

    public TagInputTextBox() => this.InitializeComponent();

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      EventHandler<KeyEventArgs> textKeyDown = this.TextKeyDown;
      if (textKeyDown == null)
        return;
      textKeyDown(sender, e);
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      EventHandler<KeyEventArgs> textKeyUp = this.TextKeyUp;
      if (textKeyUp != null)
        textKeyUp(sender, e);
      if (e.Key != Key.Up && e.Key != Key.Down)
        return;
      e.Handled = true;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      if (string.IsNullOrEmpty(this.InputText.Text))
      {
        this.HintText.Text = Utils.GetString("InputTag");
        this.HintText.Visibility = Visibility.Visible;
      }
      else
      {
        this.HintText.Text = this.InputText.Text;
        this.HintText.Visibility = Visibility.Hidden;
      }
      if (this.DataContext is SelectedTagViewModel dataContext && dataContext.IsAdd)
        dataContext.Tag = this.InputText.Text;
      EventHandler<string> textChanged = this.TextChanged;
      if (textChanged == null)
        return;
      textChanged(sender, this.InputText.Text);
    }

    private async void OnDataBinded(object sender, DependencyPropertyChangedEventArgs e)
    {
      TagInputTextBox tagInputTextBox = this;
      if (!(tagInputTextBox.DataContext is SelectedTagViewModel model))
        model = (SelectedTagViewModel) null;
      else if (!model.IsAdd)
      {
        model = (SelectedTagViewModel) null;
      }
      else
      {
        await Task.Delay(150);
        tagInputTextBox.InputText.Text = model.Tag;
        tagInputTextBox.InputText.Focus();
        model = (SelectedTagViewModel) null;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/taginputtextbox.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBinded);
          break;
        case 2:
          this.HintText = (TextBlock) target;
          break;
        case 3:
          this.InputText = (TextBox) target;
          this.InputText.PreviewKeyUp += new KeyEventHandler(this.OnKeyUp);
          this.InputText.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
          this.InputText.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
