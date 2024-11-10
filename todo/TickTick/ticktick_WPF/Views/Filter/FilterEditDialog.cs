// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.FilterEditDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Widget;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class FilterEditDialog : Window, IOkCancelWindow, IComponentConnector
  {
    private readonly FilterViewModel _viewModel;
    private bool _toastValid;
    public bool Saved;
    internal GroupTitle GroupTitle;
    internal Grid InputGrid;
    internal EmojiEditor FilterNameText;
    internal Grid EmojiSelectGrid;
    internal Grid IconGrid;
    internal Path ProjectPath;
    internal Path SetEmojiIcon;
    internal EmjTextBlock EmojiText;
    internal EscPopup EmojiSelectPopup;
    internal EmojiSelectControl EmojiSelector;
    internal NormalFilterControl NormalFilterControl;
    internal AdvancedFilterControl AdvancedFilterControl;
    internal Button SaveButton;
    internal Border ToastBorder;
    internal TextBlock ToastText;
    private bool _contentLoaded;

    public event EventHandler<FilterModel> FilterSaved;

    public FilterEditDialog(FilterModel filter = null, bool toastValid = false)
    {
      EmojiSearchHelper.Init();
      this.InitializeComponent();
      this._viewModel = filter != null ? new FilterViewModel(filter) : new FilterViewModel();
      this.InitDataContext();
      this.CheckEmojiStart(this._viewModel);
      this._toastValid = toastValid;
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.FilterNameText.TextChanged += new EventHandler(this.OnTextChanged);
      this.FilterNameText.EnterUp += new EventHandler(this.OnTextKeyUp);
      this.SaveButton.IsEnabled = !string.IsNullOrEmpty(this.EmojiText.Text.Trim() + this._viewModel.Name);
      this.FilterNameText.Text = this._viewModel.Name;
      this.GroupTitle.SetSelectedIndex(this._viewModel.Mode != 0 ? 1 : 0);
    }

    public FilterEditDialog(SearchFilterModel searchFilterViewModel)
    {
      EmojiSearchHelper.Init();
      this.InitializeComponent();
      this.GroupTitle.Visibility = Visibility.Collapsed;
      this.FilterNameText.Margin = new Thickness(20.0, 0.0, 20.0, 12.0);
      this._viewModel = new FilterViewModel(searchFilterViewModel);
      this.InitDataContext();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.FilterNameText.TextChanged += new EventHandler(this.OnTextChanged);
      this.FilterNameText.EnterUp += new EventHandler(this.OnTextKeyUp);
      this.SaveButton.IsEnabled = !string.IsNullOrEmpty(this.EmojiText.Text.Trim() + this._viewModel.Name);
      this.FilterNameText.Text = this._viewModel.Name;
      this.GroupTitle.SetSelectedIndex(this._viewModel.Mode != 0 ? 1 : 0);
    }

    private void OnTextChanged(object sender, EventArgs e)
    {
      this._viewModel.Name = this.EmojiText.Text.Trim() + this.FilterNameText.Text.Trim();
      this.SaveButton.IsEnabled = !string.IsNullOrEmpty(this._viewModel.Name);
    }

    private void OnTextKeyUp(object sender, EventArgs e)
    {
      this.SaveBtnClick((object) null, (RoutedEventArgs) null);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      FilterEditDialog filterEditDialog = this;
      Task.Run((Action) (() => AvatarHelper.GetAllProjectAvatars()));
      await Task.Delay(100);
      filterEditDialog.Activate();
      filterEditDialog.FilterNameText.FocusEnd();
      if (filterEditDialog._toastValid)
        filterEditDialog.IsFilterNameValid();
      filterEditDialog.Loaded -= new RoutedEventHandler(filterEditDialog.OnLoaded);
    }

    public event EventHandler<FilterViewModel> OnSave;

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void InitDataContext()
    {
      this.NormalFilterControl.ViewModel = this._viewModel.NormalViewModel;
      this.NormalFilterControl.Init();
      this.AdvancedFilterControl.ViewModel = this._viewModel.AdvancedViewModel;
      this.AdvancedFilterControl.ExistNote = LocalSettings.Settings.IsNoteEnabled;
      this.DataContext = (object) this._viewModel;
      this.Title = this._viewModel.EditType == Constants.ModelEditType.Edit ? Utils.GetString("EditFilter") : Utils.GetString("AddFilter");
    }

    private void CancelBtnClick(object sender, RoutedEventArgs e) => this.Close();

    private async void SaveBtnClick(object sender, RoutedEventArgs e)
    {
      if (!await this.IsFilterNameValid() || !this.CheckAdvancedRule())
        return;
      this.SaveData();
    }

    private async Task<bool> IsFilterNameValid()
    {
      string str = this.EmojiText.Text.Trim() + this.FilterNameText.Text.Trim();
      if (string.IsNullOrEmpty(str))
      {
        this.Toast(Utils.GetString("AddOrEditProjectNameCantNull"));
        return false;
      }
      if (str.StartsWith("#"))
      {
        this.Toast(Utils.GetString("ListNameBeginError"));
        return false;
      }
      if (!NameUtils.IsValidNameNoCheckSharp(str, false))
      {
        this.Toast(Utils.GetString("ListNameCantContain"));
        return false;
      }
      if (!await this.CheckFilterExist(str))
        return true;
      this.Toast(Utils.GetString("AddOrEditProjectNameRepeat"));
      return false;
    }

    private async Task<bool> CheckFilterExist(string name)
    {
      FilterModel filterModel = (await FilterDao.GetAllFilters()).FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.name == name));
      if (this._viewModel.EditType == Constants.ModelEditType.New)
      {
        if (filterModel != null)
          return true;
      }
      else if (filterModel != null && filterModel.id != this._viewModel.FilterModel.id)
        return true;
      return false;
    }

    private bool CheckAdvancedRule()
    {
      if (this._viewModel.Mode == FilterMode.Normal)
        return true;
      if (this.AdvancedFilterControl.ViewModel.ConditionValid())
      {
        if (this.AdvancedFilterControl.ViewModel.CheckKeywordValid())
          return true;
        this.Toast(Utils.GetString("InvalidFilterRule"));
        return false;
      }
      this.Toast(Utils.GetString("InvalidFilterRule"));
      return false;
    }

    public void Toast(string toastText)
    {
      this.ToastBorder.Visibility = Visibility.Visible;
      this.ToastText.Text = toastText;
      this.ToastBorder.BeginStoryboard((Storyboard) this.FindResource((object) "ShowToast"));
    }

    private void OnToasted(object sender, EventArgs e)
    {
      this.ToastBorder.Visibility = Visibility.Collapsed;
    }

    private async void SaveData()
    {
      FilterEditDialog filterEditDialog = this;
      filterEditDialog._viewModel.Name = filterEditDialog.EmojiText.Text.Trim() + filterEditDialog.FilterNameText.Text.Trim();
      FilterModel filter = await filterEditDialog._viewModel.Save();
      UtilLog.Info(string.Format("FilterDialog.FilterSave : {0},isAdd {1}", (object) filterEditDialog._viewModel.FilterModel.id, (object) (filterEditDialog._viewModel.EditType == Constants.ModelEditType.New)));
      filterEditDialog.Saved = true;
      ProjectWidgetsHelper.OnProjectChanged((ProjectIdentity) new FilterProjectIdentity(filter));
      EventHandler<FilterModel> filterSaved = filterEditDialog.FilterSaved;
      if (filterSaved != null)
        filterSaved((object) null, filterEditDialog._viewModel.FilterModel);
      filterEditDialog.Close();
    }

    private void ShowExamples(object sender, MouseButtonEventArgs e)
    {
      UserActCollectUtils.AddClickEvent("edit_filter", "action", "example");
      Utils.TryProcessStartUrl(Utils.IsDida() ? "https://guide.dida365.com/customsmartlistrules.html" : "https://help.ticktick.com/articles/7055782240994721792");
    }

    private void ShowPreview(object sender, MouseButtonEventArgs e)
    {
      UserActCollectUtils.AddClickEvent("edit_filter", "action", "preview");
      this._viewModel.Name = this.EmojiText.Text.Trim() + this.FilterNameText.Text.Trim();
      if (!this.CheckAdvancedRule())
        return;
      FilterPreviewWindow filterPreviewWindow = new FilterPreviewWindow(this._viewModel.GetFilterModel());
      filterPreviewWindow.Owner = this.Owner;
      this.Close();
      filterPreviewWindow.ShowDialog();
    }

    public void OnCancel() => this.Close();

    public async void Ok()
    {
      this._viewModel.Name = this.EmojiText.Text.Trim() + this.FilterNameText.Text.Trim();
      if (!await this.IsFilterNameValid() || !this.CheckAdvancedRule())
        return;
      this.SaveData();
    }

    private void OnSelectedTitleChanged(object sender, GroupTitleViewModel e)
    {
      if (this._viewModel == null)
        return;
      if ("advanced".Equals(e.Title))
      {
        this._viewModel.Mode = FilterMode.Advanced;
        if (this._viewModel.AdvancedViewModel.CardList.Count != 0)
          return;
        this.AdvancedFilterControl.CardList = FilterConditionProvider.BuildInitData();
      }
      else
        this._viewModel.Mode = FilterMode.Normal;
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (this.FilterNameText.IsMouseOver)
        return;
      this.FilterNameText.TabIndex = -1;
      Keyboard.ClearFocus();
    }

    private void ShowEmojiSelector(object sender, MouseButtonEventArgs e)
    {
      this.EmojiSelector.GetItems();
      this.EmojiSelector.SetCanReset(!string.IsNullOrEmpty(this.EmojiText.Text));
      this.EmojiSelectPopup.IsOpen = true;
    }

    private void CheckEmojiStart(FilterViewModel model)
    {
      string emojiIcon = EmojiHelper.GetEmojiIcon(model.Name);
      if (string.IsNullOrEmpty(emojiIcon) || !model.Name.StartsWith(emojiIcon))
        return;
      model.Name = model.Name.Remove(0, emojiIcon.Length);
      this.SetEmoji(emojiIcon);
    }

    private void SetEmoji(string emoji)
    {
      this.EmojiText.Text = emoji ?? string.Empty;
      this.IconGrid.Visibility = string.IsNullOrEmpty(emoji) ? Visibility.Visible : Visibility.Collapsed;
      this.FilterNameText.MaxLength = 64 - this.EmojiText.Text.Length;
    }

    private void OnEmojiSelected(string emoji, bool closePopup)
    {
      this.SetEmoji(emoji);
      if (closePopup)
        this.EmojiSelectPopup.IsOpen = false;
      this._viewModel.Name = this.EmojiText.Text.Trim() + this.FilterNameText.Text.Trim();
      this.SaveButton.IsEnabled = !string.IsNullOrEmpty(this._viewModel.Name);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      this.Owner?.Activate();
      base.OnClosing(e);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/filtereditdialog.xaml", UriKind.Relative));
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
          ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnPreviewMouseDown);
          break;
        case 2:
          ((Timeline) target).Completed += new EventHandler(this.OnToasted);
          break;
        case 3:
          this.GroupTitle = (GroupTitle) target;
          break;
        case 4:
          this.InputGrid = (Grid) target;
          break;
        case 5:
          this.FilterNameText = (EmojiEditor) target;
          break;
        case 6:
          this.EmojiSelectGrid = (Grid) target;
          this.EmojiSelectGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowEmojiSelector);
          break;
        case 7:
          this.IconGrid = (Grid) target;
          break;
        case 8:
          this.ProjectPath = (Path) target;
          break;
        case 9:
          this.SetEmojiIcon = (Path) target;
          break;
        case 10:
          this.EmojiText = (EmjTextBlock) target;
          break;
        case 11:
          this.EmojiSelectPopup = (EscPopup) target;
          break;
        case 12:
          this.EmojiSelector = (EmojiSelectControl) target;
          break;
        case 13:
          this.NormalFilterControl = (NormalFilterControl) target;
          break;
        case 14:
          this.AdvancedFilterControl = (AdvancedFilterControl) target;
          break;
        case 15:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowExamples);
          break;
        case 16:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.SaveBtnClick);
          break;
        case 17:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CancelBtnClick);
          break;
        case 18:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowPreview);
          break;
        case 19:
          this.ToastBorder = (Border) target;
          break;
        case 20:
          this.ToastText = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
