// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.UndoToast
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class UndoToast : UserControl, IComponentConnector
  {
    private UndoController _controller;
    private bool _restored;
    internal Border Container;
    internal TextBlock TitleText;
    internal TextBlock DeletedOrCompleted;
    private bool _contentLoaded;

    public UndoToast() => this.InitializeComponent();

    public UndoToast(UndoController controller)
    {
      this.InitializeComponent();
      this._controller = controller;
      this._restored = false;
      this.TitleText.Text = this._controller.GetTitle();
      string content = this._controller.GetContent();
      if (string.IsNullOrEmpty(content))
        return;
      this.DeletedOrCompleted.Text = content;
    }

    public void InitEventUndo(string eventId, string eventTitle)
    {
      this._controller = (UndoController) new EventUndo(eventId, eventTitle);
      this._restored = false;
      this.TitleText.Text = this._controller.GetTitle();
    }

    public void InitTaskUndo(string taskId, string taskTitle, bool isEmptyDelete = false)
    {
      this._controller = (UndoController) new TaskDeleteUndo(taskId, taskTitle, isEmptyDelete);
      this._restored = false;
      this.TitleText.Text = this._controller.GetTitle();
    }

    public void InitBatchTaskUndo(IEnumerable<string> taskIds, IEnumerable<string> deleteForever = null)
    {
      this._controller = (UndoController) new BatchTaskDeleteUndo(taskIds.ToList<string>(), deleteForever != null ? deleteForever.ToList<string>() : (List<string>) null);
      this._restored = false;
    }

    public void InitSubtaskUndo(TaskDetailItemModel subtaskId)
    {
      this._controller = (UndoController) new SubTaskUndo(subtaskId);
      this._restored = false;
      this.TitleText.Text = this._controller.GetTitle();
    }

    private async void OnUndoClick(object sender, MouseButtonEventArgs e) => this.OnUndo();

    public virtual void OnFinished()
    {
      if (this._restored)
        return;
      this._controller.Finished();
      SyncManager.TryDelaySync();
    }

    public virtual void OnUndo()
    {
      this.Visibility = Visibility.Collapsed;
      this._restored = true;
      this._controller.Undo();
    }

    public void InitTaskUndo(TaskDeleteRecurrenceUndoEntity undoModel)
    {
      this._controller = (UndoController) new DeleteRecurrenceUndo(undoModel);
      this._restored = false;
      this.TitleText.Text = TaskCache.GetTaskById(undoModel.sid)?.Title;
    }

    public void SetVisible(bool show)
    {
      this.Container.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/undotoast.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Container = (Border) target;
          break;
        case 2:
          this.TitleText = (TextBlock) target;
          break;
        case 3:
          this.DeletedOrCompleted = (TextBlock) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnUndoClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
