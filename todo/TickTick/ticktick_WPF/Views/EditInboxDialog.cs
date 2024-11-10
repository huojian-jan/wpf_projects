// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.EditInboxDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class EditInboxDialog : Window, IOkCancelWindow, IComponentConnector
  {
    internal ticktick_WPF.Views.Misc.ColorSelector.ColorSelector ColorItems;
    internal Button SaveButton;
    private bool _contentLoaded;

    public EditInboxDialog()
    {
      this.InitializeComponent();
      InputBindingCollection inputBindings1 = this.InputBindings;
      KeyBinding keyBinding1 = new KeyBinding(OkCancelWindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding1.CommandParameter = (object) this;
      inputBindings1.Add((InputBinding) keyBinding1);
      InputBindingCollection inputBindings2 = this.InputBindings;
      KeyBinding keyBinding2 = new KeyBinding(OkCancelWindowCommands.OkCommand, new KeyGesture(Key.Return));
      keyBinding2.CommandParameter = (object) this;
      inputBindings2.Add((InputBinding) keyBinding2);
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    protected override void OnDeactivated(EventArgs e)
    {
      this.Topmost = false;
      base.OnDeactivated(e);
    }

    protected override async void OnActivated(EventArgs e)
    {
      EditInboxDialog editInboxDialog = this;
      editInboxDialog.Topmost = false;
      await Task.Delay(10);
      editInboxDialog.Topmost = true;
      // ISSUE: reference to a compiler-generated method
      editInboxDialog.\u003C\u003En__0(e);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e) => this.Save();

    private async void Save()
    {
      EditInboxDialog editInboxDialog = this;
      editInboxDialog.SaveButton.Content = (object) Utils.GetString("Saving");
      editInboxDialog.SaveButton.IsEnabled = false;
      string selectedColor = editInboxDialog.ColorItems.GetSelectedColor();
      if (string.IsNullOrEmpty(selectedColor))
        selectedColor = "transparent";
      string str = selectedColor;
      if (str != (await ProjectDao.GetProjectById(Utils.GetInboxId()))?.color)
      {
        str = (string) null;
        ticktick_WPF.Views.Misc.ColorSelector.ColorSelector.TryAddClickEvent(selectedColor);
      }
      await ProjectDao.SaveInboxColor(selectedColor);
      SettingsHelper.PushLocalSettings();
      editInboxDialog.Close();
      selectedColor = (string) null;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.Isinbox));
      if (projectModel == null)
        return;
      this.ColorItems.SetSelectedColor(projectModel.color);
    }

    public void OnCancel() => this.Close();

    public void Ok() => this.Save();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/editinboxdialog.xaml", UriKind.Relative));
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
          break;
        case 2:
          this.ColorItems = (ticktick_WPF.Views.Misc.ColorSelector.ColorSelector) target;
          break;
        case 3:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
