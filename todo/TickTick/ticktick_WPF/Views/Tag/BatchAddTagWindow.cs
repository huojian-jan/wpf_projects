// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.BatchAddTagWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class BatchAddTagWindow : MyWindow, IComponentConnector
  {
    private TagSelectData _originalDatas;
    private readonly bool _useInDetail;
    private Window _context;
    private bool _locationSet;
    private readonly System.Windows.Point _position;
    private bool _isClosed;
    private bool _inFilter;
    private Dictionary<string, long> _sortDict;
    private bool _canDelete;
    private readonly List<string> _needAddTags = new List<string>();
    internal BatchSetTagControl SetTagControl;
    private bool _contentLoaded;

    public event EventHandler<TagSelectData> TagsSelect;

    public event EventHandler WindowHide;

    public bool IsClosed => this._isClosed;

    public BatchAddTagWindow(Window context, TagSelectData tags, bool useInDetail = true, System.Windows.Point position = default (System.Windows.Point))
    {
      this.InitializeComponent();
      this._context = context;
      this._position = position;
      this._useInDetail = useInDetail;
      this.SetTagControl.Init(tags, true);
      this.SetTagControl.Close += (EventHandler) ((o, e) => this.CloseWindow());
      this.SetTagControl.TagsSelect += (EventHandler<TagSelectData>) ((o, e) =>
      {
        EventHandler<TagSelectData> tagsSelect = this.TagsSelect;
        if (tagsSelect == null)
          return;
        tagsSelect(o, e);
      });
    }

    private static System.Windows.Point GetMousePosition()
    {
      System.Drawing.Point mousePosition = Control.MousePosition;
      return new System.Windows.Point((double) mousePosition.X, (double) mousePosition.Y);
    }

    private void SetWindowLocation()
    {
      if (!this._useInDetail)
      {
        if (this._locationSet)
          return;
        this._locationSet = true;
        if (this._position == new System.Windows.Point())
        {
          CompositionTarget compositionTarget = PresentationSource.FromVisual((Visual) this)?.CompositionTarget;
          if (compositionTarget == null)
            return;
          System.Windows.Point point = compositionTarget.TransformFromDevice.Transform(BatchAddTagWindow.GetMousePosition());
          this.Left = point.X - this.ActualWidth / 2.0;
          this.Top = point.Y - this.ActualHeight / 2.0 - 40.0;
        }
        else
        {
          this.Left = this._position.X;
          this.Top = this._position.Y;
        }
      }
      else
      {
        if (this._context == null)
          this._context = (Window) App.Window;
        double actualWidth = this._context.ActualWidth;
        double actualHeight = this._context.ActualHeight;
        double num1 = this._context.Left;
        double num2 = this._context.Top;
        if (this._context.WindowState == WindowState.Maximized)
        {
          num1 = -10.0;
          num2 = -10.0;
        }
        this.Left = actualWidth + num1 - this.ActualWidth;
        this.Top = actualHeight + num2 - this.ActualHeight - 40.0;
      }
    }

    private void OnWindowDeactivated(object sender, EventArgs e) => this.CloseWindow();

    private void CloseWindow()
    {
      if (this._isClosed)
        return;
      this._isClosed = true;
      EventHandler windowHide = this.WindowHide;
      if (windowHide != null)
        windowHide((object) this, (EventArgs) null);
      this.TagsSelect = (EventHandler<TagSelectData>) null;
      this.WindowHide = (EventHandler) null;
      this.Close();
    }

    private void OnWindowSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.SetWindowLocation();
    }

    public void ClearEvent() => this.TagsSelect = (EventHandler<TagSelectData>) null;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/batchaddtagwindow.xaml", UriKind.Relative));
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
      if (connectionId == 1)
        this.SetTagControl = (BatchSetTagControl) target;
      else
        this._contentLoaded = true;
    }
  }
}
