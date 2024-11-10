// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.BatchSetTagControl
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
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.MainListView;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class BatchSetTagControl : UserControl, IComponentConnector, IStyleConnector
  {
    private TagSelectData _tagDatas;
    private bool _locationSet;
    private bool _inFilter;
    private Dictionary<string, long> _sortDict;
    private bool _canDelete;
    private readonly List<string> _needAddTags = new List<string>();
    private int _tabIndex;
    internal Image SearchIcon;
    internal ScrollViewer SelectedScroller;
    internal ItemsControl SelectedTags;
    internal TagSelectionControl TagItems;
    internal Button SaveButton;
    internal Button CancelButton;
    private bool _contentLoaded;

    public event EventHandler<TagSelectData> TagsSelect;

    public event EventHandler Close;

    public BatchSetTagControl()
    {
      this.InitializeComponent();
      this.TagItems.TagSelectedChanged += new EventHandler<TagDisplayViewModel>(this.OnTagSelectedChanged);
    }

    public void Init(TagSelectData tags, bool firstInit)
    {
      this._tagDatas = tags;
      this._tabIndex = 0;
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, false);
      UiUtils.SetCancelButtonTabSelected(this.CancelButton, false);
      this.TagItems.SetSelectedTags(this._tagDatas);
      this.SetSelectedTags(this._tagDatas);
      if (!firstInit && this._sortDict != null)
        return;
      this._sortDict = CacheManager.GetTags().ToDictionary<TagModel, string, long>((Func<TagModel, string>) (t => t.name), (Func<TagModel, long>) (t => t.sortOrder));
    }

    private void SetSelectedTags(TagSelectData tagData)
    {
      ObservableCollection<SelectedTagViewModel> observableCollection = new ObservableCollection<SelectedTagViewModel>();
      List<TagModel> tags = CacheManager.GetTags();
      foreach (string omniSelectTag in tagData.OmniSelectTags)
      {
        string tagName = omniSelectTag;
        TagModel tagModel = tags.FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == tagName));
        observableCollection.Add(new SelectedTagViewModel()
        {
          Tag = tagModel == null ? tagName : tagModel.GetDisplayName()
        });
      }
      observableCollection.Add(new SelectedTagViewModel()
      {
        IsAdd = true
      });
      this.SelectedTags.ItemsSource = (IEnumerable) observableCollection;
      this.SetSearchVisible(observableCollection.Count <= 1);
    }

    private void SetSearchVisible(bool visible)
    {
      this.SearchIcon.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
      this.SelectedScroller.Margin = new Thickness(visible ? 32.0 : 8.0, 0.0, 12.0, 12.0);
    }

    private void OnTextKeyDown(object sender, KeyEventArgs e)
    {
      this._canDelete = false;
      if (e.Key != Key.Back || !(sender is TextBox textBox) || textBox.CaretIndex != 0 || !string.IsNullOrEmpty(textBox.Text))
        return;
      this._canDelete = true;
    }

    private async void OnKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Tab:
          this.HandleTab(Utils.IfShiftPressed());
          break;
        case Key.Up:
          if (this._tabIndex >= 1)
            break;
          this.TagItems.MoveUp();
          break;
        case Key.Down:
          if (this._tabIndex >= 1)
            break;
          this.TagItems.MoveDown();
          break;
      }
    }

    public void HandleTab(bool shift)
    {
      this._tabIndex += 3 + (shift ? -1 : 1);
      this._tabIndex %= 3;
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, this._tabIndex == 1);
      UiUtils.SetCancelButtonTabSelected(this.CancelButton, this._tabIndex == 2);
    }

    private void OnTagAddTextChanged(object sender, string text)
    {
      if (!NameUtils.IsValidName(text))
      {
        Utils.Toast(Utils.GetString("TagNotValid"));
        if (!(sender is TextBox textBox))
          return;
        textBox.SelectAll();
      }
      else if (string.IsNullOrEmpty(text.Trim()))
        this.TagItems.SetSelectedTags(this._tagDatas);
      else
        this.TagItems.Filter(text, this._tagDatas.OmniSelectTags);
    }

    private void SetSaveButtonContent(bool textEmpty)
    {
      if (!textEmpty)
      {
        this.SaveButton.Content = (object) Utils.GetString("OK");
        this.SaveButton.IsEnabled = false;
      }
      else
      {
        this.SaveButton.Content = (object) Utils.GetString("PublicSave");
        this.SaveButton.IsEnabled = true;
      }
    }

    private void OnTagSelectedChanged(object sender, TagDisplayViewModel tag)
    {
      if (tag.Selected)
      {
        if (!this._tagDatas.OmniSelectTags.Contains(tag.Tag))
          this._tagDatas.OmniSelectTags.Add(tag.Tag);
        this._tagDatas.PartSelectTags.Remove(tag.Tag);
      }
      else if (tag.PartSelected)
      {
        if (!this._tagDatas.PartSelectTags.Contains(tag.Tag))
          this._tagDatas.PartSelectTags.Add(tag.Tag);
        this._tagDatas.OmniSelectTags.Remove(tag.Tag);
      }
      else
      {
        this._tagDatas.PartSelectTags.Remove(tag.Tag);
        this._tagDatas.OmniSelectTags.Remove(tag.Tag);
      }
      if (!(this.SelectedTags.ItemsSource is ObservableCollection<SelectedTagViewModel> itemsSource))
        return;
      bool flag = !string.IsNullOrEmpty(itemsSource[itemsSource.Count - 1].Tag);
      SelectedTagViewModel selectedTagViewModel = itemsSource.FirstOrDefault<SelectedTagViewModel>((Func<SelectedTagViewModel, bool>) (t => !t.IsAdd && string.Equals(t.Tag, tag.Title, StringComparison.CurrentCultureIgnoreCase)));
      if (tag.Selected && selectedTagViewModel == null)
      {
        if (tag.IsAddTag)
        {
          itemsSource.Insert(0, new SelectedTagViewModel()
          {
            Tag = tag.Tag
          });
          this._needAddTags.Add(tag.Tag);
        }
        else
        {
          long num = this._sortDict.ContainsKey(tag.Tag) ? this._sortDict[tag.Tag] : 0L;
          int index = 0;
          while (index < itemsSource.Count && !itemsSource[index].IsAdd && (this._sortDict.ContainsKey(itemsSource[index].Tag) ? this._sortDict[itemsSource[index].Tag] : 0L) <= num)
            ++index;
          itemsSource.Insert(index, new SelectedTagViewModel()
          {
            Tag = tag.Title
          });
        }
      }
      else if (!tag.Selected && selectedTagViewModel != null)
      {
        itemsSource.Remove(selectedTagViewModel);
        this._needAddTags.Remove(tag.Tag);
      }
      if (flag)
      {
        itemsSource.RemoveAt(itemsSource.Count - 1);
        itemsSource.Add(new SelectedTagViewModel()
        {
          IsAdd = true
        });
        this.TagItems.SetSelectedTags(this._tagDatas);
      }
      this.SetSearchVisible(itemsSource.Count <= 1);
    }

    private static List<string> GetMergeSelectedTag(
      List<string> original,
      Dictionary<string, bool> selected)
    {
      if (selected == null)
        return new List<string>();
      foreach (KeyValuePair<string, bool> keyValuePair in selected)
      {
        string key = keyValuePair.Key;
        if (original.Contains(key))
        {
          if (!keyValuePair.Value)
            original.Remove(key);
        }
        else if (keyValuePair.Value)
          original.Add(key);
      }
      return original;
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      BatchSetTagControl sender1 = this;
      HashSet<string> localTags = new HashSet<string>(CacheManager.GetTags().Select<TagModel, string>((Func<TagModel, string>) (t => t.name)));
      bool flag = false;
      foreach (string omniSelectTag in sender1._tagDatas.OmniSelectTags)
      {
        if (!localTags.Contains(omniSelectTag.ToLower()))
        {
          TagModel tag = await TagService.TryCreateTag(omniSelectTag);
          flag = true;
        }
      }
      if (flag)
        ListViewContainer.ReloadProjectData();
      EventHandler<TagSelectData> tagsSelect = sender1.TagsSelect;
      if (tagsSelect != null)
        tagsSelect((object) sender1, sender1._tagDatas);
      sender1.TryClose();
      localTags = (HashSet<string>) null;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.TryClose();

    private void TryClose()
    {
      EventHandler close = this.Close;
      if (close == null)
        return;
      close((object) this, (EventArgs) null);
    }

    private void OnDeleteClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Border border) || !(border.DataContext is SelectedTagViewModel dataContext))
        return;
      if (this.SelectedTags.ItemsSource is ObservableCollection<SelectedTagViewModel> itemsSource)
      {
        itemsSource.Remove(dataContext);
        this.SetSearchVisible(itemsSource.Count <= 1);
      }
      if (this._tagDatas.OmniSelectTags.Contains(dataContext.Tag.ToLower()))
        this._tagDatas.OmniSelectTags.Remove(dataContext.Tag.ToLower());
      this.TagItems.SetSelectedTags(this._tagDatas);
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (this._canDelete && e.Key == Key.Back && sender is TextBox textBox && textBox.CaretIndex == 0 && this.SelectedTags.ItemsSource is ObservableCollection<SelectedTagViewModel> itemsSource)
      {
        SelectedTagViewModel selectedTagViewModel = itemsSource.LastOrDefault<SelectedTagViewModel>((Func<SelectedTagViewModel, bool>) (t => !t.IsAdd));
        if (selectedTagViewModel != null)
        {
          itemsSource.Remove(selectedTagViewModel);
          if (this._tagDatas.OmniSelectTags.Contains(selectedTagViewModel.Tag))
            this._tagDatas.OmniSelectTags.Remove(selectedTagViewModel.Tag);
          this.TagItems.SetSelectedTags(this._tagDatas);
        }
        this.SetSearchVisible(itemsSource.Count <= 1);
      }
      switch (e.Key)
      {
        case Key.Return:
          switch (this._tabIndex)
          {
            case 0:
              this.TagItems.TrySelectFocusedTag(false);
              break;
            case 1:
              this.OnSaveClick((object) null, (RoutedEventArgs) null);
              break;
            case 2:
              this.TryClose();
              break;
          }
          e.Handled = true;
          break;
        case Key.Escape:
          this.TryClose();
          e.Handled = true;
          break;
      }
    }

    public void ClearEvent() => this.TagsSelect = (EventHandler<TagSelectData>) null;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/batchsettagcontrol.xaml", UriKind.Relative));
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
          ((UIElement) target).PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
          ((UIElement) target).KeyUp += new KeyEventHandler(this.OnKeyUp);
          break;
        case 3:
          this.SearchIcon = (Image) target;
          break;
        case 4:
          this.SelectedScroller = (ScrollViewer) target;
          break;
        case 5:
          this.SelectedTags = (ItemsControl) target;
          break;
        case 6:
          this.TagItems = (TagSelectionControl) target;
          break;
        case 7:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 8:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
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
      if (connectionId != 2)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteClick);
    }
  }
}
