// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.NewUser.NewUserGuide
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.NewUser
{
  public class NewUserGuide : UserControl, IComponentConnector, IStyleConnector
  {
    private readonly Dictionary<string, Geometry> GuideFeatures = new Dictionary<string, Geometry>()
    {
      {
        Utils.GetString("Task"),
        Utils.GetIcon("TaskViewPath")
      },
      {
        Utils.GetString("Calendar"),
        Utils.GetIcon("IcCalendarView")
      },
      {
        Utils.GetString("Matrix"),
        Utils.GetIcon("EMViewPath")
      },
      {
        Utils.GetString("PomoFocus"),
        Utils.GetIcon("FocusViewPath")
      },
      {
        Utils.GetString("LoginTitleHabit"),
        Utils.GetIcon("HabitViewPath")
      }
    };
    public static readonly Dictionary<string, string> DefaultProjects = new Dictionary<string, string>()
    {
      {
        Utils.GetString("PresetListWork"),
        "\uD83D\uDCBC"
      },
      {
        Utils.GetString("PresetListMemo"),
        "\uD83C\uDFE1"
      },
      {
        Utils.GetString("PresetListShopping"),
        "\uD83D\uDCE6"
      },
      {
        Utils.GetString("PresetListList"),
        "\uD83E\uDD84"
      },
      {
        Utils.GetString("PresetListStudy"),
        "\uD83D\uDCD6"
      },
      {
        Utils.GetString("PresetListExercise"),
        "\uD83C\uDFC3"
      },
      {
        Utils.GetString("PresetListBirthday"),
        "\uD83C\uDF82"
      }
    };
    private int _pageIndex;
    private readonly List<string> _selectedFeatures = new List<string>()
    {
      Utils.GetString("Task"),
      Utils.GetString("Calendar")
    };
    private readonly List<string> _selectedProjects = new List<string>();
    private int _showMessageRange;
    internal Grid ChooseFeatureGrid;
    internal StackPanel BackIcon;
    internal EmjTextBlock PageTitle;
    internal ItemsControl ChooseFeatureItems;
    internal TextBlock MessageText;
    internal Rectangle BotRect;
    internal Ellipse BotEll;
    internal Button Continue;
    internal Image FeatureImage;
    internal ItemsControl SelectedFeatureItems;
    internal Grid ProjectGrid;
    internal Path TodayIcon;
    internal Path WeekIcon;
    internal Border EmptyList;
    internal ItemsControl SelectedProjectItems;
    private bool _contentLoaded;

    public NewUserGuide()
    {
      this.InitializeComponent();
      this.SetPage(0);
      this.TodayIcon.Data = Utils.GetIconData("CalDayIcon" + DateTime.Today.Day.ToString());
      this.WeekIcon.Data = Utils.GetIconData("CalWeekIcon" + DateTime.Today.DayOfWeek.ToString().Substring(0, 2));
    }

    private void SetPage(int i)
    {
      this.MessageText.Visibility = Visibility.Collapsed;
      this._pageIndex = i;
      if (this._pageIndex == 0)
      {
        this.BackIcon.Visibility = Visibility.Collapsed;
        this.ProjectGrid.Visibility = Visibility.Collapsed;
        this.PageTitle.Text = "\uD83D\uDC4B" + Utils.GetString("NewUserChooseFeature");
        this.InitChooseFeatures();
        this.Continue.Content = (object) Utils.GetString("Continue");
        this.SwitchFeature(this._selectedFeatures[0]);
        this.BotRect.SetValue(Grid.ColumnProperty, (object) 0);
        this.BotEll.SetValue(Grid.ColumnProperty, (object) 1);
      }
      else
      {
        this.ProjectGrid.Visibility = Visibility.Visible;
        this.BackIcon.Visibility = Visibility.Visible;
        this.PageTitle.Text = "\uD83D\uDC4B" + Utils.GetString("NewUserChooseList");
        this.InitPresetProject();
        this.FeatureImage.Source = (ImageSource) new BitmapImage(new Uri("pack://application:,,,/Assets/ImageSource/NewUserList.png"));
        this.Continue.Content = (object) Utils.GetString("GetStartedApp");
        this.SetSelectedProject(this._selectedProjects.LastOrDefault<string>());
        this.BotRect.SetValue(Grid.ColumnProperty, (object) 1);
        this.BotEll.SetValue(Grid.ColumnProperty, (object) 0);
      }
    }

    private void InitChooseFeatures()
    {
      List<string> list = this.GuideFeatures.Keys.ToList<string>();
      ObservableCollection<GuideChooseViewModel> observableCollection = new ObservableCollection<GuideChooseViewModel>();
      foreach (string str in list)
        observableCollection.Add(new GuideChooseViewModel(str, this.GuideFeatures[str])
        {
          Selected = this._selectedFeatures.Contains(str)
        });
      this.ChooseFeatureItems.ItemsSource = (IEnumerable) observableCollection;
    }

    private void InitPresetProject()
    {
      List<string> list = NewUserGuide.DefaultProjects.Keys.ToList<string>();
      ObservableCollection<GuideChooseViewModel> observableCollection = new ObservableCollection<GuideChooseViewModel>();
      foreach (string str in list)
        observableCollection.Add(new GuideChooseViewModel(str, NewUserGuide.DefaultProjects[str])
        {
          Selected = this._selectedProjects.Contains(str)
        });
      this.ChooseFeatureItems.ItemsSource = (IEnumerable) observableCollection;
    }

    private void SetSelectedProject(string selected)
    {
      if (string.IsNullOrEmpty(selected))
      {
        this.EmptyList.Visibility = Visibility.Visible;
        this.SelectedProjectItems.ItemsSource = (IEnumerable) null;
      }
      else
      {
        this.EmptyList.Visibility = Visibility.Collapsed;
        ObservableCollection<GuideChooseViewModel> observableCollection = new ObservableCollection<GuideChooseViewModel>();
        foreach (string selectedProject in this._selectedProjects)
          observableCollection.Add(new GuideChooseViewModel(selectedProject, NewUserGuide.DefaultProjects[selectedProject])
          {
            Selected = selected == selectedProject
          });
        this.SelectedProjectItems.ItemsSource = (IEnumerable) observableCollection;
      }
      this.SwitchFeature(this._selectedFeatures[0], false);
    }

    private void OnSkipClick(object sender, MouseButtonEventArgs e)
    {
      App.Window.RemoveNewUserGuide(this);
    }

    private async void OnContinueClick(object sender, RoutedEventArgs e)
    {
      NewUserGuide guide = this;
      if (guide._pageIndex == 0)
      {
        guide.SetPage(1);
      }
      else
      {
        await ProjectDao.InitProjects(guide._selectedProjects);
        List<string> list = guide.GuideFeatures.Keys.ToList<string>();
        DesktopTabBar desktopTabBar1 = new DesktopTabBar();
        desktopTabBar1.bars = new List<TabBarModel>()
        {
          new TabBarModel()
          {
            name = "TASK",
            sortOrder = 0L,
            status = "active"
          },
          new TabBarModel()
          {
            name = "CALENDAR",
            sortOrder = 1L,
            status = "active"
          },
          new TabBarModel()
          {
            name = "SEARCH",
            sortOrder = (long) guide._selectedFeatures.Count,
            status = "active"
          }
        };
        desktopTabBar1.mtime = Utils.GetNowTimeStampInMills();
        DesktopTabBar desktopTabBar2 = desktopTabBar1;
        UserActCollectUtils.AddClickEvent("newuser_guide", "tab_bar_enable", "task");
        UserActCollectUtils.AddClickEvent("newuser_guide", "tab_bar_enable", "calendar");
        for (int index = 2; index < guide._selectedFeatures.Count; ++index)
        {
          if (guide._selectedFeatures[index] == list[2])
          {
            desktopTabBar2.bars.Add(new TabBarModel()
            {
              name = "MATRIX",
              sortOrder = (long) index,
              status = "active"
            });
            UserActCollectUtils.AddClickEvent("newuser_guide", "tab_bar_enable", "enable_matrix");
          }
          if (guide._selectedFeatures[index] == list[3])
          {
            desktopTabBar2.bars.Add(new TabBarModel()
            {
              name = "POMO",
              sortOrder = (long) index,
              status = "active"
            });
            UserActCollectUtils.AddClickEvent("newuser_guide", "tab_bar_enable", "enable_pomo");
          }
          if (guide._selectedFeatures[index] == list[4])
          {
            desktopTabBar2.bars.Add(new TabBarModel()
            {
              name = "HABIT",
              sortOrder = (long) index,
              status = "active"
            });
            UserActCollectUtils.AddClickEvent("newuser_guide", "tab_bar_enable", "habit");
          }
        }
        foreach (string selectedProject in guide._selectedProjects)
        {
          string str;
          UserActCollectUtils.AddClickEvent("newuser_guide", "preset_list_data", (NewUserGuide.DefaultProjects.TryGetValue(selectedProject, out str) ? str : "") + selectedProject);
        }
        UserActCollectUtils.AddClickEvent("newuser_guide", "enable_tab_count", guide._selectedFeatures.Count.ToString() ?? "");
        UserActCollectUtils.AddClickEvent("newuser_guide", "preset_list_count", guide._selectedProjects.Count.ToString() ?? "");
        LocalSettings.Settings.UserPreference.desktopTabBars = desktopTabBar2;
        SettingsHelper.PushLocalPreference();
        LocalSettings.Settings.Save(true);
        App.Window.RemoveNewUserGuide(guide);
      }
    }

    private void OnItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is GuideChooseViewModel dataContext))
        return;
      this.MessageText.Visibility = Visibility.Collapsed;
      if (this._pageIndex == 0)
      {
        if (this._selectedFeatures.Contains(dataContext.Name))
        {
          if (this._selectedFeatures.IndexOf(dataContext.Name) <= 1)
          {
            this.ShowMessageText();
            this.SwitchFeature(dataContext.Name);
            return;
          }
          this._selectedFeatures.Remove(dataContext.Name);
          dataContext.Selected = false;
        }
        else
        {
          this._selectedFeatures.Add(dataContext.Name);
          dataContext.Selected = true;
        }
        this.SwitchFeature(this._selectedFeatures.LastOrDefault<string>());
      }
      else
      {
        if (this._selectedProjects.Contains(dataContext.Name))
        {
          this._selectedProjects.Remove(dataContext.Name);
          dataContext.Selected = false;
        }
        else
        {
          this._selectedProjects.Add(dataContext.Name);
          dataContext.Selected = true;
        }
        this.SetSelectedProject(this._selectedProjects.LastOrDefault<string>());
      }
    }

    private async void ShowMessageText()
    {
      int range = new Random().Next(0, 10000);
      this._showMessageRange = range;
      this.MessageText.Visibility = Visibility.Visible;
      await Task.Delay(3000);
      if (range != this._showMessageRange)
        return;
      this.MessageText.Visibility = Visibility.Collapsed;
    }

    private void SwitchFeature(string current, bool setImage = true)
    {
      ObservableCollection<GuideChooseViewModel> observableCollection = new ObservableCollection<GuideChooseViewModel>();
      foreach (string selectedFeature in this._selectedFeatures)
        observableCollection.Add(new GuideChooseViewModel(selectedFeature, this.GuideFeatures[selectedFeature])
        {
          Selected = current == selectedFeature
        });
      this.SelectedFeatureItems.ItemsSource = (IEnumerable) observableCollection;
      int num = this.GuideFeatures.Keys.ToList<string>().IndexOf(current);
      if (!setImage || num < 0)
        return;
      this.FeatureImage.Source = (ImageSource) new BitmapImage(new Uri(string.Format("pack://application:,,,/Assets/ImageSource/NewUserFeature{0}.png", (object) num)));
    }

    private void PreviousClick(object sender, MouseButtonEventArgs e) => this.SetPage(0);

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/newuser/newuserguide.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.ChooseFeatureGrid = (Grid) target;
          break;
        case 2:
          this.BackIcon = (StackPanel) target;
          this.BackIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.PreviousClick);
          break;
        case 3:
          this.PageTitle = (EmjTextBlock) target;
          break;
        case 4:
          this.ChooseFeatureItems = (ItemsControl) target;
          break;
        case 6:
          this.MessageText = (TextBlock) target;
          break;
        case 7:
          this.BotRect = (Rectangle) target;
          break;
        case 8:
          this.BotEll = (Ellipse) target;
          break;
        case 9:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSkipClick);
          break;
        case 10:
          this.Continue = (Button) target;
          this.Continue.Click += new RoutedEventHandler(this.OnContinueClick);
          break;
        case 11:
          this.FeatureImage = (Image) target;
          break;
        case 12:
          this.SelectedFeatureItems = (ItemsControl) target;
          break;
        case 13:
          this.ProjectGrid = (Grid) target;
          break;
        case 14:
          this.TodayIcon = (Path) target;
          break;
        case 15:
          this.WeekIcon = (Path) target;
          break;
        case 16:
          this.EmptyList = (Border) target;
          break;
        case 17:
          this.SelectedProjectItems = (ItemsControl) target;
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
      if (connectionId != 5)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnItemClick);
    }
  }
}
