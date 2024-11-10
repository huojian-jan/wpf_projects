// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchDialog
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
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchDialog : MyWindow, IOkCancelWindow, IComponentConnector
  {
    private bool _restore;
    internal SearchInputControl SearchInput;
    private bool _contentLoaded;

    public SearchDialog(bool restore)
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Closed += new EventHandler(this.OnClosed);
      this.Activated += new EventHandler(this.OnWindowActivated);
      this._restore = restore;
      SearchProjectHelper.UpdateSortOrder();
      this.SearchInput.InitSearch(restore);
    }

    private void OnClosed(object sender, EventArgs e) => this.SearchInput.Dispose();

    public bool Showing { get; set; } = true;

    private void OnWindowActivated(object sender, EventArgs e) => this.SearchInput.FocusEnd();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.SearchInput.Search -= new EventHandler<SearchExtra>(this.OnSearch);
      this.SearchInput.Search += new EventHandler<SearchExtra>(this.OnSearch);
      this.SearchInput.Close -= new EventHandler<EventArgs>(this.OnInputClose);
      this.SearchInput.Close += new EventHandler<EventArgs>(this.OnInputClose);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      this.Owner?.Activate();
      this.Showing = false;
      base.OnClosing(e);
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      if (this.GetTemplateChild("TitleGrid") is Grid templateChild)
        templateChild.Visibility = Visibility.Collapsed;
      base.OnApplyTemplate();
    }

    private void OnInputClose(object sender, EventArgs e)
    {
      if (!LocalSettings.Settings.InSearch)
        App.Window.ResetLeftSelected();
      this.Close();
    }

    private void OnSearch(object sender, SearchExtra e)
    {
      App.Window.OnSearch(e, this._restore);
      this.Close();
    }

    public void OnCancel()
    {
      App.Window.ResetLeftSelected();
      this.Close();
    }

    public async void Ok()
    {
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || !this.SearchInput.CanDragMove())
        return;
      this.DragMove();
    }

    public void TryClose()
    {
      try
      {
        App.Window.ResetLeftSelected();
        this.Close();
      }
      catch (Exception ex)
      {
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/search/searchdialog.xaml", UriKind.Relative));
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
        this.SearchInput = (SearchInputControl) target;
      else
        this._contentLoaded = true;
    }
  }
}
