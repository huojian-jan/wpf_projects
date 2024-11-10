// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchOperationControl
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
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchOperationControl : UserControl, IComponentConnector
  {
    private List<SearchOperationViewModel> _models;
    private ObservableCollection<SearchOperationViewModel> _items = new ObservableCollection<SearchOperationViewModel>();
    private System.Windows.Point _position;
    private bool _canEnter;
    internal StackPanel Container;
    internal Border TopLine;
    internal TextBox InputText;
    internal UpDownSelectListView ItemsView;
    internal StackPanel EmptyGrid;
    private bool _contentLoaded;

    public SearchOperationControl()
    {
      this.InitializeComponent();
      this.GetOperationModels();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      SearchProjectHelper.UpdateSortOrder();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      if (!(this.ItemsView.Template.FindName("ScrollViewer", (FrameworkElement) this.ItemsView) is ScrollViewer name))
        return;
      name.ScrollChanged += (ScrollChangedEventHandler) ((o, args) => this.TopLine.Visibility = args.VerticalOffset > 0.0 ? Visibility.Visible : Visibility.Collapsed);
    }

    private void GetOperationModels()
    {
      List<SearchOperationViewModel> operationViewModelList = new List<SearchOperationViewModel>();
      SearchOperationViewModel operationViewModel = new SearchOperationViewModel(SearchOperationType.AddTask);
      operationViewModel.HoverSelected = true;
      operationViewModelList.Add(operationViewModel);
      operationViewModelList.Add(new SearchOperationViewModel(Utils.GetString("GlobalOperate")));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GlobalAddTask));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.ShowOrHideApp));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.OpenOrCloseFocusWindow));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.NewSticky));
      operationViewModelList.Add(new SearchOperationViewModel(Utils.GetString("Navigation")));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.Task));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.Calendar));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.Matrix));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.Habit));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.SearchTask));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoSettings));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoAll));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoToday));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoTomorrow));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoNext7Day));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoAssignToMe));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoInbox));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoCompleted));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoAbandoned));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoSummary));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.GoTrash));
      operationViewModelList.Add(new SearchOperationViewModel(Utils.GetString("Help")));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.HelpCenter));
      operationViewModelList.Add(new SearchOperationViewModel(SearchOperationType.ShowShortCut));
      this._models = operationViewModelList;
      foreach (SearchOperationViewModel model in this._models)
        this._items.Add(model);
      this.ItemsView.ItemsSource = (IEnumerable) this._items;
    }

    public void FocusInput() => this.InputText.Focus();

    private void OnMouseDown(object sender, MouseButtonEventArgs e) => this.InputText.Focus();

    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
      this._models.ForEach((Action<SearchOperationViewModel>) (m => m.HoverSelected = false));
      string lower = this.InputText.Text.Trim().ToLower();
      if (string.IsNullOrEmpty(lower))
      {
        this.ItemsView.Visibility = Visibility.Visible;
        ItemsSourceHelper.CopyTo<SearchOperationViewModel>(this._models, this._items);
      }
      else
      {
        List<SearchOperationViewModel> source = new List<SearchOperationViewModel>();
        List<SearchTagAndProjectModel> modelsMatched = SearchProjectHelper.GetModelsMatched(lower);
        for (int index = 0; index <= this._models.Count - 1; ++index)
        {
          SearchOperationViewModel model = this._models[index];
          if (model.Type == SearchOperationType.None)
          {
            if (model.Title == Utils.GetString("Help"))
            {
              foreach (SearchTagAndProjectModel tagAndProjectModel in modelsMatched)
                source.Add(new SearchOperationViewModel()
                {
                  Type = tagAndProjectModel.IsTag ? SearchOperationType.GoTag : (tagAndProjectModel.IsProject ? SearchOperationType.GoProject : SearchOperationType.GoFilter),
                  Title = tagAndProjectModel.Name,
                  Icon = tagAndProjectModel.IconData,
                  PtfId = tagAndProjectModel.Id,
                  Emoji = tagAndProjectModel.Emoji
                });
            }
            SearchOperationViewModel operationViewModel = source.LastOrDefault<SearchOperationViewModel>();
            if (operationViewModel != null && operationViewModel.Type == SearchOperationType.None)
              source.Remove(operationViewModel);
            source.Add(model);
          }
          else if (model.Pinyin.Contains(lower) || model.Title.ToLower().Contains(lower) || model.Keyword != null && model.Keyword.ToLower().Contains(lower))
            source.Add(model);
        }
        SearchOperationViewModel operationViewModel1 = source.LastOrDefault<SearchOperationViewModel>();
        if (operationViewModel1 != null && operationViewModel1.Type == SearchOperationType.None)
          source.Remove(operationViewModel1);
        if (source.Count == 0)
          source.Add(new SearchOperationViewModel(Utils.GetString("SearchCommandNoData")));
        source.Add(new SearchOperationViewModel()
        {
          Type = SearchOperationType.GoSearch,
          Title = Utils.GetString("Search") + " \"" + lower + "\"",
          Shortcut = "",
          SearchText = lower,
          Icon = Utils.GetIcon("IcSearch")
        });
        this.ItemsView.Visibility = source.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        ItemsSourceHelper.CopyTo<SearchOperationViewModel>(source, this._items);
      }
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Return)
      {
        if (this._canEnter)
        {
          SearchOperationViewModel model = this._items.FirstOrDefault<SearchOperationViewModel>((Func<SearchOperationViewModel, bool>) (i => i.HoverSelected && i.Type != 0));
          string str = this.InputText.Text.Trim();
          if (model != null)
          {
            MainWindowManager.DoOperation(model);
            Window.GetWindow((DependencyObject) this)?.Close();
          }
          else if (!string.IsNullOrEmpty(str))
          {
            MainWindowManager.DoOperation(new SearchOperationViewModel()
            {
              Type = SearchOperationType.GoSearch,
              SearchText = str
            });
            Window.GetWindow((DependencyObject) this)?.Close();
          }
        }
        e.Handled = true;
      }
      else
        this._canEnter = true;
    }

    private void OnTextKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Tab:
          e.Handled = true;
          break;
        case Key.Up:
        case Key.Down:
          this.ItemsView.UpDownSelect(e.Key == Key.Up);
          break;
        case Key.ImeProcessed:
          this._canEnter = false;
          break;
      }
    }

    private void OnItemSelected(bool onenter, UpDownSelectViewModel e)
    {
      if (!(e is SearchOperationViewModel model) || model.Type == SearchOperationType.None)
        return;
      MainWindowManager.DoOperation(model);
      Window.GetWindow((DependencyObject) this)?.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/search/searchoperationcontrol.xaml", UriKind.Relative));
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
          this.Container = (StackPanel) target;
          this.Container.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
          break;
        case 2:
          this.TopLine = (Border) target;
          break;
        case 3:
          this.InputText = (TextBox) target;
          this.InputText.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          this.InputText.PreviewKeyDown += new KeyEventHandler(this.OnTextKeyDown);
          this.InputText.PreviewKeyUp += new KeyEventHandler(this.OnKeyUp);
          break;
        case 4:
          this.ItemsView = (UpDownSelectListView) target;
          break;
        case 5:
          this.EmptyGrid = (StackPanel) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
