// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.MergeTagWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class MergeTagWindow : Window, IOkCancelWindow, IComponentConnector
  {
    public string MergeTag;
    private readonly TagModel _tag;
    internal TextBlock TagHint;
    internal CustomComboBox TagComobox;
    internal Button SaveButton;
    private bool _contentLoaded;

    public MergeTagWindow(TagModel tag)
    {
      string displayName = tag.GetDisplayName();
      this._tag = tag;
      this.InitializeComponent();
      this.TagHint.Text = string.Format(Utils.GetString("MergeTagHint"), (object) displayName);
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      MergeTagWindow mergeTagWindow = this;
      mergeTagWindow.SaveButton.Content = (object) Utils.GetString("Merging");
      mergeTagWindow.SaveButton.IsEnabled = false;
      await TagService.MergeTag(mergeTagWindow._tag.name, mergeTagWindow.MergeTag);
      mergeTagWindow.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      List<TagModel> list = TagDataHelper.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => t.name != this._tag.name)).ToList<TagModel>();
      if (list.Any<TagModel>((Func<TagModel, bool>) (t => t.parent == this._tag.name)))
        list = list.Where<TagModel>((Func<TagModel, bool>) (t => !t.IsChild())).ToList<TagModel>();
      ObservableCollection<ComboBoxViewModel> items = new ObservableCollection<ComboBoxViewModel>(list.Select<TagModel, string>((Func<TagModel, string>) (t => t.GetDisplayName())).ToList<string>().Select<string, ComboBoxViewModel>((Func<string, ComboBoxViewModel>) (t => new ComboBoxViewModel((object) t, t, 32.0))));
      this.TagComobox.Init<ComboBoxViewModel>(items, items[0]);
      this.TagComobox.ItemSelected += new EventHandler<ComboBoxViewModel>(this.OnTagSelected);
      this.TagComobox.SelectedText.Text = Utils.GetString("SelectTag");
    }

    private void OnTagSelected(object sender, ComboBoxViewModel e)
    {
      this.MergeTag = e.Title;
      this.SaveButton.IsEnabled = true;
    }

    public void OnCancel()
    {
      if (this.TagComobox.IsOpen)
        return;
      this.Close();
    }

    public async void Ok()
    {
      MergeTagWindow mergeTagWindow = this;
      if (mergeTagWindow.TagComobox.IsOpen || !mergeTagWindow.SaveButton.IsEnabled)
        return;
      mergeTagWindow.SaveButton.Content = (object) Utils.GetString("Merging");
      mergeTagWindow.SaveButton.IsEnabled = false;
      await TagService.MergeTag(mergeTagWindow._tag.name, mergeTagWindow.MergeTag);
      mergeTagWindow.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/mergetagwindow.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnWindowLoaded);
          break;
        case 2:
          this.TagHint = (TextBlock) target;
          break;
        case 3:
          this.TagComobox = (CustomComboBox) target;
          break;
        case 4:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 5:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
