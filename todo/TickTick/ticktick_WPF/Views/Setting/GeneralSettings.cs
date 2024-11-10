// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.GeneralSettings
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class GeneralSettings : UserControl, IComponentConnector
  {
    internal Grid LanguageGrid;
    internal CustomSimpleComboBox LanguageComboBox;
    private bool _contentLoaded;

    public GeneralSettings()
    {
      this.InitializeComponent();
      this.LanguageComboBox.ItemsSource = new List<string>()
      {
        this.FindResource((object) "SimpleChinese") as string,
        "English",
        this.FindResource((object) "TraditionalChinese") as string,
        this.FindResource((object) "Japanese") as string,
        this.FindResource((object) "Korean") as string,
        this.FindResource((object) "French") as string,
        this.FindResource((object) "Russian") as string,
        this.FindResource((object) "Brasil") as string
      };
      this.InitData();
    }

    private void InitData() => this.GetLanguageUse();

    private void GetLanguageUse()
    {
      this.LanguageComboBox.SelectedIndex = this.GetItemIndexByLanguage(LocalSettings.Settings.UserChooseLanguage);
    }

    private int GetItemIndexByLanguage(string language)
    {
      if (language != null && language.Length == 5)
      {
        switch (language[3])
        {
          case 'B':
            if (language == "pt-BR")
              return 7;
            break;
          case 'C':
            if (language == "zh-CN")
              break;
            break;
          case 'F':
            if (language == "fr-FR")
              return 5;
            break;
          case 'J':
            if (language == "ja-JP")
              return 3;
            break;
          case 'K':
            if (language == "ko-KR")
              return 4;
            break;
          case 'R':
            if (language == "ru-RU")
              return 6;
            break;
          case 'T':
            if (language == "zh-TW")
              return 2;
            break;
          case 'U':
            if (language == "en-US")
              return 1;
            break;
        }
      }
      return 0;
    }

    private SettingDialog GetParent() => Utils.FindParent<SettingDialog>((DependencyObject) this);

    private async void LanguageComboBoxSelectionChanged(object sender, SimpleComboBoxViewModel e)
    {
      if (this.LanguageComboBox.SelectedIndex == -1)
        return;
      string userChooseLanguage = LocalSettings.Settings.UserChooseLanguage;
      LocalSettings.Settings.UserChooseLanguage = this.GetLanguageByIndex(this.LanguageComboBox.SelectedIndex);
      if (userChooseLanguage == LocalSettings.Settings.UserChooseLanguage)
        return;
      bool? nullable = new CustomerDialog(Utils.GetString("Language1"), Utils.GetString("ChangeLanguageContent"), Utils.GetString("Restart"), Utils.GetString("Cancel")).ShowDialog();
      bool flag = true;
      if (nullable.GetValueOrDefault() == flag & nullable.HasValue)
      {
        await LocalSettings.Settings.Save();
        await UserActCollectUtils.OnDeviceDataChanged();
        this.GetParent()?.Close();
        App.Instance.Restart();
      }
      else
      {
        LocalSettings.Settings.UserChooseLanguage = userChooseLanguage;
        this.LanguageComboBox.SelectedIndex = this.GetItemIndexByLanguage(userChooseLanguage);
      }
    }

    private string GetLanguageByIndex(int index)
    {
      switch (this.LanguageComboBox.SelectedIndex)
      {
        case 1:
          return "en-US";
        case 2:
          return "zh-TW";
        case 3:
          return "ja-JP";
        case 4:
          return "ko-KR";
        case 5:
          return "fr-FR";
        case 6:
          return "ru-RU";
        case 7:
          return "pt-BR";
        default:
          return "zh-CN";
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setting/generalsettings.xaml", UriKind.Relative));
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
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.LanguageComboBox = (CustomSimpleComboBox) target;
        else
          this._contentLoaded = true;
      }
      else
        this.LanguageGrid = (Grid) target;
    }
  }
}
