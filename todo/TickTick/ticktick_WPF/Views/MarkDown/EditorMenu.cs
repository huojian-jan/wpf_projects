// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.EditorMenu
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class EditorMenu : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty IsImmersiveModeProperty = DependencyProperty.Register(nameof (IsImmersiveMode), typeof (bool), typeof (EditorMenu), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    public static readonly DependencyProperty IsFlatModeProperty = DependencyProperty.Register(nameof (IsFlatMode), typeof (bool), typeof (EditorMenu), new PropertyMetadata((object) true, (PropertyChangedCallback) null));
    private readonly List<FrameworkElement> _moreOptionElements = new List<FrameworkElement>();
    private DateTime? _headerClosedTime;
    private bool _isPopMode = true;
    internal EditorMenu Root;
    internal ContentControl Content;
    internal ItemsControl EditorPanel;
    internal ContentControl NavigateBackBtn;
    internal ContentControl ExitImmersiveBtn;
    internal ContentControl EnterImmersiveBtn;
    internal Grid ImmersiveSeparator;
    internal ContentControl HeaderButton;
    internal Popup HeaderPopup;
    internal ContentControl DateItem;
    internal Popup DatePopup;
    internal Grid Split;
    internal ContentControl Attachment;
    internal Grid MoreOptionGrid;
    internal ContentControl MoreGrid;
    internal Popup MoreOptionPopup;
    internal ContentControl MoreItalicItem;
    internal ContentControl MoreUnderLineItem;
    internal ContentControl MoreStrikeThroughItem;
    internal ContentControl MoreLineItem;
    internal ContentControl ExtraDateItem;
    internal Popup ExtraDatePopup;
    internal Grid MoreSplit;
    internal ContentControl MoreLink;
    internal ContentControl MoreCodeItem;
    internal ContentControl MoreQuote;
    internal Grid MoreSplit2;
    internal ContentControl MoreAttachment;
    private bool _contentLoaded;

    public EditorMenu()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnEditorMenuLoaded);
    }

    private void OnEditorMenuLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnEditorMenuLoaded);
      this.InitMoreOptions();
      this.InitEvents();
    }

    public bool IsFlatMode
    {
      get => (bool) this.GetValue(EditorMenu.IsFlatModeProperty);
      set => this.SetValue(EditorMenu.IsFlatModeProperty, (object) value);
    }

    public bool IsImmersiveMode
    {
      get => (bool) this.GetValue(EditorMenu.IsImmersiveModeProperty);
      set => this.SetValue(EditorMenu.IsImmersiveModeProperty, (object) value);
    }

    public bool IsPopMode
    {
      get => this._isPopMode;
      set
      {
        this._isPopMode = value;
        this.OnPopModeSet(value);
      }
    }

    private void OnPopModeSet(bool value)
    {
      if (value)
        return;
      this.Content.Style = (Style) null;
      this.NavigateBackBtn.Visibility = Visibility.Collapsed;
      this.ExitImmersiveBtn.Visibility = Visibility.Collapsed;
      this.EnterImmersiveBtn.Visibility = Visibility.Collapsed;
      this.ImmersiveSeparator.Visibility = Visibility.Collapsed;
      this.MoreAttachment.Visibility = Visibility.Collapsed;
      this.MoreSplit2.Visibility = Visibility.Collapsed;
      this.Attachment.Visibility = Visibility.Collapsed;
      this.Split.Visibility = Visibility.Collapsed;
    }

    public event EventHandler EnterImmersive;

    public event EventHandler ExitImmersive;

    public event EventHandler<string> EditorAction;

    public event EventHandler NavigateBack;

    private void InitMoreOptions()
    {
      this._moreOptionElements.Add((FrameworkElement) this.MoreItalicItem);
      this._moreOptionElements.Add((FrameworkElement) this.MoreUnderLineItem);
      this._moreOptionElements.Add((FrameworkElement) this.MoreStrikeThroughItem);
      this._moreOptionElements.Add((FrameworkElement) this.MoreLineItem);
      this._moreOptionElements.Add((FrameworkElement) this.ExtraDateItem);
      this._moreOptionElements.Add((FrameworkElement) this.MoreSplit);
      this._moreOptionElements.Add((FrameworkElement) this.MoreLink);
      this._moreOptionElements.Add((FrameworkElement) this.MoreCodeItem);
      this._moreOptionElements.Add((FrameworkElement) this.MoreQuote);
      if (this._isPopMode)
      {
        this._moreOptionElements.Add((FrameworkElement) this.MoreSplit2);
        this._moreOptionElements.Add((FrameworkElement) this.MoreAttachment);
      }
      this.SetMoreIcons();
    }

    private void InitEvents()
    {
      this.HeaderPopup.Closed -= new EventHandler(this.OnHeaderOpened);
      this.HeaderPopup.Closed += new EventHandler(this.OnHeaderOpened);
      this.DatePopup.Closed -= new EventHandler(this.OnHeaderOpened);
      this.DatePopup.Closed += new EventHandler(this.OnHeaderOpened);
    }

    private void OnHeaderOpened(object sender, EventArgs e)
    {
      this._headerClosedTime = new DateTime?(DateTime.Now);
    }

    private void OnExitClick(object sender, MouseButtonEventArgs e)
    {
      EventHandler exitImmersive = this.ExitImmersive;
      if (exitImmersive == null)
        return;
      exitImmersive(sender, (EventArgs) e);
    }

    private void OnStyleClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.Tag is string tag))
        return;
      e.Handled = true;
      this.MoreOptionPopup.IsOpen = false;
      this.HeaderPopup.IsOpen = false;
      EventHandler<string> editorAction = this.EditorAction;
      if (editorAction == null)
        return;
      editorAction(sender, tag);
    }

    private void OnExtraDateClick(object sender, MouseButtonEventArgs e)
    {
      this.ShowDatePopup(this.ExtraDatePopup);
      e.Handled = true;
    }

    private void OnDateClick(object sender, MouseButtonEventArgs e)
    {
      this.ShowDatePopup(this.DatePopup);
      e.Handled = true;
    }

    private void ShowDatePopup(Popup datePopup)
    {
      if (datePopup.IsOpen)
      {
        datePopup.IsOpen = false;
      }
      else
      {
        if (this._headerClosedTime.HasValue && (DateTime.Now - this._headerClosedTime.Value).TotalMilliseconds <= 200.0)
          return;
        DateTime now = DateTime.Now;
        string str1 = DateUtils.FormatFullDate(now);
        string timeText = DateUtils.GetTimeText(now);
        string str2 = str1 + " " + timeText;
        string str3 = now.ToString("yyyy/MM/dd");
        CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) new List<CustomMenuItemViewModel>()
        {
          new CustomMenuItemViewModel((object) str2, str2, (Geometry) null)
          {
            FontSize = 11
          },
          new CustomMenuItemViewModel((object) str1, str1, (Geometry) null)
          {
            FontSize = 11
          },
          new CustomMenuItemViewModel((object) str3, str3, (Geometry) null)
          {
            FontSize = 11
          },
          new CustomMenuItemViewModel((object) timeText, timeText, (Geometry) null)
          {
            FontSize = 11
          }
        }, datePopup);
        customMenuList.Operated += (EventHandler<object>) ((o, tag) =>
        {
          EventHandler<string> editorAction = this.EditorAction;
          if (editorAction == null)
            return;
          editorAction((object) null, "DateTime_" + tag?.ToString());
        });
        customMenuList.Show();
      }
    }

    private void OnHeaderClick(object sender, MouseButtonEventArgs e)
    {
      if (this.HeaderPopup.IsOpen)
        this.HeaderPopup.IsOpen = false;
      else if (!this._headerClosedTime.HasValue || (DateTime.Now - this._headerClosedTime.Value).TotalMilliseconds > 200.0)
        this.HeaderPopup.IsOpen = true;
      e.Handled = true;
    }

    private void OnMenuSizeChanged(object sender, SizeChangedEventArgs e) => this.SetMoreIcons();

    private void SetMoreIcons()
    {
      string itemsInFirstRow = EditorMenu.GetItemsInFirstRow(this.EditorPanel);
      this.MoreGrid.Visibility = this.Attachment.Visibility == Visibility.Visible && itemsInFirstRow == "Attachment" || this.Attachment.Visibility != Visibility.Visible && itemsInFirstRow == "Quote" ? Visibility.Collapsed : Visibility.Visible;
      if (!this.MoreGrid.IsVisible)
        return;
      bool flag = true;
      for (int index = this._moreOptionElements.Count - 1; index >= 0; --index)
      {
        if (this._moreOptionElements[index].Tag.ToString() == itemsInFirstRow || this._moreOptionElements[index].Tag.ToString() == "SplitSplitLine" && itemsInFirstRow == "DateTime" || this._moreOptionElements[index].Tag.ToString() == "SplitQuote" && itemsInFirstRow == "Quote")
          flag = false;
        this._moreOptionElements[index].Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    private void OnMoreClick(object sender, MouseButtonEventArgs e)
    {
      this.MoreOptionPopup.IsOpen = !this.MoreOptionPopup.IsOpen;
    }

    private void OnImmersiveClick(object sender, MouseButtonEventArgs e)
    {
      EventHandler enterImmersive = this.EnterImmersive;
      if (enterImmersive == null)
        return;
      enterImmersive((object) this, (EventArgs) null);
    }

    private static string GetItemsInFirstRow(ItemsControl itemsControl)
    {
      double num = -1.0;
      string itemsInFirstRow = string.Empty;
      for (int index = 8; index < itemsControl.Items.Count; ++index)
      {
        FrameworkElement frameworkElement = (FrameworkElement) itemsControl.ItemContainerGenerator.ContainerFromIndex(index);
        if (frameworkElement != null)
        {
          double x = frameworkElement.TranslatePoint(new System.Windows.Point(), (UIElement) itemsControl).X;
          if (x > num || x <= 0.0)
          {
            if (frameworkElement.Tag is string tag && tag != "SplitSplitLine" && tag != "SplitQuote" && frameworkElement.IsVisible)
              itemsInFirstRow = tag;
            num = x;
          }
          else
            break;
        }
        else
          break;
      }
      return itemsInFirstRow;
    }

    public void HideImmersive()
    {
      this.EnterImmersiveBtn.Visibility = Visibility.Collapsed;
      this.ImmersiveSeparator.Visibility = Visibility.Collapsed;
    }

    public void ShowImmersive()
    {
      this.EnterImmersiveBtn.Visibility = Visibility.Visible;
      this.ImmersiveSeparator.Visibility = Visibility.Visible;
    }

    private void OnNavigateBackClick(object sender, MouseButtonEventArgs e)
    {
      EventHandler navigateBack = this.NavigateBack;
      if (navigateBack == null)
        return;
      navigateBack((object) this, (EventArgs) null);
    }

    public void SetReadOnly(bool readOnly)
    {
      for (int index = 2; index < this.EditorPanel.Items.Count; ++index)
      {
        UIElement uiElement = (UIElement) this.EditorPanel.ItemContainerGenerator.ContainerFromIndex(index);
        uiElement.IsEnabled = !readOnly;
        uiElement.Opacity = readOnly ? 0.30000001192092896 : 1.0;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/markdown/editormenu.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (EditorMenu) target;
          this.Root.SizeChanged += new SizeChangedEventHandler(this.OnMenuSizeChanged);
          break;
        case 2:
          this.Content = (ContentControl) target;
          break;
        case 3:
          this.EditorPanel = (ItemsControl) target;
          break;
        case 4:
          this.NavigateBackBtn = (ContentControl) target;
          this.NavigateBackBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnNavigateBackClick);
          break;
        case 5:
          this.ExitImmersiveBtn = (ContentControl) target;
          this.ExitImmersiveBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnExitClick);
          break;
        case 6:
          this.EnterImmersiveBtn = (ContentControl) target;
          this.EnterImmersiveBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnImmersiveClick);
          break;
        case 7:
          this.ImmersiveSeparator = (Grid) target;
          break;
        case 8:
          this.HeaderButton = (ContentControl) target;
          this.HeaderButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnHeaderClick);
          break;
        case 9:
          this.HeaderPopup = (Popup) target;
          break;
        case 10:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 11:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 12:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 13:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 14:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 15:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 16:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 17:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 18:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 19:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 20:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 21:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 22:
          this.DateItem = (ContentControl) target;
          this.DateItem.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDateClick);
          break;
        case 23:
          this.DatePopup = (Popup) target;
          break;
        case 24:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 25:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 26:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 27:
          this.Split = (Grid) target;
          break;
        case 28:
          this.Attachment = (ContentControl) target;
          this.Attachment.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 29:
          this.MoreOptionGrid = (Grid) target;
          break;
        case 30:
          this.MoreGrid = (ContentControl) target;
          break;
        case 31:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
          break;
        case 32:
          this.MoreOptionPopup = (Popup) target;
          break;
        case 33:
          this.MoreItalicItem = (ContentControl) target;
          this.MoreItalicItem.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 34:
          this.MoreUnderLineItem = (ContentControl) target;
          this.MoreUnderLineItem.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 35:
          this.MoreStrikeThroughItem = (ContentControl) target;
          this.MoreStrikeThroughItem.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 36:
          this.MoreLineItem = (ContentControl) target;
          this.MoreLineItem.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 37:
          this.ExtraDateItem = (ContentControl) target;
          this.ExtraDateItem.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnExtraDateClick);
          break;
        case 38:
          this.ExtraDatePopup = (Popup) target;
          break;
        case 39:
          this.MoreSplit = (Grid) target;
          break;
        case 40:
          this.MoreLink = (ContentControl) target;
          this.MoreLink.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 41:
          this.MoreCodeItem = (ContentControl) target;
          this.MoreCodeItem.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 42:
          this.MoreQuote = (ContentControl) target;
          this.MoreQuote.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        case 43:
          this.MoreSplit2 = (Grid) target;
          break;
        case 44:
          this.MoreAttachment = (ContentControl) target;
          this.MoreAttachment.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnStyleClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
