// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Activity.TaskActivityItem
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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.Views.MarkDown;

#nullable disable
namespace ticktick_WPF.Views.Activity
{
  public class TaskActivityItem : UserControl, IComponentConnector
  {
    internal Grid ContentGrid;
    internal EmojiEditor ContentTxt;
    internal StackPanel ExpandPanel;
    internal TextBlock ExpandOrCollapseBtn;
    internal Path ExpandPath;
    internal RotateTransform ExpandPathRotate;
    private bool _contentLoaded;

    public TaskActivityItem()
    {
      this.InitializeComponent();
      this.ContentTxt.TextChanged += new EventHandler(this.OnTextChanged);
      this.ContentTxt.EditBox.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
    }

    private async void OnTextChanged(object sender, EventArgs e)
    {
      TaskActivityItem taskActivityItem = this;
      taskActivityItem.UpdateLayout();
      await Task.Delay(10);
      if (taskActivityItem.ContentTxt.EditBox.TextArea.TextView.GetVisualPosition(new TextViewPosition(taskActivityItem.ContentTxt.EditBox.Document.GetLocation(taskActivityItem.ContentTxt.Text.Length)), VisualYPosition.LineBottom).Y <= (double) taskActivityItem.FindResource((object) "Height68"))
        return;
      taskActivityItem.ExpandPanel.Visibility = Visibility.Visible;
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue is TaskActivityViewModel newValue && !string.IsNullOrEmpty(newValue.Description))
      {
        this.ExpandPanel.Visibility = Visibility.Collapsed;
        this.ExpandPathRotate.Angle = newValue.Fold ? 0.0 : 180.0;
        this.ExpandOrCollapseBtn.Text = Utils.GetString(newValue.Fold ? "expand" : "Collapse");
        this.ContentTxt.Text = !newValue.Fold || newValue.Description.Length <= newValue.MaxDescLength ? newValue.Description : newValue.Description.Substring(0, newValue.MaxDescLength);
        this.ContentTxt.Visibility = Visibility.Visible;
      }
      else
      {
        this.ExpandPanel.Visibility = Visibility.Collapsed;
        this.ContentTxt.Visibility = Visibility.Collapsed;
      }
    }

    private void ExpandClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TaskActivityViewModel dataContext))
        return;
      if (this.ContentTxt.Text != dataContext.Description)
      {
        this.ContentTxt.TextChanged -= new EventHandler(this.OnTextChanged);
        this.ContentTxt.Text = dataContext.Description;
        this.ContentTxt.TextChanged += new EventHandler(this.OnTextChanged);
      }
      if (dataContext.Fold)
      {
        dataContext.Fold = false;
        this.ExpandOrCollapseBtn.Text = Utils.GetString("Collapse");
        this.ExpandPathRotate.Angle = 180.0;
      }
      else
      {
        dataContext.Fold = true;
        this.ExpandOrCollapseBtn.Text = Utils.GetString("expand");
        this.ExpandPathRotate.Angle = 0.0;
      }
    }

    private void OnBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/activity/taskactivityitem.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
          break;
        case 2:
          this.ContentGrid = (Grid) target;
          break;
        case 3:
          this.ContentTxt = (EmojiEditor) target;
          break;
        case 4:
          this.ExpandPanel = (StackPanel) target;
          this.ExpandPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.ExpandClick);
          break;
        case 5:
          this.ExpandOrCollapseBtn = (TextBlock) target;
          break;
        case 6:
          this.ExpandPath = (Path) target;
          break;
        case 7:
          this.ExpandPathRotate = (RotateTransform) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
