// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagSearchFilterControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class TagSearchFilterControl : UserControl, IComponentConnector
  {
    internal TagSelectionControl TagItems;
    private bool _contentLoaded;

    public event EventHandler Cancel;

    public event EventHandler<List<string>> Save;

    public TagSearchFilterControl(List<string> tags)
    {
      this.InitializeComponent();
      this.TagItems.SetSelectedTags(new TagSelectData()
      {
        OmniSelectTags = tags != null ? tags.Select<string, string>((Func<string, string>) (t => t.ToLower())).ToList<string>() : (List<string>) null
      });
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      List<string> e1 = this.TagItems.GetSelectedTags();
      if (e1.Contains("*tag"))
        e1 = new List<string>();
      EventHandler<List<string>> save = this.Save;
      if (save == null)
        return;
      save((object) this, e1);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler cancel = this.Cancel;
      if (cancel == null)
        return;
      cancel((object) this, (EventArgs) null);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/tagsearchfiltercontrol.xaml", UriKind.Relative));
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
          this.TagItems = (TagSelectionControl) target;
          break;
        case 2:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
