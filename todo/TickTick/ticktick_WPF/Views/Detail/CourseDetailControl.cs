// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Detail.CourseDetailControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Detail
{
  public class CourseDetailControl : Grid, IComponentConnector
  {
    internal Border BackBtn;
    internal ListView CourseItems;
    private bool _contentLoaded;

    public string CourseId { get; set; }

    public event EventHandler HideDetail;

    public CourseDetailControl() => this.InitializeComponent();

    public async void NavigateCourse(string id)
    {
      CourseDetailControl courseDetailControl = this;
      if (string.IsNullOrEmpty(id))
        return;
      int length = id.IndexOf("_", StringComparison.Ordinal);
      if (length > 0)
        id = id.Substring(0, length);
      courseDetailControl.CourseId = id;
      CourseDetailViewModel detailViewModelById = await CourseDetailViewModel.GetDetailViewModelById(id);
      if (detailViewModelById != null)
        courseDetailControl.DataContext = (object) detailViewModelById;
      if (detailViewModelById == null)
        return;
      int? count = detailViewModelById.Items?.Count;
      int num = 0;
      if (!(count.GetValueOrDefault() > num & count.HasValue))
        return;
      courseDetailControl.CourseItems.ScrollIntoView((object) detailViewModelById.Items[0]);
    }

    private void OnBackClick(object sender, MouseButtonEventArgs e)
    {
      EventHandler hideDetail = this.HideDetail;
      if (hideDetail == null)
        return;
      hideDetail((object) this, (EventArgs) null);
    }

    public void Reload() => this.NavigateCourse(this.CourseId);

    public void ClearEvent() => this.HideDetail = (EventHandler) null;

    public void ShowBackMenu() => this.BackBtn.Visibility = Visibility.Visible;

    public void HideBackMenu() => this.BackBtn.Visibility = Visibility.Collapsed;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/detail/coursedetailcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.CourseItems = (ListView) target;
        else
          this._contentLoaded = true;
      }
      else
      {
        this.BackBtn = (Border) target;
        this.BackBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBackClick);
      }
    }
  }
}
