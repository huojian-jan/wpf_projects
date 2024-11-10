// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TutorialPopup
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public class TutorialPopup : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty StepDependencyProperty = DependencyProperty.Register(nameof (Step), typeof (int), typeof (TutorialPopup), new PropertyMetadata((object) 1, new PropertyChangedCallback(TutorialPopup.OnStepChangedCallback)));
    internal Border FirstBorder;
    internal Border SecondBorder;
    internal Border ThirdBorder;
    internal Grid TPopup;
    internal TextBlock TitleText;
    internal TextBlock ContentText;
    internal Ellipse FirstIndicator;
    internal Ellipse SecondIndicator;
    internal Ellipse ThirdIndicator;
    internal Ellipse ForthIndicator;
    internal Button ActionButton;
    internal TextBlock SkipText;
    private bool _contentLoaded;

    public int Step
    {
      get => (int) this.GetValue(TutorialPopup.StepDependencyProperty);
      set => this.SetValue(TutorialPopup.StepDependencyProperty, (object) value);
    }

    private static void OnStepChangedCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is TutorialPopup tutorialPopup) || e.NewValue == null)
        return;
      tutorialPopup.InitContent();
    }

    public TutorialPopup()
    {
      this.InitializeComponent();
      this.InitContent();
    }

    private void InitContent()
    {
      switch (this.Step)
      {
        case 1:
          this.TitleText.Text = Utils.GetString("Inbox");
          this.ContentText.Text = Utils.GetString("TutorialFirst");
          this.ResetSetIndicator();
          this.FirstIndicator.Opacity = 1.0;
          break;
        case 2:
          this.TitleText.Text = Utils.GetString("SmartList");
          this.ContentText.Text = Utils.GetString("TutorialSecond");
          this.ResetSetIndicator();
          this.SecondIndicator.Opacity = 1.0;
          break;
        case 3:
          this.TitleText.Text = Utils.GetString("lists");
          this.ContentText.Text = Utils.GetString("TutorialThird");
          this.ResetSetIndicator();
          this.ThirdIndicator.Opacity = 1.0;
          break;
        case 4:
          this.TitleText.Text = Utils.GetString("CreateTask");
          this.ContentText.Text = Utils.GetString("TutorialForth");
          this.ResetSetIndicator();
          this.ForthIndicator.Opacity = 1.0;
          this.ActionButton.Content = (object) Utils.GetString("StartUse");
          this.SkipText.Visibility = Visibility.Collapsed;
          break;
      }
    }

    private void ResetSetIndicator()
    {
      this.FirstIndicator.Opacity = 0.36000001430511475;
      this.SecondIndicator.Opacity = 0.36000001430511475;
      this.ThirdIndicator.Opacity = 0.36000001430511475;
      this.ForthIndicator.Opacity = 0.36000001430511475;
    }

    private void OnNextClick(object sender, RoutedEventArgs e)
    {
      switch (this.Step)
      {
        case 1:
          this.MoveToSecondStep();
          break;
        case 2:
          this.MoveToThirdStep();
          break;
        case 3:
          this.MoveToForthStep();
          break;
        case 4:
          this.Finish();
          break;
      }
    }

    private void Finish()
    {
      Utils.FindParent<MainWindow>((DependencyObject) this)?.FinishTutorial();
    }

    public void MoveToSecondStep()
    {
      this.FirstBorder.Visibility = Visibility.Collapsed;
      this.SecondBorder.Visibility = Visibility.Visible;
      this.Step = 2;
      this.TPopup.Margin = new Thickness(0.0, 92.0, 0.0, 0.0);
    }

    public void MoveToThirdStep()
    {
      this.SecondBorder.Visibility = Visibility.Collapsed;
      this.ThirdBorder.Visibility = Visibility.Visible;
      this.Step = 3;
      this.TPopup.Margin = new Thickness(0.0, 200.0, 0.0, 0.0);
    }

    public void MoveToForthStep()
    {
      this.ThirdBorder.Visibility = Visibility.Collapsed;
      this.TPopup.Margin = new Thickness(257.0, 118.0, 0.0, 0.0);
      this.Step = 4;
    }

    private void OnSkipClick(object sender, MouseButtonEventArgs e) => this.Finish();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/tutorialpopup.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.FirstBorder = (Border) target;
          break;
        case 2:
          this.SecondBorder = (Border) target;
          break;
        case 3:
          this.ThirdBorder = (Border) target;
          break;
        case 4:
          this.TPopup = (Grid) target;
          break;
        case 5:
          this.TitleText = (TextBlock) target;
          break;
        case 6:
          this.ContentText = (TextBlock) target;
          break;
        case 7:
          this.FirstIndicator = (Ellipse) target;
          break;
        case 8:
          this.SecondIndicator = (Ellipse) target;
          break;
        case 9:
          this.ThirdIndicator = (Ellipse) target;
          break;
        case 10:
          this.ForthIndicator = (Ellipse) target;
          break;
        case 11:
          this.ActionButton = (Button) target;
          this.ActionButton.Click += new RoutedEventHandler(this.OnNextClick);
          break;
        case 12:
          this.SkipText = (TextBlock) target;
          this.SkipText.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSkipClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
