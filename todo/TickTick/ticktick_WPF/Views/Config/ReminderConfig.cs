// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.ReminderConfig
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Filter;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class ReminderConfig : UserControl, IComponentConnector
  {
    private string _email;
    private bool _verifiedEmail;
    private bool _isEmailPopupOpen;
    private List<string> _emailNotificationEntityTypes;
    internal CustomSimpleComboBox ShowReminderComboBox;
    internal CustomSimpleComboBox ReminderDetailComboBox;
    internal Grid ReminderRingGrid;
    internal CustomSimpleComboBox RemindSoundComboBox;
    internal CustomSimpleComboBox CompletionSoundComboBox;
    internal Border EmailReminderGrid;
    internal CheckBox EmailReminderCheckBox;
    internal StackPanel EmailPanel;
    internal TextBlock EmailText;
    internal Path VerifiedIcon;
    internal StackPanel NotificationItemPanel;
    internal EmjTextBlock EmailNotificationType;
    internal EscPopup ListPopup;
    private bool _contentLoaded;

    public ReminderConfig()
    {
      this.InitializeComponent();
      this.InitData();
    }

    private void InitData()
    {
      this.InitShowReminder();
      this.InitEmailReminder();
      this.InitRemindSound();
      this.InitRemindDetail();
      this.InitCompletionSound();
    }

    private async void InitEmailReminder()
    {
      this.EmailReminderGrid.Visibility = Visibility.Collapsed;
    }

    private void ShowReminderSelectionChanged(object sender, SimpleComboBoxViewModel e)
    {
      if (this.ShowReminderComboBox.ItemsSource.Count == 3)
      {
        if (this.ShowReminderComboBox.SelectedIndex == 1 && !SystemToastUtils.CheckSystemToastEnable())
          this.ShowReminderComboBox.SelectedIndex = 0;
        LocalSettings.Settings.ShowReminderInClient = this.ShowReminderComboBox.SelectedIndex == 0;
        LocalSettings.Settings.ShowReminder = this.ShowReminderComboBox.SelectedIndex != 2;
      }
      else
        LocalSettings.Settings.ShowReminder = this.ShowReminderComboBox.SelectedIndex == 0;
    }

    private void InitShowReminder()
    {
      List<string> stringList = new List<string>()
      {
        Utils.GetString("ReminderMeOnClient"),
        Utils.GetString("ReminderMeOnSystem"),
        Utils.GetString("DoNotReminderMe")
      };
      if (!Utils.IsWindows7())
      {
        bool flag = SystemToastUtils.CheckSystemToastEnable();
        if (!flag)
          stringList.RemoveAt(1);
        if (LocalSettings.Settings.ShowReminder && !LocalSettings.Settings.ShowReminderInClient && !flag)
          LocalSettings.Settings.ShowReminderInClient = true;
        this.ShowReminderComboBox.SelectedIndex = !flag ? (!LocalSettings.Settings.ShowReminder ? 1 : 0) : (LocalSettings.Settings.ShowReminder ? (!LocalSettings.Settings.ShowReminderInClient ? 1 : 0) : 2);
      }
      else
      {
        stringList.RemoveAt(1);
        this.ShowReminderComboBox.SelectedIndex = !LocalSettings.Settings.ShowReminder ? 1 : 0;
        LocalSettings.Settings.ShowReminderInClient = true;
      }
      this.ShowReminderComboBox.ItemsSource = stringList;
    }

    private void InitCompletionSound()
    {
      this.CompletionSoundComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("none"),
        Utils.GetString("Jingle"),
        Utils.GetString("Drip"),
        Utils.GetString("Knock"),
        Utils.GetString("Spiral")
      };
      CompletionSound result;
      Enum.TryParse<CompletionSound>(LocalSettings.Settings.CompletionSound, out result);
      this.CompletionSoundComboBox.SelectedIndex = (int) result;
    }

    private void InitRemindSound()
    {
      this.RemindSoundComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("none"),
        Utils.GetString("Default"),
        Utils.GetString("MusicBoxSound"),
        Utils.GetString("DrumSound"),
        Utils.GetString("BeepSound"),
        Utils.GetString("BlocksSound"),
        Utils.GetString("ChimesSound"),
        Utils.GetString("CrystalSound"),
        Utils.GetString("LadderSound"),
        Utils.GetString("LatticeSound"),
        Utils.GetString("LeapSound"),
        Utils.GetString("MatrixSound"),
        Utils.GetString("PulseSound"),
        Utils.GetString("Spiral")
      };
      RemindSound result;
      if (Enum.TryParse<RemindSound>(LocalSettings.Settings.ExtraSettings.RemindSound, out result))
        this.RemindSoundComboBox.SelectedIndex = (int) result;
      else
        this.RemindSoundComboBox.SelectedIndex = LocalSettings.Settings.EnableRingtone ? 1 : 0;
    }

    private void OnRemindSoundChanged(object sender, SimpleComboBoxViewModel e)
    {
      LocalSettings.Settings.ExtraSettings.RemindSound = ((RemindSound) this.RemindSoundComboBox.SelectedIndex).ToString();
      RemindSoundPlayer.PlayTaskRemindSound(true);
    }

    private void InitRemindDetail()
    {
      this.ReminderDetailComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("Always"),
        Utils.GetString("WhenUnlocked"),
        Utils.GetString("Never")
      };
      this.ReminderDetailComboBox.SelectedIndex = LocalSettings.Settings.ExtraSettings.RemindDetail;
    }

    private void OnRemindDetailChanged(object sender, SimpleComboBoxViewModel e)
    {
      LocalSettings.Settings.ExtraSettings.RemindDetail = this.ReminderDetailComboBox.SelectedIndex;
    }

    private void OnCompletionSoundChanged(object sender, SimpleComboBoxViewModel e)
    {
      LocalSettings.Settings.CompletionSound = ((CompletionSound) this.CompletionSoundComboBox.SelectedIndex).ToString();
      Utils.PlayCompletionSound();
    }

    private async void EmailReminderClick(object sender, MouseButtonEventArgs e)
    {
      ReminderConfig child = this;
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!ProChecker.CheckPro(ProType.EmailReminder))
        return;
      if (string.IsNullOrEmpty(child._email) || !child._verifiedEmail)
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString(string.IsNullOrEmpty(child._email) ? "NoEmail" : "NotVerified"), Utils.GetString(string.IsNullOrEmpty(child._email) ? "NotSetEmailCantReminder" : "NotVerifiedCantReminder"), Utils.GetString(string.IsNullOrEmpty(child._email) ? "GotoSet" : "GotoVerify"), Utils.GetString("Cancel"));
        customerDialog.Owner = Window.GetWindow((DependencyObject) child);
        if (!customerDialog.ShowDialog().GetValueOrDefault())
          return;
        if (string.IsNullOrEmpty(child._email))
        {
          ReminderConfig.GotoSetEmail();
        }
        else
        {
          await Communicator.ResentVerifyEmail();
          Utils.FindParent<SettingDialog>((DependencyObject) child)?.Toast(Utils.GetString("SendEmailSucceeded"));
        }
      }
      else
      {
        child.EmailReminderCheckBox.IsChecked = new bool?(!child.EmailReminderCheckBox.IsChecked.GetValueOrDefault());
        LocalSettings.Settings.SetEmailReminder(child.EmailReminderCheckBox.IsChecked.GetValueOrDefault());
        SettingsHelper.PushLocalPreference();
      }
    }

    private static void GotoSetEmail()
    {
      Utils.TryProcessStartUrlWithToken("/webapp/#settings/profile");
    }

    private void OnTaskEditClick(object sender, MouseButtonEventArgs e)
    {
      if (this.ListPopup.IsOpen || this._isEmailPopupOpen)
      {
        this.ListPopup.IsOpen = false;
      }
      else
      {
        NotificationEntityTypeEditDialog entityTypeEditDialog = new NotificationEntityTypeEditDialog(this._emailNotificationEntityTypes);
        this.ListPopup.Child = (UIElement) entityTypeEditDialog;
        this.ListPopup.IsOpen = true;
        this._isEmailPopupOpen = true;
        entityTypeEditDialog.OnSelectedTypesChanged += (EventHandler<List<string>>) ((o, list) => this._emailNotificationEntityTypes = list);
        entityTypeEditDialog.OnSave += (EventHandler<FilterConditionViewModel>) ((o, model) =>
        {
          if (this._emailNotificationEntityTypes.Count > 0)
          {
            this.EmailNotificationType.Text = ReminderConfig.ConvertNotificationDisplayText(this._emailNotificationEntityTypes);
            LocalSettings.Settings.SetEmailRemindItems(this._emailNotificationEntityTypes.Distinct<string>().ToList<string>());
            SettingsHelper.PushLocalPreference();
          }
          this.ListPopup.IsOpen = false;
        });
        entityTypeEditDialog.OnCancel += (EventHandler) ((o, model) => this.ListPopup.IsOpen = false);
        this.ListPopup.Closed -= new EventHandler(this.OnListPopupClosed);
        this.ListPopup.Closed += new EventHandler(this.OnListPopupClosed);
      }
    }

    private async void OnListPopupClosed(object sender, EventArgs e)
    {
      await Task.Delay(100);
      this._isEmailPopupOpen = false;
    }

    private static string ConvertNotificationDisplayText(List<string> keys)
    {
      List<string> source = new List<string>();
      foreach (string key in keys)
      {
        switch (key)
        {
          case "task":
            source.Add(Utils.GetString("task_plural"));
            continue;
          case "habit":
            source.Add(Utils.GetString("Habits"));
            continue;
          default:
            continue;
        }
      }
      return string.Join(", ", (IEnumerable<string>) source.Distinct<string>().ToList<string>());
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/reminderconfig.xaml", UriKind.Relative));
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
          this.ShowReminderComboBox = (CustomSimpleComboBox) target;
          break;
        case 2:
          this.ReminderDetailComboBox = (CustomSimpleComboBox) target;
          break;
        case 3:
          this.ReminderRingGrid = (Grid) target;
          break;
        case 4:
          this.RemindSoundComboBox = (CustomSimpleComboBox) target;
          break;
        case 5:
          this.CompletionSoundComboBox = (CustomSimpleComboBox) target;
          break;
        case 6:
          this.EmailReminderGrid = (Border) target;
          break;
        case 7:
          this.EmailReminderCheckBox = (CheckBox) target;
          this.EmailReminderCheckBox.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.EmailReminderClick);
          break;
        case 8:
          this.EmailPanel = (StackPanel) target;
          break;
        case 9:
          this.EmailText = (TextBlock) target;
          break;
        case 10:
          this.VerifiedIcon = (Path) target;
          break;
        case 11:
          this.NotificationItemPanel = (StackPanel) target;
          break;
        case 12:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnTaskEditClick);
          break;
        case 13:
          this.EmailNotificationType = (EmjTextBlock) target;
          break;
        case 14:
          this.ListPopup = (EscPopup) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
