// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CommentItemControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.MarkDown;

#nullable disable
namespace ticktick_WPF.Views
{
  public class CommentItemControl : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty FoldProperty = DependencyProperty.Register(nameof (Fold), typeof (bool), typeof (CommentItemControl), new PropertyMetadata((object) true));
    private Button _saveButton;
    internal CommentItemControl Root;
    internal Image ReplyGrid;
    internal Image EditGrid;
    internal Image Delete;
    internal Grid ContentGrid;
    internal EmojiEditor ContentTxt;
    internal StackPanel ExpandPanel;
    internal TextBlock ExpandOrCollapseBtn;
    internal Path ExpandPath;
    internal RotateTransform ExpandPathRotate;
    private bool _contentLoaded;

    public event EventHandler<CommentViewModel> ShowAddComment;

    public CommentItemControl()
    {
      this.InitializeComponent();
      this.Delete.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCommentDeleted);
      this.EditGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCommentEditClick);
      this.ReplyGrid.MouseLeftButtonUp += (MouseButtonEventHandler) ((sender, e) =>
      {
        CommentViewModel viewModel = this.GetViewModel();
        if (viewModel == null)
          return;
        EventHandler<CommentViewModel> showAddComment = this.ShowAddComment;
        if (showAddComment == null)
          return;
        showAddComment((object) null, viewModel);
      });
      this.ContentTxt.EditBox.PreviewKeyDown += new KeyEventHandler(this.OnTextKeyDown);
      this.ContentTxt.EditBox.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
      this.ContentTxt.KeysUp += new EventHandler<KeyEventArgs>(this.OnTextKeyUp);
      this.ContentTxt.EnterUp += new EventHandler(this.OnEnterUp);
    }

    private void OnEnterUp(object sender, EventArgs e)
    {
      if (Utils.IfShiftPressed() || this.ContentTxt.Text.Length > 1024)
        return;
      this.OnSaveClick((object) null, (RoutedEventArgs) null);
    }

    private void OnTextKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape)
        return;
      this.OnCancelClick((object) null, (RoutedEventArgs) null);
    }

    private void OnTextKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return || Utils.IfShiftPressed())
        return;
      e.Handled = true;
    }

    private async void OnCommentEditClick(object sender, MouseButtonEventArgs e)
    {
      CommentItemControl commentItemControl = this;
      commentItemControl.ContentTxt.BorderThickness = new Thickness(1.0);
      commentItemControl.ContentTxt.ReadOnly = false;
      commentItemControl.ContentTxt.BorderCorner = new CornerRadius(2.0);
      commentItemControl.ContentTxt.TextPadding = new Thickness(8.0, 8.0, 8.0, 36.0);
      commentItemControl.ContentTxt.SetResourceReference(EmojiEditor.BorderBrushProperty, (object) "BaseColorOpacity10");
      commentItemControl.ContentTxt.FocusEnd();
      CommentViewModel viewModel = commentItemControl.GetViewModel();
      if (viewModel != null)
        viewModel.Editing = true;
      commentItemControl.SetEditButton();
      await Task.Delay(100);
      Utils.FindParent<TaskDetailView>((DependencyObject) commentItemControl)?.OnCommentEdit(commentItemControl);
    }

    public bool Fold
    {
      get => (bool) this.GetValue(CommentItemControl.FoldProperty);
      set => this.SetValue(CommentItemControl.FoldProperty, (object) value);
    }

    private async void OnCommentDeleted(object sender, MouseButtonEventArgs e)
    {
      CommentViewModel viewModel = this.GetViewModel();
      if (viewModel == null)
        return;
      await viewModel.RemoveComment(viewModel);
    }

    protected override void OnMouseEnter(MouseEventArgs e)
    {
      base.OnMouseEnter(e);
      CommentViewModel viewModel = this.GetViewModel();
      if (viewModel == null || !viewModel.CanEdit || viewModel.Editing)
        return;
      if (viewModel.IsMySelf)
      {
        this.Delete.Visibility = Visibility.Visible;
        this.EditGrid.Visibility = Visibility.Visible;
        this.ReplyGrid.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.ReplyGrid.Visibility = Visibility.Visible;
        this.Delete.Visibility = Visibility.Collapsed;
        this.EditGrid.Visibility = Visibility.Collapsed;
      }
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
      base.OnMouseLeave(e);
      this.Delete.Visibility = Visibility.Collapsed;
      this.ReplyGrid.Visibility = Visibility.Collapsed;
      this.EditGrid.Visibility = Visibility.Collapsed;
    }

    private CommentViewModel GetViewModel() => this.DataContext as CommentViewModel;

    private void TextBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      this.ToggleExpandOrCollapse();
      if (this.DataContext is CommentViewModel dataContext)
        this.SetLastItemHeight(dataContext);
      e.Handled = true;
    }

    private void ToggleExpandOrCollapse()
    {
      if (this.Fold)
      {
        this.Fold = false;
        this.ExpandOrCollapseBtn.Text = Utils.GetString("Collapse");
        this.ExpandPathRotate.Angle = 180.0;
      }
      else
      {
        this.Fold = true;
        this.ExpandOrCollapseBtn.Text = Utils.GetString("expand");
        this.ExpandPathRotate.Angle = 0.0;
      }
    }

    private void OnDataBinded(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is CommentViewModel dataContext))
        return;
      this.ContentTxt.Text = dataContext.Content;
      this.SetLastItemHeight(dataContext);
    }

    private async void SetLastItemHeight(CommentViewModel model)
    {
      CommentItemControl commentItemControl = this;
      if (!model.IsLastOne())
        return;
      await Task.Delay(10);
      model.ListModel?.EditComment?.SetLastHeight(commentItemControl.ActualHeight);
    }

    private void OnTextSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (this.ContentTxt.EditBox.TextArea.TextView.GetVisualPosition(new TextViewPosition(this.ContentTxt.EditBox.Document.GetLocation(this.ContentTxt.Text.Length)), VisualYPosition.LineBottom).Y <= this.ContentGrid.MaxHeight)
        return;
      this.ContentTxt.SizeChanged -= new SizeChangedEventHandler(this.OnTextSizeChanged);
      this.ExpandPanel.Visibility = Visibility.Visible;
      this.ExpandOrCollapseBtn.Text = Utils.GetString("expand");
    }

    private void OnBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }

    private void SetEditButton()
    {
      StackPanel stackPanel = new StackPanel();
      stackPanel.Orientation = Orientation.Horizontal;
      stackPanel.VerticalAlignment = VerticalAlignment.Bottom;
      stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
      stackPanel.Margin = new Thickness(0.0, 0.0, 6.0, 6.0);
      StackPanel element1 = stackPanel;
      Button button1 = new Button();
      button1.MinWidth = 48.0;
      button1.Height = 24.0;
      button1.Content = (object) Utils.GetString("PublicSave");
      button1.Margin = new Thickness(0.0, 0.0, 8.0, 0.0);
      this._saveButton = button1;
      this._saveButton.SetResourceReference(FrameworkElement.StyleProperty, (object) "SaveButtonStyle");
      element1.Children.Add((UIElement) this._saveButton);
      this._saveButton.Click += new RoutedEventHandler(this.OnSaveClick);
      Button button2 = new Button();
      button2.MinWidth = 48.0;
      button2.Height = 24.0;
      button2.Content = (object) Utils.GetString("Cancel");
      Button element2 = button2;
      element2.Click += new RoutedEventHandler(this.OnCancelClick);
      element2.SetResourceReference(FrameworkElement.StyleProperty, (object) "CancelButtonStyle");
      element1.Children.Add((UIElement) element2);
      this.ContentGrid.Children.Add((UIElement) element1);
      TextBlock textBlock = new TextBlock();
      textBlock.VerticalAlignment = VerticalAlignment.Bottom;
      textBlock.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock.Margin = new Thickness(8.0, 0.0, 0.0, 6.0);
      textBlock.FontSize = 12.0;
      TextBlock element3 = textBlock;
      element3.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      element3.Visibility = this.ContentTxt.Text.Length >= 924 ? Visibility.Visible : Visibility.Collapsed;
      element3.Inlines.Add((Inline) new Run()
      {
        Text = (this.ContentTxt.Text.Length.ToString() ?? "")
      });
      Run run = new Run() { Text = "/1024" };
      run.SetResourceReference(TextElement.ForegroundProperty, (object) "BaseColorOpacity60");
      element3.Inlines.Add((Inline) run);
      this.ContentGrid.Children.Add((UIElement) element3);
      this.Delete.Visibility = Visibility.Collapsed;
      this.EditGrid.Visibility = Visibility.Collapsed;
      this.ExpandPanel.Opacity = 0.0;
      this.ContentTxt.EditBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
      this.ContentGrid.MouseEnter -= new MouseEventHandler(this.OnEditingMouseEnter);
      this.ContentGrid.MouseLeave -= new MouseEventHandler(this.OnEditingMouseLeave);
      this.ContentGrid.MouseEnter += new MouseEventHandler(this.OnEditingMouseEnter);
      this.ContentGrid.MouseLeave += new MouseEventHandler(this.OnEditingMouseLeave);
      Utils.FindParent<TaskDetailView>((DependencyObject) this)?.SetCanScroll(!this.ContentGrid.IsMouseOver);
    }

    private void OnEditingMouseLeave(object sender, MouseEventArgs e)
    {
      Utils.FindParent<TaskDetailView>((DependencyObject) this)?.SetCanScroll(true);
    }

    private void OnEditingMouseEnter(object sender, MouseEventArgs e)
    {
      Utils.FindParent<TaskDetailView>((DependencyObject) this)?.SetCanScroll(false);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      CommentViewModel viewModel = this.GetViewModel();
      if (viewModel != null)
      {
        viewModel.Content = this.ContentTxt.Text;
        viewModel.Save();
      }
      this.ExitEditMode();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      CommentViewModel viewModel = this.GetViewModel();
      if (viewModel != null)
        this.ContentTxt.Text = viewModel.Content;
      this.ExitEditMode();
    }

    private void ExitEditMode()
    {
      this.ContentTxt.BorderThickness = new Thickness(0.0);
      this.ContentTxt.ReadOnly = true;
      this.ContentTxt.BorderCorner = new CornerRadius(0.0);
      this.ContentTxt.TextPadding = new Thickness(0.0);
      this.ContentTxt.BorderBrush = (Brush) Brushes.Transparent;
      this.ContentTxt.EditBox.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
      CommentViewModel viewModel = this.GetViewModel();
      if (viewModel != null)
        viewModel.Editing = false;
      if (this.ContentGrid.Children.Count == 3)
        this.ContentGrid.Children.RemoveRange(1, 2);
      if (this.IsMouseOver)
      {
        this.Delete.Visibility = Visibility.Visible;
        this.EditGrid.Visibility = Visibility.Visible;
      }
      this.ExpandPanel.Opacity = 1.0;
      this._saveButton = (Button) null;
      this.ContentGrid.MouseEnter -= new MouseEventHandler(this.OnEditingMouseEnter);
      this.ContentGrid.MouseLeave -= new MouseEventHandler(this.OnEditingMouseLeave);
      Utils.FindParent<TaskDetailView>((DependencyObject) this)?.SetCanScroll(true);
    }

    private void OnCommentContentChanged(object sender, EventArgs e)
    {
      if (!(this.ContentGrid.Children[this.ContentGrid.Children.Count - 1] is TextBlock child))
        return;
      if (child.Inlines.FirstInline is Run firstInline)
      {
        firstInline.Text = this.ContentTxt.Text.Length.ToString() ?? "";
        if (this.ContentTxt.Text.Length > 1024)
          firstInline.Foreground = (Brush) Brushes.Red;
        else
          firstInline.SetResourceReference(TextElement.ForegroundProperty, (object) "BaseColorOpacity60");
      }
      child.Visibility = this.ContentTxt.Text.Length >= 924 ? Visibility.Visible : Visibility.Collapsed;
      if (this._saveButton == null)
        return;
      this._saveButton.IsEnabled = this.ContentTxt.Text.Length <= 1024;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/commentitemcontrol.xaml", UriKind.Relative));
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
          this.Root = (CommentItemControl) target;
          this.Root.RequestBringIntoView += new RequestBringIntoViewEventHandler(this.OnBringIntoView);
          this.Root.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBinded);
          break;
        case 2:
          this.ReplyGrid = (Image) target;
          break;
        case 3:
          this.EditGrid = (Image) target;
          break;
        case 4:
          this.Delete = (Image) target;
          break;
        case 5:
          this.ContentGrid = (Grid) target;
          break;
        case 6:
          this.ContentTxt = (EmojiEditor) target;
          break;
        case 7:
          this.ExpandPanel = (StackPanel) target;
          this.ExpandPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.TextBlock_MouseLeftButtonUp);
          break;
        case 8:
          this.ExpandOrCollapseBtn = (TextBlock) target;
          break;
        case 9:
          this.ExpandPath = (Path) target;
          break;
        case 10:
          this.ExpandPathRotate = (RotateTransform) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
