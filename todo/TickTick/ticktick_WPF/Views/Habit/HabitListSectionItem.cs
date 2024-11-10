// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitListSectionItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.TaskList;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitListSectionItem : EditableSectionControl
  {
    private bool _startDrag;
    private HabitListControl _parent;

    private HabitSectionListViewModel Model => this.DataContext as HabitSectionListViewModel;

    public HabitListSectionItem()
      : base(true)
    {
      this.InitializeComponent();
      this.EditBox.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
      this.Container.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSectionClick);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.Margin = new Thickness(0.0, 8.0, 0.0, 2.0);
    }

    private async void Rename()
    {
      HabitListSectionItem habitListSectionItem = this;
      habitListSectionItem.MorePopup.IsOpen = false;
      habitListSectionItem.SetEditing(true);
      habitListSectionItem.EditBox.Text = habitListSectionItem.Model.Title;
      await Task.Delay(50);
      habitListSectionItem.EditBox.Focus();
      habitListSectionItem.EditBox.SelectAll();
      habitListSectionItem.EditBox.LostFocus -= new RoutedEventHandler(habitListSectionItem.OnLostFocus);
      habitListSectionItem.EditBox.LostFocus += new RoutedEventHandler(habitListSectionItem.OnLostFocus);
    }

    private async void OnLostFocus(object sender, RoutedEventArgs e)
    {
      HabitListSectionItem habitListSectionItem = this;
      if (!habitListSectionItem.Editing)
        return;
      habitListSectionItem.SaveRename();
      habitListSectionItem.ErrorPopup.IsOpen = false;
    }

    private void Delete()
    {
      bool flag = true;
      if (this.Model.Num > 0)
        flag = new CustomerDialog(Utils.GetString(nameof (Delete)) + " " + this.Model.Title, Utils.GetString("HabitSectionDeleteInfo"), Utils.GetString(nameof (Delete)), Utils.GetString("Cancel")).ShowDialog().GetValueOrDefault();
      if (flag)
        HabitService.DeleteSectionById(this.Model.Id);
      this.MorePopup.IsOpen = false;
    }

    protected override void SetOptionAction(CustomSectionOption option)
    {
      option.SetAction((Action) null, (Action) null, new Action(this.Rename), new Action(this.Delete));
    }

    protected override async void AddButtonClick()
    {
      if (!await HabitUtils.CheckHabitLimit())
        return;
      AddOrEditHabitDialog orEditHabitDialog = new AddOrEditHabitDialog(this.Model.Id);
      orEditHabitDialog.Owner = (Window) App.Window;
      orEditHabitDialog.ShowDialog();
    }

    private async void SaveRename()
    {
      HabitListSectionItem habitListSectionItem = this;
      HabitSectionListViewModel model = habitListSectionItem.Model;
      string columnName;
      if (model?.Section == null)
      {
        model = (HabitSectionListViewModel) null;
        columnName = (string) null;
      }
      else
      {
        columnName = habitListSectionItem.EditBox.Text.Trim();
        habitListSectionItem.SetEditing(false);
        if (columnName == model.Title)
        {
          model = (HabitSectionListViewModel) null;
          columnName = (string) null;
        }
        else
        {
          (bool flag, string message) = await HabitService.CheckSectionName(columnName);
          if (flag)
          {
            model.Title = columnName;
            model.Section.Name = columnName;
            HabitService.UpdateSection(model.Section);
            model = (HabitSectionListViewModel) null;
            columnName = (string) null;
          }
          else
          {
            Utils.Toast(message);
            model = (HabitSectionListViewModel) null;
            columnName = (string) null;
          }
        }
      }
    }

    private void CancelRename() => this.SetEditing(false);

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Return:
          this.SaveRename();
          break;
        case Key.Escape:
          this.CancelRename();
          break;
      }
    }

    private void OnSectionClick(object sender, MouseButtonEventArgs e)
    {
      if (this.Model == null)
        return;
      this.Model.IsOpen = !this.Model.IsOpen;
      HabitService.UpdateSection(this.Model.Section);
      this.GetParent()?.OnSectionOpenChanged(this.Model);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (this.Model == null)
        return;
      this.MoreButton.Visibility = this.Model.IsOther ? Visibility.Collapsed : Visibility.Visible;
    }

    protected override void StartDrag(MouseEventArgs e)
    {
      this.GetParent()?.TryDragItem((object) this, e);
    }

    private HabitListControl GetParent()
    {
      this._parent = this._parent ?? Utils.FindParent<HabitListControl>((DependencyObject) this);
      return this._parent;
    }
  }
}
