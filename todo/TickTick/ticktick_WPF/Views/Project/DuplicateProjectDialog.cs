// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.DuplicateProjectDialog
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

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class DuplicateProjectDialog : Window, IComponentConnector
  {
    public DuplicateProjectEnum Option;
    internal RadioButton UncompletedRadio;
    internal RadioButton KeepCompletedRadio;
    internal RadioButton ResetStatusRadio;
    internal Button OkButton;
    internal Button CancelButton;
    internal Button CloseButton;
    private bool _contentLoaded;

    public DuplicateProjectDialog() => this.InitializeComponent();

    private void OnOkClick(object sender, RoutedEventArgs e)
    {
      if (this.UncompletedRadio.IsChecked.GetValueOrDefault())
        this.Option = DuplicateProjectEnum.OnlyUncompleted;
      if (this.KeepCompletedRadio.IsChecked.GetValueOrDefault())
        this.Option = DuplicateProjectEnum.KeepCompleted;
      if (this.ResetStatusRadio.IsChecked.GetValueOrDefault())
        this.Option = DuplicateProjectEnum.Default;
      this.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnCloseClick(object sender, RoutedEventArgs e) => this.Close();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/project/duplicateprojectdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.UncompletedRadio = (RadioButton) target;
          break;
        case 2:
          this.KeepCompletedRadio = (RadioButton) target;
          break;
        case 3:
          this.ResetStatusRadio = (RadioButton) target;
          break;
        case 4:
          this.OkButton = (Button) target;
          this.OkButton.Click += new RoutedEventHandler(this.OnOkClick);
          break;
        case 5:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 6:
          this.CloseButton = (Button) target;
          this.CloseButton.Click += new RoutedEventHandler(this.OnCloseClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
