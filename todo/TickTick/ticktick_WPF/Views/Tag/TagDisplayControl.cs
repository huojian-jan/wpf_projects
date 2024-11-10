// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagDisplayControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class TagDisplayControl : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty CanClickTagProperty = DependencyProperty.Register(nameof (CanClickTag), typeof (bool), typeof (TagDisplayControl), new PropertyMetadata((object) true, (PropertyChangedCallback) null));
    private ObservableCollection<TagLabelViewModel> _models = new ObservableCollection<TagLabelViewModel>();
    public List<string> OriginalTags = new List<string>();
    internal TagDisplayControl Root;
    internal ItemsControl TagItems;
    private bool _contentLoaded;

    public TagDisplayControl()
    {
      this.InitializeComponent();
      this.OriginalTags = new List<string>();
      this.TagItems.ItemsSource = (IEnumerable) this._models;
    }

    public TagDisplayControl(IReadOnlyCollection<string> tags, bool editable)
    {
      this.OriginalTags = new List<string>((IEnumerable<string>) tags);
      this.InitializeComponent();
      this.TagItems.ItemsSource = (IEnumerable) this._models;
      this.LoadTagData(tags, editable);
    }

    public void InitData(IReadOnlyCollection<string> tags, bool editable)
    {
      this.OriginalTags = new List<string>((IEnumerable<string>) tags);
      this.LoadTagData(tags, editable);
    }

    public bool CanClickTag
    {
      get => (bool) this.GetValue(TagDisplayControl.CanClickTagProperty);
      set => this.SetValue(TagDisplayControl.CanClickTagProperty, (object) value);
    }

    public bool AddPopupOpened { get; set; }

    public event EventHandler<List<string>> TagsChanged;

    public event EventHandler<string> TagClick;

    public event EventHandler<Visibility> TagPanelVisibleChanged;

    public async void LoadTagData(IReadOnlyCollection<string> tags, bool editable = true)
    {
      TagDisplayControl tagDisplayControl = this;
      TagLabelViewModel tagLabelViewModel1 = tagDisplayControl._models.LastOrDefault<TagLabelViewModel>((Func<TagLabelViewModel, bool>) (m => m.IsAdd));
      List<TagLabelViewModel> list1 = tagDisplayControl._models.ToList<TagLabelViewModel>();
      list1.Remove(tagLabelViewModel1);
      // ISSUE: reference to a compiler-generated method
      list1.ForEach(new Action<TagLabelViewModel>(tagDisplayControl.\u003CLoadTagData\u003Eb__22_1));
      if (tags == null || tags.Count <= 0)
        return;
      if (tags.Count > 0)
      {
        List<TagModel> cachedTags = TagDataHelper.GetTags();
        List<TagLabelViewModel> list2 = tags.Select(tag => new
        {
          tag = tag,
          local = cachedTags.FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == tag))
        }).Select(_param1 => _param1.local == null ? new TagLabelViewModel(_param1.tag) : new TagLabelViewModel(_param1.local)).ToList<TagLabelViewModel>().OrderBy<TagLabelViewModel, long>((Func<TagLabelViewModel, long>) (m => m.SortOrder)).ToList<TagLabelViewModel>();
        for (int index = 0; index < list2.Count; ++index)
        {
          TagLabelViewModel tagLabelViewModel2 = list2[index];
          tagLabelViewModel2.Editable = editable;
          tagDisplayControl._models.Insert(index, tagLabelViewModel2);
        }
      }
      if (editable)
      {
        if (tagLabelViewModel1 != null)
          return;
        tagDisplayControl._models.Add(TagLabelViewModel.BuildAddModel());
      }
      else
        tagDisplayControl._models.Remove(tagLabelViewModel1);
    }

    public void AddNewTag(string tag)
    {
      if (this.OriginalTags.Contains(tag))
        return;
      this.OriginalTags.Add(tag);
      this.LoadTagData((IReadOnlyCollection<string>) this.OriginalTags);
      EventHandler<List<string>> tagsChanged = this.TagsChanged;
      if (tagsChanged == null)
        return;
      tagsChanged((object) this, this.OriginalTags);
    }

    public bool CheckIfTagExisted(string tag) => this.OriginalTags.Contains(tag.ToLower());

    public void RemoveTag(string tag)
    {
      this.OriginalTags.Remove(tag);
      this.LoadTagData((IReadOnlyCollection<string>) this.OriginalTags);
      EventHandler<List<string>> tagsChanged = this.TagsChanged;
      if (tagsChanged == null)
        return;
      tagsChanged((object) this, this.OriginalTags);
    }

    public void NotifyTagClick(string tag)
    {
      EventHandler<string> tagClick = this.TagClick;
      if (tagClick == null)
        return;
      tagClick((object) this, tag);
    }

    public void NotifyTagPanelVisibleChanged(Visibility visibility)
    {
      this.SetAddPopupOpened(visibility == Visibility.Visible);
      EventHandler<Visibility> panelVisibleChanged = this.TagPanelVisibleChanged;
      if (panelVisibleChanged == null)
        return;
      panelVisibleChanged((object) this, visibility);
    }

    public void SetAddPopupOpened(bool opened) => this.AddPopupOpened = opened;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/tagdisplaycontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.TagItems = (ItemsControl) target;
        else
          this._contentLoaded = true;
      }
      else
        this.Root = (TagDisplayControl) target;
    }
  }
}
