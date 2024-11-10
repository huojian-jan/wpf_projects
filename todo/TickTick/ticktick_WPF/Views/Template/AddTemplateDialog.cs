// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Template.AddTemplateDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Config;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.QuickAdd;

#nullable disable
namespace ticktick_WPF.Views.Template
{
  public class AddTemplateDialog : Window, IComponentConnector
  {
    private string _templateId;
    internal TemplateControl TemplateControl;
    private bool _contentLoaded;

    public AddTemplateDialog(
      TemplateKind kind,
      AddTaskViewModel addModel = null,
      ListViewContainer listView = null)
    {
      this.InitializeComponent();
      this.TemplateControl.TemplateSelected -= new EventHandler<TemplateViewModel>(this.OnTemplateSelected);
      this.TemplateControl.TemplateSelected += new EventHandler<TemplateViewModel>(this.OnTemplateSelected);
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding(WindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
      this.Title = Utils.GetString(kind == TemplateKind.Note ? "NoteTemplate" : "TaskTemplate");
      this.TemplateControl.Init(kind, addModel, listView);
    }

    public event EventHandler<string> TemplateSelected;

    private void OnTemplateSelected(object sender, TemplateViewModel model)
    {
      if (model != null)
      {
        EventHandler<string> templateSelected = this.TemplateSelected;
        if (templateSelected != null)
          templateSelected((object) this, model.Id);
      }
      this.Close();
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/template/addtemplatedialog.xaml", UriKind.Relative));
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
        this.TemplateControl = (TemplateControl) target;
      else
        this._contentLoaded = true;
    }
  }
}
