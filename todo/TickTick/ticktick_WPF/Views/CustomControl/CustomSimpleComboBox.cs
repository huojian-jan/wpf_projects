// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomControl.CustomSimpleComboBox
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
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
  public class CustomSimpleComboBox : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty TabSelectedProperty = DependencyProperty.Register(nameof (TabSelected), typeof (bool), typeof (CustomSimpleComboBox), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof (SelectedIndex), typeof (int), typeof (CustomSimpleComboBox), new PropertyMetadata((object) -1, new PropertyChangedCallback(CustomSimpleComboBox.SelectedIndexChanged)));
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof (ItemsSource), typeof (List<string>), typeof (CustomSimpleComboBox), new PropertyMetadata((object) null, new PropertyChangedCallback(CustomSimpleComboBox.ItemsSourceChanged)));
    private bool _mouseDown;
    internal CustomSimpleComboBox Root;
    internal StackPanel Panel;
    internal EmjTextBlock SelectedText;
    internal EscPopup ListPopup;
    internal UpDownSelectListView ListView;
    private bool _contentLoaded;

    private static void ItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is CustomSimpleComboBox customSimpleComboBox) || !(e.NewValue is List<string> newValue))
        return;
      customSimpleComboBox.Init(newValue);
    }

    private static void SelectedIndexChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is CustomSimpleComboBox customSimpleComboBox) || !(e.NewValue is int newValue))
        return;
      customSimpleComboBox.SetSelectedIndex(newValue);
    }

    public int SelectedIndex
    {
      get => (int) this.GetValue(CustomSimpleComboBox.SelectedIndexProperty);
      set => this.SetValue(CustomSimpleComboBox.SelectedIndexProperty, (object) value);
    }

    public List<string> ItemsSource
    {
      get => (List<string>) this.GetValue(CustomSimpleComboBox.ItemsSourceProperty);
      set => this.SetValue(CustomSimpleComboBox.ItemsSourceProperty, (object) value);
    }

    public bool TabSelected
    {
      get => (bool) this.GetValue(CustomSimpleComboBox.TabSelectedProperty);
      set => this.SetValue(CustomSimpleComboBox.TabSelectedProperty, (object) value);
    }

    public event EventHandler<SimpleComboBoxViewModel> ItemSelected;

    public SimpleComboBoxViewModel SelectedItem { get; set; }

    public bool IsOpen
    {
      get => this.ListPopup.IsOpen;
      set => this.ListPopup.IsOpen = value;
    }

    private ObservableCollection<SimpleComboBoxViewModel> Items
    {
      get => this.ListView.ItemsSource as ObservableCollection<SimpleComboBoxViewModel>;
    }

    public bool NeedClearCapture { get; set; }

    public CustomSimpleComboBox() => this.InitializeComponent();

    public void Init(List<string> items)
    {
      ObservableCollection<SimpleComboBoxViewModel> observableCollection = new ObservableCollection<SimpleComboBoxViewModel>();
      foreach (string text in items)
      {
        SimpleComboBoxViewModel comboBoxViewModel = new SimpleComboBoxViewModel(text);
        observableCollection.Add(comboBoxViewModel);
      }
      this.ListView.ItemsSource = (IEnumerable) observableCollection;
      if (this.SelectedIndex >= 0 && this.SelectedIndex < observableCollection.Count)
      {
        SimpleComboBoxViewModel comboBoxViewModel = observableCollection[this.SelectedIndex];
        this.SelectedItem = comboBoxViewModel;
        this.SelectedItem.Selected = true;
        this.SelectedText.Text = comboBoxViewModel.Title;
      }
      else
      {
        if (observableCollection.Count <= 0)
          return;
        this.SelectedItem = observableCollection[0];
        this.SelectedItem.Selected = true;
        this.SelectedText.Text = this.SelectedItem.Title;
      }
    }

    private void SetSelectedIndex(int index)
    {
      this.SelectedIndex = index;
      ObservableCollection<SimpleComboBoxViewModel> items = this.Items;
      if (items == null || index >= items.Count)
        return;
      foreach (UpDownSelectViewModel downSelectViewModel in (Collection<SimpleComboBoxViewModel>) this.Items)
        downSelectViewModel.Selected = false;
      items[index].Selected = true;
      this.SelectedText.Text = items[index].Title;
    }

    public SimpleComboBoxViewModel GetSelectItemOrDefault()
    {
      SimpleComboBoxViewModel selectedItem = this.SelectedItem;
      if (selectedItem != null)
        return selectedItem;
      ObservableCollection<SimpleComboBoxViewModel> items = this.Items;
      // ISSUE: explicit non-virtual call
      return (items != null ? (__nonvirtual (items.Count) > 0 ? 1 : 0) : 0) == 0 ? (SimpleComboBoxViewModel) null : this.Items[0];
    }

    private void OnItemSelected(bool onEnter, UpDownSelectViewModel e)
    {
      if (e is SimpleComboBoxViewModel e1)
      {
        if (this.Items != null)
        {
          foreach (UpDownSelectViewModel downSelectViewModel in (Collection<SimpleComboBoxViewModel>) this.Items)
            downSelectViewModel.Selected = false;
        }
        e1.Selected = true;
        this.SelectedItem = e1;
        this.SelectedText.Text = e1.Title;
        this.SelectedIndex = this.Items.IndexOf(e1);
        EventHandler<SimpleComboBoxViewModel> itemSelected = this.ItemSelected;
        if (itemSelected != null)
          itemSelected((object) this, e1);
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
      if (this.Items.Count > 0)
      {
        this.ListView.ScrollIntoView((object) this.Items.Last<SimpleComboBoxViewModel>());
        this.UpdateLayout();
        SimpleComboBoxViewModel comboBoxViewModel = this.SelectedItem ?? this.Items.First<SimpleComboBoxViewModel>();
        int num = this.Items.IndexOf(comboBoxViewModel);
        this.ListView.ScrollIntoView(num > 0 ? (object) this.Items[num - 1] : (object) comboBoxViewModel);
      }
      e.Handled = true;
    }

    public bool HandleEnter()
    {
      if (this.ListPopup.IsOpen)
        return this.ListPopup.HandleEnter();
      this._mouseDown = true;
      this.ListPopup.IsOpen = true;
      if (this.Items.Count > 0)
      {
        this.ListView.ScrollIntoView((object) this.Items.Last<SimpleComboBoxViewModel>());
        this.UpdateLayout();
        SimpleComboBoxViewModel comboBoxViewModel = this.SelectedItem ?? this.Items.First<SimpleComboBoxViewModel>();
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
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/customcontrol/customsimplecombobox.xaml", UriKind.Relative));
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
          this.Root = (CustomSimpleComboBox) target;
          break;
        case 2:
          ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.MouseDown);
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.ShowPopup);
          break;
        case 3:
          this.Panel = (StackPanel) target;
          break;
        case 4:
          this.SelectedText = (EmjTextBlock) target;
          break;
        case 5:
          this.ListPopup = (EscPopup) target;
          break;
        case 6:
          this.ListView = (UpDownSelectListView) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
