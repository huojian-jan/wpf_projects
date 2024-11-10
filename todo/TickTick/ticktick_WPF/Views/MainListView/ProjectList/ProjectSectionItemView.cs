// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ProjectList.ProjectSectionItemView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.MainListView.ProjectList
{
  public class ProjectSectionItemView : Grid
  {
    private Border _messageText;
    private bool _popupShow;
    private bool _mouseDown;

    public ProjectSectionItemView()
    {
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(22.0)
      });
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this.ColumnDefinitions.Add(new ColumnDefinition());
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this.Cursor = Cursors.Hand;
      this.Background = (Brush) Brushes.Transparent;
      this.SetResourceReference(FrameworkElement.HeightProperty, (object) "Height30");
      this.InitTitle();
      this.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnLoaded);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnItemMouseDown);
    }

    private void OnItemMouseDown(object sender, MouseButtonEventArgs e)
    {
      StackPanel rightOptions = this.GetRightOptions();
      // ISSUE: explicit non-virtual call
      if ((rightOptions != null ? (__nonvirtual (rightOptions.IsMouseOver) ? 1 : 0) : 0) != 0)
        return;
      Border messageText = this._messageText;
      // ISSUE: explicit non-virtual call
      if ((messageText != null ? (__nonvirtual (messageText.IsMouseOver) ? 1 : 0) : 0) != 0)
        return;
      this._mouseDown = true;
    }

    private void OnUnLoaded(object sender, RoutedEventArgs e)
    {
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) ProjectMenuItemHoverModel.Model, new EventHandler<PropertyChangedEventArgs>(this.OnHoverPropertyChanged), "HoverType");
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) ProjectMenuItemHoverModel.Model, new EventHandler<PropertyChangedEventArgs>(this.OnHoverPropertyChanged), "HoverType");
    }

    private void InitTitle()
    {
      TextBlock textBlock = new TextBlock();
      textBlock.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock.VerticalAlignment = VerticalAlignment.Center;
      textBlock.IsHitTestVisible = false;
      textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
      TextBlock element = textBlock;
      element.SetBinding(TextBlock.TextProperty, "Title");
      element.SetResourceReference(TextBlock.FontSizeProperty, (object) "Font12");
      element.SetResourceReference(TextBlock.FontWeightProperty, (object) "BoldToMedium");
      element.SetResourceReference(TextBlock.ForegroundProperty, (object) "ProjectSectionColor");
      element.SetValue(Panel.ZIndexProperty, (object) 10);
      element.SetValue(Grid.ColumnProperty, (object) 1);
      this.Children.Add((UIElement) element);
    }

    private Border GetFoldBorder()
    {
      foreach (object child in this.Children)
      {
        if (child is Border foldBorder && foldBorder.Name == "FoldBorder")
          return foldBorder;
      }
      return (Border) null;
    }

    private void ShowOrHideFoldBorder(bool show, bool open, bool showAdd)
    {
      Border foldBorder = this.GetFoldBorder();
      if (!show)
      {
        if (foldBorder == null)
          return;
        this.Children.Remove((UIElement) foldBorder);
      }
      else
      {
        if (foldBorder == null)
        {
          Border border1 = new Border();
          border1.Name = "FoldBorder";
          border1.Margin = new Thickness(8.0, 0.0, 8.0, 0.0);
          border1.CornerRadius = new CornerRadius(4.0);
          Border element = border1;
          element.SetResourceReference(Panel.BackgroundProperty, (object) "ProjectHoverBackGround");
          element.SetValue(Panel.ZIndexProperty, (object) 0);
          element.SetValue(Grid.ColumnSpanProperty, (object) 4);
          Border border2 = new Border();
          border2.HorizontalAlignment = HorizontalAlignment.Left;
          border2.Margin = new Thickness(2.0, 0.0, 0.0, 0.0);
          Border border3 = border2;
          border3.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle80_100");
          Path path = UiUtils.CreatePath("ArrowLine", "ProjectSectionColor", "Path01");
          path.Width = 12.0;
          path.Height = 12.0;
          path.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
          RotateTransform rotateTransform = new RotateTransform()
          {
            Angle = open ? 0.0 : -90.0
          };
          path.RenderTransform = (Transform) rotateTransform;
          element.Child = (UIElement) border3;
          border3.Child = (UIElement) path;
          this.Children.Add((UIElement) element);
        }
        this.ShowOrHideAddBorder(showAdd);
      }
    }

    private StackPanel GetRightOptions()
    {
      foreach (object child in this.Children)
      {
        if (child is StackPanel rightOptions && rightOptions.Name == "RightPanel")
          return rightOptions;
      }
      return (StackPanel) null;
    }

    private void ShowOrHideAddBorder(bool show)
    {
      if (!(this.DataContext is PtfAllViewModel dataContext))
        show = false;
      else if (dataContext.IsProject && LocalSettings.Settings.ExtraSettings.ShowProjectTimes > 0)
        show = true;
      StackPanel rightOptions = this.GetRightOptions();
      if (!show)
      {
        if (rightOptions == null)
          return;
        this.Children.Remove((UIElement) rightOptions);
      }
      else
      {
        if (rightOptions != null)
          return;
        StackPanel stackPanel = new StackPanel();
        stackPanel.Name = "RightPanel";
        stackPanel.Margin = new Thickness(0.0, 0.0, 10.0, 0.0);
        stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
        stackPanel.Orientation = Orientation.Horizontal;
        StackPanel element1 = stackPanel;
        element1.SetValue(Panel.ZIndexProperty, (object) 10);
        if (dataContext.IsFilter || dataContext.IsTag)
        {
          Button button = new Button();
          button.Width = 22.0;
          button.Height = 22.0;
          Button element2 = button;
          element2.Click += new RoutedEventHandler(this.OnMoreClick);
          element2.SetResourceReference(FrameworkElement.StyleProperty, (object) "OpacityHoverButtonStyle");
          Path path = UiUtils.CreatePath("IcMore", "ProjectMenuColorOpacity80", "Path01");
          path.Width = 16.0;
          path.Height = 16.0;
          element2.Content = (object) path;
          element1.Children.Add((UIElement) element2);
        }
        Button button1 = new Button();
        button1.Width = 22.0;
        button1.Height = 22.0;
        Button element3 = button1;
        element3.Click += new RoutedEventHandler(this.OnAddClick);
        element3.SetResourceReference(FrameworkElement.StyleProperty, (object) "OpacityHoverButtonStyle");
        Path path1 = UiUtils.CreatePath("IcAdd", "ProjectMenuColorOpacity80", "Path01");
        path1.Width = 16.0;
        path1.Height = 16.0;
        element3.Content = (object) path1;
        element1.Children.Add((UIElement) element3);
        element1.SetValue(Grid.ColumnProperty, (object) 3);
        this.Children.Add((UIElement) element1);
      }
    }

    private async void OnMoreClick(object sender, RoutedEventArgs e)
    {
      ProjectSectionItemView projectSectionItemView = this;
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      PtfAllViewModel model = projectSectionItemView.DataContext as PtfAllViewModel;
      if (model != null && (model.IsTag || model.IsFilter))
      {
        IEnumerable<ContextAction> contextActions = await model.GetContextActions();
        List<ContextAction> list = contextActions != null ? contextActions.ToList<ContextAction>() : (List<ContextAction>) null;
        if (list != null)
        {
          int num = model.IsTag ? LocalSettings.Settings.SmartListTag : LocalSettings.Settings.ShowCustomSmartList;
          List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
          foreach (ContextAction contextAction in list)
          {
            CustomMenuItemViewModel menuItemViewModel = new CustomMenuItemViewModel((object) contextAction.ActionKey, contextAction.Title, (Geometry) null);
            menuItemViewModel.Selected = contextAction.ActionKey == ContextActionKey.Show && num == 0 || contextAction.ActionKey == ContextActionKey.Hide && num == 1 || contextAction.ActionKey == ContextActionKey.ShowIfNotEmpty && num == 2;
            types.Add(menuItemViewModel);
          }
          EscPopup escPopup1 = new EscPopup();
          escPopup1.StaysOpen = false;
          escPopup1.Placement = PlacementMode.Mouse;
          escPopup1.PlacementTarget = (UIElement) projectSectionItemView;
          escPopup1.HorizontalOffset = -5.0;
          escPopup1.VerticalOffset = -5.0;
          EscPopup escPopup2 = escPopup1;
          // ISSUE: reference to a compiler-generated method
          escPopup2.Closed += new EventHandler(projectSectionItemView.\u003COnMoreClick\u003Eb__12_0);
          CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) escPopup2);
          customMenuList.Operated += new EventHandler<object>(projectSectionItemView.OnActionSelected);
          projectSectionItemView._popupShow = true;
          customMenuList.Show();
          projectSectionItemView.OnMouseEnter();
        }
      }
      projectSectionItemView.ShowOrHideAddBorder(true);
      model = (PtfAllViewModel) null;
    }

    private void OnActionSelected(object sender, object e)
    {
      if (!(e is ContextActionKey contextActionKey) || !(this.DataContext is PtfAllViewModel dataContext))
        return;
      switch (contextActionKey)
      {
        case ContextActionKey.Show:
          Utils.FindParent<ProjectListView>((DependencyObject) this)?.ShowOrHidePtf(dataContext, 0);
          break;
        case ContextActionKey.Hide:
          Utils.FindParent<ProjectListView>((DependencyObject) this)?.ShowOrHidePtf(dataContext, 1);
          break;
        case ContextActionKey.ShowIfNotEmpty:
          Utils.FindParent<ProjectListView>((DependencyObject) this)?.ShowOrHidePtf(dataContext, 2);
          break;
      }
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._mouseDown)
        return;
      this._mouseDown = false;
      if (!(this.DataContext is ProjectItemViewModel dataContext))
        return;
      Utils.FindParent<ProjectListView>((DependencyObject) this)?.OnPtfAllClick(dataContext);
      if (!(dataContext is TeamGroupViewModel))
        return;
      if (dataContext.Id != "c1a7e08345e444dea187e21a692f0d7a")
        TeamDao.SaveOpenStatus(dataContext.Id, dataContext.Open);
      else
        LocalSettings.Settings.ExpandPersonalSection = dataContext.Open;
    }

    private void OnAddClick(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      if (this.DataContext == null || !(this.DataContext is PtfAllViewModel dataContext))
        return;
      switch (dataContext.Type)
      {
        case PtfType.Project:
          if (LocalSettings.Settings.ExtraSettings.ShowProjectTimes > 0)
            --LocalSettings.Settings.ExtraSettings.ShowProjectTimes;
          Utils.FindParent<ProjectListView>((DependencyObject) this)?.OnAddProjectClick(string.Empty, dataContext.TeamId);
          break;
        case PtfType.Tag:
          Utils.FindParent<ProjectListView>((DependencyObject) this)?.OnAddTagClick();
          break;
        case PtfType.Filter:
          Utils.FindParent<ProjectListView>((DependencyObject) this)?.OnAddFilterClick();
          break;
        case PtfType.Subscribe:
          if (!ProChecker.CheckPro(ProType.SubscribeCalendar))
            break;
          Mouse.Capture((IInputElement) null);
          new AddSubscribeDialog((UIElement) this).Show();
          break;
      }
    }

    private void OnMouseEnter(object sender, MouseEventArgs e) => this.OnMouseEnter();

    private void OnMouseEnter()
    {
      if (!(this.DataContext is PtfAllViewModel dataContext))
        return;
      this.ShowOrHideFoldBorder(true, dataContext.Open, !dataContext.InSubSection);
      ProjectMenuItemHoverModel.Model.HoverType = dataContext.Type;
    }

    private void OnMouseLeave(object sender, MouseEventArgs e) => this.OnMouseLeave();

    private void OnMouseLeave()
    {
      if (!(this.DataContext is ProjectItemViewModel dataContext) || this._popupShow)
        return;
      this.ShowOrHideFoldBorder(false, dataContext.Open, false);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      ProjectItemViewModel oldValue = e.OldValue as ProjectItemViewModel;
      PtfAllViewModel newValue = e.NewValue as PtfAllViewModel;
      if (oldValue != null)
      {
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnOpenChanged), "Open");
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(this.OnDragSelectChanged), "DragSelected");
      }
      if (newValue == null)
        return;
      this.Opacity = (double) (!newValue.DragSelected ? 1 : 0);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnOpenChanged), "Open");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(this.OnDragSelectChanged), "DragSelected");
      this.SetNotifyMessage((ProjectItemViewModel) newValue);
      this.Margin = new Thickness(newValue.InSubSection ? 18.0 : 0.0, newValue.InSubSection ? 4.0 : 8.0, 0.0, 0.0);
      if (!newValue.IsProject || LocalSettings.Settings.ExtraSettings.ShowProjectTimes <= 0)
        return;
      this.ShowOrHideAddBorder(true);
    }

    private void SetNotifyMessage(ProjectItemViewModel model)
    {
      if (!string.IsNullOrEmpty(model.NotifyMessage))
      {
        if (this._messageText == null)
        {
          Border border = new Border();
          border.HorizontalAlignment = HorizontalAlignment.Left;
          border.Height = 16.0;
          border.CornerRadius = new CornerRadius(4.0);
          border.Margin = new Thickness(6.0, 1.0, 0.0, 0.0);
          this._messageText = border;
          this._messageText.SetValue(Grid.ColumnProperty, (object) 2);
          this._messageText.SetResourceReference(Panel.BackgroundProperty, (object) "BaseColorOpacity10");
          this.Children.Add((UIElement) this._messageText);
          TextBlock textBlock1 = new TextBlock();
          textBlock1.HorizontalAlignment = HorizontalAlignment.Left;
          textBlock1.VerticalAlignment = VerticalAlignment.Center;
          textBlock1.IsHitTestVisible = false;
          textBlock1.FontSize = 10.0;
          textBlock1.TextTrimming = TextTrimming.CharacterEllipsis;
          textBlock1.Margin = new Thickness(4.0, 0.0, 4.0, 0.0);
          TextBlock textBlock2 = textBlock1;
          textBlock2.SetBinding(TextBlock.TextProperty, "NotifyMessage");
          textBlock2.SetResourceReference(TextBlock.ForegroundProperty, (object) "ProjectSectionColor");
          this._messageText.SetValue(Panel.ZIndexProperty, (object) 100);
          this._messageText.Child = (UIElement) textBlock2;
          this._messageText.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnMessageMouseUp);
        }
        this._messageText.Visibility = Visibility.Visible;
      }
      else
      {
        if (this._messageText == null)
          return;
        this._messageText.Visibility = Visibility.Collapsed;
      }
    }

    private void OnMessageMouseUp(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (UserDao.IsPro())
        return;
      ProChecker.CheckPro(CacheManager.GetProjectsWithoutInbox().Count<ProjectModel>((Func<ProjectModel, bool>) (p => !p.delete_status)) >= 9 ? ProType.MoreLists : ProType.MoreListsUnlimited);
    }

    private void OnDragSelectChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(this.DataContext is ProjectItemViewModel dataContext))
        return;
      this.Opacity = (double) (!dataContext.DragSelected ? 1 : 0);
    }

    private void OnOpenChanged(object sender, PropertyChangedEventArgs e)
    {
      Border foldBorder = this.GetFoldBorder();
      if (foldBorder == null || !(foldBorder.Child is Border child1) || !(child1.Child is Path child2) || !(child2.RenderTransform is RotateTransform renderTransform) || !(this.DataContext is ProjectItemViewModel dataContext))
        return;
      DependencyProperty angleProperty = RotateTransform.AngleProperty;
      DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(), dataContext.Open ? 0.0 : -90.0, 120);
      renderTransform.BeginAnimation(angleProperty, (AnimationTimeline) doubleAnimation);
    }

    private void OnHoverPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(this.DataContext is PtfAllViewModel dataContext) || dataContext.InSubSection)
        return;
      this.ShowOrHideAddBorder(ProjectMenuItemHoverModel.Model.HoverType == dataContext.Type);
    }
  }
}
