// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomControl.CustomComboBox
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.CustomControl
{
  public class CustomComboBox : Grid, IComponentConnector
  {
    public static readonly DependencyProperty TabSelectedProperty = DependencyProperty.Register(nameof (TabSelected), typeof (bool), typeof (CustomComboBox), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof (ItemsSource), typeof (ObservableCollection<ComboBoxViewModel>), typeof (CustomComboBox), new PropertyMetadata((object) null, new PropertyChangedCallback(CustomComboBox.ItemsSourceChanged)));
    private bool _mouseDown;
    internal CustomComboBox Root;
    internal EmjTextBlock SelectedText;
    internal EscPopup ListPopup;
    internal UpDownSelectListView ListView;
    private bool _contentLoaded;

    private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is CustomComboBox customComboBox) || !(e.NewValue is ObservableCollection<ComboBoxViewModel> newValue))
        return;
      customComboBox.Init<ComboBoxViewModel>(newValue, (ComboBoxViewModel) null);
    }

    public ObservableCollection<ComboBoxViewModel> ItemsSource
    {
      get
      {
        return (ObservableCollection<ComboBoxViewModel>) this.GetValue(CustomComboBox.ItemsSourceProperty);
      }
      set => this.SetValue(CustomComboBox.ItemsSourceProperty, (object) value);
    }

    public bool TabSelected
    {
      get => (bool) this.GetValue(CustomComboBox.TabSelectedProperty);
      set => this.SetValue(CustomComboBox.TabSelectedProperty, (object) value);
    }

    public event EventHandler<ComboBoxViewModel> ItemSelected;

    public ComboBoxViewModel SelectedItem { get; set; }

    public bool IsOpen
    {
      get => this.ListPopup.IsOpen;
      set => this.ListPopup.IsOpen = value;
    }

    public ObservableCollection<ComboBoxViewModel> Items
    {
      get => this.ListView.ItemsSource as ObservableCollection<ComboBoxViewModel>;
    }

    public bool NeedClearCapture { get; set; }

    public int SelectedIndex
    {
      get
      {
        if (this.SelectedItem == null)
          return 0;
        ObservableCollection<ComboBoxViewModel> items = this.Items;
        return items == null ? 0 : __nonvirtual (items.IndexOf(this.SelectedItem));
      }
    }

    public CustomComboBox() => this.InitializeComponent();

    public void Init<T>(ObservableCollection<T> items, T selected) where T : ComboBoxViewModel
    {
      if ((object) selected == null)
      {
        selected = items.FirstOrDefault<T>((Func<T, bool>) (i => i.Selected)) ?? items[0];
        selected.Selected = true;
      }
      this.ListView.ItemsSource = (IEnumerable) items;
      this.SelectedItem = (ComboBoxViewModel) selected;
      this.SelectedText.Text = selected.Title;
    }

    public void Init<T>(ObservableCollection<T> items) where T : ComboBoxViewModel
    {
      this.ListView.ItemsSource = (IEnumerable) items;
    }

    public ComboBoxViewModel GetSelectItemOrDefault()
    {
      ComboBoxViewModel selectedItem = this.SelectedItem;
      if (selectedItem != null)
        return selectedItem;
      ObservableCollection<ComboBoxViewModel> items = this.Items;
      // ISSUE: explicit non-virtual call
      return (items != null ? (__nonvirtual (items.Count) > 0 ? 1 : 0) : 0) == 0 ? (ComboBoxViewModel) null : this.Items[0];
    }

    public void SetSelected(object selected)
    {
      foreach (ComboBoxViewModel comboBoxViewModel in (Collection<ComboBoxViewModel>) this.Items)
      {
        comboBoxViewModel.Selected = comboBoxViewModel.Value.Equals(selected);
        if (comboBoxViewModel.Selected)
        {
          this.SelectedItem = comboBoxViewModel;
          this.SelectedText.Text = comboBoxViewModel.Title;
        }
      }
    }

    private void OnItemSelected(bool onEnter, UpDownSelectViewModel e)
    {
      if (e is ComboBoxViewModel e1)
      {
        if (e1.CanSelect)
        {
          if (this.Items != null)
          {
            foreach (UpDownSelectViewModel downSelectViewModel in (Collection<ComboBoxViewModel>) this.Items)
              downSelectViewModel.Selected = false;
          }
          e1.Selected = true;
          this.SelectedItem = e1;
          this.SelectedText.Text = e1.Title;
        }
        EventHandler<ComboBoxViewModel> itemSelected = this.ItemSelected;
        if (itemSelected != null)
          itemSelected((object) null, e1);
      }
      this.ListPopup.IsOpen = false;
    }

    private void ShowPopup(object sender, MouseButtonEventArgs e)
    {
      if (!this._mouseDown)
        return;
      this._mouseDown = false;
      if (this.NeedClearCapture)
        Mouse.Capture((IInputElement) null);
      this.ListPopup.IsOpen = true;
      ObservableCollection<ComboBoxViewModel> items = this.Items;
      // ISSUE: explicit non-virtual call
      if ((items != null ? (__nonvirtual (items.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        this.ListView.ScrollIntoView((object) this.Items.Last<ComboBoxViewModel>());
        this.UpdateLayout();
        ComboBoxViewModel comboBoxViewModel = this.SelectedItem ?? this.Items.First<ComboBoxViewModel>();
        int num = this.Items.IndexOf(comboBoxViewModel);
        this.ListView.ScrollIntoView(num > 0 ? (object) this.Items[num - 1] : (object) comboBoxViewModel);
      }
      e.Handled = true;
    }

    public bool HandleEnter()
    {
      if (this.ListPopup.IsOpen)
        return this.ListPopup.HandleEnter();
      this.ListPopup.IsOpen = true;
      if (this.Items.Count > 0)
      {
        this.ListView.ScrollIntoView((object) this.Items.Last<ComboBoxViewModel>());
        this.UpdateLayout();
        ComboBoxViewModel comboBoxViewModel = this.SelectedItem ?? this.Items.First<ComboBoxViewModel>();
        int num = this.Items.IndexOf(comboBoxViewModel);
        this.ListView.ScrollIntoView(num > 0 ? (object) this.Items[num - 1] : (object) comboBoxViewModel);
        this.ListView.ClearHover();
        comboBoxViewModel.HoverSelected = true;
      }
      return true;
    }

    public void UpDownSelect(bool isUp) => this.ListView.UpDownSelect(isUp);

    public void Close() => this.ListPopup.IsOpen = false;

    public void SetPopupPosition(PlacementMode placement, int h, int v)
    {
      this.ListPopup.Placement = placement;
      this.ListPopup.VerticalOffset = (double) v;
      this.ListPopup.HorizontalOffset = (double) h;
    }

    private void MouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this.ListPopup.IsOpen)
        return;
      this._mouseDown = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/customcontrol/customcombobox.xaml", UriKind.Relative));
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
          this.Root = (CustomComboBox) target;
          break;
        case 2:
          ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.MouseDown);
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.ShowPopup);
          break;
        case 3:
          this.SelectedText = (EmjTextBlock) target;
          break;
        case 4:
          this.ListPopup = (EscPopup) target;
          break;
        case 5:
          this.ListView = (UpDownSelectListView) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
