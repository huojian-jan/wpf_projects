// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.TaskDetailHead
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class TaskDetailHead : Grid, IComponentConnector
  {
    private bool _selectTimeMouseDown;
    internal Border BackBtn;
    internal Grid CheckIcon;
    internal Path CheckPath;
    internal Grid DateGrid;
    internal Grid ChooseTimeGrid;
    internal Path DatePath;
    internal Grid AssignGrid;
    internal Border AvatarImageRectangle;
    internal ImageBrush AvatarImage;
    internal HoverIconButton AssignOtherGrid;
    internal EscPopup SetAssignPopup;
    internal HoverIconButton SetPriorityGrid;
    internal EscPopup SetPriorityPopup;
    internal Border NoteMessageGrid;
    internal Popup NoteMessagePopup;
    internal TextBlock TextCount;
    internal TextBlock CreatedAt;
    internal TextBlock ModifiedAt;
    internal TaskProgressControl TaskProgressGrid;
    internal Grid ProgressPreviewGrid;
    private bool _contentLoaded;

    public TaskDetailHead(TaskDetailView detailControl) => this.InitializeComponent();

    public void SetBackIconEnabled(bool enable)
    {
      this.BackBtn.IsEnabled = enable;
      this.BackBtn.Visibility = enable ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnBackClick(object sender, MouseButtonEventArgs e)
    {
      this.BackBtn.Visibility = Visibility.Collapsed;
      e.Handled = true;
    }

    private void OnCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
    }

    private void OnCheckBoxMouseRightUp(object sender, MouseButtonEventArgs e)
    {
    }

    private void SelectDateClick(object sender, MouseButtonEventArgs e)
    {
      this._selectTimeMouseDown = false;
    }

    private async void OnSetAssigneeMouseUp(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
    }

    private void SetPriorityMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (this.SetPriorityPopup.IsOpen)
      {
        this.SetPriorityPopup.IsOpen = false;
      }
      else
      {
        if (!(this.DataContext is TaskDetailViewModel dataContext))
          return;
        SetPriorityDialog setPriorityDialog = new SetPriorityDialog(dataContext.Priority);
        this.SetPriorityPopup.Child = (UIElement) setPriorityDialog;
        this.SetPriorityPopup.IsOpen = true;
        // ISSUE: method pointer
        setPriorityDialog.PrioritySelect += new EventHandler<int>((object) this, __methodptr(\u003CSetPriorityMouseUp\u003Eg__OnPrioritySelected\u007C8_0));
      }
    }

    private void NoteMessageClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TaskDetailViewModel dataContext))
        return;
      this.TextCount.Text = Utils.IntToStringWithDivide(string.IsNullOrEmpty(dataContext.TaskContent) ? 0 : dataContext.TaskContent.Replace("\r", "").Replace("\n", "").Replace(" ", "").Length);
      TextBlock createdAt = this.CreatedAt;
      DateTime? nullable = dataContext.CreateDate;
      ref DateTime? local1 = ref nullable;
      string str1 = local1.HasValue ? local1.GetValueOrDefault().ToString("D", (IFormatProvider) App.Ci) : (string) null;
      createdAt.Text = str1;
      TextBlock modifiedAt = this.ModifiedAt;
      nullable = dataContext.ModifiedDate;
      ref DateTime? local2 = ref nullable;
      string str2 = local2.HasValue ? local2.GetValueOrDefault().ToString("D", (IFormatProvider) App.Ci) : (string) null;
      modifiedAt.Text = str2;
      this.NoteMessagePopup.IsOpen = true;
    }

    private async void ProgressPreviewMouseMove(object sender, MouseEventArgs e)
    {
      if (!(this.DataContext is TaskDetailViewModel dataContext))
        return;
      int num = dataContext.Enable ? 1 : 0;
    }

    private void ProgressMouseLeave(object sender, MouseEventArgs e)
    {
      DelayActionHandlerCenter.RemoveAction("DetailShowProgressPointer");
      this.TaskProgressGrid.HidePointer();
    }

    private void ProgressClick(object sender, MouseButtonEventArgs e)
    {
      this.TaskProgressGrid.GetPointerProgress();
      this.TaskProgressGrid.GetProgress();
    }

    public async Task SetProgress(int pointerProgress, bool withAnimal = false)
    {
      this.TaskProgressGrid.SetProgress(pointerProgress, withAnimal);
    }

    public async Task SetAvatarVisible(
      bool showAssignOther,
      bool showAvatarImage,
      string avatarUrl)
    {
      ImageBrush imageBrush = this.AvatarImage;
      imageBrush.ImageSource = (ImageSource) await AvatarHelper.GetAvatarByUrlAsync(avatarUrl);
      imageBrush = (ImageBrush) null;
      this.AvatarImageRectangle.Visibility = showAvatarImage ? Visibility.Visible : Visibility.Collapsed;
      this.AssignOtherGrid.Visibility = showAssignOther ? Visibility.Visible : Visibility.Collapsed;
    }

    public void CheckProgress()
    {
      if (!(this.DataContext is TaskDetailViewModel dataContext))
        return;
      int progress1 = this.TaskProgressGrid.GetProgress();
      int? progress2 = dataContext.Progress;
      int valueOrDefault1 = progress2.GetValueOrDefault();
      if (progress1 == valueOrDefault1 & progress2.HasValue)
        return;
      TaskProgressControl taskProgressGrid = this.TaskProgressGrid;
      progress2 = dataContext.Progress;
      int valueOrDefault2 = progress2.GetValueOrDefault();
      taskProgressGrid.SetProgress(valueOrDefault2);
    }

    private void OnSelectTimeMouseDown(object sender, MouseButtonEventArgs e)
    {
    }

    public void SetPriorityIcon(int priority)
    {
      DrawingImage drawingImage = (DrawingImage) null;
      switch (priority)
      {
        case 0:
          drawingImage = Utils.GetImageSource("NonePriorityDrawingImage", (FrameworkElement) this);
          break;
        case 1:
          drawingImage = Utils.GetImageSource("LowPriorityDrawingImage");
          break;
        case 3:
          drawingImage = Utils.GetImageSource("MidPriorityDrawingImage");
          break;
        case 5:
          drawingImage = Utils.GetImageSource("HighPriorityDrawingImage");
          break;
      }
      this.SetPriorityGrid.ImageSource = (ImageSource) drawingImage;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/taskdetailhead.xaml", UriKind.Relative));
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
          this.BackBtn = (Border) target;
          this.BackBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBackClick);
          break;
        case 2:
          this.CheckIcon = (Grid) target;
          this.CheckIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCheckBoxClick);
          this.CheckIcon.MouseRightButtonUp += new MouseButtonEventHandler(this.OnCheckBoxMouseRightUp);
          break;
        case 3:
          this.CheckPath = (Path) target;
          break;
        case 4:
          this.DateGrid = (Grid) target;
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectDateClick);
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnSelectTimeMouseDown);
          break;
        case 6:
          this.ChooseTimeGrid = (Grid) target;
          break;
        case 7:
          this.DatePath = (Path) target;
          break;
        case 8:
          this.AssignGrid = (Grid) target;
          this.AssignGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSetAssigneeMouseUp);
          break;
        case 9:
          this.AvatarImageRectangle = (Border) target;
          break;
        case 10:
          this.AvatarImage = (ImageBrush) target;
          break;
        case 11:
          this.AssignOtherGrid = (HoverIconButton) target;
          break;
        case 12:
          this.SetAssignPopup = (EscPopup) target;
          break;
        case 13:
          this.SetPriorityGrid = (HoverIconButton) target;
          break;
        case 14:
          this.SetPriorityPopup = (EscPopup) target;
          break;
        case 15:
          this.NoteMessageGrid = (Border) target;
          this.NoteMessageGrid.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.NoteMessageClick);
          break;
        case 16:
          this.NoteMessagePopup = (Popup) target;
          break;
        case 17:
          this.TextCount = (TextBlock) target;
          break;
        case 18:
          this.CreatedAt = (TextBlock) target;
          break;
        case 19:
          this.ModifiedAt = (TextBlock) target;
          break;
        case 20:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectDateClick);
          break;
        case 21:
          this.TaskProgressGrid = (TaskProgressControl) target;
          break;
        case 22:
          this.ProgressPreviewGrid = (Grid) target;
          this.ProgressPreviewGrid.PreviewMouseMove += new MouseEventHandler(this.ProgressPreviewMouseMove);
          this.ProgressPreviewGrid.MouseLeave += new MouseEventHandler(this.ProgressMouseLeave);
          this.ProgressPreviewGrid.MouseLeftButtonDown += new MouseButtonEventHandler(this.ProgressClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
