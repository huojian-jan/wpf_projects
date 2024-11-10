// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.DetailHintPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class DetailHintPanel : StackPanel, IComponentConnector
  {
    internal Image EmptyImage;
    internal Path TaskEmptyPath;
    internal Polygon HabitEmptyPath;
    internal TextBlock EmptyText;
    private bool _contentLoaded;

    public DetailHintPanel(bool isTask, bool isNote)
    {
      this.InitializeComponent();
      this.SetEmptyImageVisible(isTask, isNote);
    }

    public void SetEmptyImageVisible(bool isTask, bool isNote)
    {
      if (isTask | isNote)
      {
        this.EmptyImage.SetResourceReference(Image.SourceProperty, (object) "EmptyDetailDrawingImage");
        this.TaskEmptyPath.Visibility = Visibility.Visible;
        this.HabitEmptyPath.Visibility = Visibility.Collapsed;
        this.EmptyText.Text = Utils.GetString(isNote ? "NoteDetailEmpty" : "TaskDetailEmpty");
      }
      else
      {
        this.EmptyImage.SetResourceReference(Image.SourceProperty, (object) "HabitDetailEmptyDrawingImage");
        this.HabitEmptyPath.Visibility = Visibility.Visible;
        this.TaskEmptyPath.Visibility = Visibility.Collapsed;
        this.EmptyText.Text = Utils.GetString("HabitDetailEmpty");
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/detailhintpanel.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.EmptyImage = (Image) target;
          break;
        case 2:
          this.TaskEmptyPath = (Path) target;
          break;
        case 3:
          this.HabitEmptyPath = (Polygon) target;
          break;
        case 4:
          this.EmptyText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
