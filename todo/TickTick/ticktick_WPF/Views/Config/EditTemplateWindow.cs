// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.EditTemplateWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class EditTemplateWindow : Window, IComponentConnector
  {
    private readonly string _title;
    private readonly List<string> _exsitTitles;
    private bool _isAdd;
    private bool _appendNameIndex;
    internal TextBlock AddTemplateText;
    internal TextBox TitleTextBox;
    internal Button ClearButton;
    internal TextBlock WarningText;
    internal Button OkButton;
    private bool _contentLoaded;

    public event EventHandler<string> Save;

    public event EventHandler<string> Replace;

    public EditTemplateWindow(
      string title,
      List<string> exsitTitles,
      bool isAdd = false,
      string desc = "",
      bool appendNameIndex = false)
    {
      this.InitializeComponent();
      this._title = title;
      if (isAdd)
      {
        this._isAdd = true;
        this.AddTemplateText.Visibility = Visibility.Visible;
        this.AddTemplateText.Text = desc;
        this._title = string.Empty;
      }
      this.AddTemplateText.Visibility = string.IsNullOrEmpty(desc) ? Visibility.Collapsed : Visibility.Visible;
      this._exsitTitles = exsitTitles;
      this._appendNameIndex = appendNameIndex;
      this.TitleTextBox.Text = title;
      this.OkButton.IsEnabled = !string.IsNullOrEmpty(title.Trim());
      DataObject.AddPastingHandler((DependencyObject) this.TitleTextBox, new DataObjectPastingEventHandler(this.OnPaste));
      this.Closing += (CancelEventHandler) ((sender, e) => this.Owner?.Activate());
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      await Task.Delay(50);
      this.TitleTextBox.Focus();
      this.TitleTextBox.SelectAll();
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
      e.CancelCommand();
      if (!e.SourceDataObject.GetDataPresent(DataFormats.UnicodeText, true) || !(e.SourceDataObject.GetData(DataFormats.UnicodeText) is string data))
        return;
      this.InsertPasteContent(data.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " "));
    }

    private void InsertPasteContent(string text)
    {
      string text1 = this.TitleTextBox.Text;
      int selectionStart = this.TitleTextBox.SelectionStart;
      try
      {
        this.TitleTextBox.Text = text1.Remove(selectionStart, this.TitleTextBox.SelectionLength).Insert(selectionStart, text);
        this.TitleTextBox.SelectionStart = selectionStart + text.Length;
      }
      catch (Exception ex)
      {
      }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnSaveClick(object sender, RoutedEventArgs e) => this.SaveTemplate();

    private void SaveTemplate()
    {
      string e = this.TitleTextBox.Text.Trim();
      if (this._exsitTitles != null && (!this._exsitTitles.Contains(this.TitleTextBox.Text.Trim()) ? 0 : (this.TitleTextBox.Text.Trim() != this._title ? 1 : 0)) != 0)
      {
        if (this._appendNameIndex)
        {
          e = this.GetNewTemplateName(this.TitleTextBox.Text.Trim(), this._title, this._exsitTitles);
        }
        else
        {
          if (this._isAdd)
          {
            CustomerDialog customerDialog = new CustomerDialog(Utils.GetString(nameof (SaveTemplate)), Utils.GetString("ReplaceTemplateContent"), Utils.GetString("Replace"), Utils.GetString("Cancel"));
            customerDialog.Owner = (Window) this;
            customerDialog.ShowDialog();
            if (!customerDialog.DialogResult.GetValueOrDefault())
              return;
            EventHandler<string> replace = this.Replace;
            if (replace != null)
              replace((object) null, this.TitleTextBox.Text.Trim());
            this.Close();
            return;
          }
          this.WarningText.Visibility = Visibility.Visible;
          this.OkButton.IsEnabled = false;
          return;
        }
      }
      EventHandler<string> save = this.Save;
      if (save != null)
        save((object) null, e);
      this.Close();
    }

    private string GetNewTemplateName(string newName, string title, List<string> names)
    {
      if (string.Equals(newName, title))
        return newName;
      while (names.Contains(newName))
        newName += "(1)";
      return newName;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnEscKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          if (!this.OkButton.IsEnabled)
            break;
          this.SaveTemplate();
          break;
        case Key.Escape:
          this.Close();
          break;
      }
    }

    private void ClearText(object sender, RoutedEventArgs e) => this.TitleTextBox.Text = "";

    private void OnTitleTextChanged(object sender, TextChangedEventArgs e)
    {
      this.WarningText.Visibility = Visibility.Collapsed;
      this.OkButton.IsEnabled = !string.IsNullOrEmpty(this.TitleTextBox.Text.Trim());
      if (!string.IsNullOrEmpty(this.TitleTextBox.Text))
        this.ClearButton.Visibility = Visibility.Visible;
      else
        this.ClearButton.Visibility = Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/edittemplatewindow.xaml", UriKind.Relative));
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
          this.AddTemplateText = (TextBlock) target;
          break;
        case 3:
          this.TitleTextBox = (TextBox) target;
          this.TitleTextBox.KeyDown += new KeyEventHandler(this.OnEscKeyDown);
          this.TitleTextBox.TextChanged += new TextChangedEventHandler(this.OnTitleTextChanged);
          break;
        case 4:
          this.ClearButton = (Button) target;
          this.ClearButton.Click += new RoutedEventHandler(this.ClearText);
          break;
        case 5:
          this.WarningText = (TextBlock) target;
          break;
        case 6:
          this.OkButton = (Button) target;
          this.OkButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 7:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
