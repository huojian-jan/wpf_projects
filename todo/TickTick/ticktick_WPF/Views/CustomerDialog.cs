// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomerDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views
{
  public class CustomerDialog : MyWindow, IOkCancelWindow, IComponentConnector
  {
    internal TextBlock TitleTextBlock;
    internal TextBlock ContentTextBlock;
    internal TextBlock ContentComplexTextBlock;
    internal Run CenPreText;
    internal Run CenCenterText;
    internal Run CenPostText;
    internal StackPanel EmphasizeTextBlock;
    internal TextBlock EmphasizeText;
    internal Button OkButton;
    internal Button CancelButton;
    internal Border CloseButton;
    private bool _contentLoaded;

    public event EventHandler TextClick;

    public event EventHandler CloseClick;

    public CustomerDialog() => this.InitializeComponent();

    public CustomerDialog(
      string title,
      string content,
      MessageBoxButton messageBoxButton,
      Window owner = null)
    {
      this.InitializeComponent();
      this.TitleTextBlock.Text = title;
      this.Title = string.IsNullOrEmpty(title) ? "  " : title;
      this.ContentTextBlock.Text = content;
      switch (messageBoxButton)
      {
        case MessageBoxButton.OKCancel:
        case MessageBoxButton.YesNoCancel:
        case MessageBoxButton.YesNo:
          this.OkButton.Visibility = Visibility.Visible;
          this.CancelButton.Visibility = Visibility.Visible;
          break;
        default:
          this.OkButton.Visibility = Visibility.Visible;
          this.CancelButton.Visibility = Visibility.Collapsed;
          break;
      }
      if (owner != null)
      {
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        if (!owner.Activate())
          return;
        this.Owner = owner;
      }
      else
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    public CustomerDialog(
      string title,
      string pretext,
      string centertext,
      string posttext,
      string okbuttonText)
    {
      this.InitializeComponent();
      this.TitleTextBlock.Text = title;
      this.Title = string.IsNullOrEmpty(title) ? "  " : title;
      this.ContentTextBlock.Visibility = Visibility.Collapsed;
      this.ContentComplexTextBlock.Visibility = Visibility.Visible;
      this.CenPreText.Text = pretext;
      this.CenCenterText.Text = centertext;
      this.CenPostText.Text = posttext;
      this.OkButton.Content = (object) okbuttonText;
      this.CancelButton.Visibility = Visibility.Collapsed;
    }

    public CustomerDialog(
      string title,
      string content,
      string okButtonText,
      string cancelButtonText,
      Window owner = null,
      bool showClose = true)
    {
      this.InitializeComponent();
      this.TitleTextBlock.Text = title;
      this.Title = string.IsNullOrEmpty(title) ? "  " : title;
      this.TitleTextBlock.Visibility = string.IsNullOrEmpty(title) ? Visibility.Collapsed : Visibility.Visible;
      this.ContentTextBlock.Text = content;
      this.OkButton.Content = (object) okButtonText;
      this.CancelButton.Content = (object) cancelButtonText;
      if (string.IsNullOrEmpty(cancelButtonText))
        this.CancelButton.Visibility = Visibility.Collapsed;
      if (!showClose)
        this.CloseButton.Visibility = Visibility.Collapsed;
      if (owner != null)
      {
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        if (!owner.Activate())
          return;
        this.Owner = owner;
      }
      else
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    public void SetEmphasizeText(string text)
    {
      this.EmphasizeTextBlock.Visibility = Visibility.Visible;
      this.EmphasizeText.Text = text;
    }

    public CustomerDialog(
      string title,
      List<Inline> content,
      string okButtonText,
      string cancelButtonText,
      Window owner = null)
    {
      this.InitializeComponent();
      this.TitleTextBlock.Text = title;
      this.TitleTextBlock.Visibility = string.IsNullOrEmpty(title) ? Visibility.Collapsed : Visibility.Visible;
      if (!this.TitleTextBlock.IsVisible)
        this.ContentTextBlock.Margin = new Thickness(0.0, 20.0, 0.0, 10.0);
      this.Title = string.IsNullOrEmpty(title) ? "  " : title;
      this.ContentTextBlock.Inlines.Clear();
      this.ContentTextBlock.Inlines.AddRange((IEnumerable) content);
      this.OkButton.Content = (object) okButtonText;
      this.CancelButton.Content = (object) cancelButtonText;
      if (string.IsNullOrEmpty(cancelButtonText))
        this.CancelButton.Visibility = Visibility.Collapsed;
      if (owner != null)
      {
        this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        this.Owner = owner;
      }
      else
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Cancel();

    protected virtual void Cancel()
    {
      this.DialogResult = new bool?(false);
      this.Close();
    }

    protected virtual void OnOkClick(object sender, RoutedEventArgs e) => this.OnOk();

    protected virtual void OnOk()
    {
      this.DialogResult = new bool?(true);
      this.Close();
    }

    protected virtual async void OnCloseClick(object sender, MouseButtonEventArgs e)
    {
      CustomerDialog sender1 = this;
      if (sender1.CloseClick != null)
      {
        sender1.CloseClick((object) sender1, (EventArgs) null);
        await Task.Delay(10);
      }
      sender1.Close();
    }

    private void OnCenterTextClick(object sender, RoutedEventArgs routedEventArgs)
    {
      EventHandler textClick = this.TextClick;
      if (textClick == null)
        return;
      textClick((object) this, (EventArgs) null);
    }

    public async void OnCancel()
    {
      CustomerDialog sender = this;
      if (sender.CloseClick != null)
      {
        sender.CloseClick((object) sender, (EventArgs) null);
        await Task.Delay(10);
      }
      sender.Close();
    }

    public void Ok() => this.OnOk();

    protected override void OnClosed(EventArgs eventArgs)
    {
      try
      {
        if (this.Owner == null)
          return;
        this.Owner.Activate();
      }
      catch (Exception ex)
      {
      }
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

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/customerdialog.xaml", UriKind.Relative));
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
          this.ContentComplexTextBlock = (TextBlock) target;
          break;
        case 4:
          this.CenPreText = (Run) target;
          break;
        case 5:
          this.CenCenterText = (Run) target;
          this.CenCenterText.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCenterTextClick);
          break;
        case 6:
          this.CenPostText = (Run) target;
          break;
        case 7:
          this.EmphasizeTextBlock = (StackPanel) target;
          break;
        case 8:
          this.EmphasizeText = (TextBlock) target;
          break;
        case 9:
          this.OkButton = (Button) target;
          this.OkButton.Click += new RoutedEventHandler(this.OnOkClick);
          break;
        case 10:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 11:
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
