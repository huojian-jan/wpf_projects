// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.SetPriorityDialog
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
namespace ticktick_WPF.Views
{
  public class SetPriorityDialog : UserControl, ITabControl, IComponentConnector
  {
    public static readonly DependencyProperty PriorityProperty = DependencyProperty.Register(nameof (Priority), typeof (int), typeof (SetPriorityDialog), new PropertyMetadata((object) -1));
    public static readonly DependencyProperty HoverPriorityProperty = DependencyProperty.Register(nameof (HoverPriority), typeof (int), typeof (SetPriorityDialog), new PropertyMetadata((object) 5));
    private readonly Popup _popup;
    internal SetPriorityDialog Root;
    private bool _contentLoaded;

    public int Priority
    {
      get => (int) this.GetValue(SetPriorityDialog.PriorityProperty);
      set => this.SetValue(SetPriorityDialog.PriorityProperty, (object) value);
    }

    public int HoverPriority
    {
      get => (int) this.GetValue(SetPriorityDialog.HoverPriorityProperty);
      set => this.SetValue(SetPriorityDialog.HoverPriorityProperty, (object) value);
    }

    public SetPriorityDialog() => this.InitializeComponent();

    public SetPriorityDialog(int priority)
    {
      this.InitializeComponent();
      this.Priority = priority;
      this.HoverPriority = priority;
    }

    public SetPriorityDialog(Popup popup, int priority = 0)
    {
      this.InitializeComponent();
      this._popup = popup;
      this.Priority = priority;
      this.HoverPriority = priority;
    }

    public event EventHandler<int> PrioritySelect;

    public void Show()
    {
      if (this._popup == null)
        return;
      this._popup.Child = (UIElement) this;
      this._popup.IsOpen = true;
    }

    private async void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      SetPriorityDialog sender1 = this;
      if (sender1._popup != null)
        sender1._popup.IsOpen = false;
      if (!(sender is FrameworkElement frameworkElement))
        return;
      int e1 = int.Parse(frameworkElement.Tag.ToString());
      EventHandler<int> prioritySelect = sender1.PrioritySelect;
      if (prioritySelect != null)
        prioritySelect((object) sender1, e1);
      sender1.PrioritySelect = (EventHandler<int>) null;
    }

    private void OnItemMouseEnter(object sender, MouseEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement))
        return;
      this.HoverPriority = int.Parse(frameworkElement.Tag.ToString());
    }

    public void MoveHover(bool isUp)
    {
      switch (this.HoverPriority)
      {
        case 0:
          this.HoverPriority = isUp ? 1 : 5;
          break;
        case 1:
          this.HoverPriority = isUp ? 3 : 0;
          break;
        case 3:
          this.HoverPriority = isUp ? 5 : 1;
          break;
        case 5:
          this.HoverPriority = isUp ? 0 : 3;
          break;
      }
    }

    public void EnterSelect()
    {
      this.Priority = this.HoverPriority;
      EventHandler<int> prioritySelect = this.PrioritySelect;
      if (prioritySelect == null)
        return;
      prioritySelect((object) this, this.Priority);
    }

    public bool HandleTab(bool shift)
    {
      this.MoveHover(shift);
      return true;
    }

    public bool HandleEnter()
    {
      this.EnterSelect();
      return true;
    }

    public bool HandleEsc() => false;

    public bool UpDownSelect(bool isUp)
    {
      this.MoveHover(isUp);
      return true;
    }

    public bool LeftRightSelect(bool isLeft) => false;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setprioritydialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (SetPriorityDialog) target;
          break;
        case 2:
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnItemMouseEnter);
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseLeftButtonUp);
          break;
        case 4:
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnItemMouseEnter);
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseLeftButtonUp);
          break;
        case 6:
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnItemMouseEnter);
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseLeftButtonUp);
          break;
        case 8:
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnItemMouseEnter);
          break;
        case 9:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseLeftButtonUp);
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnItemMouseEnter);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
