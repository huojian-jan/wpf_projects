// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.AddOrEditTagWindow
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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.MarkDown;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class AddOrEditTagWindow : Window, IOkCancelWindow, IComponentConnector
  {
    private TagModel _originalTag;
    private readonly string _originalTagId;
    private TagModel _addTagModel;
    private bool _isSaved;
    internal EmojiEditor TagTextBox;
    internal TextBlock TagValidHintText;
    internal ticktick_WPF.Views.Misc.ColorSelector.ColorSelector ColorItems;
    internal Grid TagParentPanel;
    internal CustomComboBox TagParentComboBox;
    internal Button SaveButton;
    private bool _contentLoaded;

    public event EventHandler<TagModel> TagSaved;

    public event EventHandler Cancel;

    public AddOrEditTagWindow()
    {
      this.InitializeComponent();
      this.ColorItems.SetSelectedColor(ThemeUtil.GetRandomColor());
      this.SaveButton.IsEnabled = false;
      this.TagParentPanel.Visibility = Visibility.Visible;
      this.TagTextBox.EnterUp += new EventHandler(this.OnEnterKeyDown);
    }

    public AddOrEditTagWindow(TagModel model, bool canEditTitle = true, bool isCreat = false)
    {
      this.InitializeComponent();
      this.TagTextBox.EnterUp += new EventHandler(this.OnEnterKeyDown);
      this.TagTextBox.ReadOnly = !canEditTitle;
      if (isCreat)
      {
        this._addTagModel = model;
        this.SaveButton.IsEnabled = false;
      }
      else
      {
        this._originalTagId = model.id;
        if (!model.IsParent())
          this.TagParentPanel.Visibility = Visibility.Visible;
      }
      this.ColorItems.SetSelectedColor(model.color);
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      this.SaveButton.Content = (object) Utils.GetString("Saving");
      this.SaveButton.IsEnabled = false;
      await this.SaveTag();
    }

    private async Task SaveTag()
    {
      AddOrEditTagWindow sender = this;
      if (sender.TagValidHintText.Visibility != Visibility.Collapsed)
        return;
      TagModel savedTag = (TagModel) null;
      string parentName = (sender.TagParentPanel.Visibility == Visibility.Collapsed || sender.TagParentComboBox.SelectedIndex == 0 ? "" : sender.TagParentComboBox.SelectedItem.Title)?.ToLower();
      TagDao.UpdateParent(parentName);
      string label = sender.TagTextBox.Text?.Trim();
      string tagName = label?.ToLower();
      string originColor = "null";
      string color = sender.ColorItems.GetSelectedColor();
      if (sender._addTagModel != null)
      {
        savedTag = sender._addTagModel;
        savedTag.name = tagName;
        savedTag.label = label;
        savedTag.color = color;
        await TagDao.SaveNewTag(savedTag);
      }
      else if (sender._originalTag == null)
      {
        savedTag = await TagService.TryCreateTag(label, color, parentName, false);
      }
      else
      {
        originColor = sender._originalTag.color;
        await TagService.SaveTag(sender._originalTag.id, sender._originalTag.name, label, color, parentName);
        if (sender._originalTag.GetDisplayName() != label)
        {
          string str = await Communicator.RenameTag(sender._originalTag.name, label);
          UtilLog.Info("EditTagWindow.SaveTag Rename : " + sender._originalTag.name + " to " + label);
          await SyncSortOrderDao.OnTagRenamed(sender._originalTag.name, label?.ToLower());
          if (sender._originalTag.name != tagName)
          {
            // ISSUE: reference to a compiler-generated method
            foreach (TagModel tag in CacheManager.GetTags().Where<TagModel>(new Func<TagModel, bool>(sender.\u003CSaveTag\u003Eb__13_0)).ToList<TagModel>())
            {
              tag.parent = tagName;
              if (tag.status != 0)
                tag.status = 1;
              await TagDao.UpdateTag(tag);
            }
          }
        }
        if (!string.IsNullOrEmpty(sender._originalTag.parent) && sender._originalTag.parent != parentName)
          TagDao.CheckParent(sender._originalTag.parent);
        SyncManager.Sync();
        savedTag = sender._originalTag;
        sender._originalTag.name = tagName;
        sender._originalTag.label = label;
        sender._originalTag.color = color;
      }
      EventHandler<TagModel> tagSaved = sender.TagSaved;
      if (tagSaved != null)
        tagSaved((object) sender, savedTag);
      sender._isSaved = true;
      if (originColor != color)
        ticktick_WPF.Views.Misc.ColorSelector.ColorSelector.TryAddClickEvent(color);
      sender.Close();
      savedTag = (TagModel) null;
      parentName = (string) null;
      label = (string) null;
      tagName = (string) null;
      originColor = (string) null;
      color = (string) null;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    protected override void OnClosing(CancelEventArgs e)
    {
      this.Owner?.Activate();
      base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
      if (!this._isSaved)
      {
        EventHandler cancel = this.Cancel;
        if (cancel != null)
          cancel((object) null, (EventArgs) null);
      }
      base.OnClosed(e);
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      AddOrEditTagWindow addOrEditTagWindow = this;
      // ISSUE: explicit non-virtual call
      __nonvirtual (addOrEditTagWindow.Title) = Utils.GetString(string.IsNullOrEmpty(addOrEditTagWindow._originalTagId) ? "AddTag" : "EditTag");
      if (!string.IsNullOrEmpty(addOrEditTagWindow._originalTagId))
      {
        TagModel tagById = await TagDao.GetTagById(addOrEditTagWindow._originalTagId);
        addOrEditTagWindow._originalTag = tagById;
        if (addOrEditTagWindow._originalTag != null)
          addOrEditTagWindow.TagTextBox.Text = addOrEditTagWindow._originalTag.GetDisplayName();
      }
      if (addOrEditTagWindow._addTagModel != null)
        addOrEditTagWindow.TagTextBox.Text = addOrEditTagWindow._addTagModel.GetDisplayName();
      if (addOrEditTagWindow.TagParentPanel.Visibility == Visibility.Visible)
      {
        // ISSUE: reference to a compiler-generated method
        List<string> list = CacheManager.GetTags().Where<TagModel>(new Func<TagModel, bool>(addOrEditTagWindow.\u003COnWindowLoaded\u003Eb__17_0)).OrderBy<TagModel, int>((Func<TagModel, int>) (t => t.GetTagType())).ThenBy<TagModel, long>((Func<TagModel, long>) (t => t.sortOrder)).Select<TagModel, string>((Func<TagModel, string>) (t => t.GetDisplayName())).ToList<string>();
        list.Insert(0, Utils.GetString("none"));
        // ISSUE: reference to a compiler-generated method
        ObservableCollection<ComboBoxViewModel> items = new ObservableCollection<ComboBoxViewModel>(list.Select<string, ComboBoxViewModel>(new Func<string, ComboBoxViewModel>(addOrEditTagWindow.\u003COnWindowLoaded\u003Eb__17_4)));
        addOrEditTagWindow.TagParentComboBox.Init<ComboBoxViewModel>(items, (ComboBoxViewModel) null);
      }
      addOrEditTagWindow.TagTextBox.FocusEnd();
    }

    private void OnTagTextChanged(object sender, EventArgs e)
    {
      this.CheckTagValid();
      this.SaveButton.IsEnabled = !this.TagValidHintText.IsVisible;
    }

    private void CheckTagValid()
    {
      if (this._originalTag != null && this.TagTextBox.Text == this._originalTag.name)
        return;
      this.SaveButton.IsEnabled = !string.IsNullOrEmpty(this.TagTextBox.Text);
      if (!NameUtils.IsValidName(this.TagTextBox.Text))
      {
        this.TagValidHintText.Text = Utils.GetString("TagNotValid");
        this.TagValidHintText.Visibility = Visibility.Visible;
      }
      else if (this.CheckIfTagExist(this.TagTextBox.Text))
      {
        this.TagValidHintText.Text = Utils.GetString("TagExisted");
        this.TagValidHintText.Visibility = Visibility.Visible;
      }
      else if (string.IsNullOrEmpty(this.TagTextBox.Text))
      {
        this.TagValidHintText.Text = Utils.GetString("TagCannotBeEmpty");
        this.TagValidHintText.Visibility = Visibility.Visible;
      }
      else
        this.TagValidHintText.Visibility = Visibility.Collapsed;
    }

    private bool CheckIfTagExist(string text)
    {
      return TagDataHelper.CheckIfTagExisted(this._originalTagId, text);
    }

    private void OnEnterKeyDown(object sender, EventArgs e)
    {
      if (!this.SaveButton.IsEnabled)
        return;
      this.OnSaveClick((object) null, (RoutedEventArgs) null);
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    public void OnCancel()
    {
      if (this.TagParentComboBox.IsOpen)
        return;
      this.Close();
    }

    public async void Ok()
    {
      if (this.TagParentComboBox.IsOpen || !this.SaveButton.IsEnabled)
        return;
      this.SaveButton.Content = (object) Utils.GetString("Saving");
      this.SaveButton.IsEnabled = false;
      await this.SaveTag();
    }

    private async void OnActivated(object sender, EventArgs e)
    {
      await Task.Delay(10);
      this.TagTextBox.FocusEnd();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tag/addoredittagwindow.xaml", UriKind.Relative));
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
          ((Window) target).Activated += new EventHandler(this.OnActivated);
          break;
        case 2:
          this.TagTextBox = (EmojiEditor) target;
          break;
        case 3:
          this.TagValidHintText = (TextBlock) target;
          break;
        case 4:
          this.ColorItems = (ticktick_WPF.Views.Misc.ColorSelector.ColorSelector) target;
          break;
        case 5:
          this.TagParentPanel = (Grid) target;
          break;
        case 6:
          this.TagParentComboBox = (CustomComboBox) target;
          break;
        case 7:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 8:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
