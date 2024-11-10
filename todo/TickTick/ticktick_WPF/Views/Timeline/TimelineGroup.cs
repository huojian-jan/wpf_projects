// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineGroup
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Converter;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.TaskList;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineGroup : Border, IComponentConnector
  {
    public static readonly DependencyProperty PreviewDataProperty = DependencyProperty.Register(nameof (PreviewData), typeof (object), typeof (TimelineGroup), new PropertyMetadata((object) null));
    private System.Windows.Point _dragStartPoint;
    private Grid _container;
    private Border _foldBorder;
    private Border _moreButton;
    private EscPopup _morePopup;
    private TextBox _inputText;
    private Popup _errorPopup;
    private Grid _root;
    private EmjTextBlock _emjText;
    internal TimelineGroup Root;
    private bool _contentLoaded;

    public object PreviewData
    {
      get => this.GetValue(TimelineGroup.PreviewDataProperty);
      set => this.SetValue(TimelineGroup.PreviewDataProperty, value);
    }

    public TimelineGroup()
    {
      this.InitializeComponent();
      this.LoadContent();
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is TimelineGroupViewModel newValue))
        return;
      if (newValue.Editable && this._moreButton == null)
      {
        this.LoadMoreButton();
        this.LoadEditContent();
      }
      if (this._emjText == null)
        return;
      this._emjText.Margin = new Thickness(newValue.Editable || !newValue.IsAvatar || !TaskUtils.IsAvatarUrlEmpty(newValue.AvatarUrl) ? 2.0 : 8.0, 0.0, 0.0, 0.0);
    }

    private void LoadMoreButton()
    {
      this._moreButton = new Border();
      EscPopup escPopup = new EscPopup();
      escPopup.PlacementTarget = (UIElement) this._moreButton;
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.PopupAnimation = PopupAnimation.Fade;
      escPopup.StaysOpen = false;
      this._morePopup = escPopup;
      this._morePopup.Closed += new EventHandler(this.OnPopupClosed);
      BoolOrVisibilityConverter visibilityConverter = new BoolOrVisibilityConverter();
      this._moreButton.SetBinding(UIElement.VisibilityProperty, (BindingBase) new MultiBinding()
      {
        Converter = (IMultiValueConverter) visibilityConverter,
        Bindings = {
          (BindingBase) new Binding("IsMouseOver")
          {
            Source = (object) this._container
          },
          (BindingBase) new Binding("IsOpen")
          {
            Source = (object) this._morePopup
          }
        }
      });
      this._moreButton.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_100");
      this._moreButton.VerticalAlignment = VerticalAlignment.Center;
      this._moreButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMoreClick);
      this._moreButton.SetValue(Grid.ColumnProperty, (object) 3);
      Image image = new Image();
      image.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeLineGroupMoreImageStyle");
      this._moreButton.Child = (UIElement) image;
      this._container.Children.Add((UIElement) this._moreButton);
      this._container.Children.Add((UIElement) this._morePopup);
    }

    private void LoadEditContent()
    {
      Grid element1 = new Grid();
      element1.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeLineGroupEditGridStyle");
      element1.IsVisibleChanged += new DependencyPropertyChangedEventHandler(this.OnEditBoxVisibleChanged);
      this._inputText = new TextBox();
      this._inputText.Margin = new Thickness(21.0, 3.0, 10.0, 0.0);
      this._inputText.FontSize = 13.0;
      this._inputText.Height = 20.0;
      this._inputText.MaxLength = 64;
      this._inputText.VerticalAlignment = VerticalAlignment.Top;
      this._inputText.SetResourceReference(FrameworkElement.StyleProperty, (object) "NoBorderTextBoxWithHint");
      this._inputText.LostFocus += new RoutedEventHandler(this.OnLostFocus);
      this._inputText.TextChanged += new TextChangedEventHandler(this.OnNameTextChanged);
      this._inputText.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
      element1.Children.Add((UIElement) this._inputText);
      this._errorPopup = new Popup()
      {
        StaysOpen = false,
        AllowsTransparency = true,
        Placement = PlacementMode.Relative,
        PlacementTarget = (UIElement) this._inputText,
        VerticalOffset = 30.0
      };
      ContentControl contentControl = new ContentControl();
      contentControl.SetResourceReference(FrameworkElement.StyleProperty, (object) "PopupContentStyle");
      contentControl.Margin = new Thickness(0.0);
      TextBlock textBlock1 = new TextBlock();
      textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock1.Margin = new Thickness(6.0);
      textBlock1.Width = 160.0;
      textBlock1.FontSize = 12.0;
      TextBlock textBlock2 = textBlock1;
      textBlock2.SetResourceReference(TextBlock.TextProperty, (object) "SectionNotValid");
      textBlock2.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100_80");
      contentControl.Content = (object) textBlock2;
      this._errorPopup.Child = (UIElement) contentControl;
      element1.Children.Add((UIElement) this._errorPopup);
      Border border = new Border();
      border.VerticalAlignment = VerticalAlignment.Top;
      border.Height = 22.0;
      border.BorderThickness = new Thickness(0.0, 0.0, 0.0, 1.0);
      border.Margin = new Thickness(20.0, 3.0, 10.0, 0.0);
      Border element2 = border;
      element2.SetResourceReference(Border.BorderBrushProperty, (object) "PrimaryColor");
      element1.Children.Add((UIElement) element2);
      this._root.Children.Add((UIElement) element1);
    }

    private void LoadContent()
    {
      Grid grid = new Grid();
      grid.Height = 40.0;
      grid.VerticalAlignment = VerticalAlignment.Top;
      grid.Background = (Brush) Brushes.Transparent;
      this._root = grid;
      this._root.MouseMove += new MouseEventHandler(this.OnDragMove);
      this.Child = (UIElement) this._root;
      Border element1 = new Border();
      element1.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimelineGroupTopLineStyle");
      this._root.Children.Add((UIElement) element1);
      this.LoadContainer();
      this.LoadFoldIcon();
      Ellipse element2 = new Ellipse();
      element2.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeLineGroupAvatarStyle");
      this._container.Children.Add((UIElement) element2);
      this.LoadText();
    }

    private void LoadText()
    {
      this._emjText = new EmjTextBlock();
      this._emjText.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimeLineGroupTextStyle");
      this._container.Children.Add((UIElement) this._emjText);
    }

    private void LoadContainer()
    {
      Grid grid = new Grid();
      grid.Background = (Brush) Brushes.Transparent;
      grid.Height = 25.0;
      grid.Cursor = Cursors.Hand;
      grid.VerticalAlignment = VerticalAlignment.Center;
      grid.Margin = new Thickness(0.0, 1.0, 0.0, 0.0);
      this._container = grid;
      this._container.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnContainerMouseUp);
      this._container.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnDragMouseDown);
      this._container.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(0.0, GridUnitType.Auto)
      });
      this._container.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(0.0, GridUnitType.Auto),
        MinWidth = 2.0
      });
      this._container.ColumnDefinitions.Add(new ColumnDefinition());
      this._container.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(0.0, GridUnitType.Auto)
      });
      this._root.Children.Add((UIElement) this._container);
    }

    private void LoadFoldIcon()
    {
      this._foldBorder = new Border();
      this._foldBorder.VerticalAlignment = VerticalAlignment.Center;
      this._foldBorder.SetValue(Grid.ColumnProperty, (object) 0);
      this._foldBorder.Margin = new Thickness(5.0, 0.0, 0.0, 0.0);
      this._foldBorder.SetBinding(UIElement.VisibilityProperty, (BindingBase) new Binding("ShowArrow"));
      this._foldBorder.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_100");
      this._foldBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnColumnOpenCloseMouseUp);
      Path path = new Path();
      path.SetResourceReference(FrameworkElement.StyleProperty, (object) "TimelineGroupFoldPathStyle");
      this._foldBorder.Child = (UIElement) path;
      this._container.Children.Add((UIElement) this._foldBorder);
    }

    private async void OnColumnOpenCloseMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TimelineGroupViewModel dataContext))
        return;
      await dataContext.UpdateIsOpen(!dataContext.IsOpen);
    }

    private void OnNameTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!NameUtils.IsValidColumnName(this._inputText.Text.Trim()))
      {
        this._errorPopup.IsOpen = true;
        this._inputText.SelectAll();
      }
      else
        this._errorPopup.IsOpen = false;
    }

    private void OnMoreClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
      if (!(this.DataContext is TimelineGroupViewModel))
        return;
      CustomSectionOption option = new CustomSectionOption(this._morePopup);
      this.SetOptionAction(option);
      option.Show();
    }

    protected virtual void SetOptionAction(CustomSectionOption option)
    {
      option.SetAction(new Action(this.AddSectionUpper), new Action(this.AddSectionUnder), new Action(this.Rename), new Action(this.Delete));
    }

    private void OnMoveColumnItemSelected(
      string columnId,
      string originProjectId,
      SelectableItemViewModel e)
    {
      string id = e.Id;
      if (!(id != originProjectId))
        return;
      TaskService.MoveColumnAsync(columnId, originProjectId, id);
    }

    private void AddSectionUpper()
    {
      this._morePopup.IsOpen = false;
      if (!(this.DataContext is TimelineGroupViewModel dataContext))
        return;
      dataContext.Parent?.AddNewColumn(dataContext, false);
    }

    private void AddSectionUnder()
    {
      this._morePopup.IsOpen = false;
      if (!(this.DataContext is TimelineGroupViewModel dataContext))
        return;
      dataContext.Parent?.AddNewColumn(dataContext, true);
    }

    private async void Rename()
    {
      TimelineGroup timelineGroup = this;
      timelineGroup._morePopup.IsOpen = false;
      if (!(timelineGroup.DataContext is TimelineGroupViewModel dataContext))
        return;
      dataContext.Editing = true;
      timelineGroup._inputText.Text = dataContext.Title;
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
      if (this.DataContext is TimelineGroupViewModel dataContext && dataContext.Editing)
      {
        string str = this._inputText.Text.Trim();
        dataContext.Editing = false;
        if (!string.IsNullOrEmpty(str) && NameUtils.IsValidColumnName(str))
          this.SaveSectionName(dataContext, str);
        else if (string.IsNullOrEmpty(dataContext.Id))
          dataContext.Parent.RemoveGroup(dataContext);
      }
      this._errorPopup.IsOpen = false;
    }

    private async Task SaveSectionName(TimelineGroupViewModel columnModel, string saveName)
    {
      TimelineGroupViewModel model = columnModel;
      if (string.IsNullOrEmpty(saveName))
        model = (TimelineGroupViewModel) null;
      else if (saveName == model.Title)
        model = (TimelineGroupViewModel) null;
      else if (await this.CheckIfColumnNameExisted(columnModel, saveName))
      {
        Utils.Toast(Utils.GetString("SectionNameExisted"));
        if (!string.IsNullOrEmpty(columnModel.Id))
        {
          model = (TimelineGroupViewModel) null;
        }
        else
        {
          columnModel.Parent.RemoveGroup(columnModel);
          model = (TimelineGroupViewModel) null;
        }
      }
      else
      {
        if (string.IsNullOrEmpty(columnModel.Id))
          model.Id = (await ColumnDao.AddColumn(saveName, model.ProjectId, model.SortOrder, model.Id)).id;
        else
          await ColumnDao.SaveColumnName(saveName, model.Id);
        model.Title = saveName;
        SyncManager.TryDelaySync();
        model = (TimelineGroupViewModel) null;
      }
    }

    private async Task<bool> CheckIfColumnNameExisted(TimelineGroupViewModel model, string name)
    {
      if (name == model.Title)
        return false;
      List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(model.ProjectId);
      return columnsByProjectId != null && columnsByProjectId.Exists((Predicate<ColumnModel>) (c => c.name == name));
    }

    private async void Delete()
    {
      TimelineGroup timelineGroup = this;
      timelineGroup._morePopup.IsOpen = false;
      if (!(timelineGroup.DataContext is TimelineGroupViewModel model))
        model = (TimelineGroupViewModel) null;
      else if (!model.Editable)
      {
        model = (TimelineGroupViewModel) null;
      }
      else
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("DeleteColumn"), Utils.GetString("DeleteColumnHint"), Utils.GetString(nameof (Delete)), Utils.GetString("Cancel"));
        customerDialog.Owner = Window.GetWindow((DependencyObject) timelineGroup);
        bool? nullable = customerDialog.ShowDialog();
        bool flag = true;
        if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        {
          model = (TimelineGroupViewModel) null;
        }
        else
        {
          await ColumnDao.DeleteColumn(model.Id);
          string projectDefaultColumnId = await ColumnDao.GetProjectDefaultColumnId(model.ProjectId);
          UtilLog.Info("TimelineGroup.Delete : " + model.Id);
          await TaskService.BatchDeleteTaskInColumn(model.ProjectId, model.Id, projectDefaultColumnId);
          Utils.Toast(Utils.GetString("Deleted"));
          SyncManager.TryDelaySync();
          model.Parent.RemoveGroup(model);
          model = (TimelineGroupViewModel) null;
        }
      }
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
    }

    private void OnDragMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this._foldBorder != null && this._foldBorder.IsMouseOver || this._moreButton != null && this._moreButton.IsMouseOver)
        return;
      this._dragStartPoint = e.GetPosition((IInputElement) this);
    }

    private void OnDragMove(object sender, MouseEventArgs e)
    {
      if (this._dragStartPoint != new System.Windows.Point() && e.LeftButton == MouseButtonState.Pressed)
      {
        if (!(this.DataContext is TimelineGroupViewModel dataContext) || !dataContext.Editable)
          return;
        System.Windows.Point position = e.GetPosition((IInputElement) this);
        if (Math.Abs(position.X - this._dragStartPoint.X) <= 4.0 && Math.Abs(position.Y - this._dragStartPoint.Y) <= 4.0)
          return;
        Utils.FindParent<TimelineContainer>((DependencyObject) this)?.ShowDragColumnPopup(dataContext, e);
      }
      else
        this._dragStartPoint = new System.Windows.Point();
    }

    private void OnContainerMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!(this._dragStartPoint != new System.Windows.Point()))
        return;
      this._dragStartPoint = new System.Windows.Point();
      if (!(this.DataContext is TimelineGroupViewModel dataContext) || dataContext.ShowArrow != Visibility.Visible)
        return;
      dataContext.UpdateIsOpen(!dataContext.IsOpen);
    }

    private async void OnEditBoxVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      TimelineGroup timelineGroup = this;
      if (!(timelineGroup.DataContext is TimelineGroupViewModel dataContext) || !dataContext.Editing)
        return;
      timelineGroup._inputText.Tag = string.IsNullOrEmpty(dataContext.Id) ? (object) Utils.GetString("NewColumn") : (object) (string) null;
      await Task.Delay(20);
      timelineGroup._inputText.Focus();
      timelineGroup._inputText.SelectAll();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          if (this.DataContext is TimelineGroupViewModel dataContext1 && dataContext1.Editing)
          {
            string str = this._inputText.Text.Trim();
            dataContext1.Editing = false;
            if (!string.IsNullOrEmpty(str) && NameUtils.IsValidColumnName(str))
              this.SaveSectionName(dataContext1, str);
            else if (string.IsNullOrEmpty(dataContext1.Id))
              dataContext1.Parent.RemoveGroup(dataContext1);
          }
          this._errorPopup.IsOpen = false;
          break;
        case Key.Escape:
          if (this.DataContext is TimelineGroupViewModel dataContext2 && dataContext2.Editing)
          {
            dataContext2.Editing = false;
            if (string.IsNullOrEmpty(dataContext2.Id))
              dataContext2.Parent.RemoveGroup(dataContext2);
          }
          this._errorPopup.IsOpen = false;
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
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/timeline/timelinegroup.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.Root = (TimelineGroup) target;
      else
        this._contentLoaded = true;
    }
  }
}
