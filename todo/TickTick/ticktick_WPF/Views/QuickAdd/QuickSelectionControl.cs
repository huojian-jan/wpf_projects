// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.QuickAdd.QuickSelectionControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Views.QuickAdd
{
  public class QuickSelectionControl : UserControl, IComponentConnector
  {
    private IQuickInput _quickInputItems;
    private readonly Popup _popup;
    private string _avatarProject;
    private DateTime _lastPullTime;
    private List<AvatarViewModel> _avatars;
    internal Grid ItemsContainer;
    private bool _contentLoaded;

    public event EventHandler<QuickSetModel> ItemSelected;

    public event EventHandler<bool> PopupOpenChanged;

    public QuickSelectionControl(Popup popup)
    {
      Popup popup1 = popup;
      if (popup1 == null)
      {
        EscPopup escPopup = new EscPopup();
        escPopup.StaysOpen = false;
        escPopup.PopupAnimation = PopupAnimation.Fade;
        escPopup.Placement = PlacementMode.Absolute;
        popup1 = (Popup) escPopup;
      }
      this._popup = popup1;
      this.InitializeComponent();
      this._popup.Opened += (EventHandler) ((o, e) =>
      {
        EventHandler<bool> popupOpenChanged = this.PopupOpenChanged;
        if (popupOpenChanged == null)
          return;
        popupOpenChanged((object) this, true);
      });
      this._popup.Closed += (EventHandler) ((o, e) =>
      {
        EventHandler<bool> popupOpenChanged = this.PopupOpenChanged;
        if (popupOpenChanged == null)
          return;
        popupOpenChanged((object) this, false);
      });
      this._popup.Child = (UIElement) this;
    }

    public bool PopupOpened()
    {
      Popup popup = this._popup;
      return popup != null && popup.IsOpen;
    }

    public bool SetSelectionItems(
      string mark,
      string content,
      bool usedInCal,
      string calId,
      string projectId,
      int priority,
      List<string> tags,
      string assignee)
    {
      if (mark != null)
      {
        switch (mark.Length)
        {
          case 1:
            switch (mark[0])
            {
              case '!':
              case '！':
                if (!usedInCal)
                {
                  if (!(this._quickInputItems is PriorityInputItems))
                  {
                    this._quickInputItems = (IQuickInput) new PriorityInputItems(priority);
                    ((BaseInputItems<int>) this._quickInputItems).OnItemSelected -= new EventHandler<InputItemViewModel<int>>(this.OnPrioritySelect);
                    ((BaseInputItems<int>) this._quickInputItems).OnItemSelected += new EventHandler<InputItemViewModel<int>>(this.OnPrioritySelect);
                    break;
                  }
                  ((PriorityInputItems) this._quickInputItems).SetSelected(priority);
                  break;
                }
                break;
              case '#':
              case '＃':
                if (!usedInCal && !(this._quickInputItems is TagSelectionControl))
                {
                  this._quickInputItems = (IQuickInput) new TagSelectionControl(tags);
                  ((TagSelectionControl) this._quickInputItems).TagSelected -= new EventHandler<string>(this.TagSelect);
                  ((TagSelectionControl) this._quickInputItems).TagSelected += new EventHandler<string>(this.TagSelect);
                  break;
                }
                break;
              case '*':
                if (!(this._quickInputItems is DateInputItems))
                {
                  this._quickInputItems = (IQuickInput) new DateInputItems();
                  ((BaseInputItems<DateTime>) this._quickInputItems).OnItemSelected -= new EventHandler<InputItemViewModel<DateTime>>(this.OnQuickDateSelected);
                  ((BaseInputItems<DateTime>) this._quickInputItems).OnItemSelected += new EventHandler<InputItemViewModel<DateTime>>(this.OnQuickDateSelected);
                  break;
                }
                break;
              case '@':
                if (!usedInCal)
                {
                  this.GetAvatars(projectId);
                  if (this._avatars == null)
                  {
                    this._quickInputItems = (IQuickInput) null;
                    break;
                  }
                  if (this._quickInputItems is AssignInputItems quickInputItems && (quickInputItems.Models == this._avatars || this.IsOpen()))
                  {
                    quickInputItems.SetSelected(assignee);
                    if (quickInputItems.Models != this._avatars)
                    {
                      quickInputItems.SetModels(this._avatars);
                      break;
                    }
                    break;
                  }
                  this._quickInputItems = (IQuickInput) new AssignInputItems(this._avatars, selected: assignee);
                  ((BaseInputItems<AvatarViewModel>) this._quickInputItems).OnItemSelected -= new EventHandler<InputItemViewModel<AvatarViewModel>>(this.AssignSelect);
                  ((BaseInputItems<AvatarViewModel>) this._quickInputItems).OnItemSelected += new EventHandler<InputItemViewModel<AvatarViewModel>>(this.AssignSelect);
                  break;
                }
                break;
              case '^':
              case '~':
                if (!usedInCal)
                {
                  if (!(this._quickInputItems is ProjectInputItems))
                  {
                    this._quickInputItems = (IQuickInput) new ProjectInputItems(projectId);
                    ((BaseInputItems<ProjectModel>) this._quickInputItems).OnItemSelected -= new EventHandler<InputItemViewModel<ProjectModel>>(this.OnProjectSelect);
                    ((BaseInputItems<ProjectModel>) this._quickInputItems).OnItemSelected += new EventHandler<InputItemViewModel<ProjectModel>>(this.OnProjectSelect);
                    break;
                  }
                  ((ProjectInputItems) this._quickInputItems).SetSelected(projectId);
                  break;
                }
                if (!(this._quickInputItems is CalendarInputItems quickInputItems1) || !(quickInputItems1.AccountId == calId))
                {
                  this._quickInputItems = (IQuickInput) new CalendarInputItems(calId);
                  ((BaseInputItems<string>) this._quickInputItems).OnItemSelected -= new EventHandler<InputItemViewModel<string>>(this.OnCalendarSelect);
                  ((BaseInputItems<string>) this._quickInputItems).OnItemSelected += new EventHandler<InputItemViewModel<string>>(this.OnCalendarSelect);
                  break;
                }
                break;
            }
            break;
          case 2:
            if (mark == "[[")
            {
              List<TaskBaseViewModel> taskInQuickLink = TaskCache.GetTaskInQuickLink();
              taskInQuickLink.Sort((Comparison<TaskBaseViewModel>) ((a, b) => string.Compare(a.Title, b.Title, StringComparison.CurrentCulture)));
              if (taskInQuickLink.Count > 0 && !(this._quickInputItems is TaskLinkInputItems))
              {
                this._quickInputItems = (IQuickInput) new TaskLinkInputItems(taskInQuickLink);
                ((BaseInputItems<TaskBaseViewModel>) this._quickInputItems).OnItemSelected -= new EventHandler<InputItemViewModel<TaskBaseViewModel>>(this.TaskSelect);
                ((BaseInputItems<TaskBaseViewModel>) this._quickInputItems).OnItemSelected += new EventHandler<InputItemViewModel<TaskBaseViewModel>>(this.TaskSelect);
                break;
              }
              break;
            }
            break;
        }
      }
      return this._quickInputItems != null;
    }

    private void TaskSelect(object sender, InputItemViewModel<TaskBaseViewModel> e)
    {
      EventHandler<QuickSetModel> itemSelected = this.ItemSelected;
      if (itemSelected != null)
        itemSelected(sender, new QuickSetModel()
        {
          Type = QuickSetType.TaskLink,
          task = e.Entity,
          Title = e.Title
        });
      this._popup.IsOpen = false;
    }

    private void GetAvatars(string avatarProjectId)
    {
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == avatarProjectId));
      if (projectModel == null || !projectModel.IsShareList())
      {
        this._avatars = (List<AvatarViewModel>) null;
      }
      else
      {
        this._avatars = AvatarHelper.GetCacheProjectAvatars(avatarProjectId);
        this.TryPullRemoteUsers(avatarProjectId);
        this._avatarProject = avatarProjectId;
      }
    }

    private void OnQuickDateSelected(object sender, InputItemViewModel<DateTime> e)
    {
      EventHandler<QuickSetModel> itemSelected = this.ItemSelected;
      if (itemSelected != null)
        itemSelected(sender, new QuickSetModel()
        {
          Type = QuickSetType.Date,
          Date = new DateTime?(e.Entity),
          Title = e.Value
        });
      this._popup.IsOpen = false;
    }

    private void AssignSelect(object sender, InputItemViewModel<AvatarViewModel> e)
    {
      EventHandler<QuickSetModel> itemSelected = this.ItemSelected;
      if (itemSelected != null)
        itemSelected(sender, new QuickSetModel()
        {
          Type = QuickSetType.Assign,
          Avatar = e.Entity,
          Title = e.Title
        });
      this._popup.IsOpen = false;
    }

    private void TagSelect(object sender, string e)
    {
      EventHandler<QuickSetModel> itemSelected = this.ItemSelected;
      if (itemSelected != null)
        itemSelected(sender, new QuickSetModel()
        {
          Type = QuickSetType.Tag,
          Tag = e,
          Title = e
        });
      this._popup.IsOpen = false;
    }

    private void OnCalendarSelect(object sender, InputItemViewModel<string> e)
    {
      EventHandler<QuickSetModel> itemSelected = this.ItemSelected;
      if (itemSelected != null)
        itemSelected(sender, new QuickSetModel()
        {
          Type = QuickSetType.CalId,
          CalId = e.Entity,
          Title = e.Title
        });
      this._popup.IsOpen = false;
    }

    private void OnProjectSelect(object sender, InputItemViewModel<ProjectModel> e)
    {
      this._popup.IsOpen = false;
      EventHandler<QuickSetModel> itemSelected = this.ItemSelected;
      if (itemSelected == null)
        return;
      itemSelected(sender, new QuickSetModel()
      {
        Type = QuickSetType.Project,
        Project = e.Entity,
        Title = e.Title
      });
    }

    private void OnPrioritySelect(object sender, InputItemViewModel<int> e)
    {
      this._popup.IsOpen = false;
      EventHandler<QuickSetModel> itemSelected = this.ItemSelected;
      if (itemSelected == null)
        return;
      itemSelected(sender, new QuickSetModel()
      {
        Type = QuickSetType.Priority,
        Priority = e.Entity,
        Title = e.Title
      });
    }

    public void TryShowPopup(string mark, string content, System.Windows.Point position)
    {
      if (this._quickInputItems.Filter(content))
      {
        this.ItemsContainer.Children.Clear();
        if (this._quickInputItems is UIElement quickInputItems)
          this.ItemsContainer.Children.Add(quickInputItems);
        this._popup.HorizontalOffset = position.X;
        this._popup.VerticalOffset = position.Y;
        this._popup.IsOpen = true;
        this.Tag = (object) mark;
      }
      else
      {
        this.ItemsContainer.Children.Clear();
        this._popup.IsOpen = false;
      }
    }

    public void ClosePopup(bool clearChildren = true)
    {
      this._popup.IsOpen = false;
      if (!clearChildren)
        return;
      this.ItemsContainer.Children.Clear();
    }

    public void TryMovePopupItem(bool forward)
    {
      if (!this._popup.IsOpen || !this.ItemsContainer.IsVisible || !(this.ItemsContainer.Children[0] is IQuickInput child))
        return;
      child.Move(forward);
    }

    public void TrySelectItem()
    {
      if (!this.ItemsContainer.IsVisible || !(this.ItemsContainer.Children[0] is IQuickInput child))
        return;
      child.TrySelectItem();
    }

    public void Move(bool forward)
    {
      if (!this.ItemsContainer.IsVisible || !(this.ItemsContainer.Children[0] is IQuickInput child))
        return;
      child.Move(forward);
    }

    public async void TryPullRemoteUsers(string projectId)
    {
      if (!(this._avatarProject != projectId) && (DateTime.Now - this._lastPullTime).TotalMinutes < 5.0)
        return;
      this._avatarProject = projectId;
      this._lastPullTime = DateTime.Now;
      await AvatarHelper.ResetProjectAvatars(projectId);
      if (!(this._quickInputItems is AssignInputItems quickInputItems))
        return;
      List<AvatarViewModel> cacheProjectAvatars = AvatarHelper.GetCacheProjectAvatars(projectId);
      List<string> list1 = cacheProjectAvatars != null ? cacheProjectAvatars.Select<AvatarViewModel, string>((Func<AvatarViewModel, string>) (a => a.UserCode)).ToList<string>() : (List<string>) null;
      List<AvatarViewModel> avatars = this._avatars;
      List<string> list2 = avatars != null ? avatars.Select<AvatarViewModel, string>((Func<AvatarViewModel, string>) (a => a.UserCode)).ToList<string>() : (List<string>) null;
      if (list1 != null && list2 != null && list1.Count == list2.Count && list1.Union<string>((IEnumerable<string>) list2).Count<string>() == list2.Count)
        return;
      this._avatars = cacheProjectAvatars;
      quickInputItems.SetModels(this._avatars);
    }

    internal void TryRemoveTaskSelector()
    {
      if (!(this._quickInputItems is TaskLinkInputItems))
        return;
      this._quickInputItems = (IQuickInput) null;
    }

    public bool IsOpen() => this._popup.IsOpen;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/quickadd/quickselectioncontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.ItemsContainer = (Grid) target;
      else
        this._contentLoaded = true;
    }
  }
}
