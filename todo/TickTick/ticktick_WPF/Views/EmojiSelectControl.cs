// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.EmojiSelectControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.MarkDown;

#nullable disable
namespace ticktick_WPF.Views
{
  public class EmojiSelectControl : UserControl, IComponentConnector, IStyleConnector
  {
    private static List<EmojiListModel> _emojiListsInJson;
    private static Dictionary<string, string> _keyEmoji = new Dictionary<string, string>();
    private static List<EmojiKeyViewModel> _emojiKeyViewModel = new List<EmojiKeyViewModel>();
    private static readonly HashSet<string> FoldedEmojiListKeys = new HashSet<string>();
    private Border _lastHoverBd;
    private List<EmojiSelectViewModel> _items;
    internal EmojiEditor SearchTextBox;
    internal Border EmojiHeadBorder;
    internal ItemsControl EmojiKeyList;
    internal StackPanel HeadPanel;
    internal TextBlock ResetText;
    internal ListView EmojiList;
    internal Grid SearchEmptyGrid;
    internal Popup EmojiSkinPopup;
    internal ItemsControl SkinItems;
    private bool _contentLoaded;

    public event EmojiSelectControl.SelectEmoji EmojiSelected;

    public EmojiSelectControl()
    {
      this.InitializeComponent();
      this.InitEmojiKey();
      this.GetEmojiModelsFromJson();
      this.EmojiList.PreviewMouseWheel += new MouseWheelEventHandler(this.OnEmojiListMouseWheel);
    }

    private void InitEmojiKey()
    {
      EmojiSelectControl._keyEmoji["RecentUse"] = "IcEmojiKeyRecentUse";
      EmojiSelectControl._keyEmoji["PeopleAndBody"] = "IcEmojiKeyPeopleAndBody";
      EmojiSelectControl._keyEmoji["AnimalsAndNature"] = "IcEmojiKeyAnimalsAndNature";
      EmojiSelectControl._keyEmoji["FoodAndDrink"] = "IcEmojiKeyFoodAndDrink";
      EmojiSelectControl._keyEmoji["Activities"] = "IcEmojiKeyActivities";
      EmojiSelectControl._keyEmoji["TravelAndPlaces"] = "IcEmojiKeyTravelAndPlaces";
      EmojiSelectControl._keyEmoji["Objects"] = "IcEmojiKeyObjects";
      EmojiSelectControl._keyEmoji["Symbols"] = "IcEmojiKeySymbols";
      EmojiSelectControl._keyEmoji["Flags"] = "IcEmojiKeyFlags";
    }

    private string GetEmojiKey(string key)
    {
      return string.IsNullOrEmpty(key) || !EmojiSelectControl._keyEmoji.ContainsKey(key) ? string.Empty : EmojiSelectControl._keyEmoji[key];
    }

    private void GetEmojiModelsFromJson()
    {
      List<EmojiListModel> emojiListsInJson = EmojiSelectControl._emojiListsInJson;
      // ISSUE: explicit non-virtual call
      if ((emojiListsInJson != null ? (__nonvirtual (emojiListsInJson.Count) > 0 ? 1 : 0) : 0) != 0)
        return;
      EmojiSelectControl._emojiListsInJson = JsonConvert.DeserializeObject<List<EmojiListModel>>(IOUtils.GetFileContentInAssemblyFold("EmojiJsonWithSkin.json"));
    }

    public async void GetItems(bool focus = true)
    {
      EmojiSelectControl emojiSelectControl = this;
      EmojiSelectControl._emojiKeyViewModel.Clear();
      List<EmojiSelectViewModel> items = new List<EmojiSelectViewModel>();
      string recentUsedEmojis = LocalSettings.Settings.RecentUsedEmojis;
      List<string> stringList;
      if (recentUsedEmojis == null)
        stringList = (List<string>) null;
      else
        stringList = ((IEnumerable<string>) recentUsedEmojis.Split(';')).Where<string>((Func<string, bool>) (e => !string.IsNullOrEmpty(e))).ToList<string>();
      List<string> source = stringList;
      // ISSUE: explicit non-virtual call
      if (source != null && __nonvirtual (source.Count) > 0)
      {
        bool flag = EmojiSelectControl.FoldedEmojiListKeys.Contains("RecentUse");
        items.Add(new EmojiSelectViewModel()
        {
          Key = "RecentUse",
          Text = Utils.GetString("RecentUse"),
          IsSection = true,
          Folded = flag
        });
        EmojiSelectControl._emojiKeyViewModel.Add(new EmojiKeyViewModel()
        {
          Key = "RecentUse",
          EmojiPathName = emojiSelectControl.GetEmojiKey("RecentUse"),
          Text = Utils.GetString("RecentUse")
        });
        if (!flag)
        {
          EmojiSelectViewModel emojiSelectViewModel = new EmojiSelectViewModel()
          {
            Children = source.Select<string, EmojiSelectViewModel>((Func<string, EmojiSelectViewModel>) (emoji => new EmojiSelectViewModel()
            {
              Text = emoji
            })).ToList<EmojiSelectViewModel>()
          };
          items.Add(emojiSelectViewModel);
        }
      }
      List<EmojiListModel> emojiListsInJson = EmojiSelectControl._emojiListsInJson;
      // ISSUE: explicit non-virtual call
      if ((emojiListsInJson != null ? (__nonvirtual (emojiListsInJson.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        foreach (EmojiListModel emojiListModel in EmojiSelectControl._emojiListsInJson)
        {
          if (emojiListModel.Items != null && emojiListModel.Items.Count != 0)
          {
            bool flag = EmojiSelectControl.FoldedEmojiListKeys.Contains(emojiListModel.Key);
            EmojiSelectViewModel emojiSelectViewModel1 = new EmojiSelectViewModel()
            {
              Text = Utils.GetString(emojiListModel.Key),
              Key = emojiListModel.Key,
              IsSection = true,
              Folded = flag
            };
            items.Add(emojiSelectViewModel1);
            EmojiSelectControl._emojiKeyViewModel.Add(new EmojiKeyViewModel()
            {
              Key = emojiSelectViewModel1.Key,
              EmojiPathName = emojiSelectControl.GetEmojiKey(emojiSelectViewModel1.Key),
              Text = Utils.GetString(emojiListModel.Key)
            });
            EmojiSelectViewModel emojiSelectViewModel2 = (EmojiSelectViewModel) null;
            if (!flag)
            {
              for (int index = 0; index < emojiListModel.Items.Count; ++index)
              {
                if (index % 9 == 0)
                {
                  emojiSelectViewModel2 = new EmojiSelectViewModel()
                  {
                    Key = emojiSelectViewModel1.Key
                  };
                  emojiSelectViewModel2.Children = new List<EmojiSelectViewModel>();
                  items.Add(emojiSelectViewModel2);
                }
                EmojiItemModel emojiItemModel = emojiListModel.Items[index];
                EmojiSelectViewModel emojiSelectViewModel3 = new EmojiSelectViewModel()
                {
                  Text = emojiItemModel.Key
                };
                List<string> skin = emojiItemModel.Skin;
                // ISSUE: explicit non-virtual call
                if ((skin != null ? (__nonvirtual (skin.Count) > 0 ? 1 : 0) : 0) != 0)
                  emojiSelectViewModel3.Children = emojiItemModel.Skin.Select<string, EmojiSelectViewModel>((Func<string, EmojiSelectViewModel>) (s => new EmojiSelectViewModel()
                  {
                    Text = s
                  })).ToList<EmojiSelectViewModel>();
                emojiSelectViewModel2?.Children.Add(emojiSelectViewModel3);
              }
            }
          }
        }
      }
      ItemsSourceHelper.SetItemsSource<EmojiKeyViewModel>(emojiSelectControl.EmojiKeyList, EmojiSelectControl._emojiKeyViewModel);
      ItemsSourceHelper.SetItemsSource<EmojiSelectViewModel>((ItemsControl) emojiSelectControl.EmojiList, items);
      emojiSelectControl._items = items;
      emojiSelectControl.SearchTextBox.TextChanged -= new EventHandler(emojiSelectControl.TrySearchEmoji);
      emojiSelectControl.SearchTextBox.Text = string.Empty;
      emojiSelectControl.SearchTextBox.TextChanged += new EventHandler(emojiSelectControl.TrySearchEmoji);
      if (!focus)
        return;
      await Task.Delay(500);
      emojiSelectControl.SearchTextBox.Focus();
    }

    private void OnSwitchClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is StackPanel stackPanel) || !(stackPanel.DataContext is EmojiSelectViewModel dataContext))
        return;
      if (dataContext.Folded)
        EmojiSelectControl.FoldedEmojiListKeys.Remove(dataContext.Key);
      else
        EmojiSelectControl.FoldedEmojiListKeys.Add(dataContext.Key);
      this.GetItems(false);
    }

    private void OnEmojiClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Border border) || !(border.DataContext is EmojiSelectViewModel dataContext))
        return;
      EmojiSelectControl.SelectEmoji emojiSelected = this.EmojiSelected;
      if (emojiSelected != null)
        emojiSelected(dataContext.Text, true);
      this.SetRecentSelected(dataContext.Text);
    }

    private void SetRecentSelected(string emoji)
    {
      string recentUsedEmojis = LocalSettings.Settings.RecentUsedEmojis;
      List<string> stringList;
      if (recentUsedEmojis == null)
        stringList = (List<string>) null;
      else
        stringList = ((IEnumerable<string>) recentUsedEmojis.Split(';')).Where<string>((Func<string, bool>) (e => !string.IsNullOrEmpty(e))).ToList<string>();
      List<string> values = stringList;
      // ISSUE: explicit non-virtual call
      if (values != null && __nonvirtual (values.Count) > 0)
      {
        values.Remove(emoji);
        // ISSUE: explicit non-virtual call
        if (values != null && __nonvirtual (values.Count) == 12)
          values.Remove(values[11]);
        if (values.Count > 0)
        {
          string str = string.Join(";", (IEnumerable<string>) values);
          LocalSettings.Settings.RecentUsedEmojis = emoji + ";" + str;
        }
        else
          LocalSettings.Settings.RecentUsedEmojis = emoji;
      }
      else
        LocalSettings.Settings.RecentUsedEmojis = emoji;
    }

    private void OnEmojiMouseMove(object sender, MouseEventArgs e)
    {
      if (!(sender is Border border) || this._lastHoverBd != null && this._lastHoverBd.Equals((object) border) || !(border.DataContext is EmojiSelectViewModel dataContext) || this.EmojiSkinPopup.IsMouseOver)
        return;
      this._lastHoverBd = border;
      List<EmojiSelectViewModel> children = dataContext.Children;
      // ISSUE: explicit non-virtual call
      if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        this.EmojiSkinPopup.VerticalOffset = border.TranslatePoint(new System.Windows.Point(0.0, 0.0), (UIElement) this.EmojiList).Y >= 30.0 ? -30.0 : 35.0;
        this.SkinItems.ItemsSource = (IEnumerable) dataContext.Children;
        this.EmojiSkinPopup.PlacementTarget = (UIElement) border;
        this.EmojiSkinPopup.IsOpen = false;
        this.EmojiSkinPopup.IsOpen = true;
      }
      else
        this.EmojiSkinPopup.IsOpen = false;
    }

    private void OnResetClick(object sender, MouseButtonEventArgs e)
    {
      EmojiSelectControl.SelectEmoji emojiSelected = this.EmojiSelected;
      if (emojiSelected != null)
        emojiSelected(string.Empty, false);
      this.SetCanReset(false);
    }

    public void SetCanReset(bool enable)
    {
      this.ResetText.IsEnabled = enable;
      this.ResetText.Opacity = enable ? 1.0 : 0.4;
    }

    private void OnRandomClick(object sender, MouseButtonEventArgs e)
    {
      List<EmojiListModel> emojiListsInJson = EmojiSelectControl._emojiListsInJson;
      // ISSUE: explicit non-virtual call
      if ((emojiListsInJson != null ? (__nonvirtual (emojiListsInJson.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      int index1 = new Random().Next(0, EmojiSelectControl._emojiListsInJson.Count);
      EmojiListModel emojiListModel = EmojiSelectControl._emojiListsInJson[index1];
      int index2 = new Random().Next(0, emojiListModel.Items.Count);
      EmojiItemModel emojiItemModel = emojiListModel.Items[index2];
      this.SetCanReset(true);
      EmojiSelectControl.SelectEmoji emojiSelected = this.EmojiSelected;
      if (emojiSelected == null)
        return;
      emojiSelected(emojiItemModel.Key, false);
    }

    private ScrollViewer GetScrollViewer()
    {
      return this.EmojiList.Template.FindName("ScrollViewer", (FrameworkElement) this.EmojiList) is ScrollViewer name ? name : (ScrollViewer) null;
    }

    private void OnEmojiKeyItemMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      this.EmojiList.PreviewMouseWheel -= new MouseWheelEventHandler(this.OnEmojiListMouseWheel);
      OnItemMouseUp();
      this.EmojiList.PreviewMouseWheel += new MouseWheelEventHandler(this.OnEmojiListMouseWheel);

      void OnItemMouseUp()
      {
        if (!(sender is Grid grid) || !(grid.DataContext is EmojiKeyViewModel dataContext))
          return;
        foreach (EmojiKeyViewModel emojiKeyViewModel in EmojiSelectControl._emojiKeyViewModel)
        {
          if (emojiKeyViewModel.Key.Equals(dataContext.Key) && emojiKeyViewModel.IsSelected)
            return;
          emojiKeyViewModel.IsSelected = false;
        }
        dataContext.IsSelected = true;
        int val1 = 0;
        foreach (object obj in (IEnumerable) this.EmojiList.Items)
        {
          if (obj is EmojiSelectViewModel emojiSelectViewModel)
          {
            if (emojiSelectViewModel.IsSection && emojiSelectViewModel.Key != null && emojiSelectViewModel.Key.Equals(dataContext.Key))
            {
              ScrollViewer scrollViewer = this.GetScrollViewer();
              scrollViewer?.ScrollToVerticalOffset(Math.Min((double) val1, scrollViewer.ScrollableHeight));
              this.EmojiList.ScrollIntoView((object) emojiSelectViewModel);
              break;
            }
            if (emojiSelectViewModel.IsSection)
              val1 += 28;
            else
              val1 += 38;
          }
        }
      }
    }

    private void OnEmojiListMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ListBoxItem mousePointItem1 = Utils.GetMousePointItem<ListBoxItem>(new System.Windows.Point(200.0, 15.0), (FrameworkElement) this.EmojiList);
      ListBoxItem mousePointItem2 = Utils.GetMousePointItem<ListBoxItem>(new System.Windows.Point(200.0, this.EmojiList.ActualHeight - 5.0), (FrameworkElement) this.EmojiList);
      ListBoxItem listBoxItem = mousePointItem1 == mousePointItem2 ? mousePointItem1 : mousePointItem2;
      if (listBoxItem == null || !(listBoxItem.DataContext is EmojiSelectViewModel dataContext) || dataContext.Key == null)
        return;
      string key = dataContext.Key;
      foreach (EmojiKeyViewModel emojiKeyViewModel in this.EmojiKeyList.ItemsSource)
      {
        emojiKeyViewModel.IsSelected = false;
        if (emojiKeyViewModel.Key.Equals(key))
          emojiKeyViewModel.IsSelected = true;
      }
    }

    private void OnEmojiListMouseMove(object sender, MouseEventArgs e)
    {
      if (e.GetPosition((IInputElement) this.EmojiList).Y >= 0.0 || this.EmojiSkinPopup.IsMouseOver)
        return;
      this.EmojiSkinPopup.IsOpen = false;
    }

    private void TrySearchEmoji(object sender, EventArgs e)
    {
      DelayActionHandlerCenter.TryDoAction("SearchEmoji", (EventHandler) ((o, args) => this.Dispatcher.Invoke(new Action(this.SearchEmoji))));
    }

    private void SearchEmoji()
    {
      string text = this.SearchTextBox.Text;
      if (string.IsNullOrWhiteSpace(text))
      {
        this.EmojiHeadBorder.Visibility = Visibility.Visible;
        this.HeadPanel.Visibility = Visibility.Visible;
        this.SearchEmptyGrid.Visibility = Visibility.Collapsed;
        ItemsSourceHelper.SetItemsSource<EmojiSelectViewModel>((ItemsControl) this.EmojiList, this._items);
      }
      else
      {
        if (this.EmojiHeadBorder.Visibility == Visibility.Visible)
        {
          this.EmojiHeadBorder.Visibility = Visibility.Collapsed;
          this.HeadPanel.Visibility = Visibility.Collapsed;
        }
        List<string> matchedEmoji = EmojiSearchHelper.GetMatchedEmoji(text.ToLower());
        List<EmojiSelectViewModel> items = new List<EmojiSelectViewModel>();
        if (matchedEmoji.Count > 0)
        {
          EmojiSelectViewModel emojiSelectViewModel1 = (EmojiSelectViewModel) null;
          for (int index = 0; index < matchedEmoji.Count; ++index)
          {
            if (index % 12 == 0)
            {
              emojiSelectViewModel1 = new EmojiSelectViewModel()
              {
                Key = string.Empty,
                Children = new List<EmojiSelectViewModel>()
              };
              items.Add(emojiSelectViewModel1);
            }
            string str = matchedEmoji[index];
            EmojiSelectViewModel emojiSelectViewModel2 = new EmojiSelectViewModel()
            {
              Text = str
            };
            emojiSelectViewModel1?.Children.Add(emojiSelectViewModel2);
          }
        }
        ItemsSourceHelper.SetItemsSource<EmojiSelectViewModel>((ItemsControl) this.EmojiList, items);
        this.SearchEmptyGrid.Visibility = items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/emojiselectcontrol.xaml", UriKind.Relative));
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
        case 4:
          ((UIElement) target).MouseMove += new MouseEventHandler(this.OnEmojiListMouseMove);
          break;
        case 5:
          this.SearchTextBox = (EmojiEditor) target;
          break;
        case 6:
          this.EmojiHeadBorder = (Border) target;
          break;
        case 7:
          this.EmojiKeyList = (ItemsControl) target;
          break;
        case 8:
          this.HeadPanel = (StackPanel) target;
          break;
        case 9:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnRandomClick);
          break;
        case 10:
          this.ResetText = (TextBlock) target;
          this.ResetText.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnResetClick);
          break;
        case 11:
          this.EmojiList = (ListView) target;
          break;
        case 12:
          this.SearchEmptyGrid = (Grid) target;
          break;
        case 13:
          this.EmojiSkinPopup = (Popup) target;
          break;
        case 14:
          this.SkinItems = (ItemsControl) target;
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
        case 1:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnEmojiClick);
          ((UIElement) target).MouseMove += new MouseEventHandler(this.OnEmojiMouseMove);
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSwitchClick);
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnEmojiKeyItemMouseLeftButtonUp);
          break;
      }
    }

    public delegate void SelectEmoji(string emoji, bool closePopup);
  }
}
