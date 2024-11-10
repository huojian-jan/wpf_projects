// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.HotkeyControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class HotkeyControl : UserControl, IComponentConnector
  {
    internal HotkeyControl Root;
    internal TextBox HotkeyTextBox;
    internal TextBlock HotKeyTextBlock;
    internal Grid DeleteGrid;
    private bool _contentLoaded;

    public HotkeyModel CurrentHotkey { get; private set; }

    public event EventHandler HotkeyChanged;

    public event EventHandler HotkeyClear;

    protected virtual void OnHotkeyChanged()
    {
      EventHandler hotkeyChanged = this.HotkeyChanged;
      if (hotkeyChanged == null)
        return;
      hotkeyChanged((object) this, EventArgs.Empty);
    }

    public HotkeyControl() => this.InitializeComponent();

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(e.NewValue is ShortcutViewModel newValue) || !newValue.Editable)
        return;
      this.SetHotkey(newValue.Shortcut);
    }

    public void SetNormalMode()
    {
      this.HotKeyTextBlock.TextAlignment = TextAlignment.Center;
      this.HotKeyTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
    }

    private void TbHotkey_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
      e.Handled = true;
      if (e.Key == Key.System && (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt) || e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl || e.Key == Key.LeftShift || e.Key == Key.RightShift || e.Key == Key.LeftAlt || e.Key == Key.RightAlt || e.Key == Key.LWin || e.Key == Key.RWin)
        return;
      SpecialKeyState specialKeyState = GlobalHotkey.Instance.CheckModifiers();
      if (!specialKeyState.AltPressed && !specialKeyState.CtrlPressed && !specialKeyState.WinPressed)
      {
        if (e.Key == Key.Back || e.Key == Key.Delete)
          this.SetHotkey(new HotkeyModel(false, false, false, false, Key.None));
        if (this.DataContext is ShortcutViewModel dataContext && !dataContext.EnableOneWord && (e.Key > Key.F12 || e.Key < Key.F1))
          return;
      }
      Key key = e.Key == Key.System ? e.SystemKey : e.Key;
      if (specialKeyState.AltPressed && key >= Key.NumPad0 && key <= Key.NumPad9 || specialKeyState.CtrlPressed && key == Key.Return)
        return;
      HotkeyModel hotkeyModel = new HotkeyModel(specialKeyState.AltPressed, specialKeyState.ShiftPressed, specialKeyState.WinPressed, specialKeyState.CtrlPressed, key);
      if (hotkeyModel.ToString() == this.HotkeyTextBox.Text)
        return;
      if (this.HotKeyExisted(hotkeyModel.ToRealString().Replace(" ", "")))
        this.TryToastExist();
      else
        this.Dispatcher.InvokeAsync<Task>((Func<Task>) (async () =>
        {
          await Task.Delay(500);
          this.SetHotkey(hotkeyModel);
        }));
    }

    private void TryToastExist()
    {
      SettingDialog parent = Utils.FindParent<SettingDialog>((DependencyObject) this);
      if (parent != null)
        parent.Toast(Utils.GetString("ExistedShortcut"));
      else
        Utils.Toast(Utils.GetString("ExistedShortcut"));
    }

    private bool HotKeyExisted(string hotkey)
    {
      return LocalSettings.Settings.ShortcutAddTask == hotkey || LocalSettings.Settings.ShortcutOpenOrClose == hotkey || LocalSettings.Settings.ShortcutPin == hotkey || LocalSettings.Settings.LockShortcut == hotkey || LocalSettings.Settings.PomoShortcut == hotkey || LocalSettings.Settings.ShortCutModel.ExistKey(hotkey);
    }

    private void SetHotkey(HotkeyModel keyModel, bool init = false)
    {
      this.CurrentHotkey = keyModel;
      bool flag = this.CurrentHotkey.ToString() == "Back" || string.IsNullOrEmpty(this.CurrentHotkey.ToString());
      if (!(this.DataContext is ShortcutViewModel shortcutViewModel))
      {
        string str = keyModel.ToString();
        shortcutViewModel = new ShortcutViewModel()
        {
          OriginHotKey = string.IsNullOrEmpty(str) ? Utils.GetString("SetShortcut") : str
        };
        this.DataContext = (object) shortcutViewModel;
      }
      if (!init)
      {
        shortcutViewModel.NewHotKey = flag ? string.Empty : this.CurrentHotkey.ToString();
        shortcutViewModel.OriginHotKey = flag ? Utils.GetString("SetShortcut") : this.CurrentHotkey.ToString();
      }
      this.HotkeyTextBox.Select(this.HotkeyTextBox.Text.Length, 0);
      if (flag)
      {
        EventHandler hotkeyClear = this.HotkeyClear;
        if (hotkeyClear == null)
          return;
        hotkeyClear((object) this, (EventArgs) null);
      }
      else
      {
        if (init)
          return;
        this.OnHotkeyChanged();
      }
    }

    public void SetHotkey(string keyStr) => this.SetHotkey(new HotkeyModel(keyStr), true);

    public void TryFocus()
    {
      if (this.HotkeyTextBox.IsFocused)
        return;
      if (this.DataContext is ShortcutViewModel dataContext)
        dataContext.NewHotKey = string.Empty;
      this.HotkeyTextBox.Focus();
      this.HotkeyTextBox.SelectAll();
    }

    private void OnDeleteMouseDown(object sender, MouseButtonEventArgs e)
    {
      this.SetHotkey(new HotkeyModel(string.Empty));
      e.Handled = true;
    }

    private void TryFocusInputBox(object sender, MouseButtonEventArgs e)
    {
      this.HotkeyTextBox.Visibility = Visibility.Visible;
      this.HotkeyTextBox.Focus();
      e.Handled = true;
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Escape)
        return;
      Keyboard.ClearFocus();
      e.Handled = true;
    }

    private void OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.HotkeyTextBox.Visibility = Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/hotkeycontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (HotkeyControl) target;
          break;
        case 2:
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataContextChanged);
          break;
        case 3:
          this.HotkeyTextBox = (TextBox) target;
          this.HotkeyTextBox.PreviewKeyDown += new KeyEventHandler(this.TbHotkey_OnPreviewKeyDown);
          this.HotkeyTextBox.PreviewKeyUp += new KeyEventHandler(this.OnKeyUp);
          this.HotkeyTextBox.LostFocus += new RoutedEventHandler(this.OnLostFocus);
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.TryFocusInputBox);
          break;
        case 5:
          this.HotKeyTextBlock = (TextBlock) target;
          break;
        case 6:
          this.DeleteGrid = (Grid) target;
          this.DeleteGrid.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnDeleteMouseDown);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
