// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.ConditionCard
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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class ConditionCard : UserControl, IComponentConnector
  {
    protected ConditionEditDialog EditDialog;
    protected CardViewModel ViewModel;
    internal ConditionCard Card;
    internal Grid NormalGrid;
    internal Border DeleteBorder;
    internal Path DeletePath;
    internal EscPopup Popup;
    internal Grid KeywordsGrid;
    internal TextBox KeywordsText;
    private bool _contentLoaded;

    public ConditionCard(CardViewModel viewModel)
    {
      this.InitializeComponent();
      this.ViewModel = viewModel;
      this.DataContext = (object) this.ViewModel;
      if (viewModel is KeywordsViewModel keywordsViewModel)
      {
        this.KeywordsGrid.Visibility = Visibility.Visible;
        this.NormalGrid.Visibility = Visibility.Collapsed;
        this.KeywordsText.Text = keywordsViewModel.Keyword;
        if (keywordsViewModel.IsNewAdd)
          this.DelayFocus();
      }
      this.DeleteBorder.Visibility = this.ViewModel.Type == CardType.Filter || this.ViewModel.Type == CardType.LogicAnd || this.ViewModel.Type == CardType.LogicOr ? Visibility.Visible : Visibility.Hidden;
    }

    private async void DelayFocus()
    {
      await Task.Delay(100);
      this.KeywordsText.Focus();
    }

    public event EventHandler<ChangeData> OnConditionChanged;

    public event ConditionCard.GetDropdownListHandler GetDropDownList;

    private void OnClick(object sender, MouseButtonEventArgs e) => this.OnCardClick();

    protected void ShowSelectLogicDialog()
    {
      this.ShowDropdownDialog(new List<ListItemData>()
      {
        new ListItemData((object) "and", Utils.GetString("and")),
        new ListItemData((object) "or", Utils.GetString("or"))
      }, (EventHandler<ListItemData>) ((x, item) =>
      {
        CardViewModel cardViewModel = new CardViewModel()
        {
          Index = this.ViewModel.Index
        };
        if (item.Key.ToString() == "and")
          cardViewModel.Type = CardType.LogicAnd;
        else if (item.Key.ToString() == "or")
          cardViewModel.Type = CardType.LogicOr;
        this.FireEvent(new ChangeData()
        {
          Type = ChangeType.Logic,
          From = this.ViewModel,
          To = cardViewModel
        });
        this.Popup.IsOpen = false;
      }));
    }

    protected void ShowDropdownDialog(
      List<ListItemData> listItems,
      EventHandler<ListItemData> handler)
    {
      DropdownDialog dropdownDialog = new DropdownDialog()
      {
        ItemsSource = listItems
      };
      dropdownDialog.OnItemSelected += handler;
      this.Popup.Child = (UIElement) dropdownDialog;
      this.Popup.IsOpen = true;
    }

    private void OnDeleteClick(object sender, MouseButtonEventArgs e)
    {
      this.OnDeleteClick();
      e.Handled = true;
    }

    protected virtual void OnCardClick()
    {
    }

    protected virtual void OnDeleteClick()
    {
    }

    protected virtual ConditionEditDialog GenerateEditDialog() => (ConditionEditDialog) null;

    protected void FireEvent(ChangeData data)
    {
      EventHandler<ChangeData> conditionChanged = this.OnConditionChanged;
      if (conditionChanged == null)
        return;
      conditionChanged((object) this, data);
    }

    protected List<CondType> GetLeftCondTypes()
    {
      ConditionCard.GetDropdownListHandler getDropDownList = this.GetDropDownList;
      return getDropDownList == null ? (List<CondType>) null : getDropDownList();
    }

    private void OnKeyWordsTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!(this.ViewModel is KeywordsViewModel viewModel))
        return;
      viewModel.Keyword = this.KeywordsText.Text.Trim();
    }

    private void OnKeyWordsGridClick(object sender, MouseButtonEventArgs e)
    {
      if (this.KeywordsText.IsFocused)
        return;
      this.KeywordsText.Focus();
      this.KeywordsText.Select(this.KeywordsText.Text.Length, 0);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/filter/filterconditioncard.xaml", UriKind.Relative));
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
          this.Card = (ConditionCard) target;
          break;
        case 2:
          this.NormalGrid = (Grid) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClick);
          break;
        case 4:
          this.DeleteBorder = (Border) target;
          this.DeleteBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteClick);
          break;
        case 5:
          this.DeletePath = (Path) target;
          break;
        case 6:
          this.Popup = (EscPopup) target;
          break;
        case 7:
          this.KeywordsGrid = (Grid) target;
          this.KeywordsGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnKeyWordsGridClick);
          break;
        case 8:
          this.KeywordsText = (TextBox) target;
          this.KeywordsText.TextChanged += new TextChangedEventHandler(this.OnKeyWordsTextChanged);
          break;
        case 9:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate List<CondType> GetDropdownListHandler();
  }
}
