// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchInputControl
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
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchInputControl : UserControl, IComponentConnector, IStyleConnector
  {
    private readonly SolidColorBrush _primaryTextColor = ThemeUtil.GetColor("BaseColorOpacity100_80");
    private readonly DelayActionHandler _searchActionHandler = new DelayActionHandler(100);
    private bool _isTagOpen;
    private bool _keepOldText;
    private SearchExtra _searchExtra;
    private ObservableCollection<SearchHistoryViewModel> _searchHistoryModels = new ObservableCollection<SearchHistoryViewModel>();
    private ObservableCollection<SearchItemBaseModel> _searchItemModels = new ObservableCollection<SearchItemBaseModel>();
    private int _selectedIndex = -1;
    private List<string> _tags;
    private bool _restore;
    private static string _inputText;
    private static bool _tagExpand;
    private static bool _listExpand;
    private static bool _filterExpand;
    internal SearchInputControl Root;
    internal Line FocusHighlightLine;
    internal TextBlock HintText;
    internal ScrollViewer Visual;
    internal RichTextBox TitleTextBox;
    internal Border ClearGrid;
    internal Border CloseGrid;
    internal StackPanel SearchHistoryPage;
    internal ItemsControl SearchHistoryList;
    internal ListView SearchItemList;
    internal Border SearchMore;
    internal StackPanel EmptyGrid;
    internal TextBlock NoTaskGridTitleTextBox;
    internal TextBlock NoTaskGridTitleTextBoxOnSearch;
    private bool _contentLoaded;

    public SearchInputControl()
    {
      this.InitializeComponent();
      this._searchActionHandler.SetAction(new EventHandler(this.TryDelaySearch));
      DataObject.AddPastingHandler((DependencyObject) this.TitleTextBox, new DataObjectPastingEventHandler(this.OnPaste));
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.FocusEnd();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
    }

    public event EventHandler<EventArgs> Close;

    public event EventHandler<SearchExtra> Search;

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
      e.CancelCommand();
      try
      {
        if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true) || !(e.SourceDataObject.GetData(DataFormats.UnicodeText) is string data))
          return;
        this.InsertPasteContent(data.Replace("\n", " ").Replace("\r", " "));
      }
      catch (Exception ex)
      {
      }
    }

    private void InsertPasteContent(string content)
    {
      this.TitleTextBox.Selection.Text = string.Empty;
      this.TitleTextBox.CaretPosition.InsertTextInRun(content);
    }

    private void TryDelaySearch(object sender, EventArgs e)
    {
      if (this.Visibility != Visibility.Visible)
        return;
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.StartSearch));
    }

    private void StartSearch()
    {
      if (this._isTagOpen)
        return;
      string searchKey = this._searchExtra.SearchKey;
      if ((searchKey != null ? (searchKey.Trim().Length > 64 ? 1 : 0) : 0) != 0)
        this._searchExtra.SearchKey = this._searchExtra.SearchKey?.Trim().Substring(0, 64);
      UserActCollectUtils.AddClickEvent("search", "search_type", "task");
      EventHandler<SearchExtra> search = this.Search;
      if (search != null)
        search((object) this, this._searchExtra);
      this._keepOldText = false;
      SearchHistoryDao.AddHistoryModel(this._searchExtra.SearchKey, this._searchExtra.Tags);
    }

    private string GetContentText()
    {
      StringBuilder stringBuilder = new StringBuilder();
      InlineCollection textInlines = this.GetTextInlines();
      if (textInlines != null && textInlines.Count > 0)
      {
        foreach (Inline inline in (TextElementCollection<Inline>) textInlines)
        {
          if (inline is Run run)
            stringBuilder.Append(run.Text);
        }
      }
      return stringBuilder.ToString();
    }

    private InlineCollection GetTextInlines()
    {
      BlockCollection blocks = this.TitleTextBox.Document.Blocks;
      return blocks.Count == 1 && blocks.FirstOrDefault<Block>() is Paragraph paragraph ? paragraph.Inlines : (InlineCollection) null;
    }

    public SearchExtra GetSearchExtra()
    {
      return new SearchExtra()
      {
        SearchKey = this.GetContentText().Trim(),
        Tags = this.GetInputTags()
      };
    }

    private void TryDelaySearch(string taskId = null)
    {
      this._searchExtra = new SearchExtra()
      {
        SearchKey = this.GetContentText().Trim(),
        Tags = this.GetInputTags(),
        SearchId = taskId
      };
      if (this._searchExtra.Empty())
        return;
      this._searchActionHandler.TryDoAction();
    }

    private void TaskTextChanged(object sender, TextChangedEventArgs e)
    {
      this._keepOldText = true;
      this.OnTextChanged();
    }

    private void OnTextBoxClick(object sender, RoutedEventArgs e) => this.OnTextChanged();

    private async void OnTextChanged(bool forceSearch = false)
    {
      this._selectedIndex = -1;
      string text = this.GetContentText()?.Trim();
      this.SetHintVisibility(text);
      if (await this.TryShowTagPopup(text))
      {
        text = (string) null;
      }
      else
      {
        if (SearchInputControl._inputText == text)
        {
          if (!forceSearch)
          {
            text = (string) null;
            return;
          }
        }
        else
        {
          SearchInputControl._tagExpand = false;
          SearchInputControl._listExpand = false;
          SearchInputControl._filterExpand = false;
          SearchInputControl._inputText = text;
        }
        this.SetSearchItem(text);
        text = (string) null;
      }
    }

    private void SetSearchItem(string text)
    {
      this._selectedIndex = -1;
      this.SetTags();
      text = text?.Trim() ?? string.Empty;
      if (this._tags.Count == 0 && string.IsNullOrEmpty(text))
      {
        DelayActionHandlerCenter.RemoveAction("OnSearchInput");
        this.SearchMore.Visibility = Visibility.Collapsed;
        this.ClearGrid.Visibility = Visibility.Collapsed;
        this.SearchItemList.IsHitTestVisible = false;
        this._searchItemModels = new ObservableCollection<SearchItemBaseModel>();
        this.SearchItemList.ItemsSource = (IEnumerable) this._searchItemModels;
        this.ShowSearchHistory();
      }
      else
        DelayActionHandlerCenter.TryDoAction("OnSearchInput", (EventHandler) ((sender, args) => this.Dispatcher.Invoke((Action) (() =>
        {
          this.SearchHistoryPage.Visibility = Visibility.Collapsed;
          this.ClearGrid.Visibility = Visibility.Visible;
          this.SearchItemList.IsHitTestVisible = true;
          List<string> list = ((IEnumerable<string>) text.ToLower().Split(' ')).ToList<string>();
          list.Remove("");
          string str = "(" + Utils.HandleReg(text.ToLower()) + ")";
          if (list.Count > 1)
            str = list.Aggregate<string, string>(str, (Func<string, string, string>) ((current, key) => current + "|(" + Utils.HandleReg(key) + ")"));
          SearchHelper.PreSearchRegex = new Regex(str);
          this.GetSearchDisplayModels(text);
        }))), 150);
    }

    private void SetTags()
    {
      List<string> inputTags = this.GetInputTags();
      int? count1 = inputTags?.Count;
      int? count2 = this._tags?.Count;
      if (!(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue))
      {
        SearchHelper.ClearTaskSearchModels();
        SearchHelper.GetTaskSearchModels(this._restore ? SearchHelper.SearchFilter.ToSearchFilter() : new SearchFilterModel(), inputTags);
      }
      this._tags = inputTags;
    }

    private async void ShowSearchHistory()
    {
      List<SearchExtra> historyViewModels = await SearchHistoryDao.GetHistoryViewModels();
      List<SearchHistoryViewModel> list = historyViewModels != null ? historyViewModels.Select<SearchExtra, SearchHistoryViewModel>((Func<SearchExtra, SearchHistoryViewModel>) (m => new SearchHistoryViewModel(m))).ToList<SearchHistoryViewModel>() : (List<SearchHistoryViewModel>) null;
      if (list == null || list.Count == 0)
        this._searchHistoryModels.Clear();
      else
        this._searchHistoryModels = new ObservableCollection<SearchHistoryViewModel>(list);
      if (this._searchHistoryModels.Count == 0)
      {
        this.SearchHistoryPage.Visibility = Visibility.Collapsed;
        this.EmptyGrid.Visibility = Visibility.Visible;
        this.HintText.Text = Utils.GetString("Search");
      }
      else
      {
        this.SearchHistoryPage.Visibility = Visibility.Visible;
        this.EmptyGrid.Visibility = Visibility.Collapsed;
        this.HintText.Text = Utils.GetString("SearchHint");
      }
      this.SearchHistoryList.ItemsSource = (IEnumerable) this._searchHistoryModels;
    }

    private async Task GetSearchDisplayModels(string text)
    {
      // ISSUE: variable of a compiler-generated type
      SearchInputControl.\u003C\u003Ec__DisplayClass36_0 cDisplayClass360;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass360.items = new ObservableCollection<SearchItemBaseModel>();
      List<TaskBaseViewModel> source = await TaskDisplayService.GetDisplayTaskInSearch(text, this._restore ? new SearchFilterViewModel() : SearchHelper.SearchFilter, this._tags, true);
      // ISSUE: reference to a compiler-generated field
      cDisplayClass360.needSplit = false;
      if (source.Count > 0)
      {
        // ISSUE: reference to a compiler-generated field
        cDisplayClass360.needSplit = true;
        // ISSUE: reference to a compiler-generated field
        cDisplayClass360.items.Add((SearchItemBaseModel) new SearchTitleModel()
        {
          Title = Utils.GetString("Task")
        });
        SearchLoadMoreModel searchLoadMoreModel = (SearchLoadMoreModel) null;
        if (source.Count > 5)
        {
          source = source.Take<TaskBaseViewModel>(5).ToList<TaskBaseViewModel>();
          searchLoadMoreModel = new SearchLoadMoreModel()
          {
            IsTask = true
          };
        }
        foreach (TaskBaseViewModel task in source)
        {
          SearchTaskItemViewModel taskItemViewModel = new SearchTaskItemViewModel(task);
          taskItemViewModel.InitData();
          // ISSUE: reference to a compiler-generated field
          cDisplayClass360.items.Add((SearchItemBaseModel) taskItemViewModel);
        }
        if (searchLoadMoreModel != null)
        {
          // ISSUE: reference to a compiler-generated field
          cDisplayClass360.items.Add((SearchItemBaseModel) searchLoadMoreModel);
        }
      }
      if (this._tags.Count == 0)
      {
        List<SearchTagAndProjectModel> modelsMatched = SearchProjectHelper.GetModelsMatched(text, withInbox: true);
        List<SearchTagAndProjectModel> list1 = modelsMatched.Where<SearchTagAndProjectModel>((Func<SearchTagAndProjectModel, bool>) (p => p.IsProject)).ToList<SearchTagAndProjectModel>();
        List<SearchTagAndProjectModel> list2 = modelsMatched.Where<SearchTagAndProjectModel>((Func<SearchTagAndProjectModel, bool>) (p => p.IsTag)).ToList<SearchTagAndProjectModel>();
        List<SearchTagAndProjectModel> list3 = modelsMatched.Where<SearchTagAndProjectModel>((Func<SearchTagAndProjectModel, bool>) (p => p.IsFilter)).ToList<SearchTagAndProjectModel>();
        SearchInputControl.\u003CGetSearchDisplayModels\u003Eg__AddModels\u007C36_3(list2, "Tags", SearchInputControl._tagExpand, PtfType.Tag, ref cDisplayClass360);
        SearchInputControl.\u003CGetSearchDisplayModels\u003Eg__AddModels\u007C36_3(list1, "List", SearchInputControl._listExpand, PtfType.Project, ref cDisplayClass360);
        int num = SearchInputControl._filterExpand ? 1 : 0;
        ref SearchInputControl.\u003C\u003Ec__DisplayClass36_0 local = ref cDisplayClass360;
        SearchInputControl.\u003CGetSearchDisplayModels\u003Eg__AddModels\u007C36_3(list3, "Filter", num != 0, PtfType.Filter, ref local);
      }
      // ISSUE: reference to a compiler-generated field
      this.SearchMore.Visibility = cDisplayClass360.items.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
      // ISSUE: reference to a compiler-generated field
      this.EmptyGrid.Visibility = cDisplayClass360.items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
      this._selectedIndex = -1;
      // ISSUE: reference to a compiler-generated field
      this._searchItemModels = cDisplayClass360.items;
      this.SearchItemList.ItemsSource = (IEnumerable) this._searchItemModels;
      // ISSUE: object of a compiler-generated type is created
      cDisplayClass360 = new SearchInputControl.\u003C\u003Ec__DisplayClass36_0();
    }

    private async void SetHintVisibility(string text)
    {
      if (string.IsNullOrEmpty(text) && this.GetInputTags().Count == 0)
      {
        this.HintText.Visibility = Visibility.Visible;
      }
      else
      {
        await Task.Delay(50);
        this.HintText.Visibility = Visibility.Collapsed;
      }
    }

    private async Task<bool> TryShowTagPopup(string text)
    {
      SearchInputControl searchInputControl = this;
      if (searchInputControl._isTagOpen)
        return true;
      if (!text.EndsWith("#"))
        return false;
      if (Utils.IsWindows7())
        await Task.Delay(50);
      TagSelectAddWindow tagSelectAddWindow = new TagSelectAddWindow(false);
      tagSelectAddWindow.TagItems.TagSelected -= new EventHandler<string>(searchInputControl.OnTagAdded);
      tagSelectAddWindow.TagItems.TagSelected += new EventHandler<string>(searchInputControl.OnTagAdded);
      tagSelectAddWindow.TagExit -= new EventHandler<string>(searchInputControl.OnTagExit);
      tagSelectAddWindow.TagExit += new EventHandler<string>(searchInputControl.OnTagExit);
      tagSelectAddWindow.Closed -= new EventHandler(searchInputControl.OnTagPopClosed);
      tagSelectAddWindow.Closed += new EventHandler(searchInputControl.OnTagPopClosed);
      Rect characterRect = searchInputControl.TitleTextBox.CaretPosition.GetCharacterRect(LogicalDirection.Forward);
      System.Windows.Point popupOffset = Utils.GetPopupOffset((Window) App.Window, (FrameworkElement) searchInputControl.TitleTextBox, characterRect.X, characterRect.Y);
      tagSelectAddWindow.Show(popupOffset.X, popupOffset.Y);
      searchInputControl._isTagOpen = true;
      return true;
    }

    private void OnTagExit(object sender, string text)
    {
      Run tagRun = this.GetTagRun();
      if (tagRun == null)
        return;
      string str = tagRun.Text;
      if (string.IsNullOrEmpty(text) && str.EndsWith("#"))
        str = str.Substring(0, str.Length - 1);
      tagRun.Text = str + (text == null || text.Length < 1 ? text : text.Substring(1));
      this.FocusEnd();
    }

    private void OnTagPopClosed(object sender, EventArgs e)
    {
      this.SetSearchItem(this.GetContentText());
      this._isTagOpen = false;
    }

    private void OnTagAdded(object sender, string tag)
    {
      this.TitleTextBox.TextChanged -= new TextChangedEventHandler(this.TaskTextChanged);
      this.TryAddTag(tag);
      this.TryAdjustTextAlignment();
      this.TitleTextBox.TextChanged += new TextChangedEventHandler(this.TaskTextChanged);
      this.SetSearchItem(this.GetContentText());
      this.SetHintVisibility(this.GetContentText());
    }

    private void TryAdjustTextAlignment()
    {
      InlineCollection textInlines = this.GetTextInlines();
      if (textInlines == null || textInlines.Count <= 0)
        return;
      foreach (Inline inline in (TextElementCollection<Inline>) textInlines)
        inline.BaselineAlignment = BaselineAlignment.Top;
    }

    private void TryAddTag(string tag)
    {
      SearchInputControl._inputText = (string) null;
      string str = "#" + tag;
      if (this.GetInputTags().Contains(tag))
      {
        this.FocusEnd();
      }
      else
      {
        Run tagRun = this.GetTagRun();
        if (tagRun == null)
          return;
        string text1 = tagRun.Text;
        if (!text1.EndsWith("#"))
          return;
        InlineCollection textInlines = this.GetTextInlines();
        string text2 = text1.Substring(0, text1.Length - 1).Trim();
        if (!string.IsNullOrEmpty(text2) || this.GetInputTags().Count > 0)
          text2 = text2.TrimEnd() + " ";
        Run newItem1 = new Run(text2);
        Run newItem2 = new Run(" ");
        InlineUIContainer inlineUiContainer = new InlineUIContainer((UIElement) new QuickAddDisplayControl()
        {
          Text = {
            Text = str
          }
        });
        textInlines.InsertAfter((Inline) tagRun, (Inline) inlineUiContainer);
        textInlines.InsertBefore((Inline) inlineUiContainer, (Inline) newItem1);
        textInlines.InsertAfter((Inline) inlineUiContainer, (Inline) newItem2);
        textInlines.Remove((Inline) tagRun);
        this.RemoveEmptyRuns();
        this.FocusEnd();
      }
    }

    public async void FocusEnd(int deley = 100, bool isStart = false)
    {
      await Task.Delay(deley);
      this.TitleTextBox.Focus();
      this.TitleTextBox.CaretPosition = this.TitleTextBox.CaretPosition.DocumentEnd;
      if (!isStart)
        return;
      this.OnTextChanged();
      this.TitleTextBox.SelectAll();
    }

    public async void InitSearch(bool restore)
    {
      SearchInputControl searchInputControl = this;
      searchInputControl._restore = restore;
      SearchHelper.ClearTaskSearchModels();
      if (restore)
      {
        searchInputControl._searchExtra = new SearchExtra()
        {
          SearchKey = SearchHelper.SearchKey,
          Tags = SearchHelper.Tags
        };
        searchInputControl._tags = SearchHelper.Tags;
        searchInputControl.SetTitleText(searchInputControl._searchExtra);
        searchInputControl.OnTextChanged(true);
      }
      else
      {
        SearchInputControl._inputText = (string) null;
        searchInputControl._tags = new List<string>();
        searchInputControl.SetSearchItem(string.Empty);
      }
      searchInputControl.TitleTextBox.TextChanged += new TextChangedEventHandler(searchInputControl.TaskTextChanged);
    }

    private List<string> GetInputTags()
    {
      List<string> inputTags = new List<string>();
      InlineCollection textInlines = this.GetTextInlines();
      if (textInlines != null)
      {
        foreach (Inline inline in (TextElementCollection<Inline>) textInlines)
        {
          if (inline is InlineUIContainer inlineUiContainer && inlineUiContainer.Child != null && inlineUiContainer.Child is QuickAddDisplayControl child)
            inputTags.Add(child.Text.Text.Replace("#", string.Empty).Trim());
        }
      }
      return inputTags;
    }

    private void RemoveEmptyRuns()
    {
      List<Inline> list = this.GetTextInlines().ToList<Inline>();
      for (int index = 0; index < list.Count - 1; ++index)
      {
        Inline inline1 = list[index];
        Inline inline2 = list[index + 1];
        if (inline1 is Run run1 && inline2 is Run run2 && run1.Text == " " && run2.Text == " ")
          run2.Text = string.Empty;
      }
    }

    private Run GetTagRun()
    {
      InlineCollection textInlines = this.GetTextInlines();
      if (textInlines != null)
      {
        foreach (Inline inline in (TextElementCollection<Inline>) textInlines)
        {
          if (inline is Run tagRun && tagRun.Text.EndsWith("#"))
            return tagRun;
        }
      }
      return (Run) null;
    }

    private void OnTagOrProjectSelected(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement))
        return;
      if (!(frameworkElement.DataContext is SearchTagAndProjectModel dataContext))
        return;
      try
      {
        int num = this._searchItemModels.IndexOf((SearchItemBaseModel) dataContext);
        if (num > 0)
        {
          for (int index = num; index >= 0; --index)
          {
            if (this._searchItemModels[index] is SearchTitleModel)
            {
              UserActCollectUtils.AddClickEvent("search", "search_result_order", (dataContext.IsTag ? "tag" : (dataContext.IsProject ? "list" : "filter")) + "_order_" + (num - index).ToString());
              break;
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      if (dataContext.IsTag)
      {
        UserActCollectUtils.AddClickEvent("search", "search_type", "tag");
        this.AddClickEvent("click_tag");
        App.Window.NavigateProject("tag", dataContext.Name.ToLower());
      }
      else if (dataContext.IsProject)
      {
        UserActCollectUtils.AddClickEvent("search", "search_type", "list");
        this.AddClickEvent("click_list");
        App.Window.NavigateProject("project", dataContext.Id);
      }
      else if (dataContext.IsFilter)
      {
        UserActCollectUtils.AddClickEvent("search", "search_type", "filter");
        this.AddClickEvent("click_filter");
        App.Window.NavigateProject("filter", dataContext.Id);
      }
      EventHandler<EventArgs> close = this.Close;
      if (close != null)
        close((object) this, (EventArgs) null);
      this._keepOldText = false;
      SearchHistoryDao.AddHistoryModel(this.GetContentText(), this.GetInputTags());
    }

    private void SearchHistoryClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Grid grid) || !(grid.DataContext is SearchHistoryViewModel dataContext))
        return;
      this.SearchHistory(dataContext);
    }

    private void SearchHistory(SearchHistoryViewModel model)
    {
      this._searchExtra = new SearchExtra()
      {
        SearchKey = model.SearchKey,
        Tags = model.Tags
      };
      SearchInputControl._inputText = (string) null;
      this.SetTitleText(this._searchExtra);
    }

    private void SetTitleText(SearchExtra searchExtra)
    {
      Paragraph paragraph = new Paragraph();
      InlineCollection inlines = paragraph.Inlines;
      if (searchExtra.Tags != null)
      {
        foreach (string tag in searchExtra.Tags)
        {
          InlineUIContainer inlineUiContainer = new InlineUIContainer((UIElement) new QuickAddDisplayControl()
          {
            Text = {
              Text = ("#" + tag)
            }
          });
          inlines.Add((Inline) inlineUiContainer);
          inlines.Add((Inline) new Run(" "));
        }
      }
      if (!string.IsNullOrEmpty(searchExtra.SearchKey))
      {
        InlineCollection inlineCollection = inlines;
        Run run = new Run(searchExtra.SearchKey);
        run.Foreground = (Brush) ThemeUtil.GetColor("BaseColorOpacity100_80");
        inlineCollection.Add((Inline) run);
      }
      this.TitleTextBox.Document.Blocks.Clear();
      this.TitleTextBox.Document.Blocks.Add((Block) paragraph);
      this.TitleTextBox.CaretPosition = this.TitleTextBox.CaretPosition.DocumentEnd;
      this.TryAdjustTextAlignment();
    }

    private void OnTextKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Tab:
          if (Utils.IfShiftPressed())
          {
            this.MoveUp();
            break;
          }
          this.MoveDown();
          break;
        case Key.Return:
          if (!this.SelectItem())
            break;
          e.Handled = true;
          break;
        case Key.Up:
          this.MoveUp();
          break;
        case Key.Down:
          this.MoveDown();
          break;
      }
    }

    private bool SelectItem()
    {
      if (this.SearchItemList.IsHitTestVisible)
      {
        if (this._selectedIndex == -1 || this._selectedIndex == -2)
        {
          this.AddClickEvent("click_task");
          this.TryDelaySearch();
        }
        if (this._selectedIndex >= 0 && this._selectedIndex < this._searchItemModels.Count)
        {
          SearchItemBaseModel searchItemModel = this._searchItemModels[this._selectedIndex];
          if (searchItemModel is SearchTagAndProjectModel tagAndProjectModel)
          {
            EventHandler<EventArgs> close = this.Close;
            if (close != null)
              close((object) this, (EventArgs) null);
            if (tagAndProjectModel.IsTag)
              App.SelectTagProject(tagAndProjectModel.Name.ToLower());
            else if (tagAndProjectModel.IsFilter)
              App.Window.NavigateFilter(searchItemModel.Id);
            else
              App.Window.NavigateNormalProject(searchItemModel.Id);
            return true;
          }
          this.AddClickEvent("click_task");
          this.TryDelaySearch(searchItemModel is SearchTaskItemViewModel taskItemViewModel ? taskItemViewModel.SourceModel.Id : (string) null);
        }
        else
          this.TryDelaySearch();
      }
      else if (this.SearchHistoryList.IsVisible && this._selectedIndex >= 0 && this._searchHistoryModels[this._selectedIndex] != null)
      {
        this.SearchHistory(this._searchHistoryModels[this._selectedIndex]);
        return true;
      }
      return false;
    }

    private void MoveDown()
    {
      if (this.SearchItemList.IsVisible)
      {
        foreach (SearchItemBaseModel searchItemModel in (Collection<SearchItemBaseModel>) this._searchItemModels)
          searchItemModel.Selected = false;
        if (this._searchItemModels.Count <= 0)
          return;
        while (++this._selectedIndex < this._searchItemModels.Count)
        {
          if (this._searchItemModels[this._selectedIndex].CanSelect)
          {
            this._searchItemModels[this._selectedIndex].Selected = true;
            this.SearchItemList.ScrollIntoView(this.SearchItemList.Items[this._selectedIndex]);
            break;
          }
        }
        if (this._selectedIndex < this._searchItemModels.Count)
          return;
        this._selectedIndex = -1;
      }
      else
      {
        if (this._searchHistoryModels.Count <= 0)
          return;
        foreach (SearchHistoryViewModel searchHistoryModel in (Collection<SearchHistoryViewModel>) this._searchHistoryModels)
          searchHistoryModel.Selected = false;
        if (++this._selectedIndex < this._searchHistoryModels.Count)
          this._searchHistoryModels[this._selectedIndex].Selected = true;
        else
          this._selectedIndex = -1;
      }
    }

    private void MoveUp()
    {
      if (this.SearchItemList.IsVisible)
      {
        foreach (SearchItemBaseModel searchItemModel in (Collection<SearchItemBaseModel>) this._searchItemModels)
          searchItemModel.Selected = false;
        if (this._searchItemModels.Count <= 0)
          return;
        if (this._selectedIndex <= -1)
          this._selectedIndex = this._searchItemModels.Count;
        while (--this._selectedIndex >= 0)
        {
          if (this._searchItemModels[this._selectedIndex].CanSelect)
          {
            this._searchItemModels[this._selectedIndex].Selected = true;
            this.SearchItemList.ScrollIntoView(this.SearchItemList.Items[this._selectedIndex]);
            break;
          }
        }
      }
      else
      {
        ObservableCollection<SearchHistoryViewModel> searchHistoryModels = this._searchHistoryModels;
        // ISSUE: explicit non-virtual call
        if ((searchHistoryModels != null ? (__nonvirtual (searchHistoryModels.Count) > 0 ? 1 : 0) : 0) == 0)
          return;
        foreach (SearchHistoryViewModel searchHistoryModel in (Collection<SearchHistoryViewModel>) this._searchHistoryModels)
          searchHistoryModel.Selected = false;
        --this._selectedIndex;
        if (this._selectedIndex == -1)
          return;
        if (this._selectedIndex < -1 || this._selectedIndex >= this._searchHistoryModels.Count)
          this._selectedIndex = this._searchHistoryModels.Count - 1;
        this._searchHistoryModels[this._selectedIndex].Selected = true;
      }
    }

    private void OnItemHover(object sender, MouseEventArgs e)
    {
      foreach (SearchItemBaseModel searchItemModel in (Collection<SearchItemBaseModel>) this._searchItemModels)
        searchItemModel.Selected = false;
      if (!(sender is FrameworkElement frameworkElement))
        return;
      if (frameworkElement.DataContext is SearchItemBaseModel dataContext1)
      {
        if (this._selectedIndex >= 0)
          this._searchItemModels[this._selectedIndex].Selected = false;
        dataContext1.Selected = true;
        this._selectedIndex = this._searchItemModels.IndexOf(dataContext1);
      }
      else
      {
        if (!(frameworkElement.DataContext is SearchHistoryViewModel dataContext))
          return;
        if (this._selectedIndex >= 0)
          this._searchHistoryModels[this._selectedIndex].Selected = false;
        dataContext.Selected = true;
        this._selectedIndex = this._searchHistoryModels.IndexOf(dataContext);
      }
    }

    private void OnItemMouseLeave(object sender, MouseEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement))
        return;
      if (frameworkElement.DataContext is SearchTagAndProjectModel dataContext2)
      {
        if (!dataContext2.Selected)
          return;
        this._selectedIndex = -1;
        dataContext2.Selected = false;
      }
      else if (frameworkElement.DataContext is SearchTaskItemViewModel dataContext1)
      {
        if (!dataContext1.Selected)
          return;
        this._selectedIndex = -1;
        dataContext1.Selected = false;
      }
      else
      {
        if (!(frameworkElement.DataContext is SearchHistoryViewModel dataContext) || !dataContext.Selected)
          return;
        this._selectedIndex = -1;
        dataContext.Selected = false;
      }
    }

    private async void OnClearHistoryClick(object sender, MouseButtonEventArgs e)
    {
      this._selectedIndex = -1;
      await SearchHistoryDao.ClearHistory();
      this.TitleTextBox.Focus();
      this.SetSearchItem(string.Empty);
    }

    private void CloseClick(object sender, MouseButtonEventArgs e)
    {
      EventHandler<EventArgs> close = this.Close;
      if (close == null)
        return;
      close((object) this, (EventArgs) null);
    }

    public bool CanDragMove()
    {
      return !this.CloseGrid.IsMouseOver && !this.ClearGrid.IsMouseOver && !this.SearchHistoryPage.IsMouseOver && !this.SearchMore.IsMouseOver && !this.NoTaskGridTitleTextBoxOnSearch.IsMouseOver;
    }

    private void ClearClick(object sender, MouseButtonEventArgs e)
    {
      this.TitleTextBox.Document.Blocks.Clear();
      this.OnTextChanged();
    }

    private void OnLoadMoreClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is SearchLoadMoreModel dataContext))
        return;
      if (dataContext.IsTask)
      {
        this.TryDelaySearch();
        this.AddClickEvent("view_more_task");
      }
      else
      {
        List<SearchTagAndProjectModel> children = dataContext.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 5 ? 1 : 0) : 0) == 0)
          return;
        int index1 = this._searchItemModels.IndexOf((SearchItemBaseModel) dataContext);
        if (index1 <= 0)
          return;
        this._searchItemModels.RemoveAt(index1);
        switch (dataContext.Type)
        {
          case PtfType.Project:
            this.AddClickEvent("expand_list");
            SearchInputControl._listExpand = true;
            break;
          case PtfType.Tag:
            this.AddClickEvent("expand_tag");
            SearchInputControl._tagExpand = true;
            break;
          case PtfType.Filter:
            this.AddClickEvent("expand_filter");
            SearchInputControl._filterExpand = true;
            break;
        }
        for (int index2 = 5; index2 < dataContext.Children.Count; ++index2)
        {
          SearchTagAndProjectModel child = dataContext.Children[index2];
          this._searchItemModels.Insert(index1, (SearchItemBaseModel) child);
          ++index1;
        }
      }
    }

    public void Dispose()
    {
      this.Search = (EventHandler<SearchExtra>) null;
      this.Close = (EventHandler<EventArgs>) null;
      this._searchActionHandler.Dispose();
    }

    private void OnSearchItemSelected(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement))
        return;
      if (!(frameworkElement.DataContext is SearchTaskItemViewModel dataContext))
        return;
      try
      {
        int num = this._searchItemModels.IndexOf((SearchItemBaseModel) dataContext);
        if (num > 0)
        {
          for (int index = num; index >= 0; --index)
          {
            if (this._searchItemModels[index] is SearchTitleModel)
              UserActCollectUtils.AddClickEvent("search", "search_result_order", "task_order_" + (num - index).ToString());
          }
        }
      }
      catch (Exception ex)
      {
      }
      this.TryDelaySearch(dataContext.SourceModel.Id);
      this.AddClickEvent("click_task");
    }

    public void OnItemCheckClick()
    {
      SearchHelper.ClearTaskSearchModels();
      SearchHelper.GetTaskSearchModels(this._restore ? SearchHelper.SearchFilter.ToSearchFilter() : new SearchFilterModel(), this.GetInputTags());
      string contentText = this.GetContentText();
      this.GetSearchDisplayModels(contentText);
      SearchHistoryDao.AddHistoryModel(contentText, this.GetInputTags());
    }

    public void OnItemProjectClick(string projectId)
    {
      App.Window.StopSearch(false);
      App.Window.NavigateNormalProject(projectId);
      EventHandler<EventArgs> close = this.Close;
      if (close != null)
        close((object) this, (EventArgs) null);
      this._keepOldText = false;
    }

    private void OnTryCloudSearchClick(object sender, MouseButtonEventArgs e)
    {
      this.AddClickEvent("view_more_task");
      this.TryDelaySearch();
    }

    private void AddClickEvent(string label)
    {
      UserActCollectUtils.AddClickEvent("search", "search_page", label);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/search/searchinputcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (SearchInputControl) target;
          break;
        case 4:
          this.FocusHighlightLine = (Line) target;
          break;
        case 5:
          this.HintText = (TextBlock) target;
          break;
        case 6:
          this.Visual = (ScrollViewer) target;
          break;
        case 7:
          this.TitleTextBox = (RichTextBox) target;
          this.TitleTextBox.PreviewKeyDown += new KeyEventHandler(this.OnTextKeyDown);
          this.TitleTextBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnTextBoxClick);
          break;
        case 8:
          this.ClearGrid = (Border) target;
          this.ClearGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.ClearClick);
          break;
        case 9:
          this.CloseGrid = (Border) target;
          this.CloseGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.CloseClick);
          break;
        case 10:
          this.SearchHistoryPage = (StackPanel) target;
          break;
        case 11:
          this.SearchHistoryList = (ItemsControl) target;
          break;
        case 12:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClearHistoryClick);
          break;
        case 13:
          this.SearchItemList = (ListView) target;
          break;
        case 14:
          this.SearchMore = (Border) target;
          break;
        case 15:
          ((ContentElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnTryCloudSearchClick);
          break;
        case 16:
          this.EmptyGrid = (StackPanel) target;
          break;
        case 17:
          this.NoTaskGridTitleTextBox = (TextBlock) target;
          break;
        case 18:
          this.NoTaskGridTitleTextBoxOnSearch = (TextBlock) target;
          break;
        case 19:
          ((ContentElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnTryCloudSearchClick);
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
      {
        if (connectionId != 3)
          return;
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.SearchHistoryClick);
        ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnItemHover);
        ((UIElement) target).MouseLeave += new MouseEventHandler(this.OnItemMouseLeave);
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnLoadMoreClick);
    }
  }
}
