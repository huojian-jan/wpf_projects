// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ProjectList.ProjectListItemView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.MainListView.ProjectList
{
  public class ProjectListItemView : Grid
  {
    private bool _canSelect;
    private readonly ColumnDefinition _secondColumn;
    private int _checkInt;
    private readonly ContentControl _backBorder;
    private readonly Grid _iconGrid;
    private readonly EmjTextBlock _title;
    private readonly ContentControl _rightControl;

    public ProjectListItemView()
    {
      ContentControl contentControl1 = new ContentControl();
      contentControl1.Margin = new Thickness(8.0, 0.0, 8.0, 0.0);
      this._backBorder = contentControl1;
      Grid grid = new Grid();
      grid.HorizontalAlignment = HorizontalAlignment.Left;
      grid.Width = 20.0;
      grid.Height = 22.0;
      grid.VerticalAlignment = VerticalAlignment.Center;
      this._iconGrid = grid;
      EmjTextBlock emjTextBlock = new EmjTextBlock();
      emjTextBlock.Margin = new Thickness(28.0, 0.0, 0.0, 0.0);
      emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
      emjTextBlock.TextWrapping = TextWrapping.Wrap;
      emjTextBlock.ClipToBounds = true;
      this._title = emjTextBlock;
      ContentControl contentControl2 = new ContentControl();
      contentControl2.HorizontalAlignment = HorizontalAlignment.Right;
      contentControl2.Margin = new Thickness(0.0, 0.0, 16.0, 0.0);
      this._rightControl = contentControl2;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this.Cursor = Cursors.Hand;
      this.Background = (Brush) Brushes.Transparent;
      this.SetResourceReference(FrameworkElement.HeightProperty, (object) "Height40");
      this.MouseEnter += new MouseEventHandler(this.OnMouseEnter);
      this.MouseLeave += new MouseEventHandler(this.OnMouseLeave);
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnProjectClick);
      this.MouseRightButtonUp += new MouseButtonEventHandler(this.OnRightClick);
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnPreviewMouseDown);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(20.0)
      });
      this._secondColumn = new ColumnDefinition();
      this.ColumnDefinitions.Add(this._secondColumn);
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = GridLength.Auto
      });
      this.InitElements();
    }

    private Effect GetTextEffect()
    {
      return (Effect) new DropShadowEffect()
      {
        BlurRadius = 2.0,
        Direction = 270.0,
        ShadowDepth = 1.0,
        Opacity = LocalSettings.Settings.ProjectTextShadowOpacity
      };
    }

    private void InitElements()
    {
      this.InitBackBorder();
      this.InitIcon();
      this.InitTitle();
      this.InitColorAndCount();
    }

    private void InitColorAndCount()
    {
      this.Children.Add((UIElement) this._rightControl);
      this._rightControl.SetValue(Grid.ColumnProperty, (object) 3);
      this._rightControl.SetResourceReference(FrameworkElement.MinWidthProperty, (object) "Height24");
    }

    private void InitTitle()
    {
      this.Children.Add((UIElement) this._title);
      this._title.SetValue(Grid.ColumnProperty, (object) 1);
      this._title.SetResourceReference(TextBlock.FontSizeProperty, (object) "Font14");
      this._title.SetResourceReference(TextBlock.ForegroundProperty, (object) "ProjectMenuColorOpacity100_80");
      this._title.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body01");
      this._title.SetResourceReference(FrameworkElement.MaxHeightProperty, (object) "Double20Add2");
      this._title.SetBinding(EmjTextBlock.TextProperty, "TitleText");
      this._title.Effect = this.GetTextEffect();
      this.SetValue(ToolTipService.InitialShowDelayProperty, (object) 300);
    }

    private void InitIcon()
    {
      this.Children.Add((UIElement) this._iconGrid);
      this._iconGrid.SetValue(Grid.ColumnProperty, (object) 1);
    }

    private Border GetFoldBorder()
    {
      foreach (object child in this.Children)
      {
        if (child is Border foldBorder && (int) foldBorder.GetValue(Grid.ColumnProperty) == 0 && (int) foldBorder.GetValue(Grid.ColumnSpanProperty) == 1)
          return foldBorder;
      }
      return (Border) null;
    }

    private void ShowOrHideFoldIcon(bool show, bool open)
    {
      Border foldBorder = this.GetFoldBorder();
      if (!show)
      {
        if (foldBorder == null)
          return;
        this.Children.Remove((UIElement) foldBorder);
      }
      else if (foldBorder == null)
      {
        Border border = new Border();
        border.Width = 20.0;
        border.Margin = new Thickness(6.0, 0.0, 0.0, 0.0);
        Border element = border;
        element.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle60_100");
        element.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCollapseClick);
        Path path = UiUtils.CreatePath("ArrowLine", "ProjectMenuColorOpacity100_80", "Path01");
        path.Width = 12.0;
        path.Height = 12.0;
        path.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
        RotateTransform rotateTransform = new RotateTransform()
        {
          Angle = open ? 0.0 : -90.0
        };
        path.RenderTransform = (Transform) rotateTransform;
        element.Child = (UIElement) path;
        this.Children.Add((UIElement) element);
      }
      else
      {
        if (!(foldBorder.Child is Path child))
          return;
        RotateTransform rotateTransform = new RotateTransform()
        {
          Angle = open ? 0.0 : -90.0
        };
        child.RenderTransform = (Transform) rotateTransform;
      }
    }

    private void ShowOrHideColor(bool show)
    {
      Ellipse element1 = (Ellipse) null;
      foreach (object child in this.Children)
      {
        if (child is Ellipse ellipse)
        {
          element1 = ellipse;
          break;
        }
      }
      if (!show)
      {
        if (element1 == null)
          return;
        this.Children.Remove((UIElement) element1);
      }
      else
      {
        if (element1 != null)
          return;
        Ellipse ellipse = new Ellipse();
        ellipse.Width = 8.0;
        ellipse.Height = 8.0;
        Ellipse element2 = ellipse;
        this.Children.Add((UIElement) element2);
        element2.SetBinding(Shape.FillProperty, "Color");
        element2.SetValue(Grid.ColumnProperty, (object) 2);
      }
    }

    private void InitBackBorder()
    {
      this.Children.Add((UIElement) this._backBorder);
      this._backBorder.SetValue(Grid.ColumnProperty, (object) 0);
      this._backBorder.SetValue(Grid.ColumnSpanProperty, (object) 4);
    }

    private void SetBackground()
    {
      if (!(this.DataContext is ProjectItemViewModel dataContext))
        return;
      Border border;
      if (dataContext.DropSelected || dataContext.IsTabSelected)
      {
        border = new Border()
        {
          CornerRadius = new CornerRadius(6.0),
          BorderThickness = new Thickness(1.0),
          Background = dataContext.DropSelected ? (Brush) ThemeUtil.GetColorInString("#19FFFFFF") : (Brush) Brushes.Transparent
        };
        border.SetResourceReference(Border.BorderBrushProperty, dataContext.DropSelected ? (object) "ProjectMenuColorOpacity40" : (object) "PrimaryColor");
      }
      else if (dataContext.Selected)
      {
        border = new Border()
        {
          CornerRadius = new CornerRadius(6.0),
          BorderThickness = new Thickness(0.0)
        };
        border.SetResourceReference(Border.BackgroundProperty, (object) "ProjectSelectedBackground");
      }
      else
      {
        border = new Border()
        {
          CornerRadius = new CornerRadius(6.0)
        };
        border.SetResourceReference(FrameworkElement.StyleProperty, (object) "ProjectItemHoverBorderStyle");
      }
      this._backBorder.Content = (object) border;
    }

    private void SetRightContent(ProjectItemViewModel model)
    {
      if (model?.InfoIcon != null)
      {
        if (this._rightControl.Content is Border content && content.Name == "InvalidPath")
          return;
        Border border1 = new Border();
        border1.Name = "InvalidPath";
        border1.Width = 16.0;
        border1.Height = 16.0;
        border1.Background = (Brush) Brushes.Transparent;
        border1.Cursor = Cursors.Hand;
        Border border2 = border1;
        border2.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnInvalidInfoClick);
        border2.SetBinding(FrameworkElement.ToolTipProperty, "Info");
        Path path = UiUtils.CreatePath("", "", "ProjectIconStyle");
        path.Width = 14.0;
        path.Data = model.InfoIcon;
        path.Fill = (Brush) ThemeUtil.GetColorInString("#FFB000");
        border2.Child = (UIElement) path;
        this._rightControl.Content = (object) border2;
      }
      else if (this.IsMouseOver && !(model is ClosedProjectGroupViewModel))
      {
        if (this._rightControl.Content is Button content && content.Name == "MoreButton")
          return;
        Button button1 = new Button();
        button1.Name = "MoreButton";
        button1.Cursor = Cursors.Hand;
        button1.Width = 16.0;
        button1.HorizontalAlignment = HorizontalAlignment.Right;
        Button button2 = button1;
        button2.SetResourceReference(FrameworkElement.StyleProperty, (object) "OpacityHoverButtonStyle");
        button2.Click += new RoutedEventHandler(this.OnMoreClick);
        Path path = UiUtils.CreatePath("IcMore", "ProjectMenuColorOpacity80", "Path01");
        path.Width = 16.0;
        path.Height = 16.0;
        button2.Content = (object) path;
        this._rightControl.Content = (object) button2;
      }
      else if (model != null && model.Count > 0)
      {
        if (this._rightControl.Content is TextBlock)
          return;
        TextBlock textBlock1 = new TextBlock();
        textBlock1.HorizontalAlignment = HorizontalAlignment.Right;
        textBlock1.Opacity = 0.8;
        TextBlock textBlock2 = textBlock1;
        textBlock2.SetResourceReference(FrameworkElement.StyleProperty, (object) "Tag01");
        textBlock2.SetResourceReference(TextBlock.ForegroundProperty, (object) "ProjectMenuColorOpacity80");
        textBlock2.SetBinding(TextBlock.TextProperty, "Count");
        textBlock2.Effect = this._title.Effect;
        this._rightControl.Content = (object) textBlock2;
      }
      else
        this._rightControl.Content = (object) null;
    }

    private void SetIcon(ProjectItemViewModel origin, ProjectItemViewModel model)
    {
      if (origin != null && origin.ShowIcon == model.ShowIcon && (!origin.ShowIcon || string.IsNullOrEmpty(origin.IconText) == string.IsNullOrEmpty(model.IconText)))
        return;
      this._iconGrid.Children.Clear();
      if (model.ShowIcon)
      {
        Path path = UiUtils.CreatePath("", "ProjectMenuIconColor", "Path01");
        path.Width = 18.0;
        path.Height = 18.0;
        path.HorizontalAlignment = HorizontalAlignment.Center;
        path.VerticalAlignment = VerticalAlignment.Center;
        path.SetBinding(Path.DataProperty, "Icon");
        this._iconGrid.Children.Add((UIElement) path);
      }
      else
      {
        EmjTextBlock emjTextBlock = new EmjTextBlock();
        emjTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
        emjTextBlock.TextAlignment = TextAlignment.Center;
        emjTextBlock.Width = 20.0;
        emjTextBlock.FontSize = 16.0;
        emjTextBlock.Margin = new Thickness(0.0, -1.0, 0.0, 0.0);
        EmjTextBlock element = emjTextBlock;
        element.SetResourceReference(FrameworkElement.StyleProperty, (object) "Body01");
        element.SetResourceReference(TextBlock.ForegroundProperty, (object) "ProjectMenuColorOpacity80");
        element.SetBinding(EmjTextBlock.TextProperty, "EmojiText");
        this._iconGrid.Children.Add((UIElement) element);
      }
    }

    private void OnMoreClick(object sender, RoutedEventArgs routedEventArgs)
    {
      Mouse.Capture((IInputElement) null);
      this.ShowActionPopup();
    }

    private void OnInvalidInfoClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext is BindCalendarAccountProjectViewModel dataContext1)
      {
        try
        {
          SubscribeExpiredWindow.TryShowWindow(new List<BindCalendarAccountModel>()
          {
            dataContext1.Account
          });
        }
        catch (Exception ex)
        {
          UtilLog.Error(ex.Message);
        }
      }
      if (!(this.DataContext is SubscribeCalendarProjectViewModel dataContext2))
        return;
      try
      {
        SubscribeExpiredWindow.TryShowWindow((List<BindCalendarAccountModel>) null, new List<CalendarSubscribeProfileModel>()
        {
          dataContext2.Profile
        });
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex.Message);
      }
    }

    private void OnProjectClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._canSelect)
        return;
      if (this.DataContext is ProjectItemViewModel dataContext)
      {
        if (dataContext is ClosedProjectGroupViewModel)
          Utils.FindParent<ProjectListView>((DependencyObject) this)?.CollapseItem(dataContext);
        else
          Utils.FindParent<ProjectListView>((DependencyObject) this)?.OnProjectClick(dataContext, true);
      }
      this._canSelect = false;
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      Border foldBorder = this.GetFoldBorder();
      if (foldBorder != null && foldBorder.IsMouseOver || this._rightControl.IsMouseOver)
        return;
      this._canSelect = true;
    }

    private void OnMouseEnter(object sender, MouseEventArgs e)
    {
      if (!(this.DataContext is ProjectItemViewModel dataContext))
        return;
      this.SetRightContent(dataContext);
      PtfType ptfType = dataContext.GetPtfType();
      ProjectMenuItemHoverModel.Model.HoverType = ptfType;
    }

    private async void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      ProjectListItemView projectListItemView = this;
      ProjectItemViewModel oldValue = e.OldValue as ProjectItemViewModel;
      ProjectItemViewModel newValue = e.NewValue as ProjectItemViewModel;
      if (oldValue != null)
        PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) oldValue, new EventHandler<PropertyChangedEventArgs>(projectListItemView.OnModelPropertyChanged), "");
      if (newValue != null)
      {
        PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) newValue, new EventHandler<PropertyChangedEventArgs>(projectListItemView.OnModelPropertyChanged), "");
        projectListItemView.SetIcon(oldValue, newValue);
        projectListItemView.ShowOrHideFoldIcon(newValue.IsGroupItem, newValue.Open);
        projectListItemView.ShowOrHideColor(!string.IsNullOrEmpty(newValue.Color) && newValue.Color.ToLower() != "transparent");
        projectListItemView.SetRightContent(newValue);
        projectListItemView.Margin = new Thickness((double) ((newValue.InSubSection ? 18 : 0) + (newValue.IsSubItem ? 10 : 0)), 0.0, 0.0, 0.0);
        projectListItemView.SetBackground();
        projectListItemView.Opacity = (double) (!newValue.DragSelected ? 1 : 0);
        if (newValue.Count <= 0 && LocalSettings.Settings.ExtraSettings.NumDisplayType != 2)
          newValue.LoadCount();
        projectListItemView.CheckTitleTooltip();
      }
      if (!(projectListItemView._title.Effect is DropShadowEffect effect))
        return;
      effect.Opacity = LocalSettings.Settings.ProjectTextShadowOpacity;
    }

    private async void CheckTitleTooltip()
    {
      ProjectListItemView projectListItemView = this;
      int num = new Random().Next(0, 1000);
      projectListItemView._checkInt = num;
      await Task.Delay(200);
      if (projectListItemView._checkInt != num || !(projectListItemView.DataContext is ProjectItemViewModel dataContext) || projectListItemView._title == null)
        return;
      double num1 = Utils.MeasureStringWidth(dataContext.TitleText, projectListItemView._title.FontSize);
      projectListItemView._title.ToolTip = num1 > projectListItemView._title.ActualWidth ? (object) dataContext.Title : (object) (string) null;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => this.CheckTitleTooltip();

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "Count")
        this.SetRightContent(this.DataContext as ProjectItemViewModel);
      if (e.PropertyName == "Selected" || e.PropertyName == "DropSelected")
        this.SetBackground();
      if (e.PropertyName == "Open")
        this.BeginFoldAnimation();
      if (e.PropertyName == "Level" && this.DataContext is ProjectItemViewModel dataContext1)
        this.Margin = new Thickness((double) ((dataContext1.InSubSection ? 18 : 0) + (dataContext1.IsSubItem ? 10 : 0)), 0.0, 0.0, 0.0);
      if (!(e.PropertyName == "DragSelected") || !(this.DataContext is ProjectItemViewModel dataContext2))
        return;
      this.Opacity = (double) (!dataContext2.DragSelected ? 1 : 0);
    }

    private void BeginFoldAnimation()
    {
      Border foldBorder = this.GetFoldBorder();
      if (foldBorder == null || !(foldBorder.Child is Path child) || !(child.RenderTransform is RotateTransform renderTransform) || !(this.DataContext is ProjectItemViewModel dataContext))
        return;
      DependencyProperty angleProperty = RotateTransform.AngleProperty;
      DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(), dataContext.Open ? 0.0 : -90.0, 120);
      renderTransform.BeginAnimation(angleProperty, (AnimationTimeline) doubleAnimation);
    }

    private void OnMouseLeave(object sender, MouseEventArgs e)
    {
      if (!(this.DataContext is ProjectItemViewModel dataContext))
        return;
      this.SetRightContent(dataContext);
    }

    private void OnCollapseClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is ProjectItemViewModel dataContext))
        return;
      Utils.FindParent<ProjectListView>((DependencyObject) this)?.CollapseItem(dataContext);
    }

    private void OnRightClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      this.ShowActionPopup();
    }

    private async Task ShowActionPopup()
    {
      ProjectListItemView projectListItemView = this;
      if (!(projectListItemView.DataContext is ProjectItemViewModel model))
        model = (ProjectItemViewModel) null;
      else if (TeamDao.IsTeamExpired(model.TeamId))
      {
        Utils.Toast(Utils.GetString("TeamExpiredOperate"));
        model = (ProjectItemViewModel) null;
      }
      else
      {
        IEnumerable<ContextAction> contextActions = await model.GetContextActions();
        List<ContextAction> list = contextActions != null ? contextActions.ToList<ContextAction>() : (List<ContextAction>) null;
        if (list == null)
        {
          model = (ProjectItemViewModel) null;
        }
        else
        {
          List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
          foreach (ContextAction contextAction in list)
          {
            CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) contextAction.ActionKey, contextAction.Title, (Geometry) null);
            if (contextAction.ActionKey == ContextActionKey.Exit && CacheManager.GetProjectById(model.Id).OpenToTeam)
            {
              menuItemViewModel1.IsEnable = false;
              menuItemViewModel1.IsMessage = true;
              menuItemViewModel1.ExtraIcon = Utils.GetIcon("IcHelp");
              menuItemViewModel1.ExtraIconColor = ThemeUtil.GetColor("BaseColorOpacity100");
              menuItemViewModel1.ExtraIconTips = Utils.GetString("CannotExitOpenProject");
            }
            List<ContextAction> subActions = contextAction.SubActions;
            if ((subActions != null ? (subActions.Count<ContextAction>() > 0 ? 1 : 0) : 0) != 0)
            {
              menuItemViewModel1.SubActions = new List<CustomMenuItemViewModel>();
              foreach (ContextAction subAction in contextAction.SubActions)
              {
                CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) subAction.ActionKey, subAction.Title, (Geometry) null);
                menuItemViewModel2.Selected = subAction.Selected;
                menuItemViewModel2.ShowSelected = true;
                CustomMenuItemViewModel menuItemViewModel3 = menuItemViewModel2;
                menuItemViewModel1.SubActions.Add(menuItemViewModel3);
              }
            }
            types.Add(menuItemViewModel1);
          }
          EscPopup escPopup1 = new EscPopup();
          escPopup1.StaysOpen = false;
          escPopup1.Placement = PlacementMode.Mouse;
          escPopup1.PlacementTarget = (UIElement) projectListItemView;
          escPopup1.HorizontalOffset = -5.0;
          escPopup1.VerticalOffset = -5.0;
          EscPopup escPopup2 = escPopup1;
          CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) escPopup2);
          customMenuList.Operated += new EventHandler<object>(projectListItemView.OnActionSelected);
          customMenuList.Show();
          model = (ProjectItemViewModel) null;
        }
      }
    }

    private void OnActionSelected(object sender, object e)
    {
      if (!(e is ContextActionKey action))
        return;
      switch (action)
      {
        case ContextActionKey.OpenNewWindow:
        case ContextActionKey.CloseWindow:
          TaskListWindow.OpenOrCloseWindow(this.GetModel<ProjectItemViewModel>().GetSaveIdentity(), Window.GetWindow((DependencyObject) this));
          break;
        default:
          if (this.DataContext == null)
            break;
          switch (this.DataContext)
          {
            case FilterProjectViewModel _:
              this.HandleFilterAction(action);
              return;
            case TagProjectViewModel projectViewModel:
              this.HandleTagAction(projectViewModel.TagModel, action);
              return;
            case SubscribeCalendarProjectViewModel _:
              this.HandleCalendarAction(action);
              return;
            case BindCalendarAccountProjectViewModel _:
              this.HandleBindAccountAction(action);
              return;
            case ticktick_WPF.ViewModels.SmartProjectViewModel smart:
              this.HandleSmartProjectAction(action, smart);
              return;
            case ticktick_WPF.ViewModels.ProjectGroupViewModel group:
              this.HandleGroupProjectAction(group, action);
              return;
            default:
              this.HandleProjectAction(action);
              return;
          }
      }
    }

    private ProjectListView GetParent()
    {
      return Utils.FindParent<ProjectListView>((DependencyObject) this);
    }

    private void HandleGroupProjectAction(ticktick_WPF.ViewModels.ProjectGroupViewModel group, ContextActionKey action)
    {
      switch (action)
      {
        case ContextActionKey.Edit:
          UserActCollectUtils.AddClickEvent("project_list_ui", "folder", "cm.rename");
          this.GetParent()?.OnRenameGroup(group.ProjectGroup);
          break;
        case ContextActionKey.Ungroup:
          UserActCollectUtils.AddClickEvent("project_list_ui", "folder", "cm.ungroup");
          this.GetParent()?.OnDeleteProjectGroup(group.ProjectGroup);
          break;
        case ContextActionKey.AddProject:
          UserActCollectUtils.AddClickEvent("project_list_ui", "folder", "cm.add_list");
          this.GetParent()?.OnAddProjectClick(group.ProjectGroup.id, group.ProjectGroup.teamId);
          break;
        case ContextActionKey.Pin:
        case ContextActionKey.Unpin:
          UserActCollectUtils.AddClickEvent("project_list_ui", "folder", action == ContextActionKey.Pin ? "cm.pin" : "cm.unpin");
          this.GetParent()?.OnPinClick(group.ProjectGroup.id, 6, action == ContextActionKey.Pin);
          break;
      }
    }

    private void HandleSmartProjectAction(ContextActionKey action, ticktick_WPF.ViewModels.SmartProjectViewModel smart)
    {
      switch (action)
      {
        case ContextActionKey.Edit:
          this.GetParent().EditInboxProject();
          break;
        case ContextActionKey.Show:
        case ContextActionKey.Hide:
        case ContextActionKey.ShowIfNotEmpty:
          this.GetParent().ShowOrHideProject(this.GetSmartProject(), action);
          break;
        case ContextActionKey.Pin:
        case ContextActionKey.Unpin:
          UserActCollectUtils.AddClickEvent("project_list_ui", "tag", action == ContextActionKey.Pin ? "cm.pin" : "cm.unpin");
          this.GetParent()?.OnPinClick(smart.Project.UserEventId, 11, action == ContextActionKey.Pin);
          break;
      }
    }

    private void HandleBindAccountAction(ContextActionKey action)
    {
      BindCalendarAccountModel bindCalendar = this.GetBindCalendar();
      if (bindCalendar == null)
        return;
      switch (action)
      {
        case ContextActionKey.Edit:
          UserActCollectUtils.AddClickEvent("project_list_ui", "event", "cm.edit");
          this.GetParent().EditBindCalendar(bindCalendar.Id);
          break;
        case ContextActionKey.Unsubscribe:
          this.GetParent().UnbindCalendar(bindCalendar.Id);
          break;
        case ContextActionKey.Reauthorize:
          BindCalendarAccountProjectViewModel model = this.GetModel<BindCalendarAccountProjectViewModel>();
          if (model.Account.IsBindAccountPassword())
          {
            BindAccountWindow bindAccountWindow = new BindAccountWindow(model.Account);
            bindAccountWindow.Owner = Utils.GetParentWindow((DependencyObject) this);
            bindAccountWindow.ShowDialog();
            break;
          }
          if (model.Account.Site == "outlook")
          {
            BindOutlookWindow bindOutlookWindow = new BindOutlookWindow();
            bindOutlookWindow.SetOriginAccount(model.Account?.Id);
            bindOutlookWindow.ShowDialog();
            break;
          }
          if (model.Account.Site == "google")
          {
            BindGoogleAccount.GetInstance().Start(model.Account?.Id);
            break;
          }
          if (!(model.Account.Kind == "icloud"))
            break;
          new BindICloudWindow((SubscribeCalendar) null, model.Account).ShowDialog();
          break;
        case ContextActionKey.Pin:
        case ContextActionKey.Unpin:
          this.GetParent()?.OnPinClick(bindCalendar.Id, 9, action == ContextActionKey.Pin);
          UserActCollectUtils.AddClickEvent("project_list_ui", "event", action == ContextActionKey.Pin ? "cm.pin" : "cm.unpin");
          break;
      }
    }

    private void HandleCalendarAction(ContextActionKey action)
    {
      CalendarSubscribeProfileModel calendarModel = this.GetCalendarModel();
      if (calendarModel == null)
        return;
      switch (action)
      {
        case ContextActionKey.Edit:
          UserActCollectUtils.AddClickEvent("project_list_ui", "event", "cm.edit");
          this.GetParent().EditSubscribeCalendar(calendarModel);
          break;
        case ContextActionKey.Unsubscribe:
          this.GetParent()?.UnsubscribeCalendar(calendarModel);
          break;
        case ContextActionKey.Pin:
        case ContextActionKey.Unpin:
          UserActCollectUtils.AddClickEvent("project_list_ui", "event", action == ContextActionKey.Pin ? "cm.pin" : "cm.unpin");
          this.GetParent()?.OnPinClick(calendarModel.Id, 9, action == ContextActionKey.Pin);
          break;
      }
    }

    private async void HandleTagAction(TagModel tag, ContextActionKey action)
    {
      bool canEditTitle = await ProjectListItemView.CheckCanEditTag(tag.name);
      switch (action)
      {
        case ContextActionKey.Edit:
          UserActCollectUtils.AddClickEvent("project_list_ui", nameof (tag), "cm.edit");
          this.GetParent()?.EditTag(tag, canEditTitle);
          break;
        case ContextActionKey.Delete:
          UserActCollectUtils.AddClickEvent("project_list_ui", nameof (tag), "cm.delete");
          if (!canEditTitle)
          {
            Utils.Toast(Utils.GetString("NoNetwork"));
            break;
          }
          this.GetParent()?.DeleteTag(tag);
          break;
        case ContextActionKey.MergeTags:
          UserActCollectUtils.AddClickEvent("project_list_ui", nameof (tag), "cm.merge");
          if (!canEditTitle)
          {
            Utils.Toast(Utils.GetString("NoNetwork"));
            break;
          }
          this.GetParent()?.MergeTag(tag);
          break;
        case ContextActionKey.Pin:
        case ContextActionKey.Unpin:
          UserActCollectUtils.AddClickEvent("project_list_ui", nameof (tag), action == ContextActionKey.Pin ? "cm.pin" : "cm.unpin");
          this.GetParent()?.OnPinClick(tag.name, 7, action == ContextActionKey.Pin);
          break;
        case ContextActionKey.ToShareTag:
        case ContextActionKey.ToPersonalTag:
          TagService.SwitchTagType(tag.name);
          break;
      }
    }

    private static async Task<bool> CheckCanEditTag(string tag)
    {
      return Utils.IsNetworkAvailable() || (await TagDao.GetTagByName(tag)).status != 2;
    }

    private void HandleProjectAction(ContextActionKey action)
    {
      switch (action)
      {
        case ContextActionKey.Edit:
          UserActCollectUtils.AddClickEvent("project_list_ui", "list", "cm.edit");
          this.GetParent()?.EditProject(this.GetProjectModel());
          break;
        case ContextActionKey.Delete:
          UserActCollectUtils.AddClickEvent("project_list_ui", "list", "cm.delete");
          ProjectListView parent1 = this.GetParent();
          if (parent1 == null)
            break;
          parent1.DeleteOrExitProject(this.GetProjectModel());
          break;
        case ContextActionKey.Share:
          UserActCollectUtils.AddClickEvent("project_list_ui", "list", "cm.share");
          this.GetParent()?.ShareProject(this.GetProjectModel());
          break;
        case ContextActionKey.Exit:
          UserActCollectUtils.AddClickEvent("project_list_ui", "list", "cm.delete");
          ProjectListView parent2 = this.GetParent();
          if (parent2 == null)
            break;
          parent2.DeleteOrExitProject(this.GetProjectModel(), false);
          break;
        case ContextActionKey.Duplicate:
          UserActCollectUtils.AddClickEvent("project_list_ui", "list", "cm.duplicate");
          ProjectListView parent3 = this.GetParent();
          if (parent3 == null)
            break;
          parent3.DuplicateProject(this.GetProjectModel());
          break;
        case ContextActionKey.Archive:
          UserActCollectUtils.AddClickEvent("project_list_ui", "list", "cm.archive");
          this.GetParent()?.CloseOrOpenProject(this.GetProjectModel());
          break;
        case ContextActionKey.UnArchiveList:
          UserActCollectUtils.AddClickEvent("project_list_ui", "list", "cm.unarchive");
          this.GetParent()?.CloseOrOpenProject(this.GetProjectModel());
          break;
        case ContextActionKey.Pin:
        case ContextActionKey.Unpin:
          ProjectModel projectModel = this.GetProjectModel();
          UserActCollectUtils.AddClickEvent("project_list_ui", "list", action == ContextActionKey.Pin ? "cm.pin" : "cm.unpin");
          this.GetParent()?.OnPinClick(projectModel.id, 5, action == ContextActionKey.Pin);
          break;
      }
    }

    private void HandleFilterAction(ContextActionKey action)
    {
      switch (action)
      {
        case ContextActionKey.Edit:
          UserActCollectUtils.AddClickEvent("project_list_ui", "filter", "cm.edit");
          this.GetParent()?.EditFilter(this.GetFilterModel());
          break;
        case ContextActionKey.Delete:
          UserActCollectUtils.AddClickEvent("project_list_ui", "filter", "cm.delete");
          this.GetParent()?.DeleteFilter(this.GetFilterModel());
          break;
        case ContextActionKey.Duplicate:
          if (!ProChecker.CheckPro(ProType.Filter))
            break;
          UserActCollectUtils.AddClickEvent("project_list_ui", "filter", "cm.duplicate");
          ProjectListView parent = this.GetParent();
          if (parent == null)
            break;
          parent.DuplicateFilter(this.GetFilterModel());
          break;
        case ContextActionKey.Pin:
        case ContextActionKey.Unpin:
          FilterModel filterModel = this.GetFilterModel();
          UserActCollectUtils.AddClickEvent("project_list_ui", "filter", action == ContextActionKey.Pin ? "cm.pin" : "cm.unpin");
          this.GetParent()?.OnPinClick(filterModel.id, 8, action == ContextActionKey.Pin);
          break;
      }
    }

    private ProjectModel GetProjectModel()
    {
      if (this.DataContext != null)
      {
        if (this.DataContext is NormalProjectViewModel dataContext1)
          return dataContext1.Project;
        if (this.DataContext is ticktick_WPF.ViewModels.SubProjectViewModel dataContext2)
          return dataContext2.Project;
      }
      return (ProjectModel) null;
    }

    private SmartProject GetSmartProject() => this.GetModel<ticktick_WPF.ViewModels.SmartProjectViewModel>().Project;

    private BindCalendarAccountModel GetBindCalendar()
    {
      return this.GetModel<BindCalendarAccountProjectViewModel>().Account;
    }

    private CalendarSubscribeProfileModel GetCalendarModel()
    {
      return this.GetModel<SubscribeCalendarProjectViewModel>().Profile;
    }

    private FilterModel GetFilterModel() => this.GetModel<FilterProjectViewModel>().Filter;

    private T GetModel<T>()
    {
      return this.DataContext != null && this.DataContext is T dataContext ? dataContext : default (T);
    }
  }
}
