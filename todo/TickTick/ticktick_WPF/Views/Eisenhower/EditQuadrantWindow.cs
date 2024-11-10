// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Eisenhower.EditQuadrantWindow
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
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Filter;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Eisenhower
{
  public class EditQuadrantWindow : Window, IOkCancelWindow, IComponentConnector
  {
    private readonly int _level;
    private NormalFilterViewModel _normalViewModel;
    private NormalFilterViewModel _defaultViewModel;
    private readonly string _defaultName;
    private string _previousName;
    internal TextBlock TitleText;
    internal Path ProIcon;
    internal Grid InputGrid;
    internal EmojiEditor QuadrantNameText;
    internal Grid EmojiSelectGrid;
    internal Grid IconGrid;
    internal Path QuadrantIcon;
    internal Path SetEmojiIcon;
    internal EmjTextBlock EmojiText;
    internal EscPopup EmojiSelectPopup;
    internal EmojiSelectControl EmojiSelector;
    internal TextBlock ErrorText;
    internal NormalFilterControl FilterControl;
    internal TextBlock ResetText;
    internal TextBlock ExampleText;
    internal Button SaveButton;
    private bool _contentLoaded;

    public EditQuadrantWindow(int level)
    {
      this.InitializeComponent();
      this.TitleText.Text = Utils.GetString("EditMatrix");
      this._defaultName = Utils.GetString("MatrixTitleQuadrant" + level.ToString());
      this.QuadrantIcon.Data = Utils.GetIcon("MatrixIconQuadrant" + level.ToString());
      this.QuadrantIcon.Fill = (Brush) ThemeUtil.GetColor("MatrixColorQuadrant" + level.ToString());
      this.QuadrantNameText.TextChanged += new EventHandler(this.OnTextChanged);
      this.QuadrantNameText.EnterUp += new EventHandler(this.OnTextKeyUp);
      if (!UserDao.IsUserValid())
      {
        this.ProIcon.Visibility = Visibility.Visible;
        this.QuadrantNameText.ReadOnly = true;
      }
      this._level = level;
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      if (InternetExplorerBrowserEmulation.IsVersion11())
        return;
      this.ExampleText.Visibility = Visibility.Collapsed;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      QuadrantModel quadrantModel1 = MatrixManager.GetQuadrantByLevel(this._level);
      QuadrantModel quadrantModel2 = QuadrantModel.GetDefault(this._level);
      if (Parser.GetFilterRuleType(quadrantModel1.rule) != 0)
        quadrantModel1 = quadrantModel2;
      this._normalViewModel = Parser.ToNormalModel(quadrantModel1.rule);
      this._defaultViewModel = Parser.ToNormalModel(quadrantModel2.rule);
      this.FilterControl.ViewModel = this._normalViewModel;
      this.FilterControl.Init(true);
      this._previousName = string.IsNullOrEmpty(quadrantModel1.name) ? this._defaultName : quadrantModel1.name;
      this.SetQuadrantName(this._previousName);
      this.SetDefaultEnable();
    }

    protected override async void OnActivated(EventArgs e)
    {
      base.OnActivated(e);
      if (!UserDao.IsUserValid())
        return;
      await Task.Delay(50);
      this.QuadrantNameText.FocusEnd();
    }

    private void SetQuadrantName(string name)
    {
      string emojiIcon = EmojiHelper.GetEmojiIcon(name);
      if (!string.IsNullOrEmpty(emojiIcon) && name.StartsWith(emojiIcon))
      {
        this.QuadrantNameText.Text = name.Remove(0, emojiIcon.Length);
        this.SetEmoji(emojiIcon);
      }
      else
        this.QuadrantNameText.Text = name;
    }

    private void SetDefaultEnable() => this.ResetText.IsEnabled = !this.CheckIsDefault();

    private bool CheckIsDefault()
    {
      return !(this.EmojiText.Text + this.QuadrantNameText.Text.Trim() != this._defaultName) && this._normalViewModel.TaskTypes.Count != 1 && !this._normalViewModel.Projects.Any<string>() && !this._normalViewModel.Groups.Any<string>() && !this._normalViewModel.Tags.Any<string>() && this._normalViewModel.DueDates.Count == this._defaultViewModel.DueDates.Count && this._normalViewModel.DueDates.All<string>(new Func<string, bool>(this._defaultViewModel.DueDates.Contains)) && this._normalViewModel.Priorities.Count == this._defaultViewModel.Priorities.Count && this._normalViewModel.Priorities.All<int>(new Func<int, bool>(this._defaultViewModel.Priorities.Contains));
    }

    private void OnTextKeyUp(object sender, EventArgs e) => this.OnSave();

    private void OnTextChanged(object sender, EventArgs e)
    {
      this.CheckNameValid();
      this.SetDefaultEnable();
    }

    private void CheckNameValid()
    {
      string exp = this.EmojiText.Text + this.QuadrantNameText.Text.Trim();
      string empty = string.Empty;
      if (string.IsNullOrEmpty(exp))
        empty = Utils.GetString("MatrixNameEmpty");
      else if (!NameUtils.IsValidNameNoCheckSharp(exp, false))
        empty = Utils.GetString("ListNameCantContain");
      this.ErrorText.Text = empty;
      this.ErrorText.Visibility = string.IsNullOrEmpty(empty) ? Visibility.Collapsed : Visibility.Visible;
      this.SaveButton.IsEnabled = string.IsNullOrEmpty(empty);
    }

    private void SaveBtnClick(object sender, RoutedEventArgs e) => this.OnSave();

    private void OnSave()
    {
      string str = this.EmojiText.Text + this.QuadrantNameText.Text.Trim();
      string name = str == this._previousName ? (string) null : (str == this._defaultName ? string.Empty : str);
      string rule = this._normalViewModel.ToRule(false);
      LocalSettings.Settings.SaveMatrixQuadrant(this._level, rule, (SortOption) null, name);
      DataChangedNotifier.NotifyMatrixQuadrantChanged(this._level);
      this.Close();
    }

    private void CancelBtnClick(object sender, RoutedEventArgs e) => this.Close();

    private void ResetClick(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.Matrix, (Window) this))
        return;
      this._normalViewModel = Parser.ToNormalModel(MatrixManager.GetDefaultQuadrantRule(MatrixType.Simple, this._level));
      this.ResetText.IsEnabled = false;
      this.FilterControl.ViewModel = this._normalViewModel;
      this.FilterControl.Init(true);
      this.EmojiText.Text = string.Empty;
      this.QuadrantNameText.Text = this._defaultName;
      this.IconGrid.Visibility = Visibility.Visible;
      this.QuadrantNameText.FocusEnd();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);
      this.Owner?.Activate();
    }

    private void OnNotifyInvalid(object sender, EventArgs e)
    {
      ProChecker.CheckPro(ProType.Matrix, (Window) this);
    }

    public void OnCancel()
    {
      if (this.FilterControl.PopupOpened || this.EmojiSelectPopup.IsOpen)
        return;
      this.Close();
    }

    public void Ok()
    {
      if (this.FilterControl.PopupOpened || this.EmojiSelectPopup.IsOpen)
        return;
      this.OnSave();
    }

    private void OnRuleChanged(object sender, EventArgs e) => this.SetDefaultEnable();

    private void ShowEmojiSelector(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.Matrix, (Window) this))
        return;
      this.EmojiSelector.GetItems();
      this.EmojiSelector.SetCanReset(!string.IsNullOrEmpty(this.EmojiText.Text));
      this.EmojiSelectPopup.IsOpen = true;
    }

    private void OnEmojiSelected(string emoji, bool closePopup)
    {
      this.SetEmoji(emoji);
      if (closePopup)
        this.EmojiSelectPopup.IsOpen = false;
      this.CheckNameValid();
      this.SetDefaultEnable();
    }

    private void SetEmoji(string emoji)
    {
      this.EmojiText.Text = emoji ?? string.Empty;
      this.IconGrid.Visibility = string.IsNullOrEmpty(emoji) ? Visibility.Visible : Visibility.Collapsed;
      this.QuadrantNameText.MaxLength = 64 - this.EmojiText.Text.Length;
    }

    private void OnInputMouseDown(object sender, MouseButtonEventArgs e)
    {
      ProChecker.CheckPro(ProType.Matrix, (Window) this);
    }

    private void ShowExamples(object sender, MouseButtonEventArgs e) => this.ShowExampleWindow();

    private async void ShowExampleWindow()
    {
      EditQuadrantWindow editQuadrantWindow = this;
      NavigateWebBrowserWindow webBrowserWindow = new NavigateWebBrowserWindow("/webview/matrixRule", closeAction: new Action<string>(editQuadrantWindow.SetDefaultMatrixRule));
      webBrowserWindow.Owner = (Window) editQuadrantWindow;
      webBrowserWindow.Title = Utils.GetString("HowToCustomizeRules");
      webBrowserWindow.Show();
      webBrowserWindow.Topmost = true;
    }

    private void SetDefaultMatrixRule(string type)
    {
      MatrixManager.ResetMatrixRule(type == "simple");
      this.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/eisenhower/editquadrantwindow.xaml", UriKind.Relative));
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
          this.TitleText = (TextBlock) target;
          break;
        case 2:
          this.ProIcon = (Path) target;
          break;
        case 3:
          this.InputGrid = (Grid) target;
          break;
        case 4:
          this.QuadrantNameText = (EmojiEditor) target;
          break;
        case 5:
          this.EmojiSelectGrid = (Grid) target;
          this.EmojiSelectGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowEmojiSelector);
          break;
        case 6:
          this.IconGrid = (Grid) target;
          break;
        case 7:
          this.QuadrantIcon = (Path) target;
          break;
        case 8:
          this.SetEmojiIcon = (Path) target;
          break;
        case 9:
          this.EmojiText = (EmjTextBlock) target;
          break;
        case 10:
          this.EmojiSelectPopup = (EscPopup) target;
          break;
        case 11:
          this.EmojiSelector = (EmojiSelectControl) target;
          break;
        case 12:
          this.ErrorText = (TextBlock) target;
          break;
        case 13:
          this.FilterControl = (NormalFilterControl) target;
          break;
        case 14:
          this.ResetText = (TextBlock) target;
          this.ResetText.MouseLeftButtonUp += new MouseButtonEventHandler(this.ResetClick);
          break;
        case 15:
          this.ExampleText = (TextBlock) target;
          this.ExampleText.MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowExamples);
          break;
        case 16:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.SaveBtnClick);
          break;
        case 17:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.CancelBtnClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
