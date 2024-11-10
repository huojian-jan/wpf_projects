// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.EditorCommands
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Input;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public static class EditorCommands
  {
    public static readonly ICommand ToggleBoldCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).Bold()));
    public static readonly ICommand ToggleStrokeCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).StrokeLine()));
    public static readonly ICommand ToggleItalicCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).Italic()));
    public static readonly ICommand ToggleHighLightCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).Highlight()));
    public static readonly ICommand ToggleUnderLineCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).UnderLine()));
    public static readonly ICommand ToggleCodeCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).Code()));
    public static readonly ICommand InsertLinkCommand = (ICommand) new RelayCommand((Action<object>) (editor =>
    {
      MarkDownEditor markDownEditor = (MarkDownEditor) editor;
      markDownEditor.ShowInsertLink(markDownEditor.EditBox.SelectedText, string.Empty);
    }));
    public static readonly ICommand InsertHead1Command = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).InsertHeader(1)));
    public static readonly ICommand InsertHead2Command = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).InsertHeader(2)));
    public static readonly ICommand InsertHead3Command = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).InsertHeader(3)));
    public static readonly ICommand InsertHead4Command = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).InsertHeader(4)));
    public static readonly ICommand InsertHead5Command = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).InsertHeader(5)));
    public static readonly ICommand InsertHead6Command = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).InsertHeader(6)));
    public static readonly ICommand InsertUnOrderList = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).InsertUnOrderList()));
    public static readonly ICommand InsertNumberedList = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).InsertNumberedList()));
    public static readonly ICommand IndentCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).Indent()));
    public static readonly ICommand OutIndentCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).DeIndent()));
    public static readonly ICommand QuoteCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).Quote()));
    public static readonly ICommand InsertItemCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).InsertCheckItem()));
    public static readonly ICommand SplitLineCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).InsertLine()));
    public static readonly ICommand ExitCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).ExitEditor()));
    public static readonly ICommand ToggleCheckItemCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).ToggleCheckItem()));
    public static readonly ICommand SaveContentCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).TrySaveContent()));
    public static readonly ICommand EnterImmersiveCommand = (ICommand) new RelayCommand((Action<object>) (editor => ((MarkDownEditor) editor).EnterImmersiveMode()));
  }
}
