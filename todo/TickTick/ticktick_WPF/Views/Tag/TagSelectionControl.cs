// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagSelectionControl
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
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.QuickAdd;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class TagSelectionControl : UserControl, IQuickInput, IComponentConnector, IStyleConnector
  {
    public static readonly DependencyProperty BatchModeProperty = DependencyProperty.Register(nameof (BatchMode), typeof (bool), typeof (TagSelectionControl), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    public static readonly DependencyProperty SearchModeProperty = DependencyProperty.Register(nameof (SearchMode), typeof (bool), typeof (TagSelectionControl), new PropertyMetadata((object) false, (PropertyChangedCallback) null));
    public static readonly DependencyProperty MaxCountProperty = DependencyProperty.Register(nameof (MaxCount), typeof (int), typeof (TagSelectionControl), new PropertyMetadata((object) 8, (PropertyChangedCallback) null));
    private bool _inFilter;
    private TagSelectData _originalSelectedTags = new TagSelectData();
    private TagSelectData _selectedData = new TagSelectData();
    internal TagSelectionControl RootView;
    internal ListView TagItems;
    internal StackPanel EmptyPanel;
    private bool _contentLoaded;

    public TagSelectionControl()
    {
      this.InitializeComponent();
      this.MaxHeight = (double) (36 * this.MaxCount);
    }

    public TagSelectionControl(List<string> tags)
    {
      this.InitializeComponent();
      this.MaxHeight = (double) (36 * this.MaxCount);
    }

    public bool BatchMode
    {
      get => (bool) this.GetValue(TagSelectionControl.BatchModeProperty);
      set => this.SetValue(TagSelectionControl.BatchModeProperty, (object) value);
    }

    public bool SearchMode
    {
      private get => (bool) this.GetValue(TagSelectionControl.SearchModeProperty);
      set => this.SetValue(TagSelectionControl.SearchModeProperty, (object) value);
    }

    public int MaxCount
    {
      private get => (int) this.GetValue(TagSelectionControl.MaxCountProperty);
      set => this.SetValue(TagSelectionControl.MaxCountProperty, (object) value);
    }

    private ObservableCollection<TagDisplayViewModel> DisplayTags
    {
      get => (ObservableCollection<TagDisplayViewModel>) this.TagItems.ItemsSource;
    }

    public bool CanCreateTag { private get; set; } = true;

    public bool Filter(string content, List<string> selected = null)
    {
      content = content?.Trim() ?? "";
      if (!this._inFilter && !string.IsNullOrEmpty(content))
      {
        this._selectedData = this.BatchMode ? this.GetSelectedData() : new TagSelectData();
        this._inFilter = true;
      }
      if (string.IsNullOrEmpty(content))
        this.SetSelectedTags(this._selectedData);
      if (!NameUtils.IsValidName(content))
      {
        this.TagItems.ItemsSource = (IEnumerable) new ObservableCollection<TagDisplayViewModel>();
        this.TagItems.Visibility = Visibility.Collapsed;
        EventHandler<int> tagsCountChanged = this.TagsCountChanged;
        if (tagsCountChanged != null)
          tagsCountChanged((object) this, 0);
      }
      else
      {
        string content1 = content.Trim();
        string lower = content1.ToLower();
        ObservableCollection<TagDisplayViewModel> displayTags = this.GetDisplayTags(content1);
        if ((selected == null || !selected.Contains(content1)) && content1.Length <= 64 && !string.IsNullOrEmpty(lower) && !TagSelectionControl.IsTagMatched(lower, (IEnumerable<TagDisplayViewModel>) displayTags) && this.CanCreateTag)
          displayTags.Add(TagDisplayViewModel.BuildAddTag(content));
        if (displayTags.Count > 0)
          displayTags[0].Focused = true;
        if (this.BatchMode)
          this.EmptyPanel.Visibility = displayTags.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        this.TagItems.ItemsSource = (IEnumerable) displayTags;
        this.TagItems.Visibility = displayTags.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        EventHandler<int> tagsCountChanged = this.TagsCountChanged;
        if (tagsCountChanged != null)
          tagsCountChanged((object) this, displayTags.Count);
      }
      return this.TagItems.ItemsSource != null && this.TagItems.ItemsSource is ObservableCollection<TagDisplayViewModel> itemsSource && itemsSource.Any<TagDisplayViewModel>();
    }

    public bool IsTag() => true;

    public void Move(bool forward)
    {
      if (forward)
        this.MoveUp();
      else
        this.MoveDown();
    }

    public void TrySelectItem(bool exactly = false) => this.TrySelectFocusedTag();

    public void SetTags(List<string> tags)
    {
    }

    public event EventHandler<int> TagsCountChanged;

    public event EventHandler<string> TagSelected;

    public event EventHandler<TagDisplayViewModel> TagSelectedChanged;

    public List<string> GetSelectedTags()
    {
      return this.DisplayTags.Where<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (tag => tag.Selected)).Select<TagDisplayViewModel, string>((Func<TagDisplayViewModel, string>) (tag => tag.Tag)).Distinct<string>().ToList<string>();
    }

    public TagSelectData GetSelectedData()
    {
      TagSelectData selectedData = new TagSelectData();
      ObservableCollection<TagDisplayViewModel> displayTags1 = this.DisplayTags;
      selectedData.OmniSelectTags = (displayTags1 != null ? displayTags1.Where<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (tag => tag.Selected)).Select<TagDisplayViewModel, string>((Func<TagDisplayViewModel, string>) (tag => tag.Tag)).Distinct<string>().ToList<string>() : (List<string>) null) ?? new List<string>();
      ObservableCollection<TagDisplayViewModel> displayTags2 = this.DisplayTags;
      selectedData.PartSelectTags = (displayTags2 != null ? displayTags2.Where<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (tag => tag.PartSelected)).Select<TagDisplayViewModel, string>((Func<TagDisplayViewModel, string>) (tag => tag.Tag)).Distinct<string>().ToList<string>() : (List<string>) null) ?? new List<string>();
      return selectedData;
    }

    public Dictionary<string, bool> GetTagSelectedDictionary()
    {
      Dictionary<string, bool> selectedDictionary = new Dictionary<string, bool>();
      if (this.DisplayTags != null && this.DisplayTags.Count > 0)
      {
        foreach (TagDisplayViewModel displayTag in (Collection<TagDisplayViewModel>) this.DisplayTags)
        {
          if (!selectedDictionary.ContainsKey(displayTag.Tag))
            selectedDictionary.Add(displayTag.Tag, displayTag.Selected);
        }
      }
      return selectedDictionary;
    }

    public void SetSelectedTags(TagSelectData tags)
    {
      this._originalSelectedTags = tags ?? new TagSelectData();
      this.InitTagData(this._originalSelectedTags);
      this._inFilter = false;
    }

    private static bool IsTagMatched(string prefix, IEnumerable<TagDisplayViewModel> tags)
    {
      return tags.FirstOrDefault<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (tag => tag.Tag == prefix)) != null;
    }

    private ObservableCollection<TagDisplayViewModel> GetDisplayTags(string content)
    {
      List<TagModel> tags = TagDataHelper.GetTags();
      ObservableCollection<TagDisplayViewModel> source = new ObservableCollection<TagDisplayViewModel>();
      bool flag = false;
      foreach (TagModel tag in tags)
      {
        if (tag.name == content.ToLower())
          flag = true;
        else if (string.IsNullOrEmpty(content) || TagSelectionControl.IsTagCandidate(tag, content))
          source.Add(new TagDisplayViewModel(tag.GetDisplayName(), this._selectedData.OmniSelectTags.Contains(tag.name), this._selectedData.PartSelectTags.Contains(tag.name)));
      }
      ObservableCollection<TagDisplayViewModel> displayTags = new ObservableCollection<TagDisplayViewModel>((IEnumerable<TagDisplayViewModel>) source.OrderByDescending<TagDisplayViewModel, bool>((Func<TagDisplayViewModel, bool>) (tag => tag.Selected)).ThenByDescending<TagDisplayViewModel, bool>((Func<TagDisplayViewModel, bool>) (tag => tag.PartSelected)));
      if (flag)
      {
        string lower = content.ToLower();
        string tagDisplayName = TagDataHelper.GetTagDisplayName(lower);
        displayTags.Insert(0, new TagDisplayViewModel(tagDisplayName, !this.BatchMode || this._selectedData.OmniSelectTags.Contains(lower), this._selectedData.PartSelectTags.Contains(lower)));
      }
      return displayTags;
    }

    private static bool IsTagCandidate(TagModel tag, string text)
    {
      string lower = text.ToLower();
      return tag.name.Contains(lower) || !string.IsNullOrEmpty(tag.pinyin) && tag.pinyin.Contains(lower) || !string.IsNullOrEmpty(tag.inits) && tag.inits.Contains(lower);
    }

    public void MoveUp()
    {
      if (this.TryFocusFirstTag())
        return;
      this.MoveNext(this.GetNextIndex(true));
    }

    private int GetNextIndex(bool up)
    {
      int focusedIndex = this.GetFocusedIndex();
      int index = focusedIndex;
      if (focusedIndex == -1)
        return -1;
      do
      {
        index += up ? -1 : 1;
        if (index < 0)
          index = this.DisplayTags.Count - 1;
        if (index > this.DisplayTags.Count - 1)
          index = 0;
        if (this.DisplayTags.Count > index && index >= 0 && !this.DisplayTags[index].Collapsed)
          return index;
      }
      while (index != focusedIndex);
      return focusedIndex;
    }

    public void MoveDown()
    {
      if (this.TryFocusFirstTag())
        return;
      this.MoveNext(this.GetNextIndex(false));
    }

    private void MoveNext(int nextIndex)
    {
      if (nextIndex < 0 || nextIndex > this.DisplayTags.Count - 1)
        return;
      this.ClearCurrenFocused();
      TagDisplayViewModel displayTag = this.DisplayTags[nextIndex];
      displayTag.Focused = true;
      this.TagItems.ScrollIntoView((object) displayTag);
    }

    private void ClearCurrenFocused()
    {
      TagDisplayViewModel focusedTag = this.GetFocusedTag();
      if (focusedTag == null)
        return;
      focusedTag.Focused = false;
    }

    private bool TryFocusFirstTag()
    {
      if (this.GetFocusedTag() != null || this.DisplayTags.Count <= 0)
        return false;
      this.DisplayTags[0].Focused = true;
      return true;
    }

    private TagDisplayViewModel GetFocusedTag()
    {
      ObservableCollection<TagDisplayViewModel> displayTags = this.DisplayTags;
      return displayTags == null ? (TagDisplayViewModel) null : displayTags.FirstOrDefault<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (tag => tag.Focused));
    }

    private int GetFocusedIndex()
    {
      TagDisplayViewModel focusedTag = this.GetFocusedTag();
      return focusedTag != null && this.DisplayTags != null && this.DisplayTags.Any<TagDisplayViewModel>() ? this.DisplayTags.IndexOf(focusedTag) : -1;
    }

    public async void TrySelectFocusedTag(bool add = true)
    {
      TagSelectionControl sender = this;
      TagDisplayViewModel focused = sender.GetFocusedTag();
      if (focused == null)
      {
        focused = (TagDisplayViewModel) null;
      }
      else
      {
        string tag = !focused.IsAddTag ? focused.Title.Trim() : focused.Tag.Trim();
        if (NameUtils.IsValidName(tag))
        {
          if (focused.IsAddTag & add)
          {
            TagModel tag1 = await TagService.TryCreateTag(tag);
            ListViewContainer.ReloadProjectData();
          }
          EventHandler<string> tagSelected = sender.TagSelected;
          if (tagSelected != null)
            tagSelected((object) sender, tag);
        }
        focused.Selected = !focused.Selected;
        focused.PartSelected = false;
        sender.OnItemSelectedChanged(focused);
        tag = (string) null;
        focused = (TagDisplayViewModel) null;
      }
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!this.BatchMode)
        this.TrySelectFocusedTag();
      else
        this.ToggleItemSelected(sender);
    }

    private void ToggleItemSelected(object sender)
    {
      if (!(sender is Grid grid) || !(grid.DataContext is TagDisplayViewModel dataContext))
        return;
      if (dataContext.PartSelected)
      {
        dataContext.PartSelected = false;
        dataContext.Selected = true;
      }
      else
        dataContext.Selected = !dataContext.Selected;
      if (this._inFilter && this._selectedData != null)
      {
        this._selectedData.PartSelectTags.Remove(dataContext.Tag);
        if (dataContext.Selected)
        {
          if (!this._selectedData.OmniSelectTags.Contains(dataContext.Tag))
            this._selectedData.OmniSelectTags.Add(dataContext.Tag);
        }
        else
          this._selectedData.OmniSelectTags.Remove(dataContext.Tag);
      }
      if (this.SearchMode)
      {
        if (dataContext.Selected)
        {
          if (dataContext.IsAllTag)
          {
            foreach (TagDisplayViewModel displayTag in (Collection<TagDisplayViewModel>) this.DisplayTags)
            {
              if (!displayTag.IsAllTag)
                displayTag.Selected = false;
            }
          }
          else
            this.SetAllTagItemSelected(false);
        }
        else if (this.DisplayTags.Count<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (tag => tag.Selected)) == 0)
          this.SetAllTagItemSelected(true);
      }
      this.OnItemSelectedChanged(dataContext);
    }

    private void OnItemSelectedChanged(TagDisplayViewModel model)
    {
      if (this.SearchMode)
      {
        if (model.IsParent)
        {
          List<TagDisplayViewModel> children = model.Children;
          // ISSUE: explicit non-virtual call
          if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
          {
            model.Children?.ForEach((Action<TagDisplayViewModel>) (c => c.Selected = model.Selected));
            model.ChildSelected = model.Selected;
          }
        }
        if (string.IsNullOrEmpty(model.Parent))
          return;
        TagDisplayViewModel displayViewModel1 = this.DisplayTags.FirstOrDefault<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (t => t.Tag == model.Parent));
        if (displayViewModel1 == null)
          return;
        displayViewModel1.Selected = false;
        TagDisplayViewModel displayViewModel2 = displayViewModel1;
        int num;
        if (!model.Selected)
        {
          List<TagDisplayViewModel> children = displayViewModel1.Children;
          num = children != null ? (children.Any<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (c => c.Selected || c.PartSelected)) ? 1 : 0) : 0;
        }
        else
          num = 1;
        displayViewModel2.ChildSelected = num != 0;
      }
      else
      {
        this.DisplayTags.Where<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (t => t.Tag == model.Tag)).ToList<TagDisplayViewModel>().ForEach((Action<TagDisplayViewModel>) (m =>
        {
          m.Selected = model.Selected;
          m.PartSelected = model.PartSelected;
        }));
        if (!string.IsNullOrEmpty(model.Parent))
          this.DisplayTags.Where<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (t => t.Tag == model.Parent)).ToList<TagDisplayViewModel>().ForEach((Action<TagDisplayViewModel>) (m =>
          {
            TagDisplayViewModel displayViewModel = m;
            List<TagDisplayViewModel> children = m.Children;
            int num = children != null ? (children.Any<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (c => c.Selected || c.PartSelected)) ? 1 : 0) : 0;
            displayViewModel.ChildSelected = num != 0;
          }));
        EventHandler<TagDisplayViewModel> tagSelectedChanged = this.TagSelectedChanged;
        if (tagSelectedChanged == null)
          return;
        tagSelectedChanged((object) this, model);
      }
    }

    private void SetAllTagItemSelected(bool selected)
    {
      TagDisplayViewModel displayViewModel = this.DisplayTags.FirstOrDefault<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (tag => tag.IsAllTag));
      if (displayViewModel == null)
        return;
      displayViewModel.Selected = selected;
    }

    private void OnControlLoaded(object sender, RoutedEventArgs e)
    {
      if (this.DisplayTags != null && this.DisplayTags.Any<TagDisplayViewModel>())
        return;
      this.InitTagData(this._originalSelectedTags ?? new TagSelectData());
    }

    private void InitTagData(TagSelectData tagData)
    {
      List<TagModel> tags = TagDataHelper.GetTags();
      List<TagDisplayViewModel> source = new List<TagDisplayViewModel>();
      ObservableCollection<TagDisplayViewModel> observableCollection = new ObservableCollection<TagDisplayViewModel>();
      if (tags != null && tags.Any<TagModel>())
      {
        List<TagModel> list = tags.Where<TagModel>((Func<TagModel, bool>) (t => t.IsChild())).ToList<TagModel>();
        foreach (TagModel tagModel in tags.Where<TagModel>((Func<TagModel, bool>) (tag => !tag.IsChild())))
        {
          TagModel tag = tagModel;
          TagDisplayViewModel model = new TagDisplayViewModel(tag.GetDisplayName(), tagData.OmniSelectTags.Contains(tag.name), tagData.PartSelectTags.Contains(tag.name))
          {
            IsParent = tag.IsParent()
          };
          if (model.IsParent)
          {
            model.IsOpen = !tag.collapsed;
            model.Children = list.Where<TagModel>((Func<TagModel, bool>) (t => t.parent == model.Tag)).Select<TagModel, TagDisplayViewModel>((Func<TagModel, TagDisplayViewModel>) (t => new TagDisplayViewModel(t.GetDisplayName(), tagData.OmniSelectTags.Contains(t.name), tagData.PartSelectTags.Contains(t.name))
            {
              IsSubTag = true,
              Collapsed = this.BatchMode && tag.collapsed,
              Parent = tag.name
            })).ToList<TagDisplayViewModel>();
            TagDisplayViewModel displayViewModel = model;
            List<TagDisplayViewModel> children = model.Children;
            bool? nullable1;
            bool? nullable2;
            if (children == null)
            {
              nullable1 = new bool?();
              nullable2 = nullable1;
            }
            else
              nullable2 = new bool?(children.Any<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (c => c.Selected || c.PartSelected)));
            nullable1 = nullable2;
            int num = nullable1.Value ? 1 : 0;
            displayViewModel.ChildSelected = num != 0;
          }
          source.Add(model);
        }
        if (this.SearchMode)
        {
          TagDisplayViewModel displayViewModel1 = TagDisplayViewModel.BuildAllTag();
          if (tagData.OmniSelectTags.Count > 0)
            displayViewModel1.Selected = false;
          TagDisplayViewModel displayViewModel2 = TagDisplayViewModel.BuildNoTag();
          if (tagData.OmniSelectTags.Contains("!tag"))
            displayViewModel2.Selected = true;
          observableCollection.Add(displayViewModel1);
          observableCollection.Add(new TagDisplayViewModel(true));
          observableCollection.Add(displayViewModel2);
        }
        foreach (TagDisplayViewModel displayViewModel in source.Where<TagDisplayViewModel>((Func<TagDisplayViewModel, bool>) (tag => string.IsNullOrEmpty(tag.Parent))))
        {
          if (displayViewModel.IsParent)
          {
            List<TagDisplayViewModel> children = displayViewModel.Children;
            // ISSUE: explicit non-virtual call
            if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
            {
              observableCollection.Add(displayViewModel);
              displayViewModel.Children.ForEach(new Action<TagDisplayViewModel>(((Collection<TagDisplayViewModel>) observableCollection).Add));
              continue;
            }
          }
          observableCollection.Add(displayViewModel);
        }
      }
      if (this.BatchMode)
        this.EmptyPanel.Visibility = observableCollection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
      this.TagItems.ItemsSource = (IEnumerable) observableCollection;
      if (observableCollection.Count > 0)
      {
        observableCollection[0].Focused = true;
        this.TagItems.ScrollIntoView((object) observableCollection[0]);
      }
      EventHandler<int> tagsCountChanged = this.TagsCountChanged;
      if (tagsCountChanged == null)
        return;
      tagsCountChanged((object) this, observableCollection.Count);
    }

    private void OnItemEnter(object sender, MouseEventArgs e)
    {
      if (!(sender is Grid grid))
        return;
      this.ClearCurrenFocused();
      if (!(grid.DataContext is TagDisplayViewModel dataContext) || dataContext.Focused)
        return;
      dataContext.Focused = true;
    }

    private void OnCollapsedClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Grid grid))
        return;
      TagDisplayViewModel model = grid.DataContext as TagDisplayViewModel;
      if (model == null)
        return;
      model.IsOpen = !model.IsOpen;
      model.Children?.ForEach((Action<TagDisplayViewModel>) (m => m.Collapsed = !model.IsOpen));
      e.Handled = true;
    }

    public void OnTagAdded(string tag) => this._selectedData.OmniSelectTags.Add(tag);

    private void OnCollapsedMouseDown(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private void OnBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/tagselectioncontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.RootView = (TagSelectionControl) target;
          this.RootView.Loaded += new RoutedEventHandler(this.OnControlLoaded);
          break;
        case 2:
          this.TagItems = (ListView) target;
          break;
        case 6:
          this.EmptyPanel = (StackPanel) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 3:
          ((FrameworkElement) target).RequestBringIntoView += new RequestBringIntoViewEventHandler(this.OnBringIntoView);
          break;
        case 4:
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnItemEnter);
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
          break;
        case 5:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCollapsedClick);
          break;
      }
    }
  }
}
