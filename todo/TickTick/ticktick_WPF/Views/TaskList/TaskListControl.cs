// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.TaskListControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Event;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Kanban.Item;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class TaskListControl : UserControl, IComponentConnector
  {
    private int _caretIndex = -1;
    private bool _isLoading;
    private ProjectIdentity _projectIdentity;
    private string _selectId = string.Empty;
    private string NewAddId = string.Empty;
    private TaskSelectType _selectType;
    private int _dragCurrentIndex = -1;
    private double _dragStartX;
    private int _dragStartIndex = -1;
    private int _parentLevel = -1;
    private DisplayItemModel _dragCheckItem;
    private DisplayItemModel _frontItem;
    private bool _noteDragging;
    private System.Windows.Point _dragPoint;
    private bool _splitting;
    private Popup _quickSetPopup;
    private DisplayItemModel _selectedModel;
    private DateTime _autoScrollTime;
    internal Grid ListGrid;
    internal ListView TaskList;
    internal Popup TaskDragPopup;
    internal TaskPopupItem TaskPopupItem;
    internal Border SectionPopupItem;
    internal Popup RecordPopup;
    internal StackPanel RecordHintGrid;
    internal TextBlock CheckInAmountText;
    private bool _contentLoaded;

    public event EventHandler<string> NavigateTask;

    public event EventHandler NeedReload;

    public TaskListControl() => this.InitializeComponent();

    public ObservableCollection<DisplayItemModel> DisplayModels
    {
      get
      {
        return this.TaskList.ItemsSource is ObservableCollection<DisplayItemModel> itemsSource ? itemsSource : new ObservableCollection<DisplayItemModel>();
      }
    }

    public string SelectedId
    {
      get => this._selectId;
      set => this._selectId = value;
    }

    private bool IsAssignToMeProject => this._projectIdentity is AssignToMeProjectIdentity;

    public List<string> SelectedTaskIds { get; set; } = new List<string>();

    public bool InDetail { get; set; }

    public bool InKanban { get; set; }

    public int QuadrantLevel { get; set; }

    private bool InMatrix => this.QuadrantLevel > 0;

    public bool IsMainList { get; set; }

    public event EventHandler<List<string>> BatchSelect;

    public event EventHandler<DragMouseEvent> DragOver;

    public event EventHandler<string> DragDropped;

    public event EventHandler<List<string>> BatchDragDropped;

    public event EventHandler<List<DisplayItemModel>> ItemsChanged;

    public event EventHandler ItemsCountChanged;

    public event EventHandler MoveUpCaret;

    public ProjectIdentity ProjectIdentity => this._projectIdentity;

    public bool IsLocked { get; set; }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tasklist/tasklistcontrol.xaml", UriKind.Relative));
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
          this.ListGrid = (Grid) target;
          break;
        case 2:
          this.TaskList = (ListView) target;
          break;
        case 3:
          this.TaskDragPopup = (Popup) target;
          break;
        case 4:
          this.TaskPopupItem = (TaskPopupItem) target;
          break;
        case 5:
          this.SectionPopupItem = (Border) target;
          break;
        case 6:
          this.RecordPopup = (Popup) target;
          break;
        case 7:
          this.RecordHintGrid = (StackPanel) target;
          break;
        case 8:
          this.CheckInAmountText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
